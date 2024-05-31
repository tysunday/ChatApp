using System.IO;
using System.Text;

namespace ChatClient.Net.IO
{
    class PacketBuilder
    {
        MemoryStream _ms;
        public PacketBuilder()
        {
            _ms = new MemoryStream();
        }

        public void WriteOpCode(byte opcode)
        {
            _ms.WriteByte(opcode);
        }

        public void WriteMessage(string msg)
        {
            var msgBytes = Encoding.Unicode.GetBytes(msg);
            var msgLength = msgBytes.Length;
            _ms.Write(BitConverter.GetBytes(msgLength), 0, 4);
            _ms.Write(msgBytes, 0, msgBytes.Length);
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
