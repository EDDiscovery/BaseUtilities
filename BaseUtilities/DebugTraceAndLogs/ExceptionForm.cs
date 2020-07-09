/*
 * Copyright © 2016 - 2020 EDDiscovery development team
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed under
 * the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
 * ANY KIND, either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 * 
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */
using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BaseUtils
{
    public partial class ExceptionForm : Form
    {
        #region Public interfaces

        public static DialogResult ShowException(Exception ex, string desc, string reportIssueURL, bool isFatal = false, Form parent = null)
        {
            ExceptionForm f = new ExceptionForm(ex, desc, reportIssueURL, isFatal)
            {
                Owner = parent,
                StartPosition = parent != null ? FormStartPosition.CenterParent : FormStartPosition.WindowsDefaultLocation
            };

            DialogResult res = f.ShowDialog(parent);

            if (isFatal || res != DialogResult.Ignore)
            {
                Environment.Exit(1);
            }

            f.Dispose();
            return res;
        }

        #endregion


        #region Implementation

        private static string DoubleNewLine { get; } = Environment.NewLine + Environment.NewLine;

        private string description;
        private Exception exception;
        private bool isFatal;
        private string reportURL;

        private Image icon = null;
        private bool isExpanded = true;
        private int lastExpandedHeight = -1;

        private readonly int CollapsedHeight = -1;
        private readonly int MinimumHeightExpanded = -1;

        private ExceptionForm(Exception ex, string desc, string reportUrl, bool isFatal = false)
        {
            InitializeComponent();

            description = desc;
            exception = ex;
            this.isFatal = isFatal;
            reportURL = reportUrl;

            // Get some information from the designer.
            CollapsedHeight = Height - pnlDetails.Height - (pnlDetails.Top - pnlHeader.Bottom);
            MinimumHeightExpanded = CollapsedHeight + 80;   // 80 ≈ 4 lines of text.
        }


        // Sanitize keyboard-accelerated Control.Text values. "a&bc" = "abc", "&a &b && &c" = "a b & c", "abc&" = "abc&", etc.
        private string NoAmp(Control c)
        {
            if (!string.IsNullOrEmpty(c?.Text))
                return Regex.Replace(c.Text, "&(.?)", "$1");
            else
                return string.Empty;
        }

        // Show or hide the extended details panel, and resize the form to fit appropriately.
        private void ToggleDetails()
        {
            SuspendLayout();

            isExpanded = !isExpanded;
            btnDetails.Text = (isExpanded ? "▲" : "▼") + "  &Details";
            pnlDetails.Visible = isExpanded;

            if (isExpanded)
            {
                MinimumSize = new Size(MinimumSize.Width, MinimumHeightExpanded);
                MaximumSize = Size.Empty;
                Height = lastExpandedHeight;
            }
            else
            {
                lastExpandedHeight = Height;
                MaximumSize = new Size(int.MaxValue, CollapsedHeight);
                MinimumSize = new Size(MinimumSize.Width, CollapsedHeight);
            }

            ResumeLayout(true);
        }


        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            icon?.Dispose();
            description = null;
            exception = null;
            icon = null;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ToggleDetails();
            icon = SystemIcons.Error.ToBitmap();
            pnlIcon.BackgroundImage = icon;

            if (isFatal)
            {
                // Remove btnContinue
                pnlHeader.Controls.Remove(btnContinue);
                btnContinue.Dispose();

                // Center btnReport in the panel
                btnReport.Anchor &= ~AnchorStyles.Left;
                btnReport.Left = pnlHeader.ClientSize.Width / 2 - btnReport.Width / 2;
            }

            string appShortName = Assembly.GetEntryAssembly().GetName().Name;
            Text = BrowserInfo.UserAgent + (isFatal ? " Fatal" : string.Empty) + " Error";

            lblHeader.Text =
                  description + DoubleNewLine
                + (!string.IsNullOrEmpty(reportURL) ? $"Click the \"{NoAmp(btnReport)}\" button to copy diagnostic information to your clipboard, and provide this info to help us make {appShortName} better. Otherwise, click " : "Click ")
                + (isFatal ? $"{NoAmp(btnExit)} to close the program." : $"{NoAmp(btnContinue)} to try and ignore the error, or {NoAmp(btnExit)} to close the program.");
            textboxDetails.Text =
                  description + DoubleNewLine
                + "==== BEGIN ====" + Environment.NewLine
                + exception.ToString() + Environment.NewLine
                + "===== END =====";

            if (!string.IsNullOrWhiteSpace(reportURL))
            {
                // Tag the report button with markdown-formatted information about this exception.
                btnReport.Tag =
                      "### Description" + Environment.NewLine
                    + "<!-- Please write a brief description about what you were doing when the exception was encountered. -->" + DoubleNewLine
                    + "### Additional Information" + Environment.NewLine
                    + "<!-- Please attach (drag and drop) any relevant trace logs, screenshots, and/or journal files here to provide information about the problem. -->" + DoubleNewLine
                    + "### Exception Details:" + Environment.NewLine
                    + ">" + BrowserInfo.UserAgent + " " + description + Environment.NewLine
                    + ">```" + Environment.NewLine
                    + ">==== BEGIN ====" + Environment.NewLine
                    + ">" + exception.ToString().Replace(Environment.NewLine, Environment.NewLine + ">") + Environment.NewLine
                    + ">===== END =====" + Environment.NewLine
                    + ">```";
            }
            else
            {
                pnlHeader.Controls.Remove(btnReport);
                btnReport.Dispose();
            }
        }


        private void btnDetails_Click(object sender, EventArgs e)
        {
            ToggleDetails();
        }

        private void btnContinueOrExit_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Close();
                DialogResult = ((Button)sender).DialogResult;
            }
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            bool clipSet = false;
            Control ctl = sender as Control;

            if (ctl?.Tag != null)
            {
                try
                {
                    Clipboard.SetText(ctl.Tag as string);
                    clipSet = true;
                }
                catch { }
            }

            try
            {
                Process.Start(reportURL);
                if (clipSet)
                    MessageBox.Show(this, "Diagnostic information has been copied to your clipboard. Please include this in your issue submission.", string.Empty, MessageBoxButtons.OK);
            }
            catch { }
        }

        #endregion
    }
}
