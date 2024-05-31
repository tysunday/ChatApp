using System.Net.Sockets;
using System.Net;
using ChatServer.Net.IO;

namespace ChatServer
{
    internal class Program
    {
        static List<Client> _users;
        static TcpListener _listener;

        static async Task Main(string[] args)
        {
            _users = new List<Client>();
            _listener  = new TcpListener(IPAddress.Parse("127.0.0.1"), 7891);
            _listener.Start();
            while (true)
            {
                var client = new Client(_listener.AcceptTcpClient());
                _users.Add(client);
                /*Broadcast the connection to everyone on the server*/
               await BroadcastConnection();
            }
        }
        static async Task BroadcastConnection()
        {
            foreach(var user in _users)
            {
                foreach(var usr in _users)
                {
                    var broadcastPacket = new PacketBuilder();
                    broadcastPacket.WriteOpCode((byte)OperationCodes.ConnectedToServer);
                    broadcastPacket.WriteMessage(usr.Username);
                    broadcastPacket.WriteMessage(usr.UID.ToString());
                    await user.ClientSocket.Client.SendAsync(broadcastPacket.GetPacketBytes());
                }
            }
        }
        public static async Task BroadcastMessage(string message)
        {
            foreach(var user in _users)
            {
                var msgPacket = new PacketBuilder();
                msgPacket.WriteOpCode((byte)OperationCodes.MsgReceived);
                msgPacket.WriteMessage(message);
                await user.ClientSocket.Client.SendAsync(msgPacket.GetPacketBytes());
            }
        }

        public static async Task BroadcastAudioMessage(byte[] audioBytes)
        {
            foreach(var user in _users)
            {
                var audioPacket = new PacketBuilder();
                audioPacket.WriteOpCode((byte)OperationCodes.AudioMessageReceived);
                audioPacket.WriteAudioMessage(audioBytes);
                await user.ClientSocket.Client.SendAsync(audioPacket.GetPacketBytes());
            }
        }
        public static async Task BroadcastDiconnect(string uid)
        {
            var disconnectedUser = _users.Where(x => x.UID.ToString() == uid).FirstOrDefault();
            _users.Remove(disconnectedUser);
            foreach (var user in _users)
            {
                var broadcastPacket = new PacketBuilder();
                broadcastPacket.WriteOpCode((byte)OperationCodes.UserDisconnected);
                broadcastPacket.WriteMessage(uid);
                await user.ClientSocket.Client.SendAsync(broadcastPacket.GetPacketBytes());
            }

            await BroadcastMessage($"[{disconnectedUser.Username}] Disconnected!");
        }


    }
}
