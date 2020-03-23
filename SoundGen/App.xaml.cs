using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using log4net;
using log4net.Config;

namespace SoundGen
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

        }

    }
}