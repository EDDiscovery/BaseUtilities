namespace TestOpenTk
{
    partial class ShaderTestStarPoints
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
            this.glControlContainer = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // glControlContainer
            // 
            this.glControlContainer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.glControlContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glControlContainer.Location = new System.Drawing.Point(0, 0);
            this.glControlContainer.Name = "glControlContainer";
            this.glControlContainer.Size = new System.Drawing.Size(1555, 1005);
            this.glControlContainer.TabIndex = 0;
            // 
            // ShaderTest4
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1555, 1005);
            this.Controls.Add(this.glControlContainer);
            this.Name = "ShaderTest4";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel glControlContainer;
    }
}

