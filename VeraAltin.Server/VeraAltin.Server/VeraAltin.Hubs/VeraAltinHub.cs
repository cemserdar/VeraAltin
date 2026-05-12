using Microsoft.AspNetCore.SignalR;

namespace VeraAltin.Server.Hubs
{
    public class VeraAltinHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"[SignalR] Yeni bir bağlantı sağlandı: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }
    }
}