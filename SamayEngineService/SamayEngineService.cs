using System.Diagnostics;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace Technisient
{
    public partial class SamayEngineService : ServiceBase
    {
        private EventLog eventLog = new EventLog();

        public SamayEngineService()
        {
            InitializeComponent();
            if (!System.Diagnostics.EventLog.SourceExists("Technisient Samay"))
                System.Diagnostics.EventLog.CreateEventSource("Technisient Samay",
                                                                      "Samay Engine");

            eventLog.Source = "Technisient Samay";// the event log source by which the application is registered on the computer

            eventLog.Log = "Samay Engine";

            //try
            //{
            //}
            //catch (Exception ex)
            //{
            //    eventLog.WriteEntry(ex.ToString());
            //}
        }

        protected override void OnShutdown()
        {
            Technisient.SamayEngine.SafeStopEngine();
            base.OnShutdown();
        }

        protected override void OnStart(string[] args)
        {
            eventLog.WriteEntry("Samay Engine Service Started");

            base.OnStart(args);

            Technisient.SamayEngine samayEngine = new SamayEngine();
            System.Threading.Tasks.Task t = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    samayEngine.RunEngine();
                }, TaskCreationOptions.LongRunning);
        }

        protected override void OnStop()
        {
            Technisient.SamayEngine.SafeStopEngine();
            // eventLog.WriteEntry("Samay Engine Service Stoped");
            //while ((Technisient.SamayEngine.Status == EngineStatus.Running) ||
            //    (Technisient.SamayEngine.Status != EngineStatus.Stopping))
            //{
            //    System.Threading.Thread.Sleep(2000);
            //}
            base.OnStop();
        }
    }
}