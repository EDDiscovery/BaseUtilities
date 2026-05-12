namespace BaseUtils
{
    partial class FontDialog
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
            this.panelTop = new System.Windows.Forms.Panel();
            this.labelFontName = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.labelSample = new System.Windows.Forms.Label();
            this.comboBoxStyle = new System.Windows.Forms.ComboBox();
            this.comboBoxSize = new System.Windows.Forms.ComboBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelsyleprompt = new System.Windows.Forms.Label();
            this.labelFontsize = new System.Windows.Forms.Label();
            this.labelFontprompt = new System.Windows.Forms.Label();
            this.labelSampleName = new System.Windows.Forms.Label();
            this.panelFonts = new System.Windows.Forms.Panel();
            this.panelTop.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.labelFontName);
            this.panelTop.Controls.Add(this.panel1);
            this.panelTop.Controls.Add(this.comboBoxStyle);
            this.panelTop.Controls.Add(this.comboBoxSize);
            this.panelTop.Controls.Add(this.buttonCancel);
            this.panelTop.Controls.Add(this.buttonOK);
            this.panelTop.Controls.Add(this.labelsyleprompt);
            this.panelTop.Controls.Add(this.labelFontsize);
            this.panelTop.Controls.Add(this.labelFontprompt);
            this.panelTop.Controls.Add(this.labelSampleName);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(920, 132);
            this.panelTop.TabIndex = 0;
            // 
            // labelFontName
            // 
            this.labelFontName.AutoSize = true;
            this.labelFontName.Location = new System.Drawing.Point(64, 9);
            this.labelFontName.Name = "labelFontName";
            this.labelFontName.Size = new System.Drawing.Size(59, 13);
            this.labelFontName.TabIndex = 0;
            this.labelFontName.Text = "Font Name";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.labelSample);
            this.panel1.Location = new System.Drawing.Point(60, 36);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(762, 83);
            this.panel1.TabIndex = 3;
            // 
            // labelSample
            // 
            this.labelSample.AutoSize = true;
            this.labelSample.Location = new System.Drawing.Point(3, 10);
            this.labelSample.Name = "labelSample";
            this.labelSample.Size = new System.Drawing.Size(68, 13);
            this.labelSample.TabIndex = 0;
            this.labelSample.Text = "Current Font:";
            // 
            // comboBoxStyle
            // 
            this.comboBoxStyle.Enabled = false;
            this.comboBoxStyle.FormattingEnabled = true;
            this.comboBoxStyle.Location = new System.Drawing.Point(418, 6);
            this.comboBoxStyle.Name = "comboBoxStyle";
            this.comboBoxStyle.Size = new System.Drawing.Size(71, 21);
            this.comboBoxStyle.TabIndex = 2;
            // 
            // comboBoxSize
            // 
            this.comboBoxSize.Enabled = false;
            this.comboBoxSize.FormattingEnabled = true;
            this.comboBoxSize.Location = new System.Drawing.Point(272, 6);
            this.comboBoxSize.Name = "comboBoxSize";
            this.comboBoxSize.Size = new System.Drawing.Size(71, 21);
            this.comboBoxSize.TabIndex = 2;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(828, 42);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(828, 13);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // labelsyleprompt
            // 
            this.labelsyleprompt.AutoSize = true;
            this.labelsyleprompt.Location = new System.Drawing.Point(366, 9);
            this.labelsyleprompt.Name = "labelsyleprompt";
            this.labelsyleprompt.Size = new System.Drawing.Size(33, 13);
            this.labelsyleprompt.TabIndex = 0;
            this.labelsyleprompt.Text = "Style:";
            // 
            // labelFontsize
            // 
            this.labelFontsize.AutoSize = true;
            this.labelFontsize.Location = new System.Drawing.Point(235, 9);
            this.labelFontsize.Name = "labelFontsize";
            this.labelFontsize.Size = new System.Drawing.Size(30, 13);
            this.labelFontsize.TabIndex = 0;
            this.labelFontsize.Text = "Size:";
            // 
            // labelFontprompt
            // 
            this.labelFontprompt.AutoSize = true;
            this.labelFontprompt.Location = new System.Drawing.Point(28, 9);
            this.labelFontprompt.Name = "labelFontprompt";
            this.labelFontprompt.Size = new System.Drawing.Size(31, 13);
            this.labelFontprompt.TabIndex = 0;
            this.labelFontprompt.Text = "Font:";
            // 
            // labelSampleName
            // 
            this.labelSampleName.AutoSize = true;
            this.labelSampleName.Location = new System.Drawing.Point(14, 45);
            this.labelSampleName.Name = "labelSampleName";
            this.labelSampleName.Size = new System.Drawing.Size(45, 13);
            this.labelSampleName.TabIndex = 0;
            this.labelSampleName.Text = "Sample:";
            // 
            // panelFonts
            // 
            this.panelFonts.AutoScroll = true;
            this.panelFonts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelFonts.Location = new System.Drawing.Point(0, 132);
            this.panelFonts.Name = "panelFonts";
            this.panelFonts.Size = new System.Drawing.Size(920, 363);
            this.panelFonts.TabIndex = 1;
            // 
            // FontDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(920, 495);
            this.Controls.Add(this.panelFonts);
            this.Controls.Add(this.panelTop);
            this.Name = "FontDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FontForm";
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Panel panelFonts;
        private System.Windows.Forms.ComboBox comboBoxSize;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelFontprompt;
        private System.Windows.Forms.Label labelSample;
        private System.Windows.Forms.Label labelSampleName;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label labelFontName;
        private System.Windows.Forms.ComboBox comboBoxStyle;
        private System.Windows.Forms.Label labelFontsize;
        private System.Windows.Forms.Label labelsyleprompt;
    }
}