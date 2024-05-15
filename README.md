[![Run Tests](https://github.com/parentelement/ReProcess/actions/workflows/run-tests.yml/badge.svg?branch=main)](https://github.com/parentelement/ReProcess/actions/workflows/run-tests.yml)

# ReProcess
Lightweight Async Executable Process Wrapper for .NET Inspired by CliWrap.  Full usage documentation in the Wiki.  Here are two basic examples, one simple and one full:

## NuGet
ReProcess is available via NuGet:  [ParentElement.ReProcess](https://www.nuget.org/packages/ParentElement.ReProcess/)

## Simple Example
```
var cmd = CommandBuilder.Create("ping")
  .WithArgument("127.0.0.1")
  .Build();

cmd.Start(cancellationToken);
```
## Full Example
```
var cmd = CommandBuilder.Create("docker")
    .WithArgument("build")
    .WithArgument("-f Dockerfile")
    .WithArgument("--build-arg POSTGRES_USER=postgres")
    .WithArgument("--build-arg POSTGRES_PASSWORD=$uper$ecretPa$$w0rd")
    .WithArgument("--progress=plain")
    .WithArgument("--no-cache")
    .WithArgument("-t MyCustomImage:latest")
    .WithArgument(".")
    .WithWorkingDirectory(Path.Combine(Directory.GetCurrentDirectory(), "CustomContainer"))
    .WithAggressiveOutputProcessing()
    .WithOutput(1000) //Only cache the last 1k messages if unprocessed
    .Build();

if(cmd.Start(cancellationToken))
{
    await foreach (var msg in cmd.ReadOutputAsync(cancellationToken))
    {
        await Console.Out.WriteLineAsync($"{msg.MessageType.ToString()}: {msg.Data}");
    }

    var exitCode = await cmd.WaitForExitAsync();

    await Console.Out.WriteLineAsync($"Process exited with code: {exitCode}");
}
else
{
    await Console.Out.WriteLineAsync("Process failed to start");
}
```
