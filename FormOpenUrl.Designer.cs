namespace DougVIdeoPlayer
{
    partial class FormOpenUrl
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
            this.TUrl = new System.Windows.Forms.TextBox();
            this.BtnOpenURL = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "URL";
            // 
            // TUrl
            // 
            this.TUrl.Location = new System.Drawing.Point(47, 12);
            this.TUrl.Name = "TUrl";
            this.TUrl.Size = new System.Drawing.Size(612, 20);
            this.TUrl.TabIndex = 1;
            // 
            // BtnOpenURL
            // 
            this.BtnOpenURL.Location = new System.Drawing.Point(294, 38);
            this.BtnOpenURL.Name = "BtnOpenURL";
            this.BtnOpenURL.Size = new System.Drawing.Size(75, 23);
            this.BtnOpenURL.TabIndex = 2;
            this.BtnOpenURL.Text = "Open URL";
            this.BtnOpenURL.UseVisualStyleBackColor = true;
            this.BtnOpenURL.Click += new System.EventHandler(this.BtnOpenURL_Click);
            // 
            // FormOpenUrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(671, 70);
            this.Controls.Add(this.BtnOpenURL);
            this.Controls.Add(this.TUrl);
            this.Controls.Add(this.label1);
            this.Name = "FormOpenUrl";
            this.Text = "Open URL";
            this.Shown += new System.EventHandler(this.FormOpenUrl_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TUrl;
        private System.Windows.Forms.Button BtnOpenURL;
    }
}