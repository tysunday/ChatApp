using ChatServer.Net.IO;
using System.Net.Sockets;

namespace ChatServer
{
    class Client
    {
        public string Username { get; set; }
        public Guid UID { get; set; }
        public TcpClient ClientSocket { get; set; }

        PacketReader _packetReader;

        public Client(TcpClient client)
        {
            ClientSocket = client;
            UID = Guid.NewGuid();
            _packetReader = new PacketReader(ClientSocket.GetStream());
            var opcode = _packetReader.ReadByte();
            Username = _packetReader.ReadMessage();

            Console.WriteLine($"[{DateTime.Now}]: Client has connected with the username: {Username}");

            Task.Run(() => Process());
        }

        async Task Process()
        {
            while (true)
            {
                try
                {
                    var opcode = _packetReader.ReadByte();
                    switch (opcode)
                    {
                        case 5:
                            var msg = _packetReader.ReadMessage();
                            Console.WriteLine($"[{DateTime.Now}]: Message received! {msg}");
                            await Program.BroadcastMessage($"[{Username}]: {msg}");
                            break;
                        case 7:
                            var audioMsg = _packetReader.ReadAudioMessage();
                            Console.WriteLine($"[{DateTime.Now}]: Audio message received!");
                            await Program.BroadcastMessage($"[{Username}]: send audio message.");
                            await Program.BroadcastAudioMessage(audioMsg);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine($"[{UID}]: Disconnected!");
                    await Program.BroadcastDiconnect(UID.ToString());
                    ClientSocket.Close();
                    break;
                }
            }
        }
    }
}
