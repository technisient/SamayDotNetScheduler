using System;
using System.ServiceModel;

namespace Technisient
{
    public enum EngineStatus
    {
        Starting,
        Running,
        Stopping,
        Stopped,
        Unreachable,
        Disabled
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

    [ServiceContract]
    public interface ISamayEngineListener
    {
        [OperationContract]
        bool AddJob(string job, string comment);

        [OperationContract]
        DateTime GetEngineStartTime();

        [OperationContract]
        EngineStatus GetEngineStatus();

        [OperationContract]
        string GetSamayConfig();

        [OperationContract]
        string GetVersion();

        [OperationContract]
        void Log(string logMsg, string job, string task, string id, LogLevel logLevel);

        //only used for testing!
        [OperationContract]
        void LoggerLevel(LogLevel logLevel);

        [OperationContract]
        void ReloadConfig(string requester);

        [OperationContract]
        bool RemoveJob(string jobName, string comment);

        [OperationContract]
        void SafeStopEngine(string requester);
        [OperationContract]
        bool SaveSamayConfig(string config, string comment);
    }

    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public struct TaskContext
    {
        public string jobGUId;
        public string jobId;
        public string jobName;

        public LogLevel logLevel;
        public long runCount;
        public string taskId;
    }

    /// <summary>
    ///
    /// </summary>
    public class SamaySharedLib
    {
        private ISamayEngineListener samayEngineListenerProxy;

        public SamaySharedLib()
        {
            try
            {
                System.Threading.Thread.Sleep(3000);
                InitListener("net.pipe://localhost/SamayEngineListener");
                samayEngineListenerProxy.GetSamayConfig();
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to communicate with Samay Service. Please make sure it is running.", ex);
            }
        }

        public void AddJob(string job, string comment = null)
        {
            samayEngineListenerProxy.AddJob(job, comment);
        }

        public string GetConfig()
        {
            return samayEngineListenerProxy.GetSamayConfig();
        }

        public void InitListener(string listener)
        {
            NetNamedPipeBinding npb = new NetNamedPipeBinding();

            npb.ReaderQuotas.MaxStringContentLength = 2048000;
            ChannelFactory<ISamayEngineListener> samayEngineListener = new ChannelFactory<ISamayEngineListener>(
                npb, new EndpointAddress(listener));

            samayEngineListenerProxy = samayEngineListener.CreateChannel();
        }
        public void ReloadConfig(string requestor = null)
        {
            samayEngineListenerProxy.ReloadConfig(requestor);
        }

        public void RemoveJob(string jobName, string comment = null)
        {
            samayEngineListenerProxy.RemoveJob(jobName, comment);
        }

        public bool SaveConfig(string config, string comment = null)
        {
            return samayEngineListenerProxy.SaveSamayConfig(config, comment);
        }
    }
}