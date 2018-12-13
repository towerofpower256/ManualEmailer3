using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ManualEmailer3
{
    public static class MyApp
    {
        // Output message functionality
        public delegate void OutputMessageHandler(object sender, OutputMessageEventArgs e);
        public static event OutputMessageHandler OnOutputMessage;

        public static void OutputMessage(object sender, object msg)
        {
            // If passed an array, iterate through that array
            if (msg.GetType().IsArray)
            {
                foreach (object o in (IEnumerable<object>)msg)
                {
                    OutputMessage(sender, o);
                }
                
                return; // Do nothing else
            }

            if (OnOutputMessage != null)
            {
                OnOutputMessage(sender,
                new OutputMessageEventArgs(string.Format("{0} - {1}", DateTime.Now.ToString("HH:mm:ss"), msg))
                ); // Trigger the event with these parameters, and timestamp the message
            }
            
        }

        public static void OutputMessage(object msg)
        {
            OutputMessage("", msg);
        }

        // Settings
        public static EmailToSend LoadOrNew()
        {
            if (File.Exists(Const.SETTINGS_FILENAME))
            {
                try
                {
                    OutputMessage("Loading settings");
                    XmlSerializer serializer = new XmlSerializer(typeof(EmailToSend));
                    FileStream fs = new FileStream(Const.SETTINGS_FILENAME, FileMode.Open);
                    EmailToSend ets = (EmailToSend)serializer.Deserialize(fs);
                    fs.Close();
                    return ets;
                }
                catch (Exception ex)
                {
                    OutputMessage("Error loading settings");
                    OutputMessage(ex.GetType());
                    OutputMessage(ex.Message);
                }
            }

            // Couldn't load settings, init new
            OutputMessage("No settings to load");
            return new EmailToSend();

        }

        public static void SaveSettings(EmailToSend ets)
        {
            OutputMessage("Saving settings");
            XmlSerializer serializer = new XmlSerializer(typeof(EmailToSend));
            StreamWriter sw = new StreamWriter(Const.SETTINGS_FILENAME);
            serializer.Serialize(sw, ets);
            sw.Close();
        }

        public static Exception[] RecurseException(Exception ex)
        {
            List<Exception> results = new List<Exception>();
            Exception currentException = ex;
            
            while (true)
            {
                results.Add(currentException);

                if (ex.InnerException != null)
                {
                    if (results.Contains(ex.InnerException))
                    {
                        break; // We're back at an exception we've already seen, stop recursing
                    }
                    else
                    {
                        currentException = ex.InnerException;
                        continue; //Loop again, but do the inner exception
                    }
                }
                else
                {
                    // Exception doesn't have an inner exception, stop recursing
                    break;
                }

            }

            return results.ToArray();
        }

        public static string[] RecurseExceptionAsString(Exception ex)
        {
            Exception[] exceptions = RecurseException(ex);
            string[] results = new string[exceptions.Length];
            for (int i=0; i < exceptions.Length; i++)
            {
                results[i] = string.Format("{0} - {1}", exceptions[i].GetType(), exceptions[i].Message);
            }

            return results.ToArray();
        }
    }

    public class OutputMessageEventArgs : EventArgs
    {
        public string Message { get; set; }

        public OutputMessageEventArgs(string msg)
        {
            Message = msg;
        }
    }
}
