namespace Arkanis.Overlay.LocalLink.UnitTests.Services;

using System.IO.Pipes;
using System.Text.Json;
using Exceptions;
using LocalLink.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Models;
using Models.Commands;
using Shouldly;

public class NamedPipeCommandCommunicationTests
{
    private readonly ILogger<NamedPipeCommandClient> _clientLogger = NullLogger<NamedPipeCommandClient>.Instance;

    private readonly ILogger<NamedPipeCommandServerBackgroundPublisherService> _serverLogger =
        NullLogger<NamedPipeCommandServerBackgroundPublisherService>.Instance;

    [Fact]
    public async Task NamedPipeCommandClient_SendAsync_ShouldSerializeAndSendCommand()
    {
        // Arrange
        var pipeName = CreatePipeName();
        var command = new TestCommand
        {
            TestPropertyString = "TestValue",
            TestPropertyInt = 4468,
        };
        await using var server = new NamedPipeServerStream(pipeName, PipeDirection.In);
        var client = new NamedPipeCommandClient(_clientLogger, pipeName);

        // Act
        var sendTask = client.SendAsync(command, CancellationToken.None);
        await server.WaitForConnectionAsync(TestContext.Current.CancellationToken);

        // Assert
        var receivedCommand = await JsonSerializer.DeserializeAsync<LocalLinkCommandBase>(server, cancellationToken: TestContext.Current.CancellationToken);
        receivedCommand.ShouldNotBeNull();

        var typedCommand = receivedCommand.ShouldBeOfType<TestCommand>();
        typedCommand.TestPropertyString.ShouldBe(command.TestPropertyString);
        typedCommand.TestPropertyInt.ShouldBe(command.TestPropertyInt);

        await sendTask;
    }

    [Fact]
    public async Task NamedPipeCommandServer_ReceiveAsync_ShouldReceiveAndDeserializeCommand()
    {
        // Arrange
        var pipeName = CreatePipeName();
        var command = new TestCommand
        {
            TestPropertyString = "TestValue",
            TestPropertyInt = 879,
        };
        var clientPipe = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);

        var server = new NamedPipeCommandServer(_serverLogger, pipeName);

        // Act
        var receiveTask = server.ReceiveAsync(TimeSpan.FromSeconds(2), CancellationToken.None);
        await clientPipe.ConnectAsync(TestContext.Current.CancellationToken);

        await JsonSerializer.SerializeAsync<LocalLinkCommandBase>(clientPipe, command, cancellationToken: TestContext.Current.CancellationToken);
        await clientPipe.DisposeAsync();

        // Assert
        var receivedCommand = await receiveTask;

        var typedCommand = receivedCommand.ShouldBeOfType<TestCommand>();
        typedCommand.TestPropertyString.ShouldBe(command.TestPropertyString);
        typedCommand.TestPropertyInt.ShouldBe(command.TestPropertyInt);
    }

    [Fact]
    public async Task NamedPipeCommandServer_ReceiveAsync_ShouldThrowOnTimeout_WhenSendingTakesTooLong()
    {
        // Arrange
        var pipeName = CreatePipeName();
        var server = new NamedPipeCommandServer(_serverLogger, pipeName);
        await using var clientPipe = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
        var communicationTimeout = TimeSpan.FromMilliseconds(100);

        // Act
        var receiveTask = server.ReceiveAsync(communicationTimeout, CancellationToken.None);
        await clientPipe.ConnectAsync(TestContext.Current.CancellationToken);

        await Task.Delay(communicationTimeout * 2, TestContext.Current.CancellationToken);

        // Assert
        await Assert.ThrowsAsync<LocalLinkConnectionException>(() => receiveTask);
    }

    private static string CreatePipeName()
        => $"{NamedPipeCommandServer.PipeName}-{Guid.NewGuid():N}";
}
