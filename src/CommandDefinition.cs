using System.Diagnostics;

namespace ReProcess
{
    internal sealed class CommandDefinition
    {
        internal string CommandName { get; } = string.Empty;
        internal ICollection<string> Arguments { get; } = new List<string>();
        internal IDictionary<string, string> EnvironmentVariables { get; } = new Dictionary<string, string>();
        internal string? WorkingDirectory { get; set; }
        internal int? MaxBufferSize { get; set; } = null;
        internal bool RelayOutput { get; set; } = false;
        internal bool UseAggressiveOutputProcessing { get; set; } = false;

        internal CommandDefinition(string commandName)
        {
            CommandName = commandName;
        }

        internal ProcessStartInfo ToProcessStartInfo()
        {
            var result = new ProcessStartInfo
            {
                Arguments = string.Join(" ", Arguments),
                CreateNoWindow = true,
                FileName = CommandName,
                RedirectStandardError = RelayOutput,
                RedirectStandardInput = RelayOutput,
                RedirectStandardOutput = RelayOutput,
                UseShellExecute = false,
                WorkingDirectory = WorkingDirectory
            };

            foreach(var (key, value) in EnvironmentVariables)
            {
                result.EnvironmentVariables.Add(key, value);
            }

            return result;
        }
    }
}
