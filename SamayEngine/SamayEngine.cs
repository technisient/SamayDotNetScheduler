#define USETASKS //multitasking!    //uncommnet for PROD
//#define ALLOW_CONCURRENT //run one job while another of same type is already running setting

using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Technisient.Properties;
using Technisient.SamayConfig;

namespace Technisient
{
    public partial class SamayEngine
    {
        #region Declarations

        private static DateTime _engineStartTime = DateTime.Now;
        private static EngineStatus _engineStatus;
        private static bool _reloadConfig = false;
        private static bool _stopEngine = false;
        private ConcurrentDictionary<string, RunningJob> _currentlyRunningJobs = new ConcurrentDictionary<string, RunningJob>();
        private List<DateTime> _GlobalExcludeDates;
        private Dictionary<string, List<DateTime>> _jobExcludeDates;
        private HashSet<DateTime> _SystemJobsLastRunDate = new HashSet<DateTime>();
        private string _tasksFolder = "";
        private Communication cm;
        private bool TASKS_IN_APPDOMAINS = true;

        public static EngineStatus Status
        {
            get { return _engineStatus; }
            set { _engineStatus = value; }
        }

        public ConcurrentDictionary<string, RunningJob> CurrentlyRunningJobs
        {
            get { return _currentlyRunningJobs; }
            set { _currentlyRunningJobs = value; }
        }

        public static DateTime EngineStartTime()
        {
            return _engineStartTime;
        }

        public class RunningJob
        {
            public bool complete = false;
            public long count = 0;
            public DateTime firstRun = new DateTime();
            public Guid jobGUID = Guid.NewGuid();
            public DateTime lastRun = new DateTime();
            public System.Threading.Tasks.Task task;

#if ALLOW_CONCURRENT
            public bool jobChainBusy = false;
#endif
        }

        #endregion Declarations

        public SamayEngine()
        {  
            //when starting from scratch
            Config.InitSamayDB(EnsureFullPath(Settings.Default.DatabasePath), GetDBConnectionString());

            SamayLogger.InitSamayLogger(GetDBConnectionString());
            SamayLogger.SetLogLevel("Startup", LogLevel.Trace);
            SamayLogger.SetLogLevel("Engine Stop", LogLevel.Trace);
            SamayLogger.SetLogLevel("Engine", LogLevel.Trace);
            cm = new Communication(); //open the communication channel
        }

        public static void ReloadConfig()
        {
            _reloadConfig = true;
        }

        public static void SafeStopEngine()
        {
            _stopEngine = true;
        }

        public void RunEngine()
        {
#if !DEBUG
            // System.Threading.Thread.Sleep(15000); //to attach when running inside service
#endif

            RunEngine(null, Config.GetSamayConfig(SamayEngine.GetDBConnectionString()));
        }

