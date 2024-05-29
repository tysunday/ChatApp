using System.IO;
using System.Text;

namespace ChatClient.Net.IO
{
    public class PacketBuilder
    {
        private MemoryStream _ms;
        private BinaryWriter _writer;

        public PacketBuilder()
        {
            _ms = new MemoryStream();
            _writer = new BinaryWriter(_ms);
        }
        public void WriteOpCode(byte opcode)
        {
            _ms.WriteByte(opcode);
        }
        public void WriteMessage(string msg)
        {
            var msgLength = msg.Length;
            _ms.Write(BitConverter.GetBytes(msgLength));
            _ms.Write(Encoding.ASCII.GetBytes(msg));
        }

        public void WriteAudioMessage(byte[] audioMsg)
        {
            var audioLength = audioMsg.Length;
            _ms.Write(BitConverter.GetBytes(audioLength), 0, 4);
            _ms.Write(audioMsg, 0, audioLength);
        }

        public byte[] GetPacketBytes()
        {
            return _ms.ToArray();
        }
    }
}
