using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Xml;
using Technisient.SamayConfig;
using Newtonsoft.Json;

namespace Technisient
{
    class Communication
    {
        ServiceHost host;
        public Communication()
        {
            NetNamedPipeBinding npb = new NetNamedPipeBinding();
            npb.ReaderQuotas.MaxStringContentLength = 2048000;

            host = new ServiceHost(typeof(SamayEngineListener),
                new Uri[]{
                    //new Uri("http://localhost:49710"), 
                    new Uri("net.pipe://localhost")
                         });
           
            host.AddServiceEndpoint(typeof(ISamayEngineListener), npb, "SamayEngineListener");
            //   host.AddServiceEndpoint(typeof(ILogger), new BasicHttpBinding(), "Log");

            host.Open();
        }

        ~Communication()
        {
            host.Close();
        }
    }


    public class SamayEngineListener : Technisient.ISamayEngineListener
    {
        public void Log(string logMsg, string job, string task, string id, LogLevel logLevel)
        {
            SamayLogger.Log(logMsg, job, task, id, logLevel, DateTime.Now);
        }


        public void SafeStopEngine(string requester)
        {
            SamayLogger.LogInfo("Engine Stop initiated by " + requester, "Engine Stop", null, "Engine Stop");
            SamayEngine.SafeStopEngine();
        }

        public EngineStatus GetEngineStatus()
        {
            return SamayEngine.Status;
        }

        public DateTime GetEngineStartTime()
        {
            return SamayEngine.EngineStartTime();
        }

        public string GetSamayConfig()
        {
            return JsonConvert.SerializeObject(Config.GetSamayConfig(SamayEngine.GetDBConnectionString()), Newtonsoft.Json.Formatting.Indented);
        }

        public bool SaveSamayConfig(string config, string comment)
        {
            return Config.SaveSamayConfig(JsonConvert.DeserializeObject<Engine>(config), comment, SamayEngine.GetDBConnectionString()); 
        }

        public void LoggerLevel(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }


        public string GetVersion()
        {
            return "2.1";
        }


        public void ReloadConfig(string requester)
        {
            SamayLogger.LogInfo("Engine Config Reload initiated by " + requester, "Engine Stop", null, "Engine Stop");
            SamayEngine.ReloadConfig();
        }


        public bool AddJob(string job, string comment)
        {
            return Config.AddJob(job, comment, SamayEngine.GetDBConnectionString());
        }

        public bool RemoveJob(string jobName, string comment)
        {
            return Config.RemoveJob(jobName, comment, SamayEngine.GetDBConnectionString());
        }


    }
}
