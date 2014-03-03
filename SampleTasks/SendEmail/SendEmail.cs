using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using Technisient;

namespace JobsNS
{
    class SendEmail : TaskBase
    {
        string emailTo;
        [SamayParameter(LabelText = "Email To", IsRequired = true, Index = 2, Regex = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$",
            ValidationMessage = "Please set a valid email address")]
        public string EmailTo
        {
            get { return emailTo; }
            set { emailTo = value; }
        }


        string emailFrom;
        [SamayParameter(LabelText = "Email From", IsRequired = true, Index = 1, Regex = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$",
            ValidationMessage = "Please set a valid email address")]
        public string EmailFrom
        {
            get { return emailFrom; }
            set { emailFrom = value; }
        }

        string subject;
        [SamayParameter(LabelText = "Subject", IsRequired = true, Index = 3)]
        public string Subject
        {
            get { return subject; }
            set { subject = value; }
        }

        string body;
        [SamayParameter(LabelText = "Body", IsRequired = true, Index = 4)]
        public string Body
        {
            get { return body; }
            set { body = value; }
        }

        string smtpServer;
        [SamayParameter(LabelText="SMTP Server Name", DefaultValue="localhost", Index=0,
            IsRequired=true)]
        public string SmtpServer
        {
            get { return smtpServer; }
            set { smtpServer = value; }
        }

        public override object Run(object input)
        {
            MailMessage mail = new MailMessage();
            mail.To.Add(emailTo);
            mail.From = new MailAddress(emailFrom);

            mail.Subject = subject;
            mail.Body = body;

            //send the message
            SmtpClient smtp = new SmtpClient(smtpServer);
            smtp.Send(mail);
            return true;
        }
    }
}
