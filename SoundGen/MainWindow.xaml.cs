using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using log4net;
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
        
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MainWindow()
        {
            InitializeComponent();

            CoeffList.ItemsSource = new[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
            CoeffList.SelectedValue = 1;

            GenerationProgressBar.Minimum = 0;
            GenerationProgressBar.Maximum = 100;

            _backgroundWorker.WorkerSupportsCancellation = true;
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.DoWork += OnPerformFileGeneration;
            _backgroundWorker.RunWorkerCompleted += OnFileGenerationCompleted;
            _backgroundWorker.ProgressChanged += OnGenerationProgressUpdate;
            Log.Info("Application Started");
        }

        private void OnPerformFileGeneration(object sender, DoWorkEventArgs args)
        {
            Log.Info("Starting Wav file generation, argument: "+args.Argument);
            if (args.Argument == null) return;
            var (fileName, divider) = (Tuple<string, int>) args.Argument;
            args.Result = _wavFileGenerator.WriteSoundData(fileName, divider, sender as BackgroundWorker, args);
        }

        private void OnGenerationProgressUpdate(object sender, ProgressChangedEventArgs args)
        {
            GenerationProgressBar.Value = args.ProgressPercentage;
        }

        private void OnFileGenerationCompleted(object sender, RunWorkerCompletedEventArgs args)
        {
            if (args.Error != null)
            {
                Log.Error("Error in file generation", args.Error);
                
                MessageBox.Show(
                    "File generation error: " + args.Error.Message,
                    "File Generation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (args.Cancelled)
            {
                Log.Info("Generation was cancelled");
                MessageBox.Show("File Generation Cancelled",
                    "File Generation Cancelled", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else if (args.Result is WavFileGenerationResult result)
            {
                Log.Info("Generation successful, file name is "+result.FileName);
                MessageBox.Show(
                    "File: " + result.FileName + " is generated. File size: " + result.FileSize + " bytes",
                    "File Generated", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                Log.Error("Unspecified File Generation Error. File Generation Result is not valid: \""+args.Result+"\"");
                MessageBox.Show(
                    "Unspecified file generation error: result is invalid. Please consider log files",
                    "File Generation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenFileBtn_OnClick(object sender, RoutedEventArgs e)
        {
            GenerationProgressBar.Value = 0;
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

        private void CancelBtn_OnClick(object sender, RoutedEventArgs args)
        {
            Log.Info("Cancelling wav file generation, sender is \""+sender+"\"");
            _backgroundWorker.CancelAsync();
        }
    }
}