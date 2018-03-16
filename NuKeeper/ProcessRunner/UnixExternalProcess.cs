using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NuKeeper.ProcessRunner
{
    public class UnixProcess : IExternalProcess
    {
        public async Task<ProcessOutput> Run(string workingDirectory, string command, string arguments, bool ensureSuccess)
        {
            Process process;

            try
            {
                var processInfo = new ProcessStartInfo(command, arguments)
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    WorkingDirectory = workingDirectory
                };

                process = Process.Start(processInfo);
            }
            catch (Exception ex)
            {
                if (ensureSuccess)
                {
                    throw;
                }

                var message = $"Error starting unix external process for {command}: {ex.GetType().Name} {ex.Message}";
                return new ProcessOutput(string.Empty, message, 1);
            }

            if (process == null)
            {
                throw new Exception($"Could not start external process for {command}");
            }

            var textOut = await process.StandardOutput.ReadToEndAsync();
            var errorOut = await process.StandardError.ReadToEndAsync();

            process.WaitForExit();

            var exitCode = process.ExitCode;

            if (ensureSuccess && exitCode != 0)
            {
                throw new Exception($"Exit code: {exitCode}\n\n{textOut}\n\n{errorOut}");
            }

            return new ProcessOutput(textOut, errorOut, exitCode);
        }
    }
}