using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Reflection;
using Technisient.Properties;
using Technisient.SamayConfig;

namespace Technisient
{
    public static class DateTimeExtensions
    {
        private static GregorianCalendar _gc = new GregorianCalendar();

        public static int GetWeekOfMonth(this DateTime dt)
        {
            return ((dt.Day - 1) / 7) + 1;
        }

        public static int GetWeekOfYear(this DateTime time)
        {
            return _gc.GetWeekOfYear(time, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
        }

        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(-1 * diff).Date;
        }
    }

    partial class SamayEngine
    {
        internal static Dictionary<string, string> _TaskAssemblyDict = new Dictionary<string, string>();

        internal static TaskBase CreateInstance(string className)
        {
            //try multiple times before giving up!
            for (int i = 0; i < 4; i++)
            {
                try
                {
                    return (TaskBase)Activator.CreateInstanceFrom(AppDomain.CurrentDomain, _TaskAssemblyDict[className], className).Unwrap();
                }
                catch (Exception ex)
                {
                    SamayLogger.LogWarning("Unable to create instance of " + className + " on Try " + i.ToString() + " Trying Again" + "\nERROR:" + ex.ToString(), SamayLogger.SamayEngineLogJobName, "Engine", SamayLogger.SamayEngineLoggingGUID, DateTime.Now);
                    System.Threading.Thread.Sleep(500);
                }
            }

            return (TaskBase)Activator.CreateInstanceFrom(AppDomain.CurrentDomain, _TaskAssemblyDict[className], className).Unwrap();
        }

        internal static TaskBase CreateInstanceInAppDomain(string className, AppDomain jobDomain)
        {
            //try multiple times before giving up!
            for (int i = 0; i < 4; i++)
            {
                try
                {
                    return (TaskBase)Activator.CreateInstanceFrom(jobDomain, _TaskAssemblyDict[className], className).Unwrap();
                }
                catch (Exception ex)
                {
                    SamayLogger.LogWarning("Unable to create instance of " + className + " on Try " + i.ToString() + " Trying Again" + "\nERROR:" + ex.ToString(), SamayLogger.SamayEngineLogJobName, "Engine", SamayLogger.SamayEngineLoggingGUID, DateTime.Now);
                    System.Threading.Thread.Sleep(500);
                }
            }

            return (TaskBase)Activator.CreateInstanceFrom(jobDomain, _TaskAssemblyDict[className], className).Unwrap();
        }

        internal static string EnsureFullPath(string path)
        {
            if (Path.IsPathRooted(path)) return path;
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return Path.Combine(GetWindowsAppRootPath(), path);
        }

        internal static string GetDBConnectionString()
        {
            string c = "Data Source=" + EnsureFullPath(Settings.Default.DatabasePath) + ";Version=3;";
            //if (!string.IsNullOrEmpty(Settings.Default.DatabasePassword))
            //    c += " Password=" + Settings.Default.DatabasePassword;
            return c;
        }

        #region LOG

        internal static void LogDebug(string logMsg)
        {
            SamayLogger.Log(logMsg, SamayLogger.SamayEngineLogJobName, "Engine", SamayLogger.SamayEngineLoggingGUID, LogLevel.Debug, DateTime.Now);
        }

        internal static void LogError(string logMsg)
        {
            SamayLogger.Log(logMsg, SamayLogger.SamayEngineLogJobName, "Engine", SamayLogger.SamayEngineLoggingGUID, LogLevel.Error, DateTime.Now);
        }

        internal static void LogInfo(string logMsg)
        {
            SamayLogger.Log(logMsg, SamayLogger.SamayEngineLogJobName, "Engine", SamayLogger.SamayEngineLoggingGUID, LogLevel.Info, DateTime.Now);
        }

        internal static void LogWarning(string logMsg)
        {
            SamayLogger.Log(logMsg, SamayLogger.SamayEngineLogJobName, "Engine", SamayLogger.SamayEngineLoggingGUID, LogLevel.Warn, DateTime.Now);
        }
        #endregion LOG

        // Dictionary<string, PropertyInfo[]> _cacheProperties = new Dictionary<string, PropertyInfo[]>();

        private static string GetWindowsAppRootPath()
        {
            string appFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase) + Path.DirectorySeparatorChar;
            if (appFolder.Contains(@"file:\"))
                appFolder = appFolder.Remove(0, 6);
            if (appFolder[1] != ':') appFolder = "\\" + appFolder;
            return appFolder;
        }

        private void DoDBCleanup(int logRetentionDays)
        {
            using (SQLiteConnection cn = new SQLiteConnection(SamayEngine.GetDBConnectionString()))
            {
                try
                {
                    cn.Open();
                    SQLiteCommand dCommand = new SQLiteCommand(@"DELETE FROM LOGS where TimeStamp < @LogRetentionDays", cn);

                    SQLiteParameter param = new SQLiteParameter("@LogRetentionDays", DateTime.Now.AddDays(logRetentionDays * -1).Date);
                    dCommand.Parameters.Add(param);

                    int count = dCommand.ExecuteNonQuery();

                    using (SQLiteCommand command = cn.CreateCommand())
                    {
                        command.CommandText = "vacuum;";
                        command.ExecuteNonQuery();
                    }

                    if (count > 0)
                        LogInfo("Logs Older than " + logRetentionDays + " Days Purged");
                }
                catch (Exception ex)
                {
                    LogError(ex.ToString());
                }
                finally
                {
                    cn.Close();
                }
            }
        }

        private Dictionary<string, List<string>> GetParameters(TaskBase tbClass, List<TaskParameter> parameters)
        {
            Dictionary<string, List<string>> taskParams = new Dictionary<string, List<string>>();
            foreach (TaskParameter p in parameters)
            {
                taskParams.Add(p.Name, p.Value);
            }
            return taskParams;
        }
        private void RunSystemJobs(Engine engine)
        {
            //TODO: Run of a timer
            DoDBCleanup(engine.EngineConfig.LogRetentionDays);
        }
    }
}