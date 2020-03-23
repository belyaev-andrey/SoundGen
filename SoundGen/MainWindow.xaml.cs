using System;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Windows;
using Microsoft.Win32;

namespace SoundGen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly WavFileGenerator _wavFileGenerator = new WavFileGenerator();
        private readonly BackgroundWorker _backgroundWorker = new BackgroundWorker();

        public MainWindow()
        {
            InitializeComponent();

            CoeffList.ItemsSource = new[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
            CoeffList.SelectedValue = 1;

            _backgroundWorker.DoWork += OnPerformFileGeneration;
            _backgroundWorker.RunWorkerCompleted += OnFileGenerationCompleted;
        }

        private void OnPerformFileGeneration(object sender, DoWorkEventArgs args)
        {
            if (args.Argument == null) return;
            var (fileName, divider) = (Tuple<string, int>) args.Argument;
            _wavFileGenerator.WriteSoundData(fileName, divider);
        }
        
        private void OnFileGenerationCompleted (object sender, RunWorkerCompletedEventArgs args)
        {
            if (!args.Cancelled)
            {
                MessageBox.Show("File: " + _wavFileGenerator.FileName + " is generated. File size: " + _wavFileGenerator.FileSize + " bytes",
                    "File Generated", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("File: " + _wavFileGenerator.FileName + ". Generation Cancelled",
                    "File Generation Cancelled", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void OpenFileBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var fileName = GetCsvFileName();
            if (fileName != null)
            {
                FileNameLbl.Text = fileName;
               _backgroundWorker.RunWorkerAsync(new Tuple<string, int>(fileName, (int) CoeffList.SelectedValue));
            }
        }

        private string GetCsvFileName()
        {
            var dlg = new OpenFileDialog
                {Filter = "CSV Tables|*.csv|All files|*.*", Multiselect = false, CheckFileExists = true};
            
            if (dlg.ShowDialog().GetValueOrDefault(false))
            {
                return dlg.FileName;
            }
            return null;
        }

        private void CancelBtn_OnClick(object sender, RoutedEventArgs e)
        {
            _backgroundWorker.CancelAsync();
        }
    }
}