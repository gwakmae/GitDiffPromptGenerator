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

        // --- 버튼 1: 변경점 추출 및 프롬프트 생성 ---
        private async void btnGenerate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFolder) || !Directory.Exists(selectedFolder))
            {
                MessageBox.Show("먼저 Git 저장소 폴더를 선택하세요.");
                return;
            }

            if (!Directory.Exists(Path.Combine(selectedFolder, ".git")))
            {
                MessageBox.Show("이 기능은 Git 저장소에서만 동작합니다.\n'초기 커밋 프롬프트 생성'을 이용하거나 폴더에서 'git init'을 먼저 실행해주세요.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // 로딩 표시
                this.Cursor = Cursors.WaitCursor;
                txtPrompt.Text = "Git 변경사항을 분석하고 번역 중입니다... 잠깐만 기다려주세요.";
                Application.DoEvents();

                string diff = await GetGitDiffWithTranslation(selectedFolder);
                if (string.IsNullOrEmpty(diff))
                {
                    MessageBox.Show("변경 사항이 없습니다.");
                    txtPrompt.Text = "";
                    return;
                }

                string prompt = GenerateDiffPrompt(diff);
                txtPrompt.Text = prompt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류가 발생했습니다: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPrompt.Text = "";
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        // --- 버튼 2: 초기 커밋 프롬프트 생성 ---
        private void btnGenerateForNewFiles_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFolder) || !Directory.Exists(selectedFolder))
            {
                MessageBox.Show("먼저 프로젝트 폴더를 선택하세요.");
                return;
            }

            try
            {
                string allCode = GetAllFilesContent(selectedFolder);
                if (string.IsNullOrEmpty(allCode))
                {
                    MessageBox.Show("폴더에서 분석할 수 있는 코드 파일을 찾지 못했습니다.");
                    return;
                }
                string prompt = GenerateInitialCommitPrompt(allCode);
                txtPrompt.Text = prompt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("파일을 읽는 중 오류가 발생했습니다: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region HTTP 직접 호출로 LibreTranslate 사용 (라이브러리 문제 해결)

        /// <summary>
        /// HTTP 직접 호출로 LibreTranslate API 사용 - 라이브러리 의존성 제거
        /// </summary>
        private async Task<string> GetGitDiffWithTranslation(string workingDirectory)
        {
            try
            {
                // 1단계: Git을 영어 환경에서 실행 (인코딩 문제 회피)
                string rawDiff = RunGitCommandInEnglish(workingDirectory, "diff origin/main");

                if (string.IsNullOrWhiteSpace(rawDiff))
                {
                    // origin/main이 없으면 status로 대체
                    rawDiff = RunGitCommandInEnglish(workingDirectory, "status --porcelain");
                    if (!string.IsNullOrWhiteSpace(rawDiff))
                    {
                        return await TranslateGitStatusWithHTTP(rawDiff, workingDirectory);
                    }
                    return "변경 사항이 없습니다.";
                }

                // 2단계: HTTP로 LibreTranslate API 직접 호출하여 번역
                return await TranslateGitDiffWithHTTP(rawDiff);
            }
            catch (Exception ex)
            {
                return $"Git 명령어 실행 중 오류 발생: {ex.Message}";
            }
        }

        /// <summary>
        /// 영어 환경에서 Git 명령어 실행 (한글 인코딩 문제 완전 회피)
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

            // 완전한 영어 환경 설정으로 한글 문제 원천 차단
            psi.Environment.Clear();
            psi.Environment["LANG"] = "C";
            psi.Environment["LC_ALL"] = "C";
            psi.Environment["PATH"] = Environment.GetEnvironmentVariable("PATH") ?? "";

            using var process = Process.Start(psi);
            if (process == null)
                throw new Exception("Git 프로세스를 시작할 수 없습니다.");

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0 && !string.IsNullOrWhiteSpace(error))
                throw new Exception($"Git 오류: {error}");

            return output;
        }

        /// <summary>
        /// HTTP 직접 호출로 LibreTranslate API 사용하여 Git diff 번역
        /// </summary>
        private async Task<string> TranslateGitDiffWithHTTP(string englishDiff)
        {
            var result = new StringBuilder();
            result.AppendLine("=== Git Diff 결과 (LibreTranslate HTTP API 번역) ===");
            result.AppendLine();

            var lines = englishDiff.Split('\n');

            foreach (var line in lines.Take(20)) // 너무 많으면 번역 시간이 오래 걸림
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                string trimmedLine = line.Trim();

                // Git 특수 라인들은 직접 번역
                if (trimmedLine.StartsWith("diff --git"))
                {
                    result.AppendLine($"파일 비교: {trimmedLine.Substring(11)}");
                }
                else if (trimmedLine.StartsWith("index "))
                {
                    result.AppendLine($"인덱스: {trimmedLine.Substring(6)}");
                }
                else if (trimmedLine.StartsWith("--- "))
                {
                    result.AppendLine($"이전 파일: {trimmedLine.Substring(4)}");
                }
                else if (trimmedLine.StartsWith("+++ "))
                {
                    result.AppendLine($"새 파일: {trimmedLine.Substring(4)}");
                }
                else if (trimmedLine.StartsWith("@@"))
                {
                    result.AppendLine($"변경 위치: {trimmedLine}");
                }
                else if (ContainsEnglishText(trimmedLine))
                {
                    // 영어 텍스트가 포함된 라인만 번역
                    try
                    {
                        string translatedText = await TranslateTextWithHTTP(trimmedLine);
                        result.AppendLine($"원본: {trimmedLine}");
                        result.AppendLine($"번역: {translatedText}");
                    }
                    catch (Exception ex)
                    {
                        result.AppendLine($"번역 실패: {trimmedLine} (오류: {ex.Message})");
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
        /// HTTP로 LibreTranslate API 직접 호출
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

                // LibreTranslate 무료 서버 사용
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

                return text; // 번역 실패 시 원본 반환
            }
            catch
            {
                return text; // 오류 시 원본 반환
            }
        }

        /// <summary>
        /// Git status 결과를 HTTP API로 번역
        /// </summary>
        private async Task<string> TranslateGitStatusWithHTTP(string englishStatus, string workingDir)
        {
            var result = new StringBuilder();
            result.AppendLine("=== 작업 디렉토리 상태 (HTTP API 번역) ===");
            result.AppendLine();

            var statusLines = englishStatus.Split('\n')
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            foreach (var line in statusLines.Take(10)) // 최대 10개 파일만
            {
                if (line.Length < 3) continue;

                string status = line.Substring(0, 2);
                string filePath = line.Substring(3);

                // 파일 상태를 한글로 번역
                string translatedStatus = await TranslateFileStatusWithHTTP(status);
                result.AppendLine($"{translatedStatus}: {filePath}");

                // 파일 내용도 일부 표시
                try
                {
                    string fullPath = Path.Combine(workingDir, filePath);
                    if (File.Exists(fullPath) && IsTextFile(fullPath))
                    {
                        string content = File.ReadAllText(fullPath, Encoding.UTF8);
                        if (content.Length > 800)
                            content = content.Substring(0, 800) + "\n... (내용이 길어서 일부만 표시)";
                        result.AppendLine("파일 내용:");
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

            return $"알 수 없는 상태({status})";
        }

        private bool ContainsEnglishText(string text)
        {
            // 영어가 포함된 의미있는 텍스트인지 확인
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

        #region 기존 메서드들

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
                catch (IOException) { /* 파일 사용 중이면 건너뜀 */ }
                catch (UnauthorizedAccessException) { /* 접근 권한 없으면 건너뜀 */ }
            }
            return contentBuilder.ToString();
        }

        private string GenerateDiffPrompt(string diff)
        {
            return
        $@"아래는 'git diff' 결과입니다. 이 변경 사항에 대한 커밋 메시지를 다음 규칙에 따라 영어로 작성해 주세요.

1.  **제목 (Subject):**
    *   변경 사항을 요약하는 한 줄의 제목을 작성합니다.
    *   **반드시 50자 이내**로 작성해야 합니다.
    *   명령형으로 작성합니다 (예: 'Fix' not 'Fixed', 'Add' not 'Added').
    *   제목 끝에 마침표를 찍지 마세요.

2.  **본문 (Body):**
    *   제목 아래에 한 줄을 비웁니다.
    *   무엇을, 왜 변경했는지 **자세히 설명**합니다. 한 줄이 아닌 **여러 줄로 작성**해도 좋습니다.
    *   어떻게 변경했는지는 코드 diff에 있으므로, 그보다는 **변경의 이유와 맥락**에 집중해주세요.

3.  **출력 형식:**
    *   생성된 제목과 본문을 합쳐서, 아래 예시처럼 바로 사용할 수 있는 'git commit -m ""...""' 명령어 형식으로 만들어주세요.

[커밋 메시지 예시]
git commit -m ""feat: Implement user authentication endpoint

- Add user registration and login logic.
- Use JWT for token-based authentication.
- Include basic validation for user input.""

이제 아래 diff 내용을 분석하여 커밋 메시지를 생성해 주세요.

diff 내용:
------------------------
{diff}
";
        }

        private string GenerateInitialCommitPrompt(string allCode)
        {
            return
        $@"아래는 프로젝트의 전체 소스 코드입니다.

이 프로젝트가 어떤 기능을 하는지 분석하고, 프로젝트의 첫 커밋(Initial Commit)에 어울리는 커밋 메시지를 생성해주세요.

- 먼저 프로젝트에 대한 간단한 설명을 한글로 작성합니다.
- 그리고 아래 형식에 맞춰 바로 사용할 수 있는 커밋 명령어를 영어로 생성합니다.

(커밋 명령어 예시:
git add .
git commit -m ""feat: Initial commit for GitDiffPromptGenerator project"")

전체 코드 내용:
------------------------
{allCode}
";
        }

        #endregion

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "프로젝트 루트 폴더를 선택하세요.";
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
                MessageBox.Show("새 창으로 열 내용이 없습니다.");
                return;
            }
            var promptForm = new PromptDisplayForm(txtPrompt.Text);
            promptForm.Show();
        }

      
    }
}
