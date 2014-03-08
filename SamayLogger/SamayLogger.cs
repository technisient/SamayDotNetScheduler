//#define DEBUG_NLOG

using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;

namespace Technisient
{
    //*************************************************************
    //
    //  TRACE <  DEBUG < INFO < WARN < ERROR < FATAL
    //
    //*************************************************************
    //this should always be STATIC for Performance. Done want to intantiate this all the time!
    public static class SamayLogger
    {
        public const string SamayEngineLoggingGUID = "00000000-0000-0000-0000-000000000000";
        public const string SamayEngineLogJobName = "Samay Engine";//this is used for logging in the 'Job' column
        //this should always be STATIC for Performance. Done want to intantiate this all the time!

        private static NLog.Config.LoggingConfiguration config = new NLog.Config.LoggingConfiguration();
        private static ColoredConsoleTarget consoleTarget = new ColoredConsoleTarget();
        private static DatabaseTarget dbTarget = new DatabaseTarget();
        private static FileTarget fileTarget = new FileTarget();

#if DEBUG_NLOG
        static Logger logger = LogManager.GetLogger("Example");
#else
#endif

        private static List<string> logrules = new List<string>();
        private static Dictionary<NLog.LogLevel, LogLevel> NLogLevelToSamayLogLevel = new Dictionary<NLog.LogLevel, LogLevel>();
        private static Dictionary<LogLevel, NLog.LogLevel> SamayLogLevelToNLogLevel = new Dictionary<LogLevel, NLog.LogLevel>();

        public static void Flush()
        {
            LogManager.Flush();
        }

        public static void InitSamayLogger(string samayDB_ConnectionString)
        {
            //initialize the logger to log using NLog to SQLite Database
            try
            {
                InitConversions();
                DatabaseParameterInfo param;
                dbTarget.KeepConnection = false;
                dbTarget.UseTransactions = false;
                dbTarget.DBProvider = "System.Data.SQLite.SQLiteConnection, System.Data.SQLite";
                dbTarget.ConnectionString = samayDB_ConnectionString;

                dbTarget.CommandText = "INSERT into LOGS (TimeStamp, Level, Message, JobName, TaskName) values(@Timestamp, @Loglevel,@Message, @JobName, @TaskName)";

                param = new DatabaseParameterInfo();
                param.Name = "@Timestamp";
                //param.Layout = "${longdate}";
                // param.Layout = new NLog.LayoutRenderers.DateLayoutRenderer();//
                param.Layout = "${event-context:item=timestamp}";
                dbTarget.Parameters.Add(param);

                param = new DatabaseParameterInfo();
                param.Name = "@Loglevel";
                param.Layout = "${level}";
                dbTarget.Parameters.Add(param);

                param = new DatabaseParameterInfo();
                param.Name = "@Message";
                param.Layout = "${message}";
                dbTarget.Parameters.Add(param);

                param = new DatabaseParameterInfo();
                param.Name = "@JobName";
                param.Layout = "${event-context:item=jobname}";
                dbTarget.Parameters.Add(param);

                param = new DatabaseParameterInfo();
                param.Name = "@TaskName";
                param.Layout = "${event-context:item=taskname}";
                dbTarget.Parameters.Add(param);

#if DEBUG
                config.AddTarget("Console", consoleTarget);
                consoleTarget.Layout = "${date:format=HH\\:mm\\:ss} ${logger}   ${message}";
#endif

                fileTarget.Layout = "${longdate} ${logger} ${message}";
                fileTarget.FileName = @"c:\samay\logs\logfile.txt";
                //fileTarget.ArchiveFileName = @"c:\samay\archives\log.{#}.txt";
                //fileTarget.ArchiveEvery = FileArchivePeriod.Day;
                //fileTarget.ArchiveNumbering = ArchiveNumberingMode.Date;
                //fileTarget.MaxArchiveFiles = 7;
                //fileTarget.ConcurrentWrites = true;
                //fileTarget.KeepFileOpen = false;

                config.AddTarget("file", fileTarget);

                fileTarget.Layout = "${longdate} ${logger} ${message}";
                fileTarget.FileName = "${basedir}/log.txt";
                fileTarget.ArchiveFileName = "${basedir}/archives/log.{#}.txt";
                fileTarget.ArchiveEvery = FileArchivePeriod.Day;
                fileTarget.ArchiveNumbering = ArchiveNumberingMode.Date;
                fileTarget.MaxArchiveFiles = 7;
                fileTarget.ConcurrentWrites = true;
                fileTarget.KeepFileOpen = false;

                config.AddTarget("file", fileTarget);
                config.AddTarget("dbTarget", dbTarget);

                config.AddTarget("file", fileTarget);

                config.AddTarget("dbTarget", dbTarget);
            }
            catch
            {
            }
            finally
            {
                LogManager.Configuration = config;
            }
        }

