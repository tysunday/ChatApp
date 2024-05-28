using NAudio.Wave;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace ChatClient
{
    public class AudioClass
    {
        public WaveInEvent waveSource = null;
        private WaveFileWriter waveFile = null;
        private string outputFileName = null;
        List<WaveFileWriter> waveFiles;

        public string OutputFileName
        {
            get { return outputFileName; }
            set { outputFileName = value; }
        }
        public List<WaveFileWriter> WaveFiles
        {
            get { return waveFiles; }
            set { waveFiles = value; }
        }

        public AudioClass()
        {
            WaveFiles = new List<WaveFileWriter>();
        }

        public string GetFileDuration(WaveFileWriter waveFile)
        {
            try
            {
                if (waveFile != null)
                {
                    waveFile.Dispose(); // здесь освобождаем ресурсы чтобы им можно было воспользоваться далее или что-то типа такого

                    using (WaveFileReader reader = new WaveFileReader(waveFile.Filename))
                    {
                        TimeSpan duration = reader.TotalTime;
                        return duration.ToString(@"hh\:mm\:ss");
                    }
                }
                return "N/A";
            }
            catch (NAudio.MmException naudioex)
            {
                MessageBox.Show($"Ошибка в MmException: {naudioex.Message}");
                return "N/A";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка в Exception: {ex.Message}");
                return "N/A";
            }
        }

        //public void StartRecordAudio(List<WaveFileWriter> WaveFile = null)
        //{
        //    try
        //    {
        //        if (waveSource == null)
        //        {
        //            OutputFileName = "audio#" + Guid.NewGuid().ToString();

        //            waveSource = new WaveInEvent();
        //            waveSource.WaveFormat = new WaveFormat(44100, 1);
        //            waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_DataAvailable);
        //            waveSource.RecordingStopped += new EventHandler<StoppedEventArgs>(waveSource_RecordingStopped);

        //            waveFile = new WaveFileWriter(OutputFileName, waveSource.WaveFormat);


        //            if (WaveFiles != null)
        //                WaveFiles.Add(waveFile);

        //            waveSource.StartRecording();
        //        }
        //        else
        //        {
        //            MessageBox.Show("Запись уже в процессе.");
        //        }
        //    }
        //    catch (NAudio.MmException naudioex)
        //    {
        //        MessageBox.Show($"Ошибка в MmException: {naudioex.Message}");
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Ошибка в Exception: {ex.Message}");
        //    }
        //}
        public void StartRecordAudio(ObservableCollection<WaveFileWriter> WaveFile = null)
        {
            try
            {
                if (waveSource == null)
                {
                    OutputFileName = $"audio_{Guid.NewGuid()}.wav";

                    waveSource = new WaveInEvent();
                    waveSource.WaveFormat = new WaveFormat(44100, 1);
                    waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_DataAvailable);
                    waveSource.RecordingStopped += new EventHandler<StoppedEventArgs>(waveSource_RecordingStopped);

                    waveFile = new WaveFileWriter(OutputFileName, waveSource.WaveFormat);


                    WaveFiles?.Add(waveFile);

                    waveSource.StartRecording();
                }
                else
                {
                    MessageBox.Show("Запись уже в процессе.");
                }
            }
            catch (NAudio.MmException naudioex)
            {
                MessageBox.Show($"Ошибка в MmException: {naudioex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка в Exception: {ex.Message}");
            }
        }

        public void StopRecord()
        {
            try
            {

                waveSource?.StopRecording();

            }
            catch (NAudio.MmException naudioex)
            {
                MessageBox.Show($"Ошибка в MmException: {naudioex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка в Exception: {ex.Message}");
            }
        }

        public void PlayAudio()
        {
            try
            {
                WaveFileReader fileReader = new WaveFileReader(OutputFileName);
                WaveOutEvent waveOut = new WaveOutEvent();
                waveOut.Init(fileReader);
                do
                {
                    waveOut.Play();
                }
                while (waveOut.PlaybackState == PlaybackState.Playing);

                waveOut.Dispose();
                waveOut = null;
            }
            catch (NAudio.MmException naudioex)
            {
                MessageBox.Show($"Ошибка в MmException: {naudioex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка в Exception: {ex.Message}");
            }
        }

        private void waveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            try
            {

                waveFile?.Write(e.Buffer, 0, e.BytesRecorded);
                waveFile?.Flush();

            }
            catch (NAudio.MmException naudioex)
            {
                MessageBox.Show($"Ошибка в MmException: {naudioex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка в Exception: {ex.Message}");
            }
        }
        private void waveSource_RecordingStopped(object sender, StoppedEventArgs e)
        {
            try
            {

                waveSource?.Dispose();
                waveSource = null;

                waveFile?.Dispose();
                waveFile = null;

            }
            catch (NAudio.MmException naudioex)
            {
                MessageBox.Show($"Ошибка в MmException: {naudioex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка в Exception: {ex.Message}");
            }
        }
        public byte[] GetRecordedAudioBytes(string outputFileName = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(outputFileName) && File.Exists(outputFileName))
                {
                    return File.ReadAllBytes(outputFileName);
                }
                else
                {
                    MessageBox.Show("Файл записи не найден.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при чтении файла: {ex.Message}");
                return null;
            }
        }
        public string GetRecordedAudioFilePath()
        {
            return OutputFileName;
        }

    }
}
