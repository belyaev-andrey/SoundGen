using System;
using System.Text;
using System.Windows;
using Microsoft.Win32;

namespace SoundGen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private FileConverter _fileConverter = new FileConverter();
        
        public MainWindow()
        {
            InitializeComponent();

            CoeffList.ItemsSource = new[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
            CoeffList.SelectedValue = 1;
            
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        private void OnLineProcessed(object source, EventArgs args)
        {
            
        }

        private void OnWavGenerated(object source, EventArgs args)
        {
            MessageBox.Show(this, "The file was generated", "File Generation", MessageBoxButton.OK);
        }

        
        private void OpenFileBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog {Filter = "CSV Tables|*.csv|All files|*.*", Multiselect = false};
            if (!dlg.ShowDialog().GetValueOrDefault(false)) return;
            _fileConverter.OnLineProcessed += OnLineProcessed;
            _fileConverter.OnWavGenerated += OnWavGenerated;
            FileNameLbl.Text = dlg.FileName;
            var fileName = dlg.FileName;
            _fileConverter.WriteSoundData(fileName, (int)CoeffList.SelectedValue);
        }

    }
}