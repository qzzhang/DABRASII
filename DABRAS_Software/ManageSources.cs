<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DABRAS_Software
{
    public partial class ManageSources : Form
    {
        #region Data Members
        private List<RadionuclideFamily> ListOfFamily;
        private List<Radioactive_Source> ListOfSources;
        Radioactive_Source R = null;
        private FormCLB LaunchedFrom;
        #endregion

        #region Constructor
        public ManageSources(FormCLB _Parent, List<RadionuclideFamily> _ListOfFamily, List<Radioactive_Source> _ListOfSources)
        {
            InitializeComponent();
            this.ListOfFamily = _ListOfFamily;
            this.ListOfSources = _ListOfSources;
            this.LaunchedFrom = _Parent;

            this.btnSaveNewNuclide.Enabled = false;
            this.btnSaveNewNuclide.Visible = true;
            this.btnSaveNuclideChanges.Enabled = false;
            this.btnSaveNuclideChanges.Visible = true;
            this.btnRemoveNuclide.Enabled = false;
            this.btnRemoveNuclide.Visible = true;
            this.cmbRadionuclides.Enabled = true;
            this.tbNewNuclideNm.Enabled = false;
            this.tbNewNuclideNm.Text = "";
            this.DisplayEnergyBand();

            this.btnDeleteSource.Enabled = false;
            this.btnSaveNewSource.Enabled = false;
            this.btnSaveSourceChange.Enabled = false;
            this.btnDeleteSource.Visible = true;
            this.btnSaveNewSource.Visible = true;
            this.btnSaveSourceChange.Visible = true;
            this.rdBtnViewSources.Checked = true;
            this.Serial_TB.Enabled = false;
            this.Serial_TB.Text = "";

            this.FillFamilyCombo();

            this.HalfLife_Combobox.Items.Clear();
            this.HalfLife_Combobox.Items.Add("Seconds");
            this.HalfLife_Combobox.Items.Add("Minutes");
            this.HalfLife_Combobox.Items.Add("Days");
            this.HalfLife_Combobox.Items.Add("Months");
            this.HalfLife_Combobox.Items.Add("Years");
        }
        #endregion

        #region Private Utility Functions
        private void Set_Beta_Modifier_Buttons(bool EnabledValue)
        {
            this._100_Energy_Button.Enabled = EnabledValue;
            this._100_200_Energy_Button.Enabled = EnabledValue;
            this._200_400_Energy_Button.Enabled = EnabledValue;
            this._400_1200_Energy_Button.Enabled = EnabledValue;
            this._1200_Energy_Button.Enabled = EnabledValue;
            return;
        }
        private void FillFamilyCombo()
        {
            string fnm = "";
            this.cmbRadionuclides.Items.Clear();
            foreach (RadionuclideFamily rf in this.ListOfFamily)
            {
                fnm = rf.GetName();
                if(fnm != "" && fnm != "Background")
                    this.cmbRadionuclides.Items.Add(fnm);
            }
            try
            {
                this.cmbRadionuclides.SelectedIndex = 0;
            }
            catch
            {
                MessageBox.Show("The familylist is empty.");
            }
        }
        private void FillFamilyData(RadionuclideFamily F)
        {
            if (F == null)
                return;

            this.cmbRadionuclides.Text = F.GetName();

            switch (F.GetSourceType())
            {
                case RadionuclideFamily.RadiationType.Alpha:
                    this.Alpha_Button.Checked = true;
                    this.grpBetaEnergyBand.Visible = false;
                    break;
                case RadionuclideFamily.RadiationType.Beta:
                    this.Beta_Button.Checked = true;
                    this.grpBetaEnergyBand.Visible = true;
                    this.SetBetaEnergyLevel();
                    break;
                /*case RadionuclideFamily.RadiationType.Gamma:
                    this.Gamma_Button.Checked = true;
                    this.grpBetaEnergyBand.Visible = false;
                    break;
                case RadionuclideFamily.RadiationType.AlphaBeta:
                    this.Alpha_Beta_Button.Checked = true;
                    this.grpBetaEnergyBand.Visible = false;
                    break;
                case RadionuclideFamily.RadiationType.Unknown:
                    this.Unknown_Button.Checked = true;
                    this.grpBetaEnergyBand.Visible = false;
                    break;*/
                default:
                    this.Alpha_Button.Checked = false;
                    this.Beta_Button.Checked = false;
                    this.grpBetaEnergyBand.Visible = false;
                    break;
            }
            
            //Convert from seconds to a more user-friendly format
            double HalfLife = F.GetHalfLife();
            double FinalValue = 0.0;
            string Identifier = "Seconds";

            if ((HalfLife > 60.0) && (HalfLife < 3600.0))
            {
                Identifier = "Minutes";
                FinalValue = HalfLife / 60.0;
            }
            else if ((HalfLife >= 3600) && (HalfLife < 2678400))
            {
                Identifier = "Days";
                FinalValue /= 3600.0;
            }
            else if ((HalfLife > 2678400) && (HalfLife < 31556000))
            {
                Identifier = "Months";
                FinalValue = HalfLife / 2678400.0;
            }
            else if (HalfLife >= 31556000.0)
            {
                Identifier = "Years";
                FinalValue = HalfLife / 31556000.0;
            }

            this.HalfLife_TB.Text = Convert.ToString(FinalValue);
            this.HalfLife_Combobox.Text = Identifier;
        }
        private void fillSourceCombo(int fid)
        {
            this.Source_ComboBox.Items.Clear();
            foreach (Radioactive_Source rs in this.ListOfSources)
            {
                if (rs.GetFamilyID() == fid)
                {
                    this.Source_ComboBox.Items.Add(rs.GetSerialNumber());
                }
            }
            try
            {
                this.Source_ComboBox.SelectedIndex = 0;
            }
            catch
            {
                MessageBox.Show("The source list for this family is empty.");
            }
        }
        private void FillSourceData(Radioactive_Source R)
        {
            if (R == null)
                return;
            this.Source_ComboBox.Text = R.GetSerialNumber();
            this.Description_TB.Text = R.GetDescription();
            this.CertAct_TB.Text = Convert.ToString(R.GetCertifiedActivity());
            this.CurAct_TB.Text = Convert.ToString(R.GetCurrentlyAppliedActivity());
            DateTime CertificationDate = DateTime.Parse(R.GetCertificationDate());
            CertDate_DTP.Value = CertificationDate;
        }
        private void SetBetaEnergyLevel()
        {
            RadionuclideFamily RF = this.ListOfFamily.Find(x => (x.GetName() == this.cmbRadionuclides.Text));
            if (RF != null)
            {
                if (RF.GetSourceType() != RadionuclideFamily.RadiationType.Beta)
                {
                    this._100_Energy_Button.Enabled = false;
                    this._100_200_Energy_Button.Enabled = false;
                    this._200_400_Energy_Button.Enabled = false;
                    this._400_1200_Energy_Button.Enabled = false;
                    this._1200_Energy_Button.Enabled = false;
                    return;
                }

                //We have a beta source. Enable buttons
                this._100_Energy_Button.Enabled = true;
                this._100_200_Energy_Button.Enabled = true;
                this._200_400_Energy_Button.Enabled = true;
                this._400_1200_Energy_Button.Enabled = true;
                this._1200_Energy_Button.Enabled = true;

                RadionuclideFamily.EnergyBand CurrentEnergyLevel = RF.GetEnergyBand();

                switch (CurrentEnergyLevel)
                {
                    case RadionuclideFamily.EnergyBand.Beta_Less_100KeV:
                        this._100_Energy_Button.Checked = true;
                        this._100_200_Energy_Button.Checked = false;
                        this._200_400_Energy_Button.Checked = false;
                        this._400_1200_Energy_Button.Checked = false;
                        this._1200_Energy_Button.Checked = false;
                        break;
                    case RadionuclideFamily.EnergyBand.Beta_100_200KeV:
                        this._100_Energy_Button.Checked = false;
                        this._100_200_Energy_Button.Checked = true;
                        this._200_400_Energy_Button.Checked = false;
                        this._400_1200_Energy_Button.Checked = false;
                        this._1200_Energy_Button.Checked = false;
                        break;
                    case RadionuclideFamily.EnergyBand.Beta_200_400KeV:
                        this._100_Energy_Button.Checked = false;
                        this._100_200_Energy_Button.Checked = false;
                        this._200_400_Energy_Button.Checked = true;
                        this._400_1200_Energy_Button.Checked = false;
                        this._1200_Energy_Button.Checked = false;
                        break;
                    case RadionuclideFamily.EnergyBand.Beta_400_1200KeV:
                        this._100_Energy_Button.Checked = false;
                        this._100_200_Energy_Button.Checked = false;
                        this._200_400_Energy_Button.Checked = false;
                        this._400_1200_Energy_Button.Checked = true;
                        this._1200_Energy_Button.Checked = false;
                        break;
                    case RadionuclideFamily.EnergyBand.Beta_More_1200KeV:
                        this._100_Energy_Button.Checked = false;
                        this._100_200_Energy_Button.Checked = false;
                        this._200_400_Energy_Button.Checked = false;
                        this._400_1200_Energy_Button.Checked = false;
                        this._1200_Energy_Button.Checked = true;
                        break;
                    default:
                        this._100_Energy_Button.Checked = false;
                        this._100_200_Energy_Button.Checked = false;
                        this._200_400_Energy_Button.Checked = false;
                        this._400_1200_Energy_Button.Checked = false;
                        this._1200_Energy_Button.Checked = false;
                        break;
                }
            }
        }
        #endregion

        #region Getters
        private RadionuclideFamily.RadiationType GetRadioationType()
        {
            if (this.Alpha_Button.Checked)
                return RadionuclideFamily.RadiationType.Alpha;
            if (this.Beta_Button.Checked)
                return RadionuclideFamily.RadiationType.Beta;
            /*if (this.Alpha_Beta_Button.Checked)
                return RadionuclideFamily.RadiationType.AlphaBeta;
            if (this.Gamma_Button.Checked)
                return RadionuclideFamily.RadiationType.Gamma;*/
            return RadionuclideFamily.RadiationType.Unknown;
        }
        public List<Radioactive_Source> GetRadioActiveSourceList()
        {
            return ListOfSources;
        }
        private double GetHalfLife()
        {
            if (String.Compare(this.HalfLife_Combobox.Text, "Seconds") == 0)
            {
                return Convert.ToDouble(this.HalfLife_TB.Text);
            }

            if (String.Compare(this.HalfLife_Combobox.Text, "Minutes") == 0)
            {
                double Temp = Convert.ToDouble(this.HalfLife_TB.Text);
                return Temp * 60;
            }

            if (String.Compare(this.HalfLife_Combobox.Text, "Hours") == 0)
            {
                double Temp = Convert.ToDouble(this.HalfLife_TB.Text);
                return Temp * 3600;
            }

            if (String.Compare(this.HalfLife_Combobox.Text, "Days") == 0)
            {
                double Temp = Convert.ToDouble(this.HalfLife_TB.Text);
                return Temp * 86400;
            }

            if (String.Compare(this.HalfLife_Combobox.Text, "Months") == 0)
            {
                double Temp = Convert.ToDouble(this.HalfLife_TB.Text);
                return Temp * 2678400;
            }

            if (String.Compare(this.HalfLife_Combobox.Text, "Years") == 0)
            {
                double Temp = Convert.ToDouble(this.HalfLife_TB.Text);
                return Temp * 31556000;
            }

            /****Set half-life                 
                if (this.HalfLife_Combobox.Text == "Seconds")
                {
                    HalfLife = Convert.ToDouble(HalfLife_TB.Text);
                }

                if (this.HalfLife_Combobox.Text == "Minutes")
                {
                    HalfLife = Convert.ToDouble(HalfLife_TB.Text) * 60.0;
                }

                if (this.HalfLife_Combobox.Text == "Days")
                {
                    HalfLife = Convert.ToDouble(HalfLife_TB.Text) * 86400.0;
                }

                if (this.HalfLife_Combobox.Text == "Months")
                {
                    HalfLife = Convert.ToDouble(HalfLife_TB.Text) * 2678400.0;
                }

                if (this.HalfLife_Combobox.Text == "Years")
                {
                    HalfLife = Convert.ToDouble(HalfLife_TB.Text) * 31556000.0;
                }             * */

            return 0;
        }
        private RadionuclideFamily.EnergyBand GetBetaEnergyLevel()
        {
            if (this.Alpha_Button.Checked)
            {
                return RadionuclideFamily.EnergyBand.Alpha;
            }

            if (this.Beta_Button.Checked)
            {
                if (this._100_Energy_Button.Checked)
                {
                    return RadionuclideFamily.EnergyBand.Beta_Less_100KeV;
                }
                if (this._100_200_Energy_Button.Checked)
                {
                    return RadionuclideFamily.EnergyBand.Beta_100_200KeV;
                }

                if (this._200_400_Energy_Button.Checked)
                {
                    return RadionuclideFamily.EnergyBand.Beta_200_400KeV;
                }

                if (this._400_1200_Energy_Button.Checked)
                {
                    return RadionuclideFamily.EnergyBand.Beta_400_1200KeV;
                }

                if (this._1200_Energy_Button.Checked)
                {
                    return RadionuclideFamily.EnergyBand.Beta_More_1200KeV;
                }
            }
            /*if (this.Alpha_Beta_Button.Checked)
            {
                return RadionuclideFamily.EnergyBand.AlphaBeta;
            }*/

            return RadionuclideFamily.EnergyBand.Unknown;
        }
        #endregion

        #region radio button handlers
        private void rdAddNewNuclide_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rdAddNewNuclide.Checked)
            {
                //Allow user to edit, block the rest of the form
                this.btnSaveNewNuclide.Enabled = true;
                this.btnSaveNewNuclide.Visible = true;
                this.btnSaveNuclideChanges.Enabled = false;
                this.btnSaveNuclideChanges.Visible = true;
                this.btnRemoveNuclide.Enabled = false;
                this.btnRemoveNuclide.Visible = true;
                this.cmbRadionuclides.Enabled = false;
                this.DisplayEnergyBand();
                this.tbNewNuclideNm.Enabled = true;
                this.tbNewNuclideNm.Visible = true;

                this.btnSaveSourceChange.Enabled = false;
                this.btnSaveSourceChange.Visible = true;
                this.btnSaveNewSource.Enabled = false;
                this.btnSaveNewSource.Visible = true;
                this.btnDeleteSource.Enabled = false;
                this.btnDeleteSource.Visible = true;
                this.Serial_TB.Enabled = false;
                this.Serial_TB.Text = "";
                this.Description_TB.Enabled = false;
                this.Alpha_Button.Enabled = true;
                this.Beta_Button.Enabled = true;
                /*this.Gamma_Button.Enabled = true;
                this.Alpha_Beta_Button.Enabled = true;
                this.Unknown_Button.Enabled = true;*/
                this.CertDate_DTP.Enabled = false;
                this.CertAct_TB.Enabled = false;

                this.HalfLife_TB.Enabled = true;
                this.HalfLife_Combobox.Enabled = true;

                this.Source_ComboBox.Enabled = false;
            }
        }
        private void rdBtnRemoveNuclide_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rdBtnRemoveNuclide.Checked)
            {//Don't Allow user to edit, Enable the rest of the form for view
                this.btnSaveNewNuclide.Enabled = false;
                this.btnSaveNewNuclide.Visible = true;
                this.btnSaveNuclideChanges.Enabled = false;
                this.btnSaveNuclideChanges.Visible = true;
                this.btnRemoveNuclide.Enabled = true;
                this.btnRemoveNuclide.Visible = true;
                this.cmbRadionuclides.Enabled = true;
                this.DisplayEnergyBand();
                this.tbNewNuclideNm.Enabled = false;

                this.btnSaveSourceChange.Enabled = false;
                this.btnSaveSourceChange.Visible = true;
                this.btnSaveNewSource.Enabled = false;
                this.btnSaveNewSource.Visible = true;
                this.btnDeleteSource.Enabled = false;
                this.btnDeleteSource.Visible = true;
                this.Serial_TB.Text = "";
                this.Serial_TB.Enabled = false;
                this.Description_TB.Enabled = false;
                this.CertDate_DTP.Enabled = false;
                this.CertAct_TB.Enabled = false;

                this.Alpha_Button.Enabled = false;
                this.Beta_Button.Enabled = false;
                /*this.Gamma_Button.Enabled = false;
                this.Alpha_Beta_Button.Enabled = false;
                this.Unknown_Button.Enabled = false;*/
                this.HalfLife_Combobox.Enabled = false;
                this.HalfLife_TB.Enabled = false;

                this.Source_ComboBox.Enabled = false;
            }
        }
        private void rdEditNuclide_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rdEditNuclide.Checked)
            {//Don't Allow user to edit, Enable the rest of the form for view
                this.btnSaveNuclideChanges.Enabled = true;
                this.btnSaveNuclideChanges.Visible = true;
                this.btnSaveNewNuclide.Enabled = false;
                this.btnSaveNewNuclide.Visible = true;
                this.btnRemoveNuclide.Enabled = false;
                this.btnRemoveNuclide.Visible = true;
                this.cmbRadionuclides.Enabled = true;
                this.DisplayEnergyBand();
                this.tbNewNuclideNm.Enabled = false;

                this.btnSaveSourceChange.Enabled = false;
                this.btnSaveSourceChange.Visible = true;
                this.btnSaveNewSource.Enabled = false;
                this.btnSaveNewSource.Visible = true;
                this.btnDeleteSource.Enabled = false;
                this.btnDeleteSource.Visible = true;
                this.Serial_TB.Enabled = false;
                this.Serial_TB.Text = "";
                this.Description_TB.Enabled = false;
                this.Alpha_Button.Enabled = true;
                this.Beta_Button.Enabled = true;               
                /*this.Gamma_Button.Enabled = true;
                this.Alpha_Beta_Button.Enabled = true;
                this.Unknown_Button.Enabled = true;*/

                this.CertDate_DTP.Enabled = false;
                this.CertAct_TB.Enabled = false;

                this.HalfLife_TB.Enabled = true;
                this.HalfLife_Combobox.Enabled = true;

                this.Source_ComboBox.Enabled = false;
            }
        }
        private void rdBtnEditSource_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rdBtnEditSource.Checked)
            {
                //Allow user to edit, block the rest of the form
                this.btnSaveNewNuclide.Enabled = false;
                this.btnSaveNewNuclide.Visible = true;
                this.btnRemoveNuclide.Enabled = false;
                this.btnRemoveNuclide.Visible = true;
                this.btnSaveNuclideChanges.Enabled = false;
                this.cmbRadionuclides.Enabled = true;
                this.DisplayEnergyBand();

                this.btnSaveSourceChange.Enabled = true;
                this.btnSaveSourceChange.Visible = true;
                this.btnSaveNewSource.Enabled = false;
                this.btnSaveNewSource.Visible = true;
                this.btnDeleteSource.Enabled = false;
                this.btnDeleteSource.Visible = true;
                this.Description_TB.Enabled = true;
                this.Serial_TB.Enabled = false;
                this.Serial_TB.Text = "";
                this.Serial_TB.Enabled = false;
                this.Description_TB.Enabled = true;
                this.CertDate_DTP.Enabled = true;
                this.CertAct_TB.Enabled = true;

                this.Alpha_Button.Enabled = false;
                this.Beta_Button.Enabled = false;
                /*this.Gamma_Button.Enabled = false;
                this.Alpha_Beta_Button.Enabled = false;
                this.Unknown_Button.Enabled = false;*/
                this.HalfLife_TB.Enabled = false;
                this.HalfLife_Combobox.Enabled = false;
                this.Source_ComboBox.Enabled = true;
            }
        }
        private void rdBtnAddNewSource_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rdBtnAddNewSource.Checked)
            {
                //Allow user to edit, block the rest of the form
                this.btnSaveNewNuclide.Enabled = false;
                this.btnSaveNewNuclide.Visible = true;
                this.btnRemoveNuclide.Enabled = false;
                this.btnRemoveNuclide.Visible = true;
                this.btnSaveNuclideChanges.Enabled = false;
                this.cmbRadionuclides.Enabled = true;
                this.DisplayEnergyBand();
                this.tbNewNuclideNm.Enabled = false;

                this.btnSaveSourceChange.Enabled = false;
                this.btnSaveSourceChange.Visible = true;
                this.btnSaveNewSource.Enabled = true;
                this.btnSaveNewSource.Visible = true;
                this.btnDeleteSource.Enabled = false;
                this.btnDeleteSource.Visible = true;
                this.Description_TB.Enabled = true;
                this.Serial_TB.Enabled = true;
                this.CertDate_DTP.Enabled = true;
                this.CertAct_TB.Enabled = true;

                this.Alpha_Button.Enabled = false;
                this.Beta_Button.Enabled = false;
                /*this.Gamma_Button.Enabled = false;
                this.Alpha_Beta_Button.Enabled = false;
                this.Unknown_Button.Enabled = false;*/
                this.HalfLife_TB.Enabled = false;
                this.HalfLife_Combobox.Enabled = false;

                this.Source_ComboBox.Enabled = false;
            }
        }
        private void rdBtnViewSources_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rdBtnViewSources.Checked)
            {//Don't Allow user to edit, Enable the rest of the form for view
                this.btnSaveNewNuclide.Enabled = false;
                this.btnSaveNewNuclide.Visible = true;
                this.btnRemoveNuclide.Enabled = false;
                this.btnRemoveNuclide.Visible = true;
                this.btnSaveNuclideChanges.Enabled = false;
                this.cmbRadionuclides.Enabled = true;
                this.DisplayEnergyBand();
                this.tbNewNuclideNm.Enabled = false;

                this.btnSaveSourceChange.Enabled = false;
                this.btnSaveSourceChange.Visible = true;
                this.btnSaveNewSource.Enabled = false;
                this.btnSaveNewSource.Visible = true;
                this.btnDeleteSource.Enabled = false;
                this.btnDeleteSource.Visible = true;
                this.Serial_TB.Enabled = false;
                this.Serial_TB.Text = "";
                this.Serial_TB.Enabled = false;
                this.Description_TB.Enabled = false;
                this.CertDate_DTP.Enabled = false;
                this.CertAct_TB.Enabled = false;

                this.Alpha_Button.Enabled = false;
                this.Beta_Button.Enabled = false;
                /*this.Gamma_Button.Enabled = false;
                this.Alpha_Beta_Button.Enabled = false;
                this.Unknown_Button.Enabled = false;*/
                this.HalfLife_Combobox.Enabled = false;
                this.HalfLife_TB.Enabled = false;
                this.Source_ComboBox.Enabled = true;
            }
        }
        private void rdBtnRemoveSource_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rdBtnRemoveSource.Checked)
            {//Don't Allow user to edit, Enable the rest of the form for view
                this.btnSaveNewNuclide.Enabled = false;
                this.btnSaveNewNuclide.Visible = true;
                this.btnRemoveNuclide.Enabled = false;
                this.btnRemoveNuclide.Visible = true;
                this.btnSaveNuclideChanges.Enabled = false;
                this.cmbRadionuclides.Enabled = true;
                this.DisplayEnergyBand();
                this.tbNewNuclideNm.Enabled = false;

                this.btnSaveSourceChange.Enabled = false;
                this.btnSaveSourceChange.Visible = true;
                this.btnSaveNewSource.Enabled = false;
                this.btnSaveNewSource.Visible = true;
                this.btnDeleteSource.Enabled = true;
                this.btnDeleteSource.Visible = true;
                this.Serial_TB.Enabled = false;
                this.Serial_TB.Text = "";
                this.Serial_TB.Enabled = false;
                this.Description_TB.Enabled = false;
                this.CertDate_DTP.Enabled = false;
                this.CertAct_TB.Enabled = false;

                this.Alpha_Button.Enabled = false;
                this.Beta_Button.Enabled = false;
                /*this.Gamma_Button.Enabled = false;
                this.Alpha_Beta_Button.Enabled = false;
                this.Unknown_Button.Enabled = false;*/
                this.HalfLife_Combobox.Enabled = false;
                this.Source_ComboBox.Enabled = true;
            }
        }

        private void DisplayEnergyBand()
        {
            if (this.Beta_Button.Checked)
            {
                this.grpBetaEnergyBand.Enabled = true;
                this.grpBetaEnergyBand.Visible = true;
                if (this.rdEditNuclide.Checked || this.rdAddNewNuclide.Checked)
                {
                    this.Set_Beta_Modifier_Buttons(true);
                }
                else
                {
                    this.Set_Beta_Modifier_Buttons(false);
                }
            }
            else
            {
                this.grpBetaEnergyBand.Enabled = false;
                this.grpBetaEnergyBand.Visible = false;
                this.Set_Beta_Modifier_Buttons(false);
            }
        }

        private void Alpha_Button_CheckedChanged(object sender, EventArgs e)
        {
            this.DisplayEnergyBand();
        }

        private void Beta_Button_CheckedChanged(object sender, EventArgs e)
        {
            this.DisplayEnergyBand();
        }

        private void Gamma_Button_CheckedChanged(object sender, EventArgs e)
        {
            this.DisplayEnergyBand();
        }

        private void Alpha_Beta_Button_CheckedChanged(object sender, EventArgs e)
        {
            this.DisplayEnergyBand();
        }

        private void Unknown_Button_CheckedChanged(object sender, EventArgs e)
        {
            this.DisplayEnergyBand();
        }
        #endregion

        #region Remove Source Handler
        private void btnDeleteSource_Click(object sender, EventArgs e)
        {
            if (this.rdBtnRemoveSource.Checked)
            {
                string src_serial = this.Source_ComboBox.Text;
                if (this.ListOfSources.Count == 1)
                {
                    if (MessageBox.Show("This is the last source in the calibration list, delete it will reset the calibration list to its default. Proceed?", "Confirm Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        this.LaunchedFrom.GetParentForm().resetDABRAS2Default();
                        this.LaunchedFrom.updFamilyAndSource(this.LaunchedFrom.GetParentForm().GetListOfFamily(), this.LaunchedFrom.GetParentForm().GetListOfSources());
                        this.ListOfFamily = this.LaunchedFrom.GetParentForm().GetListOfFamily();
                        this.ListOfSources = this.LaunchedFrom.GetParentForm().GetListOfSources();
                    }
                }
                else if (src_serial != "")
                {
                    int src_index = this.ListOfSources.FindIndex(x => (x.GetSerialNumber() == src_serial));
                    if (src_index >= 1)
                    {
                        if (MessageBox.Show("Delete this source from the " + this.cmbRadionuclides.Text + " family? This CANNOT be undone.", "Confirm Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            this.ListOfSources.RemoveAt(src_index);
                            this.LaunchedFrom.updFamilyAndSource(this.ListOfFamily, this.ListOfSources);
                            int f_id = this.ListOfFamily.Find(x => (x.GetName() == this.cmbRadionuclides.Text)).GetFamilyID();
                            this.fillSourceCombo(f_id);
                            MessageBox.Show(src_serial + " has been deleted.");
                        }
                    }
                    else if( src_index == 0)
                    {
                        if (MessageBox.Show("This is the ONLY source in " + this.cmbRadionuclides.Text + " family, delete? This CANNOT be undone.", "Confirm Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            this.ListOfSources.RemoveAt(src_index);
                            this.LaunchedFrom.updFamilyAndSource(this.ListOfFamily, this.ListOfSources);
                            int f_id = this.ListOfFamily.Find(x => (x.GetName() == this.cmbRadionuclides.Text)).GetFamilyID();
                            this.fillSourceCombo(f_id);
                            MessageBox.Show(src_serial + " has been deleted.");
                        }
                    }
                }
                this.FillFamilyCombo();
                this.LaunchedFrom.refresh_Source_ComboBox();
            }
        }
        #endregion

        #region Save New Source Handler
        private void btn_SaveNewSource_Click(object sender, EventArgs e)
        {
            if (this.rdBtnAddNewSource.Checked)
            {
                int f_id = this.ListOfFamily.Find(x => (x.GetName() == this.cmbRadionuclides.Text)).GetFamilyID();
                if (this.Serial_TB.Text != "")
                {
                    if (MessageBox.Show("Add a new source " + this.Serial_TB.Text + "?", "Confirm Action", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        if (this.ListOfSources.Find(x => (x.GetSerialNumber() == this.Serial_TB.Text)) == null)
                        {
                            R = new Radioactive_Source(f_id, this.Serial_TB.Text, this.Description_TB.Text, this.CertDate_DTP.Text, Convert.ToInt32(this.CertAct_TB.Text));
                            R.SetHalfLife((this.ListOfFamily.Find(x => (x.GetFamilyID() == R.GetFamilyID()))).GetHalfLife());
                            this.ListOfSources.Add(R);
                            this.LaunchedFrom.updFamilyAndSource(this.ListOfFamily, this.ListOfSources);
                            MessageBox.Show("New source added.");

                            this.fillSourceCombo(f_id);
                            this.Serial_TB.Text = "";
                            return;
                        }
                        else
                        {
                            MessageBox.Show("Serial number already exists.");
                            return;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Serial number is blank.");
                    return;
                }
            }
            return;
        }
        #endregion

        #region Save Source Changes Handler
        private void btnSaveChanges_Click(object sender, EventArgs e)
        {
            string src_serial = Source_ComboBox.Text;
            if (!this.rdBtnEditSource.Checked)
                return;
            if (MessageBox.Show("Save data? This will overwrite the previous source data of " + src_serial + ".", "Confirm Write", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Radioactive_Source CurrentSource = ListOfSources.Find(x => (x.GetSerialNumber() == Source_ComboBox.Text));
                CurrentSource.SetDescription(this.Description_TB.Text);
                CurrentSource.SetCertificationDate(this.CertDate_DTP.Text);
                CurrentSource.SetCertifiedActivity(Convert.ToInt32(this.CertAct_TB.Text));

                this.fillSourceCombo(CurrentSource.GetFamilyID());
                this.LaunchedFrom.updFamilyAndSource(this.ListOfFamily, this.ListOfSources);

                MessageBox.Show(src_serial + " source data has been updated.");
            }
        }
        #endregion

        #region Add New Nuclide Handler
        private void btnSaveNewNuclide_Click(object sender, EventArgs e)
        {
            if (this.rdAddNewNuclide.Checked)
            {
                if (MessageBox.Show("Add a new radionuclide " + this.tbNewNuclideNm.Text + "?", "Confirm Action", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    double HalfLife = this.GetHalfLife();

                    RadionuclideFamily RF = this.ListOfFamily.Find(x => (x.GetName() == this.tbNewNuclideNm.Text));
                    if (RF == null)
                    {
                        RF = new RadionuclideFamily(this.ListOfFamily.Count, this.tbNewNuclideNm.Text, this.GetRadioationType(), this.GetBetaEnergyLevel(), HalfLife);

                        this.ListOfFamily.Add(RF);
                        MessageBox.Show("New radionuclide added.");
                    }
                    else
                    {
                        RF.SetSourceType(this.GetRadioationType());
                        RF.SetEnergyBand(this.GetBetaEnergyLevel());
                        RF.SetHalfLife(HalfLife);
                    }
                    this.FillFamilyCombo();
                    this.LaunchedFrom.updFamilyAndSource(this.ListOfFamily, this.ListOfSources);
                }
            }
        }
        #endregion

        #region Save Changed Nuclide Handler
        private void btnSaveNuclideChanges_Click(object sender, EventArgs e)
        {
            if (this.rdEditNuclide.Checked)
            {
                if (MessageBox.Show("Apply changes to radionuclide " + this.cmbRadionuclides.Text + "?", "Confirm Action", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    double HalfLife = this.GetHalfLife();

                    RadionuclideFamily RF = this.ListOfFamily.Find(x => (x.GetName() == this.cmbRadionuclides.Text));
                    if (RF == null)
                    {
                        RF = new RadionuclideFamily(this.ListOfFamily.Count, this.cmbRadionuclides.Text, this.GetRadioationType(), this.GetBetaEnergyLevel(), HalfLife);

                        this.ListOfFamily.Add(RF);
                        MessageBox.Show("New radionuclide added.");
                    }
                    else
                    {
                        RF.SetSourceType(this.GetRadioationType());
                        RF.SetEnergyBand(this.GetBetaEnergyLevel());
                        RF.SetHalfLife(HalfLife);
                        MessageBox.Show(this.cmbRadionuclides.Text + " has been updated.");
                    }
                    this.FillFamilyCombo();
                    this.LaunchedFrom.updFamilyAndSource(this.ListOfFamily, this.ListOfSources);
                }
            }

        }
        #endregion

        #region Remove Nuclide Handler
        private void btnRemoveNuclide_Click(object sender, EventArgs e)
        {
            if (this.rdBtnRemoveNuclide.Checked)
            {
                string nucld_nm = this.cmbRadionuclides.Text;
                if (MessageBox.Show("Deleting this family means all sources under this family will be deleted as well. This CANNOT be undone. Proceed?", "Confirm Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    RadionuclideFamily RF = this.ListOfFamily.Find(x => (x.GetName() == nucld_nm));
                    if (RF != null)
                    {
                        int fid = RF.GetFamilyID();
                        foreach(Radioactive_Source rs in this.ListOfSources.ToList())
                        {
                            if (rs.GetFamilyID() == fid)
                                this.ListOfSources.Remove(rs);
                        }
                        this.ListOfFamily.Remove(RF);
                        this.FillFamilyCombo();
                        this.LaunchedFrom.updFamilyAndSource(this.ListOfFamily, this.ListOfSources);
                        this.LaunchedFrom.refresh_Source_ComboBox();
                        MessageBox.Show(nucld_nm + " and its dependents have been removed." );
                    }
                }
            }
        }
        #endregion

        #region Misc GUI Handlers
        private void Source_ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.Source_ComboBox_TextUpdate(sender, e);
        }

        private void Source_ComboBox_TextUpdate(object sender, EventArgs e)
        {
            Radioactive_Source R = ListOfSources.Find(x => (x.GetSerialNumber() == Source_ComboBox.Text));
            this.FillSourceData(R);
        }
        private void cmbRadionuclides_SelectedIndexChanged(object sender, EventArgs e)
        {
            string nm = this.cmbRadionuclides.Text;
            RadionuclideFamily rf = this.ListOfFamily.Find(x => (x.GetName() == nm));
            if (rf != null)
            {
                this.FillFamilyData(rf);
                this.fillSourceCombo(rf.GetFamilyID());
                this.DisplayEnergyBand();
            }
        }
        #endregion

        #region Finalization
        private void ManageSources_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if (MessageBox.Show("Do you want to save all your changes to the radionuclide sources?", "Confirm Action", MessageBoxButtons.YesNo) == DialogResult.Yes)
            //{
                //save all the changes to the "DABRAS_conf.dat" file
                this.LaunchedFrom.updFamilyAndSource(this.ListOfFamily, this.ListOfSources);
            //}
            this.LaunchedFrom.Enabled = true;
        }
        #endregion
    }
}
=======
﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DABRAS_Software
{
    public partial class ManageSources : Form
    {
        #region Data Members
        private List<RadionuclideFamily> ListOfFamily;
        private List<Radioactive_Source> ListOfSources;
        Radioactive_Source R = null;
        private FormCLB LaunchedFrom;
        #endregion

        #region Constructor
        public ManageSources(FormCLB _Parent, List<RadionuclideFamily> _ListOfFamily, List<Radioactive_Source> _ListOfSources)
        {
            InitializeComponent();
            this.ListOfFamily = _ListOfFamily;
            this.ListOfSources = _ListOfSources;
            this.LaunchedFrom = _Parent;

            this.btnSaveNewNuclide.Enabled = false;
            this.btnSaveNewNuclide.Visible = true;
            this.btnSaveNuclideChanges.Enabled = false;
            this.btnSaveNuclideChanges.Visible = true;
            this.btnRemoveNuclide.Enabled = false;
            this.btnRemoveNuclide.Visible = true;
            this.cmbRadionuclides.Enabled = true;
            this.tbNewNuclideNm.Enabled = false;
            this.tbNewNuclideNm.Text = "";
            this.DisplayEnergyBand();

            this.btnDeleteSource.Enabled = false;
            this.btnSaveNewSource.Enabled = false;
            this.btnSaveSourceChange.Enabled = false;
            this.btnDeleteSource.Visible = true;
            this.btnSaveNewSource.Visible = true;
            this.btnSaveSourceChange.Visible = true;
            this.rdBtnViewSources.Checked = true;
            this.Serial_TB.Enabled = false;
            this.Serial_TB.Text = "";

            this.FillFamilyCombo();

            this.HalfLife_Combobox.Items.Clear();
            this.HalfLife_Combobox.Items.Add("Seconds");
            this.HalfLife_Combobox.Items.Add("Minutes");
            this.HalfLife_Combobox.Items.Add("Days");
            this.HalfLife_Combobox.Items.Add("Months");
            this.HalfLife_Combobox.Items.Add("Years");
        }
        #endregion

        #region Private Utility Functions
        private void Set_Beta_Modifier_Buttons(bool EnabledValue)
        {
            this._100_Energy_Button.Enabled = EnabledValue;
            this._100_200_Energy_Button.Enabled = EnabledValue;
            this._200_400_Energy_Button.Enabled = EnabledValue;
            this._400_1200_Energy_Button.Enabled = EnabledValue;
            this._1200_Energy_Button.Enabled = EnabledValue;
            return;
        }
        private void FillFamilyCombo()
        {
            string fnm = "";
            this.cmbRadionuclides.Items.Clear();
            foreach (RadionuclideFamily rf in this.ListOfFamily)
            {
                fnm = rf.GetName();
                if(fnm != "" && fnm != "Background")
                    this.cmbRadionuclides.Items.Add(fnm);
            }
            try
            {
                this.cmbRadionuclides.SelectedIndex = 0;
            }
            catch
            {
                MessageBox.Show("The familylist is empty.");
            }
        }
        private void FillFamilyData(RadionuclideFamily F)
        {
            if (F == null)
                return;

            this.cmbRadionuclides.Text = F.GetName();

            switch (F.GetSourceType())
            {
                case RadionuclideFamily.RadiationType.Alpha:
                    this.Alpha_Button.Checked = true;
                    this.grpBetaEnergyBand.Visible = false;
                    break;
                case RadionuclideFamily.RadiationType.Beta:
                    this.Beta_Button.Checked = true;
                    this.grpBetaEnergyBand.Visible = true;
                    this.SetBetaEnergyLevel();
                    break;
                /*case RadionuclideFamily.RadiationType.Gamma:
                    this.Gamma_Button.Checked = true;
                    this.grpBetaEnergyBand.Visible = false;
                    break;
                case RadionuclideFamily.RadiationType.AlphaBeta:
                    this.Alpha_Beta_Button.Checked = true;
                    this.grpBetaEnergyBand.Visible = false;
                    break;
                case RadionuclideFamily.RadiationType.Unknown:
                    this.Unknown_Button.Checked = true;
                    this.grpBetaEnergyBand.Visible = false;
                    break;*/
                default:
                    this.Alpha_Button.Checked = false;
                    this.Beta_Button.Checked = false;
                    this.grpBetaEnergyBand.Visible = false;
                    break;
            }
            
            //Convert from seconds to a more user-friendly format
            double HalfLife = F.GetHalfLife();
            double FinalValue = 0.0;
            string Identifier = "Seconds";

            if ((HalfLife > 60.0) && (HalfLife < 3600.0))
            {
                Identifier = "Minutes";
                FinalValue = HalfLife / 60.0;
            }
            else if ((HalfLife >= 3600) && (HalfLife < 2678400))
            {
                Identifier = "Days";
                FinalValue /= 3600.0;
            }
            else if ((HalfLife > 2678400) && (HalfLife < 31556000))
            {
                Identifier = "Months";
                FinalValue = HalfLife / 2678400.0;
            }
            else if (HalfLife >= 31556000.0)
            {
                Identifier = "Years";
                FinalValue = HalfLife / 31556000.0;
            }

            this.HalfLife_TB.Text = Convert.ToString(FinalValue);
            this.HalfLife_Combobox.Text = Identifier;
        }
        private void fillSourceCombo(int fid)
        {
            this.Source_ComboBox.Items.Clear();
            foreach (Radioactive_Source rs in this.ListOfSources)
            {
                if (rs.GetFamilyID() == fid)
                {
                    this.Source_ComboBox.Items.Add(rs.GetSerialNumber());
                }
            }
            try
            {
                this.Source_ComboBox.SelectedIndex = 0;
            }
            catch
            {
                MessageBox.Show("The source list for this family is empty.");
            }
        }
        private void FillSourceData(Radioactive_Source R)
        {
            if (R == null)
                return;
            this.Source_ComboBox.Text = R.GetSerialNumber();
            this.Description_TB.Text = R.GetDescription();
            this.CertAct_TB.Text = Convert.ToString(R.GetCertifiedActivity());
            this.CurAct_TB.Text = Convert.ToString(R.GetCurrentlyAppliedActivity());
            DateTime CertificationDate = DateTime.Parse(R.GetCertificationDate());
            CertDate_DTP.Value = CertificationDate;
        }
        private void SetBetaEnergyLevel()
        {
            RadionuclideFamily RF = this.ListOfFamily.Find(x => (x.GetName() == this.cmbRadionuclides.Text));
            if (RF != null)
            {
                if (RF.GetSourceType() != RadionuclideFamily.RadiationType.Beta)
                {
                    this._100_Energy_Button.Enabled = false;
                    this._100_200_Energy_Button.Enabled = false;
                    this._200_400_Energy_Button.Enabled = false;
                    this._400_1200_Energy_Button.Enabled = false;
                    this._1200_Energy_Button.Enabled = false;
                    return;
                }

                //We have a beta source. Enable buttons
                this._100_Energy_Button.Enabled = true;
                this._100_200_Energy_Button.Enabled = true;
                this._200_400_Energy_Button.Enabled = true;
                this._400_1200_Energy_Button.Enabled = true;
                this._1200_Energy_Button.Enabled = true;

                RadionuclideFamily.EnergyBand CurrentEnergyLevel = RF.GetEnergyBand();

                switch (CurrentEnergyLevel)
                {
                    case RadionuclideFamily.EnergyBand.Beta_Less_100KeV:
                        this._100_Energy_Button.Checked = true;
                        this._100_200_Energy_Button.Checked = false;
                        this._200_400_Energy_Button.Checked = false;
                        this._400_1200_Energy_Button.Checked = false;
                        this._1200_Energy_Button.Checked = false;
                        break;
                    case RadionuclideFamily.EnergyBand.Beta_100_200KeV:
                        this._100_Energy_Button.Checked = false;
                        this._100_200_Energy_Button.Checked = true;
                        this._200_400_Energy_Button.Checked = false;
                        this._400_1200_Energy_Button.Checked = false;
                        this._1200_Energy_Button.Checked = false;
                        break;
                    case RadionuclideFamily.EnergyBand.Beta_200_400KeV:
                        this._100_Energy_Button.Checked = false;
                        this._100_200_Energy_Button.Checked = false;
                        this._200_400_Energy_Button.Checked = true;
                        this._400_1200_Energy_Button.Checked = false;
                        this._1200_Energy_Button.Checked = false;
                        break;
                    case RadionuclideFamily.EnergyBand.Beta_400_1200KeV:
                        this._100_Energy_Button.Checked = false;
                        this._100_200_Energy_Button.Checked = false;
                        this._200_400_Energy_Button.Checked = false;
                        this._400_1200_Energy_Button.Checked = true;
                        this._1200_Energy_Button.Checked = false;
                        break;
                    case RadionuclideFamily.EnergyBand.Beta_More_1200KeV:
                        this._100_Energy_Button.Checked = false;
                        this._100_200_Energy_Button.Checked = false;
                        this._200_400_Energy_Button.Checked = false;
                        this._400_1200_Energy_Button.Checked = false;
                        this._1200_Energy_Button.Checked = true;
                        break;
                    default:
                        this._100_Energy_Button.Checked = false;
                        this._100_200_Energy_Button.Checked = false;
                        this._200_400_Energy_Button.Checked = false;
                        this._400_1200_Energy_Button.Checked = false;
                        this._1200_Energy_Button.Checked = false;
                        break;
                }
            }
        }
        #endregion

        #region Getters
        private RadionuclideFamily.RadiationType GetRadioationType()
        {
            if (this.Alpha_Button.Checked)
                return RadionuclideFamily.RadiationType.Alpha;
            if (this.Beta_Button.Checked)
                return RadionuclideFamily.RadiationType.Beta;
            /*if (this.Alpha_Beta_Button.Checked)
                return RadionuclideFamily.RadiationType.AlphaBeta;
            if (this.Gamma_Button.Checked)
                return RadionuclideFamily.RadiationType.Gamma;*/
            return RadionuclideFamily.RadiationType.Unknown;
        }
        public List<Radioactive_Source> GetRadioActiveSourceList()
        {
            return ListOfSources;
        }
        private double GetHalfLife()
        {
            if (String.Compare(this.HalfLife_Combobox.Text, "Seconds") == 0)
            {
                return Convert.ToDouble(this.HalfLife_TB.Text);
            }

            if (String.Compare(this.HalfLife_Combobox.Text, "Minutes") == 0)
            {
                double Temp = Convert.ToDouble(this.HalfLife_TB.Text);
                return Temp * 60;
            }

            if (String.Compare(this.HalfLife_Combobox.Text, "Hours") == 0)
            {
                double Temp = Convert.ToDouble(this.HalfLife_TB.Text);
                return Temp * 3600;
            }

            if (String.Compare(this.HalfLife_Combobox.Text, "Days") == 0)
            {
                double Temp = Convert.ToDouble(this.HalfLife_TB.Text);
                return Temp * 86400;
            }

            if (String.Compare(this.HalfLife_Combobox.Text, "Months") == 0)
            {
                double Temp = Convert.ToDouble(this.HalfLife_TB.Text);
                return Temp * 2678400;
            }

            if (String.Compare(this.HalfLife_Combobox.Text, "Years") == 0)
            {
                double Temp = Convert.ToDouble(this.HalfLife_TB.Text);
                return Temp * 31556000;
            }

            /****Set half-life                 
                if (this.HalfLife_Combobox.Text == "Seconds")
                {
                    HalfLife = Convert.ToDouble(HalfLife_TB.Text);
                }

                if (this.HalfLife_Combobox.Text == "Minutes")
                {
                    HalfLife = Convert.ToDouble(HalfLife_TB.Text) * 60.0;
                }

                if (this.HalfLife_Combobox.Text == "Days")
                {
                    HalfLife = Convert.ToDouble(HalfLife_TB.Text) * 86400.0;
                }

                if (this.HalfLife_Combobox.Text == "Months")
                {
                    HalfLife = Convert.ToDouble(HalfLife_TB.Text) * 2678400.0;
                }

                if (this.HalfLife_Combobox.Text == "Years")
                {
                    HalfLife = Convert.ToDouble(HalfLife_TB.Text) * 31556000.0;
                }             * */

            return 0;
        }
        private RadionuclideFamily.EnergyBand GetBetaEnergyLevel()
        {
            if (this.Alpha_Button.Checked)
            {
                return RadionuclideFamily.EnergyBand.Alpha;
            }

            if (this.Beta_Button.Checked)
            {
                if (this._100_Energy_Button.Checked)
                {
                    return RadionuclideFamily.EnergyBand.Beta_Less_100KeV;
                }
                if (this._100_200_Energy_Button.Checked)
                {
                    return RadionuclideFamily.EnergyBand.Beta_100_200KeV;
                }

                if (this._200_400_Energy_Button.Checked)
                {
                    return RadionuclideFamily.EnergyBand.Beta_200_400KeV;
                }

                if (this._400_1200_Energy_Button.Checked)
                {
                    return RadionuclideFamily.EnergyBand.Beta_400_1200KeV;
                }

                if (this._1200_Energy_Button.Checked)
                {
                    return RadionuclideFamily.EnergyBand.Beta_More_1200KeV;
                }
            }
            /*if (this.Alpha_Beta_Button.Checked)
            {
                return RadionuclideFamily.EnergyBand.AlphaBeta;
            }*/

            return RadionuclideFamily.EnergyBand.Unknown;
        }
        #endregion

        #region radio button handlers
        private void rdAddNewNuclide_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rdAddNewNuclide.Checked)
            {
                //Allow user to edit, block the rest of the form
                this.btnSaveNewNuclide.Enabled = true;
                this.btnSaveNewNuclide.Visible = true;
                this.btnSaveNuclideChanges.Enabled = false;
                this.btnSaveNuclideChanges.Visible = true;
                this.btnRemoveNuclide.Enabled = false;
                this.btnRemoveNuclide.Visible = true;
                this.cmbRadionuclides.Enabled = false;
                this.DisplayEnergyBand();
                this.tbNewNuclideNm.Enabled = true;
                this.tbNewNuclideNm.Visible = true;

                this.btnSaveSourceChange.Enabled = false;
                this.btnSaveSourceChange.Visible = true;
                this.btnSaveNewSource.Enabled = false;
                this.btnSaveNewSource.Visible = true;
                this.btnDeleteSource.Enabled = false;
                this.btnDeleteSource.Visible = true;
                this.Serial_TB.Enabled = false;
                this.Serial_TB.Text = "";
                this.Description_TB.Enabled = false;
                this.Alpha_Button.Enabled = true;
                this.Beta_Button.Enabled = true;
                /*this.Gamma_Button.Enabled = true;
                this.Alpha_Beta_Button.Enabled = true;
                this.Unknown_Button.Enabled = true;*/
                this.CertDate_DTP.Enabled = false;
                this.CertAct_TB.Enabled = false;

                this.HalfLife_TB.Enabled = true;
                this.HalfLife_Combobox.Enabled = true;

                this.Source_ComboBox.Enabled = false;
            }
        }
        private void rdBtnRemoveNuclide_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rdBtnRemoveNuclide.Checked)
            {//Don't Allow user to edit, Enable the rest of the form for view
                this.btnSaveNewNuclide.Enabled = false;
                this.btnSaveNewNuclide.Visible = true;
                this.btnSaveNuclideChanges.Enabled = false;
                this.btnSaveNuclideChanges.Visible = true;
                this.btnRemoveNuclide.Enabled = true;
                this.btnRemoveNuclide.Visible = true;
                this.cmbRadionuclides.Enabled = true;
                this.DisplayEnergyBand();
                this.tbNewNuclideNm.Enabled = false;

                this.btnSaveSourceChange.Enabled = false;
                this.btnSaveSourceChange.Visible = true;
                this.btnSaveNewSource.Enabled = false;
                this.btnSaveNewSource.Visible = true;
                this.btnDeleteSource.Enabled = false;
                this.btnDeleteSource.Visible = true;
                this.Serial_TB.Text = "";
                this.Serial_TB.Enabled = false;
                this.Description_TB.Enabled = false;
                this.CertDate_DTP.Enabled = false;
                this.CertAct_TB.Enabled = false;

                this.Alpha_Button.Enabled = false;
                this.Beta_Button.Enabled = false;
                /*this.Gamma_Button.Enabled = false;
                this.Alpha_Beta_Button.Enabled = false;
                this.Unknown_Button.Enabled = false;*/
                this.HalfLife_Combobox.Enabled = false;
                this.HalfLife_TB.Enabled = false;

                this.Source_ComboBox.Enabled = false;
            }
        }
        private void rdEditNuclide_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rdEditNuclide.Checked)
            {//Don't Allow user to edit, Enable the rest of the form for view
                this.btnSaveNuclideChanges.Enabled = true;
                this.btnSaveNuclideChanges.Visible = true;
                this.btnSaveNewNuclide.Enabled = false;
                this.btnSaveNewNuclide.Visible = true;
                this.btnRemoveNuclide.Enabled = false;
                this.btnRemoveNuclide.Visible = true;
                this.cmbRadionuclides.Enabled = true;
                this.DisplayEnergyBand();
                this.tbNewNuclideNm.Enabled = false;

                this.btnSaveSourceChange.Enabled = false;
                this.btnSaveSourceChange.Visible = true;
                this.btnSaveNewSource.Enabled = false;
                this.btnSaveNewSource.Visible = true;
                this.btnDeleteSource.Enabled = false;
                this.btnDeleteSource.Visible = true;
                this.Serial_TB.Enabled = false;
                this.Serial_TB.Text = "";
                this.Description_TB.Enabled = false;
                this.Alpha_Button.Enabled = true;
                this.Beta_Button.Enabled = true;               
                /*this.Gamma_Button.Enabled = true;
                this.Alpha_Beta_Button.Enabled = true;
                this.Unknown_Button.Enabled = true;*/

                this.CertDate_DTP.Enabled = false;
                this.CertAct_TB.Enabled = false;

                this.HalfLife_TB.Enabled = true;
                this.HalfLife_Combobox.Enabled = true;

                this.Source_ComboBox.Enabled = false;
            }
        }
        private void rdBtnEditSource_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rdBtnEditSource.Checked)
            {
                //Allow user to edit, block the rest of the form
                this.btnSaveNewNuclide.Enabled = false;
                this.btnSaveNewNuclide.Visible = true;
                this.btnRemoveNuclide.Enabled = false;
                this.btnRemoveNuclide.Visible = true;
                this.btnSaveNuclideChanges.Enabled = false;
                this.cmbRadionuclides.Enabled = true;
                this.DisplayEnergyBand();

                this.btnSaveSourceChange.Enabled = true;
                this.btnSaveSourceChange.Visible = true;
                this.btnSaveNewSource.Enabled = false;
                this.btnSaveNewSource.Visible = true;
                this.btnDeleteSource.Enabled = false;
                this.btnDeleteSource.Visible = true;
                this.Description_TB.Enabled = true;
                this.Serial_TB.Enabled = false;
                this.Serial_TB.Text = "";
                this.Serial_TB.Enabled = false;
                this.Description_TB.Enabled = true;
                this.CertDate_DTP.Enabled = true;
                this.CertAct_TB.Enabled = true;

                this.Alpha_Button.Enabled = false;
                this.Beta_Button.Enabled = false;
                /*this.Gamma_Button.Enabled = false;
                this.Alpha_Beta_Button.Enabled = false;
                this.Unknown_Button.Enabled = false;*/
                this.HalfLife_TB.Enabled = false;
                this.HalfLife_Combobox.Enabled = false;
                this.Source_ComboBox.Enabled = true;
            }
        }
        private void rdBtnAddNewSource_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rdBtnAddNewSource.Checked)
            {
                //Allow user to edit, block the rest of the form
                this.btnSaveNewNuclide.Enabled = false;
                this.btnSaveNewNuclide.Visible = true;
                this.btnRemoveNuclide.Enabled = false;
                this.btnRemoveNuclide.Visible = true;
                this.btnSaveNuclideChanges.Enabled = false;
                this.cmbRadionuclides.Enabled = true;
                this.DisplayEnergyBand();
                this.tbNewNuclideNm.Enabled = false;

                this.btnSaveSourceChange.Enabled = false;
                this.btnSaveSourceChange.Visible = true;
                this.btnSaveNewSource.Enabled = true;
                this.btnSaveNewSource.Visible = true;
                this.btnDeleteSource.Enabled = false;
                this.btnDeleteSource.Visible = true;
                this.Description_TB.Enabled = true;
                this.Serial_TB.Enabled = true;
                this.CertDate_DTP.Enabled = true;
                this.CertAct_TB.Enabled = true;

                this.Alpha_Button.Enabled = false;
                this.Beta_Button.Enabled = false;
                /*this.Gamma_Button.Enabled = false;
                this.Alpha_Beta_Button.Enabled = false;
                this.Unknown_Button.Enabled = false;*/
                this.HalfLife_TB.Enabled = false;
                this.HalfLife_Combobox.Enabled = false;

                this.Source_ComboBox.Enabled = false;
            }
        }
        private void rdBtnViewSources_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rdBtnViewSources.Checked)
            {//Don't Allow user to edit, Enable the rest of the form for view
                this.btnSaveNewNuclide.Enabled = false;
                this.btnSaveNewNuclide.Visible = true;
                this.btnRemoveNuclide.Enabled = false;
                this.btnRemoveNuclide.Visible = true;
                this.btnSaveNuclideChanges.Enabled = false;
                this.cmbRadionuclides.Enabled = true;
                this.DisplayEnergyBand();
                this.tbNewNuclideNm.Enabled = false;

                this.btnSaveSourceChange.Enabled = false;
                this.btnSaveSourceChange.Visible = true;
                this.btnSaveNewSource.Enabled = false;
                this.btnSaveNewSource.Visible = true;
                this.btnDeleteSource.Enabled = false;
                this.btnDeleteSource.Visible = true;
                this.Serial_TB.Enabled = false;
                this.Serial_TB.Text = "";
                this.Serial_TB.Enabled = false;
                this.Description_TB.Enabled = false;
                this.CertDate_DTP.Enabled = false;
                this.CertAct_TB.Enabled = false;

                this.Alpha_Button.Enabled = false;
                this.Beta_Button.Enabled = false;
                /*this.Gamma_Button.Enabled = false;
                this.Alpha_Beta_Button.Enabled = false;
                this.Unknown_Button.Enabled = false;*/
                this.HalfLife_Combobox.Enabled = false;
                this.HalfLife_TB.Enabled = false;
                this.Source_ComboBox.Enabled = true;
            }
        }
        private void rdBtnRemoveSource_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rdBtnRemoveSource.Checked)
            {//Don't Allow user to edit, Enable the rest of the form for view
                this.btnSaveNewNuclide.Enabled = false;
                this.btnSaveNewNuclide.Visible = true;
                this.btnRemoveNuclide.Enabled = false;
                this.btnRemoveNuclide.Visible = true;
                this.btnSaveNuclideChanges.Enabled = false;
                this.cmbRadionuclides.Enabled = true;
                this.DisplayEnergyBand();
                this.tbNewNuclideNm.Enabled = false;

                this.btnSaveSourceChange.Enabled = false;
                this.btnSaveSourceChange.Visible = true;
                this.btnSaveNewSource.Enabled = false;
                this.btnSaveNewSource.Visible = true;
                this.btnDeleteSource.Enabled = true;
                this.btnDeleteSource.Visible = true;
                this.Serial_TB.Enabled = false;
                this.Serial_TB.Text = "";
                this.Serial_TB.Enabled = false;
                this.Description_TB.Enabled = false;
                this.CertDate_DTP.Enabled = false;
                this.CertAct_TB.Enabled = false;

                this.Alpha_Button.Enabled = false;
                this.Beta_Button.Enabled = false;
                /*this.Gamma_Button.Enabled = false;
                this.Alpha_Beta_Button.Enabled = false;
                this.Unknown_Button.Enabled = false;*/
                this.HalfLife_Combobox.Enabled = false;
                this.Source_ComboBox.Enabled = true;
            }
        }

        private void DisplayEnergyBand()
        {
            if (this.Beta_Button.Checked)
            {
                this.grpBetaEnergyBand.Enabled = true;
                this.grpBetaEnergyBand.Visible = true;
                if (this.rdEditNuclide.Checked || this.rdAddNewNuclide.Checked)
                {
                    this.Set_Beta_Modifier_Buttons(true);
                }
                else
                {
                    this.Set_Beta_Modifier_Buttons(false);
                }
            }
            else
            {
                this.grpBetaEnergyBand.Enabled = false;
                this.grpBetaEnergyBand.Visible = false;
                this.Set_Beta_Modifier_Buttons(false);
            }
        }

        private void Alpha_Button_CheckedChanged(object sender, EventArgs e)
        {
            this.DisplayEnergyBand();
        }

        private void Beta_Button_CheckedChanged(object sender, EventArgs e)
        {
            this.DisplayEnergyBand();
        }

        private void Gamma_Button_CheckedChanged(object sender, EventArgs e)
        {
            this.DisplayEnergyBand();
        }

        private void Alpha_Beta_Button_CheckedChanged(object sender, EventArgs e)
        {
            this.DisplayEnergyBand();
        }

        private void Unknown_Button_CheckedChanged(object sender, EventArgs e)
        {
            this.DisplayEnergyBand();
        }
        #endregion

        #region Remove Source Handler
        private void btnDeleteSource_Click(object sender, EventArgs e)
        {
            if (this.rdBtnRemoveSource.Checked)
            {
                string src_serial = this.Source_ComboBox.Text;
                if (this.ListOfSources.Count == 1)
                {
                    if (MessageBox.Show("This is the last source in the calibration list, delete it will reset the calibration list to its default. Proceed?", "Confirm Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        this.LaunchedFrom.GetParentForm().resetDABRAS2Default();
                        this.LaunchedFrom.updFamilyAndSource(this.LaunchedFrom.GetParentForm().GetListOfFamily(), this.LaunchedFrom.GetParentForm().GetListOfSources());
                        this.ListOfFamily = this.LaunchedFrom.GetParentForm().GetListOfFamily();
                        this.ListOfSources = this.LaunchedFrom.GetParentForm().GetListOfSources();
                    }
                }
                else if (src_serial != "")
                {
                    int src_index = this.ListOfSources.FindIndex(x => (x.GetSerialNumber() == src_serial));
                    if (src_index >= 1)
                    {
                        if (MessageBox.Show("Delete this source from the " + this.cmbRadionuclides.Text + " family? This CANNOT be undone.", "Confirm Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            this.ListOfSources.RemoveAt(src_index);
                            this.LaunchedFrom.updFamilyAndSource(this.ListOfFamily, this.ListOfSources);
                            int f_id = this.ListOfFamily.Find(x => (x.GetName() == this.cmbRadionuclides.Text)).GetFamilyID();
                            this.fillSourceCombo(f_id);
                            MessageBox.Show(src_serial + " has been deleted.");
                        }
                    }
                    else if( src_index == 0)
                    {
                        if (MessageBox.Show("This is the ONLY source in " + this.cmbRadionuclides.Text + " family, delete? This CANNOT be undone.", "Confirm Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            this.ListOfSources.RemoveAt(src_index);
                            this.LaunchedFrom.updFamilyAndSource(this.ListOfFamily, this.ListOfSources);
                            int f_id = this.ListOfFamily.Find(x => (x.GetName() == this.cmbRadionuclides.Text)).GetFamilyID();
                            this.fillSourceCombo(f_id);
                            MessageBox.Show(src_serial + " has been deleted.");
                        }
                    }
                }
                this.FillFamilyCombo();
                this.LaunchedFrom.refresh_Source_ComboBox();
            }
        }
        #endregion

        #region Save New Source Handler
        private void btn_SaveNewSource_Click(object sender, EventArgs e)
        {
            if (this.rdBtnAddNewSource.Checked)
            {
                int f_id = this.ListOfFamily.Find(x => (x.GetName() == this.cmbRadionuclides.Text)).GetFamilyID();
                if (this.Serial_TB.Text != "")
                {
                    if (MessageBox.Show("Add a new source " + this.Serial_TB.Text + "?", "Confirm Action", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        if (this.ListOfSources.Find(x => (x.GetSerialNumber() == this.Serial_TB.Text)) == null)
                        {
                            R = new Radioactive_Source(f_id, this.Serial_TB.Text, this.Description_TB.Text, this.CertDate_DTP.Text, Convert.ToInt32(this.CertAct_TB.Text));
                            R.SetHalfLife((this.ListOfFamily.Find(x => (x.GetFamilyID() == R.GetFamilyID()))).GetHalfLife());
                            this.ListOfSources.Add(R);
                            this.LaunchedFrom.updFamilyAndSource(this.ListOfFamily, this.ListOfSources);
                            MessageBox.Show("New source added.");

                            this.fillSourceCombo(f_id);
                            this.Serial_TB.Text = "";
                            return;
                        }
                        else
                        {
                            MessageBox.Show("Serial number already exists.");
                            return;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Serial number is blank.");
                    return;
                }
            }
            return;
        }
        #endregion

        #region Save Source Changes Handler
        private void btnSaveChanges_Click(object sender, EventArgs e)
        {
            string src_serial = Source_ComboBox.Text;
            if (!this.rdBtnEditSource.Checked)
                return;
            if (MessageBox.Show("Save data? This will overwrite the previous source data of " + src_serial + ".", "Confirm Write", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Radioactive_Source CurrentSource = ListOfSources.Find(x => (x.GetSerialNumber() == Source_ComboBox.Text));
                CurrentSource.SetDescription(this.Description_TB.Text);
                CurrentSource.SetCertificationDate(this.CertDate_DTP.Text);
                CurrentSource.SetCertifiedActivity(Convert.ToInt32(this.CertAct_TB.Text));

                this.fillSourceCombo(CurrentSource.GetFamilyID());
                this.LaunchedFrom.updFamilyAndSource(this.ListOfFamily, this.ListOfSources);

                MessageBox.Show(src_serial + " source data has been updated.");
            }
        }
        #endregion

        #region Add New Nuclide Handler
        private void btnSaveNewNuclide_Click(object sender, EventArgs e)
        {
            if (this.rdAddNewNuclide.Checked)
            {
                if (MessageBox.Show("Add a new radionuclide " + this.tbNewNuclideNm.Text + "?", "Confirm Action", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    double HalfLife = this.GetHalfLife();

                    RadionuclideFamily RF = this.ListOfFamily.Find(x => (x.GetName() == this.tbNewNuclideNm.Text));
                    if (RF == null)
                    {
                        RF = new RadionuclideFamily(this.ListOfFamily.Count, this.tbNewNuclideNm.Text, this.GetRadioationType(), this.GetBetaEnergyLevel(), HalfLife);

                        this.ListOfFamily.Add(RF);
                        MessageBox.Show("New radionuclide added.");
                    }
                    else
                    {
                        RF.SetSourceType(this.GetRadioationType());
                        RF.SetEnergyBand(this.GetBetaEnergyLevel());
                        RF.SetHalfLife(HalfLife);
                    }
                    this.FillFamilyCombo();
                    this.LaunchedFrom.updFamilyAndSource(this.ListOfFamily, this.ListOfSources);
                }
            }
        }
        #endregion

        #region Save Changed Nuclide Handler
        private void btnSaveNuclideChanges_Click(object sender, EventArgs e)
        {
            if (this.rdEditNuclide.Checked)
            {
                if (MessageBox.Show("Apply changes to radionuclide " + this.cmbRadionuclides.Text + "?", "Confirm Action", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    double HalfLife = this.GetHalfLife();

                    RadionuclideFamily RF = this.ListOfFamily.Find(x => (x.GetName() == this.cmbRadionuclides.Text));
                    if (RF == null)
                    {
                        RF = new RadionuclideFamily(this.ListOfFamily.Count, this.cmbRadionuclides.Text, this.GetRadioationType(), this.GetBetaEnergyLevel(), HalfLife);

                        this.ListOfFamily.Add(RF);
                        MessageBox.Show("New radionuclide added.");
                    }
                    else
                    {
                        RF.SetSourceType(this.GetRadioationType());
                        RF.SetEnergyBand(this.GetBetaEnergyLevel());
                        RF.SetHalfLife(HalfLife);
                        MessageBox.Show(this.cmbRadionuclides.Text + " has been updated.");
                    }
                    this.FillFamilyCombo();
                    this.LaunchedFrom.updFamilyAndSource(this.ListOfFamily, this.ListOfSources);
                }
            }

        }
        #endregion

        #region Remove Nuclide Handler
        private void btnRemoveNuclide_Click(object sender, EventArgs e)
        {
            if (this.rdBtnRemoveNuclide.Checked)
            {
                string nucld_nm = this.cmbRadionuclides.Text;
                if (MessageBox.Show("Deleting this family means all sources under this family will be deleted as well. This CANNOT be undone. Proceed?", "Confirm Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    RadionuclideFamily RF = this.ListOfFamily.Find(x => (x.GetName() == nucld_nm));
                    if (RF != null)
                    {
                        int fid = RF.GetFamilyID();
                        foreach(Radioactive_Source rs in this.ListOfSources.ToList())
                        {
                            if (rs.GetFamilyID() == fid)
                                this.ListOfSources.Remove(rs);
                        }
                        this.ListOfFamily.Remove(RF);
                        this.FillFamilyCombo();
                        this.LaunchedFrom.updFamilyAndSource(this.ListOfFamily, this.ListOfSources);
                        this.LaunchedFrom.refresh_Source_ComboBox();
                        MessageBox.Show(nucld_nm + " and its dependents have been removed." );
                    }
                }
            }
        }
        #endregion

        #region Misc GUI Handlers
        private void Source_ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.Source_ComboBox_TextUpdate(sender, e);
        }

        private void Source_ComboBox_TextUpdate(object sender, EventArgs e)
        {
            Radioactive_Source R = ListOfSources.Find(x => (x.GetSerialNumber() == Source_ComboBox.Text));
            this.FillSourceData(R);
        }
        private void cmbRadionuclides_SelectedIndexChanged(object sender, EventArgs e)
        {
            string nm = this.cmbRadionuclides.Text;
            RadionuclideFamily rf = this.ListOfFamily.Find(x => (x.GetName() == nm));
            if (rf != null)
            {
                this.FillFamilyData(rf);
                this.fillSourceCombo(rf.GetFamilyID());
                this.DisplayEnergyBand();
            }
        }
        #endregion

        #region Finalization
        private void ManageSources_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if (MessageBox.Show("Do you want to save all your changes to the radionuclide sources?", "Confirm Action", MessageBoxButtons.YesNo) == DialogResult.Yes)
            //{
                //save all the changes to the "DABRAS_conf.dat" file
                this.LaunchedFrom.updFamilyAndSource(this.ListOfFamily, this.ListOfSources);
            //}
            this.LaunchedFrom.Enabled = true;
        }
        #endregion
    }
}
>>>>>>> 66cc4ad (added changes related to RCS decimal points and DPM activity display; allowing QC tests to be stopped and/or skipped during running)
