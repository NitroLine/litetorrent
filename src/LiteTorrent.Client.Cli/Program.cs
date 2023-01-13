using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text.Json;
using LiteTorrent.Backend.Dto;
using Microsoft.Extensions.Configuration;

namespace LiteTorrent.Client.Cli;

public static class Program
{
#pragma warning disable CS8618
    private static string baseUrl;
#pragma warning restore CS8618
    
    public static void Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        
        baseUrl = $"http://127.0.0.1:{config["BackendAddress"]}";

        if (args.Length < 1)
        {
            Console.WriteLine("No command. Use 'help'");
            return;
        }
        
        switch (args[0])
        {
            case "create":
            {
                Create(args);
                break;
            }
            case "add":
            {
                Add(args);
                break;
            }
            case "show":
            {
                ShowSharedFiles();
                break;
            }
            case "download":
            {
                StartDownloading(args);
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
        
        var createInfo = new DtoCreateInfo(args[1], 1024, args[2]);

        using var client = new HttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/commands/create");
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

    private static void Add(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("No ABS_TORRENT_FILE_PATH");
            return;
        }

        using var client = new HttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/commands/add");

        var torrentFile = new DtoTorrentFile(args[1]);
        request.Content = new StringContent(
            JsonSerializer.Serialize(torrentFile),
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
    
    private static void StartDownloading(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("No HOSTs");
            return;
        }

        using var client = new HttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/commands/download");

        var downloadingInfo = new DtoStartDownloadingInfo(args[2..], args[1]);
        request.Content = new StringContent(
            JsonSerializer.Serialize(downloadingInfo),
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
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/commands/show");
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
        var dir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()!.Location);
        foreach (var line in File.ReadLines(Path.Join(dir, "help.txt"))) 
            Console.WriteLine(line);
    }
}
