using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DABRAS_Software
{
    static class StaticMethods
    {
        #region Static Rounding
        /*Implement rounding rule as discussed in tech note TBD-003*/
        public static string RoundToSigFigs(double NumberToRound)
        {            
            if (NumberToRound > 100)
            {
                return RoundToSpecificSigFigs(NumberToRound, 3);
            }

            return RoundToSpecificSigFigs(NumberToRound, 2);
        }

        /*Overload for integers*/
        public static string RoundToSigFigs(int NumberToRound)
        {
            return RoundToSigFigs(Convert.ToDouble(NumberToRound));
        }

        public static string RoundToSpecificSigFigs(double NumberToRound, int NumFigs)
        {
            /*Catch zero*/
            if (NumberToRound == 0)
            {
                return "0";
            }

            double LargestDigitPlace = Math.Ceiling(Math.Log10(Math.Abs(NumberToRound)));

            double Magnitude = Math.Pow(10, (NumFigs - LargestDigitPlace));
            double Shift = Math.Round(Magnitude * NumberToRound);

            return String.Format("{0}", (Shift / Magnitude));
        }

        /*Actually rounding the numbers breaks the standard deviation calculators*/
        public static string RoundToDecimal(double NumberToRound, int NumPlaces)
        {
            string retstr;
            string fmtstr = "{0:0.00#}";

            if (NumberToRound == 0)
            {
                retstr = "0.00";
            }
            switch (NumPlaces)
            {
                case 0:
                    fmtstr = "{0:0.}";
                    break;
                case 1:
                    fmtstr = "{0:0.0}";
                    break;
                case 2: 
                    fmtstr = "{0:0.00}";
                    break;
                case 3: 
                    fmtstr = "{0:0.000}";
                    break;
                case 4: 
                    fmtstr = "{0:0.0000}";
                    break;
                default:
                    fmtstr = "{0:0.00#}";
                    break;
            }

            retstr = String.Format(fmtstr, NumberToRound);
            return retstr;       
        }
        #endregion

    }
}
