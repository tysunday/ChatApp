using NAudio.Wave;
using System.Net.Sockets;
using System.Text;

namespace ChatServer.Net.IO
{
    class PacketReader : BinaryReader
    {
        private NetworkStream _ns;
        public PacketReader(NetworkStream ns) : base(ns)
        {
            _ns = ns;
        }
        public string ReadMessage()
        {
            byte[] msgBuffer;
            var length = ReadInt32();
            msgBuffer = new byte[length];
            _ns.Read(msgBuffer, 0, length);
            var msg = Encoding.ASCII.GetString(msgBuffer);
            return msg;

        }
        public byte[] ReadAudioMessage()
        {
            var length = ReadInt32();
            var audioBuffer = new byte[length];
            _ns.Read(audioBuffer, 0, length);
            return audioBuffer;
        }
        public void SaveAudioMessage(byte[] audioData)
        {
            string filepath = Guid.NewGuid().ToString();
            using (var waveFileWriter = new WaveFileWriter((filepath), new WaveFormat()))
            {
                waveFileWriter.Write(audioData, 0, audioData.Length);
            }
        }
    }
}
