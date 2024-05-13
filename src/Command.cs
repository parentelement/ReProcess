using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace ParentElement.ReProcess
{
    /// <summary>
    /// Represents a command that can be executed by the system.
    /// </summary>
    public sealed class Command
    {
        private Process _process;
        private bool _isRunning = false;
        private CommandDefinition _definition;
        private CancellationToken _token;

        private Channel<ConsoleMessage>? _buffer;

        internal Command(CommandDefinition definition)
        {
            _definition = definition;
            _process = new Process();
            _process.StartInfo = definition.ToProcessStartInfo();

            ConfigureProcess();
        }

        /// <summary>
        /// Attempts to run the configured process.
        /// </summary>
        /// <returns><see langword="true"/> if the process starts successfully.  <see langword="false"/> if the process failed or was already running</returns>
        public bool Start(CancellationToken cancellationToken)
        {
            if (_isRunning) return false;

            _isRunning = true;

            CreateOutputBuffer();

            _isRunning = _process.Start();

            if(_isRunning)
            {
                var s = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                _token = s.Token;
                _token.Register(Kill);
            }

            if (_definition.RelayOutput)
            {
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();
            }

            return _isRunning;
        }

        /// <summary>
        /// Attempts to run the configured process asynchronously.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the process</param>
        /// <returns></returns>
        public Task<bool> StartAsync(CancellationToken cancellationToken) => Task.Run(() => Start(cancellationToken));

        /// <summary>
        /// Kills the process if it is running.
        /// </summary>
        public void Kill() => _process.Kill();

        /// <summary>
        /// If "M:ReProcess.CommandBuilder.WithOutput" was called, this method will return the output of the process as it is written to StdOut and StdErr.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the process</param>
        /// <returns></returns>
        public async IAsyncEnumerable<ConsoleMessage> ReadOutputAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (_buffer == null || !_definition.RelayOutput)
                yield break;

            await foreach (var consoleMessage in _buffer.Reader.ReadAllAsync(cancellationToken))
            {
                yield return consoleMessage;

                if (!_definition.UseAggressiveOutputProcessing)
                    await Task.Delay(10);
            }
        }

        /// <summary>
        /// Waits for the process to exit.
        /// </summary>
        /// <returns>The Exit Code supplied by the process</returns>
        public async Task<int> WaitForExitAsync()
        {
            while (_isRunning)
            {
                if (!_definition.UseAggressiveOutputProcessing)
                    await Task.Delay(50);
            }

            return _process.ExitCode;
        }

        private void ConfigureProcess()
        {
            _process.EnableRaisingEvents = true;

            _process.Exited += (sender, args) =>
            {
                _isRunning = false;

                if (_buffer == null)
                    return;

                _buffer?.Writer.TryComplete();
            };

            if (_definition.RelayOutput)
            {
                _process.OutputDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrWhiteSpace(args.Data))
                        _buffer!.Writer.TryWrite(new ConsoleMessage(this, args.Data, MessageType.Output));
                };

                _process.ErrorDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrWhiteSpace(args.Data))
                        _buffer!.Writer.TryWrite(new ConsoleMessage(this, args.Data, MessageType.Error));
                };
            }
        }

        private void CreateOutputBuffer()
        {
            _buffer = _definition.MaxBufferSize.HasValue
                   ? Channel.CreateBounded<ConsoleMessage>(
                       new BoundedChannelOptions(_definition.MaxBufferSize.Value)
                       {
                           SingleReader = true,
                           FullMode = BoundedChannelFullMode.DropOldest
                       })
                   : Channel.CreateUnbounded<ConsoleMessage>(
                       new UnboundedChannelOptions()
                       {
                           SingleReader = true,
                       }
                   );
        }
    }
}
