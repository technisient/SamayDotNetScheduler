#define LOG
using Technisient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Technisient.SamayConfig;
using System.Collections.Specialized;

namespace TestSchedule
{


    /// <summary>
    ///This is a test class for SamayEngineTest and is intended
    ///to contain all SamayEngineTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SamayEngineTest
    {
        private TestContext testContextInstance;

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

        #region Additional test attributes

        static DateTime startTestTime = new DateTime(2012, 1, 1);
        static DateTime endTestTime = new DateTime(2016, 4, 1);
        static DateTime Midnightime = new DateTime(1, 1, 1, 0, 0, 0);

        private static Time GetDayTime()
        {
            Time dayTime = new Time();
            dayTime.StartTime = new DateTime(1, 1, 1, 20, 0, 0); //8pm
            dayTime.EndTime = new DateTime(1, 1, 1, 22, 0, 0); //10pm
            return dayTime;
        }

        private static Time GetOvernightTime()
        {
            Time overnightTime = new Time();
            overnightTime.StartTime = new DateTime(1, 1, 1, 20, 0, 0); //8pm
            overnightTime.EndTime = new DateTime(1, 1, 1, 4, 0, 0); //4am
            return overnightTime;
        }

        static JobSchedule dailyDaySchedule = new JobSchedule();
        static JobSchedule dailyOvernightSchedule = new JobSchedule();

        static JobSchedule monthlyDaySchedule = new JobSchedule();
        static JobSchedule monthlyOvernightSchedule = new JobSchedule();

        static JobSchedule yearlyDaySchedule = new JobSchedule();
        static JobSchedule yearlyOvernightSchedule = new JobSchedule();

        static JobSchedule OneTimeDaySchedule = new JobSchedule();
        static JobSchedule OneTimeOvernightSchedule = new JobSchedule();

       
       

        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            //DAILY
            JobScheduleDaily d_d = new JobScheduleDaily();
            d_d.Time = GetDayTime();
            dailyDaySchedule.ScheduleConfig = d_d;

            JobScheduleDaily d_o = new JobScheduleDaily();
            d_o.Time = GetOvernightTime();
            dailyOvernightSchedule.ScheduleConfig = d_o;


            //MONTHLY
            //TODO: run from 0 to 40 days in a loop?
            JobScheduleMonthly m_d = new JobScheduleMonthly();
            m_d.Day = new JobScheduleMonthlyDay();
            m_d.Day.Day = 31;
            m_d.Day.Time = GetDayTime();
            monthlyDaySchedule.ScheduleConfig = m_d;

            JobScheduleMonthly m_o = new JobScheduleMonthly();
            m_o.Day = new JobScheduleMonthlyDay();
            m_o.Day.Day = 31;
            m_o.Day.Time = GetOvernightTime();
            monthlyOvernightSchedule.ScheduleConfig = m_o;


            //YEARLY
            JobScheduleYearly y_d= new JobScheduleYearly();
            y_d.Month = new JobScheduleYearlyMonth();
            y_d.Month.MonthName = MonthName.March;
            y_d.Month.Day = 31;
            y_d.Month.Time = GetDayTime();
            yearlyDaySchedule.ScheduleConfig = y_d;

            JobScheduleYearly y_o = new JobScheduleYearly();
            y_o.Month = new JobScheduleYearlyMonth();
            y_o.Month.MonthName = MonthName.March;
            y_o.Month.Day = 31;
            y_o.Month.Time = GetOvernightTime();
            yearlyOvernightSchedule.ScheduleConfig = y_o;


            //ONETIME
            //TODO: run for a year
            JobScheduleOneTimeOnly o_d = new JobScheduleOneTimeOnly();
            o_d.StartDateTime = new DateTime(2011, 1, 31,6,0,0);
            o_d.EndDateTime = new DateTime(2011, 1,31,14,30,0);
            OneTimeDaySchedule.ScheduleConfig = o_d;

            JobScheduleOneTimeOnly o_o = new JobScheduleOneTimeOnly();
            o_o.StartDateTime = new DateTime(2011, 12, 31, 6, 0, 0);
            o_o.EndDateTime = new DateTime(2012, 1, 2, 14, 30, 0);
            OneTimeOvernightSchedule.ScheduleConfig = o_o;
        }

       
        #endregion

       
        /// <summary>
        ///A test for IsJobScheduledToRunNow
        ///</summary>
        [TestMethod()]
        public void DailyDayTest()
        {
            DateTime NOW = startTestTime;
            bool lastexpected = true;
            try
            {
                while (NOW <= endTestTime)
                {
                    NOW = NOW.AddMinutes(1);
                    bool expected = (NOW.TimeOfDay >= GetDayTime().StartTime.TimeOfDay) && (NOW.TimeOfDay <= GetDayTime().EndTime.TimeOfDay);
                    bool actual = SamayEngine.IsJobScheduledToRunNow(dailyDaySchedule, NOW);
                    Log(NOW, expected, ref lastexpected);
                    Assert.AreEqual(expected, actual);
                }
            }
            catch
            {
                throw new Exception("Failed for Time: " + NOW.ToString());
            }
        }

