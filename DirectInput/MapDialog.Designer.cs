
namespace DirectInputDevices
{
    partial class MapDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxDevice = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.radioButtonPressed = new System.Windows.Forms.RadioButton();
            this.radioButtonRelease = new System.Windows.Forms.RadioButton();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(219, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Press a Joystick button, mouse button or Key";
            // 
            // textBoxDevice
            // 
            this.textBoxDevice.Location = new System.Drawing.Point(97, 53);
            this.textBoxDevice.Name = "textBoxDevice";
            this.textBoxDevice.ReadOnly = true;
            this.textBoxDevice.Size = new System.Drawing.Size(214, 20);
            this.textBoxDevice.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Device";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 91);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Button/Key";
            // 
            // textBoxName
            // 
            this.textBoxName.Location = new System.Drawing.Point(97, 88);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.ReadOnly = true;
            this.textBoxName.Size = new System.Drawing.Size(214, 20);
            this.textBoxName.TabIndex = 1;
            // 
            // radioButtonPressed
            // 
            this.radioButtonPressed.AutoSize = true;
            this.radioButtonPressed.Checked = true;
            this.radioButtonPressed.Location = new System.Drawing.Point(9, 12);
            this.radioButtonPressed.Name = "radioButtonPressed";
            this.radioButtonPressed.Size = new System.Drawing.Size(95, 17);
            this.radioButtonPressed.TabIndex = 2;
            this.radioButtonPressed.TabStop = true;
            this.radioButtonPressed.Text = "When Pressed";
            this.radioButtonPressed.UseVisualStyleBackColor = true;
            // 
            // radioButtonRelease
            // 
            this.radioButtonRelease.AutoSize = true;
            this.radioButtonRelease.Location = new System.Drawing.Point(116, 12);
            this.radioButtonRelease.Name = "radioButtonRelease";
            this.radioButtonRelease.Size = new System.Drawing.Size(102, 17);
            this.radioButtonRelease.TabIndex = 3;
            this.radioButtonRelease.Text = "When Released";
            this.radioButtonRelease.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(236, 170);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(146, 170);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 5;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.radioButtonRelease);
            this.panel1.Controls.Add(this.radioButtonPressed);
            this.panel1.Location = new System.Drawing.Point(3, 114);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(306, 37);
            this.panel1.TabIndex = 6;
            // 
            // MapDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(321, 210);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBoxName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxDevice);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "MapDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Input Select";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxDevice;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.RadioButton radioButtonPressed;
        private System.Windows.Forms.RadioButton radioButtonRelease;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Panel panel1;
    }
}