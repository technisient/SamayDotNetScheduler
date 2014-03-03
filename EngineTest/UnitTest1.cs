using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Technisient;
using Technisient.SamayConfig;
using System.Data.SQLite;
using System.IO;
using System.Reflection;

namespace EngineTest
{
    [TestClass]
    public class UnitTest1
    {
        private TestContext testContextInstance;
        const string dbConnString = @"Data Source=C:\SVN\Development\EngineTest\DB\Samay.db; Password=kDyS2NwPzC^1G*7@";
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
        static Time t = new Time();
        static Engine engineConfig = new Engine();
        static Job dailyRunOnceJob = new Job();
        static Job monthlyRunOnceJob = new Job();
        static Job weeklyRunOnceJob = new Job();
        static Job yearlyRunOnceJob = new Job();
        static List<Job> jobsList = new List<Job>();

        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            t.StartTime = new DateTime(2012, 1, 20);
            t.EndTime = new DateTime(2012, 2, 5);

            CleanUpLogs();
            engineConfig.EngineConfig = new EngineSettings();
            engineConfig.EngineConfig.LogLevel = LogLevel.Info;
            engineConfig.EngineConfig.SleepTicks = 10000;
            engineConfig.EngineConfig.Enabled = true;

            SetupDailyRunOnce();
            SetupMonthlyRunOnce();
            SetupWeeklyRunOnce();
            SetupYearlyRunOnce();

            engineConfig.Jobs = jobsList;
            SamayEngine se = new SamayEngine();
            se.RunEngine(t, engineConfig);
        }

        private static void SetupDailyRunOnce()
        {
            dailyRunOnceJob.JobName = "Test_DailyRunOnce";
            dailyRunOnceJob.ExecuteTimes = 1;

            JobScheduleDaily dailyRunOnce = new JobScheduleDaily();
            dailyRunOnce.Time = new Time();
            dailyRunOnce.Time.StartTime = new DateTime(1, 1, 1, 2, 0, 0);
            AddJobToEngine(dailyRunOnceJob, dailyRunOnce);
        }

        [TestMethod]
        public void DailyRunOnce()
        {
            List<DateTime> expectedDates = new List<DateTime>();
            DateTime s = t.StartTime.Add(((JobScheduleDaily)dailyRunOnceJob.Schedules[0].ScheduleConfig).Time.StartTime.TimeOfDay);
            while (s <= t.EndTime)
            {
                expectedDates.Add(s);
                s = s.AddDays(1);
            }
            VerifyRun(dailyRunOnceJob.JobName, expectedDates);
        }

        private static void SetupMonthlyRunOnce()
        {
            monthlyRunOnceJob.JobName = "Test_MonthlyRunOnce";
            monthlyRunOnceJob.ExecuteTimes = 1;

            JobScheduleMonthly monthly = new JobScheduleMonthly();
            monthly.Day = new JobScheduleMonthlyDay();
            monthly.Day.Day = 20;
            monthly.Day.Time = new Time();
            monthly.Day.Time.StartTime = new DateTime(1, 1, 1, 2, 0, 0);
            AddJobToEngine(monthlyRunOnceJob, monthly);
        }

        [TestMethod]
        public void MonthlyRunOnce()
        {
            List<DateTime> expectedDates = new List<DateTime>();
            JobScheduleMonthly monthly = ((JobScheduleMonthly)monthlyRunOnceJob.Schedules[0].ScheduleConfig);
            DateTime s = t.StartTime.Add(monthly.Day.Time.StartTime.TimeOfDay);

            while (s.Day != monthly.Day.Day)
                s = s.AddDays(1);

            while (s <= t.EndTime)
            {
                expectedDates.Add(s);
                s = s.AddMonths(1);
            }
            VerifyRun(monthlyRunOnceJob.JobName, expectedDates);
        }

        private static void SetupWeeklyRunOnce()
        {
            weeklyRunOnceJob.JobName = "Test_WeeklyRunOnce";
            weeklyRunOnceJob.ExecuteTimes = 1;

            JobScheduleWeekly weekly = new JobScheduleWeekly();
            weekly.Day = new JobScheduleWeeklyDay();
            weekly.Day.DayName = DayName.Friday;
            weekly.Day.Time = new Time();
            weekly.Day.Time.StartTime = new DateTime(1, 1, 1, 2, 0, 0);

            AddJobToEngine(weeklyRunOnceJob, weekly);
        }

