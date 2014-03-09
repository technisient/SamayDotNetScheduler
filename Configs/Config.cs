using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using Technisient.SamayConfig;

namespace Technisient
{
    public static class Config
    {
        public static bool AddJob(string jobString, string comment, string samayDB_ConnectionString)
        {
            Engine engine = GetSamayConfig(samayDB_ConnectionString);
            Job job = JsonConvert.DeserializeObject<Job>(jobString);

            foreach (var j in engine.Jobs)
            {
                if (j.JobName.ToLower() == job.JobName.ToLower())
                {
                    SamayLogger.Log("Failed to add new Job '" + job.JobName + "'. Job already exists", SamayLogger.SamayEngineLogJobName, "Config", "Engine", LogLevel.Error);
                    throw new Exception("Job with the name '" + job.JobName + "' already exists");
                }
            }

            engine.Jobs.Add(job);
            SaveSamayConfig(engine, comment, samayDB_ConnectionString);
            SamayLogger.Log("Successfully added new Job '" + job.JobName + "'", SamayLogger.SamayEngineLogJobName, "Config", "Engine", LogLevel.Info);
            return true;


        }

        public static Engine GetSamayConfig(string samayDB_ConnectionString)
        {
            // XmlSerializer serializer = new XmlSerializer(typeof(Engine));
            Engine output;
            using (SQLiteConnection cn = new SQLiteConnection(samayDB_ConnectionString))
            {
                try
                {
                    cn.Open();
                    SQLiteCommand sCommand = new SQLiteCommand(
                       @"SELECT Config FROM ConfigHistory ORDER BY TimeStamp DESC LIMIT 1", cn);

                    string engineString = (string)sCommand.ExecuteScalar();
                    output = JsonConvert.DeserializeObject<Engine>(engineString);
                    if (output == null)
                        throw new Exception();
                }
                catch (Exception ex)
                {
                    SamayLogger.Log("Failed to Read Samay Engine Configuration. " + ex.ToString(), "Config", "Config Reader", "Startup", LogLevel.Info, DateTime.Now);
                    throw ex;
                }
                finally
                {
                    cn.Close();
                }
            }

            //using (StringReader reader = new StringReader(File.ReadAllText("EngineConfig.xml")))
            //{
            //    output = (Engine)serializer.Deserialize(reader);
            //}

            SamayLogger.Log("Samay Engine Configuration sucessfully read into memory", SamayLogger.SamayEngineLogJobName, "Config", "Startup", LogLevel.Info, DateTime.Now);

            return output;
        }

        public static Engine GetSampleConfig()
        {
            Engine engine = new Engine();
            engine.EngineConfig.GlobalExcludeDates.Add(new GlobalExcludeDate { Date = new DateTime(2014, 12, 25), Note = "Christmas" });
            engine.EngineConfig.LogLevel = LogLevel.Trace;

            Job calcPi = new Job();
            calcPi.JobName = "Calculate Pie";

            calcPi.Interval = new JobInterval { Interval_msec = 60000 };
            calcPi.Schedules.Add(new JobSchedule
            {
                JobScheduleType = JobScheduleTypeEnum.JobScheduleDaily,
                ScheduleConfig = new JobScheduleDaily
                {
                    Time = new Time
                    {
                        StartTime = new DateTime(1, 1, 1, 00, 5, 0),
                        EndTime = new DateTime(1, 1, 1, 23, 55, 0),
                        EndTimeSpecified = true
                    }
                }
            });

            Task piTask = new Task
            {
                ClassName = "JobsNS.CalculatePi",
                LogLevel = Technisient.LogLevel.Trace,
                param = new List<TaskParameter>{
                    new TaskParameter{
                        Name= "PiLength",
                        Value = new List<string>{"20"}
                    }
                }
            };
            calcPi.TaskChain = new TaskChain { Task = new List<Task> { piTask } };

            engine.Jobs.Add(calcPi);

            return engine;
        }

        public static bool RemoveJob(string jobName, string comment, string samayDB_ConnectionString)
        {
            Engine engine = GetSamayConfig(samayDB_ConnectionString);
            Job jobToRemove = null;

            foreach (var j in engine.Jobs)
            {
                if (j.JobName.ToLower() == jobName.ToLower())
                {
                    jobToRemove = j;
                    break;
                }
            }

            if (jobToRemove != null)
            {
                engine.Jobs.Remove(jobToRemove);
                SaveSamayConfig(engine, comment, samayDB_ConnectionString);
                SamayLogger.Log("Successfully removed new Job '" + jobName + "'", SamayLogger.SamayEngineLogJobName, "Config", "Engine", LogLevel.Info);
                return true;
            }

            SamayLogger.Log("Failed to removed new Job '" + jobName + "'. Job does not exist", SamayLogger.SamayEngineLogJobName, "Config", "Engine", LogLevel.Error);

            throw new Exception("Job '" + jobName + "' does not exist");
        }

        public static bool SaveSamayConfig(Engine config, string comment, string samayDB_ConnectionString)
        {
            using (SQLiteConnection cn = new SQLiteConnection(samayDB_ConnectionString))
            {
                try
                {
                    cn.Open();

                    SQLiteCommand sCommand1 = new SQLiteCommand(@"create table if not exists ConfigHistory (ID integer primary key autoincrement, TimeStamp datetime default current_timestamp, UserID integer,Title varchar,Config varchar);", cn);
                    sCommand1.ExecuteScalar();

                    SQLiteCommand sCommand2 = new SQLiteCommand(@"INSERT INTO ConfigHistory ([TimeStamp], UserID, Title, Config)
                           VALUES (@TimeStamp, @UserID, @Title, @Config);
                           SELECT last_insert_rowid();", cn);

                    sCommand2.Parameters.AddRange(new[]
                                         {
                                           new SQLiteParameter {DbType = DbType.DateTime, ParameterName = "TimeStamp", Value = DateTime.UtcNow},
                                           new SQLiteParameter {DbType = DbType.Int16, ParameterName = "UserID", Value = 1},
                                           new SQLiteParameter {DbType = DbType.String, ParameterName = "Title", Value = comment},
                                           new SQLiteParameter {DbType = DbType.String, ParameterName = "Config", Value = JsonConvert.SerializeObject(config)},
                                         });

                    sCommand2.ExecuteScalar();

                    SQLiteCommand sCommand3 = new SQLiteCommand(@"create table if not exists LOGS (ID integer primary key autoincrement, TimeStamp datetime default current_timestamp, Level varchar, Message varchar, JobName varchar, TaskName varchar);", cn);
                    sCommand3.ExecuteScalar();
                    return true;
                }
                catch (Exception ex)
                {
                    SamayLogger.Log("Error while saving config. " + ex.ToString(), SamayLogger.SamayEngineLogJobName, "Config", "Startup", LogLevel.Error, DateTime.Now);
                    return false;
                }
                finally
                {
                    cn.Close();
                }
            }
        }

        public static void InitSamayDB(string filePath, string dbconnectionString)
        {
            if (!File.Exists(filePath))
            {
                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                SQLiteConnection.CreateFile(filePath);
                Engine config = GetSampleConfig();
                SaveSamayConfig(config, "Initialize Samples Config", dbconnectionString);
            }
        }
    }
}