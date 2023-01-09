using System.Net.Http.Headers;
using System.Net.Mime;

namespace LiteTorrent.Client.Cli;

public static class Program
{
    private const string BaseUrl = "http://localhost:3000";
    
    public static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("No command");
            return;
        }
        
        switch (args[0])
        {
            case "create":
            {
                break;
            }
            case "add":
            {
                break;
            }
            case "show":
            {
                ShowSharedFiles();
                break;
            }
            case "download":
            {
                break;
            }
            case "stop":
            {
                break;
            }
            case "help":
            {
                ShowHelp();
                break;
            }
            default:
            {
                Console.WriteLine($"No command '{args[0]}'");
                ShowHelp();
                break;
            }
        }
    }

    private static void ShowSharedFiles()
    {
        using var client = new HttpClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/commands/sharedFiles");
        request.Content = new ReadOnlyMemoryContent(ReadOnlyMemory<byte>.Empty);
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Json);
        var responseMessage = client.Send(request);

        var output = new StreamReader(responseMessage.Content.ReadAsStream()).ReadToEnd();
        
        if (output.Length == 0) 
            Console.WriteLine("No shared files");
        
        Console.WriteLine(output);
    }

    private static void ShowHelp()
    {
        foreach (var line in File.ReadLines("help.txt")) 
            Console.WriteLine(line);
    }
}