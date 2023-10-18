﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using System.Threading;

using Excel = Microsoft.Office.Interop.Excel;

namespace DABRAS_Software
{
    public class Logger
    {
        #region Data Members
        /*Streamwriters*/
        TextWriter Data_ASCII_Writer = null;    //For data text file writing
        TextWriter Status_ASCII_Writer = null;  //For non-data text file writing

        /*InitializeLogger()
         * This function initializes all I/O streams.
         * It also prints the line "<stream> Initialized at <Time> to all loggers.
         * It creates the text/xml files if they do not already exist
         * 
         * @author Mitchell spryn
         * @version 0.1
         */

        bool Writing = false;
        #endregion

        #region Constructor
        public void InitializeLogger()
        {
            string CurrentDir = Environment.CurrentDirectory;
            string DataPath = String.Concat(CurrentDir, "\\data\\");
            string StatusPath = String.Concat(CurrentDir, "\\status\\");

            /*Create the directories if they don't exist*/
            if (!(Directory.Exists(DataPath)))
            {
                Directory.CreateDirectory(DataPath);
            }

            if (!(Directory.Exists(StatusPath)))
            {
                Directory.CreateDirectory(StatusPath);
            }

            /*Check to be sure that all files exist*/
            if (!File.Exists("data\\DataLogs.txt"))
            {
                File.Create("data\\DataLogs.txt");
            }
            
            if (!File.Exists("status\\DataLogs.txt"))
            {
                File.Create("status\\DataLogs.txt");
            }

            try
            {
                Data_ASCII_Writer = new StreamWriter(File.OpenWrite("data\\DataLog.txt"));
                Status_ASCII_Writer = new StreamWriter(File.OpenWrite("status\\StatusLog.txt"));
            }

            catch
            {
                ;
            }
            /*TODO: Validate?*/
            return;
        }
        #endregion

        #region Status Log Methods
        public bool WriteLineToStatusLog(string DataToWrite)
        {
            /*Wait for thread to become safe*/
            while (Writing)
            {
                Thread.Sleep(50);
            }

            Writing = true;
            
            try
            {
                /*Get a timestamp*/
                DateTime Time = DateTime.Now;

                /*Write to file*/
                Status_ASCII_Writer.WriteLine(DataToWrite);
                Status_ASCII_Writer.Flush();

                Writing = false;
                return true;
            }
            catch
            {
                Writing = false;
                return false;
            }

        }
        #endregion