        [TestMethod()]
        public void DailyOvernightTest()
        {
            DateTime NOW = startTestTime;
            bool expected, lastexpected= true;
            try
            {
                while (NOW <= endTestTime)
                {
                    NOW = NOW.AddMinutes(1);
                    //NOW = DateTime.Parse("1/1/2011 3:59:00 AM");
                    if(SamayEngine.IsBetWeen(Midnightime,GetOvernightTime().EndTime,new DateTime(NOW.TimeOfDay.Ticks)) ||
                        (SamayEngine.IsBetWeen(GetOvernightTime().StartTime,Midnightime.AddDays(1),new DateTime(NOW.TimeOfDay.Ticks))))
                        expected = true;
                    else
                        expected = false;

                    Log(NOW, expected, ref lastexpected);

                    bool actual = SamayEngine.IsJobScheduledToRunNow(dailyOvernightSchedule, NOW);
                    Assert.AreEqual(expected, actual);
                }
            }
            catch
            {
                throw new Exception("Failed for Time: " + NOW.ToString());
            }
        }


        //[TestMethod()]
        //public void MonthlyDayTest()
        //{
        //    DateTime NOW = startTestTime;
        //    bool expected, lastexpected = true;

        //    try
        //    {
        //        while (NOW <= endTestTime)
        //        {
        //            NOW = NOW.AddMinutes(1);
        //           // NOW = DateTime.Parse("2/1/2010 12:00:00 AM");
        //            if (NOW.Day != ((EngineJobScheduleMonthly)monthlyDaySchedule.Item).Day.Day)
        //                expected = false;
        //            else
        //                expected = (NOW.TimeOfDay >= GetDayTime().StartTime.TimeOfDay) && (NOW.TimeOfDay <= GetDayTime().EndTime.TimeOfDay);

        //            bool actual = SamayEngine.IsJobScheduledToRunNow(monthlyDaySchedule, NOW);
        //            Log(NOW, expected, ref lastexpected);
        //            Assert.AreEqual(expected, actual);
        //        }
        //    }
        //    catch
        //    {
        //        throw new Exception("Failed for Time: " + NOW.ToString());
        //    }
        //}

        [TestMethod()]
        public void MonthlyDayTest()
        {
            JobScheduleMonthly schedule = (JobScheduleMonthly)monthlyDaySchedule.ScheduleConfig;

            DateTime dt = startTestTime.AddMonths(-2);
            System.Collections.Generic.Dictionary<DateTime, DateTime> validRanges = new System.Collections.Generic.Dictionary<DateTime, DateTime>();

            while (dt <= endTestTime)
            {
                dt = dt.AddMonths(1);
                if (DateTime.DaysInMonth(dt.Year, dt.Month) < schedule.Day.Day)
                    continue;

                DateTime s = new DateTime(dt.Year, dt.Month, schedule.Day.Day);
                DateTime e = s;
                s = s.Add(schedule.Day.Time.StartTime.TimeOfDay);
                e = e.Add(schedule.Day.Time.EndTime.TimeOfDay);

                validRanges.Add(s, e);
            }

            RunTests(monthlyDaySchedule, validRanges);
        }

        [TestMethod()]
        public void MonthlyOvernightTest()
        {
            JobScheduleMonthly schedule = (JobScheduleMonthly)monthlyOvernightSchedule.ScheduleConfig;

            DateTime dt = startTestTime.AddMonths(-2);
            System.Collections.Generic.Dictionary<DateTime, DateTime> validRanges = new System.Collections.Generic.Dictionary<DateTime, DateTime>();

            while (dt <= endTestTime)
            {
                dt = dt.AddMonths(1);
                if (DateTime.DaysInMonth(dt.Year, dt.Month) < schedule.Day.Day)
                    continue;

                DateTime s = new DateTime(dt.Year, dt.Month, schedule.Day.Day);
                DateTime e = s.AddDays(1);
                s = s.Add(schedule.Day.Time.StartTime.TimeOfDay);
                e = e.Add(schedule.Day.Time.EndTime.TimeOfDay);

                validRanges.Add(s, e);
            }

            RunTests(monthlyOvernightSchedule, validRanges);
        }
        
