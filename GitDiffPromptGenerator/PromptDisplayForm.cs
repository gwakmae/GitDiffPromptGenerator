using System;
using System.Windows.Forms;

namespace GitDiffPromptGenerator
{
    public partial class PromptDisplayForm : Form
    {
        public PromptDisplayForm(string promptText)
        {
            InitializeComponent();
            txtPromptContent.Text = promptText;

            // 폼이 로드될 때 텍스트를 전체 선택하고 포커스를 줍니다.
            this.Load += (sender, e) =>
            {
                txtPromptContent.SelectAll();
                txtPromptContent.Focus();
            };
        }
    }
}