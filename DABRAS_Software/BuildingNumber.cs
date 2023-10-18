using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DABRAS_Software
{
    public partial class BuildingNumber : Form
    {

        private DefaultConfigurations DC;

        public BuildingNumber(DefaultConfigurations _DC)
        {
            InitializeComponent();

            this.DC = _DC;
            this.Building_TB.Text = Convert.ToString(DC.GetBuildingNo());
            this.Set_TB.Text = Convert.ToString(DC.GetSetNo());

            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(KeyPressed);
        }

        public DefaultConfigurations GetDefaultConfig()
        {
            return this.DC;
        }
        private void SubmitButton_Click(object sender, EventArgs e)
        {
            this.DC.SetBuildingNo(this.Building_TB.Text);
            this.DC.SetSetNo(this.Set_TB.Text);
            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
            return;
        }

        void KeyPressed(object sender, KeyEventArgs Key)
        {
            if (Key.Control && Key.Alt && Key.Shift)
            {
                this.DialogResult = DialogResult.Abort;
                this.Close();
                return;
            }

            if (Key.KeyCode == Keys.Enter)
            {
                SubmitButton_Click(this, null);
            }

            if (Key.KeyCode == Keys.Escape)
            {
                CancelButton_Click(this, null);
            }
        }
        
    }
}
