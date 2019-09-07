using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    public interface IDataUpdatedNotifierClient
    {
        Task SubscribeToDataUpdates(string entityType, List<int> ids);
        Task DataUpdated(string entityType, int id);
        Task SendMessage(string message);
        Task ReceiveMessage(string message);
    }

    public class DataUpdatesHub : Hub<IDataUpdatedNotifierClient>
    {
        private readonly DataNotifier _dataNotifier;

        public DataUpdatesHub(DataNotifier dataNotifier)
        {
            _dataNotifier = dataNotifier;
        }

        public async Task SubscribeToDataUpdates(string entityType, List<int> ids)
        {
            _dataNotifier.SubscribeToDataUpdates(entityType, ids, this.Context.ConnectionId);
        }


        public async Task SendMessage(string message)
        {
            await Clients.All.ReceiveMessage(message);
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            _dataNotifier.ClientDisconnected(this.Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

    }
}