        [TestMethod()]
        public void YearlyDayTest()
        {
           JobScheduleYearly schedule = (JobScheduleYearly)yearlyDaySchedule.ScheduleConfig;

            DateTime dt = startTestTime.AddYears(-2);
            System.Collections.Generic.Dictionary<DateTime, DateTime> validRanges = new System.Collections.Generic.Dictionary<DateTime, DateTime>();

            while (dt <= endTestTime)
            {
                dt = dt.AddYears(1);
                if (DateTime.DaysInMonth(dt.Year, dt.Month) < schedule.Month.Day)
                    continue;

                DateTime s = new DateTime(dt.Year, ((int)schedule.Month.MonthName)+1, schedule.Month.Day);
                DateTime e = s;

                s = s.Add(schedule.Month.Time.StartTime.TimeOfDay);
                e = e.Add(schedule.Month.Time.EndTime.TimeOfDay);
                validRanges.Add(s, e);
            }

            RunTests(yearlyDaySchedule, validRanges);
        }

        [TestMethod()]
        public void YearlyOvernightTest()
        {
            JobScheduleYearly schedule = (JobScheduleYearly)yearlyOvernightSchedule.ScheduleConfig;

            DateTime dt = startTestTime.AddYears(-2);
            System.Collections.Generic.Dictionary<DateTime, DateTime> validRanges = new System.Collections.Generic.Dictionary<DateTime, DateTime>();

            while (dt <= endTestTime)
            {
                dt = dt.AddYears(1);
                if (DateTime.DaysInMonth(dt.Year, dt.Month) < schedule.Month.Day)
                    continue;

                DateTime s = new DateTime(dt.Year, ((int)schedule.Month.MonthName) + 1, schedule.Month.Day);
                DateTime e = s.AddDays(1);

                s = s.Add(schedule.Month.Time.StartTime.TimeOfDay);
                e = e.Add(schedule.Month.Time.EndTime.TimeOfDay);
                validRanges.Add(s, e);
            }

            RunTests(yearlyOvernightSchedule, validRanges);
        }

        [TestMethod()]
        public void OneTimeDayTest()
        {
            JobScheduleOneTimeOnly schedule = (JobScheduleOneTimeOnly)OneTimeDaySchedule.ScheduleConfig;

            System.Collections.Generic.Dictionary<DateTime, DateTime> validRanges = new System.Collections.Generic.Dictionary<DateTime, DateTime>();
            validRanges.Add(schedule.StartDateTime, schedule.EndDateTime);

            RunTests(OneTimeDaySchedule, validRanges);
        }
        
        [TestMethod()]
        public void OneTimeOvernightTest()
        {
            JobScheduleOneTimeOnly schedule = (JobScheduleOneTimeOnly)OneTimeOvernightSchedule.ScheduleConfig;

            System.Collections.Generic.Dictionary<DateTime, DateTime> validRanges = new System.Collections.Generic.Dictionary<DateTime, DateTime>();
            validRanges.Add(schedule.StartDateTime, schedule.EndDateTime);

            RunTests(OneTimeOvernightSchedule, validRanges);
        }

        [TestMethod()]
        public void WeeklyDayTest_Every()
        {
            JobSchedule WeeklyDaySchedule = new JobSchedule();
            JobScheduleWeekly schedule = new JobScheduleWeekly();
            schedule.Day = new JobScheduleWeeklyDay();
            schedule.Day.DayName = DayName.Sunday;
            schedule.Day.Time = GetDayTime();
            schedule.Day.WeekRecurrence = WeekRecurrence.Every;
            WeeklyDaySchedule.ScheduleConfig = schedule;

           
            DateTime dt = startTestTime.AddMonths(-2);
            while ((int)dt.DayOfWeek != (int)schedule.Day.DayName)
            {
                dt = dt.AddDays(1);
            }

            System.Collections.Generic.Dictionary<DateTime, DateTime> validRanges = new System.Collections.Generic.Dictionary<DateTime, DateTime>();

            while (dt <= endTestTime)
            {
                dt = dt.AddDays(7);
                //if (DateTime.DaysInMonth(dt.Year, dt.Month) < schedule.Day.Day)
                //    continue;

                DateTime s = new DateTime(dt.Year, dt.Month, dt.Day);
                DateTime e = s;
                s = s.Add(schedule.Day.Time.StartTime.TimeOfDay);
                e = e.Add(schedule.Day.Time.EndTime.TimeOfDay);

                validRanges.Add(s, e);
            }

            RunTests(WeeklyDaySchedule, validRanges);
        }

