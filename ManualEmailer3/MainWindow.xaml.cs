using System;
using System.Collections.Generic;
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
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.IO;

using Microsoft.Win32;

namespace ManualEmailer3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private EmailToSend ets = new EmailToSend();
        private Task _sendingEmailJob;

        public MainWindow()
        {
            InitializeComponent();
            InitSmtpAuthOptions();

            // Capture output messages
            MyApp.OnOutputMessage += new MyApp.OutputMessageHandler(OnOutputMessage);

            //Try loading settings
            ets = MyApp.LoadOrNew();
            this.DataContext = ets;

            // Update the SMTP Auth field visibility
            UpdateSmtpAuthVisibility();

            MyApp.OutputMessage("Program started");
        }

        private void OnOutputMessage(object sender, OutputMessageEventArgs e)
        {
            UpdateOutputConsole(e.Message);
        }

        private void UpdateOutputConsole(string msg)
        {
            //Making thread-safe call
            Dispatcher.Invoke(() =>
            {
                OutputConsoleText.AppendText(string.Format("{0}\n", msg));
                OutputConsoleText.ScrollToEnd();
            });
        }

        private void UpdateSmtpAuthVisibility()
        {
            Visibility visible = Visibility.Visible;
            if (ets.SmtpAuthMethod != SmtpAuthMethod.BasicAuthentication)
                visible = Visibility.Hidden;


            SmtpUsernameText.Visibility = visible;
            SmtpPasswordText.Visibility = visible;
        }

        



        private void SaveOutputConsole()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = string.Format("saved output {0}.txt", DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
            sfd.OverwritePrompt = true;
            if (sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, OutputConsoleText.Text);
            }
        }


        private void SetSending(bool setSending)
        {
            //Making thread-safe call
            Dispatcher.Invoke(() =>
            {
                SendEmailButton.IsEnabled = !setSending;
                SendEmailButton.Content = setSending ? "Sending..." : "Send email";
            });
            
        }

        private void InitSmtpAuthOptions()
        {
            //SmtpAuthMethodCombo.ItemsSource = Enum.GetValues(typeof(SmtpAuthMethod)).Cast<SmtpAuthMethod>();

            /*
            SmtpAuthMethodCombo.ItemsSource = Enum.GetValues(typeof(SmtpAuthMethod))
                .Cast<SmtpAuthMethod>()
                .Select(value => new
                {
                    (Attribute.GetCustomAttribute(value.GetType().GetField(value.ToString()), typeof(DescriptionAttribute)) as DescriptionAttribute).Description,
                    value
                })
                .OrderBy(item => item.value)
                .ToList();
            */
        }

        // Check function for number-only text fields
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Button_SendEmail(object sender, RoutedEventArgs e)
        {
            MyApp.SaveSettings(ets);
            SetSending(true); //Set sending, lock the Send Email button

            //Get ready to send the email in the background
            Task newTask = new Task(() =>
            {
                try
                {
                    MyEmailSender mysender = new MyEmailSender();
                    ets.SmtpAuthPassword = SmtpPasswordText.SecurePassword; //Grab the password
                    mysender.SendEmail(ets);
                }
                catch (Exception ex)
                {
                    MyApp.OutputMessage("=== ERROR SENDING EMAIL! ===");
                    MyApp.OutputMessage(MyApp.RecurseExceptionAsString(ex));
                }
                finally
                {
                    // Clear out the SMTP auth password
                    if (ets.SmtpAuthPassword != null) ets.SmtpAuthPassword.Clear();
                }
            });

            newTask.ContinueWith((task) => {
                SetSending(false); // Re-enable the form
            });

            //Go
            newTask.Start();
        }


    private void Button_AddAttachment(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            ofd.Multiselect = false;
            ofd.ValidateNames = true;
            if (ofd.ShowDialog() == true)
            {
                ets.AddAttachment(ofd.FileName);
            }

        }

        private void Button_ClearAttachments(object sender, RoutedEventArgs e)
        {
            ets.ClearAttachments();
        }

        private void Button_RemoveAttachment(object sender, RoutedEventArgs e)
        {
            ets.RemoveAttachment( ((Button)sender).DataContext as AttachmentToLoad);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MyApp.SaveSettings(ets);
        }

        private void SmtpAuthMethodCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSmtpAuthVisibility();
        }

        private void SaveConsoleButton_Click(object sender, RoutedEventArgs e)
        {
            SaveOutputConsole();
        }
    }
}
