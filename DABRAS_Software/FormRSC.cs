using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;

namespace DABRAS_Software
{
    public partial class FormRSC : Form
    {
        #region Enums
        public enum RoutineSampleCountType { Time, MDA };
        #endregion

        #region Data Members
        private mainFramework frmParent;
        private DABRAS dabras;
        private RoutineSampleCountType RType = RoutineSampleCountType.Time;
        private RoutineSampleListener RL;
        private Thread BackgroundThread;

        private const double ConfidenceRange = 1.645;
        private const double ConfRange = 1.645; //95 % confidence range
        private double Current_Alpha_Eff = 100.0;//100%
        private double Current_Beta_Eff = 100.0;//100%

        private double Alpha_SelfAbsorbtion = 0;
        private double Beta_SelfAbsorbtion = 0;
        private double Beta_Backscatter = 0;
        private double Sample_Area = 0;
        private double AlphaMDA = 0;
        private double BetaMDA = 0;

        private int SampleTime = 0;
        private int index = 1; //starting from 1, incremented every time a sample is taken.

        public delegate void UpdateFormCallback();

        private bool Calibrating = false; //This should ONLY be set to true when using in quickcal mode

        private double AlphaGCPM;
        private double BetaGCPM;

        private double AlphaDPM;
        private double BetaDPM;

        private mainFramework.BackgroundType BGType;
        private ModFactors MF;
        private ModFactors UserMF;
        public string jobName = "";
        public int bdgNum = 0;
        private bool DefaultMF;
        private bool savedRSC = true;
        #endregion

        #region Constructor
        public FormRSC(mainFramework _parent)
        {
            this.frmParent = _parent;
            this.dabras = this.frmParent.GetDABRAS();
            InitializeComponent();
            this.initRSC();
            this.initRSCgridByFile();
            this.SetGUI_RSC(false);
            this.Continue_Count_Button.Enabled = false; //At the birth of this form, there is no suspended counting, so the continue button should not be enabled.

            //Add keypress event handlers
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(KeyPressed);
            this.BGType = mainFramework.BackgroundType.Annual;
            this.badge_TB.Text = Convert.ToString(this.frmParent.QCuserID);
            this.bdgNum = this.verifyBadgeNum();
            this.jobName = this.verifyJobName();
            this.savedRSC = true;
            this.lbl_BackgroundType.Text = "Currently set to: " + "Annual";
            this.UserMF = new ModFactors();
        }
        #endregion

