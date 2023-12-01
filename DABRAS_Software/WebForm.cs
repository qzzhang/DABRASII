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
    public partial class WebForm : Form
    {
        #region Data Members
        private string SurveyURL = null;
        #endregion

        #region Constructor
        public WebForm(string _URL)
        {
            InitializeComponent();

            this.SurveyURL = _URL;
            if (SurveyURL != null)
            {
                this.WebBrowser.Url = new Uri(SurveyURL);
            }

            else
            {
                MessageBox.Show("Error: Network Problems.");
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }

            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(KeyPressed);
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
    public partial class WebForm : Form
    {
        #region Data Members
        private string SurveyURL = null;
        #endregion

        #region Constructor
        public WebForm(string _URL)
        {
            InitializeComponent();

            this.SurveyURL = _URL;
            if (SurveyURL != null)
            {
                this.WebBrowser.Url = new Uri(SurveyURL);
            }

            else
            {
                MessageBox.Show("Error: Network Problems.");
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }

            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(KeyPressed);
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
        }
        #endregion

    }
}
>>>>>>> 66cc4ad (added changes related to RCS decimal points and DPM activity display; allowing QC tests to be stopped and/or skipped during running)