        [TestMethod()]
        public void WeeklyOvernightTest_Every()
        {
            JobSchedule WeeklyOvernightSchedule = new JobSchedule();
            JobScheduleWeekly schedule = new JobScheduleWeekly();
            schedule.Day = new JobScheduleWeeklyDay();
            schedule.Day.DayName = DayName.Sunday;
            schedule.Day.Time = GetOvernightTime();
            schedule.Day.WeekRecurrence = WeekRecurrence.Every;
            WeeklyOvernightSchedule.ScheduleConfig = schedule;
          
            DateTime dt = startTestTime.AddMonths(-2);
            while ((int)dt.DayOfWeek != (int)schedule.Day.DayName)
            {
                dt = dt.AddDays(1);
            }

            System.Collections.Generic.Dictionary<DateTime, DateTime> validRanges = new System.Collections.Generic.Dictionary<DateTime, DateTime>();

            while (dt <= endTestTime)
            {
                dt = dt.AddDays(7);
                //if (DateTime.DaysInMonth(dt.Year, dt.Month) < schedule.Day.Day)
                //    continue;

                DateTime s = new DateTime(dt.Year, dt.Month, dt.Day);
                DateTime e = s.AddDays(1);
                s = s.Add(schedule.Day.Time.StartTime.TimeOfDay);
                e = e.Add(schedule.Day.Time.EndTime.TimeOfDay);

                validRanges.Add(s, e);
            }

            RunTests(WeeklyOvernightSchedule, validRanges);
        }


        [TestMethod()]
        public void WeeklyDayTest_First()
        {
            JobScheduleWeekly schedule = new JobScheduleWeekly();
            schedule.Day = new JobScheduleWeeklyDay();
            schedule.Day.DayName = DayName.Sunday;
            schedule.Day.Time = GetDayTime();
            schedule.Day.WeekRecurrence = WeekRecurrence.First;

            RunForWeekly(schedule, (int)schedule.Day.WeekRecurrence);
        }

        [TestMethod()]
        public void WeeklyDayTest_Second()
        {
            JobScheduleWeekly schedule = new JobScheduleWeekly();
            schedule.Day = new JobScheduleWeeklyDay();
            schedule.Day.DayName = DayName.Sunday;
            schedule.Day.Time = GetDayTime();
            schedule.Day.WeekRecurrence = WeekRecurrence.Second;

            RunForWeekly(schedule, (int)schedule.Day.WeekRecurrence);
        }

        [TestMethod()]
        public void WeeklyDayTest_Third()
        {
            JobScheduleWeekly schedule = new JobScheduleWeekly();
            schedule.Day = new JobScheduleWeeklyDay();
            schedule.Day.DayName = DayName.Sunday;
            schedule.Day.Time = GetDayTime();
            schedule.Day.WeekRecurrence = WeekRecurrence.Third;

            RunForWeekly(schedule, (int)schedule.Day.WeekRecurrence);
        }

        [TestMethod()]
        public void WeeklyDayTest_Fourth()
        {
            JobScheduleWeekly schedule = new JobScheduleWeekly();
            schedule.Day = new JobScheduleWeeklyDay();
            schedule.Day.DayName = DayName.Sunday;
            schedule.Day.Time = GetDayTime();
            schedule.Day.WeekRecurrence = WeekRecurrence.Fourth;

            RunForWeekly(schedule, (int)schedule.Day.WeekRecurrence);
        }

        [TestMethod()]
        public void WeeklyDayTest_Fifth()
        {
            JobScheduleWeekly schedule = new JobScheduleWeekly();
            schedule.Day = new JobScheduleWeeklyDay();
            schedule.Day.DayName = DayName.Sunday;
            schedule.Day.Time = GetDayTime();
            schedule.Day.WeekRecurrence = WeekRecurrence.Fifth;

            RunForWeekly(schedule, (int)schedule.Day.WeekRecurrence);
        }

        [TestMethod()]
        public void WeeklyDayTest_Sixth()
        {
            JobScheduleWeekly schedule = new JobScheduleWeekly();
            schedule.Day = new JobScheduleWeeklyDay();
            schedule.Day.DayName = DayName.Sunday;
            schedule.Day.Time = GetDayTime();
            schedule.Day.WeekRecurrence = WeekRecurrence.Sixth;

            RunForWeekly(schedule, (int)schedule.Day.WeekRecurrence);
        }
        
