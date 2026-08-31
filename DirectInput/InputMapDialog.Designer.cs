
namespace DirectInputDevices
{
    partial class InputMapDialog
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
            this.labelTitle = new System.Windows.Forms.Label();
            this.labelDev = new System.Windows.Forms.Label();
            this.labelBut = new System.Windows.Forms.Label();
            this.radioButtonPressed = new System.Windows.Forms.RadioButton();
            this.radioButtonRelease = new System.Windows.Forms.RadioButton();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.panelPressRelease = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.buttonMouseClick = new System.Windows.Forms.Button();
            this.labelDevice = new System.Windows.Forms.Label();
            this.labelKeyboard = new System.Windows.Forms.Label();
            this.panelOuter = new System.Windows.Forms.Panel();
            this.panelPressRelease.SuspendLayout();
            this.panelOuter.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelTitle
            // 
            this.labelTitle.AutoSize = true;
            this.labelTitle.Location = new System.Drawing.Point(12, 11);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(43, 13);
            this.labelTitle.TabIndex = 0;
            this.labelTitle.Text = "<code>";
            // 
            // labelDev
            // 
            this.labelDev.AutoSize = true;
            this.labelDev.Location = new System.Drawing.Point(13, 41);
            this.labelDev.Name = "labelDev";
            this.labelDev.Size = new System.Drawing.Size(44, 13);
            this.labelDev.TabIndex = 0;
            this.labelDev.Text = "Device:";
            // 
            // labelBut
            // 
            this.labelBut.AutoSize = true;
            this.labelBut.Location = new System.Drawing.Point(13, 69);
            this.labelBut.Name = "labelBut";
            this.labelBut.Size = new System.Drawing.Size(88, 13);
            this.labelBut.TabIndex = 0;
            this.labelBut.Text = "Button/Key/Axis:";
            // 
            // radioButtonPressed
            // 
            this.radioButtonPressed.AutoSize = true;
            this.radioButtonPressed.Checked = true;
            this.radioButtonPressed.Location = new System.Drawing.Point(9, 11);
            this.radioButtonPressed.Name = "radioButtonPressed";
            this.radioButtonPressed.Size = new System.Drawing.Size(95, 17);
            this.radioButtonPressed.TabIndex = 3;
            this.radioButtonPressed.TabStop = true;
            this.radioButtonPressed.Text = "When Pressed";
            this.radioButtonPressed.UseVisualStyleBackColor = true;
            // 
            // radioButtonRelease
            // 
            this.radioButtonRelease.AutoSize = true;
            this.radioButtonRelease.Location = new System.Drawing.Point(116, 11);
            this.radioButtonRelease.Name = "radioButtonRelease";
            this.radioButtonRelease.Size = new System.Drawing.Size(102, 17);
            this.radioButtonRelease.TabIndex = 4;
            this.radioButtonRelease.Text = "When Released";
            this.radioButtonRelease.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(260, 186);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 5;
            this.buttonOK.TabStop = false;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(179, 186);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.TabStop = false;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // panelPressRelease
            // 
            this.panelPressRelease.Controls.Add(this.radioButtonRelease);
            this.panelPressRelease.Controls.Add(this.radioButtonPressed);
            this.panelPressRelease.Location = new System.Drawing.Point(16, 134);
            this.panelPressRelease.Name = "panelPressRelease";
            this.panelPressRelease.Size = new System.Drawing.Size(306, 37);
            this.panelPressRelease.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 105);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(83, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Click For Mouse";
            // 
            // buttonMouseClick
            // 
            this.buttonMouseClick.Location = new System.Drawing.Point(118, 100);
            this.buttonMouseClick.Name = "buttonMouseClick";
            this.buttonMouseClick.Size = new System.Drawing.Size(85, 23);
            this.buttonMouseClick.TabIndex = 7;
            this.buttonMouseClick.TabStop = false;
            this.buttonMouseClick.Text = "Click!";
            this.buttonMouseClick.UseVisualStyleBackColor = true;
            this.buttonMouseClick.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonMouseClick_MouseDown);
            // 
            // labelDevice
            // 
            this.labelDevice.AutoSize = true;
            this.labelDevice.Location = new System.Drawing.Point(118, 40);
            this.labelDevice.Name = "labelDevice";
            this.labelDevice.Size = new System.Drawing.Size(41, 13);
            this.labelDevice.TabIndex = 8;
            this.labelDevice.Text = "Device";
            // 
            // labelKeyboard
            // 
            this.labelKeyboard.AutoSize = true;
            this.labelKeyboard.Location = new System.Drawing.Point(118, 69);
            this.labelKeyboard.Name = "labelKeyboard";
            this.labelKeyboard.Size = new System.Drawing.Size(25, 13);
            this.labelKeyboard.TabIndex = 8;
            this.labelKeyboard.Text = "Key";
            // 
            // panelOuter
            // 
            this.panelOuter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelOuter.Controls.Add(this.labelTitle);
            this.panelOuter.Controls.Add(this.buttonOK);
            this.panelOuter.Controls.Add(this.buttonCancel);
            this.panelOuter.Controls.Add(this.labelKeyboard);
            this.panelOuter.Controls.Add(this.label4);
            this.panelOuter.Controls.Add(this.labelDevice);
            this.panelOuter.Controls.Add(this.panelPressRelease);
            this.panelOuter.Controls.Add(this.buttonMouseClick);
            this.panelOuter.Controls.Add(this.labelBut);
            this.panelOuter.Controls.Add(this.labelDev);
            this.panelOuter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelOuter.Location = new System.Drawing.Point(0, 0);
            this.panelOuter.Name = "panelOuter";
            this.panelOuter.Size = new System.Drawing.Size(351, 227);
            this.panelOuter.TabIndex = 9;
            // 
            // InputMapDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(351, 227);
            this.Controls.Add(this.panelOuter);
            this.Name = "InputMapDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Input Select";
            this.panelPressRelease.ResumeLayout(false);
            this.panelPressRelease.PerformLayout();
            this.panelOuter.ResumeLayout(false);
            this.panelOuter.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelDev;
        private System.Windows.Forms.Label labelBut;
        private System.Windows.Forms.RadioButton radioButtonPressed;
        private System.Windows.Forms.RadioButton radioButtonRelease;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Panel panelPressRelease;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button buttonMouseClick;
        private System.Windows.Forms.Label labelDevice;
        private System.Windows.Forms.Label labelKeyboard;
        private System.Windows.Forms.Panel panelOuter;
    }
}