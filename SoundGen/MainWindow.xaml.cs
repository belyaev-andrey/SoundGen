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
            var reader = new StreamReader(dlg.OpenFile(), Encoding.GetEncoding("windows-1251"));
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                LinesText.AppendText(line);
                LinesText.AppendText(Environment.NewLine);
            }
            reader.Close();

            var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(25000, 4);
            var waveFileWriter = new WaveFileWriter(dlg.FileName+".wav", waveFormat);
            const float amplitude = 1f;
            const float frequency = 1000;
                
            for (var n = 0; n < waveFileWriter.WaveFormat.SampleRate*4; n++)
            {
                var sample = (float)(amplitude * Math.Sin((2 * Math.PI * n * frequency) / waveFileWriter.WaveFormat.SampleRate));
                waveFileWriter.WriteSample(sample);
            }
                
            waveFileWriter.Close();
        }
    }
}