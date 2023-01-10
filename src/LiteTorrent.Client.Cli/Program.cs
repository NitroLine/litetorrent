using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text.Json;
using LiteTorrent.Backend.Dto;

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
                Create(args);
                break;
            }
            case "parse":
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

    private static void Create(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("No RELATIVE_PATH");
            return;
        }
        
        var createInfo = new DtoCreateInfo(args[1], 32, args[2]);

        using var client = new HttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/commands/create");
        request.Content = new StringContent(
            JsonSerializer.Serialize(createInfo),
            MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Json));
        var responseMessage = client.Send(request);
        
        if (responseMessage.StatusCode is HttpStatusCode.NoContent)
            Console.WriteLine("Successfully completed");
        else
        {
            var output = new StreamReader(responseMessage.Content.ReadAsStream()).ReadToEnd();
            Console.WriteLine(output);
        }
    }

    private static void ShowSharedFiles()
    {
        using var client = new HttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/commands/show");
        request.Content = new ReadOnlyMemoryContent(ReadOnlyMemory<byte>.Empty);
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Json);
        var responseMessage = client.Send(request);

        var output = new StreamReader(responseMessage.Content.ReadAsStream()).ReadToEnd();
        
        if (output.Length == 0) 
            Console.WriteLine("No shared files");

        try
        {
            var jsonDocument = JsonSerializer.Deserialize<JsonDocument>(output);
            
            Console.WriteLine(
                JsonSerializer.Serialize(jsonDocument, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch
        {
            Console.WriteLine(output);
        }
    }

    private static void ShowHelp()
    {
        foreach (var line in File.ReadLines("help.txt")) 
            Console.WriteLine(line);
    }
}