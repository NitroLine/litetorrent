using System.Net;
using LiteTorrent.Core;
using LiteTorrent.Domain.Services.Commands;
using LiteTorrent.Domain.Services.Commands.AddSharedFile;
using LiteTorrent.Domain.Services.Commands.CreateSharedFile;
using LiteTorrent.Domain.Services.Common.Serialization;
using LiteTorrent.Domain.Services.LocalStorage.Configuration;
using MessagePack;
using MessagePipe;
using Microsoft.Extensions.Hosting;

namespace LiteTorrent.Client.Cli;

public class CliService : BackgroundService
{
    private readonly IAsyncRequestHandler<AddSharedFileCommand, Result<Unit>> addSharedFileCommandHandler;
    private readonly IAsyncRequestHandler<CreateSharedFileCommand, Result<DtoUserSharedFile>> createSharedFileCommandHandler;
    private readonly LocalStorageConfiguration localStorageConfiguration;

    public CliService(
        IAsyncRequestHandler<AddSharedFileCommand, Result<Unit>> addSharedFileCommandHandler,
        IAsyncRequestHandler<CreateSharedFileCommand, Result<DtoUserSharedFile>> createSharedFileCommandHandler,
        LocalStorageConfiguration localStorageConfiguration)
    {
        this.addSharedFileCommandHandler = addSharedFileCommandHandler;
        this.createSharedFileCommandHandler = createSharedFileCommandHandler;
        this.localStorageConfiguration = localStorageConfiguration;
    }
    
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // TODO: command: log
        
        while (!cancellationToken.IsCancellationRequested)
        {
            Console.Write(">");
            var input = Console.ReadLine()?.Split(' ') ?? throw new NullReferenceException("Null input");
            if (input.Length < 1)
                Console.WriteLine("Bad input");

            switch (input[0])
            {
                case "add":
                {
                    Console.WriteLine(input[1]);
                    var addResult = await addSharedFileCommandHandler.InvokeAsync(
                        new AddSharedFileCommand(input[1]),
                        cancellationToken);
                    
                    if (addResult.TryGetError(out _, out var error))
                    {
                        Console.WriteLine(error.Message);
                        continue;
                    }
                    
                    Console.WriteLine("OK");
                    break;
                }
                case "create":
                {
                    var relativePath = input[1];
                    var fileInfo = new FileInfo(Path.Join(localStorageConfiguration.ShardDirectoryPath, relativePath));
                    var output = Path.Join(localStorageConfiguration.SharedFileDirectoryPath, input[2]);
                    var command = new CreateSharedFileCommand(
                        Array.Empty<DnsEndPoint>(),
                        relativePath,
                        (ulong)fileInfo.Length,
                        32,
                        output);

                    var createResult = await createSharedFileCommandHandler.InvokeAsync(command, cancellationToken);
                    if (createResult.TryGetError(out var userSharedFile, out var error))
                        Console.WriteLine(error.Message);

                    await using var file = new FileStream(output, FileMode.Open, FileAccess.Write);

                    await MessagePackSerializer.SerializeAsync(
                        file,
                        userSharedFile,
                        SerializerHelper.DefaultOptions,
                        cancellationToken);
                    
                    Console.WriteLine("OK");
                    
                    break;
                }
            }
        }
    }
}