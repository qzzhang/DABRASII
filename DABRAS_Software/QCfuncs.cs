<<<<<<< HEAD
﻿using System;
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
    public partial class FormQC
    {
        #region Background Callback
        public void InvokeBGCallback()
        {
            this.Invoke(new BG_GUI_Callback(this.BG_BackgroundThreadFinished));
        }

        private void BG_BackgroundThreadFinished()
        {
            if (!this.DABRAS.IsConnected())
            {
                this.frmParent.refreshConnectStatus();
                this.endFormActivities();
                return;
            }

            if(!QCBG.WasTestCompleted())
            {
                MessageBox.Show("Background QC testing interrupted!");
                this.lbl_QCtestDate.Text = "Test interrupted, use previous data.";
                return;
            }

            string Comment = "";
            if (QCBG.WasTestPassed() && QCBG.WasTestCompleted())
            {
                Comment = "Passed";
                this.bgPassed = true;
                this.lbl_QCtestDate.Text = "Test passed on " + DateTime.Now.ToString("g");
            }
            else if (!QCBG.WasTestPassed() && QCBG.WasTestCompleted())
            {
                MessageBox.Show("Background QC testing failed.");
                this.bgPassed = false;
                this.lbl_QCtestDate.Text = "Test failed on " + DateTime.Now.ToString("g");
            }

            //Write daily limits to background source
            Radioactive_Source R = this.ListOfSources.Find(x => x.GetSerialNumber() == "Background");

            R.SetDailyAlphaCPM(QCBG.GetAlphaGCPM());
            R.SetDailyBetaCPM(QCBG.GetBetaGCPM());
            R.SetDailyCalibratedDate(DateTime.Now);
            R.SetDailyCalibratedTimespan(QCBG.GetSampleTime() * QCBG.GetNumSamples());
            R.SetDailyCalibrationPassed(this.bgPassed);

            //Add QC object to List
            QCCalResultNode NewResult = new QCCalResultNode(DateTime.Now, TypeOfQC.Background, 0, 0, QCBG.GetBadgeNo(), true, Comment, QCBG.GetName(), QCBG.GetSampleTime());
            QC_List.Add(NewResult);

            //Write results to a file
            this.autoWriteBG2Files();
            this.DCModified = true;
            this.SourcesModified = true;
            this.updateSourcesAndFamily();

            this.OperationThread_QCBG = null;
            this.resetTabButtons();
            this.SetGUI(false);
            this.QCBG.RequestStop();
            this.QCBG = null;
            this.frmParent.isAcquiring = false;
            this.DefaultButtonCheck();
            //this.enableSorting(true, this.ShortDataGridView);
        }

        public bool QCbackgroundPassed()
        {
            return this.bgPassed;
        }
        #endregion

        #region Alpha or Beta Callback

        public void InvokeABCallback()
        {
            this.Invoke(new AB_GUI_Callback(this.AB_BackgroundThreadFinished));
        }

        private void AB_BackgroundThreadFinished()
        {
            if (!this.DABRAS.IsConnected())
            {
                this.frmParent.refreshConnectStatus();
                this.endFormActivities();
                return;
            }

            if (!QCAB.WasTestCompleted())
            {
                MessageBox.Show("Alpha/Beta QC testing interrupted!");
                this.lbl_QCtestDate.Text = "Test interrupted, use previous data.";
                return;
            }
            if (QCAB.WasTestPassed() && QCAB.WasTestCompleted())
            {
                this.abPassed = true;
                this.lbl_QCtestDate.Text = "Test passed on " + DateTime.Now.ToString("g");
            }
            else if ((!(QCAB.WasTestPassed())) && QCAB.WasTestCompleted())
            {
                MessageBox.Show("Alpha/Beta QC testing failed.");
                this.abPassed = false;
                this.lbl_QCtestDate.Text = "Test failed on " + DateTime.Now.ToString("g");
            }

            //Write daily limits to calibrated source
            if (this.Type == TypeOfQC.Alpha)
            {
                RadionuclideFamily RFa = this.ListOfFamily.Find(x => x.GetName() == "Am-241");

                RFa.SetDailyAlphaCPM(QCAB.GetAlphaNCPM());
                RFa.SetDailyBetaCPM(QCAB.GetBetaNCPM());
                RFa.SetDailyCalibratedDate(DateTime.Now);
                RFa.SetDailyCalibratedTimespan(QCAB.GetSampleTime());// * QCAB.GetNumSamples());
                RFa.SetDailyCalibrationPassed(this.abPassed);

                //Add QC object to List
                QCCalResultNode NewResult = new QCCalResultNode(DateTime.Now, TypeOfQC.Alpha, QCAB.GetAlphaNCPM(), QCAB.GetBetaNCPM(), QCAB.GetBadgeNo(), true, "Passed", QCAB.GetName(), QCAB.GetSampleTime());
                QC_List.Add(NewResult);
            }
            else
            {
                RadionuclideFamily RFb = this.ListOfFamily.Find(x => x.GetName() == "Sr-90");

                RFb.SetDailyAlphaCPM(QCAB.GetAlphaNCPM());
                RFb.SetDailyBetaCPM(QCAB.GetBetaNCPM());
                RFb.SetDailyCalibratedDate(DateTime.Now);
                RFb.SetDailyCalibratedTimespan(QCAB.GetSampleTime());
                RFb.SetDailyCalibrationPassed(this.abPassed);

                //Add QC object to List
                QCCalResultNode NewResult = new QCCalResultNode(DateTime.Now, TypeOfQC.Beta, QCAB.GetAlphaNCPM(),
                    QCAB.GetBetaNCPM(), QCAB.GetBadgeNo(), true, "Passed", QCAB.GetName(), QCAB.GetSampleTime());
                this.QC_List.Add(NewResult);
            }
            //Write results to files
            this.autoWriteAB2Files();
            this.updateSourcesAndFamily();

            DCModified = true;
            SourcesModified = true;

            this.OperationThread_QCAB = null;
            this.resetTabButtons();
            this.SetGUI(false);

            this.QCAB.RequestStop(); //to be safe
            this.QCAB = null;
            this.frmParent.isAcquiring = false;
            if (this.Type == TypeOfQC.Alpha)
                this.updAlphaCheckTab();
            else
                this.updBetaCheckTab();
            //this.enableSorting(true, this.ShortDataGridView);
        }

        public bool QCAlphaBetaPassed()
        {
            return this.abPassed;
        }

        public bool IsQCTestPassUptoDate()
        {//check if ALL three categories have their QC test done within a day
            Radioactive_Source R = ListOfSources.Find(x => x.GetSerialNumber() == "Background");
            DateTime dtbkg = R.GetDailyCalibratedTime();
            RadionuclideFamily RFa = ListOfFamily.Find(x => x.GetName() == "Am-241");
            DateTime dta = RFa.GetDailyCalibratedDate();
            RadionuclideFamily RFb = ListOfFamily.Find(x => x.GetName() == "Sr-90");
            DateTime dtb = RFb.GetDailyCalibratedDate();
            
            TimeSpan Tbkg = DateTime.Now.Subtract(dtbkg);
            TimeSpan Ta = DateTime.Now.Subtract(dta);
            TimeSpan Tb = DateTime.Now.Subtract(dtb);
            bool up2date = ((TimeSpan.Compare(Tbkg, new TimeSpan(1, 0, 0, 0)) < 0)
                 && (TimeSpan.Compare(Ta, new TimeSpan(1, 0, 0, 0)) < 0)
                 && (TimeSpan.Compare(Tb, new TimeSpan(1, 0, 0, 0)) < 0));

            return (R.GetDailyCalibrationPassed() &&
                    RFa.GetDailyCalibrationPassed() &&
                    RFb.GetDailyCalibrationPassed() && up2date);
        }
        #endregion

        #region Private Utility Functions
        private string askForComment(bool passed)
        {
            string Comment = "";
            if (passed)
            {
                if (MessageBox.Show("Test passed! Would you like to add a comment?", "Success!",
                    MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    QuickComment NewComment = new QuickComment();
                    if (NewComment.ShowDialog() == DialogResult.OK)
                    {
                        Comment = NewComment.GetComment();
                    }
                }
            }
            else
            {

                if (MessageBox.Show("QC tests failed. Would you like to add a comment? Note that in order to declare a point unplottable, you must add a comment.", "Test failed", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    QuickComment QC = new QuickComment();
                    if (QC.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        Comment = QC.GetComment();
                        if ((Comment != "") && (MessageBox.Show("Declare point unplottable?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes))
                        {
                            //Plottable = false;
                        }
                    }
                }
            }
            return Comment;
        }

        private void updateSourcesAndFamily()
        {
            this.writeQCdata();
            this.updFamilyAndSource(this.ListOfFamily, this.ListOfSources);
            this.frmParent.updFamilyAndSource(this.ListOfFamily, this.ListOfSources);
            this.setSourceDefault();
            this.setCheckButtons();
        }

        private void setPlottable(bool Plottable, string Comment)
        {
            //Write daily limits to calibrated source
            if (this.Type == TypeOfQC.Alpha)
            {
                RadionuclideFamily R = ListOfFamily.Find(x => x.GetName() == "Am-241");

                R.SetDailyAlphaCPM(Convert.ToInt32(QCAB.GetAlphaNCPM()));
                R.SetDailyBetaCPM(Convert.ToInt32(QCAB.GetBetaNCPM()));
                R.SetDailyCalibratedDate(DateTime.Now);

                //Add QC object to List
                QCCalResultNode NewResult = new QCCalResultNode(DateTime.Now, TypeOfQC.Alpha, QCAB.GetAlphaNCPM(), QCAB.GetBetaNCPM(), QCAB.GetBadgeNo(), Plottable, Comment, QCAB.GetName());
                QC_List.Add(NewResult);

                R.SetDailyCalibratedTimespan(QCAB.GetSampleTime() * QCAB.GetNumSamples());

                DCModified = true;
                SourcesModified = true;
                this.updFamilyAndSource(this.ListOfFamily, this.ListOfSources);
                this.frmParent.updFamilyAndSource(this.ListOfFamily, this.ListOfSources);
            }
            else
            {
                RadionuclideFamily R = ListOfFamily.Find(x => x.GetName() == "Sr-90");

                R.SetDailyAlphaCPM(Convert.ToInt32(QCAB.GetAlphaNCPM()));
                R.SetDailyBetaCPM(Convert.ToInt32(QCAB.GetBetaNCPM()));
                R.SetDailyCalibratedDate(DateTime.Now);

                //Add QC object to List
                QCCalResultNode NewResult = new QCCalResultNode(DateTime.Now, TypeOfQC.Beta, QCAB.GetAlphaNCPM(), QCAB.GetBetaNCPM(), QCAB.GetBadgeNo(), Plottable, Comment, QCAB.GetName());
                QC_List.Add(NewResult);

                DCModified = true;
                SourcesModified = true;
                this.updFamilyAndSource(this.ListOfFamily, this.ListOfSources);
                this.frmParent.updFamilyAndSource(this.ListOfFamily, this.ListOfSources);
            }
        }
        private void Add_Or_Subtract_Rows(int FinalNumberOfRows, DataGridView DG)
        {
            FinalNumberOfRows += 2;
            if ((DG.RowCount) == FinalNumberOfRows)
            {
                return;
            }

            if ((DG.RowCount) > FinalNumberOfRows)
            {
                /*Too many get rid of a few*/
                while ((DG.RowCount) > (FinalNumberOfRows))
                {
                    DG.Rows.RemoveAt(0);
                }
            }
            else
            {
                while ((DG.RowCount) < (FinalNumberOfRows))
                {
                    DG.Rows.Insert(0, "", "", "", "", "", "", "", "", "");//added one more column for a button column
                }
            }

            /*Re-number the rows*/
            for (int i = 0; i < DG.RowCount - 2; i++)
            {
                DG[0, i].Value = i + 1;
            }
            DG[0, DG.RowCount - 2].Value = "Avg";
            DG[0, DG.RowCount - 1].Value = "StdDev";
        }
        
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

        private IDictionary<string, string> WriteMetaData(string techNm)
        {
            IDictionary<string, string> metaD = new Dictionary<string, string>();
            metaD["Technician Name"] = techNm;
            metaD["Building#"] = this.frmParent.GetDefaultConfig().GetBuildingNo();
            metaD["Instrument Set#"] = this.frmParent.GetDefaultConfig().GetSetNo();
            metaD["DABRAS Serial#"] = Convert.ToString(this.DABRAS.Serial_Number);
            metaD["Date"] = String.Format("{0:MM/dd/yyyy}", DateTime.Now);
            metaD["Alpha UL"] = Convert.ToString(this.AlphaHi);
            metaD["Alpha LL"] = Convert.ToString(this.AlphaLo);
            metaD["Beta UL"] = Convert.ToString(this.BetaHi);
            metaD["Beta LL"] = Convert.ToString(this.BetaLo);
            metaD["Background Alpha UL"] = Convert.ToString(this.bkgAlphaHi);
            metaD["Background Alpha LL"] = Convert.ToString(this.bkgAlphaLo);
            metaD["Background Beta UL"] = Convert.ToString(this.bkgBetaHi);
            metaD["Background Beta LL"] = Convert.ToString(this.bkgBetaLo);
            return metaD;
        }
        private IDictionary<string, string> WriteQCSummary()
        {
            IDictionary<string, string> QCsmry = new Dictionary<string, string>();
            
            if (this.bkgrdData != null)
            {
                int bgRows = this.bkgrdData.GetLength(0);
                int bgCols = this.bkgrdData.GetLength(1);
                int smpTime = Convert.ToInt32(bkgrdData[1, 1]);
                int smpNum = bgRows - 3;//ignore the header, avg and stdev rows

                QCsmry["Bkg Rate (alpha)"] = this.bkgrdData[smpNum + 1, 2];//the average row
                QCsmry["Bkg Rate (beta)"] = this.bkgrdData[smpNum + 1, 3];//the average row
                QCsmry["Background Pass/Fail"] = this.bkgrdData[smpNum + 1, 4];//the average row
            }
            if (this.eff_Data_alpha != null)
            {
                QCsmry["Alpha UL"] = Convert.ToString(this.AlphaHi);
                QCsmry["Alpha LL"] = Convert.ToString(this.AlphaLo);
                int aRows = this.eff_Data_alpha.GetLength(0);
                QCsmry["Gross alpha Source Response"] = this.eff_Data_alpha[aRows - 2, 2];
                QCsmry["Net alpha Source Response"] = this.eff_Data_alpha[aRows - 2, 3];
                QCsmry["Pass/Fail Alpha"] = this.eff_Data_alpha[aRows - 2, 6];
            }
            if (this.eff_Data_beta != null)
            {
                QCsmry["Beta UL"] = Convert.ToString(this.BetaHi);
                QCsmry["Beta LL"] = Convert.ToString(this.BetaLo);
                int bRows = this.eff_Data_beta.GetLength(0);
                QCsmry["Gross beta Source Response"] = this.eff_Data_beta[bRows - 2, 4];
                QCsmry["Net beta Source Response"] = this.eff_Data_beta[bRows - 2, 5];
                QCsmry["Pass/Fail Beta"] = this.eff_Data_beta[bRows - 2, 6];
            }

            return QCsmry;
        }

        private void autoWriteBG2Files()
        {
            int yr = DateTime.Now.Year;
            int mn = DateTime.Now.Month;
            int dy = DateTime.Now.Day;

            //Write to two CSV files
            string BkgdDir = String.Format("{0}\\data\\QC\\Bkgd", Environment.CurrentDirectory);
            string BkgdMasterDir = String.Format("{0}\\data\\QC\\Bkgd\\Monthly", Environment.CurrentDirectory);
            string BkgdFilePath = String.Format("{0}\\data\\QC\\Bkgd\\{1}_{2}_{3}_{4}_{5}_{6}_BkgdQC.csv", Environment.CurrentDirectory, yr, mn, dy, QCBG.GetBadgeNo(), QCBG.GetName(), DABRAS.Serial_Number);
            string BkgdMasterPath = String.Format("{0}\\data\\QC\\Bkgd\\Monthly\\Background_{1}.csv", Environment.CurrentDirectory, String.Format("{0:MMMM_yyyy}", DateTime.Now));
            string[,] DataToWrite = this.frmParent.MakeDataWritable(ShortDataGridView);
            try
            {
                if (!Directory.Exists(BkgdDir))
                {
                    Directory.CreateDirectory(BkgdDir);
                }
                if (!Directory.Exists(BkgdMasterDir))
                {
                    Directory.CreateDirectory(BkgdMasterDir);
                }

                if (!File.Exists(BkgdFilePath))
                {
                    File.Create(BkgdFilePath).Dispose();
                }
                using (FileStream F = new FileStream(BkgdFilePath, FileMode.Create))
                {
                    this.frmParent.GetLogger().WriteCSV(F, DataToWrite);
                }

                if (!File.Exists(BkgdMasterPath))
                {
                    File.Create(BkgdMasterPath).Dispose();
                }
                using (FileStream F = new FileStream(BkgdMasterPath, FileMode.Append))
                {
                    this.frmParent.GetLogger().WriteDicData(F, this.WriteMetaData(QCBG.GetName()));
                }
                using (FileStream F = new FileStream(BkgdMasterPath, FileMode.Append))
                {
                    this.frmParent.GetLogger().WriteCSV(F, DataToWrite);
                }
                this.BackgroundData = DataToWrite;
                Radioactive_Source R = ListOfSources.Find(x => x.GetSerialNumber() == "Background");
                R.SetBackgroundQC(DataToWrite);
            }
            catch(Exception e)
            {
                MessageBox.Show("Automatic write background data failed--" + e.Message);
            }
        }

        private void autoWriteAB2Files()
        {
            int yr = DateTime.Now.Year;
            int mn = DateTime.Now.Month;
            int dy = DateTime.Now.Day;

            string abFilePath, abMasterPath;
            string AlphaDir = String.Format("{0}\\data\\QC\\Alpha", Environment.CurrentDirectory);
            string AlphaMasterDir = String.Format("{0}\\data\\QC\\Alpha\\Monthly", Environment.CurrentDirectory);
            string BetaDir = String.Format("{0}\\data\\QC\\Beta", Environment.CurrentDirectory);
            string BetaMasterDir = String.Format("{0}\\data\\QC\\Beta\\Monthly", Environment.CurrentDirectory);
            string[,] DataToWrite = this.frmParent.MakeDataWritable(ShortDataGridView);

            if (QCAB.AlphaTest())
            {
                abFilePath = String.Format("{0}\\data\\QC\\Alpha\\{1}_{2}_{3}_{4}_{5}_{6}_AlphaQC.csv", Environment.CurrentDirectory, yr, mn, dy, QCAB.GetBadgeNo(), QCAB.GetName(), DABRAS.Serial_Number);
                abMasterPath = String.Format("{0}\\data\\QC\\Alpha\\Monthly\\Alpha_{1}.csv", Environment.CurrentDirectory, String.Format("{0:MMMM_yyyy}", DateTime.Now));
                this.AlphaEffData = DataToWrite;
                Radioactive_Source R = this.ListOfFamily.Find(x => x.GetName() == "Am-241").GetCurrentSource();
                R.SetAlpha_QC(DataToWrite);
            }
            else
            {
                abFilePath = String.Format("{0}\\data\\QC\\Beta\\{1}_{2}_{3}_{4}_{5}_{6}_BetaQC.csv", Environment.CurrentDirectory, yr, mn, dy, QCAB.GetBadgeNo(), QCAB.GetName(), DABRAS.Serial_Number);
                abMasterPath = String.Format("{0}\\data\\QC\\Beta\\Monthly\\Beta_{1}.csv", Environment.CurrentDirectory, String.Format("{0:MMMM_yyyy}", DateTime.Now));
                this.BetaEffData = DataToWrite;
                Radioactive_Source R = this.ListOfFamily.Find(x => x.GetName() == "Sr-90").GetCurrentSource();
                R.SetBeta_QC(DataToWrite);
            }

            try
            {
                if (!Directory.Exists(AlphaDir))
                {
                    Directory.CreateDirectory(AlphaDir);
                }
                if (!Directory.Exists(AlphaMasterDir))
                {
                    Directory.CreateDirectory(AlphaMasterDir);
                }
                if (!Directory.Exists(BetaDir))
                {
                    Directory.CreateDirectory(BetaDir);
                }
                if (!Directory.Exists(BetaMasterDir))
                {
                    Directory.CreateDirectory(BetaMasterDir);
                }

                if (!File.Exists(abFilePath))
                {
                    File.Create(abFilePath).Dispose();
                }
                using (FileStream F = new FileStream(abFilePath, FileMode.Create))
                {
                    this.frmParent.GetLogger().WriteCSV(F, DataToWrite);
                }

                if (!File.Exists(abMasterPath))
                {
                    File.Create(abMasterPath).Dispose();
                }
                using (FileStream F = new FileStream(abMasterPath, FileMode.Append))
                {
                    this.frmParent.GetLogger().WriteDicData(F, this.WriteMetaData(QCAB.GetName()));
                }
                using (FileStream F = new FileStream(abMasterPath, FileMode.Append))
                {
                    this.frmParent.GetLogger().WriteCSV(F, DataToWrite);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Writing of alpha/beta data failed--" + e.Message);
            }
        }

        public void updFamilyAndSource(List<RadionuclideFamily> famList, List<Radioactive_Source> srcList)
        {
            this.ListOfSources = srcList;
            
            //and update the family's current_efficiency with the source which has the most current calibration date.
            DateTime daily_dt = new DateTime(2006, 8, 1, 0, 0, 0);
            DateTime hilo_dt = new DateTime(2006, 8, 1, 0, 0, 0);

            foreach (RadionuclideFamily rf in famList)
            {
                daily_dt = rf.GetCurrentSource().GetDailyCalibratedTime();

                Radioactive_Source curr_src = null;
                foreach (Radioactive_Source src in srcList)
                {
                    if (src.GetFamilyID() == rf.GetFamilyID())
                    {
                        DateTime src_dt = src.GetDailyCalibratedTime();
                        if (DateTime.Compare(daily_dt, src_dt) < 0)
                        {
                            daily_dt = src_dt;
                            curr_src = src;
                        }
                    }
                }

                if (curr_src != null)
                    rf.SetCurrentSource(curr_src);

                foreach (Radioactive_Source src in srcList)
                {
                    if (src.GetFamilyID() == rf.GetFamilyID() && src.GetFamilyID() == 0)
                    {
                        rf.SetDailyCalibratedDate(src.GetDailyCalibratedTime());
                        break;
                    }
                }
            }

            this.ListOfFamily = famList;
        }
        #endregion
    }

    #region listeners
    public class QCBackgroundListener
    {
        #region Data Members
        private bool Done;
        private DataGridView QC_Table;
        private int SampleTime;
        private int NumSamples;
        private DABRAS DABRAS;
        private bool ShouldStop;
        private bool WasBackgroundFinishedSuccessfully;
        private DateTime BackgroundFinished;
        private DateTime TestStarted;
        private bool TestPassed = false;

        private double AlphaHi;
        private double AlphaLo;
        private double BetaHi;
        private double BetaLo;

        private double AlphaGCPM;
        private double BetaGCPM;
        //private double AlphaNCPM;
        //private double BetaNCPM;

        private bool Running = false;

        private int BadgeNo;
        private string Name;

        public event EventHandler BackgroundThreadFinished;
        public mainFramework frmCaller;
        #endregion

        #region Constructor
        public QCBackgroundListener(DABRAS _DABRAS, int _SampleTime, int _NumSamples, DataGridView _QC_Table, double _AH, double _AL, double _BH, double _BL, DateTime _Today, int _BadgeNo, string _Name)
        {
            this.DABRAS = _DABRAS;
            this.QC_Table = _QC_Table;
            this.SampleTime = _SampleTime;
            this.NumSamples = _NumSamples;
            this.TestStarted = _Today;
            this.WasBackgroundFinishedSuccessfully = false;
            ShouldStop = false;

            this.AlphaHi = _AH;
            this.AlphaLo = _AL;
            this.BetaHi = _BH;
            this.BetaLo = _BL;
            this.BadgeNo = _BadgeNo;
            this.Name = _Name;
        }
        #endregion

        #region Control Functions
        public void setCallerForm(mainFramework clr)
        {
            this.frmCaller = clr;
        }
        public void RequestStop()
        {
            ShouldStop = true;
        }
        private string BuildToolTipText(double avga, double avgb, bool lessAlphaLo, bool greaterAlphaHi, bool lessBetaLo, bool greaterBetaHi)
        {
            StringBuilder resnBuilder = new StringBuilder();

            if (lessAlphaLo)
            {
                resnBuilder.AppendFormat("Average Alpha background = {0} is less than the alpha lower limit {1}", StaticMethods.RoundToDecimal(avga, 2), AlphaLo);
                resnBuilder.Append(Environment.NewLine);
            }
            if (greaterAlphaHi)
            {
                resnBuilder.AppendFormat("Average Alpha background = {0} is higher than the alpha higher limit {1}", StaticMethods.RoundToDecimal(avga, 2), AlphaHi);
                resnBuilder.Append(Environment.NewLine);
            }
            if (lessBetaLo)
            {
                resnBuilder.AppendFormat("Average Beta background = {0} is less than the beta lower limit {1}", StaticMethods.RoundToDecimal(avgb, 2), BetaLo);
                resnBuilder.Append(Environment.NewLine);
            }
            if (greaterBetaHi)
            {
                resnBuilder.AppendFormat("Average Beta background = {0} is higher than the beta higher limit {1}", StaticMethods.RoundToDecimal(avgb, 2), BetaHi);
                resnBuilder.Append(Environment.NewLine);
            }
            return resnBuilder.ToString();
        }

        private bool Wait4IncomingData()
        {
            //Wait for incoming data packet
            try
            {
                while (!DABRAS.IsDataReady() && !ShouldStop)
                {
                    Thread.Sleep(100);
                    if (!DABRAS.IsConnected())
                    {
                        throw new TimeoutException();
                    }
                }
                this.frmCaller.isAcquiring = true;
                return true;
            }
            catch
            {
                MessageBox.Show("Error: Connection lost by background acquring in QC.");
                this.frmCaller.refreshConnectStatus();
                DABRAS.DisableWatchdog();
                this.frmCaller.isAcquiring = false;
                this.WasBackgroundFinishedSuccessfully = false;

                if (BackgroundThreadFinished != null)
                {
                    using (SoundPlayer S = new SoundPlayer(DABRAS_Software.Properties.Resources.DoneSound))
                    {
                        S.Play();
                    }
                }
                BackgroundThreadFinished(this, null);
                return false;
            }
        }
        private bool ComputeAvgsAndStdDevs()
        {
            //Compute averages
            double AverageAlphaGCPM = 0;
            double AverageBetaGCPM = 0;

            for (int i = 0; i < NumSamples; i++)
            {
                AverageAlphaGCPM += Convert.ToDouble(QC_Table[2, i].Value);  
                AverageBetaGCPM += Convert.ToDouble(QC_Table[3, i].Value);
            }

            AverageAlphaGCPM /= (double)NumSamples;
            AverageBetaGCPM /= (double)NumSamples;

            DataGridViewCell AverageAlphaGCPMCell = QC_Table[2, NumSamples];
            DataGridViewCell AverageBetaGCPMCell = QC_Table[3, NumSamples];
            DataGridViewCell FinalPassFail = QC_Table[4, NumSamples];

            DataGridViewCell StdDevAlphaGCell = QC_Table[2, NumSamples + 1];
            DataGridViewCell StdDevBetaGCell = QC_Table[3, NumSamples + 1];

            AverageAlphaGCPMCell.Value = StaticMethods.RoundToDecimal(AverageAlphaGCPM, 2);
            AverageBetaGCPMCell.Value = StaticMethods.RoundToDecimal(AverageBetaGCPM, 2);

            this.AlphaGCPM = AverageAlphaGCPM;
            this.BetaGCPM = AverageBetaGCPM;

            //Compute Standard Deviations
            double StdDevGAlpha = 0.0;
            double StdDevGBeta = 0.0;

            for (int i = 0; i < NumSamples; i++)
            {
                StdDevGAlpha += (AverageAlphaGCPM - Convert.ToDouble(QC_Table[2, i].Value)) * (AverageAlphaGCPM - Convert.ToDouble(QC_Table[2, i].Value));
                StdDevGBeta += (AverageBetaGCPM - Convert.ToDouble(QC_Table[3, i].Value)) * (AverageBetaGCPM - Convert.ToDouble(QC_Table[3, i].Value));
            }

            if (NumSamples > 1)
            {
                StdDevGAlpha /= (double)(NumSamples - 1);
                StdDevGBeta /= (double)(NumSamples - 1);

                StdDevGAlpha = Math.Sqrt(StdDevGAlpha);
                StdDevGBeta = Math.Sqrt(StdDevGBeta);
            }
            else if (NumSamples == 1)
            {
                StdDevGAlpha = 0.0;
                StdDevGBeta = 0.0;
            }
            StdDevAlphaGCell.Value = StaticMethods.RoundToDecimal(StdDevGAlpha, 2);
            StdDevBetaGCell.Value = StaticMethods.RoundToDecimal(StdDevGBeta, 2);

            //Display overall pass/fail--Test fails if any one of the tests fails.
            bool lessAlphaLo = !(this.AlphaGCPM >= AlphaLo);
            bool greaterAlphaHi = !(this.AlphaGCPM <= AlphaHi);
            bool lessBetaLo = !(this.BetaGCPM >= BetaLo);
            bool greaterBetaHi = !(this.BetaGCPM <= BetaHi);
            if (!lessAlphaLo && !greaterAlphaHi && !lessBetaLo && !greaterBetaHi)
            {
                FinalPassFail.Value = "PASS";
                FinalPassFail.Style = new DataGridViewCellStyle { ForeColor = Color.Green, Font = new Font(this.QC_Table.Font, FontStyle.Bold) };
                return true;
            }
            else
            {
                FinalPassFail.Value = "FAIL (?)";
                //FinalPassFail.ToolTipText = "Test fails if any one of the tests fails.";
                FinalPassFail.ToolTipText = BuildToolTipText(this.AlphaGCPM, this.BetaGCPM, lessAlphaLo, greaterAlphaHi, lessBetaLo, greaterBetaHi);
                FinalPassFail.Style = new DataGridViewCellStyle { ForeColor = Color.Red, Font = new Font(this.QC_Table.Font, FontStyle.Bold) };
                return false;
            }
        }
        #endregion

        #region Getters
        public string GetName()
        {
            return this.Name;
        }

        public int GetBadgeNo()
        {
            return this.BadgeNo;
        }

        public DateTime GetDateTimeStarted()
        {
            return this.TestStarted;
        }

        public double GetAlphaHi()
        {
            return this.AlphaHi;
        }

        public double GetAlphaLo()
        {
            return this.AlphaLo;
        }

        public double GetBetaHi()
        {
            return this.BetaHi;
        }

        public int GetNumSamples()
        {
            return this.NumSamples;
        }

        public int GetSampleTime()
        {
            return this.SampleTime;
        }

        public DateTime GetStartTime()
        {
            return this.TestStarted;
        }

        public double GetBetaLo()
        {
            return this.BetaLo;
        }

        public bool WasTestPassed()
        {
            return this.TestPassed;
        }

        public double GetAlphaGCPM()
        {
            return this.AlphaGCPM;
        }

        public double GetBetaGCPM()
        {
            return this.BetaGCPM;
        }
        /*
        public double GetAlphaNCPM()
        {
            return this.AlphaNCPM;
        }

        public double GetBetaNCPM()
        {
            return this.BetaNCPM;
        }
        */
        public DateTime GetDateTimeCompleted()
        {
            return this.BackgroundFinished;
        }

        public bool WasTestCompleted()
        {
            return this.WasBackgroundFinishedSuccessfully;
        }

        public bool IsRunning()
        {
            return this.Running;
        }
        #endregion

        #region Main Background Thread
        public void Get_Background()
        {
            this.Running = true;
            //Stop all background threads
            this.DABRAS.Cut();

            //Set aquisition time
            this.DABRAS.Write_To_Serial_Port("t");
            Thread.Sleep(250);
            this.DABRAS.Write_To_Serial_Port(Convert.ToString(SampleTime));

            //Clear any data left in the buffer
            this.DABRAS.ClearSerialPacket();
            this.DABRAS.ClearPacketFlag();
            this.DABRAS.EnableWatchdog();

            //Set test passed flag
            this.TestPassed = true;

            for (int i = 0; i < NumSamples; i++)
            {
                bool RowComplete = false;
                this.DABRAS.Write_To_Serial_Port("g");
                Thread.Sleep(150);
                this.DABRAS.ClearSerialPacket();
                this.DABRAS.ClearPacketFlag();

                //Check for the first good packet
                while (!ShouldStop)
                {
                    if (Wait4IncomingData())
                    {
                        SerialPacket IncomingData = DABRAS.ReadSerialPacket();

                        this.DABRAS.KickWatchdog();

                        if (IncomingData != null && IncomingData.ElTime == 0 && IncomingData.TargetTime == SampleTime)
                        {
                            break;
                        }

                        if (IncomingData != null && IncomingData.ElTime > 3)
                        {
                            this.DABRAS.Write_To_Serial_Port("t");
                            Thread.Sleep(250);
                            this.DABRAS.Write_To_Serial_Port(Convert.ToString(SampleTime));
                            this.DABRAS.Write_To_Serial_Port("g");
                            this.DABRAS.ClearSerialPacket();
                            this.DABRAS.ClearPacketFlag();
                        }
                    }
                    else
                        break;
                }//first good packet of data

                //Do not increment the row index until the current sample time has elapsed
                while (!RowComplete && !ShouldStop)
                {
                    if (Wait4IncomingData())
                    {
                        SerialPacket IncomingData = DABRAS.ReadSerialPacket();

                        //Grab handles to form
                        DataGridViewCell TimeElapsed = this.QC_Table[1, i];
                        DataGridViewCell AlphaGCPM = this.QC_Table[2, i];
                        DataGridViewCell BetaGCPM = this.QC_Table[3, i];
                        DataGridViewCell PassFail = this.QC_Table[4, i];

                        //Parse data to form
                        TimeElapsed.Value = IncomingData.ElTime;

                        if (IncomingData != null && IncomingData.ElTime != 0)
                        {
                            double totVala = (double)(IncomingData.AlphaTot);
                            double totValb = (double)(IncomingData.BetaTot);
                            double totTime = (double)(IncomingData.ElTime) / 60.0;
                            AlphaGCPM.Value = StaticMethods.RoundToDecimal(totVala / totTime, 2); //Convert raw counts into CPM. +1 to avoid DBZ, PIC uses zero based counting.
                            BetaGCPM.Value = StaticMethods.RoundToDecimal(totValb / totTime, 2);

                            //Re-draw table
                            QC_Table.Invalidate();

                            //If the sample time has elapsed, increment the row //and compute Pass/Fail.
                            if (IncomingData.ElTime >= SampleTime)
                            {
                                RowComplete = true;
                            }
                        }
                        this.DABRAS.KickWatchdog();
                    }
                    else
                        break;
                }
            }//finished all samples

            this.DABRAS.DisableWatchdog();

            if (!ShouldStop)
            {
                this.TestPassed = this.ComputeAvgsAndStdDevs();

                this.BackgroundFinished = DateTime.Now;
                WasBackgroundFinishedSuccessfully = true;

                if (BackgroundThreadFinished != null)
                {
                    using (SoundPlayer S = new SoundPlayer(DABRAS_Software.Properties.Resources.DoneSound))
                    {
                        S.Play();
                    }

                    BackgroundThreadFinished(this, null);
                }
                //MessageBox.Show("Done!");
            }

            //Resume background threads, if they existed
            this.DABRAS.ResumeBackgroundMonitors();
            this.Running = false;
            this.frmCaller.isAcquiring = this.Running;

            return;
        }
        #endregion
    }

    public class QCAlphaBetaListener
    {
        #region Data Members

        private DataGridView QC_Table;
        private int SampleTime;
        private int NumSamples;
        private DABRAS DABRAS;
        private bool ShouldStop;
        private bool WasBackgroundFinishedSuccessfully;
        private DateTime BackgroundFinished;

        private bool TestPassed = false;
        private bool IsAlphaTest = false;

        private double Hi;
        private double Lo;

        private double AlphaGCPM;
        private double BetaGCPM;
        private double AlphaNCPM;
        private double BetaNCPM;

        private double AlphaBackground;
        private double BetaBackground;

        private DateTime TestStarted;

        public event EventHandler BackgroundThreadFinished;

        private bool Running = false;

        private int BadgeNo;
        private string Name;

        public mainFramework frmCaller;
        #endregion

        #region Constructor
        public QCAlphaBetaListener(DABRAS _DABRAS, int _SampleTime, int _NumSamples, DataGridView _QC_Table, double _Hi, double _Lo, bool _IsAlpha, double _AlphaBG, double _BetaBG, DateTime _Today, int _BadgeNo, string _Name)
        {
            this.DABRAS = _DABRAS;
            this.QC_Table = _QC_Table;
            this.SampleTime = _SampleTime;
            this.NumSamples = _NumSamples;
            this.WasBackgroundFinishedSuccessfully = false;
            ShouldStop = false;

            this.TestStarted = _Today;

            this.Hi = _Hi;
            this.Lo = _Lo;

            this.IsAlphaTest = _IsAlpha;

            this.AlphaBackground = _AlphaBG;
            this.BetaBackground = _BetaBG;
            this.BadgeNo = _BadgeNo;
            this.Name = _Name;
            return;
        }
        #endregion

        #region Control Functions
        public void setCallerForm(mainFramework clr)
        {
            this.frmCaller = clr;
        }
        public void RequestStop()
        {
            ShouldStop = true;
        }
        private bool Wait4IncomingData()
        {
            //Wait for incoming data packet
            try
            {
                while (!DABRAS.IsDataReady() && !ShouldStop)
                {
                    Thread.Sleep(100);
                    if (!DABRAS.IsConnected())
                    {
                        throw new TimeoutException();
                    }
                }
                this.frmCaller.isAcquiring = true;
                return true;
            }
            catch
            {
                MessageBox.Show("Error: Connection lost by alpha/beta aquiring in QC.");
                this.frmCaller.refreshConnectStatus();
                DABRAS.DisableWatchdog();

                this.frmCaller.isAcquiring = false;

                this.WasBackgroundFinishedSuccessfully = false;

                if (BackgroundThreadFinished != null)
                {
                    using (SoundPlayer S = new SoundPlayer(DABRAS_Software.Properties.Resources.DoneSound))
                    {
                        S.Play();
                    }
                }
                BackgroundThreadFinished(this, null);
                return false;
            }
        }
        private string BuildToolTipText(double avga, double avgb,bool lessAlphaLo, bool greaterAlphaHi, bool lessBetaLo, bool greaterBetaHi)
        {
            StringBuilder resnBuilder = new StringBuilder();

            if (lessAlphaLo)
            {
                resnBuilder.AppendFormat("Average Alpha - AlphaBackground = {0} is less than the alpha lower limit {1}", StaticMethods.RoundToDecimal(avga, 2), this.Lo);
                resnBuilder.Append(Environment.NewLine);
            }
            if (greaterAlphaHi)
            {
                resnBuilder.AppendFormat("Average Alpha - AlphaBackground = {0} is higher than the alpha higher limit {1}", StaticMethods.RoundToDecimal(avga, 2), this.Hi);
                resnBuilder.Append(Environment.NewLine);
            }
            if (lessBetaLo)
            {
                resnBuilder.AppendFormat("Average Beta - BetaBackground = {0} is less than the beta lower limit {1}", StaticMethods.RoundToDecimal(avgb, 2), this.Lo);
                resnBuilder.Append(Environment.NewLine);
            }
            if (greaterBetaHi)
            {
                resnBuilder.AppendFormat("Average Beta - BetaBackground = {0} is higher than the beta higher limit {1}", StaticMethods.RoundToDecimal(avgb, 2), this.Hi);
                resnBuilder.Append(Environment.NewLine);
            }
            return resnBuilder.ToString();
        }
        private bool ComputeAvgsAndStdDevs()//Compute Averages and Standard Deviations
        {
            double AverageAlphaGCPM = 0.0;
            double AverageBetaGCPM = 0.0;
            double AverageAlphaNCPM = 0.0;
            double AverageBetaNCPM = 0.0;

            double StdDevGAlpha = 0.0;
            double StdDevNAlpha = 0.0;
            double StdDevGBeta = 0.0;
            double StdDevNBeta = 0.0;

            //Compute averages
            for (int i = 0; i < NumSamples; i++)
            {
                if (this.IsAlphaTest)
                {
                    AverageAlphaGCPM += Convert.ToDouble(QC_Table[2, i].Value);
                    AverageAlphaNCPM += Convert.ToDouble(QC_Table[3, i].Value);
                }
                else
                {//Beta test
                    AverageBetaGCPM += Convert.ToDouble(QC_Table[4, i].Value);
                    AverageBetaNCPM += Convert.ToDouble(QC_Table[5, i].Value);
                }
            }

            if (this.IsAlphaTest)
            {
                AverageAlphaGCPM /= (double)NumSamples;
                AverageAlphaNCPM /= (double)NumSamples;
                DataGridViewCell AverageAlphaGCPMCell = QC_Table[2, NumSamples];
                DataGridViewCell AverageAlphaNCPMCell = QC_Table[3, NumSamples];
                AverageAlphaGCPMCell.Value = StaticMethods.RoundToDecimal(AverageAlphaGCPM, 2);
                AverageAlphaNCPMCell.Value = StaticMethods.RoundToDecimal(AverageAlphaNCPM, 2);

                this.AlphaGCPM = AverageAlphaGCPM;
                this.AlphaNCPM = AverageAlphaNCPM;
            }
            else
            {//Beta test
                AverageBetaGCPM /= (double)NumSamples;
                AverageBetaNCPM /= (double)NumSamples;
                DataGridViewCell AverageBetaGCPMCell = QC_Table[4, NumSamples];
                DataGridViewCell AverageBetaNCPMCell = QC_Table[5, NumSamples];
                AverageBetaGCPMCell.Value = StaticMethods.RoundToDecimal(AverageBetaGCPM, 2);
                AverageBetaNCPMCell.Value = StaticMethods.RoundToDecimal(AverageBetaNCPM, 2);

                this.BetaGCPM = AverageBetaGCPM;
                this.BetaNCPM = AverageBetaNCPM;
            }

            //Compute standard deviations
            for (int i = 0; i < NumSamples; i++)
            {
                if (this.IsAlphaTest)
                {
                    StdDevGAlpha += (AverageAlphaGCPM - Convert.ToDouble(QC_Table[2, i].Value)) * (AverageAlphaGCPM - Convert.ToDouble(QC_Table[2, i].Value));
                    StdDevNAlpha += (AverageAlphaNCPM - Convert.ToDouble(QC_Table[3, i].Value)) * (AverageAlphaNCPM - Convert.ToDouble(QC_Table[3, i].Value));
                }
                else
                {//Beta test
                    StdDevGBeta += (AverageBetaGCPM - Convert.ToDouble(QC_Table[4, i].Value)) * (AverageBetaGCPM - Convert.ToDouble(QC_Table[4, i].Value));
                    StdDevNBeta += (AverageBetaNCPM - Convert.ToDouble(QC_Table[5, i].Value)) * (AverageBetaNCPM - Convert.ToDouble(QC_Table[5, i].Value));
                }
            }

            if (this.IsAlphaTest)
            {
                DataGridViewCell StdDevAlphaGCell = QC_Table[2, NumSamples + 1];
                DataGridViewCell StdDevAlphaNCell = QC_Table[3, NumSamples + 1];
                if (NumSamples > 1)
                {
                    StdDevGAlpha /= Convert.ToDouble((NumSamples - 1));
                    StdDevNAlpha /= Convert.ToDouble((NumSamples - 1));
                    StdDevGAlpha = Math.Sqrt(StdDevGAlpha);
                    StdDevNAlpha = Math.Sqrt(StdDevNAlpha);
                }
                else
                {
                    StdDevGAlpha = Math.Sqrt(AverageAlphaGCPM);
                    StdDevNAlpha = Math.Sqrt(AverageAlphaNCPM);               
                }
                StdDevAlphaGCell.Value = StaticMethods.RoundToDecimal(StdDevGAlpha, 2);
                StdDevAlphaNCell.Value = StaticMethods.RoundToDecimal(StdDevNAlpha, 2);
           }
           else
            {//Beta test
                DataGridViewCell StdDevBetaGCell = QC_Table[4, NumSamples + 1];
                DataGridViewCell StdDevBetaNCell = QC_Table[5, NumSamples + 1];
                if (NumSamples > 1)
                {
                    StdDevGBeta /= Convert.ToDouble((NumSamples - 1));
                    StdDevNBeta /= Convert.ToDouble((NumSamples - 1));
                    StdDevGBeta = Math.Sqrt(StdDevGBeta);
                    StdDevNBeta = Math.Sqrt(StdDevNBeta);
                }
                else
                {
                    StdDevGBeta = Math.Sqrt(AverageBetaGCPM);
                    StdDevNBeta = Math.Sqrt(AverageBetaNCPM);
                }
                StdDevBetaGCell.Value = StaticMethods.RoundToDecimal(StdDevGBeta, 2);
                StdDevBetaNCell.Value = StaticMethods.RoundToDecimal(StdDevNBeta, 2);
           }
                /*
                if (StdDevGAlpha != StdDevGAlpha)
                {
                    StdDevGAlpha = 0;
                }

                if (StdDevNAlpha != StdDevNAlpha)
                {
                    StdDevGAlpha = 0;
                }

                if (StdDevGBeta != StdDevGBeta)
                {
                    StdDevGBeta = 0;
                }

                if (StdDevNBeta != StdDevNBeta)
                {
                    StdDevNBeta = 0;
                }
                */
                       
            //Display overall pass/fail--Test fails if any one of the tests fails.
            bool lessAlphaLo;
            bool greaterAlphaHi;
            bool lessBetaLo;
            bool greaterBetaHi;

            DataGridViewCell FinalPassFail = QC_Table[6, NumSamples];

            if (this.IsAlphaTest)
            {
                lessAlphaLo = !(this.GetAlphaNCPM() >= this.Lo);
                greaterAlphaHi = !(this.GetAlphaNCPM() <= this.Hi);
                if (!lessAlphaLo && !greaterAlphaHi)// && !lessBetaLo && !greaterBetaHi)
                {
                    FinalPassFail.Value = "PASS";
                    FinalPassFail.Style = new DataGridViewCellStyle { ForeColor = Color.Green, Font = new Font(this.QC_Table.Font, FontStyle.Bold) };
                    return true;
                }
                else
                {
                    FinalPassFail.Value = "FAIL (?)";
                    FinalPassFail.Style = new DataGridViewCellStyle { ForeColor = Color.Red, Font = new Font(this.QC_Table.Font, FontStyle.Bold) };
                    FinalPassFail.ToolTipText = BuildToolTipText(this.AlphaNCPM, this.BetaNCPM, lessAlphaLo, greaterAlphaHi, false, false);
                    return false; 
                }
            }
            else
            {//Beta test
                lessBetaLo = !(this.GetBetaNCPM() >= this.Lo);
                greaterBetaHi = !(this.GetBetaNCPM() <= this.Hi);
                if (!lessBetaLo && !greaterBetaHi)
                {
                    FinalPassFail.Value = "PASS";
                    FinalPassFail.Style = new DataGridViewCellStyle { ForeColor = Color.Green, Font = new Font(this.QC_Table.Font, FontStyle.Bold) };
                    return true;
                }
                else
                {
                    FinalPassFail.Value = "FAIL (?)";
                    FinalPassFail.Style = new DataGridViewCellStyle { ForeColor = Color.Red, Font = new Font(this.QC_Table.Font, FontStyle.Bold) };
                    FinalPassFail.ToolTipText = BuildToolTipText(this.AlphaNCPM, this.BetaNCPM, false, false, lessBetaLo, greaterBetaHi);
                    return false; 
                }
            }

        }
        #endregion

        #region Getters
        public string GetName()
        {
            return this.Name;
        }

        public int GetBadgeNo()
        {
            return this.BadgeNo;
        }

        public int GetSampleTime()
        {
            return this.SampleTime;
        }

        public int GetNumSamples()
        {
            return this.NumSamples;
        }

        public bool WasTestPassed()
        {
            return this.TestPassed;
        }

        public double GetHi()
        {
            return this.Hi;
        }

        public double GetLo()
        {
            return this.Lo;
        }

        public bool AlphaTest()
        {
            return this.IsAlphaTest;
        }

        public double GetAlphaGCPM()
        {
            return this.AlphaGCPM;
        }

        public double GetBetaGCPM()
        {
            return this.BetaGCPM;
        }

        public double GetAlphaNCPM()
        {
            return this.AlphaNCPM;
        }

        public double GetBetaNCPM()
        {
            return this.BetaNCPM;
        }

        public DateTime GetDateTimeStarted()
        {
            return this.TestStarted;
        }

        public DateTime GetDateTimeCompleted()
        {
            return this.BackgroundFinished;
        }

        public bool WasTestCompleted()
        {
            return this.WasBackgroundFinishedSuccessfully;
        }

        public bool IsRunning()
        {
            return this.Running;
        }

        #endregion

        #region Main Background Thread
        public void Get_Background()
        {
            this.Running = true;
            //Stop all background threads
            DABRAS.Cut();

            //Set aquisition time
            DABRAS.Write_To_Serial_Port("t");//Set the acquisition time
            Thread.Sleep(250);
            DABRAS.Write_To_Serial_Port(Convert.ToString(SampleTime));//must be within one second
            Thread.Sleep(500);

            //Clear any data left in the buffer
            DABRAS.ClearSerialPacket();
            DABRAS.ClearPacketFlag();
            DABRAS.EnableWatchdog();

            //Set test passed flag
            this.TestPassed = true;

            for (int i = 0; i < NumSamples; i++)
            {
                bool RowComplete = false;
                DABRAS.Write_To_Serial_Port("g");
                DABRAS.ClearSerialPacket();
                DABRAS.ClearPacketFlag();
                //Thread.Sleep(100);

                //Check for the first good packet
                while (!ShouldStop)
                {
                    if (Wait4IncomingData())
                    {
                        SerialPacket IncomingData = DABRAS.ReadSerialPacket();

                        DABRAS.KickWatchdog();

                        if (IncomingData != null)
                        {//Aquisition time setting is a success or failure can be determined by examining the TargetTime value
                            if (IncomingData.ElTime == 0 && IncomingData.TargetTime == SampleTime)
                            {
                                break;
                            }

                            if (IncomingData.ElTime > 3)//in seconds
                            {
                                DABRAS.Write_To_Serial_Port("t");
                                Thread.Sleep(250);
                                DABRAS.Write_To_Serial_Port(Convert.ToString(SampleTime));
                                DABRAS.Write_To_Serial_Port("g");//Begin Acquisition
                            }
                        }
                    }
                    else
                        return;
                }//first good packet of data

                //Do not increment the row index until the current sample time has elapsed
                while (!RowComplete && !ShouldStop)
                {
                    if (Wait4IncomingData())
                    {
                        SerialPacket IncomingData = DABRAS.ReadSerialPacket();

                        //Grab handles to form
                        DataGridViewCell TimeElapsed = QC_Table[1, i];
                        DataGridViewCell AlphaGCPM = QC_Table[2, i];
                        DataGridViewCell AlphaNCPM = QC_Table[3, i];
                        DataGridViewCell BetaGCPM = QC_Table[4, i];
                        DataGridViewCell BetaNCPM = QC_Table[5, i];
                        DataGridViewCell PassFail = QC_Table[6, i];

                        //Parse incoming data to form
                        TimeElapsed.Value = IncomingData.ElTime;

                        if (IncomingData != null && IncomingData.ElTime != 0)
                        {
                            double totVal;
                            double totTime = (double)(IncomingData.ElTime) / 60.0;
                            if (this.IsAlphaTest)
                            {
                                totVal = (double)(IncomingData.AlphaTot);
                                AlphaGCPM.Value = StaticMethods.RoundToDecimal(totVal / totTime, 2);//gross
                                AlphaNCPM.Value = StaticMethods.RoundToDecimal(totVal / totTime - this.AlphaBackground, 2);//net
                            }
                            else
                            {//Beta test
                                totVal = (double)(IncomingData.BetaTot);
                                BetaGCPM.Value = StaticMethods.RoundToDecimal(totVal / totTime, 2);//gross
                                BetaNCPM.Value = StaticMethods.RoundToDecimal(totVal / totTime - this.BetaBackground, 2);//net
                            }
                            //Re-draw table
                            QC_Table.Invalidate();

                            //If the sample time has elapsed, increment the row
                            if (IncomingData.ElTime >= SampleTime)
                            {
                                RowComplete = true;
                            }//the sample time has elapsed
                        }
                        DABRAS.KickWatchdog();
                    }
                    else
                        return;
                }
            }//finish all the samples

            DABRAS.DisableWatchdog();

            if (!ShouldStop)
            {
                this.TestPassed = this.ComputeAvgsAndStdDevs();

                this.BackgroundFinished = DateTime.Now;
                WasBackgroundFinishedSuccessfully = true;

                if (BackgroundThreadFinished != null)
                {
                    using (SoundPlayer S = new SoundPlayer(DABRAS_Software.Properties.Resources.DoneSound))
                    {
                        S.Play();
                    }

                    BackgroundThreadFinished(this, null);
                }
                //MessageBox.Show("Done!");
            }

            //Resume any background threads, if they existed
            DABRAS.ResumeBackgroundMonitors();
            this.Running = false;
            this.frmCaller.isAcquiring = this.Running;
            return;
        }
        #endregion
    }

    #endregion
=======
﻿using System;
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
    public partial class FormQC
    {
        #region Background Callback
        public void InvokeBGCallback()
        {
            this.Invoke(new BG_GUI_Callback(this.BG_BackgroundThreadFinished));
        }

        private void BG_BackgroundThreadFinished()
        {
            if (!this.DABRAS.IsConnected())
            {
                this.frmParent.refreshConnectStatus();
                this.endFormActivities();
                return;
            }

            if (!QCBG.WasTestCompleted())
            { // Test was cancelled, suspend any further actions!
                MessageBox.Show("Background QC testing interrupted.");
                this.lbl_QCtestDate.Text = "Test cancelled, use previous data.";
                return;
            }

            string Comment = "";
            if (QCBG.WasTestPassed() && QCBG.WasTestCompleted())
            {
                Comment = "Passed";
                this.bgPassed = true;
                this.lbl_QCtestDate.Text = "Test passed on " + DateTime.Now.ToString("g");
            }
            else if (!QCBG.WasTestPassed() && QCBG.WasTestCompleted())
            {
                MessageBox.Show("Background QC testing failed.");
                this.bgPassed = false;
                this.lbl_QCtestDate.Text = "Test failed on " + DateTime.Now.ToString("g");
            }

            //Write daily limits to background source
            Radioactive_Source R = this.ListOfSources.Find(x => x.GetSerialNumber() == "Background");

            R.SetDailyAlphaCPM(QCBG.GetAlphaGCPM());
            R.SetDailyBetaCPM(QCBG.GetBetaGCPM());
            R.SetDailyCalibratedDate(DateTime.Now);
            R.SetDailyCalibratedTimespan(QCBG.GetSampleTime() * QCBG.GetNumSamples());
            R.SetDailyCalibrationPassed(this.bgPassed);

            //Add QC object to List
            QCCalResultNode NewResult = new QCCalResultNode(DateTime.Now, TypeOfQC.Background, 0, 0, QCBG.GetBadgeNo(), true, Comment, QCBG.GetName(), QCBG.GetSampleTime());
            QC_List.Add(NewResult);

            //Write results to a file
            this.autoWriteBG2Files();
            this.DCModified = true;
            this.SourcesModified = true;
            this.updateSourcesAndFamily();

            this.OperationThread_QCBG = null;
            this.resetTabButtons();
            this.SetGUI(false);
            this.QCBG.RequestStop();
            this.QCBG = null;
            this.frmParent.isAcquiring = false;
            this.DefaultButtonCheck();
            //this.enableSorting(true, this.ShortDataGridView);
        }

        public bool QCbackgroundPassed()
        {
            return this.bgPassed;
        }
        #endregion

        #region Alpha or Beta Callback

        public void InvokeABCallback()
        {
            this.Invoke(new AB_GUI_Callback(this.AB_BackgroundThreadFinished));
        }

        private void AB_BackgroundThreadFinished()
        {
            if (!this.DABRAS.IsConnected())
            {
                this.frmParent.refreshConnectStatus();
                this.endFormActivities();
                return;
            }

            if (QCAB.WasTestPassed() && QCAB.WasTestCompleted())
            {
                this.abPassed = true;
                this.lbl_QCtestDate.Text = "Test passed on " + DateTime.Now.ToString("g");
            }
            else if ((!(QCAB.WasTestPassed())) && QCAB.WasTestCompleted())
            {
                MessageBox.Show("Alpha/Beta QC testing failed.");
                this.abPassed = false;
                this.lbl_QCtestDate.Text = "Test failed on " + DateTime.Now.ToString("g");
            }

            //Write daily limits to calibrated source
            if (this.Type == TypeOfQC.Alpha)
            {
                RadionuclideFamily RFa = this.ListOfFamily.Find(x => x.GetName() == "Am-241");

                RFa.SetDailyAlphaCPM(QCAB.GetAlphaNCPM());
                RFa.SetDailyBetaCPM(QCAB.GetBetaNCPM());
                RFa.SetDailyCalibratedDate(DateTime.Now);
                RFa.SetDailyCalibratedTimespan(QCAB.GetSampleTime());// * QCAB.GetNumSamples());
                RFa.SetDailyCalibrationPassed(this.abPassed);

                //Add QC object to List
                QCCalResultNode NewResult = new QCCalResultNode(DateTime.Now, TypeOfQC.Alpha, QCAB.GetAlphaNCPM(), QCAB.GetBetaNCPM(), QCAB.GetBadgeNo(), true, "Passed", QCAB.GetName(), QCAB.GetSampleTime());
                QC_List.Add(NewResult);
            }
            else
            {
                RadionuclideFamily RFb = this.ListOfFamily.Find(x => x.GetName() == "Sr-90");

                RFb.SetDailyAlphaCPM(QCAB.GetAlphaNCPM());
                RFb.SetDailyBetaCPM(QCAB.GetBetaNCPM());
                RFb.SetDailyCalibratedDate(DateTime.Now);
                RFb.SetDailyCalibratedTimespan(QCAB.GetSampleTime());
                RFb.SetDailyCalibrationPassed(this.abPassed);

                //Add QC object to List
                QCCalResultNode NewResult = new QCCalResultNode(DateTime.Now, TypeOfQC.Beta, QCAB.GetAlphaNCPM(),
                    QCAB.GetBetaNCPM(), QCAB.GetBadgeNo(), true, "Passed", QCAB.GetName(), QCAB.GetSampleTime());
                this.QC_List.Add(NewResult);
            }
            //Write results to files
            this.autoWriteAB2Files();
            this.updateSourcesAndFamily();

            DCModified = true;
            SourcesModified = true;

            this.OperationThread_QCAB = null;
            this.resetTabButtons();
            this.SetGUI(false);

            this.QCAB.RequestStop(); //to be safe
            this.QCAB = null;
            this.frmParent.isAcquiring = false;
            if (this.Type == TypeOfQC.Alpha)
                this.updAlphaCheckTab();
            else
                this.updBetaCheckTab();
            //this.enableSorting(true, this.ShortDataGridView);
        }

        public bool QCAlphaBetaPassed()
        {
            return this.abPassed;
        }

        public bool IsQCTestPassUptoDate()
        {//check if ALL three categories have their QC test done within a day
            Radioactive_Source R = ListOfSources.Find(x => x.GetSerialNumber() == "Background");
            DateTime dtbkg = R.GetDailyCalibratedTime();
            RadionuclideFamily RFa = ListOfFamily.Find(x => x.GetName() == "Am-241");
            DateTime dta = RFa.GetDailyCalibratedDate();
            RadionuclideFamily RFb = ListOfFamily.Find(x => x.GetName() == "Sr-90");
            DateTime dtb = RFb.GetDailyCalibratedDate();
            
            TimeSpan Tbkg = DateTime.Now.Subtract(dtbkg);
            TimeSpan Ta = DateTime.Now.Subtract(dta);
            TimeSpan Tb = DateTime.Now.Subtract(dtb);
            bool up2date = ((TimeSpan.Compare(Tbkg, new TimeSpan(1, 0, 0, 0)) < 0)
                 && (TimeSpan.Compare(Ta, new TimeSpan(1, 0, 0, 0)) < 0)
                 && (TimeSpan.Compare(Tb, new TimeSpan(1, 0, 0, 0)) < 0));

            return (R.GetDailyCalibrationPassed() &&
                    RFa.GetDailyCalibrationPassed() &&
                    RFb.GetDailyCalibrationPassed() && up2date);
        }
        #endregion

        #region Private Utility Functions
        private string askForComment(bool passed)
        {
            string Comment = "";
            if (passed)
            {
                if (MessageBox.Show("Test passed! Would you like to add a comment?", "Success!",
                    MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    QuickComment NewComment = new QuickComment();
                    if (NewComment.ShowDialog() == DialogResult.OK)
                    {
                        Comment = NewComment.GetComment();
                    }
                }
            }
            else
            {

                if (MessageBox.Show("QC tests failed. Would you like to add a comment? Note that in order to declare a point unplottable, you must add a comment.", "Test failed", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    QuickComment QC = new QuickComment();
                    if (QC.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        Comment = QC.GetComment();
                        if ((Comment != "") && (MessageBox.Show("Declare point unplottable?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes))
                        {
                            //Plottable = false;
                        }
                    }
                }
            }
            return Comment;
        }

        private void updateSourcesAndFamily()
        {
            this.writeQCdata();
            this.updFamilyAndSource(this.ListOfFamily, this.ListOfSources);
            this.frmParent.updFamilyAndSource(this.ListOfFamily, this.ListOfSources);
            this.setSourceDefault();
            this.setCheckButtons();
        }

        private void setPlottable(bool Plottable, string Comment)
        {
            //Write daily limits to calibrated source
            if (this.Type == TypeOfQC.Alpha)
            {
                RadionuclideFamily R = ListOfFamily.Find(x => x.GetName() == "Am-241");

                R.SetDailyAlphaCPM(Convert.ToInt32(QCAB.GetAlphaNCPM()));
                R.SetDailyBetaCPM(Convert.ToInt32(QCAB.GetBetaNCPM()));
                R.SetDailyCalibratedDate(DateTime.Now);

                //Add QC object to List
                QCCalResultNode NewResult = new QCCalResultNode(DateTime.Now, TypeOfQC.Alpha, QCAB.GetAlphaNCPM(), QCAB.GetBetaNCPM(), QCAB.GetBadgeNo(), Plottable, Comment, QCAB.GetName());
                QC_List.Add(NewResult);

                R.SetDailyCalibratedTimespan(QCAB.GetSampleTime() * QCAB.GetNumSamples());

                DCModified = true;
                SourcesModified = true;
                this.updFamilyAndSource(this.ListOfFamily, this.ListOfSources);
                this.frmParent.updFamilyAndSource(this.ListOfFamily, this.ListOfSources);
            }
            else
            {
                RadionuclideFamily R = ListOfFamily.Find(x => x.GetName() == "Sr-90");

                R.SetDailyAlphaCPM(Convert.ToInt32(QCAB.GetAlphaNCPM()));
                R.SetDailyBetaCPM(Convert.ToInt32(QCAB.GetBetaNCPM()));
                R.SetDailyCalibratedDate(DateTime.Now);

                //Add QC object to List
                QCCalResultNode NewResult = new QCCalResultNode(DateTime.Now, TypeOfQC.Beta, QCAB.GetAlphaNCPM(), QCAB.GetBetaNCPM(), QCAB.GetBadgeNo(), Plottable, Comment, QCAB.GetName());
                QC_List.Add(NewResult);

                DCModified = true;
                SourcesModified = true;
                this.updFamilyAndSource(this.ListOfFamily, this.ListOfSources);
                this.frmParent.updFamilyAndSource(this.ListOfFamily, this.ListOfSources);
            }
        }
        private void Add_Or_Subtract_Rows(int FinalNumberOfRows, DataGridView DG)
        {
            FinalNumberOfRows += 2;
            if ((DG.RowCount) == FinalNumberOfRows)
            {
                return;
            }

            if ((DG.RowCount) > FinalNumberOfRows)
            {
                /*Too many get rid of a few*/
                while ((DG.RowCount) > (FinalNumberOfRows))
                {
                    DG.Rows.RemoveAt(0);
                }
            }
            else
            {
                while ((DG.RowCount) < (FinalNumberOfRows))
                {
                    DG.Rows.Insert(0, "", "", "", "", "", "", "", "", "");//added one more column for a button column
                }
            }

            /*Re-number the rows*/
            for (int i = 0; i < DG.RowCount - 2; i++)
            {
                DG[0, i].Value = i + 1;
            }
            DG[0, DG.RowCount - 2].Value = "Avg";
            DG[0, DG.RowCount - 1].Value = "StdDev";
        }
        
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

        private IDictionary<string, string> WriteMetaData(string techNm)
        {
            IDictionary<string, string> metaD = new Dictionary<string, string>();
            metaD["Technician Name"] = techNm;
            metaD["Building#"] = this.frmParent.GetDefaultConfig().GetBuildingNo();
            metaD["Instrument Set#"] = this.frmParent.GetDefaultConfig().GetSetNo();
            metaD["DABRAS Serial#"] = Convert.ToString(this.DABRAS.Serial_Number);
            metaD["Date"] = String.Format("{0:MM/dd/yyyy}", DateTime.Now);
            metaD["Alpha UL"] = Convert.ToString(this.AlphaHi);
            metaD["Alpha LL"] = Convert.ToString(this.AlphaLo);
            metaD["Beta UL"] = Convert.ToString(this.BetaHi);
            metaD["Beta LL"] = Convert.ToString(this.BetaLo);
            metaD["Background Alpha UL"] = Convert.ToString(this.bkgAlphaHi);
            metaD["Background Alpha LL"] = Convert.ToString(this.bkgAlphaLo);
            metaD["Background Beta UL"] = Convert.ToString(this.bkgBetaHi);
            metaD["Background Beta LL"] = Convert.ToString(this.bkgBetaLo);
            return metaD;
        }
        private IDictionary<string, string> WriteQCSummary()
        {
            IDictionary<string, string> QCsmry = new Dictionary<string, string>();
            
            if (this.bkgrdData != null)
            {
                int bgRows = this.bkgrdData.GetLength(0);
                int bgCols = this.bkgrdData.GetLength(1);
                int smpTime = Convert.ToInt32(bkgrdData[1, 1]);
                int smpNum = bgRows - 3;//ignore the header, avg and stdev rows

                QCsmry["Bkg Rate (alpha)"] = this.bkgrdData[smpNum + 1, 2];//the average row
                QCsmry["Bkg Rate (beta)"] = this.bkgrdData[smpNum + 1, 3];//the average row
                QCsmry["Background Pass/Fail"] = this.bkgrdData[smpNum + 1, 4];//the average row
            }
            if (this.eff_Data_alpha != null)
            {
                QCsmry["Alpha UL"] = Convert.ToString(this.AlphaHi);
                QCsmry["Alpha LL"] = Convert.ToString(this.AlphaLo);
                int aRows = this.eff_Data_alpha.GetLength(0);
                QCsmry["Gross alpha Source Response"] = this.eff_Data_alpha[aRows - 2, 2];
                QCsmry["Net alpha Source Response"] = this.eff_Data_alpha[aRows - 2, 3];
                QCsmry["Pass/Fail Alpha"] = this.eff_Data_alpha[aRows - 2, 6];
            }
            if (this.eff_Data_beta != null)
            {
                QCsmry["Beta UL"] = Convert.ToString(this.BetaHi);
                QCsmry["Beta LL"] = Convert.ToString(this.BetaLo);
                int bRows = this.eff_Data_beta.GetLength(0);
                QCsmry["Gross beta Source Response"] = this.eff_Data_beta[bRows - 2, 4];
                QCsmry["Net beta Source Response"] = this.eff_Data_beta[bRows - 2, 5];
                QCsmry["Pass/Fail Beta"] = this.eff_Data_beta[bRows - 2, 6];
            }

            return QCsmry;
        }

        private void autoWriteBG2Files()
        {
            int yr = DateTime.Now.Year;
            int mn = DateTime.Now.Month;
            int dy = DateTime.Now.Day;

            //Write to two CSV files
            string BkgdDir = String.Format("{0}\\data\\QC\\Bkgd", Environment.CurrentDirectory);
            string BkgdMasterDir = String.Format("{0}\\data\\QC\\Bkgd\\Monthly", Environment.CurrentDirectory);
            string BkgdFilePath = String.Format("{0}\\data\\QC\\Bkgd\\{1}_{2}_{3}_{4}_{5}_{6}_BkgdQC.csv", Environment.CurrentDirectory, yr, mn, dy, QCBG.GetBadgeNo(), QCBG.GetName(), DABRAS.Serial_Number);
            string BkgdMasterPath = String.Format("{0}\\data\\QC\\Bkgd\\Monthly\\Background_{1}.csv", Environment.CurrentDirectory, String.Format("{0:MMMM_yyyy}", DateTime.Now));
            string[,] DataToWrite = this.frmParent.MakeDataWritable(ShortDataGridView);
            try
            {
                if (!Directory.Exists(BkgdDir))
                {
                    Directory.CreateDirectory(BkgdDir);
                }
                if (!Directory.Exists(BkgdMasterDir))
                {
                    Directory.CreateDirectory(BkgdMasterDir);
                }

                if (!File.Exists(BkgdFilePath))
                {
                    File.Create(BkgdFilePath).Dispose();
                }
                using (FileStream F = new FileStream(BkgdFilePath, FileMode.Create))
                {
                    this.frmParent.GetLogger().WriteCSV(F, DataToWrite);
                }

                if (!File.Exists(BkgdMasterPath))
                {
                    File.Create(BkgdMasterPath).Dispose();
                }
                using (FileStream F = new FileStream(BkgdMasterPath, FileMode.Append))
                {
                    this.frmParent.GetLogger().WriteDicData(F, this.WriteMetaData(QCBG.GetName()));
                }
                using (FileStream F = new FileStream(BkgdMasterPath, FileMode.Append))
                {
                    this.frmParent.GetLogger().WriteCSV(F, DataToWrite);
                }
                this.BackgroundData = DataToWrite;
                Radioactive_Source R = ListOfSources.Find(x => x.GetSerialNumber() == "Background");
                R.SetBackgroundQC(DataToWrite);
            }
            catch(Exception e)
            {
                MessageBox.Show("Automatic write background data failed--" + e.Message);
            }
        }

        private void autoWriteAB2Files()
        {
            int yr = DateTime.Now.Year;
            int mn = DateTime.Now.Month;
            int dy = DateTime.Now.Day;

            string abFilePath, abMasterPath;
            string AlphaDir = String.Format("{0}\\data\\QC\\Alpha", Environment.CurrentDirectory);
            string AlphaMasterDir = String.Format("{0}\\data\\QC\\Alpha\\Monthly", Environment.CurrentDirectory);
            string BetaDir = String.Format("{0}\\data\\QC\\Beta", Environment.CurrentDirectory);
            string BetaMasterDir = String.Format("{0}\\data\\QC\\Beta\\Monthly", Environment.CurrentDirectory);
            string[,] DataToWrite = this.frmParent.MakeDataWritable(ShortDataGridView);

            if (QCAB.AlphaTest())
            {
                abFilePath = String.Format("{0}\\data\\QC\\Alpha\\{1}_{2}_{3}_{4}_{5}_{6}_AlphaQC.csv", Environment.CurrentDirectory, yr, mn, dy, QCAB.GetBadgeNo(), QCAB.GetName(), DABRAS.Serial_Number);
                abMasterPath = String.Format("{0}\\data\\QC\\Alpha\\Monthly\\Alpha_{1}.csv", Environment.CurrentDirectory, String.Format("{0:MMMM_yyyy}", DateTime.Now));
                this.AlphaEffData = DataToWrite;
                Radioactive_Source R = this.ListOfFamily.Find(x => x.GetName() == "Am-241").GetCurrentSource();
                R.SetAlpha_QC(DataToWrite);
            }
            else
            {
                abFilePath = String.Format("{0}\\data\\QC\\Beta\\{1}_{2}_{3}_{4}_{5}_{6}_BetaQC.csv", Environment.CurrentDirectory, yr, mn, dy, QCAB.GetBadgeNo(), QCAB.GetName(), DABRAS.Serial_Number);
                abMasterPath = String.Format("{0}\\data\\QC\\Beta\\Monthly\\Beta_{1}.csv", Environment.CurrentDirectory, String.Format("{0:MMMM_yyyy}", DateTime.Now));
                this.BetaEffData = DataToWrite;
                Radioactive_Source R = this.ListOfFamily.Find(x => x.GetName() == "Sr-90").GetCurrentSource();
                R.SetBeta_QC(DataToWrite);
            }

            try
            {
                if (!Directory.Exists(AlphaDir))
                {
                    Directory.CreateDirectory(AlphaDir);
                }
                if (!Directory.Exists(AlphaMasterDir))
                {
                    Directory.CreateDirectory(AlphaMasterDir);
                }
                if (!Directory.Exists(BetaDir))
                {
                    Directory.CreateDirectory(BetaDir);
                }
                if (!Directory.Exists(BetaMasterDir))
                {
                    Directory.CreateDirectory(BetaMasterDir);
                }

                if (!File.Exists(abFilePath))
                {
                    File.Create(abFilePath).Dispose();
                }
                using (FileStream F = new FileStream(abFilePath, FileMode.Create))
                {
                    this.frmParent.GetLogger().WriteCSV(F, DataToWrite);
                }

                if (!File.Exists(abMasterPath))
                {
                    File.Create(abMasterPath).Dispose();
                }
                using (FileStream F = new FileStream(abMasterPath, FileMode.Append))
                {
                    this.frmParent.GetLogger().WriteDicData(F, this.WriteMetaData(QCAB.GetName()));
                }
                using (FileStream F = new FileStream(abMasterPath, FileMode.Append))
                {
                    this.frmParent.GetLogger().WriteCSV(F, DataToWrite);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Writing of alpha/beta data failed--" + e.Message);
            }
        }

        public void updFamilyAndSource(List<RadionuclideFamily> famList, List<Radioactive_Source> srcList)
        {
            this.ListOfSources = srcList;
            
            //and update the family's current_efficiency with the source which has the most current calibration date.
            DateTime daily_dt = new DateTime(2006, 8, 1, 0, 0, 0);
            DateTime hilo_dt = new DateTime(2006, 8, 1, 0, 0, 0);

            foreach (RadionuclideFamily rf in famList)
            {
                daily_dt = rf.GetCurrentSource().GetDailyCalibratedTime();

                Radioactive_Source curr_src = null;
                foreach (Radioactive_Source src in srcList)
                {
                    if (src.GetFamilyID() == rf.GetFamilyID())
                    {
                        DateTime src_dt = src.GetDailyCalibratedTime();
                        if (DateTime.Compare(daily_dt, src_dt) < 0)
                        {
                            daily_dt = src_dt;
                            curr_src = src;
                        }
                    }
                }

                if (curr_src != null)
                    rf.SetCurrentSource(curr_src);

                foreach (Radioactive_Source src in srcList)
                {
                    if (src.GetFamilyID() == rf.GetFamilyID() && src.GetFamilyID() == 0)
                    {
                        rf.SetDailyCalibratedDate(src.GetDailyCalibratedTime());
                        break;
                    }
                }
            }

            this.ListOfFamily = famList;
        }
        #endregion
    }

    #region listeners
    public class QCBackgroundListener
    {
        #region Data Members
        private bool Done;
        private DataGridView QC_Table;
        private int SampleTime;
        private int NumSamples;
        private DABRAS DABRAS;
        private bool ShouldStop;
        private bool WasBackgroundFinishedSuccessfully;
        private DateTime BackgroundFinished;
        private DateTime TestStarted;
        private bool TestPassed = false;

        private double AlphaHi;
        private double AlphaLo;
        private double BetaHi;
        private double BetaLo;

        private double AlphaGCPM;
        private double BetaGCPM;
        //private double AlphaNCPM;
        //private double BetaNCPM;

        private bool Running = false;

        private int BadgeNo;
        private string Name;

        public event EventHandler BackgroundThreadFinished;
        public mainFramework frmCaller;
        #endregion

        #region Constructor
        public QCBackgroundListener(DABRAS _DABRAS, int _SampleTime, int _NumSamples, DataGridView _QC_Table, double _AH, double _AL, double _BH, double _BL, DateTime _Today, int _BadgeNo, string _Name)
        {
            this.DABRAS = _DABRAS;
            this.QC_Table = _QC_Table;
            this.SampleTime = _SampleTime;
            this.NumSamples = _NumSamples;
            this.TestStarted = _Today;
            this.WasBackgroundFinishedSuccessfully = false;
            ShouldStop = false;

            this.AlphaHi = _AH;
            this.AlphaLo = _AL;
            this.BetaHi = _BH;
            this.BetaLo = _BL;
            this.BadgeNo = _BadgeNo;
            this.Name = _Name;
        }
        #endregion

        #region Control Functions
        public void setCallerForm(mainFramework clr)
        {
            this.frmCaller = clr;
        }
        public void RequestStop()
        {
            ShouldStop = true;
        }
        private string BuildToolTipText(double avga, double avgb, bool lessAlphaLo, bool greaterAlphaHi, bool lessBetaLo, bool greaterBetaHi)
        {
            StringBuilder resnBuilder = new StringBuilder();

            if (lessAlphaLo)
            {
                resnBuilder.AppendFormat("Average Alpha background = {0} is less than the alpha lower limit {1}", StaticMethods.RoundToDecimal(avga, 2), AlphaLo);
                resnBuilder.Append(Environment.NewLine);
            }
            if (greaterAlphaHi)
            {
                resnBuilder.AppendFormat("Average Alpha background = {0} is higher than the alpha higher limit {1}", StaticMethods.RoundToDecimal(avga, 2), AlphaHi);
                resnBuilder.Append(Environment.NewLine);
            }
            if (lessBetaLo)
            {
                resnBuilder.AppendFormat("Average Beta background = {0} is less than the beta lower limit {1}", StaticMethods.RoundToDecimal(avgb, 2), BetaLo);
                resnBuilder.Append(Environment.NewLine);
            }
            if (greaterBetaHi)
            {
                resnBuilder.AppendFormat("Average Beta background = {0} is higher than the beta higher limit {1}", StaticMethods.RoundToDecimal(avgb, 2), BetaHi);
                resnBuilder.Append(Environment.NewLine);
            }
            return resnBuilder.ToString();
        }

        private bool Wait4IncomingData()
        {
            //Wait for incoming data packet
            try
            {
                while (!DABRAS.IsDataReady() && !ShouldStop)
                {
                    Thread.Sleep(100);
                    if (!DABRAS.IsConnected())
                    {
                        throw new TimeoutException();
                    }
                }
                this.frmCaller.isAcquiring = true;
                return true;
            }
            catch
            {
                MessageBox.Show("Error: Connection lost by background acquring in QC.");
                this.frmCaller.refreshConnectStatus();
                DABRAS.DisableWatchdog();
                this.frmCaller.isAcquiring = false;
                WasBackgroundFinishedSuccessfully = false;

                if (BackgroundThreadFinished != null)
                {
                    using (SoundPlayer S = new SoundPlayer(DABRAS_Software.Properties.Resources.DoneSound))
                    {
                        S.Play();
                    }
                }
                BackgroundThreadFinished(this, null);
                return false;
            }
        }

        private bool ComputeAvgsAndStdDevs()
        {
            //Compute averages
            double AverageAlphaGCPM = 0;
            double AverageBetaGCPM = 0;

            for (int i = 0; i < NumSamples; i++)
            {
                AverageAlphaGCPM += Convert.ToDouble(QC_Table[2, i].Value);  
                AverageBetaGCPM += Convert.ToDouble(QC_Table[3, i].Value);
            }

            AverageAlphaGCPM /= (double)NumSamples;
            AverageBetaGCPM /= (double)NumSamples;

            DataGridViewCell AverageAlphaGCPMCell = QC_Table[2, NumSamples];
            DataGridViewCell AverageBetaGCPMCell = QC_Table[3, NumSamples];
            DataGridViewCell FinalPassFail = QC_Table[4, NumSamples];

            DataGridViewCell StdDevAlphaGCell = QC_Table[2, NumSamples + 1];
            DataGridViewCell StdDevBetaGCell = QC_Table[3, NumSamples + 1];

            AverageAlphaGCPMCell.Value = StaticMethods.RoundToDecimal(AverageAlphaGCPM, 2);
            AverageBetaGCPMCell.Value = StaticMethods.RoundToDecimal(AverageBetaGCPM, 2);

            this.AlphaGCPM = AverageAlphaGCPM;
            this.BetaGCPM = AverageBetaGCPM;

            //Compute Standard Deviations
            double StdDevGAlpha = 0.0;
            double StdDevGBeta = 0.0;

            for (int i = 0; i < NumSamples; i++)
            {
                StdDevGAlpha += (AverageAlphaGCPM - Convert.ToDouble(QC_Table[2, i].Value)) * (AverageAlphaGCPM - Convert.ToDouble(QC_Table[2, i].Value));
                StdDevGBeta += (AverageBetaGCPM - Convert.ToDouble(QC_Table[3, i].Value)) * (AverageBetaGCPM - Convert.ToDouble(QC_Table[3, i].Value));
            }

            if (NumSamples > 1)
            {
                StdDevGAlpha /= (double)(NumSamples - 1);
                StdDevGBeta /= (double)(NumSamples - 1);

                StdDevGAlpha = Math.Sqrt(StdDevGAlpha);
                StdDevGBeta = Math.Sqrt(StdDevGBeta);
            }
            else if (NumSamples == 1)
            {
                StdDevGAlpha = 0.0;
                StdDevGBeta = 0.0;
            }
            StdDevAlphaGCell.Value = StaticMethods.RoundToDecimal(StdDevGAlpha, 2);
            StdDevBetaGCell.Value = StaticMethods.RoundToDecimal(StdDevGBeta, 2);

            //Display overall pass/fail--Test fails if any one of the tests fails.
            bool lessAlphaLo = !(this.AlphaGCPM >= AlphaLo);
            bool greaterAlphaHi = !(this.AlphaGCPM <= AlphaHi);
            bool lessBetaLo = !(this.BetaGCPM >= BetaLo);
            bool greaterBetaHi = !(this.BetaGCPM <= BetaHi);
            if (!lessAlphaLo && !greaterAlphaHi && !lessBetaLo && !greaterBetaHi)
            {
                FinalPassFail.Value = "PASS";
                FinalPassFail.Style = new DataGridViewCellStyle { ForeColor = Color.Green, Font = new Font(this.QC_Table.Font, FontStyle.Bold) };
                return true;
            }
            else
            {
                FinalPassFail.Value = "FAIL (?)";
                //FinalPassFail.ToolTipText = "Test fails if any one of the tests fails.";
                FinalPassFail.ToolTipText = BuildToolTipText(this.AlphaGCPM, this.BetaGCPM, lessAlphaLo, greaterAlphaHi, lessBetaLo, greaterBetaHi);
                FinalPassFail.Style = new DataGridViewCellStyle { ForeColor = Color.Red, Font = new Font(this.QC_Table.Font, FontStyle.Bold) };
                return false;
            }
        }
        #endregion

        #region Getters
        public string GetName()
        {
            return this.Name;
        }

        public int GetBadgeNo()
        {
            return this.BadgeNo;
        }

        public DateTime GetDateTimeStarted()
        {
            return this.TestStarted;
        }

        public double GetAlphaHi()
        {
            return this.AlphaHi;
        }

        public double GetAlphaLo()
        {
            return this.AlphaLo;
        }

        public double GetBetaHi()
        {
            return this.BetaHi;
        }

        public int GetNumSamples()
        {
            return this.NumSamples;
        }

        public int GetSampleTime()
        {
            return this.SampleTime;
        }

        public DateTime GetStartTime()
        {
            return this.TestStarted;
        }

        public double GetBetaLo()
        {
            return this.BetaLo;
        }

        public bool WasTestPassed()
        {
            return this.TestPassed;
        }

        public double GetAlphaGCPM()
        {
            return this.AlphaGCPM;
        }

        public double GetBetaGCPM()
        {
            return this.BetaGCPM;
        }
        /*
        public double GetAlphaNCPM()
        {
            return this.AlphaNCPM;
        }

        public double GetBetaNCPM()
        {
            return this.BetaNCPM;
        }
        */
        public DateTime GetDateTimeCompleted()
        {
            return this.BackgroundFinished;
        }

        public bool WasTestCompleted()
        {
            return this.WasBackgroundFinishedSuccessfully;
        }

        public bool IsRunning()
        {
            return this.Running;
        }
        #endregion

        #region Main Background Thread
        public void Get_Background()
        {
            this.Running = true;
            //Stop all background threads
            this.DABRAS.Cut();

            //Set aquisition time
            this.DABRAS.Write_To_Serial_Port("t");
            Thread.Sleep(250);
            this.DABRAS.Write_To_Serial_Port(Convert.ToString(SampleTime));

            //Clear any data left in the buffer
            this.DABRAS.ClearSerialPacket();
            this.DABRAS.ClearPacketFlag();
            this.DABRAS.EnableWatchdog();

            //Set test passed flag
            this.TestPassed = true;

            for (int i = 0; i < NumSamples; i++)
            {
                bool RowComplete = false;
                this.DABRAS.Write_To_Serial_Port("g");
                Thread.Sleep(150);
                this.DABRAS.ClearSerialPacket();
                this.DABRAS.ClearPacketFlag();

                //Check for the first good packet
                while (!ShouldStop)
                {
                    if (Wait4IncomingData())
                    {
                        SerialPacket IncomingData = DABRAS.ReadSerialPacket();

                        this.DABRAS.KickWatchdog();

                        if (IncomingData != null && IncomingData.ElTime == 0 && IncomingData.TargetTime == SampleTime)
                        {
                            break;
                        }

                        if (IncomingData != null && IncomingData.ElTime > 3)
                        {
                            this.DABRAS.Write_To_Serial_Port("t");
                            Thread.Sleep(250);
                            this.DABRAS.Write_To_Serial_Port(Convert.ToString(SampleTime));
                            this.DABRAS.Write_To_Serial_Port("g");
                            this.DABRAS.ClearSerialPacket();
                            this.DABRAS.ClearPacketFlag();
                        }
                    }
                    else
                        break;
                }//first good packet of data

                //Do not increment the row index until the current sample time has elapsed
                while (!RowComplete && !ShouldStop)
                {
                    if (Wait4IncomingData())
                    {
                        SerialPacket IncomingData = DABRAS.ReadSerialPacket();

                        //Grab handles to form
                        DataGridViewCell TimeElapsed = this.QC_Table[1, i];
                        DataGridViewCell AlphaGCPM = this.QC_Table[2, i];
                        DataGridViewCell BetaGCPM = this.QC_Table[3, i];
                        DataGridViewCell PassFail = this.QC_Table[4, i];

                        //Parse data to form
                        TimeElapsed.Value = IncomingData.ElTime;

                        if (IncomingData != null && IncomingData.ElTime != 0)
                        {
                            double totVala = (double)(IncomingData.AlphaTot);
                            double totValb = (double)(IncomingData.BetaTot);
                            double totTime = (double)(IncomingData.ElTime) / 60.0;
                            AlphaGCPM.Value = StaticMethods.RoundToDecimal(totVala / totTime, 2); //Convert raw counts into CPM. +1 to avoid DBZ, PIC uses zero based counting.
                            BetaGCPM.Value = StaticMethods.RoundToDecimal(totValb / totTime, 2);

                            //Re-draw table
                            QC_Table.Invalidate();

                            //If the sample time has elapsed, increment the row //and compute Pass/Fail.
                            if (IncomingData.ElTime >= SampleTime)
                            {
                                RowComplete = true;
                            }
                        }
                        this.DABRAS.KickWatchdog();
                    }
                    else
                        break;
                }
            }//finished all samples

            this.DABRAS.DisableWatchdog();

            if (!ShouldStop)
            {
                this.TestPassed = this.ComputeAvgsAndStdDevs();

                this.BackgroundFinished = DateTime.Now;
                this.WasBackgroundFinishedSuccessfully = true;

                if (BackgroundThreadFinished != null)
                {
                    using (SoundPlayer S = new SoundPlayer(DABRAS_Software.Properties.Resources.DoneSound))
                    {
                        S.Play();
                    }

                    BackgroundThreadFinished(this, null);
                }
                //MessageBox.Show("Done!");
            }

            //Resume background threads, if they existed
            this.DABRAS.ResumeBackgroundMonitors();
            this.Running = false;
            this.frmCaller.isAcquiring = this.Running;

            return;
        }
        #endregion
    }

    public class QCAlphaBetaListener
    {
        #region Data Members

        private DataGridView QC_Table;
        private int SampleTime;
        private int NumSamples;
        private DABRAS DABRAS;
        private bool ShouldStop;
        private bool WasBackgroundFinishedSuccessfully;
        private DateTime BackgroundFinished;

        private bool TestPassed = false;
        private bool IsAlphaTest = false;

        private double Hi;
        private double Lo;

        private double AlphaGCPM;
        private double BetaGCPM;
        private double AlphaNCPM;
        private double BetaNCPM;

        private double AlphaBackground;
        private double BetaBackground;

        private DateTime TestStarted;

        public event EventHandler BackgroundThreadFinished;

        private bool Running = false;

        private int BadgeNo;
        private string Name;

        public mainFramework frmCaller;
        #endregion

        #region Constructor
        public QCAlphaBetaListener(DABRAS _DABRAS, int _SampleTime, int _NumSamples, DataGridView _QC_Table, double _Hi, double _Lo, bool _IsAlpha, double _AlphaBG, double _BetaBG, DateTime _Today, int _BadgeNo, string _Name)
        {
            this.DABRAS = _DABRAS;
            this.QC_Table = _QC_Table;
            this.SampleTime = _SampleTime;
            this.NumSamples = _NumSamples;
            this.WasBackgroundFinishedSuccessfully = false;
            ShouldStop = false;

            this.TestStarted = _Today;

            this.Hi = _Hi;
            this.Lo = _Lo;

            this.IsAlphaTest = _IsAlpha;

            this.AlphaBackground = _AlphaBG;
            this.BetaBackground = _BetaBG;
            this.BadgeNo = _BadgeNo;
            this.Name = _Name;
            return;
        }
        #endregion

        #region Control Functions
        public void setCallerForm(mainFramework clr)
        {
            this.frmCaller = clr;
        }
        public void RequestStop()
        {
            ShouldStop = true;
        }

        private bool Wait4IncomingData()
        {
            //Wait for incoming data packet
            try
            {
                while (!DABRAS.IsDataReady() && !ShouldStop)
                {
                    Thread.Sleep(100);
                    if (!DABRAS.IsConnected())
                    {
                        throw new TimeoutException();
                    }
                }
                this.frmCaller.isAcquiring = true;
                return true;
            }
            catch
            {
                MessageBox.Show("Error: Connection lost by alpha/beta aquiring in QC.");
                this.frmCaller.refreshConnectStatus();
                DABRAS.DisableWatchdog();

                this.frmCaller.isAcquiring = false;

                WasBackgroundFinishedSuccessfully = false;

                if (BackgroundThreadFinished != null)
                {
                    using (SoundPlayer S = new SoundPlayer(DABRAS_Software.Properties.Resources.DoneSound))
                    {
                        S.Play();
                    }
                }
                BackgroundThreadFinished(this, null);
                return false;
            }
        }
        private string BuildToolTipText(double avga, double avgb,bool lessAlphaLo, bool greaterAlphaHi, bool lessBetaLo, bool greaterBetaHi)
        {
            StringBuilder resnBuilder = new StringBuilder();

            if (lessAlphaLo)
            {
                resnBuilder.AppendFormat("Average Alpha - AlphaBackground = {0} is less than the alpha lower limit {1}", StaticMethods.RoundToDecimal(avga, 2), this.Lo);
                resnBuilder.Append(Environment.NewLine);
            }
            if (greaterAlphaHi)
            {
                resnBuilder.AppendFormat("Average Alpha - AlphaBackground = {0} is higher than the alpha higher limit {1}", StaticMethods.RoundToDecimal(avga, 2), this.Hi);
                resnBuilder.Append(Environment.NewLine);
            }
            if (lessBetaLo)
            {
                resnBuilder.AppendFormat("Average Beta - BetaBackground = {0} is less than the beta lower limit {1}", StaticMethods.RoundToDecimal(avgb, 2), this.Lo);
                resnBuilder.Append(Environment.NewLine);
            }
            if (greaterBetaHi)
            {
                resnBuilder.AppendFormat("Average Beta - BetaBackground = {0} is higher than the beta higher limit {1}", StaticMethods.RoundToDecimal(avgb, 2), this.Hi);
                resnBuilder.Append(Environment.NewLine);
            }
            return resnBuilder.ToString();
        }
        private bool ComputeAvgsAndStdDevs()//Compute Averages and Standard Deviations
        {
            double AverageAlphaGCPM = 0.0;
            double AverageBetaGCPM = 0.0;
            double AverageAlphaNCPM = 0.0;
            double AverageBetaNCPM = 0.0;

            double StdDevGAlpha = 0.0;
            double StdDevNAlpha = 0.0;
            double StdDevGBeta = 0.0;
            double StdDevNBeta = 0.0;

            //Compute averages
            for (int i = 0; i < NumSamples; i++)
            {
                if (this.IsAlphaTest)
                {
                    AverageAlphaGCPM += Convert.ToDouble(QC_Table[2, i].Value);
                    AverageAlphaNCPM += Convert.ToDouble(QC_Table[3, i].Value);
                }
                else
                {//Beta test
                    AverageBetaGCPM += Convert.ToDouble(QC_Table[4, i].Value);
                    AverageBetaNCPM += Convert.ToDouble(QC_Table[5, i].Value);
                }
            }

            if (this.IsAlphaTest)
            {
                AverageAlphaGCPM /= (double)NumSamples;
                AverageAlphaNCPM /= (double)NumSamples;
                DataGridViewCell AverageAlphaGCPMCell = QC_Table[2, NumSamples];
                DataGridViewCell AverageAlphaNCPMCell = QC_Table[3, NumSamples];
                AverageAlphaGCPMCell.Value = StaticMethods.RoundToDecimal(AverageAlphaGCPM, 2);
                AverageAlphaNCPMCell.Value = StaticMethods.RoundToDecimal(AverageAlphaNCPM, 2);

                this.AlphaGCPM = AverageAlphaGCPM;
                this.AlphaNCPM = AverageAlphaNCPM;
            }
            else
            {//Beta test
                AverageBetaGCPM /= (double)NumSamples;
                AverageBetaNCPM /= (double)NumSamples;
                DataGridViewCell AverageBetaGCPMCell = QC_Table[4, NumSamples];
                DataGridViewCell AverageBetaNCPMCell = QC_Table[5, NumSamples];
                AverageBetaGCPMCell.Value = StaticMethods.RoundToDecimal(AverageBetaGCPM, 2);
                AverageBetaNCPMCell.Value = StaticMethods.RoundToDecimal(AverageBetaNCPM, 2);

                this.BetaGCPM = AverageBetaGCPM;
                this.BetaNCPM = AverageBetaNCPM;
            }

            //Compute standard deviations
            for (int i = 0; i < NumSamples; i++)
            {
                if (this.IsAlphaTest)
                {
                    StdDevGAlpha += (AverageAlphaGCPM - Convert.ToDouble(QC_Table[2, i].Value)) * (AverageAlphaGCPM - Convert.ToDouble(QC_Table[2, i].Value));
                    StdDevNAlpha += (AverageAlphaNCPM - Convert.ToDouble(QC_Table[3, i].Value)) * (AverageAlphaNCPM - Convert.ToDouble(QC_Table[3, i].Value));
                }
                else
                {//Beta test
                    StdDevGBeta += (AverageBetaGCPM - Convert.ToDouble(QC_Table[4, i].Value)) * (AverageBetaGCPM - Convert.ToDouble(QC_Table[4, i].Value));
                    StdDevNBeta += (AverageBetaNCPM - Convert.ToDouble(QC_Table[5, i].Value)) * (AverageBetaNCPM - Convert.ToDouble(QC_Table[5, i].Value));
                }
            }

            if (this.IsAlphaTest)
            {
                DataGridViewCell StdDevAlphaGCell = QC_Table[2, NumSamples + 1];
                DataGridViewCell StdDevAlphaNCell = QC_Table[3, NumSamples + 1];
                if (NumSamples > 1)
                {
                    StdDevGAlpha /= Convert.ToDouble((NumSamples - 1));
                    StdDevNAlpha /= Convert.ToDouble((NumSamples - 1));
                    StdDevGAlpha = Math.Sqrt(StdDevGAlpha);
                    StdDevNAlpha = Math.Sqrt(StdDevNAlpha);
                }
                else
                {
                    StdDevGAlpha = Math.Sqrt(AverageAlphaGCPM);
                    StdDevNAlpha = Math.Sqrt(AverageAlphaNCPM);               
                }
                StdDevAlphaGCell.Value = StaticMethods.RoundToDecimal(StdDevGAlpha, 2);
                StdDevAlphaNCell.Value = StaticMethods.RoundToDecimal(StdDevNAlpha, 2);
           }
           else
            {//Beta test
                DataGridViewCell StdDevBetaGCell = QC_Table[4, NumSamples + 1];
                DataGridViewCell StdDevBetaNCell = QC_Table[5, NumSamples + 1];
                if (NumSamples > 1)
                {
                    StdDevGBeta /= Convert.ToDouble((NumSamples - 1));
                    StdDevNBeta /= Convert.ToDouble((NumSamples - 1));
                    StdDevGBeta = Math.Sqrt(StdDevGBeta);
                    StdDevNBeta = Math.Sqrt(StdDevNBeta);
                }
                else
                {
                    StdDevGBeta = Math.Sqrt(AverageBetaGCPM);
                    StdDevNBeta = Math.Sqrt(AverageBetaNCPM);
                }
                StdDevBetaGCell.Value = StaticMethods.RoundToDecimal(StdDevGBeta, 2);
                StdDevBetaNCell.Value = StaticMethods.RoundToDecimal(StdDevNBeta, 2);
           }
                /*
                if (StdDevGAlpha != StdDevGAlpha)
                {
                    StdDevGAlpha = 0;
                }

                if (StdDevNAlpha != StdDevNAlpha)
                {
                    StdDevGAlpha = 0;
                }

                if (StdDevGBeta != StdDevGBeta)
                {
                    StdDevGBeta = 0;
                }

                if (StdDevNBeta != StdDevNBeta)
                {
                    StdDevNBeta = 0;
                }
                */
                       
            //Display overall pass/fail--Test fails if any one of the tests fails.
            bool lessAlphaLo;
            bool greaterAlphaHi;
            bool lessBetaLo;
            bool greaterBetaHi;

            DataGridViewCell FinalPassFail = QC_Table[6, NumSamples];

            if (this.IsAlphaTest)
            {
                lessAlphaLo = !(this.GetAlphaNCPM() >= this.Lo);
                greaterAlphaHi = !(this.GetAlphaNCPM() <= this.Hi);
                if (!lessAlphaLo && !greaterAlphaHi)// && !lessBetaLo && !greaterBetaHi)
                {
                    FinalPassFail.Value = "PASS";
                    FinalPassFail.Style = new DataGridViewCellStyle { ForeColor = Color.Green, Font = new Font(this.QC_Table.Font, FontStyle.Bold) };
                    return true;
                }
                else
                {
                    FinalPassFail.Value = "FAIL (?)";
                    FinalPassFail.Style = new DataGridViewCellStyle { ForeColor = Color.Red, Font = new Font(this.QC_Table.Font, FontStyle.Bold) };
                    FinalPassFail.ToolTipText = BuildToolTipText(this.AlphaNCPM, this.BetaNCPM, lessAlphaLo, greaterAlphaHi, false, false);
                    return false; 
                }
            }
            else
            {//Beta test
                lessBetaLo = !(this.GetBetaNCPM() >= this.Lo);
                greaterBetaHi = !(this.GetBetaNCPM() <= this.Hi);
                if (!lessBetaLo && !greaterBetaHi)
                {
                    FinalPassFail.Value = "PASS";
                    FinalPassFail.Style = new DataGridViewCellStyle { ForeColor = Color.Green, Font = new Font(this.QC_Table.Font, FontStyle.Bold) };
                    return true;
                }
                else
                {
                    FinalPassFail.Value = "FAIL (?)";
                    FinalPassFail.Style = new DataGridViewCellStyle { ForeColor = Color.Red, Font = new Font(this.QC_Table.Font, FontStyle.Bold) };
                    FinalPassFail.ToolTipText = BuildToolTipText(this.AlphaNCPM, this.BetaNCPM, false, false, lessBetaLo, greaterBetaHi);
                    return false; 
                }
            }

        }
        #endregion

        #region Getters
        public string GetName()
        {
            return this.Name;
        }

        public int GetBadgeNo()
        {
            return this.BadgeNo;
        }

        public int GetSampleTime()
        {
            return this.SampleTime;
        }

        public int GetNumSamples()
        {
            return this.NumSamples;
        }

        public bool WasTestPassed()
        {
            return this.TestPassed;
        }

        public double GetHi()
        {
            return this.Hi;
        }

        public double GetLo()
        {
            return this.Lo;
        }

        public bool AlphaTest()
        {
            return this.IsAlphaTest;
        }

        public double GetAlphaGCPM()
        {
            return this.AlphaGCPM;
        }

        public double GetBetaGCPM()
        {
            return this.BetaGCPM;
        }

        public double GetAlphaNCPM()
        {
            return this.AlphaNCPM;
        }

        public double GetBetaNCPM()
        {
            return this.BetaNCPM;
        }

        public DateTime GetDateTimeStarted()
        {
            return this.TestStarted;
        }

        public DateTime GetDateTimeCompleted()
        {
            return this.BackgroundFinished;
        }

        public bool WasTestCompleted()
        {
            return this.WasBackgroundFinishedSuccessfully;
        }

        public bool IsRunning()
        {
            return this.Running;
        }

        #endregion

        #region Main Background Thread
        public void Get_Background()
        {
            this.Running = true;
            //Stop all background threads
            DABRAS.Cut();

            //Set aquisition time
            DABRAS.Write_To_Serial_Port("t");//Set the acquisition time
            Thread.Sleep(250);
            DABRAS.Write_To_Serial_Port(Convert.ToString(SampleTime));//must be within one second
            Thread.Sleep(500);

            //Clear any data left in the buffer
            DABRAS.ClearSerialPacket();
            DABRAS.ClearPacketFlag();
            DABRAS.EnableWatchdog();

            //Set test passed flag
            this.TestPassed = true;

            for (int i = 0; i < NumSamples; i++)
            {
                bool RowComplete = false;
                DABRAS.Write_To_Serial_Port("g");
                DABRAS.ClearSerialPacket();
                DABRAS.ClearPacketFlag();
                //Thread.Sleep(100);

                //Check for the first good packet
                while (!ShouldStop)
                {
                    if (Wait4IncomingData())
                    {
                        SerialPacket IncomingData = DABRAS.ReadSerialPacket();

                        DABRAS.KickWatchdog();

                        if (IncomingData != null)
                        {//Aquisition time setting is a success or failure can be determined by examining the TargetTime value
                            if (IncomingData.ElTime == 0 && IncomingData.TargetTime == SampleTime)
                            {
                                break;
                            }

                            if (IncomingData.ElTime > 3)//in seconds
                            {
                                DABRAS.Write_To_Serial_Port("t");
                                Thread.Sleep(250);
                                DABRAS.Write_To_Serial_Port(Convert.ToString(SampleTime));
                                DABRAS.Write_To_Serial_Port("g");//Begin Acquisition
                            }
                        }
                    }
                    else
                        return;
                   }//first good packet of data

                //Do not increment the row index until the current sample time has elapsed
                while (!RowComplete && !ShouldStop)
                {
                    if (Wait4IncomingData())
                    {
                        SerialPacket IncomingData = DABRAS.ReadSerialPacket();

                        //Grab handles to form
                        DataGridViewCell TimeElapsed = QC_Table[1, i];
                        DataGridViewCell AlphaGCPM = QC_Table[2, i];
                        DataGridViewCell AlphaNCPM = QC_Table[3, i];
                        DataGridViewCell BetaGCPM = QC_Table[4, i];
                        DataGridViewCell BetaNCPM = QC_Table[5, i];
                        DataGridViewCell PassFail = QC_Table[6, i];

                        //Parse incoming data to form
                        TimeElapsed.Value = IncomingData.ElTime;

                        if (IncomingData != null && IncomingData.ElTime != 0)
                        {
                            double totVal;
                            double totTime = (double)(IncomingData.ElTime) / 60.0;
                            if (this.IsAlphaTest)
                            {
                                totVal = (double)(IncomingData.AlphaTot);
                                AlphaGCPM.Value = StaticMethods.RoundToDecimal(totVal / totTime, 2);//gross
                                AlphaNCPM.Value = StaticMethods.RoundToDecimal(totVal / totTime - this.AlphaBackground, 2);//net
                            }
                            else
                            {//Beta test
                                totVal = (double)(IncomingData.BetaTot);
                                BetaGCPM.Value = StaticMethods.RoundToDecimal(totVal / totTime, 2);//gross
                                BetaNCPM.Value = StaticMethods.RoundToDecimal(totVal / totTime - this.BetaBackground, 2);//net
                            }
                            //Re-draw table
                            QC_Table.Invalidate();

                            //If the sample time has elapsed, increment the row
                            if (IncomingData.ElTime >= SampleTime)
                            {
                                RowComplete = true;
                            }//the sample time has elapsed
                        }
                        DABRAS.KickWatchdog();
                    }
                    else
                        return;
                }
            }//finish all the samples

            DABRAS.DisableWatchdog();

            if (!ShouldStop)
            {
                this.TestPassed = this.ComputeAvgsAndStdDevs();

                this.BackgroundFinished = DateTime.Now;
                this.WasBackgroundFinishedSuccessfully = true;

                if (BackgroundThreadFinished != null)
                {
                    using (SoundPlayer S = new SoundPlayer(DABRAS_Software.Properties.Resources.DoneSound))
                    {
                        S.Play();
                    }

                    BackgroundThreadFinished(this, null);
                }
                //MessageBox.Show("Done!");
            }

            //Resume any background threads, if they existed
            DABRAS.ResumeBackgroundMonitors();
            this.Running = false;
            this.frmCaller.isAcquiring = this.Running;
            return;
        }
        #endregion
    }

    #endregion
>>>>>>> 66cc4ad (added changes related to RCS decimal points and DPM activity display; allowing QC tests to be stopped and/or skipped during running)
}