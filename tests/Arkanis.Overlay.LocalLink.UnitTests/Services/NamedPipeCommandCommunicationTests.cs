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
    private const string PipeName = NamedPipeCommandServer.PipeName;
    private readonly ILogger<NamedPipeCommandClient> _clientLogger = NullLogger<NamedPipeCommandClient>.Instance;

    private readonly ILogger<NamedPipeCommandServerBackgroundPublisherService> _serverLogger =
        NullLogger<NamedPipeCommandServerBackgroundPublisherService>.Instance;

    [Fact]
    public async Task NamedPipeCommandClient_SendAsync_ShouldSerializeAndSendCommand()
    {
        // Arrange
        var command = new TestCommand
        {
            TestPropertyString = "TestValue",
            TestPropertyInt = 4468,
        };
        await using var server = new NamedPipeServerStream(PipeName, PipeDirection.In);
        var client = new NamedPipeCommandClient(_clientLogger);

        // Act
        var sendTask = client.SendAsync(command, CancellationToken.None);
        await server.WaitForConnectionAsync();

        // Assert
        var receivedCommand = await JsonSerializer.DeserializeAsync<LocalLinkCommandBase>(server);
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
        var command = new TestCommand
        {
            TestPropertyString = "TestValue",
            TestPropertyInt = 879,
        };
        var clientPipe = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);

        var server = new NamedPipeCommandServer(_serverLogger);

        // Act
        var receiveTask = server.ReceiveAsync(TimeSpan.FromSeconds(2), CancellationToken.None);
        await clientPipe.ConnectAsync();

        await JsonSerializer.SerializeAsync<LocalLinkCommandBase>(clientPipe, command);
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
        var server = new NamedPipeCommandServer(_serverLogger);
        await using var clientPipe = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
        var communicationTimeout = TimeSpan.FromMilliseconds(100);

        // Act
        var receiveTask = server.ReceiveAsync(communicationTimeout, CancellationToken.None);
        await clientPipe.ConnectAsync();

        await Task.Delay(communicationTimeout * 2);

        // Assert
        await Assert.ThrowsAsync<LocalLinkConnectionException>(() => receiveTask);
    }
}
