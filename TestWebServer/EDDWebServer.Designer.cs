namespace TestWebServer
{
    partial class EDDWebServer
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
            this.serverLog = new System.Windows.Forms.RichTextBox();
            this.buttonPushRec = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // serverLog
            // 
            this.serverLog.Location = new System.Drawing.Point(12, 94);
            this.serverLog.Name = "serverLog";
            this.serverLog.Size = new System.Drawing.Size(765, 344);
            this.serverLog.TabIndex = 0;
            this.serverLog.Text = "";
            // 
            // buttonPushRec
            // 
            this.buttonPushRec.Location = new System.Drawing.Point(12, 13);
            this.buttonPushRec.Name = "buttonPushRec";
            this.buttonPushRec.Size = new System.Drawing.Size(75, 23);
            this.buttonPushRec.TabIndex = 1;
            this.buttonPushRec.Text = "JournalRec";
            this.buttonPushRec.UseVisualStyleBackColor = true;
            this.buttonPushRec.Click += new System.EventHandler(this.buttonPushRec_Click);
            // 
            // EDDWebServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.buttonPushRec);
            this.Controls.Add(this.serverLog);
            this.Name = "EDDWebServer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "EDD Web Server";
            this.Shown += new System.EventHandler(this.EDDWebServer_Shown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox serverLog;
        private System.Windows.Forms.Button buttonPushRec;
    }
}

