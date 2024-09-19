using System.Net.WebSockets;
using System.Text;

namespace Core.Controllers.WsActions;

public static class WsActionsUtils
{
    private static List<int> _numbers = [];
    
    public static async Task EchoMessages(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        if (!result.CloseStatus.HasValue)
        {
            var clientMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var serverMessage = Encoding.UTF8.GetBytes($"Echo: {clientMessage}");
            await webSocket.SendAsync(new ArraySegment<byte>(serverMessage, 0, serverMessage.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
        }
    }
    
    public static async Task SendFile(WebSocket webSocket, string filePath)
    {
        var buffer = new byte[1024 * 4];
        await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        int bytesRead;
        while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, bytesRead), WebSocketMessageType.Binary, bytesRead < buffer.Length, CancellationToken.None);
        }
    }
    

    public static async Task CountTo100(WebSocket webSocket)
    {
        _numbers.Clear();
        
        var tasks = new List<Task>();

        for (var i = 0; i < 10; i++)
        {
            var start = i * 10 + 1;
            var end = start + 9;
            tasks.Add(Task.Run(() => AddNumbers(start, end)));
        }

        await Task.WhenAll(tasks);

        var serverMessage = Encoding.UTF8.GetBytes($"Numbers: {string.Join(", ", _numbers)}");
        await webSocket.SendAsync(new ArraySegment<byte>(serverMessage, 0, serverMessage.Length), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private static void AddNumbers(int start, int end)
    {
        for (var i = start; i <= end; i++)
        {
            _numbers.Add(i);
        }
    }
}