<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DABRAS_Software
{
    [Serializable]
    public class ModFactors
    {
        #region Data Members
        private double Alpha_SelfAbsorbtion;
        private double Beta_SelfAbsorbtion;
        private double Beta_Backscatter;
        //private double Contaminant_Removal_Frac;
        private double Sample_Area;
        #endregion
         
        #region Constructor
        public ModFactors()
        {
            this.SetDefaultModFactors();
        }
        public ModFactors(double _ASA, double _BSA, double _BBS, double _SA)
        {
            this.Alpha_SelfAbsorbtion = _ASA;
            this.Beta_SelfAbsorbtion = _BSA;
            this.Beta_Backscatter = _BBS;
            //this.Contaminant_Removal_Frac = _CRF;
            this.Sample_Area = _SA;
        }
        #endregion

        #region Getters
        public double ComputeModdedAlpha(double InVal)
        {
            return InVal / Alpha_SelfAbsorbtion;
        }

        public double ComputeModdedBeta(double InVal)
        {
            return InVal * (100 - Beta_Backscatter) / Beta_SelfAbsorbtion; 
        }

        public double GetAlphaSelfAbsorbtion()
        {
            return this.Alpha_SelfAbsorbtion;
        }

        public double GetBetaSelfAbsorbtion()
        {
            return this.Beta_SelfAbsorbtion;
        }

        public double GetBetaBackscatter()
        {
            return this.Beta_Backscatter;
        }

        public double GetDefaultSampleArea()
        {
            return this.Sample_Area;
        }
        #endregion

        #region Setters
        public void SetDefaultModFactors()
        {
            this.Alpha_SelfAbsorbtion = 1.0;
            this.Beta_SelfAbsorbtion = 1.0;
            this.Beta_Backscatter = 1.0;
            this.Sample_Area = 100;
        }
        public bool SetAlphaSelfAbsorbtion(double _Val)
        {
            this.Alpha_SelfAbsorbtion = _Val;
            return true;
        }

        public bool SetBetaSelfAbsorbtion(double _Val)
        {
            this.Beta_SelfAbsorbtion = _Val;
            return true;
        }

        public bool SetBetaBackscatter(double _Val)
        {
            this.Beta_Backscatter = _Val;
            return true;
        }

        public bool SetDefaultSampleArea(double _Val)
        {
            this.Sample_Area = _Val;
            return true;
        }
        #endregion
    }
}
=======
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DABRAS_Software
{
    [Serializable]
    public class ModFactors
    {
        #region Data Members
        private double Alpha_SelfAbsorbtion;
        private double Beta_SelfAbsorbtion;
        private double Beta_Backscatter;
        //private double Contaminant_Removal_Frac;
        private double Sample_Area;
        #endregion
         
        #region Constructor
        public ModFactors()
        {
            this.SetDefaultModFactors();
        }
        public ModFactors(double _ASA, double _BSA, double _BBS, double _SA)
        {
            this.Alpha_SelfAbsorbtion = _ASA;
            this.Beta_SelfAbsorbtion = _BSA;
            this.Beta_Backscatter = _BBS;
            //this.Contaminant_Removal_Frac = _CRF;
            this.Sample_Area = _SA;
        }
        #endregion

        #region Getters
        public double ComputeModdedAlpha(double InVal)
        {
            return InVal / Alpha_SelfAbsorbtion;
        }

        public double ComputeModdedBeta(double InVal)
        {
            return InVal * (100 - Beta_Backscatter) / Beta_SelfAbsorbtion; 
        }

        public double GetAlphaSelfAbsorbtion()
        {
            return this.Alpha_SelfAbsorbtion;
        }

        public double GetBetaSelfAbsorbtion()
        {
            return this.Beta_SelfAbsorbtion;
        }

        public double GetBetaBackscatter()
        {
            return this.Beta_Backscatter;
        }

        public double GetDefaultSampleArea()
        {
            return this.Sample_Area;
        }
        #endregion

        #region Setters
        public void SetDefaultModFactors()
        {
            this.Alpha_SelfAbsorbtion = 1.0;
            this.Beta_SelfAbsorbtion = 1.0;
            this.Beta_Backscatter = 1.0;
            this.Sample_Area = 100;
        }
        public bool SetAlphaSelfAbsorbtion(double _Val)
        {
            this.Alpha_SelfAbsorbtion = _Val;
            return true;
        }

        public bool SetBetaSelfAbsorbtion(double _Val)
        {
            this.Beta_SelfAbsorbtion = _Val;
            return true;
        }

        public bool SetBetaBackscatter(double _Val)
        {
            this.Beta_Backscatter = _Val;
            return true;
        }

        public bool SetDefaultSampleArea(double _Val)
        {
            this.Sample_Area = _Val;
            return true;
        }
        #endregion
    }
}
>>>>>>> 66cc4ad (added changes related to RCS decimal points and DPM activity display; allowing QC tests to be stopped and/or skipped during running)
