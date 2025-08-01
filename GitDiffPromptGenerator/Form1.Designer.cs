// Form1.Designer.cs
namespace GitDiffPromptGenerator
{
    partial class Form1
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
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
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.btnGenerate = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.txtPrompt = new System.Windows.Forms.TextBox();
            this.btnSelectFolder = new System.Windows.Forms.Button();
            this.txtSelectedFolder = new System.Windows.Forms.TextBox();
            this.btnGenerateForNewFiles = new System.Windows.Forms.Button(); // 새 버튼 선언
            this.SuspendLayout();
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(12, 12);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(180, 29);
            this.btnGenerate.TabIndex = 0;
            this.btnGenerate.Text = "변경점 추출 및 프롬프트 생성";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // btnGenerateForNewFiles
            // 
            this.btnGenerateForNewFiles.Location = new System.Drawing.Point(198, 12);
            this.btnGenerateForNewFiles.Name = "btnGenerateForNewFiles";
            this.btnGenerateForNewFiles.Size = new System.Drawing.Size(180, 29);
            this.btnGenerateForNewFiles.TabIndex = 1;
            this.btnGenerateForNewFiles.Text = "초기 커밋 프롬프트 생성"; // <<< *** 이름 변경 *** >>>
            this.btnGenerateForNewFiles.UseVisualStyleBackColor = true;
            this.btnGenerateForNewFiles.Click += new System.EventHandler(this.btnGenerateForNewFiles_Click);
            // 
            // btnSelectFolder
            // 
            this.btnSelectFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectFolder.Location = new System.Drawing.Point(500, 12);
            this.btnSelectFolder.Name = "btnSelectFolder";
            this.btnSelectFolder.Size = new System.Drawing.Size(110, 29);
            this.btnSelectFolder.TabIndex = 2;
            this.btnSelectFolder.Text = "폴더 선택";
            this.btnSelectFolder.UseVisualStyleBackColor = true;
            this.btnSelectFolder.Click += new System.EventHandler(this.btnSelectFolder_Click);
            // 
            // txtSelectedFolder
            // 
            this.txtSelectedFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSelectedFolder.Location = new System.Drawing.Point(12, 47);
            this.txtSelectedFolder.Name = "txtSelectedFolder";
            this.txtSelectedFolder.ReadOnly = true;
            this.txtSelectedFolder.Size = new System.Drawing.Size(790, 23);
            this.txtSelectedFolder.TabIndex = 3;
            // 
            // txtPrompt
            // 
            this.txtPrompt.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPrompt.Location = new System.Drawing.Point(12, 76);
            this.txtPrompt.Multiline = true;
            this.txtPrompt.Name = "txtPrompt";
            this.txtPrompt.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtPrompt.Size = new System.Drawing.Size(790, 358);
            this.txtPrompt.TabIndex = 4;
            this.txtPrompt.WordWrap = false;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(616, 12);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(186, 29);
            this.btnSave.TabIndex = 5;
            this.btnSave.Text = "프롬프트 새 창으로 열기"; // <<<< *** 텍스트 변경 *** >>>>
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(814, 446);
            this.Controls.Add(this.btnGenerateForNewFiles);
            this.Controls.Add(this.txtSelectedFolder);
            this.Controls.Add(this.btnSelectFolder);
            this.Controls.Add(this.txtPrompt);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnGenerate);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(830, 300);
            this.Name = "Form1";
            this.Text = "Git 프롬프트 생성기";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.TextBox txtPrompt;
        private System.Windows.Forms.Button btnSelectFolder;
        private System.Windows.Forms.TextBox txtSelectedFolder;
        private System.Windows.Forms.Button btnGenerateForNewFiles; // 새 버튼 멤버 변수
    }
}