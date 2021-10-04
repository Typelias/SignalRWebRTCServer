using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using signalRtc.Models;

namespace signalRtc.Hubs
{
    public class SignalRtcHub : Hub
    {
        private IDictionary<string, List<string>> users;
        private IDictionary<string, string> socketToRoom;

        public SignalRtcHub(IDictionary<string, List<string>> users, IDictionary<string, string> socketToRoom)
        {
            this.users = users;
            this.socketToRoom = socketToRoom;
        }
        public async Task JoinRoom(string roomID)
        {

            await Groups.AddToGroupAsync(Context.ConnectionId, roomID);

            if (users.ContainsKey(roomID))
            {
                users[roomID].Add(Context.ConnectionId);

            }
            else
            {

                users.Add(roomID, new List<string>());
                users[roomID].Add(Context.ConnectionId);
            }

            socketToRoom.Add(Context.ConnectionId, roomID);


            var usersInRoom = users[roomID].FindAll(id => id != Context.ConnectionId);

            await Clients.Clients(Context.ConnectionId).SendAsync("GetId", Context.ConnectionId);
            await Clients.Group(roomID).SendAsync("AllUsers", JsonSerializer.Serialize(usersInRoom));
        }

        public async Task SendingSignal(string json)
        {
            PayLoad payload = JsonSerializer.Deserialize<PayLoad>(json);
            if (payload.callerID == null)
            {
                return;
            }
            System.Diagnostics.Debug.WriteLine("Payload:");
            System.Diagnostics.Debug.WriteLine("CallerID: "+payload.callerID);
            System.Diagnostics.Debug.WriteLine("userToSignal:"+ payload.userToSignal);
            await Clients.Client(payload.userToSignal).SendAsync("UserJoined", JsonSerializer.Serialize(payload));
        }

        public async Task ReturningSignal(string json)
        {
            PayLoad payload = JsonSerializer.Deserialize<PayLoad>(json);
            payload.userToSignal = Context.ConnectionId;
            System.Diagnostics.Debug.WriteLine("Payload:");
            System.Diagnostics.Debug.WriteLine("CallerID: " + payload.callerID);
            System.Diagnostics.Debug.WriteLine("userToSignal:" + payload.userToSignal);
            await Clients.Client(payload.callerID).SendAsync("ReceivingReturnedSignal", payload);
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            if (socketToRoom.TryGetValue(Context.ConnectionId, out var roomID))
            {
                if (users.TryGetValue(roomID, out var room))
                {
                    room = room.FindAll(id => id != Context.ConnectionId);
                }
            }

            return base.OnDisconnectedAsync(exception);
        }
    }
}