         [TestMethod()]
        public void WeeklyDayTest_Last()
        {
            JobScheduleWeekly schedule = new JobScheduleWeekly();
            schedule.Day = new JobScheduleWeeklyDay();
            schedule.Day.DayName = DayName.Sunday;
            schedule.Day.Time = GetDayTime();
            schedule.Day.WeekRecurrence = WeekRecurrence.Last;

            RunForWeekly(schedule, 0);
        }

        ////////////////////////////////////////////////////////////////////////////
        [TestMethod()]
        public void WeeklyOvernightTest_First()
        {
            JobScheduleWeekly schedule = new JobScheduleWeekly();
            schedule.Day = new JobScheduleWeeklyDay();
            schedule.Day.DayName = DayName.Sunday;
            schedule.Day.Time = GetOvernightTime();
            schedule.Day.WeekRecurrence = WeekRecurrence.First;

            RunForWeekly(schedule, (int)schedule.Day.WeekRecurrence);
        }

        [TestMethod()]
        public void WeeklyOvernightTest_Second()
        {
            JobScheduleWeekly schedule = new JobScheduleWeekly();
            schedule.Day = new JobScheduleWeeklyDay();
            schedule.Day.DayName = DayName.Sunday;
            schedule.Day.Time = GetOvernightTime();
            schedule.Day.WeekRecurrence = WeekRecurrence.Second;

            RunForWeekly(schedule, (int)schedule.Day.WeekRecurrence);
        }

        [TestMethod()]
        public void WeeklyOvernightTest_Third()
        {
            JobScheduleWeekly schedule = new JobScheduleWeekly();
            schedule.Day = new JobScheduleWeeklyDay();
            schedule.Day.DayName = DayName.Sunday;
            schedule.Day.Time = GetOvernightTime();
            schedule.Day.WeekRecurrence = WeekRecurrence.Third;

            RunForWeekly(schedule, (int)schedule.Day.WeekRecurrence);
        }

        [TestMethod()]
        public void WeeklyOvernightTest_Fourth()
        {
            JobScheduleWeekly schedule = new JobScheduleWeekly();
            schedule.Day = new JobScheduleWeeklyDay();
            schedule.Day.DayName = DayName.Sunday;
            schedule.Day.Time = GetOvernightTime();
            schedule.Day.WeekRecurrence = WeekRecurrence.Fourth;

            RunForWeekly(schedule, (int)schedule.Day.WeekRecurrence);
        }

        [TestMethod()]
        public void WeeklyOvernightTest_Fifth()
        {
            JobScheduleWeekly schedule = new JobScheduleWeekly();
            schedule.Day = new JobScheduleWeeklyDay();
            schedule.Day.DayName = DayName.Sunday;
            schedule.Day.Time = GetOvernightTime();
            schedule.Day.WeekRecurrence = WeekRecurrence.Fifth;

            RunForWeekly(schedule, (int)schedule.Day.WeekRecurrence);
        }

        [TestMethod()]
        public void WeeklyOvernightTest_Sixth()
        {
            JobScheduleWeekly schedule = new JobScheduleWeekly();
            schedule.Day = new JobScheduleWeeklyDay();
            schedule.Day.DayName = DayName.Sunday;
            schedule.Day.Time = GetOvernightTime();
            schedule.Day.WeekRecurrence = WeekRecurrence.Sixth;

            RunForWeekly(schedule, (int)schedule.Day.WeekRecurrence);
        }

          [TestMethod()]
        public void WeeklyOvernightTest_Last()
        {
            JobScheduleWeekly schedule = new JobScheduleWeekly();
            schedule.Day = new JobScheduleWeeklyDay();
            schedule.Day.DayName = DayName.Sunday;
            schedule.Day.Time = GetOvernightTime();
            schedule.Day.WeekRecurrence = WeekRecurrence.Last;

            RunForWeekly(schedule, 0);
        }



        private static bool IsLastDayOccuranceForMonth(DateTime dt)
        {
             int daysinmonth = DateTime.DaysInMonth(dt.Year,dt.Month);
             if (dt.Day > (daysinmonth - 7))
                 return true;
             else
                 return false;
        }
        
        ///////////////////////////////////
        

