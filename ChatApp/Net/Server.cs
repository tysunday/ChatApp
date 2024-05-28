using ChatClient.Net.IO;
using System.Net.Sockets;

namespace ChatClient.Net
{
    class Server
    {
        TcpClient _client;

        public PacketReader PacketReader;

        public event Action connectedEvent;
        public event Action msgReceivedEvent;
        public event Action userDisconnectEvent;
        public event Action audioMsgReceivedEvent;

        public Server()
        {
            _client = new TcpClient();
        }

        public void ConnectToServer(string username)
        {
            if (!_client.Connected)
            {
                _client.Connect("127.0.0.1", 7891);
                PacketReader = new PacketReader(_client.GetStream());
                if (!string.IsNullOrEmpty(username))
                {
                    var connectPacket = new PacketBuilder();
                    connectPacket.WriteOpCode(0);
                    connectPacket.WriteMessage(username);
                    _client.Client.Send(connectPacket.GetPacketBytes());
                }
                ReadPackets();
            }
        }

        private void ReadPackets()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    var opcode = PacketReader.ReadByte();
                    switch (opcode)
                    {
                        case 1:
                            connectedEvent?.Invoke();
                            break;

                        case 5:
                            msgReceivedEvent?.Invoke();
                            break;

                        case 7:
                            audioMsgReceivedEvent?.Invoke();
                            break;

                        case 10:
                            userDisconnectEvent?.Invoke();
                            break;

                        default:
                            Console.WriteLine("ah yes...");
                            break;
                    }
                }
            });
        }
        public async Task SendMessageToServer(string message)
        {
            var messagePacket = new PacketBuilder();
            messagePacket.WriteOpCode(5);
            messagePacket.WriteMessage(message);
            await _client.Client.SendAsync(messagePacket.GetPacketBytes());
        }
        public async Task SendAudioMessageToServer(byte[] audioBytes)
        {
            var audioPacket = new PacketBuilder();
            audioPacket.WriteOpCode(7);
            audioPacket.WriteAudioMessage(audioBytes);
            await _client.Client.SendAsync(audioPacket.GetPacketBytes());
        }
    }
}