        [TestMethod]
        public void WeeklyRunOnce()
        {
            List<DateTime> expectedDates = new List<DateTime>();
            JobScheduleWeekly weekly = ((JobScheduleWeekly)weeklyRunOnceJob.Schedules[0].ScheduleConfig);
            DateTime s = t.StartTime.Add(weekly.Day.Time.StartTime.TimeOfDay);

            while (s.DayOfWeek.ToString() != weekly.Day.DayName.ToString())
                s = s.AddDays(1);

            while (s <= t.EndTime)
            {
                expectedDates.Add(s);
                s = s.AddDays(7);
            }
            VerifyRun(weeklyRunOnceJob.JobName, expectedDates);
        }

        private static void SetupYearlyRunOnce()
        {
            yearlyRunOnceJob.JobName = "Test_YearlyRunOnce";
            yearlyRunOnceJob.ExecuteTimes = 1;

            JobScheduleYearly yearly = new JobScheduleYearly();
            yearly.Month = new JobScheduleYearlyMonth();
            yearly.Month.MonthName = MonthName.January;
            yearly.Month.Day = 23;
            yearly.Month.Time = new Time();
            yearly.Month.Time.StartTime = new DateTime(1, 1, 1, 2, 0, 0);

            AddJobToEngine(yearlyRunOnceJob, yearly);
        }

        [TestMethod]
        public void YearlyRunOnce()
        {
            List<DateTime> expectedDates = new List<DateTime>();
            JobScheduleYearly yearly = ((JobScheduleYearly)yearlyRunOnceJob.Schedules[0].ScheduleConfig);
            DateTime s = t.StartTime.Add(yearly.Month.Time.StartTime.TimeOfDay);

            while (true)
            {
                if (((s.Month - 1) == (int)yearly.Month.MonthName) && (s.Day == yearly.Month.Day))
                    break;

                s = s.AddDays(1);
            }

            while (s <= t.EndTime)
            {
                expectedDates.Add(s);
                s = s.AddYears(1);
            }
            VerifyRun(yearlyRunOnceJob.JobName, expectedDates);
        }

        private static void CleanUpLogs()
        {
            using (SQLiteConnection cn = new SQLiteConnection(dbConnString))
            {
                try
                {
                    cn.Open();
                    SQLiteCommand sCommand = new SQLiteCommand(
                       @"Delete from Logs", cn);
                    sCommand.ExecuteNonQuery();
                }
                finally
                {
                    cn.Close();
                }
            }
        }

        private void VerifyRun(string jobName, List<DateTime> expectedDates)
        {
            List<DateTime> ActualRunTimes = new List<DateTime>();
            using (SQLiteConnection cn = new SQLiteConnection(dbConnString))
            {
                try
                {
                    cn.Open();
                    SQLiteCommand sCommand = new SQLiteCommand(
                       @"SELECT TimeStamp FROM Logs where JobName = '" + jobName + "' and Message like 'Executing Job:%' ORDER BY TimeStamp", cn);

                    SQLiteDataReader reader = sCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        DateTime dt = Convert.ToDateTime(reader["TimeStamp"]);
                        ActualRunTimes.Add(new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0));
                    }
                }
                finally
                {
                    cn.Close();
                }
            }

            List<DateTime> ExpectedDates2 = new List<DateTime>(expectedDates);

            foreach (var item in expectedDates)
            {
                if (ActualRunTimes.Contains(item))
                {
                    ActualRunTimes.Remove(item);
                    ExpectedDates2.Remove(item);
                }
            }

            bool success = (ActualRunTimes.Count == 0) && (ExpectedDates2.Count == 0);

            if (!success)
            {
                if (ActualRunTimes.Count != 0)
                {
                    System.Diagnostics.Trace.WriteLine("Ran when not needed:");
                    foreach (var item in ActualRunTimes)
                    {
                        System.Diagnostics.Trace.WriteLine(item.ToString());
                    }
                }

                if (ExpectedDates2.Count != 0)
                {
                    System.Diagnostics.Trace.WriteLine("Did not run when it should have:");
                    foreach (var item in ExpectedDates2)
                    {
                        System.Diagnostics.Trace.WriteLine(item.ToString());
                    }
                }

                Assert.IsTrue(ActualRunTimes.Count == 0);
                Assert.IsTrue(ExpectedDates2.Count == 0);
            }
        }

        public static void AddJobToEngine(Job job, object ScheduleObj)
        {
            jobsList.Add(job);
            job.Enabled = true;

            JobSchedule schedule = new JobSchedule();
            schedule.ScheduleConfig = ScheduleObj;
            job.Schedules = new List<JobSchedule> { schedule };

            job.TaskChain = new TaskChain();
            Task task = new Task();
            task.ClassName = "JobsNS.CalculatePi";
            task.LogLevel = LogLevel.Info;

            TaskParameter p = new TaskParameter();
            p.Name = "PiLength";
            p.Value = new List<string> { "2" };
            task.param = new List<TaskParameter> { p };
            job.TaskChain.Task = new List<Task> { task };
        }
    }
}
