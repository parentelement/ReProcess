namespace ParentElement.ReProcess.Tests
{
    public class CommandBuilderTests
    {
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
    }
}
