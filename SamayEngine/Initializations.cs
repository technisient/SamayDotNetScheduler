using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Technisient.Properties;
using Technisient.SamayConfig;

namespace Technisient
{
    partial class SamayEngine
    {
        private void Initialize(Engine engine)
        {
            try
            {
                
                SamayLogger.SetLogLevel(SamayLogger.SamayEngineLoggingGUID, engine.EngineConfig.LogLevel);
                LogInfo(JsonConvert.SerializeObject(engine, Formatting.Indented));

                AppDomain ad = AppDomain.CreateDomain("SamayEnginedomain" + DateTime.Now.ToString());
                TaskFactory tf = (TaskFactory)ad.CreateInstanceAndUnwrap(
                    Assembly.GetExecutingAssembly().FullName,
                    "Technisient.TaskFactory");
                _TaskAssemblyDict = tf.Initialize();

                AppDomain.Unload(ad);
                //TaskFactory.Initialize();

           
                //at the Job level, we still use the Engine log level
                foreach (SamayConfig.Job job in engine.Jobs)
                {
                    SamayLogger.SetLogLevel(job.Id, engine.EngineConfig.LogLevel);

                    if ((job.TaskChain.Task == null) || (job.TaskChain.Task.Count() == 0))
                        continue;

                    foreach (SamayConfig.Task task in job.TaskChain.Task)
                    {
                        SamayLogger.SetLogLevel(task.Id, task.LogLevel);
                    }
                }

                //cache Global Exclude Dates
                _GlobalExcludeDates = new List<DateTime>();
                if (engine.EngineConfig.GlobalExcludeDates != null)
                    foreach (GlobalExcludeDate dt in engine.EngineConfig.GlobalExcludeDates)
                    {
                        _GlobalExcludeDates.Add(dt.Date);
                    }

                _jobExcludeDates = new Dictionary<string, List<DateTime>>();
                //cache JobExclude Dates
                foreach (Job job in engine.Jobs)
                {
                    List<DateTime> dtList = new List<DateTime>();
                    if (job.ExcludeDates != null)
                        foreach (JobExcludeDates exc in job.ExcludeDates)
                        {
                            dtList.Add(exc.Date);
                        }
                    _jobExcludeDates.Add(job.JobName, dtList);

                    // set endtime to really high number for jobs which do not run continously
                    if (job.ExecuteTimes != -1)
                        if (job.Schedules != null)
                        {
                            foreach (JobSchedule jobSchedule in job.Schedules)
                            {
                                Time t = new Time();
                                switch (jobSchedule.ScheduleConfig.GetType().Name)
                                {
                                    case "EngineJobScheduleDaily":
                                        JobScheduleDaily daily = (JobScheduleDaily)jobSchedule.ScheduleConfig;
                                        t = daily.Time;
                                        break;

                                    case "EngineJobScheduleWeekly":
                                        JobScheduleWeekly weekly = (JobScheduleWeekly)jobSchedule.ScheduleConfig;
                                        t = weekly.Day.Time;
                                        break;

                                    case "EngineJobScheduleMonthly":
                                        JobScheduleMonthly monthly = (JobScheduleMonthly)jobSchedule.ScheduleConfig;
                                        t = monthly.Day.Time;
                                        break;

                                    case "EngineJobScheduleYearly":
                                        JobScheduleYearly yearly = (JobScheduleYearly)jobSchedule.ScheduleConfig;
                                        t = yearly.Month.Time;
                                        break;

                                    case "EngineJobScheduleOneTimeOnly":
                                        JobScheduleOneTimeOnly oneTime = (JobScheduleOneTimeOnly)jobSchedule.ScheduleConfig;
                                        if (!oneTime.EndDateTimeSpecified)
                                        {
                                            oneTime.EndDateTime = oneTime.StartDateTime.AddYears(10);
                                            oneTime.EndDateTimeSpecified = true;
                                        }
                                        continue;

                                    default:
                                        throw new Exception("Unknown Schedule Type: " + jobSchedule.ScheduleConfig.GetType().Name);
                                }
                                if (!t.EndTimeSpecified)
                                {
                                    if (job.ExecuteTimes == 1)
                                        t.EndTime = t.StartTime.AddMinutes(1);
                                    else if (job.Interval.ClockTime != null)
                                    {
                                        t.EndTime = t.StartTime;
                                        int count = 0;
                                        while (count < job.ExecuteTimes)
                                        {
                                            if (job.Interval.ClockTime.Contains(t.EndTime.Minute))
                                                count++;
                                            t.EndTime = t.EndTime.AddMinutes(1);
                                        }
                                    }
                                    else
                                    {
                                        int interval = (int)job.Interval.Interval_msec;
                                        t.EndTime = t.StartTime.AddMilliseconds(interval * job.ExecuteTimes).AddMinutes(1);
                                    }
                                    t.EndTimeSpecified = true;
                                }
                            }
                        }
                }

                LogInfo("Starting Up Engine");
            }
            catch (Exception ex)
            {
                string sSource = "Technisient Samay";
                string sLog = "Samay Engine";

                if (!EventLog.SourceExists(sSource))
                    EventLog.CreateEventSource(sSource, sLog);
                EventLog.WriteEntry(sSource, ex.ToString());
            }
        }
    }     
}