        private static void RunForWeekly(JobScheduleWeekly schedule, int weekNumber)
        {

            bool isLast = (weekNumber==0)?true:false;

            JobSchedule WeeklyDaySchedule = new JobSchedule();
            WeeklyDaySchedule.ScheduleConfig = schedule;

            DateTime dt = startTestTime.AddMonths(-2);

            System.Collections.Generic.Dictionary<DateTime, DateTime> validRanges = new System.Collections.Generic.Dictionary<DateTime, DateTime>();

            int n = 0;
            DayOfWeek dw = DayOfWeek.Sunday;
            while (dt <= endTestTime)
            {
                dt = dt.AddDays(1);
                //dt = DateTime.Parse("1/22/2012 8:00:00 PM");
                if (dt.Day == 1)
                {
                    n = 0;
                    dw = dt.DayOfWeek;
                }

                if (dt.DayOfWeek == dw)
                    n++;

                bool isValid = false;
                if (isLast)
                {
                    if ((IsLastDayOccuranceForMonth(dt)) && ((int)dt.DayOfWeek == (int)schedule.Day.DayName))
                    {
                        isValid = true;
                    }
                }
                else if ((n == weekNumber) && ((int)dt.DayOfWeek == (int)schedule.Day.DayName))
                {
                    isValid = true;
                }

                if (!isValid)
                    continue;

                DateTime s = new DateTime(dt.Year, dt.Month, dt.Day);
                DateTime e = s;
                s = s.Add(schedule.Day.Time.StartTime.TimeOfDay);
                e = e.Add(schedule.Day.Time.EndTime.TimeOfDay);

                if (s.TimeOfDay > e.TimeOfDay)
                    e = e.AddDays(1);

                validRanges.Add(s, e);
            }

            RunTests(WeeklyDaySchedule, validRanges);
        }


        [TestMethod()]
        public void GetWeekOfMonthTest()
        {
            Assert.AreEqual(1,DateTimeExtensions.GetWeekOfMonth(new DateTime(2012, 1, 1)));
            Assert.AreEqual(2,DateTimeExtensions.GetWeekOfMonth(new DateTime(2012, 1, 8)));
            Assert.AreEqual(5,DateTimeExtensions.GetWeekOfMonth(new DateTime(2012, 1, 31)));
            Assert.AreEqual(1,DateTimeExtensions.GetWeekOfMonth(new DateTime(2012, 9, 1)));
            Assert.AreEqual(1,DateTimeExtensions.GetWeekOfMonth(new DateTime(2012, 10, 7)));
            Assert.AreEqual(2,DateTimeExtensions.GetWeekOfMonth(new DateTime(2012, 10, 8)));
            Assert.AreEqual(5,DateTimeExtensions.GetWeekOfMonth(new DateTime(2012, 12, 31)));
            Assert.AreEqual(2,DateTimeExtensions.GetWeekOfMonth(new DateTime(2012, 11, 14)));
            Assert.AreEqual(4,DateTimeExtensions.GetWeekOfMonth(new DateTime(2012, 5, 25)));
            Assert.AreEqual(2,DateTimeExtensions.GetWeekOfMonth(new DateTime(2012, 7, 13)));
        }

        private static void RunTests(JobSchedule schedule, System.Collections.Generic.Dictionary<DateTime, DateTime> validRanges)
        {
            DateTime NOW = startTestTime;
            bool expected, lastexpected = true;
            
            try
            {               
                while (NOW <= endTestTime)
                {
                    expected = false;
                    NOW = NOW.AddMinutes(1);
                   //NOW = DateTime.Parse("2/5/2011 9:00:00 PM");
                    foreach (var item in validRanges.Keys)
                    {
                        if (SamayEngine.IsBetWeen(item, validRanges[item], NOW))
                        {
                            expected = true;
                            break;
                        }
                    }

                    
                    bool actual = SamayEngine.IsJobScheduledToRunNow(schedule, NOW);
                    Log(NOW, expected, ref lastexpected);
                    Assert.AreEqual(expected, actual);
                }
            }
            catch
            {
                throw new Exception("Failed for Time: " + NOW.ToString());
            }
        }


        private static void Log(DateTime NOW, bool expected, ref bool lastexpected)
        {
#if LOG
            if (expected != lastexpected)
            {
                lastexpected = expected;
                //Console.WriteLine(NOW.ToString() + ": " + expected.ToString());
                if(!expected)
                Console.WriteLine(NOW.AddMinutes(-1).ToString()  + ": TRUE");//+ expected.ToString());
                else
                    Console.Write(NOW.ToString() + " - " );
            }
#endif
        }

    }
}
