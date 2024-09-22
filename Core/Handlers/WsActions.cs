using System.Diagnostics;
using System.IO.Compression;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;

namespace Core.Handlers;

public static class WsActionsUtils
{
    private static int[] _numbers = new int[100];
    private static List<string> _filesList = [];
    
    /// <summary>
    /// This method echoes the messages sent by the client.
    /// </summary>
    public static async Task EchoMessages(WebSocket webSocket, string message)
    {
        var serverMessage = Encoding.UTF8.GetBytes($"Echo: {message}");
        await webSocket.SendAsync(new ArraySegment<byte>(serverMessage, 0, serverMessage.Length), WebSocketMessageType.Text, WebSocketMessageFlags.EndOfMessage, CancellationToken.None);
    }
    
    
    /// <summary>
    /// This method sends a file to the client.
    /// </summary>
    public static async Task SendFile(WebSocket webSocket, string filePath)
    {
        try
        {
            var buffer = new byte[1024 * 4];
            await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            int bytesRead;
            while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, bytesRead), WebSocketMessageType.Binary, bytesRead < buffer.Length, CancellationToken.None);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    /// <summary>
    /// This method counts to 100 and sends the numbers to the client.
    /// </summary>
    public static async Task CountTo100(WebSocket webSocket)
    {
        var tasks = new List<Task>();

        for (var i = 0; i < 100; i++)
        {
            var num = i;
            tasks.Add(Task.Run(() => _numbers[num] = num));
        }

        await Task.WhenAll(tasks);

        var serverMessage = Encoding.UTF8.GetBytes($"Numbers: {string.Join(", ", _numbers)}");
        await webSocket.SendAsync(new ArraySegment<byte>(serverMessage, 0, serverMessage.Length), WebSocketMessageType.Text, true, CancellationToken.None);
    
    }
    
    /// <summary>
    /// This method lists all files in a directory and sends them to the client.
    /// </summary>
    public static async Task ListFiles(WebSocket webSocket, string path, int maxDepth)
    {
        try
        {
            var files = ListFilesInternal(path, maxDepth, 0);
            var filesBytes = Encoding.UTF8.GetBytes($"Files: {string.Join(", ", files)}");
            await webSocket.SendAsync(new ArraySegment<byte>(filesBytes, 0, filesBytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private static List<string> ListFilesInternal(string path, int maxDepth, int currentDepth)
    {
        try
        {
            if (currentDepth > maxDepth)
            {
                return _filesList;
            }

            _filesList.AddRange(Directory.GetFiles(path));

            foreach (var directory in Directory.GetDirectories(path))
            {
                _filesList.AddRange(ListFilesInternal(directory, maxDepth, currentDepth + 1));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return _filesList;
    }
    
    /// <summary>
    /// This method zips a directory and sends the zip file to the client.
    /// </summary>
    public static async Task ZipAndSendDirectory(WebSocket webSocket, string sourceDir)
    {
        try
        {
            var zipFileName = $"{Path.GetFileName(sourceDir)}.zip";
            var zipFilePath = Path.Combine(Path.GetTempPath(), zipFileName);

            ZipFile.CreateFromDirectory(sourceDir, zipFilePath, CompressionLevel.Optimal, false);

            var buffer = new byte[1024 * 4];
            await using var zipFileStream = new FileStream(zipFilePath, FileMode.Open, FileAccess.Read);

            int bytesRead;
            while ((bytesRead = await zipFileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, bytesRead), WebSocketMessageType.Binary, bytesRead < buffer.Length, CancellationToken.None);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    public static async Task GetAllProcesses(WebSocket webSocket)
    {
        var processes = Process.GetProcesses()
            .Select(p => new { p.Id, p.ProcessName })
            .ToList();
        var serverMessage = Encoding.UTF8.GetBytes($"Processes: {JsonConvert.SerializeObject(processes)}");
        await webSocket.SendAsync(new ArraySegment<byte>(serverMessage, 0, serverMessage.Length), WebSocketMessageType.Text, true, CancellationToken.None);
    }
    
    public static async Task CheckFileOrDirExists(WebSocket webSocket, string path)
    {
        var exists = File.Exists(path) || Directory.Exists(path);
        var response = Encoding.UTF8.GetBytes(exists.ToString());
        await webSocket.SendAsync(new ArraySegment<byte>(response, 0, response.Length), WebSocketMessageType.Text, true, CancellationToken.None);
    }
    
    public static async Task GetProcessInfo(WebSocket webSocket, int pid)
    {
        try
        {
            var process = Process.GetProcessById(pid);
            var processInfo = new
            {
                process.Id,
                process.ProcessName,
                process.StartTime,
                process.TotalProcessorTime,
                process.WorkingSet64
            };
            var response = Encoding.UTF8.GetBytes($"ProcessInfo: {JsonConvert.SerializeObject(processInfo)}");
            await webSocket.SendAsync(new ArraySegment<byte>(response, 0, response.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (Exception e)
        {
            var errorMessage = Encoding.UTF8.GetBytes($"Error: {e.Message}");
            await webSocket.SendAsync(new ArraySegment<byte>(errorMessage, 0, errorMessage.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
    
    public static async Task TerminateProcess(WebSocket webSocket, int pid)
    {
        try
        {
            var process = Process.GetProcessById(pid);
            process.Kill();
            var response = Encoding.UTF8.GetBytes($"Process {pid} killed successfully.");
            await webSocket.SendAsync(new ArraySegment<byte>(response, 0, response.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (Exception e)
        {
            var errorMessage = Encoding.UTF8.GetBytes($"Error: {e.Message}");
            await webSocket.SendAsync(new ArraySegment<byte>(errorMessage, 0, errorMessage.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}