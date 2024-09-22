using System.Net.WebSockets;
using System.Text;
using Core.Handlers;
using Newtonsoft.Json.Linq;

namespace Core;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseWebSockets();
        app.UseMiddleware<WebSocketMiddleware>();
        
        app.UseRouting();
        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}

public class WebSocketMiddleware
{
    private readonly RequestDelegate _next;

    public WebSocketMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await HandleWebSocketRequest(webSocket);
        }
        else
        {
            await _next(context);
        }
    }

    private async Task HandleWebSocketRequest(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by the WebSocket client", CancellationToken.None);
            }
            else
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var json = JObject.Parse(message);
                var eventParam = json["event"].ToString();
                var dataParam = json["data"] as  JObject;

                await RouteEvent(webSocket, eventParam, dataParam);
            }
        }
    }
    
    private async Task RouteEvent(WebSocket webSocket, string eventParam, JObject data)
    {
        switch (eventParam)
        {
            case "echo":
                var message = data["message"]!.ToString();
                await WsActionsUtils.EchoMessages(webSocket, message);
                break;
            case "count-nums":
                await WsActionsUtils.CountTo100(webSocket);
                break;
            case "list-files":
                var pathParam = data["path"]!.ToString();
                var maxDepth = int.Parse(data["maxDepth"]!.ToString());
                await WsActionsUtils.ListFiles(webSocket, pathParam, maxDepth);
                break;
            case "send-file":
                var filePath = data["filePath"]!.ToString();
                await WsActionsUtils.SendFile(webSocket, filePath);
                break;
            case "file-exist":
                var path = data["path"]!.ToString();
                await WsActionsUtils.CheckFileOrDirExists(webSocket, path);
                break;
            case "zip-dir":
                var sourceDir = data["sourceDir"]!.ToString();
                await WsActionsUtils.ZipAndSendDirectory(webSocket, sourceDir);
                break;
            case "processes":
                await WsActionsUtils.GetAllProcesses(webSocket);
                break;
            case "process-info":
                var pid = int.Parse(data["pid"]!.ToString());
                await WsActionsUtils.GetProcessInfo(webSocket, pid);
                break;
            case "terminate-process":
                var terminationPid = int.Parse(data["pid"]!.ToString());
                await WsActionsUtils.TerminateProcess(webSocket, terminationPid);
                break;
            default:
                await SendErrorMessage(webSocket, "Unknown event");
                break;
        }
    }
    
    private async Task SendErrorMessage(WebSocket webSocket, string message)
    {
        await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes($"Error: {message}")), WebSocketMessageType.Text, true, CancellationToken.None);
    }
}