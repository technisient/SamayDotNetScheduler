using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.ServiceModel;

namespace Technisient
{

    /// <summary>
    /// 
    /// </summary>
    public class SamaySharedLib
    {
        ISamayEngineListener samayEngineListenerProxy;

        public SamaySharedLib()
        {
            try
            {
                System.Threading.Thread.Sleep(3000);
                InitListener("net.pipe://localhost/SamayEngineListener");
                samayEngineListenerProxy.GetSamayConfig();
            }
            catch(Exception ex)
            {
                throw new Exception("Unable to communicate with Samay Service. Please make sure it is running.", ex);
            }
        }

        public void InitListener(string listener)
        {
            NetNamedPipeBinding npb = new NetNamedPipeBinding();

            npb.ReaderQuotas.MaxStringContentLength = 2048000;
            ChannelFactory<ISamayEngineListener> samayEngineListener = new ChannelFactory<ISamayEngineListener>(
                npb, new EndpointAddress(listener));

            samayEngineListenerProxy = samayEngineListener.CreateChannel();
        }

        public string GetConfig()
        {
            return samayEngineListenerProxy.GetSamayConfig();
        }

        public bool SaveConfig(string config, string comment=null)
        {
            return samayEngineListenerProxy.SaveSamayConfig(config, comment);
        }

        public void ReloadConfig(string requestor = null)
        {
            samayEngineListenerProxy.ReloadConfig(requestor);
        }

        public void AddJob(string job, string comment = null)
        {
            samayEngineListenerProxy.AddJob(job, comment);
        }

        public void RemoveJob(string jobName, string comment = null)
        {
            samayEngineListenerProxy.RemoveJob(jobName, comment);
        }

    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public struct TaskContext
    {
        public string jobName;

        public long runCount;
        public LogLevel logLevel;
        public string jobId;
        public string taskId;
        public string jobGUId;
    }


    /// <summary>
    /// Log Levels
    /// </summary>
    public enum LogLevel
    {
        Trace,
        Debug,
        Info,
        Warn,
        Error,
        Fatal
    }

    public enum EngineStatus
    {
        Starting,
        Running,
        Stopping,
        Stopped,
        Unreachable,
        Disabled
    }

    [ServiceContract]
    public interface ISamayEngineListener
    {
        [OperationContract]
        void Log(string logMsg, string job, string task, string id, LogLevel logLevel);

        //only used for testing!
        [OperationContract]
        void LoggerLevel(LogLevel logLevel);

        [OperationContract]
        void SafeStopEngine(string requester);

        [OperationContract]
        void ReloadConfig(string requester);

        [OperationContract]
        EngineStatus GetEngineStatus();

        [OperationContract]
        DateTime GetEngineStartTime();

        [OperationContract]
        string GetVersion();

        [OperationContract]
        string GetSamayConfig();

        [OperationContract]
        bool SaveSamayConfig(string config, string comment);

        [OperationContract]
        bool AddJob(string job, string comment);

        [OperationContract]
        bool RemoveJob(string jobName, string comment);

    }
}