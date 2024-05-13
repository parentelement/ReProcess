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
    }
}