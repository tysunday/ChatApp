using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.MVVM.Model
{
    public class MessageModel
    {
        public string Sender { get; set; }
        public DateTime Timestamp { get; set; }
        public string TextMessage { get; set; } // Может быть null для аудиосообщений
        public string AudioFilename { get; set; } // Может быть null для текстовых сообщений
    }
}
