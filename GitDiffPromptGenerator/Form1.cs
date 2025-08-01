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

        // --- 버튼 1: 기존 기능 (변경점 비교) ---
        private void btnGenerate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFolder) || !Directory.Exists(selectedFolder))
            {
                MessageBox.Show("먼저 Git 저장소 폴더를 선택하세요.");
                return;
            }

            // 이 기능은 반드시 Git 저장소에서만 동작해야 합니다.
            if (!Directory.Exists(Path.Combine(selectedFolder, ".git")))
            {
                MessageBox.Show("이 기능은 Git 저장소에서만 동작합니다.\n'초기 커밋 프롬프트 생성'을 이용하거나 폴더에서 'git init'을 먼저 실행해주세요.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                string diff = GetGitDiff(selectedFolder);
                if (string.IsNullOrEmpty(diff))
                {
                    MessageBox.Show("원격 저장소(origin/main) 대비 변경 사항이 없습니다.");
                    return;
                }
                string prompt = GenerateDiffPrompt(diff);
                txtPrompt.Text = prompt;
            }
            catch (Exception ex)
            {
                // 원격 브랜치가 없을 때 발생하는 오류 등을 포함하여 처리
                MessageBox.Show("오류가 발생했습니다.\n- 원격 저장소(origin)와 main 브랜치가 존재하는지 확인해주세요.\n- " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- 버튼 2: 새 기능 (초기 커밋용) ---
        private void btnGenerateForNewFiles_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFolder) || !Directory.Exists(selectedFolder))
            {
                MessageBox.Show("먼저 프로젝트 폴더를 선택하세요.");
                return;
            }

            try
            {
                // Git과 무관하게 폴더 내 모든 코드 파일을 읽습니다.
                string allCode = GetAllFilesContent(selectedFolder);
                if (string.IsNullOrEmpty(allCode))
                {
                    MessageBox.Show("폴더에서 분석할 수 있는 코드 파일을 찾지 못했습니다.");
                    return;
                }
                // 초기 커밋용 프롬프트를 생성합니다.
                string prompt = GenerateInitialCommitPrompt(allCode);
                txtPrompt.Text = prompt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("파일을 읽는 중 오류가 발생했습니다: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Helper Methods

        /// <summary>
        /// 지정된 폴더와 하위 폴더의 모든 텍스트 파일 내용을 읽어 하나의 문자열로 합칩니다.
        /// bin, obj, .git 등 불필요한 폴더와 바이너리 파일은 제외합니다.
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
                    contentBuilder.AppendLine($"--- 파일: {relativePath} ---");
                    contentBuilder.AppendLine(File.ReadAllText(file, Encoding.UTF8));
                    contentBuilder.AppendLine("----------------------------------------\n");
                }
                catch (IOException) { /* 파일이 사용 중이면 건너뜀 */ }
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
                if (process == null) throw new Exception("프로세스를 시작할 수 없습니다.");
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
        /// Git Diff 결과를 바탕으로 프롬프트를 생성합니다.
        /// </summary>
        private string GenerateDiffPrompt(string diff)
        {
            return
        $@"아래는 내 로컬 저장소와 원격 저장소(main) 사이의 git diff 결과입니다.

이 변경사항을 요약해서 영어로 간단하게 설명해줘.
그리고 아래 형식대로 바로 쓸 수 있는 커밋 명령어 한 줄을 완성해서 출력해줘. 
(예시: git commit -m ""fix: Correct calculation logic"")

diff 내용:
------------------------
{diff}
";
        }

        /// <summary>
        /// 프로젝트 전체 코드를 기반으로 초기 커밋 프롬프트를 생성합니다.
        /// </summary>
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