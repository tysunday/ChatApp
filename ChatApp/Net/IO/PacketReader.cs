using System.Net.Sockets;
using System.Text;
using System.IO;
using NAudio.Wave;
using Microsoft.VisualBasic.Devices;

namespace ChatClient.Net.IO
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
            var length = ReadInt32();
            var msgBuffer = ReadFully(_ns, length);
            var msg = Encoding.Unicode.GetString(msgBuffer);
            return msg;
        }

        public byte[] ReadAudioMessage()
        {
            var length = ReadInt32();
            return ReadFully(_ns, length);
        }

        private byte[] ReadFully(NetworkStream stream, int length)
        {
            var buffer = new byte[length];
            int bytesRead = 0, totalBytesRead = 0;
            while (totalBytesRead < length && (bytesRead = stream.Read(buffer, totalBytesRead, length - totalBytesRead)) > 0)
            {
                totalBytesRead += bytesRead;
            }
            return buffer;
        }

        public void SaveAudioMessage(byte[] audioData)
        {
            string filepath = Guid.NewGuid().ToString() + ".wav";
            using (var waveFileWriter = new WaveFileWriter(filepath, new WaveFormat()))
            {
                waveFileWriter.Write(audioData, 0, audioData.Length);
            }
        }
    }
}
