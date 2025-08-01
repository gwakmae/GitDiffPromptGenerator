# GitDiffPromptGenerator

A C# WinForms application that generates AI-friendly prompts from Git diffs to help create meaningful commit messages using ChatGPT or other AI assistants.

## 🚀 Features

- **Git Diff Analysis**: Extract changes between current branch and origin/main
- **Initial Commit Support**: Analyze entire project source code for first commit
- **AI-Ready Prompts**: Generate structured prompts optimized for AI assistants
- **User-Friendly Interface**: Simple folder selection and result viewing
- **Korean Language Support**: UI and prompts in Korean
- **Flexible Analysis**: Support both incremental changes and full project analysis

## 📋 Prerequisites

- .NET 8.0 or later
- Windows OS (WinForms application)
- Git installed and accessible via command line
- Git repository with remote origin configured

## 🛠️ Installation

1. Clone the repository:
```bash
git clone https://github.com/yourusername/GitDiffPromptGenerator.git
```

2. Open the solution in Visual Studio 2022 or later

3. Build and run the application

## 📖 Usage

### For Incremental Changes
1. Launch the application
2. Click "폴더 선택" (Select Folder) to choose your Git repository
3. Click "변경점 추출 및 프롬프트 생성" (Extract Changes & Generate Prompt)
4. Copy the generated prompt and paste it into ChatGPT or your preferred AI assistant

### For Initial Commit
1. Select your project folder
2. Click "초기 커밋 프롬프트 생성" (Generate Initial Commit Prompt)
3. Use the generated prompt to get AI assistance for your first commit message

### Example Generated Prompt
```
아래는 현재 브랜치와 원격 main 브랜치 간의 git diff 결과입니다.

이 변경사항을 분석해서 적절한 커밋 메시지를 생성해주세요.
그리고 아래 형식대로 바로 쓸 수 있는 커밋 명령어를 함께 완성해서 제공해주세요.
(예시: git commit -m "fix: Correct calculation logic")

diff 결과:
------------------------
[Git diff content here]
```

## 🏗️ Project Structure

```
GitDiffPromptGenerator/
├── Form1.cs                    # Main application form
├── Form1.Designer.cs          # UI design code
├── PromptDisplayForm.cs       # Prompt display window
├── PromptDisplayForm.Designer.cs
├── Program.cs                 # Application entry point
└── GitDiffPromptGenerator.csproj
```

## 🔧 Key Components

- **Git Integration**: Uses command-line Git to extract diffs
- **File Analysis**: Automatically excludes binary files and build artifacts
- **Prompt Generation**: Creates structured prompts for AI consumption
- **Error Handling**: Comprehensive error checking for Git operations

## 📝 Example Workflow

1. **Developer makes changes** to their codebase
2. **Run GitDiffPromptGenerator** and select the repository folder
3. **Generate prompt** from the Git diff
4. **Paste into ChatGPT** with a request like:
   ```
   Please analyze these changes and suggest an appropriate commit message following conventional commit format.
   ```
5. **Receive AI-generated commit message** and apply it to your repository

## ⚠️ Requirements & Limitations

- Requires Git repository with configured remote origin
- Works with repositories that have a `main` branch on origin
- Designed for Windows environments (WinForms)
- Korean language interface (can be easily localized)

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built with .NET 8.0 and Windows Forms
- Designed to work seamlessly with modern AI assistants
- Inspired by the need for better commit message generation workflows

---

**Note**: This tool is designed to assist developers in creating better commit messages by leveraging AI capabilities. The generated prompts help AI understand the context of code changes for more accurate commit message suggestions.
