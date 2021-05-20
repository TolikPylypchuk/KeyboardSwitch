using System;
using System.Diagnostics;
using System.Text;

namespace KeyboardSwitch.Linux
{
    internal static class Systemd
    {
        private record Result(int ExitCode, string Output, string ErrorOutput);

        public static bool IsLoaded(string service) =>
            GetProperty(service, "LoadState") == "loaded";

        public static bool IsActive(string service) =>
            GetProperty(service, "ActiveState") == "active";

        public static bool IsEnabled(string service) =>
            GetProperty(service, "UnitFileState") == "enabled";

        public static string GetProperty(string service, string property)
        {
            var result = Run($"show {service} --property {property}");
            return result.ExitCode == 0 ? result.Output.Trim().Substring(property.Length + 1) : String.Empty;
        }

        public static void Start(string service) =>
            Run($"start {service}");

        public static void Stop(string service) =>
            Run($"stop {service}");

        public static void Kill(string service) =>
            Run($"kill -s SIGKILL {service}");

        public static void Reload(string service) =>
            Run($"reload {service}");

        public static void Enable(string service) =>
            Run($"enable {service}");

        public static void Disable(string service) =>
            Run($"disable {service}");

        private static Result Run(string command)
        {
            var output = new StringBuilder();
            var error = new StringBuilder();

            using var systemctl = new Process()
            {
                StartInfo = new()
                {
                    FileName = "systemctl",
                    Arguments = $"--user {command}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                }
            };

            systemctl.Start();

            systemctl.OutputDataReceived += (_, args) => output.AppendLine(args.Data);
            systemctl.BeginOutputReadLine();

            systemctl.ErrorDataReceived += (_, args) => error.AppendLine(args.Data);
            systemctl.BeginErrorReadLine();

            systemctl.WaitForExit();

            return new Result(systemctl.ExitCode, output.ToString(), error.ToString());
        }
    }
}
