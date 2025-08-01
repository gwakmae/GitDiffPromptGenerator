namespace GitDiffPromptGenerator
{
    partial class PromptDisplayForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtPromptContent;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.txtPromptContent = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txtPromptContent
            // 
            this.txtPromptContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtPromptContent.Location = new System.Drawing.Point(0, 0);
            this.txtPromptContent.Margin = new System.Windows.Forms.Padding(10); // 안쪽 여백
            this.txtPromptContent.Multiline = true;
            this.txtPromptContent.Name = "txtPromptContent";
            this.txtPromptContent.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtPromptContent.Size = new System.Drawing.Size(684, 411);
            this.txtPromptContent.TabIndex = 0;
            this.txtPromptContent.WordWrap = false;
            // 
            // PromptDisplayForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 411);
            this.Controls.Add(this.txtPromptContent);
            this.Name = "PromptDisplayForm";
            this.Text = "생성된 프롬프트 (Ctrl+C로 복사)";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}