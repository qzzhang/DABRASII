using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Media;
using System.Drawing.Imaging;

namespace DABRAS_Software
{
    public partial class FormQC : Form
    {
        #region Public Enums
        public enum TypeOfQC { Background, Alpha, Beta }
        #endregion

        #region Data Members
        private mainFramework frmParent;

        private DABRAS DABRAS;
        private bool DABRASModified = false;
        private QCListKeeper QC_List;

        private DefaultConfigurations DC;
        private bool DCModified = false;

        private double AlphaHi = -1;
        private double AlphaLo = -1;
        private double BetaHi = -1;
        private double BetaLo = -1;
        private double bkgAlphaHi = -1;
        private double bkgAlphaLo = -1;
        private double bkgBetaHi = -1;
        private double bkgBetaLo = -1;

        private bool SourcesModified = false;
       
        private TypeOfQC _typ;

        QCBackgroundListener QCBG = null;
        QCAlphaBetaListener QCAB = null;

        private Thread OperationThread_QCBG = null;
        private Thread OperationThread_QCAB = null;

        private RadionuclideFamily SelectedFamily;

        private List<User> ListOfUsers;
        private List<Radioactive_Source> ListOfSources;
        private List<RadionuclideFamily> ListOfFamily;

        private Form ChildDialogue;

        public delegate void BG_GUI_Callback();
        public delegate void AB_GUI_Callback();
        private bool bgPassed, abPassed;

        private int NumSamples = 0;
        private int Sampletime = 0;
        private int BadgeNo = 0;

        private string[,] bkgrdData, eff_Data_alpha, eff_Data_beta;
        #endregion

        #region Constructor
        public FormQC(mainFramework Parent, TypeOfQC _T)
        {
            InitializeComponent();

            this.frmParent = Parent;

            this.DABRAS = frmParent.GetDABRAS();
            this.Type = _T;
            this.DC = frmParent.GetDefaultConfig();
            this.ListOfUsers = frmParent.GetListOfUsers();
            this.ListOfSources = frmParent.GetListOfSources();
            this.ListOfFamily = frmParent.GetListOfFamily();
            this.BackgroundData = null;
            this.AlphaEffData = null;
            this.BetaEffData = null;

            this.updUserCombo();

            this.initQCList();

            this.initDataGrid();

            this.FillWithPreviousData();

            this.setSourceDefault();

            this.ActivateSaveButtons(false);
        }
        #endregion        

        #region QCList initialization
        private void initQCList()
        {
            string CurrentDir = Environment.CurrentDirectory;
            string DataPath = String.Concat(CurrentDir, "\\data\\QC\\Master\\QC_Data.dat");
            if (File.Exists(DataPath))
            {
                try
                {
                    using (Stream S = File.Open(DataPath, FileMode.Open, FileAccess.Read))
                    {
                        IFormatter BF = new BinaryFormatter(); //Open for reading
                        if (S.Length != 0)
                        {
                            this.QC_List = (QCListKeeper)BF.Deserialize(S);
                        }
                    }
                }
                catch
                {
                    this.QC_List = null;
                }
            }

            if (this.QC_List == null)
            {
                this.QC_List = new QCListKeeper();
            }

            this.DABRAS.SetQCList(this.QC_List);
        }
        #endregion

