using BasicMail.Generic;
using BasicMailSharedClasses;
using System.Configuration;
using System.Data;
using System.Reflection;
using System.Windows;

namespace BasicMail
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            InstantiateExceptionHandler();
        }

        public void InstantiateExceptionHandler()
        {
            this.Dispatcher.Invoke(() =>
            {
                AppDomain.CurrentDomain.UnhandledException += (s, e) => HandleUnhandledException(Globals.mainView.LogsPath, (Exception)e.ExceptionObject);
            });

            DispatcherUnhandledException += (s, e) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    HandleUnhandledException(Globals.mainView.LogsPath, e.Exception);

                    e.Handled = true;
                });
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    HandleUnhandledException(Globals.mainView.LogsPath, e.Exception);

                    e.SetObserved();
                });
            };
        }

        public static void HandleUnhandledException(String logsPath, Exception ex)
        {
            Globals.mainView.Actions.Add(Common.GetActionLogEntry("UNHANDLED EXCEPTION", ex.Message + "\n" + ex.StackTrace));

            MessageBox.Show("Basic Mail has encountered an unhandled exception." + "\n\n" +
                            @"This exception has been logged and can be viewed in Options -> Action Log (or at BaseDirectory\BasicMail\Logs)" + "\n\n" +
                            "Please click OK to attempt to maintain integrity and continue...",
                            "Unhandled Exception",
                            MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

}