        #region Write Data Methods
        public bool WriteCSV(FileStream F, string[,] data2Write)
        {
            try
            {
                TextWriter CSV_Writer = new StreamWriter(F);

                //For some reason, it always undercounts the number of rows by one.
                for (int i = 0; i < data2Write.GetUpperBound(0) + 1; i++)
                {
                    for (int j = 0; j <= data2Write.GetUpperBound(1); j++)
                    {
                        CSV_Writer.Write(data2Write[i, j]);
                        CSV_Writer.Write(',');
                    }

                    CSV_Writer.WriteLine("");
                }

                CSV_Writer.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool WriteSummaryData(FileStream F, IDictionary<string, string> metaD, IDictionary<string, string> sumD, bool withHeaders)
        {
            try
            {
                TextWriter dic_Writer = new StreamWriter(F);
                if (withHeaders)
                {
                    dic_Writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19}", "Date", "Bkg Rate (alpha)", "Bkg UL(alpha)", "Bkg LL(alpha)", "Bkg Rate (beta)", "Bkg UL(beta)", "Bkg LL(beta)", "BackgroundPass/Fail", 
                        "Net alpha Source Response", "Alpha Low Limit", "Alpha High Limit", "Pass/Fail Alpha", "Net beta Source Response", "Beta Low Limit", "Beta High Limit", "Pass/Fail Beta", 
                        "Technician Name", "Building#", "Instrument Set#", "Comments");
                }
                else
                {
                    if (metaD.Count > 0 && sumD.Count > 0)
                        dic_Writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19}", metaD["Date"], sumD["Bkg Rate (alpha)"], metaD["Background Alpha LL"], metaD["Background Alpha UL"],
                            sumD["Bkg Rate (beta)"], metaD["Background Beta LL"], metaD["Background Beta UL"], sumD["Background Pass/Fail"], sumD["Net alpha Source Response"], metaD["Alpha LL"], metaD["Alpha UL"], sumD["Pass/Fail Alpha"], 
                            sumD["Net beta Source Response"], metaD["Beta LL"], metaD["Beta UL"], sumD["Pass/Fail Beta"], metaD["Technician Name"], metaD["Building#"], metaD["Instrument Set#"], "");
                }
                dic_Writer.Close();
                return true;
            }
            catch (Exception wex)
            {
                MessageBox.Show("Writing summary to file failed." + wex.Message);
                return false;
            }
        }

        public bool WriteDicData(FileStream F, IDictionary<string, string> metaD)
        {
            try
            {
                TextWriter dic_Writer = new StreamWriter(F);
                foreach(KeyValuePair<string, string> kvp in metaD)
                    dic_Writer.WriteLine("{0},{1}", kvp.Key, kvp.Value);

                dic_Writer.Close();
                return true;
            }
            catch (Exception wex)
            {
                MessageBox.Show("Writing summary to file failed." + wex.Message);
                return false;
            }
        }
        //TODO: writing with information on SetNo, technician's name (or initial), test date, pass_or_fail and HiLo criteria
        public bool WriteExcel(string FilePath, string[,] DataToWrite)//(FileStream F, string[,] DataToWrite)
        {
            try
            {
                string CurrentDir = "";
                if (FilePath[0] != '@')
                {
                    //MessageBox.Show("Absolute Paths not implemented yet.");
                }

                else
                {
                    CurrentDir = Environment.CurrentDirectory;
                    CurrentDir = String.Concat(CurrentDir, '/');
                }
                    if (!Directory.Exists(CurrentDir))
                    {
                        Directory.CreateDirectory(CurrentDir);
                    }

                    if (!File.Exists(FilePath))
                    {
                        File.Create(FilePath).Dispose();
                    }
                
                Excel.Application excel = new Excel.Application();
                Excel.Workbook Workbook = excel.Workbooks.Add();
                Excel.Worksheet sheet = Workbook.ActiveSheet();

                for (int i = 0; i < DataToWrite.GetUpperBound(0); i++)
                {
                    for (int j = 0; j < DataToWrite.GetUpperBound(1); j++)
                    {
                        sheet.Cells[i + 1][j + 1] = DataToWrite[i, j];
                    }
                    
                }

                Workbook.SaveAs(FilePath);
                Workbook.Close();
                return true;
            }
            catch (Exception wex)
            {
                MessageBox.Show("Writing Excel file   failed." + wex.Message);
                return false;
            }

        }
        #endregion

        #region Create File Directories
        public bool CreateDirectoryTree()
        {
            try
            {
                /*For top level*/
                #region Level One
                string Env = Environment.CurrentDirectory;
                if (!Directory.Exists(String.Format("{0}\\conf", Env)))
                {
                    Directory.CreateDirectory(String.Format("{0}\\conf", Env));
                }

                if (!Directory.Exists(String.Format("{0}\\data", Env)))
                {
                    Directory.CreateDirectory(String.Format("{0}\\data", Env));
                }

                if (!Directory.Exists(String.Format("{0}\\status", Env)))
                {
                    Directory.CreateDirectory(String.Format("{0}\\status", Env));
                }
                #endregion

                #region Calibration
                string CalEnv = String.Format("{0}\\data\\Cal", Env);

                if (!Directory.Exists(CalEnv))
                {
                    Directory.CreateDirectory(CalEnv);
                }

                if (!Directory.Exists(String.Format("{0}\\Bkgd", CalEnv)))
                {
                    Directory.CreateDirectory(String.Format("{0}\\Bkgd", CalEnv));
                }

                if (!Directory.Exists(String.Format("{0}\\HiLo", CalEnv)))
                {
                    Directory.CreateDirectory(String.Format("{0}\\HiLo", CalEnv));
                }

                if (!Directory.Exists(String.Format("{0}\\Eff", CalEnv)))
                {
                    Directory.CreateDirectory(String.Format("{0}\\Eff", CalEnv));
                }

                if (!Directory.Exists(String.Format("{0}\\HVP", CalEnv)))
                {
                    Directory.CreateDirectory(String.Format("{0}\\HVP", CalEnv));
                }
                #endregion

                #region DailyQC
                string QCPath = String.Format("{0}\\data\\QC", Env);
                if (!Directory.Exists(QCPath))
                {
                    Directory.CreateDirectory(QCPath);
                }

                if (!Directory.Exists(String.Format("{0}\\Bkgd", QCPath)))
                {
                    Directory.CreateDirectory(String.Format("{0}\\Bkgd", QCPath));
                }

                if (!Directory.Exists(String.Format("{0}\\Alpha", QCPath)))
                {
                    Directory.CreateDirectory(String.Format("{0}\\Alpha", QCPath));
                }

                if (!Directory.Exists(String.Format("{0}\\Beta", QCPath)))
                {
                    Directory.CreateDirectory(String.Format("{0}\\Beta", QCPath));
                }

                if (!Directory.Exists(String.Format("{0}\\ChiSq", QCPath)))
                {
                    Directory.CreateDirectory(String.Format("{0}\\ChiSq", QCPath));
                }

                if (!Directory.Exists(String.Format("{0}\\Master", QCPath)))
                {
                    Directory.CreateDirectory(String.Format("{0}\\Master", QCPath));
                }
                #endregion

                #region Routine
                string RoutinePath = String.Format("{0}\\data\\Routine", Env);
                if (!Directory.Exists(RoutinePath))
                {
                    Directory.CreateDirectory(RoutinePath);
                }

                if (!Directory.Exists(String.Format("{0}\\Master", RoutinePath)))
                {
                    Directory.CreateDirectory(String.Format("{0}\\Master", RoutinePath));
                }
                #endregion

                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}
