using System;
using System.Collections.Generic;
using System.Text;
using System.Messaging;
using Technisient;

namespace JobsNS
{
    class MSMQ_Receive : TaskBase
    {
        public override object Run(object input)
        {
            System.Messaging.MessageQueue mq = new System.Messaging.MessageQueue(queueName);
            System.Messaging.Message msg;
            msg = mq.Receive(new TimeSpan(0, 0,timeout));
            msg.Formatter = new XmlMessageFormatter(new String[] { "System.String,mscorlib" });
            
          LogDebug("Dequed: " + msg.Body.ToString());
            return msg.Body.ToString();
        }

        string queueName;

        [SamayParameter(LabelText="Queue Name", IsRequired= true, 
            Help="This is the queue name you want to dequeue from", Index=0)]
        public string QueueName
        {
            get { return queueName; }
            set { queueName = value; }
        }

        int timeout;

        [SamayParameter (DefaultValue="15", LabelText="Timeout (seconds)", 
            IsRequired=false,Help="MSMQ Receive timeout", Index=1)]
        public int Timeout
        {
            get { return timeout; }
            set { timeout = value; }
        }


    }
}
