
namespace TestSQL2
{
    partial class MainForm
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
            this.richTextBox = new System.Windows.Forms.RichTextBox();
            this.buttonOpen = new System.Windows.Forms.Button();
            this.buttonCreate = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.buttonFill1 = new System.Windows.Forms.Button();
            this.buttonFill2 = new System.Windows.Forms.Button();
            this.buttonRepeatFill = new System.Windows.Forms.Button();
            this.buttonRead = new System.Windows.Forms.Button();
            this.buttonReadRepeat = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // richTextBox
            // 
            this.richTextBox.Location = new System.Drawing.Point(187, 24);
            this.richTextBox.Name = "richTextBox";
            this.richTextBox.Size = new System.Drawing.Size(581, 397);
            this.richTextBox.TabIndex = 2;
            this.richTextBox.Text = "";
            // 
            // buttonOpen
            // 
            this.buttonOpen.Location = new System.Drawing.Point(12, 55);
            this.buttonOpen.Name = "buttonOpen";
            this.buttonOpen.Size = new System.Drawing.Size(75, 23);
            this.buttonOpen.TabIndex = 3;
            this.buttonOpen.Text = "Open";
            this.buttonOpen.UseVisualStyleBackColor = true;
            this.buttonOpen.Click += new System.EventHandler(this.buttonOpen_Click);
            // 
            // buttonCreate
            // 
            this.buttonCreate.Location = new System.Drawing.Point(12, 22);
            this.buttonCreate.Name = "buttonCreate";
            this.buttonCreate.Size = new System.Drawing.Size(75, 23);
            this.buttonCreate.TabIndex = 3;
            this.buttonCreate.Text = "Create";
            this.buttonCreate.UseVisualStyleBackColor = true;
            this.buttonCreate.Click += new System.EventHandler(this.buttonCreate_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(93, 55);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 3;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // buttonFill1
            // 
            this.buttonFill1.Location = new System.Drawing.Point(12, 123);
            this.buttonFill1.Name = "buttonFill1";
            this.buttonFill1.Size = new System.Drawing.Size(75, 23);
            this.buttonFill1.TabIndex = 3;
            this.buttonFill1.Text = "Fill1";
            this.buttonFill1.UseVisualStyleBackColor = true;
            this.buttonFill1.Click += new System.EventHandler(this.buttonFill1_Click);
            // 
            // buttonFill2
            // 
            this.buttonFill2.Location = new System.Drawing.Point(12, 152);
            this.buttonFill2.Name = "buttonFill2";
            this.buttonFill2.Size = new System.Drawing.Size(75, 23);
            this.buttonFill2.TabIndex = 3;
            this.buttonFill2.Text = "Fill2";
            this.buttonFill2.UseVisualStyleBackColor = true;
            this.buttonFill2.Click += new System.EventHandler(this.buttonFill2_Click);
            // 
            // buttonRepeatFill
            // 
            this.buttonRepeatFill.Location = new System.Drawing.Point(93, 152);
            this.buttonRepeatFill.Name = "buttonRepeatFill";
            this.buttonRepeatFill.Size = new System.Drawing.Size(75, 23);
            this.buttonRepeatFill.TabIndex = 3;
            this.buttonRepeatFill.Text = "Repeat Fill";
            this.buttonRepeatFill.UseVisualStyleBackColor = true;
            this.buttonRepeatFill.Click += new System.EventHandler(this.buttonRepeatFill_Click);
            // 
            // buttonRead
            // 
            this.buttonRead.Location = new System.Drawing.Point(12, 210);
            this.buttonRead.Name = "buttonRead";
            this.buttonRead.Size = new System.Drawing.Size(75, 23);
            this.buttonRead.TabIndex = 3;
            this.buttonRead.Text = "Read";
            this.buttonRead.UseVisualStyleBackColor = true;
            this.buttonRead.Click += new System.EventHandler(this.buttonRead_Click);
            // 
            // buttonReadRepeat
            // 
            this.buttonReadRepeat.Location = new System.Drawing.Point(93, 210);
            this.buttonReadRepeat.Name = "buttonReadRepeat";
            this.buttonReadRepeat.Size = new System.Drawing.Size(75, 23);
            this.buttonReadRepeat.TabIndex = 3;
            this.buttonReadRepeat.Text = "Repeat Read";
            this.buttonReadRepeat.UseVisualStyleBackColor = true;
            this.buttonReadRepeat.Click += new System.EventHandler(this.buttonReadRepeat_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.buttonCreate);
            this.Controls.Add(this.buttonReadRepeat);
            this.Controls.Add(this.buttonRead);
            this.Controls.Add(this.buttonRepeatFill);
            this.Controls.Add(this.buttonFill2);
            this.Controls.Add(this.buttonFill1);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.buttonOpen);
            this.Controls.Add(this.richTextBox);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.RichTextBox richTextBox;
        private System.Windows.Forms.Button buttonOpen;
        private System.Windows.Forms.Button buttonCreate;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Button buttonFill1;
        private System.Windows.Forms.Button buttonFill2;
        private System.Windows.Forms.Button buttonRepeatFill;
        private System.Windows.Forms.Button buttonRead;
        private System.Windows.Forms.Button buttonReadRepeat;
    }
}

