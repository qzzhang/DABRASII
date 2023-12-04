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
    public partial class FormResetAdminPass : Form
    {
        #region Data Members
        private string NewPass;
        private string OldPass;
        #endregion

        #region Constructor
        public FormResetAdminPass(string oldVal)
        {
            InitializeComponent();
            InitializePassControl();
            this.OldPass = oldVal;
        }
        #endregion

        private void InitializePassControl()
        {
            // Set to no text.
            txt_OldPass.Text = "";
            txt_NewPass.Text = "";
            txt_Confirm.Text = "";
            // The password character is an asterisk.
            txt_OldPass.PasswordChar = '*';
            txt_NewPass.PasswordChar = '*';
            txt_Confirm.PasswordChar = '*';
            // The control will allow no more than 14 characters.
            txt_OldPass.MaxLength = 14;
            txt_NewPass.MaxLength = 14;
            txt_Confirm.MaxLength = 14;
        }
        private void btn_SavePass_Click(object sender, EventArgs e)
        {
            if(txt_OldPass.Text != "" && txt_NewPass.Text != "" && txt_Confirm.Text != "")
            {
                if (String.Compare(txt_OldPass.Text, this.OldPass) != 0)
                {
                    MessageBox.Show("Incorrect Password.");
                    return;
                }
                if(txt_Confirm.Text != txt_NewPass.Text)
                {
                    MessageBox.Show("Passwords don't match.");
                    return;
                }
                this.NewPass = this.txt_NewPass.Text;
                this.DialogResult = DialogResult.OK;
                this.Close();
                return;
            }
        }
        public string GetNewPass()
        {
            return this.NewPass;
        }

        private void Cancel_Button_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
            return;
        }
    }
}
