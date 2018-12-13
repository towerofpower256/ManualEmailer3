using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ManualEmailer3
{
    /// <summary>
    /// DTO Class, represents the content and configuration of an email to be sent.
    /// </summary>
    [Serializable]
    public class EmailToSend : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string MessageContent { get; set; }
        public bool MessageImportFromFile { get; set; }
        public bool MessageEncodeNewlines { get; set; }
        public string MessageContentFilePath { get; set; }
        public string MessageFrom { get; set; }
        public string MessageTo { get; set; }
        public string MessageCC { get; set; }
        public string MessageBCC { get; set; }
        public string MessageSubject { get; set; }
        public ObservableCollection<AttachmentToLoad> MessageAttachmentsToLoad { get; set; }
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public bool SmtpUseSsl { get; set; }
        public SmtpAuthMethod SmtpAuthMethod { get; set; }
        public string SmtpAuthUsername { get; set; }

        [XmlIgnore] // Do not include the password when serializing
        public SecureString SmtpAuthPassword { get; set; }

        

        public EmailToSend()
        {
            MessageAttachmentsToLoad = new ObservableCollection<AttachmentToLoad>();

            // Defaults
            SmtpPort = 25;
            SmtpUseSsl = false;
            SmtpAuthMethod = SmtpAuthMethod.NoAuthentication;
            SmtpAuthPassword = new SecureString();
        }

        public void AddAttachment(AttachmentToLoad att)
        {
            MessageAttachmentsToLoad.Add(att);
            OnPropertyChanged("MessageAttachmentsToLoad");
        }

        public void AddAttachment(string att)
        {
            AddAttachment(new AttachmentToLoad(att));
        }

        public void ClearAttachments()
        {
            MessageAttachmentsToLoad.Clear();
            OnPropertyChanged("MessageAttachmentsToLoad");
        }

        public void RemoveAttachment(AttachmentToLoad att)
        {
            if (MessageAttachmentsToLoad.Contains(att))
            {
                MessageAttachmentsToLoad.Remove(att);
            }
            OnPropertyChanged("MessageAttachmentsToLoad");
        }


        // Create the OnPropertyChanged method to raise the event
        private void OnPropertyChanged(string name)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }


    }

    [Serializable]
    public class AttachmentToLoad
    {
        public string Path { get; set; }
        public string Name { get; set; }

        public AttachmentToLoad() { }

        public AttachmentToLoad(string path)
        {
            Path = path;
            Name = System.IO.Path.GetFileName(path);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
