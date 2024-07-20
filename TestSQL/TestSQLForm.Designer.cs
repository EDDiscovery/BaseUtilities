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
            this.buttonTestEDSM = new System.Windows.Forms.Button();
            this.buttonTestSpansh = new System.Windows.Forms.Button();
            this.richTextBox = new System.Windows.Forms.RichTextBox();
            this.buttonClearDB = new System.Windows.Forms.Button();
            this.buttonCheckMadeSpanshStars = new System.Windows.Forms.Button();
            this.buttonCheckEDSMMadeStars = new System.Windows.Forms.Button();
            this.buttonMakeSpanshL3 = new System.Windows.Forms.Button();
            this.buttonMakeEDSML3 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonTestEDSM
            // 
            this.buttonTestEDSM.Location = new System.Drawing.Point(12, 70);
            this.buttonTestEDSM.Name = "buttonTestEDSM";
            this.buttonTestEDSM.Size = new System.Drawing.Size(75, 23);
            this.buttonTestEDSM.TabIndex = 1;
            this.buttonTestEDSM.Text = "Test EDSM";
            this.buttonTestEDSM.UseVisualStyleBackColor = true;
            this.buttonTestEDSM.Click += new System.EventHandler(this.buttonTestEDSM_Click);
            // 
            // buttonTestSpansh
            // 
            this.buttonTestSpansh.Location = new System.Drawing.Point(624, 41);
            this.buttonTestSpansh.Name = "buttonTestSpansh";
            this.buttonTestSpansh.Size = new System.Drawing.Size(75, 23);
            this.buttonTestSpansh.TabIndex = 3;
            this.buttonTestSpansh.Text = "Test Spansh";
            this.buttonTestSpansh.UseVisualStyleBackColor = true;
            this.buttonTestSpansh.Click += new System.EventHandler(this.buttonTestSpansh_Click);
            // 
            // richTextBox
            // 
            this.richTextBox.Location = new System.Drawing.Point(12, 209);
            this.richTextBox.Name = "richTextBox";
            this.richTextBox.Size = new System.Drawing.Size(751, 358);
            this.richTextBox.TabIndex = 4;
            this.richTextBox.Text = "";
            // 
            // buttonClearDB
            // 
            this.buttonClearDB.Location = new System.Drawing.Point(12, 12);
            this.buttonClearDB.Name = "buttonClearDB";
            this.buttonClearDB.Size = new System.Drawing.Size(77, 23);
            this.buttonClearDB.TabIndex = 2;
            this.buttonClearDB.Text = "ClearDB";
            this.buttonClearDB.UseVisualStyleBackColor = true;
            this.buttonClearDB.Click += new System.EventHandler(this.buttonClearDB_Click);
            // 
            // buttonCheckMadeSpanshStars
            // 
            this.buttonCheckMadeSpanshStars.Location = new System.Drawing.Point(624, 70);
            this.buttonCheckMadeSpanshStars.Name = "buttonCheckMadeSpanshStars";
            this.buttonCheckMadeSpanshStars.Size = new System.Drawing.Size(148, 23);
            this.buttonCheckMadeSpanshStars.TabIndex = 3;
            this.buttonCheckMadeSpanshStars.Text = "Check Spansh Made Stars";
            this.buttonCheckMadeSpanshStars.UseVisualStyleBackColor = true;
            this.buttonCheckMadeSpanshStars.Click += new System.EventHandler(this.buttonCheckMadeSpanshStars_Click);
            // 
            // buttonCheckEDSMMadeStars
            // 
            this.buttonCheckEDSMMadeStars.Location = new System.Drawing.Point(12, 99);
            this.buttonCheckEDSMMadeStars.Name = "buttonCheckEDSMMadeStars";
            this.buttonCheckEDSMMadeStars.Size = new System.Drawing.Size(172, 23);
            this.buttonCheckEDSMMadeStars.TabIndex = 3;
            this.buttonCheckEDSMMadeStars.Text = "Check EDSM Made Stars";
            this.buttonCheckEDSMMadeStars.UseVisualStyleBackColor = true;
            this.buttonCheckEDSMMadeStars.Click += new System.EventHandler(this.buttonCheckEDSMMadeStars_Click);
            // 
            // buttonMakeSpanshL3
            // 
            this.buttonMakeSpanshL3.Location = new System.Drawing.Point(355, 41);
            this.buttonMakeSpanshL3.Name = "buttonMakeSpanshL3";
            this.buttonMakeSpanshL3.Size = new System.Drawing.Size(113, 23);
            this.buttonMakeSpanshL3.TabIndex = 2;
            this.buttonMakeSpanshL3.Text = "Make Spansh L3";
            this.buttonMakeSpanshL3.UseVisualStyleBackColor = true;
            this.buttonMakeSpanshL3.Click += new System.EventHandler(this.buttonMakeSpanshL3_Click);
            // 
            // buttonMakeEDSML3
            // 
            this.buttonMakeEDSML3.Location = new System.Drawing.Point(12, 41);
            this.buttonMakeEDSML3.Name = "buttonMakeEDSML3";
            this.buttonMakeEDSML3.Size = new System.Drawing.Size(113, 23);
            this.buttonMakeEDSML3.TabIndex = 0;
            this.buttonMakeEDSML3.Text = "Make EDSM L3";
            this.buttonMakeEDSML3.UseVisualStyleBackColor = true;
            this.buttonMakeEDSML3.Click += new System.EventHandler(this.buttonMakeEDSML3_Click);
            // 
            // TestSQLForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 579);
            this.Controls.Add(this.richTextBox);
            this.Controls.Add(this.buttonCheckEDSMMadeStars);
            this.Controls.Add(this.buttonCheckMadeSpanshStars);
            this.Controls.Add(this.buttonTestSpansh);
            this.Controls.Add(this.buttonClearDB);
            this.Controls.Add(this.buttonMakeSpanshL3);
            this.Controls.Add(this.buttonTestEDSM);
            this.Controls.Add(this.buttonMakeEDSML3);
            this.Name = "TestSQLForm";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button buttonTestEDSM;
        private System.Windows.Forms.Button buttonTestSpansh;
        private System.Windows.Forms.RichTextBox richTextBox;
        private System.Windows.Forms.Button buttonClearDB;
        private System.Windows.Forms.Button buttonCheckMadeSpanshStars;
        private System.Windows.Forms.Button buttonCheckEDSMMadeStars;
        private System.Windows.Forms.Button buttonMakeSpanshL3;
        private System.Windows.Forms.Button buttonMakeEDSML3;
    }
}

