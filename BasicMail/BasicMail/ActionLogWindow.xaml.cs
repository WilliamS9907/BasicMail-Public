using BasicMail.Generic;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using static BasicMailSharedPublicStructs.PublicStructs;

namespace BasicMail
{
    /// <summary>
    /// Interaction logic for ActionLogWindow.xaml
    /// </summary>
    public partial class ActionLogWindow : Window
    {
        public ActionLogWindow()
        {
            InitializeComponent();

            this.DataContext = Globals.mainView;

            logEntries_listBox.ItemsSource = Globals.mainView.Actions;
        }

        private void openLog_button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog()
            {
                InitialDirectory = Globals.mainView.LogsPath
            };

            fileDialog.DefaultExt = ".json";

            Nullable<bool> result = fileDialog.ShowDialog();

            try
            {
                if (result.HasValue
                    && result.Value)
                {
                    logEntries_listBox.SelectedIndex = 0;
                    logEntry_textBlock.Text = "";

                    logEntries_listBox.ItemsSource = JsonConvert.DeserializeObject<ObservableCollection<ActionLogEntry>>(File.ReadAllText(fileDialog.FileName!))!;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Action Log could not be loaded." + "\n\n" +
                                "Please be sure you've selected an appropriate action log and try again.",
                                "Invalid Action Log", 
                                MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void logEntries_listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (logEntries_listBox.SelectedItem != null)
            {
                ActionLogEntry logEntry = (ActionLogEntry)logEntries_listBox.SelectedItem;

                logEntry_textBlock.Text = "Title: " + logEntry.title + "\n" +
                                          "From: " + logEntry.from + "\n" +
                                          "To: " + logEntry.to + "\n" +
                                          "Cc: " + logEntry.cc + "\n" +
                                          "Bcc: " + logEntry.bcc + "\n" +
                                          "Subject: " + logEntry.subject + "\n" +
                                          "Status: " + logEntry.status + "\n" +
                                          "Body: \n" + logEntry.body + "\n\n" +
                                          "StackTrace: \n" + logEntry.stack;
            }
        }
    }
}
