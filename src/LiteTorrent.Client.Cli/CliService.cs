using LiteTorrent.Core;
using LiteTorrent.Domain.Services.Commands.AddSharedFile;
using MessagePipe;
using Microsoft.Extensions.Hosting;

namespace LiteTorrent.Client.Cli;

public class CliService : BackgroundService
{
    private readonly IAsyncRequestHandler<AddSharedFileCommand, Result<Unit>> addSharedFileCommandHandler;

    public CliService(
        IAsyncRequestHandler<AddSharedFileCommand, Result<Unit>> addSharedFileCommandHandler)
    {
        this.addSharedFileCommandHandler = addSharedFileCommandHandler;
    }
    
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
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
            }
        }
    }
}