using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Controllers;

namespace WebApplication1.Services
{
    public class DataNotifier
    {
        private readonly IHubContext<DataUpdatesHub> _hubContext;
        private Dictionary<string, List<Tuple<string, int>>> _connectionIdsListeningTo = new Dictionary<string, List<Tuple<string, int>>>();
        private Dictionary<Tuple<string, int>, List<string>> _objectsSubscribedByConnectionIds = new Dictionary<Tuple<string, int>, List<string>>();

        public DataNotifier(IHubContext<DataUpdatesHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task EntityUpdated(string entityType, int id)
        {
            var tuple = Tuple.Create(entityType, id);
            if (_objectsSubscribedByConnectionIds.TryGetValue(tuple, out var connectionIds))
            {
                await _hubContext.Clients.Clients(connectionIds)
                    .SendAsync("DataUpdated", entityType, id);
            }
        }

        internal void SubscribeToDataUpdates(string entityType, IList<int> ids, string connectionId)
        {
            foreach (var id in ids)
            {
                var tuple = Tuple.Create(entityType, id);

                _connectionIdsListeningTo.TryAdd(connectionId, new List<Tuple<string, int>>());
                var listeningTo = _connectionIdsListeningTo[connectionId];
                if (!listeningTo.Contains(tuple))
                {
                    listeningTo.Add(tuple);
                }

                _objectsSubscribedByConnectionIds.TryAdd(tuple, new List<string>());
                var connectionIds = _objectsSubscribedByConnectionIds[tuple];
                if (!connectionIds.Contains(connectionId))
                {
                    connectionIds.Add(connectionId);
                }
            }
        }

        internal void ClientDisconnected(string connectionId)
        {
            if (_connectionIdsListeningTo.TryGetValue(connectionId, out var tuples))
            {
                _connectionIdsListeningTo.Remove(connectionId);
                foreach (var item in tuples)
                {
                    _objectsSubscribedByConnectionIds[item].Remove(connectionId);
                }
            }
        }
    }
}
