using ChatClient.MVVM.Core;
using ChatClient.MVVM.Model;
using ChatClient.Net;
using ChatClient.Net.IO;
using NAudio.Wave;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace ChatClient.MVVM.ViewModel
{
    public class AudioMessageModel
    {
        public string Filename { get; set; }
        public string Duration { get; set; }
    }

    class MainViewModel
    {
        private AudioClass AC;

        public ObservableCollection<UserModel> Users { get; set; }
        public ObservableCollection<string> Messages { get; set; }
        public ObservableCollection<AudioMessageModel> AudioMessages { get; set; }


        public string Username { get; set; }
        public string Message { get; set; }

        private Server _server;

        public RelayCommand ConnectToServerCommand { get; set; }
        public RelayCommand SendMessageCommand { get; set; }
        public RelayCommand RecordAudioMessageCommand { get; set; }
        public RelayCommand PlayAudioCommand { get; set; }


        public MainViewModel()
        {
            AC = new AudioClass();
            AudioMessages = new ObservableCollection<AudioMessageModel>();
            Users = new ObservableCollection<UserModel>();
            Messages = new ObservableCollection<string>();
            _server = new Server();
            _server.connectedEvent += UserConnected;
            _server.msgReceivedEvent += MessageReceived;
            _server.userDisconnectEvent += RemoveUser;
            _server.audioMsgReceivedEvent += audioMessageReceived;

            ConnectToServerCommand = new RelayCommand(o => _server.ConnectToServer(Username), o => !string.IsNullOrEmpty(Username));
            SendMessageCommand = new RelayCommand(o => _server.SendMessageToServer(Message), o => !string.IsNullOrEmpty(Message));
            RecordAudioMessageCommand = new RelayCommand(o => RecordAudio());
            PlayAudioCommand = new RelayCommand(o => PlayAudio((AudioMessageModel)o));
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
                MessageBox.Show("Запись окончена.");
                var filePath = AC.GetRecordedAudioFilePath();
                if (File.Exists(filePath))
                {
                    var audioBytes = AC.GetRecordedAudioBytes(filePath);
                    await _server.SendAudioMessageToServer(audioBytes);

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

        private void PlayAudio(AudioMessageModel audioMessage)
        {
            var fileReader = new WaveFileReader("ReceivedAudio\\" + audioMessage.Filename);
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
            Application.Current.Dispatcher.Invoke(() => Messages.Add(msg));
        }

        private void audioMessageReceived()
        {
            var audioMsg = _server.PacketReader.ReadAudioMessage();
            string filePath = $"ReceivedAudio\\Audio{MyFunc.GetFormattedTime()}.wav";

            File.WriteAllBytes(filePath, audioMsg);

            var audioMessage = new AudioMessageModel
            {
                Filename = Path.GetFileName(filePath),
                Duration = GetAudioDuration(filePath)
            };
            Application.Current.Dispatcher.Invoke(() => AudioMessages.Add(audioMessage));
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
    }
}