        // To ensure KeyPress of character keys can be detected
        // even if the focus is on an item of the form instead of the keyboard
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Alt | Keys.C ))
            {
                MessageBox.Show("You pressed Alt+C!");
                if (this.New_Count_Button.Enabled)
                {
                    this.New_Count_Button_Click(this, null);
                }
            }
            else if (keyData == (Keys.Alt | Keys.S))
            {
                MessageBox.Show("You pressed Alt+S!");
                if (this.Stop_Count_Button.Enabled)
                {
                    this.Stop_Count_Button_Click(this, null);
                }
            }
            else if (keyData == (Keys.Alt | Keys.R))
            {
                MessageBox.Show("You pressed Alt+R!");
                if (this.Continue_Count_Button.Enabled)
                {
                    this.Continue_Count_Button_Click(this, null);
                }
            }
            else if (keyData == (Keys.Alt | Keys.P))
            {
                MessageBox.Show("You pressed Alt+P!");
                if (this.PauseButton.Enabled)
                {
                    this.PauseButton_Click(this, null);
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        #region New Count Handler
        private void ClearDataGridView(DataGridView DG)
        {
            for (int i = 0; i < DG.RowCount; i++)
            {
                for (int j = 1; j < DG.ColumnCount; j++)
                {
                    DG[j, i].Value = "";
                }
            }
        }

        private void New_Count_Button_Click(object sender, EventArgs e)
        {
            if (!this.dabras.IsConnected())
            {
                MessageBox.Show("Error: No connection to the DABRAS. Please re-connect, and try again.");
                this.frmParent.refreshConnectStatus();
                this.endFormActivities();
                return;
            }
            this.frmParent.EnableRSCForm(true);
            if (this.frmParent.GetSelectedTab() != 1)
                return;

            if (this.setCountingParameters() == false)
                return;

            double Alpha_Limit = 0;
            double Beta_Limit = 0;
            RadionuclideFamily AlphaRF = this.frmParent.GetListOfFamily().Find(x => x.GetName() == Alpha_ComboBox.Text);
            RadionuclideFamily BetaRF = this.frmParent.GetListOfFamily().Find(x => x.GetName() == Beta_ComboBox.Text);
            Radioactive_Source BackgroundSource = this.frmParent.GetListOfSources().Find(x => x.GetSerialNumber() == "Background");
            if (BackgroundSource == null || AlphaRF == null || BetaRF == null)
            {
                MessageBox.Show("Error: There must be calibration done first.");
                return;
            }

            try
            {
                this.Alpha_SelfAbsorbtion = Convert.ToDouble(this.Alpha_Absorption_Mod_TB.Text);
                this.Beta_SelfAbsorbtion = Convert.ToDouble(this.Beta_Absorption_Mod_TB.Text);
                this.Beta_Backscatter = Convert.ToDouble(this.Beta_Backscatter_Mod_TB.Text);

                Alpha_Limit = Convert.ToDouble(this.GrossAlphaDPMLimit_TB.Text);
                Beta_Limit = Convert.ToDouble(this.GrossBetaDPMLimit_TB.Text);

                this.Sample_Area = Convert.ToDouble(Area_Mod_TB.Text);
            }
            catch
            {
                MessageBox.Show("Invalid values");
                return;
            }
            MessageBox.Show("EventArgs: " + e);
            // Using e != null to ensure a non-hotkey (a MouseEventArgs) was pressed
            if (!Calibrating  && e != null)
            {
                if (this.RType == RoutineSampleCountType.Time)
                {
                    if ((MessageBox.Show(String.Format("Count for {0}:{1:00}?", ((SampleTime + 0) / 60), ((SampleTime + 0) % 60)), "Confirm", MessageBoxButtons.YesNo)) != DialogResult.Yes)
                    {
                        MessageBox.Show("Aborted.");
                        return;
                    }
                }
                else if (this.RType == RoutineSampleCountType.MDA)
                {
                    if (MessageBox.Show(String.Format("Count until an alpha MDA of {0} and a beta MDA of {1} is reached?", AlphaMDA, BetaMDA), "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes)
                    {
                        MessageBox.Show("Aborted");
                        return;
                    }
                }
            }

            this.startNewJob();

            Elapsed_Time_Label.Text = "Elapsed time: 0:00";

            Status_Label.Text = "Acquiring";

            // string NewColumnName = String.Format("NewColumn{0}", this.index);

            if (FullDataResults.ColumnCount > 1)
            {
                DataGridViewColumn DGC = (DataGridViewColumn)FullDataResults.Columns[1].Clone();
                DGC.HeaderText = Convert.ToString(this.index);
                FullDataResults.Columns.Insert(1, DGC);
                FullDataResults[1, 1].ReadOnly = false; //Allow for user to edit description field
            }
            else
            {
                FullDataResults.Columns.Add("NewColumn", Convert.ToString(this.index));
                FullDataResults.Columns["NewColumn"].SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            FullDataResults["NewColumn", 0].Value = this.badge_TB.Text;
            FullDataResults["NewColumn", 1].Value = "Default description";
            FullDataResults["NewColumn", 1].Selected = true;
            FullDataResults["NewColumn", 2].Value = DateTime.Now;

            if (this.RType == RoutineSampleCountType.Time)
            {
                FullDataResults["NewColumn", 4].Value = 0;//String.Format("{0}:{1:00}", ((SampleTime + 0) / 60), ((SampleTime + 0) % 60));
            }

            FullDataResults["NewColumn", 21].Value = StaticMethods.RoundToDecimal(this.Current_Alpha_Eff, 4);
            FullDataResults["NewColumn", 22].Value = StaticMethods.RoundToDecimal(this.Current_Beta_Eff, 4);
            FullDataResults["NewColumn", 23].Value = this.Alpha_ComboBox.Text;
            FullDataResults["NewColumn", 24].Value = this.Beta_ComboBox.Text;

            FullDataResults["NewColumn", 25].Value = this.Alpha_SelfAbsorbtion;
            FullDataResults["NewColumn", 26].Value = this.Beta_SelfAbsorbtion;
            FullDataResults["NewColumn", 27].Value = this.Beta_Backscatter;
            FullDataResults["NewColumn", 28].Value = this.Sample_Area;

            FullDataResults["NewColumn", 29].Value = String.Format("{0:G3}", Alpha_Limit);
            FullDataResults["NewColumn", 30].Value = String.Format("{0:G3}", Beta_Limit);

            if (this.UsingAnnual())
            {
                FullDataResults["NewColumn", 31].Value = BackgroundSource.GetAnnualCalibratedTime();
                FullDataResults["NewColumn", 32].Value = BackgroundSource.GetAnnualCalibratedTimespan();
                FullDataResults["NewColumn", 33].Value = StaticMethods.RoundToDecimal(BackgroundSource.GetAnnualAlphaCPM(), 2);
                FullDataResults["NewColumn", 34].Value = StaticMethods.RoundToDecimal(BackgroundSource.GetAnnualBetaCPM(), 2);
            }
            else if (UsingDaily())
            {
                FullDataResults["NewColumn", 31].Value = BackgroundSource.GetDailyCalibratedTime();
                FullDataResults["NewColumn", 32].Value = BackgroundSource.GetDailyCalibratedTimespan();
                FullDataResults["NewColumn", 33].Value = StaticMethods.RoundToDecimal(BackgroundSource.GetDailyAlphaCPM(), 2);
                FullDataResults["NewColumn", 34].Value = StaticMethods.RoundToDecimal(BackgroundSource.GetDailyBetaCPM(), 2);
            }

            //Don't allow the user to edit anything except for the description (It doesn't work the other way!)
            for (int i = 0; i < FullDataResults.RowCount; i++)
            {
                if (i != 1)
                {
                    FullDataResults[1, i].ReadOnly = true;
                }
            }

            if (this.UsingAnnual())
            {
                RL = new RoutineSampleListener(this.dabras, SampleTime, 0, this.FullDataResults, BackgroundSource.GetAnnualAlphaCPM(), BackgroundSource.GetAnnualBetaCPM(), this.Current_Alpha_Eff, this.Current_Beta_Eff, this.Alpha_SelfAbsorbtion, this.Beta_SelfAbsorbtion, this.Beta_Backscatter, this.Sample_Area, BackgroundSource.GetAnnualCalibratedTimespan(), RType, this.AlphaMDA, this.BetaMDA);
            }

            if (UsingDaily())
            {
                RL = new RoutineSampleListener(this.dabras, SampleTime, 0, this.FullDataResults, BackgroundSource.GetDailyAlphaCPM(), BackgroundSource.GetDailyBetaCPM(), this.Current_Alpha_Eff, this.Current_Beta_Eff, this.Alpha_SelfAbsorbtion, this.Beta_SelfAbsorbtion, this.Beta_Backscatter, this.Sample_Area, BackgroundSource.GetDailyCalibratedTimespan(), RType, this.AlphaMDA, this.BetaMDA);
            }

            RL.setCallerForm(this.frmParent);

            BackgroundThread = new Thread(() => RL.Run_Sample());
            BackgroundThread.Start();

            RL.BackgroundSampleThreadFinished += (s, args) => { InvokeCallback(); };
            RL.PacketReceived += (s, args) => { InvokeCallback(); };

            this.SetGUI_RSC(true);
            this.Alpha_Activity_TB.ForeColor = Color.Black;
            this.Beta_Activity_TB.ForeColor = Color.Black;
            this.frmParent.isAcquiring = true;

            this.index++;
        }

        public bool UsingAnnual()
        {
            return (this.BGType == mainFramework.BackgroundType.Annual);
        }
        public bool UsingDaily()
        {
            return (this.BGType == mainFramework.BackgroundType.Daily);
        }

        #endregion

        #region Show_Full_Data Checkbox Handlers
        private void Show_Full_Data_Checkbox_CheckedChanged(object sender, EventArgs e)
        {
            this.FullDataResults.Visible = Show_Full_Data_Checkbox.Checked;
            return;
        }
        #endregion

        #region Continue Sample Handler
        private void Continue_Count_Button_Click(object sender, EventArgs e)
        {
            if (RL != null)
            {
                if (RL.IsPaused())
                {
                    RL.Continue();
                }
            }
            this.Status_Label.Text = "Acquiring";
            this.SetGUI_RSC(true);
            this.frmParent.isAcquiring = true;
        }
        #endregion

        #region Pause Handler
        private void PauseButton_Click(object sender, EventArgs e)
        {
            if (RL != null)
            {
                RL.RequestPause();
            }
            this.Status_Label.Text = "Paused";
            this.SetGUI_RSC(false);
        }
        #endregion

        #region Stop Sample Handler
        private void Stop_Count_Button_Click(object sender, EventArgs e)
        {
            if (RL != null)
            {
                RL.RequestStop();
                BackgroundThread.Join();
            }

            this.FullDataResults["NewColumn", 3].Value = "CANCELLED";
            this.Status_Label.Text = "Stopped";

            this.SetGUI_RSC(false);
            this.frmParent.isAcquiring = false;
        }
        #endregion

        #region Set Background Type Handler
        private void btn_SetBackgroundType_Click(object sender, EventArgs e)
        {
            if (this.frmParent.checkPassword())
            {
                BackgroundTypeForm NewForm = new BackgroundTypeForm(this.BGType);

                NewForm.ShowDialog();

                if (NewForm.DialogResult == DialogResult.OK)
                {
                    this.BGType = NewForm.GetBackgroundType();
                    this.frmParent.updRSCbackgroundType(this.BGType);
                    if (this.BGType == mainFramework.BackgroundType.Daily)
                        this.lbl_BackgroundType.Text = "Currently set to: " + "Daily";
                    else
                        this.lbl_BackgroundType.Text = "Currently set to: " + "Annual";

                    this.initDataResults();
                }

                if (NewForm.DialogResult == DialogResult.Abort)
                {
                    //AbortAll();
                }
            }
            else
                MessageBox.Show("Authorization failed.");
        }
        private bool setCountingParameters()
        {
            int num_min,  num_sec;
            string str_min, str_sec;
            try
            {
                if (Min_TB.Text == "")
                    num_min = 0;
                else
                    num_min = Convert.ToInt32(Min_TB.Text);
                if (Sec_TB.Text == "")
                    num_sec = 0;
                else
                    num_sec = Convert.ToInt32(Sec_TB.Text);
                if(num_sec > 60 )
                {
                    num_min += num_sec / 60;
                    num_sec = num_sec % 60;
                }
                if (num_sec == 0)
                    str_sec = "00";
                else
                    str_sec = String.Format("{0:00}", num_sec);
                if (num_min == 0)
                    str_min = "00";
                else
                    str_min = String.Format("{0:00}", num_min);

                //this.SampleTime = (60 * num_min) + num_sec - 1;
                this.SampleTime = (60 * num_min) + num_sec;

                this.AlphaMDA = Convert.ToDouble(this.AlphaMDALimit_TB.Text);
                this.BetaMDA = Convert.ToDouble(this.BetaMDALimit_TB.Text);

                if (this.RType == RoutineSampleCountType.Time)
                {
                    this.Preset_Time_Label.Text = "Preset time: " + str_min + ":" + str_sec;
                }
                else if (this.RType == RoutineSampleCountType.MDA)
                {
                    this.Preset_Time_Label.Text = String.Format("Alpha MDA < {0}\nBeta MDA < {1}", this.AlphaMDA, this.BetaMDA);
                }

                return true;
            }

            catch
            {
                MessageBox.Show("Error: Bad Values");
                return false;
            }
        }
        #endregion

        #region Combobox_Handlers
        private void Alpha_ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            RadionuclideFamily RF = this.frmParent.GetListOfFamily().Find(x => x.GetName() == Alpha_ComboBox.Text);

            //Set Alpha Efficiency
            if (RF != null)
            {
                this.Current_Alpha_Eff = RF.GetCurrentSource().GetAlphaEfficiency();
                //if (Math.Abs(this.Current_Alpha_Eff) < 0.01)
                this.lblAlphaEff.Text = StaticMethods.RoundToDecimal(this.Current_Alpha_Eff, 4);
                /*else
                    this.lblAlphaEff.Text = StaticMethods.RoundToDecimal(this.Current_Alpha_Eff, 3);*/
            }
        }

        private void Beta_ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            RadionuclideFamily RF = this.frmParent.GetListOfFamily().Find(x => x.GetName() == Beta_ComboBox.Text);

            //Set Beta Efficiency
            if (RF != null)
            {
                this.Current_Beta_Eff = RF.GetCurrentSource().GetBetaEfficiency();
                //if (Math.Abs(this.Current_Beta_Eff) < 0.01)
                this.lblBetaEff.Text = StaticMethods.RoundToDecimal(this.Current_Beta_Eff, 4);
                /*else
                    this.lblBetaEff.Text = StaticMethods.RoundToDecimal(this.Current_Beta_Eff, 3);*/
            }
            this.SelectCorrectBeta();
        }
        #endregion

        #region Count Type Radiobutton Handlers
        private void TimeRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            SetTypeOfSample();
        }

        private void CountAlphaMDARadioButton_CheckedChanged(object sender, EventArgs e)
        {
            SetTypeOfSample();
        }

        private void CountBetaMDARadioButton_CheckedChanged(object sender, EventArgs e)
        {
            SetTypeOfSample();
        }
        #endregion

        #region Save CSV Button Handler
        private void btnSaveRSCcsv_Click(object sender, EventArgs e)
        {
            DataGridView tempGV = new System.Windows.Forms.DataGridView();
            this.CopyDataGridView(this.FullDataResults, tempGV);

            /*Before writing, remove the 3 rows that contain the following data:
            "Alpha Mod factor"
            "Beta Mod factor"
            "Beta Backscatter Factor"
            */
            tempGV.Rows.RemoveAt(25);
            tempGV.Rows.RemoveAt(25);
            tempGV.Rows.RemoveAt(25);

            //string[,] DataToWrite = this.frmParent.MakeDataWritable(this.FullDataResults);
            string[,] DataToWrite = this.frmParent.MakeDataWritable(tempGV);

            SaveFileDialog S = new SaveFileDialog();
            S.Filter = "Comma Separated Value|*.csv";
            S.ShowDialog();
            if (S.FileName != "")
            {
                FileStream F = (FileStream)S.OpenFile();
                string FilePath = S.FileName;
                this.frmParent.GetLogger().WriteCSV(F, DataToWrite);
            }

            return;
        }
        #endregion

        #region Image Save Handler
        private void ImagePrintButton_Click(object sender, EventArgs e)
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
                }



            }
            MessageBox.Show("Done!");
            return;
        }
        #endregion

        #region Getters
        public double GetAlphaDPM()
        {
            return this.AlphaDPM;
        }

        public double GetBetaDPM()
        {
            return this.BetaDPM;
        }

        public bool WasTestCompleted()
        {
            if (RL != null)
            {
                return RL.WasTestCompleted();
            }

            return false;
        }

        public double GetAlphaGCPM()
        {
            return this.AlphaGCPM;
        }

        public double GetBetaGCPM()
        {
            return this.BetaGCPM;
        }

        public bool WasDABRASModified()
        {
            return this.frmParent.DABRASModified;
        }

        public DABRAS GetDABRAS()
        {
            return this.dabras;
        }

        public bool WasDCModified()
        {
            return this.frmParent.DefaultConfigModified;
        }

        public DefaultConfigurations GetDefaultConfig()
        {
            return this.frmParent.GetDefaultConfig();
        }

        #endregion

        #region KeyPresses

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
                    //closeCtrlQToolStripMenuItem_Click(this, null);
                    return;
                }
            }
        }

        #endregion

        #region Finalization
        private void RoutineSampleCountingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            endFormActivities();
        }
        public void endFormActivities()
        {
            if (RL != null)
            {
                RL.RequestStop();
                BackgroundThread.Join();
            }
            RL = null;
            this.frmParent.isAcquiring = false;
        }
        #endregion

        #region Switching the Modification Factor between user values and default values
        private void switchToDefaultValueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.resetMFValues(this.MF);
            this.switchToDefaultValueToolStripMenuItem.Visible = false;
            this.switchToUserValueToolStripMenuItem.Visible = true;
            this.DefaultMF = true;
            this.Alpha_Absorption_Mod_TB.ForeColor = Color.Black;
            this.Beta_Absorption_Mod_TB.ForeColor = Color.Black;
            this.Beta_Backscatter_Mod_TB.ForeColor = Color.Black;
            this.Area_Mod_TB.ForeColor = Color.Black;
        }

        private void switchToUserValueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.resetMFValues(this.UserMF);
            this.switchToUserValueToolStripMenuItem.Visible = false;
            this.switchToDefaultValueToolStripMenuItem.Visible = true;
            this.DefaultMF = false;

            if (this.UserMF.GetAlphaSelfAbsorbtion() != this.MF.GetAlphaSelfAbsorbtion())
                this.Alpha_Absorption_Mod_TB.ForeColor = Color.Red;
            else
                this.Alpha_Absorption_Mod_TB.ForeColor = Color.Black;

            if (this.UserMF.GetBetaSelfAbsorbtion() != this.MF.GetBetaSelfAbsorbtion())
                this.Beta_Absorption_Mod_TB.ForeColor = Color.Red;
            else
                this.Beta_Absorption_Mod_TB.ForeColor = Color.Black;

            if (this.UserMF.GetBetaBackscatter() != this.MF.GetBetaBackscatter())
                this.Beta_Backscatter_Mod_TB.ForeColor = Color.Red;
            else
                this.Beta_Backscatter_Mod_TB.ForeColor = Color.Black;

            if (this.UserMF.GetDefaultSampleArea() != this.MF.GetDefaultSampleArea())
                this.Area_Mod_TB.ForeColor = Color.Red;
            else
                this.Area_Mod_TB.ForeColor = Color.Black;
        }

        private void Alpha_Absorption_Mod_TB_LostFocus(object sender, EventArgs e)
        {
            double asa = Convert.ToDouble(this.Alpha_Absorption_Mod_TB.Text);
            if (asa != this.MF.GetAlphaSelfAbsorbtion())
            {
                this.Alpha_Absorption_Mod_TB.ForeColor = Color.Red;
                this.UserMF.SetAlphaSelfAbsorbtion(asa);
                this.switchToUserValueToolStripMenuItem_Click(null, null);
            }
            else
                this.Alpha_Absorption_Mod_TB.ForeColor = Color.Black;
        }

        private void Beta_Absorption_Mod_TB_LostFocus(object sender, EventArgs e)
        {
            double bsa = Convert.ToDouble(this.Beta_Absorption_Mod_TB.Text);
            if (bsa != this.MF.GetBetaSelfAbsorbtion())
            {
                this.Beta_Absorption_Mod_TB.ForeColor = Color.Red;
                this.UserMF.SetBetaSelfAbsorbtion(bsa);
                this.switchToUserValueToolStripMenuItem_Click(null, null);
            }
            else
                this.Beta_Absorption_Mod_TB.ForeColor = Color.Black;
        }

        private void Beta_Backscatter_Mod_TB_LostFocus(object sender, EventArgs e)
        {
            double bbs = Convert.ToDouble(this.Beta_Backscatter_Mod_TB.Text);
            if (bbs != this.MF.GetBetaBackscatter())
            {
                this.UserMF.SetBetaBackscatter(bbs);
                this.Beta_Backscatter_Mod_TB.ForeColor = Color.Red;
                switchToUserValueToolStripMenuItem_Click(null, null);
            }
            else
                this.Beta_Backscatter_Mod_TB.ForeColor = Color.Black;
        }

        private void Area_Mod_TB_LostFocus(object sender, EventArgs e)
        {
            double sa = Convert.ToDouble(this.Area_Mod_TB.Text);
            if (sa != this.MF.GetDefaultSampleArea())
            {
                this.UserMF.SetDefaultSampleArea(sa);
                this.Area_Mod_TB.ForeColor = Color.Red;
                switchToUserValueToolStripMenuItem_Click(null, null);
            }
            else
                this.Area_Mod_TB.ForeColor = Color.Black;
        }
        #endregion

        private bool IsCurrentJob(int newb, string newj)
        {
            return (newj == this.jobName && newb == this.bdgNum );
        }
        public void saveOldJob()
        {
            if (this.bdgNum != 0 && !this.savedRSC)
            {
                this.WriteToFiles(this.bdgNum, this.jobName);
                this.savedRSC = true;
            }         
        }
        private void startNewJob()
        {   
            string newJobName = this.verifyJobName();
            int bgNo = this.verifyBadgeNum();
            if (!IsCurrentJob(bgNo, newJobName))
            {
                this.saveOldJob();
                this.jobName = this.verifyJobName();
                this.bdgNum = this.verifyBadgeNum();
                this.index = 1;
                this.initRSC();
            }
        }
        private void SelectCorrectBeta()
        {
            RadionuclideFamily RF = this.frmParent.GetListOfFamily().Find(y => y.GetName() == Beta_ComboBox.Text);

            if (RF.GetSourceType() != RadionuclideFamily.RadiationType.Beta)
            {
                this.groupBeta.Visible = false;
                return;
            }

            //We have a beta source, but the radio buttons are for display only, not for editing
            this.groupBeta.Visible = true;
            this.groupBeta.Enabled = false;

            RadionuclideFamily.EnergyBand CurrentEnergyLevel = RF.GetEnergyBand();

            switch (CurrentEnergyLevel)
            {
                case RadionuclideFamily.EnergyBand.Beta_Less_100KeV:
                    this._100_Energy_Button.Checked = true;
                    this._100_200_Energy_Button.Checked = false;
                    this._200_400_Energy_Button.Checked = false;
                    this._400_1200_Energy_Button.Checked = false;
                    this._1200_Energy_Button.Checked = false;
                    return;
                case RadionuclideFamily.EnergyBand.Beta_100_200KeV:
                    this._100_Energy_Button.Checked = false;
                    this._100_200_Energy_Button.Checked = true;
                    this._200_400_Energy_Button.Checked = false;
                    this._400_1200_Energy_Button.Checked = false;
                    this._1200_Energy_Button.Checked = false;
                    return;
                case RadionuclideFamily.EnergyBand.Beta_200_400KeV:
                    this._100_Energy_Button.Checked = false;
                    this._100_200_Energy_Button.Checked = false;
                    this._200_400_Energy_Button.Checked = true;
                    this._400_1200_Energy_Button.Checked = false;
                    this._1200_Energy_Button.Checked = false;
                    return;
                case RadionuclideFamily.EnergyBand.Beta_400_1200KeV:
                    this._100_Energy_Button.Checked = false;
                    this._100_200_Energy_Button.Checked = false;
                    this._200_400_Energy_Button.Checked = false;
                    this._400_1200_Energy_Button.Checked = true;
                    this._1200_Energy_Button.Checked = false;
                    return;
                case RadionuclideFamily.EnergyBand.Beta_More_1200KeV:
                    this._100_Energy_Button.Checked = false;
                    this._100_200_Energy_Button.Checked = false;
                    this._200_400_Energy_Button.Checked = false;
                    this._400_1200_Energy_Button.Checked = false;
                    this._1200_Energy_Button.Checked = true;
                    return;
                default:
                    this._100_Energy_Button.Checked = false;
                    this._100_200_Energy_Button.Checked = false;
                    this._200_400_Energy_Button.Checked = false;
                    this._400_1200_Energy_Button.Checked = false;
                    this._1200_Energy_Button.Checked = false;
                    return;
            }
        }

        private void btn_openLogFolder_Click(object sender, EventArgs e)
        {
            this.saveOldJob();

            string currDir = String.Format("{0}\\data\\Routine\\{1}", Environment.CurrentDirectory, this.bdgNum);
            try
            {
                if (!Directory.Exists(currDir))
                {
                    currDir = String.Format("{0}\\data\\Routine", Environment.CurrentDirectory);
                }
                System.Diagnostics.Process.Start(currDir);
            }
            catch { }
        }

        private void FormRSC_EnabledChanged(object sender, EventArgs e)
        {
            this.New_Count_Button.Enabled = this.Enabled;
        }
    }
}