        #region initialize the GridView
        public void initDataGrid()
        {
            this.ShortDataGridView.DataSource = null;
            this.ShortDataGridView.Rows.Clear();
            this.ShortDataGridView.Columns.Clear();

            int NumberOfRows = 0;

            try
            {
                NumberOfRows = Convert.ToInt32(this.Counts_TB.Text);
                Math.Sqrt(NumberOfRows);
            }
            catch
            {
                MessageBox.Show("Bad values");
            }

            if (this.ShortDataGridView.Columns.Count == 0)//adding columns only at the first time
            {
                ShortDataGridView.Columns.Add("ShortDataGridView_Time", "Sample No");
                ShortDataGridView.Columns.Add("ShortDataGridView_Time", "Acq Time");
                ShortDataGridView.Columns.Add("ShortDataGridView_AGCPM", "Alpha Gross CPM");
                if (this.Type != TypeOfQC.Background)
                    ShortDataGridView.Columns.Add("ShortDataGridView_ANCPM", "Alpha Net CPM");
                ShortDataGridView.Columns.Add("ShortDataGridView_BGCPM", "Beta Gross CPM");
                if (this.Type != TypeOfQC.Background)
                    ShortDataGridView.Columns.Add("ShortDataGridView_BNCPM", "Beta Net CPM");
                ShortDataGridView.Columns.Add("ShortDataGridView_PassFail", "Pass/Fail");
            }

            //Add keypress event handlers
            //this.KeyPreview = true;
            //this.KeyDown += new KeyEventHandler(KeyPressed);

            for (int col_i = 0; col_i < this.ShortDataGridView.Columns.Count; col_i++)
            {
                DataGridViewColumn column = this.ShortDataGridView.Columns[col_i];
                if (column.Index == 0 || column.Index == (this.ShortDataGridView.ColumnCount - 1))
                    column.SortMode = DataGridViewColumnSortMode.NotSortable;
                else
                    column.SortMode = DataGridViewColumnSortMode.Automatic;
            }
            Add_Or_Subtract_Rows(NumberOfRows, this.ShortDataGridView);
            this.ShortDataGridView[0, NumberOfRows].Value = "Avg";
            this.ShortDataGridView[0, NumberOfRows + 1].Value = "StdDev";
        }
        private void FillWithPreviousData()
        {
            if (this.BackgroundData != null && this.Type == TypeOfQC.Background)
                this.FillDataGridView(this.BackgroundData);
            else if (this.AlphaEffData != null && this.Type == TypeOfQC.Alpha)
                this.FillDataGridView(this.AlphaEffData);
            else if (this.BetaEffData != null && this.Type == TypeOfQC.Beta)
                this.FillDataGridView(this.BetaEffData);
        }
        private void FillDataGridView(string[,] arrData)
        {
            int Rows = arrData.GetLength(0);
            int Cols = arrData.GetLength(1);
            int smpTime = Convert.ToInt32(arrData[1, 1]);
            int smpNum = Rows - 3;//ignore the header, avg and stdev rows
            DataGridView dgv = this.ShortDataGridView;
            string dgvNm = dgv.Name;

            this.Counts_TB.Text = Convert.ToString(smpNum);
            this.Min_TB.Text = Convert.ToString(smpTime / 60);
            this.Sec_TB.Text = Convert.ToString(smpTime % 60);

            this.Add_Or_Subtract_Rows(smpNum, dgv);

            for (int i = 1; i <= smpNum + 2; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    dgv[j, i - 1].Value = arrData[i, j];
                }
            }
        }
        #endregion

