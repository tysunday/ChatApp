using ChatClient.MVVM.Core;
using ChatClient.MVVM.Model;
using ChatClient.Net;
using ChatClient.Net.IO;
using NAudio.Wave;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Windows;

namespace ChatClient.MVVM.ViewModel
{

    public class MessageModel
    {
        public string Sender { get; set; }
        public DateTime Timestamp { get; set; }
        public string TextMessage { get; set; } // Может быть null для аудиосообщений
        public string AudioFilename { get; set; } // Может быть null для текстовых сообщений
    }

    public class AudioMessageModel
    {
        public string Filename { get; set; }
        public string Duration { get; set; }
    }

    class MainViewModel : INotifyPropertyChanged
    {
        private AudioClass AC;

        public ObservableCollection<MessageModel> Messages { get; set; }
        public ObservableCollection<UserModel> Users { get; set; }
        public string _message;
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        public string Username { get; set; }

        private Server _server;

        public RelayCommand ConnectToServerCommand { get; set; }
        public RelayCommand SendMessageCommand { get; set; }
        public RelayCommand RecordAudioMessageCommand { get; set; }
        public RelayCommand PlayAudioCommand { get; set; }


        public MainViewModel()
        {
            AC = new AudioClass();
            Messages = new ObservableCollection<MessageModel>();
            Users = new ObservableCollection<UserModel>();
            _server = new Server();
            _server.connectedEvent += UserConnected;
            _server.msgReceivedEvent += MessageReceived;
            _server.userDisconnectEvent += RemoveUser;
            _server.audioMsgReceivedEvent += AudioMessageReceived;
            ;

            ConnectToServerCommand = new RelayCommand(o => _server.ConnectToServer(Username), o => !string.IsNullOrEmpty(Username));
            SendMessageCommand = new RelayCommand(o => SendMessage(), o => !string.IsNullOrEmpty(Message));
            RecordAudioMessageCommand = new RelayCommand(o => RecordAudio());
            PlayAudioCommand = new RelayCommand(o => PlayAudio((MessageModel)o));
        }

        private void SendMessage()
        {
            _server.SendMessageToServer(Message);
            //Messages.Add(new MessageModel { Sender = Username, Timestamp = DateTime.Now, TextMessage = Message });
            Message = string.Empty;
        }

        private async Task RecordAudio()
        {
            if (AC.waveSource == null)
            {
                AC.StartRecordAudio();
            }
            else
            {
                AC.StopRecord();
                var filePath = AC.GetRecordedAudioFilePath();
                if (File.Exists(filePath))
                {
                    var audioBytes = AC.GetRecordedAudioBytes(filePath);
                    await _server.SendAudioMessageToServer(audioBytes);

                    //Messages.Add(new MessageModel
                    //{
                    //    Sender = Username,
                    //    Timestamp = DateTime.Now,
                    //    AudioFilename = Path.GetFileName(filePath)
                    //});

                    File.Delete(filePath);
                }
            }
        }

        private string GetAudioDuration(string filePath)
        {
            using (var reader = new AudioFileReader(filePath))
            {
                return reader.TotalTime.ToString(@"hh\:mm\:ss");
            }
        }

        private void PlayAudio(MessageModel audioMessage)
        {
            var fileReader = new WaveFileReader("ReceivedAudio\\" + audioMessage.AudioFilename);
            var waveOut = new WaveOutEvent();
            waveOut.Init(fileReader);
            waveOut.Play();
        }

        private void RemoveUser()
        {
            var uid = _server.PacketReader.ReadMessage();
            var user = Users.FirstOrDefault(x => x.UID == uid);
            Application.Current.Dispatcher.Invoke(() => Users.Remove(user));
        }

        private void MessageReceived()
        {
            var msg = _server.PacketReader.ReadMessage();
            Application.Current.Dispatcher.Invoke(() => Messages.Add(new MessageModel { Sender = "Server", Timestamp = DateTime.Now, TextMessage = msg }));
        }

        private void AudioMessageReceived()
        {
            var audioMsg = _server.PacketReader.ReadAudioMessage();
            string filePath = $"ReceivedAudio\\Audio{MyFunc.GetFormattedTime()}.wav";
            File.WriteAllBytes(filePath, audioMsg);

            var audioMessage = new MessageModel
            {
                Sender = "Server",
                Timestamp = DateTime.Now,
                AudioFilename = Path.GetFileName(filePath)
            };
            Application.Current.Dispatcher.Invoke(() => Messages.Add(audioMessage));
        }

        private void UserConnected()
        {
            var user = new UserModel
            {
                UserName = _server.PacketReader.ReadMessage(),
                UID = _server.PacketReader.ReadMessage()
            };
            if (!Users.Any(x => x.UID == user.UID))
            {
                Application.Current.Dispatcher.Invoke(() => Users.Add(user));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

