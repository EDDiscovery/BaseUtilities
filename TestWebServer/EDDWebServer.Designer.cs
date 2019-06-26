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
            this.changeSupercruise = new System.Windows.Forms.Button();
            this.shieldChange = new System.Windows.Forms.Button();
            this.nightVision = new System.Windows.Forms.Button();
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
            // changeSupercruise
            // 
            this.changeSupercruise.Location = new System.Drawing.Point(104, 12);
            this.changeSupercruise.Name = "changeSupercruise";
            this.changeSupercruise.Size = new System.Drawing.Size(75, 23);
            this.changeSupercruise.TabIndex = 1;
            this.changeSupercruise.Text = "Supercruise";
            this.changeSupercruise.UseVisualStyleBackColor = true;
            this.changeSupercruise.Click += new System.EventHandler(this.changeSupercruise_Click);
            // 
            // shieldChange
            // 
            this.shieldChange.Location = new System.Drawing.Point(185, 12);
            this.shieldChange.Name = "shieldChange";
            this.shieldChange.Size = new System.Drawing.Size(75, 23);
            this.shieldChange.TabIndex = 1;
            this.shieldChange.Text = "Shields";
            this.shieldChange.UseVisualStyleBackColor = true;
            this.shieldChange.Click += new System.EventHandler(this.shieldChange_Click);
            // 
            // nightVision
            // 
            this.nightVision.Location = new System.Drawing.Point(266, 13);
            this.nightVision.Name = "nightVision";
            this.nightVision.Size = new System.Drawing.Size(75, 23);
            this.nightVision.TabIndex = 1;
            this.nightVision.Text = "NightVision";
            this.nightVision.UseVisualStyleBackColor = true;
            this.nightVision.Click += new System.EventHandler(this.nightvision_Click);
            // 
            // EDDWebServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.nightVision);
            this.Controls.Add(this.shieldChange);
            this.Controls.Add(this.changeSupercruise);
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
        private System.Windows.Forms.Button changeSupercruise;
        private System.Windows.Forms.Button shieldChange;
        private System.Windows.Forms.Button nightVision;
    }
}