        #region user management
        private void FillEmployeeData(object sender, EventArgs e)
        {
            int badgeNum = -1;
            try
            {
                badgeNum = Convert.ToInt32(this.Badge_CB.Text);
            }
            catch
            {
                return;
            }

            if (badgeNum != -1)
            {
                User FoundUser = null;
                try
                {
                    FoundUser = this.ListOfUsers.Find(x => x.GetBadgeNo() == badgeNum);
                }
                catch
                {
                    return;
                }
                if (FoundUser != null)
                {
                    this.Name_TB.Text = FoundUser.GetName();
                    this.frmParent.QCuserName = FoundUser.GetName();
                    this.Badge_CB.Text = Convert.ToString(FoundUser.GetBadgeNo());
                }
            }
        }
        private void Badge_CB_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillEmployeeData(this, null);
            this.frmParent.QCuserID = Convert.ToInt32(this.Badge_CB.Text);
        }
        private void updUserCombo()
        {
            this.Badge_CB.Items.Clear();

            foreach (User U in this.ListOfUsers)
            {
                this.Badge_CB.Items.Add(Convert.ToString(U.GetBadgeNo()));
            }
            this.Badge_CB.SelectedIndex = this.Badge_CB.Items.Count - 1;
        }
        private void btnAddUser_Click(object sender, EventArgs e)
        {
            string userNm;
            int bdgNum;

            try
            {
                bdgNum = Convert.ToInt32(this.tbBadge.Text);
            }
            catch (Exception eNum)
            {
                MessageBox.Show("Please enter numbers only for Badge number! " + eNum.Message);
                return;
            }
            userNm = Convert.ToString(this.tbUserName.Text);

            User tgtUser = this.ListOfUsers.Find(x => (x.GetBadgeNo() == bdgNum));
            if (tgtUser == null)
            {
                this.ListOfUsers.Add(new User(bdgNum, userNm));
            }
            else
                tgtUser.SetName(userNm);

            this.updUserCombo();

            this.frmParent.updUsers(this.ListOfUsers);

            return;
        }
        private void btnRemoveUser_Click(object sender, EventArgs e)
        {
            User tgtUser = this.ListOfUsers.Find(x => (x.GetName() == Convert.ToString(this.tbUserName.Text) && x.GetBadgeNo() == Convert.ToInt32(this.tbBadge.Text)));
            if (tgtUser != null)
            {
                this.ListOfUsers.Remove(tgtUser);
                this.updUserCombo();
                this.frmParent.updUsers(this.ListOfUsers);
            }
            return;
        }
        #endregion

        #region Find source and set labels correctly
        public void setSourceDefault()
        {
            if (this.Type == TypeOfQC.Background)
            {
                this.SelectedFamily = this.ListOfFamily.Find(x => x.GetName() == "Background");

                this.bkgAlphaHi = this.SelectedFamily.GetAlphaHi();
                this.bkgAlphaLo = this.SelectedFamily.GetAlphaLo();
                this.bkgBetaHi = this.SelectedFamily.GetBetaHi();
                this.bkgBetaLo = this.SelectedFamily.GetBetaLo();

                this.Alpha_CPM_Label.Text = String.Format("Alpha GCPM must be between {0} and {1} (Set on {2})", this.bkgAlphaLo, this.bkgAlphaHi, this.SelectedFamily.GetHiLoCalibratedTime());
                this.Beta_CPM_Label.Text = String.Format("Beta GCPM must be between {0} and {1} (Set on {2})", this.bkgBetaLo, this.bkgBetaHi, this.SelectedFamily.GetHiLoCalibratedTime());

                //Change the default to the correct value
                this.Beta_CPM_Label.Visible = true;
                this.Alpha_CPM_Label.Visible = true;
            }

            if (this.Type == TypeOfQC.Alpha)
            {
                this.SelectedFamily = this.ListOfFamily.Find(x => x.GetName() == "Am-241");

                this.AlphaHi = this.SelectedFamily.GetAlphaHi();
                this.AlphaLo = this.SelectedFamily.GetAlphaLo();

                this.Alpha_CPM_Label.Text = String.Format("Alpha NCPM must be between {0} and {1} (Set on {2})", this.AlphaLo, this.AlphaHi, this.SelectedFamily.GetHiLoCalibratedTime());
                this.Beta_CPM_Label.Visible = false;
                this.Alpha_CPM_Label.Visible = true;
            }

            if (this.Type == TypeOfQC.Beta)
            {
                this.SelectedFamily = this.ListOfFamily.Find(x => x.GetName() == "Sr-90");

                this.BetaHi = SelectedFamily.GetBetaHi() * this.SelectedFamily.GetDecayFactor(DateTime.Now, this.SelectedFamily.GetHiLoCalibratedTime());
                this.BetaLo = SelectedFamily.GetBetaLo() * this.SelectedFamily.GetDecayFactor(DateTime.Now, this.SelectedFamily.GetHiLoCalibratedTime());

                this.Alpha_CPM_Label.Visible = false;
                this.Beta_CPM_Label.Visible = true;
                this.Beta_CPM_Label.Text = String.Format("Beta NCPM must be between {0:0.00} and {1:0.00} (Set on {2})", this.BetaLo, this.BetaHi, this.SelectedFamily.GetHiLoCalibratedTime());
            }
        }
        #endregion

        #region ResetQCType and the form content respectively
        private void ResetQCType(TypeOfQC _T)
        {
            this.Type = _T;
            this.initDataGrid();
            this.FillWithPreviousData();
            this.setSourceDefault();
        }
        #endregion

        #region Start Count Button Handler
        private bool Prepare4Start()
        {
            try
            {
                this.BadgeNo = Convert.ToInt32(this.Badge_CB.Text);
                if (String.Compare(Name_TB.Text, "") == 0)
                {
                    //Throw random exception
                    Exception k = new Exception();
                    throw k;
                }
            }
            catch
            {
                MessageBox.Show("Error: Please input your name and badge number.");
                return false;
            }

            try
            {
                this.NumSamples = Math.Abs(Convert.ToInt32(this.Counts_TB.Text));
                this.Sampletime = Math.Abs((60 * Convert.ToInt32(this.Min_TB.Text)) + Convert.ToInt32(this.Sec_TB.Text));
                if(this.NumSamples <= 0 || this.Sampletime <= 0)
                {
                    MessageBox.Show("Please enter positive numbers for samples and sampling time.");
                    return false;
                }
            }
            catch
            {
                MessageBox.Show("Error: Bad Sample Parameters");
                return false;
            }

            if (!DABRAS.IsConnected())
            {
                MessageBox.Show("Error: Connection to DABRAS needed. Please re-connect and try again.");
                this.frmParent.refreshConnectStatus();
                this.endFormActivities();
                return false;
            }

            this.initDataGrid();           
            this.ClearDataGridView(this.ShortDataGridView);

            this.SetGUI(true);
            this.enableTabButtons(false);
            return true;
        }

        private void enableTabButtons(bool val)
        {
            this.btnBackgroundCheck.Enabled = val;
            this.btnAlphaCheck.Enabled = val;
            this.btnBetaCheck.Enabled = val;
        }

        private void initTabButtons()
        {
            if (this.Type == TypeOfQC.Background)
            {
                this.btnAlphaCheck.Enabled = false;
                this.btnBetaCheck.Enabled = false;
            }
            else if (this.Type == TypeOfQC.Alpha)
                this.btnBetaCheck.Enabled = false;
        }

        private void resetTabButtons()
        {
            if (this.Type == TypeOfQC.Background)
            {
                this.btnBackgroundCheck.Enabled = true;
                this.btnAlphaCheck.Enabled = true;
                this.btnBetaCheck.Enabled = false;
            }
            else 
                this.enableTabButtons(true);
        }

        public void btnStartCount_Click(object sender, EventArgs e)
        {          
            if(!this.Prepare4Start())
                return;
            if (this.Type == TypeOfQC.Background)
            {
                QCBG = new QCBackgroundListener(this.DABRAS, Sampletime, NumSamples, this.ShortDataGridView, this.bkgAlphaHi, this.bkgAlphaLo, this.bkgBetaHi, this.bkgBetaLo, DateTime.Now, BadgeNo, this.Name_TB.Text);
                QCBG.setCallerForm(this.frmParent);

                OperationThread_QCBG = new Thread(() => QCBG.Get_Background());
                OperationThread_QCBG.Start();

                QCBG.BackgroundThreadFinished += (s, args) => { InvokeBGCallback(); };
            }
            else
            {
                Radioactive_Source R = ListOfSources.Find(x => x.GetSerialNumber() == "Background");

                bool IsAlpha = (this.Type == TypeOfQC.Alpha);

                if (IsAlpha)
                {
                    QCAB = new QCAlphaBetaListener(this.DABRAS, Sampletime, NumSamples, this.ShortDataGridView, this.AlphaHi, this.AlphaLo, IsAlpha, R.GetDailyAlphaCPM(), R.GetDailyBetaCPM(), DateTime.Now, BadgeNo, this.Name_TB.Text);
                    QCAB.setCallerForm(this.frmParent);
                }
                else
                {
                    QCAB = new QCAlphaBetaListener(this.DABRAS, Sampletime, NumSamples, this.ShortDataGridView, this.BetaHi, this.BetaLo, IsAlpha, R.GetDailyAlphaCPM(), R.GetDailyBetaCPM(), DateTime.Now, BadgeNo, this.Name_TB.Text);
                    QCAB.setCallerForm(this.frmParent);
                }

                OperationThread_QCAB = new Thread(() => QCAB.Get_Background());
                OperationThread_QCAB.Start();

                QCAB.BackgroundThreadFinished += (s, args) => { InvokeABCallback(); };
            }
        }
        #endregion

        #region set check buttons
        private void setCheckButtons()
        {
            this.btnStartCount.Enabled = true;
            this.StopButton.Enabled = false;

            if (Type == TypeOfQC.Background)
            {
                this.btnAlphaCheck.Enabled = true;
                this.btnBetaCheck.Enabled = false;
            }
            if (Type == TypeOfQC.Alpha)
            {
                this.btnAlphaCheck.Enabled = true;
                this.btnBackgroundCheck.Enabled = true;
                this.btnBetaCheck.Enabled = true;
            }
            if (Type == TypeOfQC.Beta)
            {
                this.btnBackgroundCheck.Enabled = true;
                this.btnAlphaCheck.Enabled = true;
                this.btnBetaCheck.Enabled = true;
            }
        }
        #endregion

        #region check buttons' handlers
        private void btnBackgroundCheck_Click(object sender, EventArgs e)
        {
            this.btnBackgroundCheck.BackColor = Color.YellowGreen;
            this.btnAlphaCheck.BackColor = Color.LightSteelBlue;
            this.btnBetaCheck.BackColor = Color.LightSteelBlue;
            this.btnAlphaCheck.Enabled = false;
            this.btnBetaCheck.Enabled = false;
            this.ResetQCType(TypeOfQC.Background);
            if (this.BackgroundData == null)
                this.btnAlphaCheck.Enabled = false;
            else
                this.btnAlphaCheck.Enabled = true;
        }

        private void btnAlphaCheck_Click(object sender, EventArgs e)
        {
            this.btnAlphaCheck.BackColor = Color.YellowGreen;
            this.btnBackgroundCheck.BackColor = Color.LightSteelBlue;
            this.btnBetaCheck.BackColor = Color.LightSteelBlue;
            this.btnBackgroundCheck.Enabled = true;
            this.btnAlphaCheck.Enabled = true;

            this.ResetQCType(TypeOfQC.Alpha);
            if (this.AlphaEffData == null)
                this.btnBetaCheck.Enabled = false;
            else
                this.btnBetaCheck.Enabled = true;
        }

        private void btnBetaCheck_Click(object sender, EventArgs e)
        {
            this.btnBetaCheck.BackColor = Color.YellowGreen;
            this.btnBackgroundCheck.BackColor = Color.LightSteelBlue;
            this.btnAlphaCheck.BackColor = Color.LightSteelBlue;
            this.btnAlphaCheck.Enabled = true;
            this.btnBetaCheck.Enabled = true;
            this.btnBackgroundCheck.Enabled = true;
            this.ResetQCType(TypeOfQC.Beta);
        }
        #endregion

        #region Show Full Data Set Handler
        private void ShowFullDataSetButton_Click(object sender, EventArgs e)
        {
            FullDataSetPopup NewForm = new FullDataSetPopup( this.frmParent.GetLogger(), this.WriteQCSummary(), this.Sampletime);
            NewForm.ShowDialog();
        }
        #endregion

        #region Background Calibration Handlers
        private void SetButton_Click(object sender, EventArgs e)
        {
            this.initDataGrid();
        }
        #endregion

        #region Show Graph Handler
        private void ShowGraphButton_Click(object sender, EventArgs e)
        {
            DailyQCGraph NewForm = new DailyQCGraph(this, this.QC_List, this.frmParent.GetLogger(), this.ListOfSources, this.ListOfFamily, this.DC);
            this.ChildDialogue = NewForm;
            NewForm.Show();
            this.Hide();

            //if (NewForm.DialogResult == DialogResult.Abort)
            //{
            //    AbortAll();
            //}
        }
        #endregion

        #region Misc GUI Functions
        private void ActivateSaveButtons(bool isActive)
        {
            CVSSaveButton.Enabled = isActive;
            btn_saveMonthlyReport.Enabled = isActive;
            ShowGraphButton.Enabled = isActive;
            ShowFullDataSetButton.Enabled = isActive;
            ImageSaveButton.Enabled = isActive;
            ShowLogButton.Enabled = isActive;
        }
        private void SetGUI(bool Running)
        {
            this.btnStartCount.Enabled = !Running;
            this.StopButton.Enabled = Running;
            this.ActivateSaveButtons(!Running);

            this.Counts_TB.Enabled = !Running;
            this.Min_TB.Enabled = !Running;
            this.Sec_TB.Enabled = !Running;
            this.Badge_CB.Enabled = !Running;
            this.Name_TB.Enabled = !Running;
            this.btnAddUser.Enabled = !Running;
            this.btnRemoveUser.Enabled = !Running;

            return;
        }

        #endregion

        #region Close Button Handler
        private void closeWindowCtrlQToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        }
        #endregion

        #region ShowLog Button Handler
        private void ShowLogButton_Click(object sender, EventArgs e)
        {
            if (QC_List.GetFullList().Count > 0)
            {
                DailyQCLog NewForm = new DailyQCLog(this, QC_List.GetFullList(), DABRAS, this.frmParent.GetLogger());
                NewForm.Show();
                //this.Hide();
            }
            else
            {
                MessageBox.Show("No results to display.");
            }            
        }
        #endregion

        #region CSV Save Handler
        private void SaveButton_Click(object sender, EventArgs e)
        {
            string[,] DataToWrite = this.frmParent.MakeDataWritable(ShortDataGridView);
            SaveFileDialog SD = new SaveFileDialog();
            SD.Filter = "Comma Separated Value|*.csv";
            SD.ShowDialog();
            if (SD.FileName != "")
            {
                FileStream F = (FileStream)SD.OpenFile();
                this.frmParent.GetLogger().WriteCSV(F, DataToWrite);
                MessageBox.Show("Data Written.");
            }
        }
        #endregion

        #region Save Image Handler
        private void ImageSaveButton_Click(object sender, EventArgs e)
        {
            Rectangle Bounds = this.Bounds;
            using (Bitmap b = new Bitmap(Bounds.Width, Bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(b))
                {
                    g.CopyFromScreen(new Point(Bounds.Left, Bounds.Top), Point.Empty, Bounds.Size);
                }

                SaveFileDialog SD = new SaveFileDialog();
                SD.Filter = "JPEG|*.jpeg";
                SD.ShowDialog();
                if (SD.FileName != "")
                {
                    b.Save(SD.FileName, ImageFormat.Jpeg);
                    MessageBox.Show("Done!");
                }
            }
            return;
        }
        #endregion

        #region Abort Handler
        private void AbortAll()
        {
            if (QCBG != null)
            {
                QCBG.RequestStop();
                while (QCBG.IsRunning())
                {
                    Thread.Sleep(1);
                }
            }

            if (QCAB != null)
            {
                QCAB.RequestStop();
                while (QCAB.IsRunning())
                {
                    Thread.Sleep(1);
                }
            }

            this.DialogResult = DialogResult.Abort;
            this.Close();
            return;
        }
        #endregion

        #region Stop Handler
        public void StopButton_Click(object sender, EventArgs e)
        {
            if (QCAB != null)
            {
                QCAB.RequestStop();
            }

            if (QCBG != null)
            {
                QCBG.RequestStop();
            }

            this.SetGUI(false);
        }
        #endregion

        #region Getters Setters
        public bool WasDABRASModified()
        {
            return this.DABRASModified;
        }

        public DABRAS GetDABRAS()
        {
            return this.DABRAS;
        }

        public List<Radioactive_Source> GetModifiedSources()
        {
            return this.ListOfSources;
        }

        public bool WereSourcesModified()
        {
            return this.SourcesModified;
        }

        public bool WasDCModified()
        {
            return this.DCModified;
        }

        public DefaultConfigurations GetDefaultConfig()
        {
            return this.DC;
        }

        public QCListKeeper GetQCList()
        {
            return this.QC_List;
        }

        public TypeOfQC Type
        {
            get { return _typ; }
            set { _typ = value; }
        }

        public string[,] BackgroundData
        {
            get { return this.bkgrdData; }
            set { this.bkgrdData = value; }
        }
        public string[,] AlphaEffData
        {
            get { return this.eff_Data_alpha; }
            set { this.eff_Data_alpha = value; }
        }
        public string[,] BetaEffData
        {
            get { return this.eff_Data_beta; }
            set { this.eff_Data_beta = value; }
        }
        #endregion

        #region KeyPress Handler
        private void KeyPressed(object sender, KeyEventArgs Key)
        {
            if (Key.Control && Key.Alt && Key.Shift)
            {
                AbortAll();
            }

            if (Key.Control)
            {
                if (Key.KeyCode == Keys.Q)
                {
                    closeWindowCtrlQToolStripMenuItem_Click(this, null);
                    return;
                }
            }

            if (Key.KeyCode == Keys.Enter)
            {
                FillEmployeeData(this, null);
            }
        }
        #endregion

        #region Finalization
        public void QC_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.writeQCdata();
            this.endFormActivities();
        }

        public void endFormActivities()
        {
            //Stop the BG listener, if it exists
            if (QCBG != null)
            {
                QCBG.RequestStop();
            }

            //Stop the AB listener, if it exists
            if (QCAB != null)
            {
                QCAB.RequestStop();
            }

            //Wait for the background thread to terminate
            if (OperationThread_QCBG != null)
            {
                OperationThread_QCBG.Join();//added by QZ to block the current thread until the QCAB's thread terminates
            }
            if (OperationThread_QCAB != null)
            {
                OperationThread_QCAB.Join();//added by QZ to block the current thread until the QCAB's thread terminates
            }
        }

        private void writeQCdata()
        {
            //Check to see if there is anything of value to write
            if (QC_List.IsNew())
            {
                string CurrentDir = Environment.CurrentDirectory;
                string DataPath = String.Concat(CurrentDir, "\\data\\QC\\Master\\QC_Data.dat");

                try
                {
                    //Create the file if it doesn't exist
                    if (!File.Exists(DataPath))
                    {
                        File.Create(DataPath).Dispose();
                    }

                    IFormatter BF = new BinaryFormatter();
                    using (Stream FileStream = new FileStream(DataPath, FileMode.Create, FileAccess.Write))
                    {
                        BF.Serialize(FileStream, QC_List);
                    }

                    //MessageBox.Show("QC list written to file " + DataPath);
                }
                catch
                {
                    MessageBox.Show("Error in QC Master List Serialization.");
                }
            }
        }
        #endregion

        private void Counts_TB_TextChanged(object sender, EventArgs e)
        {
            try
            {
                this.NumSamples = Convert.ToInt32(this.Counts_TB.Text);
            }
            catch
            {
                MessageBox.Show("Error: Counts must be an integer greater than zero. Please try again.");
                return;
            }
            this.ClearDataGridView(this.ShortDataGridView);
            this.Add_Or_Subtract_Rows(this.NumSamples, this.ShortDataGridView);
            this.ShortDataGridView.Invalidate();
        }

        private void btn_saveMonthlyReport_Click(object sender, EventArgs e)
        {
            //Write to two CSV files
            string sumDir = String.Format("{0}\\data\\QC\\Monthly", Environment.CurrentDirectory);
            string sumMonthlyFile = String.Format("{0}\\data\\QC\\Monthly\\{1}.csv", Environment.CurrentDirectory, String.Format("{0:MMMM_yyyy}", DateTime.Now));
            try
            {
                if (!Directory.Exists(sumDir))
                {
                    Directory.CreateDirectory(sumDir);
                }
                if (!File.Exists(sumMonthlyFile))
                {
                    File.Create(sumMonthlyFile).Dispose();
                    using (FileStream F = new FileStream(sumMonthlyFile, FileMode.OpenOrCreate))
                    {
                        this.frmParent.GetLogger().WriteSummaryData(F, null, null, true);
                    }
                }
                using (FileStream F = new FileStream(sumMonthlyFile, FileMode.Append))
                {
                    if(this.frmParent.GetLogger().WriteSummaryData(F, this.WriteMetaData(this.Name_TB.Text), this.WriteQCSummary(), false))
                        MessageBox.Show("Summary data written.");
                }
            }
            catch (Exception wex)
            {
                MessageBox.Show("Writing summary data failed--" + wex.Message);
            }        
        }
    }
}
