using System.Diagnostics;

namespace ParentElement.ReProcess.Tests
{
    public class CommandTests
    {
        [Fact]
        public void Start_ShouldReturnTrueIfProcessWasStarted()
        {
            var cmd = CommandBuilder.Create("dotnet")
                .Build();

            var result = cmd.Start(CancellationToken.None);

            Assert.True(result);
        }

        [Fact]
        public async Task StartAsync_ShouldReturnTrueIfProcessWasStarted()
        {
            var cmd = CommandBuilder.Create("dotnet")
                .Build();

            var result = await cmd.StartAsync(CancellationToken.None);

            Assert.True(result);
        }

        [Fact]
        public async Task ReadOutputAsync_ShouldOutputCorrectResult()
        {
            var output = "Usage: dotnet [options]";

            var cmd = CommandBuilder.Create("dotnet")
                .WithOutput()
                .Build();

            await cmd.StartAsync(CancellationToken.None);

            await foreach (var message in cmd.ReadOutputAsync(CancellationToken.None))
            {
                Assert.Equal(output, message.Data);
                break;
            }
        }

        [Fact]
        public async Task ReadOutputAsync_ShouldNotErrorIfStartAsyncIsNotAwaited()
        {
            var cmd = CommandBuilder.Create("dotnet")
                .WithOutput()
                .Build();

            cmd.StartAsync(CancellationToken.None);

            await foreach (var message in cmd.ReadOutputAsync(CancellationToken.None))
            {
                break;
            }

            Assert.True(true);
        }

        [Fact]
        public async Task WithOutput_ShouldLimitMessageBufferSize()
        {
            var rnd = new Random();
            var targetCount = rnd.Next(1, 4);

            var cmd = CommandBuilder.Create("dotnet")
                .WithOutput(targetCount)
                .Build();

            await cmd.StartAsync(CancellationToken.None);

            await cmd.WaitForExitAsync();

            var actualCount = 0;

            await foreach (var message in cmd.ReadOutputAsync(CancellationToken.None))
            {
                actualCount++;
            }

            Assert.Equal(targetCount, actualCount);
        }

        [Fact]
        public async Task UseAggressiveProcessing_ShouldCreateImmediateReads()
        {
            var minDelay = 52; //Setting the check value slightly higher due to stopwatch precision

            var cmd = CommandBuilder.Create("dotnet")
                .WithOutput()
                .WithAggressiveOutputProcessing()
                .Build();

            cmd.Start(CancellationToken.None);
            await cmd.WaitForExitAsync();

            var sw = new Stopwatch();

            var current = 0;

            await foreach (var message in cmd.ReadOutputAsync(CancellationToken.None))
            {
                if (current++ > 0)
                {
                    Assert.True(sw.ElapsedMilliseconds < minDelay);
                    sw.Restart();
                }
                else
                {
                    sw.Start();
                }
            }
        }

        [Fact]
        public async Task ByDefault_ReadsShouldBeDelayedPerIteration()
        {
            var minDelay = 48; //Setting the check value slightly lower due to stopwatch precision

            var cmd = CommandBuilder.Create("dotnet")
                .WithOutput()
                .Build();

            cmd.Start(CancellationToken.None);
            await cmd.WaitForExitAsync();

            var sw = new Stopwatch();

            var current = 0;

            await foreach (var message in cmd.ReadOutputAsync(CancellationToken.None))
            {
                if (current > 3)
                    break;

                if (current++ > 0)
                {
                    Assert.True(sw.ElapsedMilliseconds >= minDelay);
                    sw.Restart();
                }
                else
                {
                    sw.Start();
                }
            }
        }
    }
}