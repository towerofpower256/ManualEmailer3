using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Mail;
using System.Net;
using System.ComponentModel;

namespace ManualEmailer3
{
    public class MyEmailSender
    {
        public void SendEmail(EmailToSend ets)
        {
            Msg($"=== Starting email send {DateTime.Now} ===");

            string _messageContent = ets.MessageContent; // TODO: Just read the content provided right now, I'll do the import thing later.
            List<Attachment> attachments;

            Msg("Assembling message");
            MailMessage email = new MailMessage(
                new MailAddress(ets.MessageFrom),
                new MailAddress(ets.MessageTo)
                );

            if (!string.IsNullOrWhiteSpace(ets.MessageTo)) { foreach (string t in ets.MessageTo.Split((char)';')) { email.To.Add(t); } }
            if (!string.IsNullOrWhiteSpace(ets.MessageCC)) { foreach (string t in ets.MessageCC.Split((char)';')) { email.CC.Add(t); } }
            if (!string.IsNullOrWhiteSpace(ets.MessageBCC)) { foreach (string t in ets.MessageBCC.Split((char)';')) { email.Bcc.Add(t); } }

            email.Subject = ets.MessageSubject;

            // TODO: Is this really nessessary? If importing from an HTML file, you'd think it wouldn't be needed
            email.Body = string.Format("<html><body>\r\n{0}\r\n</body></head>", _messageContent);
            email.IsBodyHtml = true;

            Msg("Grabbing attachments, if any");

            attachments = LoadAttachments(ets.MessageAttachmentsToLoad);

            if (attachments != null)
            {
                foreach (Attachment att in attachments) email.Attachments.Add(att); // Can't AddRange and can't construct your own AttachmentCollection, gotta add them 1 by 1
            }

            //It's go time!!!
            Msg("Setting up the SMTP Client");
            SmtpClient smtp = new SmtpClient(ets.SmtpHost, ets.SmtpPort);
            smtp.EnableSsl = ets.SmtpUseSsl;

            // SMTP auth
            switch(ets.SmtpAuthMethod)
            {
                case SmtpAuthMethod.BasicAuthentication:
                    Msg("Using basic SMTP auth");
                    smtp.Credentials = new NetworkCredential(ets.SmtpAuthUsername, ets.SmtpAuthPassword);
                    break;
                case SmtpAuthMethod.DefaultAuthentication:
                    Msg("Using default / current user credentials for SMTP auth");
                    smtp.UseDefaultCredentials = true;
                    break;
                default:
                    Msg("Not using SMTP authentication");
                    break;
            }

            Msg("Sending email");
            smtp.Send(email);

            Msg("Email has been sent!");

            
        }

        private List<Attachment> LoadAttachments(IEnumerable<AttachmentToLoad> attachmentsToLoad)
        {
            List<Attachment> attachments = new List<Attachment>();

            foreach (AttachmentToLoad att in attachmentsToLoad)
            {
                Msg(string.Format("Loading attachment: {0}", att.Name));

                //For each attachment, add it to the list of Attachments
                Attachment newAttachment = new Attachment(att.Path);

                // TODO do content ID stuff here
                string contentID = Path.GetFileName(att.Name);
                newAttachment.ContentId = contentID;
                newAttachment.ContentDisposition.Inline = true;
                newAttachment.ContentDisposition.DispositionType = System.Net.Mime.DispositionTypeNames.Inline;

                //Add it to the list
                attachments.Add(newAttachment);
            }

            return attachments;
        }

        private void Msg(params object[] parms)
        {
            foreach (object o in parms)
            MyApp.OutputMessage(this, o);
        }

    }
}
