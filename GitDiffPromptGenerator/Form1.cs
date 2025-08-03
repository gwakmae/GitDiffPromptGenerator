using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GitDiffPromptGenerator
{
    public partial class Form1 : Form
    {
        private string selectedFolder = "";
        private readonly HttpClient httpClient;

        public Form1()
        {
            InitializeComponent();
            httpClient = new HttpClient();
        }

        // --- ��ư 1: ������ ���� �� ������Ʈ ���� ---
        private async void btnGenerate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFolder) || !Directory.Exists(selectedFolder))
            {
                MessageBox.Show("���� Git ����� ������ �����ϼ���.");
                return;
            }

            if (!Directory.Exists(Path.Combine(selectedFolder, ".git")))
            {
                MessageBox.Show("�� ����� Git ����ҿ����� �����մϴ�.\n'�ʱ� Ŀ�� ������Ʈ ����'�� �̿��ϰų� �������� 'git init'�� ���� �������ּ���.", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // �ε� ǥ��
                this.Cursor = Cursors.WaitCursor;
                txtPrompt.Text = "Git ��������� �м��ϰ� ���� ���Դϴ�... ��� ��ٷ��ּ���.";
                Application.DoEvents();

                string diff = await GetGitDiffWithTranslation(selectedFolder);
                if (string.IsNullOrEmpty(diff))
                {
                    MessageBox.Show("���� ������ �����ϴ�.");
                    txtPrompt.Text = "";
                    return;
                }

                string prompt = GenerateDiffPrompt(diff);
                txtPrompt.Text = prompt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("������ �߻��߽��ϴ�: " + ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPrompt.Text = "";
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        // --- ��ư 2: �ʱ� Ŀ�� ������Ʈ ���� ---
        private void btnGenerateForNewFiles_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFolder) || !Directory.Exists(selectedFolder))
            {
                MessageBox.Show("���� ������Ʈ ������ �����ϼ���.");
                return;
            }

            try
            {
                string allCode = GetAllFilesContent(selectedFolder);
                if (string.IsNullOrEmpty(allCode))
                {
                    MessageBox.Show("�������� �м��� �� �ִ� �ڵ� ������ ã�� ���߽��ϴ�.");
                    return;
                }
                string prompt = GenerateInitialCommitPrompt(allCode);
                txtPrompt.Text = prompt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("������ �д� �� ������ �߻��߽��ϴ�: " + ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region HTTP ���� ȣ��� LibreTranslate ��� (���̺귯�� ���� �ذ�)

        /// <summary>
        /// HTTP ���� ȣ��� LibreTranslate API ��� - ���̺귯�� ������ ����
        /// </summary>
        private async Task<string> GetGitDiffWithTranslation(string workingDirectory)
        {
            try
            {
                // 1�ܰ�: Git�� ���� ȯ�濡�� ���� (���ڵ� ���� ȸ��)
                string rawDiff = RunGitCommandInEnglish(workingDirectory, "diff origin/main");

                if (string.IsNullOrWhiteSpace(rawDiff))
                {
                    // origin/main�� ������ status�� ��ü
                    rawDiff = RunGitCommandInEnglish(workingDirectory, "status --porcelain");
                    if (!string.IsNullOrWhiteSpace(rawDiff))
                    {
                        return await TranslateGitStatusWithHTTP(rawDiff, workingDirectory);
                    }
                    return "���� ������ �����ϴ�.";
                }

                // 2�ܰ�: HTTP�� LibreTranslate API ���� ȣ���Ͽ� ����
                return await TranslateGitDiffWithHTTP(rawDiff);
            }
            catch (Exception ex)
            {
                return $"Git ��ɾ� ���� �� ���� �߻�: {ex.Message}";
            }
        }

        /// <summary>
        /// ���� ȯ�濡�� Git ��ɾ� ���� (�ѱ� ���ڵ� ���� ���� ȸ��)
        /// </summary>
        private string RunGitCommandInEnglish(string workingDirectory, string arguments)
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
                WorkingDirectory = workingDirectory
            };

            // ������ ���� ȯ�� �������� �ѱ� ���� ��õ ����
            psi.Environment.Clear();
            psi.Environment["LANG"] = "C";
            psi.Environment["LC_ALL"] = "C";
            psi.Environment["PATH"] = Environment.GetEnvironmentVariable("PATH") ?? "";

            using var process = Process.Start(psi);
            if (process == null)
                throw new Exception("Git ���μ����� ������ �� �����ϴ�.");

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0 && !string.IsNullOrWhiteSpace(error))
                throw new Exception($"Git ����: {error}");

            return output;
        }

        /// <summary>
        /// HTTP ���� ȣ��� LibreTranslate API ����Ͽ� Git diff ����
        /// </summary>
        private async Task<string> TranslateGitDiffWithHTTP(string englishDiff)
        {
            var result = new StringBuilder();
            result.AppendLine("=== Git Diff ��� (LibreTranslate HTTP API ����) ===");
            result.AppendLine();

            var lines = englishDiff.Split('\n');

            foreach (var line in lines.Take(20)) // �ʹ� ������ ���� �ð��� ���� �ɸ�
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                string trimmedLine = line.Trim();

                // Git Ư�� ���ε��� ���� ����
                if (trimmedLine.StartsWith("diff --git"))
                {
                    result.AppendLine($"���� ��: {trimmedLine.Substring(11)}");
                }
                else if (trimmedLine.StartsWith("index "))
                {
                    result.AppendLine($"�ε���: {trimmedLine.Substring(6)}");
                }
                else if (trimmedLine.StartsWith("--- "))
                {
                    result.AppendLine($"���� ����: {trimmedLine.Substring(4)}");
                }
                else if (trimmedLine.StartsWith("+++ "))
                {
                    result.AppendLine($"�� ����: {trimmedLine.Substring(4)}");
                }
                else if (trimmedLine.StartsWith("@@"))
                {
                    result.AppendLine($"���� ��ġ: {trimmedLine}");
                }
                else if (ContainsEnglishText(trimmedLine))
                {
                    // ���� �ؽ�Ʈ�� ���Ե� ���θ� ����
                    try
                    {
                        string translatedText = await TranslateTextWithHTTP(trimmedLine);
                        result.AppendLine($"����: {trimmedLine}");
                        result.AppendLine($"����: {translatedText}");
                    }
                    catch (Exception ex)
                    {
                        result.AppendLine($"���� ����: {trimmedLine} (����: {ex.Message})");
                    }
                }
                else
                {
                    result.AppendLine(trimmedLine);
                }

                result.AppendLine();
            }

            return result.ToString();
        }

        /// <summary>
        /// HTTP�� LibreTranslate API ���� ȣ��
        /// </summary>
        private async Task<string> TranslateTextWithHTTP(string text)
        {
            try
            {
                var requestData = new
                {
                    q = text,
                    source = "en",
                    target = "ko",
                    format = "text"
                };

                string jsonContent = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // LibreTranslate ���� ���� ���
                var response = await httpClient.PostAsync("https://libretranslate.de/translate", content);

                if (response.IsSuccessStatusCode)
                {
                    string responseText = await response.Content.ReadAsStringAsync();
                    var responseObj = JsonSerializer.Deserialize<JsonElement>(responseText);

                    if (responseObj.TryGetProperty("translatedText", out JsonElement translatedElement))
                    {
                        return translatedElement.GetString() ?? text;
                    }
                }

                return text; // ���� ���� �� ���� ��ȯ
            }
            catch
            {
                return text; // ���� �� ���� ��ȯ
            }
        }

        /// <summary>
        /// Git status ����� HTTP API�� ����
        /// </summary>
        private async Task<string> TranslateGitStatusWithHTTP(string englishStatus, string workingDir)
        {
            var result = new StringBuilder();
            result.AppendLine("=== �۾� ���丮 ���� (HTTP API ����) ===");
            result.AppendLine();

            var statusLines = englishStatus.Split('\n')
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            foreach (var line in statusLines.Take(10)) // �ִ� 10�� ���ϸ�
            {
                if (line.Length < 3) continue;

                string status = line.Substring(0, 2);
                string filePath = line.Substring(3);

                // ���� ���¸� �ѱ۷� ����
                string translatedStatus = await TranslateFileStatusWithHTTP(status);
                result.AppendLine($"{translatedStatus}: {filePath}");

                // ���� ���뵵 �Ϻ� ǥ��
                try
                {
                    string fullPath = Path.Combine(workingDir, filePath);
                    if (File.Exists(fullPath) && IsTextFile(fullPath))
                    {
                        string content = File.ReadAllText(fullPath, Encoding.UTF8);
                        if (content.Length > 800)
                            content = content.Substring(0, 800) + "\n... (������ �� �Ϻθ� ǥ��)";
                        result.AppendLine("���� ����:");
                        result.AppendLine(content);
                    }
                }
                catch { }

                result.AppendLine(new string('-', 50));
            }

            return result.ToString();
        }

        private async Task<string> TranslateFileStatusWithHTTP(string status)
        {
            var statusDescriptions = new Dictionary<string, string>
            {
                {"M ", "Modified"},
                {" M", "Modified in working directory"},
                {"MM", "Staged and modified in working directory"},
                {"A ", "Added to index"},
                {" A", "Added to working directory"},
                {"D ", "Deleted from index"},
                {" D", "Deleted from working directory"},
                {"R ", "Renamed"},
                {"C ", "Copied"},
                {"??", "Untracked file"},
                {"!!", "Ignored file"}
            };

            if (statusDescriptions.TryGetValue(status, out string? englishDesc))
            {
                return await TranslateTextWithHTTP(englishDesc);
            }

            return $"�� �� ���� ����({status})";
        }

        private bool ContainsEnglishText(string text)
        {
            // ��� ���Ե� �ǹ��ִ� �ؽ�Ʈ���� Ȯ��
            return text.Length > 5 &&
                   text.Any(c => char.IsLetter(c)) &&
                   text.Any(c => c >= 'A' && c <= 'z') &&
                   !text.StartsWith("diff --git") &&
                   !text.StartsWith("index ") &&
                   !text.StartsWith("@@");
        }

        private bool IsTextFile(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            return new[] { ".cs", ".txt", ".js", ".html", ".css", ".xml", ".json", ".md", ".py", ".java", ".cpp", ".h" }.Contains(ext);
        }

        #endregion

        #region ���� �޼����

        private string GetAllFilesContent(string rootPath)
        {
            var contentBuilder = new StringBuilder();
            var ignoreDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "bin", "obj", ".vs", ".git", "__pycache__", ".venv", "venv", ".env", "env",
                ".pytest_cache", "node_modules", "build", "dist", "archive", "except", "resources", ".idea", ".vscode"
            };

            var allFiles = Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories);

            foreach (var file in allFiles)
            {
                string[] pathParts = file.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                bool isIgnored = pathParts.Any(part => ignoreDirs.Contains(part) || part.StartsWith("."));
                if (isIgnored) continue;

                string ext = Path.GetExtension(file).ToLowerInvariant();
                var excludeExtensions = new[]
                {
                    ".exe", ".dll", ".pdb", ".ico", ".png", ".jpg", ".jpeg", ".gif", ".bmp",
                    ".zip", ".rar", ".7z", ".tar", ".gz", ".suo", ".user", ".cache", ".pyc", ".pyo"
                };
                if (excludeExtensions.Contains(ext)) continue;

                try
                {
                    string relativePath = Path.GetRelativePath(rootPath, file);
                    contentBuilder.AppendLine("============================================================");
                    contentBuilder.AppendLine($"File: {relativePath}");
                    contentBuilder.AppendLine("============================================================");
                    contentBuilder.AppendLine(File.ReadAllText(file, Encoding.UTF8));
                    contentBuilder.AppendLine();
                }
                catch (IOException) { /* ���� ��� ���̸� �ǳʶ� */ }
                catch (UnauthorizedAccessException) { /* ���� ���� ������ �ǳʶ� */ }
            }
            return contentBuilder.ToString();
        }

        private string GenerateDiffPrompt(string diff)
        {
            return
        $@"�Ʒ��� 'git diff' ����Դϴ�. �� ���� ���׿� ���� Ŀ�� �޽����� ���� ��Ģ�� ���� ����� �ۼ��� �ּ���.

1.  **���� (Subject):**
    *   ���� ������ ����ϴ� �� ���� ������ �ۼ��մϴ�.
    *   **�ݵ�� 50�� �̳�**�� �ۼ��ؾ� �մϴ�.
    *   ��������� �ۼ��մϴ� (��: 'Fix' not 'Fixed', 'Add' not 'Added').
    *   ���� ���� ��ħǥ�� ���� ������.

2.  **���� (Body):**
    *   ���� �Ʒ��� �� ���� ���ϴ�.
    *   ������, �� �����ߴ��� **�ڼ��� ����**�մϴ�. �� ���� �ƴ� **���� �ٷ� �ۼ�**�ص� �����ϴ�.
    *   ��� �����ߴ����� �ڵ� diff�� �����Ƿ�, �׺��ٴ� **������ ������ �ƶ�**�� �������ּ���.

3.  **��� ����:**
    *   ������ ����� ������ ���ļ�, �Ʒ� ����ó�� �ٷ� ����� �� �ִ� 'git commit -m ""...""' ��ɾ� �������� ������ּ���.

[Ŀ�� �޽��� ����]
git commit -m ""feat: Implement user authentication endpoint

- Add user registration and login logic.
- Use JWT for token-based authentication.
- Include basic validation for user input.""

���� �Ʒ� diff ������ �м��Ͽ� Ŀ�� �޽����� ������ �ּ���.

diff ����:
------------------------
{diff}
";
        }

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
