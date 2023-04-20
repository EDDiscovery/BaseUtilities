namespace TestSQL
{
    partial class TestSQLForm
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
            this.buttonLoadEDSM = new System.Windows.Forms.Button();
            this.buttonTestEDSM = new System.Windows.Forms.Button();
            this.buttonReloadSpansh = new System.Windows.Forms.Button();
            this.buttonTestSpansh = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // buttonLoadEDSM
            // 
            this.buttonLoadEDSM.Location = new System.Drawing.Point(27, 13);
            this.buttonLoadEDSM.Name = "buttonLoadEDSM";
            this.buttonLoadEDSM.Size = new System.Drawing.Size(113, 23);
            this.buttonLoadEDSM.TabIndex = 0;
            this.buttonLoadEDSM.Text = "Reload EDSM";
            this.buttonLoadEDSM.UseVisualStyleBackColor = true;
            this.buttonLoadEDSM.Click += new System.EventHandler(this.buttonLoadEDSM_Click);
            // 
            // buttonTestEDSM
            // 
            this.buttonTestEDSM.Location = new System.Drawing.Point(172, 12);
            this.buttonTestEDSM.Name = "buttonTestEDSM";
            this.buttonTestEDSM.Size = new System.Drawing.Size(75, 23);
            this.buttonTestEDSM.TabIndex = 1;
            this.buttonTestEDSM.Text = "Test EDSM";
            this.buttonTestEDSM.UseVisualStyleBackColor = true;
            this.buttonTestEDSM.Click += new System.EventHandler(this.buttonTestEDSM_Click);
            // 
            // buttonReloadSpansh
            // 
            this.buttonReloadSpansh.Location = new System.Drawing.Point(27, 69);
            this.buttonReloadSpansh.Name = "buttonReloadSpansh";
            this.buttonReloadSpansh.Size = new System.Drawing.Size(113, 23);
            this.buttonReloadSpansh.TabIndex = 2;
            this.buttonReloadSpansh.Text = "Reload Spansh";
            this.buttonReloadSpansh.UseVisualStyleBackColor = true;
            this.buttonReloadSpansh.Click += new System.EventHandler(this.buttonReloadSpansh_Click);
            // 
            // buttonTestSpansh
            // 
            this.buttonTestSpansh.Location = new System.Drawing.Point(172, 69);
            this.buttonTestSpansh.Name = "buttonTestSpansh";
            this.buttonTestSpansh.Size = new System.Drawing.Size(75, 23);
            this.buttonTestSpansh.TabIndex = 3;
            this.buttonTestSpansh.Text = "Test Spansh";
            this.buttonTestSpansh.UseVisualStyleBackColor = true;
            this.buttonTestSpansh.Click += new System.EventHandler(this.buttonTestSpansh_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(13, 125);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(751, 300);
            this.richTextBox1.TabIndex = 4;
            this.richTextBox1.Text = "";
            // 
            // TestSQLForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.buttonTestSpansh);
            this.Controls.Add(this.buttonReloadSpansh);
            this.Controls.Add(this.buttonTestEDSM);
            this.Controls.Add(this.buttonLoadEDSM);
            this.Name = "TestSQLForm";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonLoadEDSM;
        private System.Windows.Forms.Button buttonTestEDSM;
        private System.Windows.Forms.Button buttonReloadSpansh;
        private System.Windows.Forms.Button buttonTestSpansh;
        private System.Windows.Forms.RichTextBox richTextBox1;
    }
}

