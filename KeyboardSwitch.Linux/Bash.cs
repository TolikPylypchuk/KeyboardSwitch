using System;
using System.Diagnostics;
using System.Text;

namespace KeyboardSwitch.Linux
{
    internal static class Bash
    {
        internal record Result(int ExitCode, string Output, string ErrorOutput);

        public static Result Run(string commandLine)
        {
            var output = new StringBuilder();
            var error = new StringBuilder();

            string arguments = $"-c \"{commandLine}\"";

            using var bash = new Process()
            {
                StartInfo = new()
                {
                    FileName = "bash",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                }
            };

            bash.Start();

            bash.OutputDataReceived += (_, args) => output.AppendLine(args.Data);
            bash.BeginOutputReadLine();

            bash.ErrorDataReceived += (_, args) => error.AppendLine(args.Data);
            bash.BeginErrorReadLine();

            if (!bash.DoubleWaitForExit())
            {
                throw new Exception(
                    $"Process timed out. Command line: bash {arguments}.\n" +
                    $"Output:\n{output}\n\n" +
                    $"Error:\n{error}");
            }

            return new Result(bash.ExitCode, output.ToString(), error.ToString());
        }

        private static bool DoubleWaitForExit(this Process process)
        {
            var result = process.WaitForExit(milliseconds: 500);

            if (result)
            {
                process.WaitForExit();
            }

            return result;
        }
    }
}
