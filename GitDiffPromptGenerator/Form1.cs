using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitDiffPromptGenerator
{
    public partial class Form1 : Form
    {
        private string selectedFolder = "";

        public Form1()
        {
            InitializeComponent();
        }

        // --- ��ư 1: ���� ��� (������ ��) ---
        private void btnGenerate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFolder) || !Directory.Exists(selectedFolder))
            {
                MessageBox.Show("���� Git ����� ������ �����ϼ���.");
                return;
            }

            // �� ����� �ݵ�� Git ����ҿ����� �����ؾ� �մϴ�.
            if (!Directory.Exists(Path.Combine(selectedFolder, ".git")))
            {
                MessageBox.Show("�� ����� Git ����ҿ����� �����մϴ�.\n'�ʱ� Ŀ�� ������Ʈ ����'�� �̿��ϰų� �������� 'git init'�� ���� �������ּ���.", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                string diff = GetGitDiff(selectedFolder);
                if (string.IsNullOrEmpty(diff))
                {
                    MessageBox.Show("���� �����(origin/main) ��� ���� ������ �����ϴ�.");
                    return;
                }
                string prompt = GenerateDiffPrompt(diff);
                txtPrompt.Text = prompt;
            }
            catch (Exception ex)
            {
                // ���� �귣ġ�� ���� �� �߻��ϴ� ���� ���� �����Ͽ� ó��
                MessageBox.Show("������ �߻��߽��ϴ�.\n- ���� �����(origin)�� main �귣ġ�� �����ϴ��� Ȯ�����ּ���.\n- " + ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- ��ư 2: �� ��� (�ʱ� Ŀ�Կ�) ---
        private void btnGenerateForNewFiles_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFolder) || !Directory.Exists(selectedFolder))
            {
                MessageBox.Show("���� ������Ʈ ������ �����ϼ���.");
                return;
            }

            try
            {
                // Git�� �����ϰ� ���� �� ��� �ڵ� ������ �н��ϴ�.
                string allCode = GetAllFilesContent(selectedFolder);
                if (string.IsNullOrEmpty(allCode))
                {
                    MessageBox.Show("�������� �м��� �� �ִ� �ڵ� ������ ã�� ���߽��ϴ�.");
                    return;
                }
                // �ʱ� Ŀ�Կ� ������Ʈ�� �����մϴ�.
                string prompt = GenerateInitialCommitPrompt(allCode);
                txtPrompt.Text = prompt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("������ �д� �� ������ �߻��߽��ϴ�: " + ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Helper Methods

        /// <summary>
        /// ������ ������ ���� ������ ��� �ؽ�Ʈ ���� ������ �о� �ϳ��� ���ڿ��� ��Ĩ�ϴ�.
        /// bin, obj, .git �� ���ʿ��� ������ ���̳ʸ� ������ �����մϴ�.
        /// </summary>
        private string GetAllFilesContent(string rootPath)
        {
            var contentBuilder = new StringBuilder();
            var ignoreDirs = new HashSet<string> { "bin", "obj", ".vs", ".git" };

            var allFiles = Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories);

            foreach (var file in allFiles)
            {
                bool isIgnored = ignoreDirs.Any(dir =>
                    file.Split(Path.DirectorySeparatorChar).Contains(dir, StringComparer.OrdinalIgnoreCase)
                );
                if (isIgnored) continue;

                string ext = Path.GetExtension(file).ToLowerInvariant();
                if (new[] { ".exe", ".dll", ".pdb", ".ico", ".png", ".jpg", ".zip", ".suo" }.Contains(ext)) continue;

                try
                {
                    string relativePath = Path.GetRelativePath(rootPath, file);
                    contentBuilder.AppendLine($"--- ����: {relativePath} ---");
                    contentBuilder.AppendLine(File.ReadAllText(file, Encoding.UTF8));
                    contentBuilder.AppendLine("----------------------------------------\n");
                }
                catch (IOException) { /* ������ ��� ���̸� �ǳʶ� */ }
            }

            return contentBuilder.ToString();
        }

        private string GetGitDiff(string workingDirectory) => RunGitCommand(workingDirectory, "diff origin/main");

        private string RunGitCommand(string workingDirectory, string arguments)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                WorkingDirectory = workingDirectory
            };

            using (var process = Process.Start(psi))
            {
                if (process == null) throw new Exception("���μ����� ������ �� �����ϴ�.");
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                if (process.ExitCode != 0 && !string.IsNullOrWhiteSpace(error))
                {
                    throw new Exception(error);
                }
                return output;
            }
        }

        #endregion

        #region Prompt Generators

        /// <summary>
        /// Git Diff ����� �������� ������Ʈ�� �����մϴ�.
        /// </summary>
        private string GenerateDiffPrompt(string diff)
        {
            return
        $@"�Ʒ��� �� ���� ����ҿ� ���� �����(main) ������ git diff ����Դϴ�.

�� ��������� ����ؼ� ����� �����ϰ� ��������.
�׸��� �Ʒ� ���Ĵ�� �ٷ� �� �� �ִ� Ŀ�� ��ɾ� �� ���� �ϼ��ؼ� �������. 
(����: git commit -m ""fix: Correct calculation logic"")

diff ����:
------------------------
{diff}
";
        }

        /// <summary>
        /// ������Ʈ ��ü �ڵ带 ������� �ʱ� Ŀ�� ������Ʈ�� �����մϴ�.
        /// </summary>
        private string GenerateInitialCommitPrompt(string allCode)
        {
            return
        $@"�Ʒ��� ������Ʈ�� ��ü �ҽ� �ڵ��Դϴ�.

�� ������Ʈ�� � ����� �ϴ��� �м��ϰ�, ������Ʈ�� ù Ŀ��(Initial Commit)�� ��︮�� Ŀ�� �޽����� �������ּ���.

- ���� ������Ʈ�� ���� ������ ������ �ѱ۷� �ۼ��մϴ�.
- �׸��� �Ʒ� ���Ŀ� ���� �ٷ� ����� �� �ִ� Ŀ�� ��ɾ ����� �����մϴ�.

(Ŀ�� ��ɾ� ����:
git add .
git commit -m ""feat: Initial commit for GitDiffPromptGenerator project"")

��ü �ڵ� ����:
------------------------
{allCode}
";
        }

        #endregion

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "������Ʈ ��Ʈ ������ �����ϼ���.";
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    selectedFolder = fbd.SelectedPath;
                    txtSelectedFolder.Text = selectedFolder;
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtPrompt.Text))
            {
                MessageBox.Show("�� â���� �� ������ �����ϴ�.");
                return;
            }
            var promptForm = new PromptDisplayForm(txtPrompt.Text);
            promptForm.Show();
        }
    }
}