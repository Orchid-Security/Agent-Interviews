using Microsoft.AspNetCore.Mvc;

namespace Core.Controllers.WsActions;

[ApiController]
[Route("ws")]
public class WsActionsApiController: ControllerBase
{
    [HttpGet]
    [Route("echo")]
    public async Task Echo()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await WsActionsUtils.EchoMessages(webSocket);
        }
        else
        {
            HttpContext.Response.StatusCode = 400;
        }
    }
    
    [HttpGet]
    [Route("sendfile")]
    public async Task SendFile([FromQuery] string filePath)
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await WsActionsUtils.SendFile(webSocket, filePath);
        }
        else
        {
            HttpContext.Response.StatusCode = 400;
        }
    }
    
    [HttpGet]
    [Route("countto100")]
    public async Task CountTo100()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await WsActionsUtils.CountTo100(webSocket);
        }
        else
        {
            HttpContext.Response.StatusCode = 400;
        }
    }
}