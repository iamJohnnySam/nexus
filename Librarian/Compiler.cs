using System.Diagnostics;

namespace Librarian
{
    public class Compiler(string filePath, string compilerPath = "/usr/bin/pdflatex")
    {
        public string FilePath { get; set; } = filePath;
        private readonly string compilerPath = compilerPath;

        public void Compile(string latexFile)
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = compilerPath,
                Arguments = $"\"{latexFile}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new()
            { StartInfo = startInfo })
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                File.WriteAllText("/home/pi/output.log", output);
                File.WriteAllText("/home/pi/error.log", error);
            }
        }

        public void OpenFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                Process.Start("okular", filePath);
            }
        }
    }
}
