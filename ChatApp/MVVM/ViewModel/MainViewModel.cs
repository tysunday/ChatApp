using ChatClient.MVVM.Core;
using ChatClient.MVVM.Model;
using ChatClient.Net;
using NAudio.Wave;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ChatClient.MVVM.ViewModel
{
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
        private string _buttonContent = "Start Record";
        public string ButtonContent
        {
            get => _buttonContent;
            set
            {
                _buttonContent = value;
                OnPropertyChanged(nameof(ButtonContent));
            }
        }
        private string _buttonColor = "Green";
        public string ButtonColor
        {
            get => _buttonColor;
            set
            {
                _buttonColor = value;
                OnPropertyChanged(nameof(ButtonColor));
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

            ConnectToServerCommand = new RelayCommand(o => _server.ConnectToServer(Username), o => !string.IsNullOrEmpty(Username));
            SendMessageCommand = new RelayCommand(o => SendMessage(), o => !string.IsNullOrEmpty(Message));
            RecordAudioMessageCommand = new RelayCommand(o => RecordAudio());
            PlayAudioCommand = new RelayCommand(o => PlayAudio((MessageModel)o));
        }
        private void SendMessage()
        {
            _server?.SendMessageToServer(Message);
            Message = string.Empty;
        }

        private async Task RecordAudio()
        {
            if (AC.waveSource == null)
            {
                AC.StartRecordAudio();
                ButtonContent = "Stop Record";
                ButtonColor = "Red";
            }
            else
            {
                AC.StopRecord();
                var filePath = AC.GetRecordedAudioFilePath();
                if (File.Exists(filePath))
                {
                    var audioBytes = AC.GetRecordedAudioBytes(filePath);
                    ButtonContent = "Start Record";
                    ButtonColor = "Green";
                    await _server.SendAudioMessageToServer(audioBytes);
                    File.Delete(filePath);
                }
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