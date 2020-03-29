using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using log4net;
using Microsoft.Win32;

namespace SoundGen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private WavFileGenerator _wavFileGenerator;
        private readonly BackgroundWorker _backgroundWorker = new BackgroundWorker();
        
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MainWindow()
        {
            InitializeComponent();

            CoeffList.ItemsSource = new[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
            CoeffList.SelectedValue = 1;

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
            var (fileName, divider, reverse) = (Tuple<string, int, bool>) args.Argument;
            args.Result = _wavFileGenerator.WriteSoundData(divider, reverse, sender as BackgroundWorker, args);
        }

        private void OnGenerationProgressUpdate(object sender, ProgressChangedEventArgs args)
        {
            GenerationProgressBar.Value = args.ProgressPercentage;
        }

        private void OnFileGenerationCompleted(object sender, RunWorkerCompletedEventArgs args)
        {
            GenParamsPanel.IsEnabled = true;
            CancelBtn.IsEnabled = false;
            OpenFileBtn.IsEnabled = true;
            
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
            GenParamsPanel.IsEnabled = (fileName != null);
            FileNameLbl.Content = fileName;
            if (fileName != null)
            {
                _wavFileGenerator = new WavFileGenerator(fileName);
            }
        }

        #nullable enable
        private string? GetCsvFileName()
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

        private void GenerateBtn_OnClick(object sender, RoutedEventArgs e)
        {
            GenParamsPanel.IsEnabled = false;
            CancelBtn.IsEnabled = true;
            OpenFileBtn.IsEnabled = false;
            var isChecked = ReverseCheckBox.IsChecked ?? false;
            _backgroundWorker.RunWorkerAsync(new Tuple<string, int, bool>(_wavFileGenerator.FileName, (int) CoeffList.SelectedValue, isChecked));
        }
    }
}