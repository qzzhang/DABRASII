using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace DABRAS_Software
{
    public partial class FullDataSetPopup : Form
    {
        #region Data Members
        private QCAlphaBetaListener AB_Listener = null;
        private QCBackgroundListener BG_Listener = null;
        private Logger Logger;
        private IDictionary<string, string> sumData;
        #endregion

        #region Constructor
        public FullDataSetPopup(Logger _L, IDictionary<string, string> sumry, int acqTime)
        {
            InitializeComponent();

            this.sumData = sumry;
            this.Logger = _L;
            
            FullDataSet.Columns.Add("FullDataSet_Date", "Date");
            FullDataSet.Columns.Add("FullDataSet_Time", "Acq Time");
            FullDataSet.Columns.Add("FullDataSet_BKGGCPM_A", "Bkg Rate (alpha)");
            FullDataSet.Columns.Add("FullDataSet_BKGGCPM_B", "Bkg Rate (beta)");
            FullDataSet.Columns.Add("FullDataSet_PassFail", "Background Pass/Fail");
            FullDataSet.Columns.Add("FullDataSet_AlphaGCPM", "Gross alpha Source Response");
            FullDataSet.Columns.Add("FullDataSet_AlphaNCPM", "Net alpha Source Response");
            FullDataSet.Columns.Add("FullDataSet_ALL", "Alpha LL");
            FullDataSet.Columns.Add("FullDataSet_AUL", "Alpha UL");
            FullDataSet.Columns.Add("FullDataSet_PassFail", "Alpha Pass/Fail");
            FullDataSet.Columns.Add("FullDataSet_BetaGCPM", "Gross beta Source Response");
            FullDataSet.Columns.Add("FullDataSet_BetaNCPM", "Net beta Source Response");
            FullDataSet.Columns.Add("FullDataSet_BLL", "Beta LL");
            FullDataSet.Columns.Add("FullDataSet_BUL", "Beta UL");
            FullDataSet.Columns.Add("FullDataSet_PassFail", "Beta Pass/Fail");

            string bkgRate_a, bkgRate_b, bkgPass_Fail, aSrc_g, aSrc_n, aLL, aUL, alphaPass_Fail, bSrc_g, bSrc_n, bLL, bUL, betaPass_Fail;
            if (this.sumData.ContainsKey("Bkg Rate (alpha)"))
                bkgRate_a = this.sumData["Bkg Rate (alpha)"];
            else
                bkgRate_a = "";
            if (this.sumData.ContainsKey("Bkg Rate (beta)"))
                bkgRate_b = this.sumData["Bkg Rate (beta)"];
            else
                bkgRate_b = "";
            if (this.sumData.ContainsKey("Background Pass/Fail"))
                bkgPass_Fail = this.sumData["Background Pass/Fail"];
            else
                bkgPass_Fail = "";

            if (this.sumData.ContainsKey("Gross alpha Source Response"))
                aSrc_g = this.sumData["Gross alpha Source Response"];
            else
                aSrc_g = "";
            if (this.sumData.ContainsKey("Net alpha Source Response"))
                aSrc_n = this.sumData["Net alpha Source Response"];
            else
                aSrc_n = "";
            if (this.sumData.ContainsKey("Alpha LL"))
                aLL = this.sumData["Alpha LL"];
            else
                aLL = "";
            if (this.sumData.ContainsKey("Alpha UL"))
                aUL = this.sumData["Alpha UL"];
            else
                aUL= "";
            if (this.sumData.ContainsKey("Pass/Fail Alpha"))
                alphaPass_Fail = this.sumData["Pass/Fail Alpha"];
            else
                alphaPass_Fail = "";

            if (this.sumData.ContainsKey("Gross beta Source Response"))
                bSrc_g = this.sumData["Gross beta Source Response"];
            else
                bSrc_g = "";
            if (this.sumData.ContainsKey("Net beta Source Response"))
                bSrc_n = this.sumData["Net beta Source Response"];
            else
                bSrc_n = "";
            if (this.sumData.ContainsKey("Beta LL"))
                bLL = this.sumData["Beta LL"];
            else
                bLL = "";
            if (this.sumData.ContainsKey("Beta UL"))
                bUL = this.sumData["Beta UL"];
            else
                bUL = "";
            if (this.sumData.ContainsKey("Pass/Fail Beta"))
                betaPass_Fail = this.sumData["Pass/Fail Beta"];
            else
                betaPass_Fail = "";

            FullDataSet.Rows.Add(String.Format("{0:MM/dd/yyyy}", DateTime.Now), String.Format("{0}:{1}", acqTime / 60, acqTime % 60), 
                bkgRate_a, bkgRate_b, bkgPass_Fail, aSrc_g, aSrc_n, aLL, aUL, alphaPass_Fail, bSrc_g, bSrc_n, bLL, bUL, betaPass_Fail);
        }
        #endregion

        #region Private Utility Functions
        private string[,] MakeDataWritable(DataGridView DG)
        {

            int Rows = DG.RowCount + 1;
            int Cols = DG.ColumnCount;

            string[,] ReturnString = new string[Rows, Cols];

            foreach (DataGridViewColumn i in DG.Columns)
            {
                ReturnString[0, i.Index] = i.HeaderText;
            }


            for (int i = 1; i < DG.RowCount + 1; i++)
            {
                for (int j = 0; j < DG.ColumnCount; j++)
                {
                    ReturnString[i, j] = "";
                    ReturnString[i, j] = Convert.ToString(DG[j, i - 1].Value);
                }
            }

            return ReturnString;
        }
        #endregion

        #region Write Handler
        private void writeToFileCtrlVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string[,] DataToWrite = MakeDataWritable(this.FullDataSet);

            SaveFileDialog S = new SaveFileDialog();
            S.Filter = "Comma Separated Value|*.csv";
            S.ShowDialog();
            if (S.FileName != "")
            {
                FileStream F = (FileStream)S.OpenFile();
                string FilePath = S.FileName;
                this.Logger.WriteCSV(F, DataToWrite);
            }

            return;
        }
        #endregion

        #region Close Handler
        private void closeCtrlQToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        }
        #endregion

        #region KeyPress Handler
        private void KeyPressed(object sender, KeyEventArgs Key)
        {
            if (Key.Control && Key.Alt && Key.Shift)
            {
                this.DialogResult = DialogResult.Abort;
                this.Close();
                return;
            }

            if (Key.Control)
            {
                if (Key.KeyCode == Keys.Q)
                {
                    closeCtrlQToolStripMenuItem_Click(this, null);
                }
            }
        }
        #endregion
    }
}