        public static void Log(string logMsg, string job, string task, string id, LogLevel logLevel, DateTime? timestamp = null)
        {
            if (!logrules.Contains(id))
                throw new Exception("Log Settings not defined for : " + job + " - " + task + " - " + id);

            if (!timestamp.HasValue)
                timestamp = DateTime.Now;

            try
            {
#if DEBUG_NLOG
                NLog.Logger logger = LogManager.GetLogger("Example");
#else
                NLog.Logger logger = LogManager.GetLogger(id);// + "_" + task.Replace(".", "_"));
#endif

                LogEventInfo myEvent = new LogEventInfo(SamayLogLevelToNLogLevel[logLevel], logger.Name, logMsg);
                myEvent.Properties.Add("jobname", job);
                myEvent.Properties.Add("taskname", task);
                myEvent.Properties.Add("timestamp", timestamp.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                logger.Log(myEvent);
            }
            catch (Exception ex)
            {
                //STOP Engine!??? Cant log!!
                throw ex;
            }
        }

        public static void SetLogLevel(string id, LogLevel logLevel)
        {
            if (logrules.Contains(id))
                return;

            // this is a problem.. dont use like this!!   NLog.Logger logger = LogManager.GetLogger(job/*+task/* jobID + taskID*/);// + "_" + task.Replace(".","_"));

            LoggingRule rule = new LoggingRule(id, SamayLogLevelToNLogLevel[logLevel], dbTarget);
            rule.Targets.Add(consoleTarget);
            rule.Targets.Add(fileTarget);
            config.LoggingRules.Add(rule);

            logrules.Add(id);

            //for debug
#if DEBUG
            LogManager.ThrowExceptions = true;
            //logger.Factory.ThrowExceptions = true;
#endif
        }

        private static void InitConversions()
        {
            NLogLevelToSamayLogLevel.Add(NLog.LogLevel.Debug, LogLevel.Debug);
            NLogLevelToSamayLogLevel.Add(NLog.LogLevel.Error, LogLevel.Error);
            NLogLevelToSamayLogLevel.Add(NLog.LogLevel.Fatal, LogLevel.Fatal);
            NLogLevelToSamayLogLevel.Add(NLog.LogLevel.Info, LogLevel.Info);
            NLogLevelToSamayLogLevel.Add(NLog.LogLevel.Trace, LogLevel.Trace);
            NLogLevelToSamayLogLevel.Add(NLog.LogLevel.Warn, LogLevel.Warn);

            SamayLogLevelToNLogLevel.Add(LogLevel.Debug, NLog.LogLevel.Debug);
            SamayLogLevelToNLogLevel.Add(LogLevel.Error, NLog.LogLevel.Error);
            SamayLogLevelToNLogLevel.Add(LogLevel.Fatal, NLog.LogLevel.Fatal);
            SamayLogLevelToNLogLevel.Add(LogLevel.Info, NLog.LogLevel.Info);
            SamayLogLevelToNLogLevel.Add(LogLevel.Trace, NLog.LogLevel.Trace);
            SamayLogLevelToNLogLevel.Add(LogLevel.Warn, NLog.LogLevel.Warn);
        }

        #region LOGS

        public static void LogDebug(string logMsg, string job, string task, string id, DateTime? timestamp = null)
        {
            Log(logMsg, job, task, id, LogLevel.Debug, timestamp);
        }

        public static void LogError(string logMsg, string job, string task, string id, DateTime? timestamp = null)
        {
            Log(logMsg, job, task, id, LogLevel.Error, timestamp);
        }

        public static void LogFatal(string logMsg, string job, string task, string id, DateTime? timestamp = null)
        {
            Log(logMsg, job, task, id, LogLevel.Fatal, timestamp);
        }

        public static void LogInfo(string logMsg, string job, string task, string id, DateTime? timestamp = null)
        {
            Log(logMsg, job, task, id, LogLevel.Info, timestamp);
        }

        public static void LogTrace(string logMsg, string job, string task, string id, DateTime? timestamp = null)
        {
            Log(logMsg, job, task, id, LogLevel.Trace, timestamp);
        }

        public static void LogWarning(string logMsg, string job, string task, string id, DateTime? timestamp = null)
        {
            Log(logMsg, job, task, id, LogLevel.Warn, timestamp);
        }

        #endregion LOGS
    }
}