using System;
using System.IO;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using NAudio.Wave;

namespace SoundGen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        const short bitsPerSampleQmLab = 16; //Important for QMLab 
        const byte dataTypeQmLab = 1;

        public MainWindow()
        {
            InitializeComponent();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        private void OpenFileBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog {Filter = "CSV Tables|*.csv|All files|*.*", Multiselect = false};
            if (!dlg.ShowDialog().GetValueOrDefault(false)) return;
            FileNameLbl.Text = dlg.FileName;
            var fileName = dlg.FileName;
            using (var reader = new StreamReader(dlg.OpenFile(), Encoding.GetEncoding("windows-1251")))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    LinesText.AppendText(line);
                    LinesText.AppendText(Environment.NewLine);
                }
            }

            int sampleRate = 44100;
            int lengthInSeconds = 1;
            short channels = 1;
            
            var header = CreateHeader(channels, sampleRate);
            BinaryWriter writer = new BinaryWriter(new FileStream(fileName+".wav", FileMode.Create));
            
            int totalSamples = sampleRate * lengthInSeconds;
            int fileLenBytes = (totalSamples * channels * bitsPerSampleQmLab)/8 + 44;

            header = AddFileLengthToHeader(header, fileLenBytes);
            writer.Write(header);
            
            for (int i = 0; i < totalSamples; i++)
            {
                var s = (short) i;
                Console.WriteLine(s);
                byte[] sample = BitConverter.GetBytes(s);
                writer.Write(sample);
            }
            
            writer.Close();
        }

        private static byte[] CreateHeader(short channels, int smplRate)
        {
            byte[] header = new byte[44];
            //RIFF
            header[0] = Convert.ToByte('R');
            header[1] = Convert.ToByte('I');
            header[2] = Convert.ToByte('F');
            header[3] = Convert.ToByte('F');
            //SIZE, 4 bytes, 0 for now
            for (int i = 4; i <= 7; i++) 
            {
                header[i] = 0;
            }
            //WAVE
            header[8] = Convert.ToByte('W');
            header[9] = Convert.ToByte('A');
            header[10] = Convert.ToByte('V');
            header[11] = Convert.ToByte('E');
            //fmt
            header[12] = Convert.ToByte('f');
            header[13] = Convert.ToByte('m');
            header[14] = Convert.ToByte('t');
            header[15] = Convert.ToByte(' ');
            //Header length, 4 bytes, 16 - 19 
            byte[] headerLen = BitConverter.GetBytes(16);
            Array.Copy(headerLen, 0, header, 16, 4);
            //Data Type, 2 bytes - 20 - 21
            byte[] dataType = BitConverter.GetBytes((byte)dataTypeQmLab);
            Array.Copy(dataType, 0, header, 20, 2);
            //Channels, 2 bytes - 22 - 23
            byte[] channelsNum = BitConverter.GetBytes(channels);
            Array.Copy(channelsNum, 0, header, 22, 2);
            //Discretization, Hz, 4 bytes, 24 - 27
            byte[] sampleRate = BitConverter.GetBytes(smplRate);
            Array.Copy(sampleRate, 0, header, 24, 4);
            //(Sample Rate * BitsPerSample * Channels) / 8,  4 bytes, 28 - 31
            byte[] coeff = BitConverter.GetBytes((smplRate * bitsPerSampleQmLab * channels) / 8);
            Array.Copy(coeff, 0, header, 28, 4);
            //(BitsPerSample * Channels) / 8,  2 bytes, 32 - 33
            byte[] bytesForAllChannels = BitConverter.GetBytes( (short)((bitsPerSampleQmLab * channels) / 8));
            Array.Copy(bytesForAllChannels, 0, header, 32, 2);
            //Bits per sample, 2 bytes, 34 - 35
            byte[] bitsPerSampleArr = BitConverter.GetBytes(bitsPerSampleQmLab);
            Array.Copy(bitsPerSampleArr, 0, header, 34, 2);
            //data, 
            header[36] = Convert.ToByte('d');
            header[37] = Convert.ToByte('a');
            header[38] = Convert.ToByte('t');
            header[39] = Convert.ToByte('a');
            //File size, 4 bytes
            return header;
        }

        private static byte[] AddFileLengthToHeader(byte[] header, int fileSize)
        {
            byte[] newHeader = new byte[44];
            header.CopyTo(newHeader, 0);
            byte[] fileWithHeader = BitConverter.GetBytes(fileSize - 8);
            Array.Copy(fileWithHeader, 0, newHeader, 4, 4);
            byte[] fileData = BitConverter.GetBytes(fileSize - 44);
            Array.Copy(fileData, 0, newHeader, 40, 4);
            return newHeader;
        }
    }
}