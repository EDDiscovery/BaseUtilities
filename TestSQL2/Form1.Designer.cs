
namespace TestSQL2
{
    partial class Form1
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
            this.buttonSimple = new System.Windows.Forms.Button();
            this.buttonThread = new System.Windows.Forms.Button();
            this.richTextBox = new System.Windows.Forms.RichTextBox();
            this.buttonTQuery1 = new System.Windows.Forms.Button();
            this.buttonST = new System.Windows.Forms.Button();
            this.buttonMT = new System.Windows.Forms.Button();
            this.buttonMQ1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonSimple
            // 
            this.buttonSimple.Location = new System.Drawing.Point(32, 24);
            this.buttonSimple.Name = "buttonSimple";
            this.buttonSimple.Size = new System.Drawing.Size(75, 23);
            this.buttonSimple.TabIndex = 0;
            this.buttonSimple.Text = "DirectQuery";
            this.buttonSimple.UseVisualStyleBackColor = true;
            this.buttonSimple.Click += new System.EventHandler(this.buttonSimple_Click);
            // 
            // buttonThread
            // 
            this.buttonThread.Location = new System.Drawing.Point(32, 76);
            this.buttonThread.Name = "buttonThread";
            this.buttonThread.Size = new System.Drawing.Size(75, 23);
            this.buttonThread.TabIndex = 1;
            this.buttonThread.Text = "Start Thread";
            this.buttonThread.UseVisualStyleBackColor = true;
            this.buttonThread.Click += new System.EventHandler(this.buttonThread_Click);
            // 
            // richTextBox
            // 
            this.richTextBox.Location = new System.Drawing.Point(187, 24);
            this.richTextBox.Name = "richTextBox";
            this.richTextBox.Size = new System.Drawing.Size(581, 397);
            this.richTextBox.TabIndex = 2;
            this.richTextBox.Text = "";
            // 
            // buttonTQuery1
            // 
            this.buttonTQuery1.Location = new System.Drawing.Point(32, 121);
            this.buttonTQuery1.Name = "buttonTQuery1";
            this.buttonTQuery1.Size = new System.Drawing.Size(75, 23);
            this.buttonTQuery1.TabIndex = 1;
            this.buttonTQuery1.Text = "T Q1";
            this.buttonTQuery1.UseVisualStyleBackColor = true;
            this.buttonTQuery1.Click += new System.EventHandler(this.buttonTQuery1_Click);
            // 
            // buttonST
            // 
            this.buttonST.Location = new System.Drawing.Point(32, 164);
            this.buttonST.Name = "buttonST";
            this.buttonST.Size = new System.Drawing.Size(75, 23);
            this.buttonST.TabIndex = 1;
            this.buttonST.Text = "Single Thread";
            this.buttonST.UseVisualStyleBackColor = true;
            this.buttonST.Click += new System.EventHandler(this.buttonST_Click);
            // 
            // buttonMT
            // 
            this.buttonMT.Location = new System.Drawing.Point(32, 208);
            this.buttonMT.Name = "buttonMT";
            this.buttonMT.Size = new System.Drawing.Size(75, 23);
            this.buttonMT.TabIndex = 3;
            this.buttonMT.Text = "Multi Thread";
            this.buttonMT.UseVisualStyleBackColor = true;
            this.buttonMT.Click += new System.EventHandler(this.buttonMT_Click);
            // 
            // buttonMQ1
            // 
            this.buttonMQ1.Location = new System.Drawing.Point(32, 257);
            this.buttonMQ1.Name = "buttonMQ1";
            this.buttonMQ1.Size = new System.Drawing.Size(75, 23);
            this.buttonMQ1.TabIndex = 4;
            this.buttonMQ1.Text = "Multi Query";
            this.buttonMQ1.UseVisualStyleBackColor = true;
            this.buttonMQ1.Click += new System.EventHandler(this.buttonMQ1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.buttonMQ1);
            this.Controls.Add(this.buttonMT);
            this.Controls.Add(this.richTextBox);
            this.Controls.Add(this.buttonST);
            this.Controls.Add(this.buttonTQuery1);
            this.Controls.Add(this.buttonThread);
            this.Controls.Add(this.buttonSimple);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonSimple;
        private System.Windows.Forms.Button buttonThread;
        private System.Windows.Forms.RichTextBox richTextBox;
        private System.Windows.Forms.Button buttonTQuery1;
        private System.Windows.Forms.Button buttonST;
        private System.Windows.Forms.Button buttonMT;
        private System.Windows.Forms.Button buttonMQ1;
    }
}