        public void RunEngine(Time testTime, Engine engine)
        {
            _engineStatus = EngineStatus.Starting;
            Initialize(engine);

            _tasksFolder = Technisient.SamayEngine.EnsureFullPath(Settings.Default.TasksDirectory);
            HashSet<string> noTaskJobs = new HashSet<string>();

            if (!engine.EngineConfig.Enabled)
            {
                LogWarning("Engine Has Been Disabled In The Configuration Settings. Engine Stopped.");
                _engineStatus = EngineStatus.Disabled;
                while (true)
                {
                    System.Threading.Thread.Sleep(1000); //simply do nothing.. keep things running so that website can report current status
                }
            }

            _engineStatus = EngineStatus.Running;
            DateTime CURRENT_DATETIME = DateTime.Now;

            if (testTime != null)
            {
                CURRENT_DATETIME = testTime.StartTime;
                TASKS_IN_APPDOMAINS = false;//app domain dont work well with test
            }

            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            while (true)
            {
                System.Threading.Thread.Sleep(new TimeSpan(engine.EngineConfig.SleepTicks));//ticks - 1 tick = 100 nanoseconds  & 10,000 ticks = 1 msec
                // done all the testing and 1 millisecond works perfectly fine without using much CPU when idle on Core2Duo machine

                try
                {
                    #region Handle Test Scenario

                    if (testTime != null)
                    {
                        CURRENT_DATETIME = CURRENT_DATETIME.AddSeconds(30);
                        if (CURRENT_DATETIME.TimeOfDay == new TimeSpan(0))
                            System.Diagnostics.Trace.WriteLine("Time: " + CURRENT_DATETIME.ToString());

                        if (CURRENT_DATETIME > testTime.EndTime)
                            return;
                        //LogInfo("Current Time is: " + CURRENT_DATETIME.ToString());
                        //System.Threading.Thread.Sleep(200);
                    }
                    else
                    {
                        CURRENT_DATETIME = DateTime.Now;//keep it constant for current run
                    }

                    #endregion Handle Test Scenario

                    #region Reload Config

                    if (_reloadConfig)
                    {
                        SamayLogger.Flush();
                        _reloadConfig = false;
                        //make all vars local
                        RunEngine(); //current while loop will stop forever
                    }

                    #endregion Reload Config

                    #region Stop Engine

                    if (_stopEngine)
                    {
                        // SamayLogger.Flush();

                        // bool nothingRunning = true;

                        // TODO: allow for safe stopping. let jobs complete.
                        //foreach (var j in _currentlyRunningJobs.Keys)
                        //{
                        //    if ((_currentlyRunningJobs[j].task.Status == TaskStatus.Running) ||
                        //        (_currentlyRunningJobs[j].task.Status == TaskStatus.WaitingForChildrenToComplete))
                        //    {
                        //        nothingRunning = false;
                        //        break;
                        //    }
                        //}

                        //if (nothingRunning)
                        ////{
                        ////    _engineStatus = EngineStatus.Stopped;
                        ////    LogInfo("Engine Stopped");
                        ////    return;
                        ////}
                        //break;
                    }

                    #endregion Stop Engine

                    #region Run System Jobs

                    if (testTime == null)
                        if (!_SystemJobsLastRunDate.Contains(CURRENT_DATETIME.Date))
                        {
                            _SystemJobsLastRunDate.Add(CURRENT_DATETIME.Date);
                            RunSystemJobs(engine);
                        }

                    #endregion Run System Jobs

                    //Reload config every 5 minutes? NO!
                    //whatever was started will keep running as is
                    foreach (Job job in engine.Jobs)
                    {
                        lock (_currentlyRunningJobs)
                        {
                            if (_stopEngine)
                                break;

                            if (!job.Enabled)
                                continue;

                            if ((job.TaskChain.Task == null) || (job.TaskChain.Task.Count() == 0))
                            {
                                if (!noTaskJobs.Contains(job.Id))
                                {
                                    //log just once
                                    LogError("Job: '" + job.JobName + "' has no Tasks defined to execute!");
                                    noTaskJobs.Add(job.Id);
                                }
                                continue;
                            }

                            if ((_jobExcludeDates.ContainsKey(job.JobName)) &&
                                (_jobExcludeDates[job.JobName].Contains(DateTime.Today)))
                            {
                                continue;
                            }

                            if (job.DoNotRunOnGlobalExcludeDates)
                            {
                                if (_GlobalExcludeDates.Contains(DateTime.Today))
                                    continue;
                            }

                            if (job.Schedules != null)
                                foreach (JobSchedule schedule in job.Schedules)
                                {
                                    #region Should Task be Run Now?

                                    if (!IsJobScheduledToRunNow(schedule, CURRENT_DATETIME))
                                    {
                                        if (_currentlyRunningJobs.ContainsKey(job.JobName + schedule.ID))
                                        {
                                            RunningJob temp = new RunningJob();
                                            _currentlyRunningJobs.TryRemove(job.JobName + schedule.ID, out temp);
                                        }
                                        continue;
                                    }

                                    // Job has passed all eligibility test! Run it now :)

                                    if (!_currentlyRunningJobs.ContainsKey(job.JobName + schedule.ID))
                                    {
                                        //This is the very first time this job is going to run in current schedule
                                        RunningJob rj = new RunningJob();
                                        rj.firstRun = CURRENT_DATETIME;
                                        _currentlyRunningJobs.TryAdd(job.JobName + schedule.ID, rj);
                                    }

                                    RunningJob runningJob = _currentlyRunningJobs[job.JobName + schedule.ID];

                                    if (runningJob.complete)
                                        continue;//cant remove it from _currentlyRunningJobs as it will be picked up on the next iteration!

#if ALLOW_CONCURRENT
                        if (!job.AllowConcurrent)
                            if (runningJob.jobChainBusy)
                                continue;
#endif

                                    if ((runningJob.count >= job.ExecuteTimes)
                                        && (job.ExecuteTimes != -1)) //-1 means keep running always
                                    {
                                        SamayLogger.LogDebug("Ending to Run Job: " + job.JobName + " Ran: " + runningJob.count.ToString() + " times",
                                            job.JobName, "Engine", job.Id, CURRENT_DATETIME);

                                        runningJob.complete = true;
                                        continue;
                                    }

                                    #endregion Should Task be Run Now?

                                    bool runNow = false;

                                    //if (!(DateTime.Now.Subtract(_currentlyRunningJobs[job.JobName].lastRun).TotalMilliseconds < job.ThrottlingDelay))
                                    if (job.ExecuteTimes == 1)
                                    {
                                        runNow = true;
                                    }
                                    else if (job.Interval.ClockTime != null)
                                    {
                                        //on the Clock
                                        if (runningJob.lastRun.ToString("g") != CURRENT_DATETIME.ToString("g"))
                                        {
                                            if (job.Interval.ClockTime.Contains(CURRENT_DATETIME.Minute))
                                            {
                                                runNow = true;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //frequency , not on clock
                                        int interval_msec = (int)job.Interval.Interval_msec;
                                        if ((CURRENT_DATETIME.Subtract(runningJob.firstRun).TotalMilliseconds >= interval_msec * runningJob.count)) //this will try and catchup
                                        {
                                            runNow = true;
                                        }
                                    }

                                    if (runNow)
                                    {
                                        //  Console.WriteLine("AppDomain: " + AppDomain.CurrentDomain.FriendlyName);

                                        runningJob.jobGUID = Guid.NewGuid();
                                        runningJob.count++;
                                        runningJob.lastRun = CURRENT_DATETIME;
                                        SamayLogger.LogInfo("Executing Job: " + job.JobName +
                                            " Count: " + runningJob.count.ToString(), job.JobName, "Engine", job.Id, CURRENT_DATETIME);
#if ALLOW_CONCURRENT
                            _currentlyRunningJobs[job.JobName].jobChainBusy = true;
#endif

                                        Job jobClone = JsonConvert.DeserializeObject<Job>(JsonConvert.SerializeObject(job),
                                              new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.All, ObjectCreationHandling = ObjectCreationHandling.Reuse }); //easy cloning
                                        // SamayLogger.Log("GUID of Task: " + jobClone.Id, job.JobName, );
                                        _currentlyRunningJobs[job.JobName + schedule.ID].task = ExecuteJob(jobClone, schedule, 0);
                                    }
                                }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex.ToString());
                }
            }

#if USETASKS
            //if (t != null)
            //    t.Wait
#endif
            //   _engineStatus = EngineStatus.Stopped;
        }

        private System.Threading.Tasks.Task ExecuteJob(Job job, JobSchedule schedule, long runCount)
        {
            //FUTURE: Spawn each Job out in a seperate exe?!
            //FUTURE: Send message to Tasks when stopped is recieve so that they can wrap up if they can
            TaskContext tc = new TaskContext();
            tc.jobName = job.JobName;
            tc.runCount = _currentlyRunningJobs[job.JobName + schedule.ID].count;
            tc.jobGUId = _currentlyRunningJobs[job.JobName + schedule.ID].jobGUID.ToString();
            tc.jobId = job.Id;

            //PLEASE ALWAYS MAKE IT EASY TO TURN OFF TASKS by using #defines as shown below
#if USETASKS
            System.Threading.Tasks.Task t = System.Threading.Tasks.Task.Factory.StartNew(() =>
#endif
            {
                AppDomain jobDomain = null;
                if (TASKS_IN_APPDOMAINS)
                {
                    System.AppDomainSetup appDomainSetup = new AppDomainSetup();
                    appDomainSetup.ShadowCopyFiles = "true";
                    appDomainSetup.ShadowCopyDirectories = _tasksFolder;
                    appDomainSetup.CachePath = _tasksFolder;

                    jobDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString(),
                        new System.Security.Policy.Evidence(AppDomain.CurrentDomain.Evidence), appDomainSetup);
                }

                try
                {
                    object prevTaskOP = null;

                    foreach (Technisient.SamayConfig.Task task in job.TaskChain.Task)
                    {
                        if (_TaskAssemblyDict.ContainsKey(task.ClassName))
                        {
                            try
                            {
                                TaskBase TASK;
                                if (TASKS_IN_APPDOMAINS)
                                {
                                    TASK = CreateInstanceInAppDomain(task.ClassName, jobDomain);
                                }
                                else
                                {
                                    TASK = (TaskBase)CreateInstance(task.ClassName);
                                }
                                if (task.param != null)
                                    //     AssignParameters(TASK, task.param);
                                    TASK.AssignParameters(GetParameters(TASK, task.param));

                                tc.logLevel = task.LogLevel;
                                tc.taskId = task.Id;
                                TASK._taskContext = tc;

                                TASK.InitListener("net.pipe://localhost/SamayEngineListener");

                                SamayLogger.LogInfo("Running " + task.ClassName, job.JobName, task.ClassName, task.Id);

                                //FUTURE: Impersonation in config
                                if (task.RetryOnError == null)
                                {
                                    //try only once
                                    prevTaskOP = TASK.Run(prevTaskOP);
                                }
                                else
                                {
                                    bool ranSucessfully = false;
                                    for (int i = 0; i < task.RetryOnError.RetryTimes; i++)
                                    {
                                        #region RetryOnError

                                        try
                                        {
                                            prevTaskOP = TASK.Run(prevTaskOP);
                                            ranSucessfully = true;
                                            break;
                                        }
                                        catch (Exception ex)
                                        {
                                            SamayLogger.LogWarning("Failed Executing " + task.ClassName + " in Job " + job.JobName +
                                                " (Attempt " + (i + 1).ToString() + " of " + task.RetryOnError.RetryTimes.ToString() + ")\n" + ex.ToString(),
                                                job.JobName, task.ClassName, task.Id);
                                        }

                                        Thread.Sleep(task.RetryOnError.RetryDelay_msec);

                                        #endregion RetryOnError
                                    }
                                    if (!ranSucessfully)
                                        throw new Exception("Aborting Execution of " + task.ClassName + " in Job " + job.JobName + " (Attempts Failed = " + task.RetryOnError.RetryTimes.ToString() + ")");
                                }
                            }
                            catch (Exception ex)
                            {
                                SamayLogger.LogError("Aborting execution of the Task. " + ex.ToString(), job.JobName, task.ClassName, task.Id);
                                if (!job.TaskChain.ContinueOnError)
                                    break;
                            }
                        }
                        else
                        {
                            SamayLogger.LogError("Unable to find assembly for " + task.ClassName + ". Task cannot be executed!", job.JobName, task.ClassName, task.Id);
                            if (!job.TaskChain.ContinueOnError)
                                break;
                        }
                    }

#if ALLOW_CONCURRENT
                if (!job.AllowConcurrent)
                    lock (_currentlyRunningJobs)
                    {
                        _currentlyRunningJobs[job.JobName].jobChainBusy = false;
                    }
#endif
                }
                catch (Exception ex)
                {
                    SamayLogger.LogError(ex.ToString(), job.JobName, "Engine", job.Id);
                }
                finally
                {
                    if (TASKS_IN_APPDOMAINS)
                    {
                        AppDomain.Unload(jobDomain);
                    }
                }
            }

#if USETASKS
, TaskCreationOptions.LongRunning// | TaskCreationOptions.PreferFairness -- LOOKS GOOD WITHOUT USING FAIRNESS -- DONT USE
                /* Long-Running Tasks:
                 * You may want to explicitly prevent a task from being put on a local queue. For example, you may know that a
                 * particular work item will run for a relatively long time and is likely to block all other work items on
                 * the local queue. In this case, you can specify the LongRunning option, which provides a hint to the scheduler
                 * that an additional thread might be required for the task so that it does not block the forward progress of
                 * other threads or work items on the local queue. By using this option you avoid the ThreadPool completely,
                 * including the global and local queues.
                 * */
   );

            return t;
#else
            return null;
#endif
        }
    }
}