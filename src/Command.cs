using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace ReProcess
{
    public sealed class Command
    {
        private Process _process;
        private bool _isRunning = false;
        private CommandDefinition _definition;

        private Channel<ConsoleMessage>? _buffer;

        internal Command(CommandDefinition definition)
        {
            _definition = definition;
            _process = new Process();
            _process.StartInfo = definition.ToProcessStartInfo();

            ConfigureProcess();
        }

        private void ConfigureProcess()
        {
            _process.EnableRaisingEvents = true;

            _process.Exited += (sender, args) =>
            {
                _isRunning = false;
            };

            if (_definition.RelayOutput)
            {
                _process.OutputDataReceived += async (sender, args) =>
                {
                    if (!string.IsNullOrWhiteSpace(args.Data))
                        await _buffer!.Writer.WriteAsync(new ConsoleMessage(this, args.Data, MessageType.Output));
                };

                _process.ErrorDataReceived += async (sender, args) =>
                {
                    if (!string.IsNullOrWhiteSpace(args.Data))
                        await _buffer!.Writer.WriteAsync(new ConsoleMessage(this, args.Data, MessageType.Error));
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

        /// <summary>
        /// Attempts to run the configured process.
        /// </summary>
        /// <returns><see langword="true""/> if the process starts successfully.  <see langword="false"/> if the process failed or was already running</returns>
        public bool Start()
        {
            if (_isRunning) return false;

            _isRunning = true;

            CreateOutputBuffer();

            _isRunning = _process.Start();

            if (_definition.RelayOutput)
            {
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();
            }

            Task.Run(() =>
            {
                _process.WaitForExit();
            });

            return _isRunning;
        }

        /// <summary>
        /// If "M:ReProcess.CommandBuilder.WithOutput" was called, this method will return the output of the process as it is written to StdOut and StdErr.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the process</param>
        /// <returns></returns>
        public async IAsyncEnumerable<ConsoleMessage> ReadOutputAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (!_definition.RelayOutput)
                yield break;

            do
            {
                while (_buffer!.Reader.Count > 0)
                {
                    var message = await _buffer.Reader.ReadAsync();
                    yield return message;
                }

                if (!_definition.UseAggressiveOutputProcessing)
                    await Task.Delay(50);

            } while (_isRunning && !cancellationToken.IsCancellationRequested);

            //Output any remaining messages
            while (_buffer.Reader.Count > 0 && !cancellationToken.IsCancellationRequested)
            {
                var message = await _buffer.Reader.ReadAsync();
                yield return message;
            }

            yield break;
        }

        /// <summary>
        /// Kills the process if it is running.
        /// </summary>
        public void Kill() => _process.Kill();

        /// <summary>
        /// Waits for the process to exit.
        /// </summary>
        /// <returns>The Exit Code supplied by the process</returns>
        public async Task<int> WaitForExitAsync()
        {
            while (_isRunning)
            {
                if (_definition.UseAggressiveOutputProcessing)
                    await Task.Delay(50);
            }

            return _process.ExitCode;
        }
    }
}
