namespace ParentElement.ReProcess.Tests
{
    public class CommandBuilderTests
    {
        [Fact]
        public void BuilderMethods_ShouldReturnCommandBuilderInstance()
        {
            var initialBuilder = CommandBuilder.Create("dotnet");

            var builder = initialBuilder.WithArgument("test");

            Assert.Same(initialBuilder, builder);

            builder = builder.WithArguments(["test1", "test2"]);

            Assert.Same(initialBuilder, builder);

            builder = builder.WithOutput();
            Assert.Same(initialBuilder, builder);

            builder = builder.WithWorkingDirectory(Directory.GetCurrentDirectory());
            Assert.Same(initialBuilder, builder);

            builder = builder.WithEnvironmentVariable("test", "test");
            Assert.Same(initialBuilder, builder);

            builder = builder.WithAggressiveOutputProcessing();
            Assert.Same(initialBuilder, builder);
        }

        [Fact]
        public void Build_ShouldReturnCommandInstance()
        {
            var builder = CommandBuilder.Create("dotnet");
         
            var command = builder.Build();

            Assert.NotNull(command);
            Assert.IsType<Command>(command);
        }

        [Fact]
        public void Create_ShouldThrowArgumentExceptionOnNullCommandName()
        {
            Assert.Throws<ArgumentException>(() => CommandBuilder.Create(null));
        }

        [Fact]
        public void Create_ShouldThrowArgumentExceptionOnEmptyCommandName()
        {
            Assert.Throws<ArgumentException>(() => CommandBuilder.Create(string.Empty));
        }

        [Fact]
        public void Create_ShouldThrowArgumentExceptionOnWhitespaceCommandName()
        {
            Assert.Throws<ArgumentException>(() => CommandBuilder.Create(" "));
        }

        [Fact]
        public void WithArgument_ShouldThrowArgumentExceptionOnNullArgument()
        {
            var builder = CommandBuilder.Create("dotnet");

            Assert.Throws<ArgumentException>(() => builder.WithArgument(null));
        }

        [Fact]
        public void WithArgument_ShouldThrowArgumentExceptionOnEmptyArgument()
        {
            var builder = CommandBuilder.Create("dotnet");

            Assert.Throws<ArgumentException>(() => builder.WithArgument(string.Empty));
        }

        [Fact]

        public void WithArguments_ShouldThrowArgumentNullExceptionOnNullArguments()
        {
            var builder = CommandBuilder.Create("dotnet");

            Assert.Throws<ArgumentNullException>(() => builder.WithArguments(null!));
        }

        [Fact]
        public void WithEnvironmentVariable_ShouldThrowArgumentExceptionOnNullKey()
        {
            var builder = CommandBuilder.Create("dotnet");

            Assert.Throws<ArgumentNullException>(() => builder.WithEnvironmentVariable(null!, "test"));
        }

        [Fact]
        public void WithWorkingDirectory_ShouldThrowDirectoryNotFoundExceptionOnInvalidPath()
        {
            var fakePathPart = Guid.NewGuid().ToString();
            var builder = CommandBuilder.Create("dotnet");

            Assert.Throws<DirectoryNotFoundException>(() => builder.WithWorkingDirectory($"/fakepath/{fakePathPart}"));
        }

        [Fact]
        public void WithWorkingDirectory_ShouldNotThrowOnValidPath()
        {
            var builder = CommandBuilder.Create("dotnet");

            builder.WithWorkingDirectory(Directory.GetCurrentDirectory());

            Assert.True(true);
        }

        [Fact]
        public void WithOutput_ShouldThrowOnZeroCount()
        {
            var builder = CommandBuilder.Create("dotnet");

            Assert.Throws<ArgumentException>(() => builder.WithOutput(0));
        }

        [Fact]
        public void WithOutput_ShouldThrowOnNegativeCount()
        {
            var builder = CommandBuilder.Create("dotnet");

            Assert.Throws<ArgumentException>(() => builder.WithOutput(-1));
        }

        [Fact]
        public void WithOutput_ShouldNotThrowOnPositiveCount()
        {
            var builder = CommandBuilder.Create("dotnet");

            builder.WithOutput(1);

            Assert.True(true);
        }

        [Fact]
        public void WithOutput_ShouldNotThrowOnDefaultCount()
        {
            var builder = CommandBuilder.Create("dotnet");

            builder.WithOutput();

            Assert.True(true);
        }

        [Fact]
        public void WithAggressiveOutputProcessing_ShouldNotThrow()
        {
            var builder = CommandBuilder.Create("dotnet");

            builder.WithAggressiveOutputProcessing();

            Assert.True(true);
        }
    }
}
