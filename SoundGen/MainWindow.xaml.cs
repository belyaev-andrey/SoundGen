using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

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
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "CSV Tables|*.csv|All files|*.*";
            dlg.Multiselect = false;
            var line = String.Empty;
            if (dlg.ShowDialog().GetValueOrDefault(false))
            {
                FileNameLbl.Text = dlg.FileName;
                StreamReader reader = new StreamReader(dlg.OpenFile(), Encoding.GetEncoding("windows-1251"));
                while ((line = reader.ReadLine()) != null)
                {
                    LinesText.AppendText(line);
                    LinesText.AppendText(Environment.NewLine);
                }
                reader.Close();

                var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(25000, 4);
                WaveFileWriter waveFileWriter = new WaveFileWriter(dlg.FileName+".wav", waveFormat);
                float amplitude = 1f;
                float frequency = 1000;
                
                for (int n = 0; n < waveFileWriter.WaveFormat.SampleRate*4; n++)
                {
                    float sample = (float)(amplitude * Math.Sin((2 * Math.PI * n * frequency) / waveFileWriter.WaveFormat.SampleRate));
                    waveFileWriter.WriteSample(sample);
                }
                
                waveFileWriter.Close();
                
            }
        }
    }
}