using System;   
using System.Collections.Generic;
using System.Text;
using System.Messaging;
using Technisient;

namespace JobsNS
{
    class MSMQSend : TaskBase
    {
        public override object Run(object input)
        {
           // string qn = queueName;// @".\private$\testqueue1";// taskContext.taskParams["QueueName"][0];
            string myMessage = "Hello World " + DateTime.Now.ToString();
            MessageQueue messageQueue = null;
            Message oMsg = null;
            
            //Sending Messages to MSMQ
            if (MessageQueue.Exists(queueName))
            {
                messageQueue = new MessageQueue(queueName);

                oMsg = new Message(myMessage);
                messageQueue.Send(oMsg);

                //LogFatal("Fatal Enqueued: " + myMessage);
                //LogError("Error Enqueued: " + myMessage);
                //LogWarning("Warn Enqueued: " + myMessage);
                LogInfo("Info Enqueued: " + myMessage);
                //LogDebug("Debug Enqueued: " + myMessage);
                //LogTrace("Trace Enqueued: " + myMessage);
            }

            return true;

            //else
            //{
            //    //Create the MessageQueue 
            //    messageQueue = MessageQueue.Create(queueName);
            //    //Set Queue permission
            //    //messageQueue.SetPermissions("Everyone", MessageQueueAccessRights.FullControl,
            //    //                             AccessControlEntryType.Set);
            //    messageQueue.SetPermissions("Everyone",
            //                        MessageQueueAccessRights.PeekMessage |
            //                        MessageQueueAccessRights.ReceiveMessage |
            //                        MessageQueueAccessRights.DeleteMessage |
            //                        MessageQueueAccessRights.WriteMessage,
            //                        AccessControlEntryType.Set);

            //    oMsg = new Message(myMessage);
            //    messageQueue.Send(oMsg);
            //}
        }

        private string queueName;
       
        [SamayParameter(LabelText = "Queue Name",
            IsRequired = true, Help = "This is the queue name you want to dequeue from")]     
        public string QueueName
        {
            get { return queueName; }
            set { queueName = value; }
        }

    }
}
