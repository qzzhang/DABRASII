<<<<<<< HEAD
﻿namespace DABRAS_Software
{
    partial class FormResetAdminPass
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Cancel_Button = new System.Windows.Forms.Button();
            this.btn_SavePass = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txt_OldPass = new System.Windows.Forms.TextBox();
            this.txt_NewPass = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txt_Confirm = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.BackColor = System.Drawing.Color.LightCyan;
            this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this.Cancel_Button.FlatAppearance.MouseOverBackColor = System.Drawing.Color.YellowGreen;
            this.Cancel_Button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Cancel_Button.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Cancel_Button.ForeColor = System.Drawing.Color.Maroon;
            this.Cancel_Button.Location = new System.Drawing.Point(222, 178);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.Size = new System.Drawing.Size(95, 37);
            this.Cancel_Button.TabIndex = 5;
            this.Cancel_Button.Text = "Cancel";
            this.Cancel_Button.UseVisualStyleBackColor = false;
            this.Cancel_Button.Click += new System.EventHandler(this.Cancel_Button_Click);
            // 
            // btn_SavePass
            // 
            this.btn_SavePass.BackColor = System.Drawing.Color.LightCyan;
            this.btn_SavePass.FlatAppearance.MouseOverBackColor = System.Drawing.Color.YellowGreen;
            this.btn_SavePass.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_SavePass.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_SavePass.ForeColor = System.Drawing.Color.Maroon;
            this.btn_SavePass.Location = new System.Drawing.Point(51, 178);
            this.btn_SavePass.Name = "btn_SavePass";
            this.btn_SavePass.Size = new System.Drawing.Size(165, 37);
            this.btn_SavePass.TabIndex = 4;
            this.btn_SavePass.Text = "Save Password";
            this.btn_SavePass.UseVisualStyleBackColor = false;
            this.btn_SavePass.Click += new System.EventHandler(this.btn_SavePass_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(48, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(108, 16);
            this.label1.TabIndex = 6;
            this.label1.Text = "Old Password:";
            // 
            // txt_OldPass
            // 
            this.txt_OldPass.Location = new System.Drawing.Point(183, 40);
            this.txt_OldPass.Name = "txt_OldPass";
            this.txt_OldPass.Size = new System.Drawing.Size(134, 20);
            this.txt_OldPass.TabIndex = 7;
            // 
            // txt_NewPass
            // 
            this.txt_NewPass.Location = new System.Drawing.Point(183, 85);
            this.txt_NewPass.Name = "txt_NewPass";
            this.txt_NewPass.Size = new System.Drawing.Size(134, 20);
            this.txt_NewPass.TabIndex = 9;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(48, 85);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(114, 16);
            this.label2.TabIndex = 8;
            this.label2.Text = "New Password:";
            // 
            // txt_Confirm
            // 
            this.txt_Confirm.Location = new System.Drawing.Point(183, 125);
            this.txt_Confirm.Name = "txt_Confirm";
            this.txt_Confirm.Size = new System.Drawing.Size(134, 20);
            this.txt_Confirm.TabIndex = 11;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(48, 129);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(136, 16);
            this.label3.TabIndex = 10;
            this.label3.Text = "Password Confirm:";
            // 
            // FormResetAdminPass
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(365, 262);
            this.Controls.Add(this.txt_Confirm);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txt_NewPass);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txt_OldPass);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Cancel_Button);
            this.Controls.Add(this.btn_SavePass);
            this.Name = "FormResetAdminPass";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Reset Password";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Cancel_Button;
        private System.Windows.Forms.Button btn_SavePass;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txt_OldPass;
        private System.Windows.Forms.TextBox txt_NewPass;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txt_Confirm;
        private System.Windows.Forms.Label label3;
    }
=======
﻿namespace DABRAS_Software
{
    partial class FormResetAdminPass
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Cancel_Button = new System.Windows.Forms.Button();
            this.btn_SavePass = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txt_OldPass = new System.Windows.Forms.TextBox();
            this.txt_NewPass = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txt_Confirm = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.BackColor = System.Drawing.Color.LightCyan;
            this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this.Cancel_Button.FlatAppearance.MouseOverBackColor = System.Drawing.Color.YellowGreen;
            this.Cancel_Button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Cancel_Button.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Cancel_Button.ForeColor = System.Drawing.Color.Maroon;
            this.Cancel_Button.Location = new System.Drawing.Point(222, 178);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.Size = new System.Drawing.Size(95, 37);
            this.Cancel_Button.TabIndex = 5;
            this.Cancel_Button.Text = "Cancel";
            this.Cancel_Button.UseVisualStyleBackColor = false;
            this.Cancel_Button.Click += new System.EventHandler(this.Cancel_Button_Click);
            // 
            // btn_SavePass
            // 
            this.btn_SavePass.BackColor = System.Drawing.Color.LightCyan;
            this.btn_SavePass.FlatAppearance.MouseOverBackColor = System.Drawing.Color.YellowGreen;
            this.btn_SavePass.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_SavePass.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_SavePass.ForeColor = System.Drawing.Color.Maroon;
            this.btn_SavePass.Location = new System.Drawing.Point(51, 178);
            this.btn_SavePass.Name = "btn_SavePass";
            this.btn_SavePass.Size = new System.Drawing.Size(165, 37);
            this.btn_SavePass.TabIndex = 4;
            this.btn_SavePass.Text = "Save Password";
            this.btn_SavePass.UseVisualStyleBackColor = false;
            this.btn_SavePass.Click += new System.EventHandler(this.btn_SavePass_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(48, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(108, 16);
            this.label1.TabIndex = 6;
            this.label1.Text = "Old Password:";
            // 
            // txt_OldPass
            // 
            this.txt_OldPass.Location = new System.Drawing.Point(183, 40);
            this.txt_OldPass.Name = "txt_OldPass";
            this.txt_OldPass.Size = new System.Drawing.Size(134, 20);
            this.txt_OldPass.TabIndex = 7;
            // 
            // txt_NewPass
            // 
            this.txt_NewPass.Location = new System.Drawing.Point(183, 85);
            this.txt_NewPass.Name = "txt_NewPass";
            this.txt_NewPass.Size = new System.Drawing.Size(134, 20);
            this.txt_NewPass.TabIndex = 9;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(48, 85);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(114, 16);
            this.label2.TabIndex = 8;
            this.label2.Text = "New Password:";
            // 
            // txt_Confirm
            // 
            this.txt_Confirm.Location = new System.Drawing.Point(183, 125);
            this.txt_Confirm.Name = "txt_Confirm";
            this.txt_Confirm.Size = new System.Drawing.Size(134, 20);
            this.txt_Confirm.TabIndex = 11;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(48, 129);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(136, 16);
            this.label3.TabIndex = 10;
            this.label3.Text = "Password Confirm:";
            // 
            // FormResetAdminPass
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(365, 262);
            this.Controls.Add(this.txt_Confirm);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txt_NewPass);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txt_OldPass);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Cancel_Button);
            this.Controls.Add(this.btn_SavePass);
            this.Name = "FormResetAdminPass";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Reset Password";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Cancel_Button;
        private System.Windows.Forms.Button btn_SavePass;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txt_OldPass;
        private System.Windows.Forms.TextBox txt_NewPass;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txt_Confirm;
        private System.Windows.Forms.Label label3;
    }
>>>>>>> 66cc4ad (added changes related to RCS decimal points and DPM activity display; allowing QC tests to be stopped and/or skipped during running)
}