using Newtonsoft.Json;
using System;
using Technisient.SamayConfig;

namespace Technisient
{
    public partial class SamayEngine
    {
        public static Time AdjustIfOvernight(Time time)
        {
            if (time.StartTime.TimeOfDay > time.EndTime.TimeOfDay)
                time.EndTime = time.EndTime.AddDays(1);
            return time;
        }

        public static bool IsBetWeen(DateTime start, DateTime end, DateTime date)
        {
            if ((date >= start) && (date <= end))
                return true;
            else
                return false;
        }

        public static bool IsJobScheduledToRunNow(JobSchedule jobSchedule, DateTime NOW)
        {
            switch (jobSchedule.JobScheduleType)
            {
                case JobScheduleTypeEnum.JobScheduleDaily:
                    JobScheduleDaily daily = JsonConvert.DeserializeObject<JobScheduleDaily>(jobSchedule.ScheduleConfig.ToString());
                    return IsTimeToRunNow(GetDailyTime(daily, NOW), NOW);

                case JobScheduleTypeEnum.JobScheduleWeekly:
                    JobScheduleWeekly weekly = JsonConvert.DeserializeObject<JobScheduleWeekly>(jobSchedule.ScheduleConfig.ToString());
                    return DoWeeklyCheck(weekly, NOW);

                case JobScheduleTypeEnum.JobScheduleMonthly:
                    JobScheduleMonthly monthly = JsonConvert.DeserializeObject<JobScheduleMonthly>(jobSchedule.ScheduleConfig.ToString());
                    return IsTimeToRunNow(GetMonthlyTime(monthly, NOW), NOW);

                case JobScheduleTypeEnum.JobScheduleYearly:
                    JobScheduleYearly yearly = JsonConvert.DeserializeObject<JobScheduleYearly>(jobSchedule.ScheduleConfig.ToString());
                    return IsTimeToRunNow(GetYearlyTime(yearly, NOW), NOW);

                case JobScheduleTypeEnum.JobScheduleOneTimeOnly:
                    JobScheduleOneTimeOnly oneTime = JsonConvert.DeserializeObject<JobScheduleOneTimeOnly>(jobSchedule.ScheduleConfig.ToString());
                    return DoOneTimeOnlyCheck(oneTime, NOW);

                default:
                    throw new Exception("Unknown Schedule Type: " + jobSchedule.JobScheduleType.ToString());
            }
        }

        private static bool DoOneTimeOnlyCheck(JobScheduleOneTimeOnly oneTime, DateTime NOW)
        {
            if ((NOW >= oneTime.StartDateTime)
                && (NOW <= oneTime.EndDateTime))
                return true;
            else return false;
        }

        private static bool DoWeeklyCheck(JobScheduleWeekly weekly, DateTime NOW)
        {
            DateTime OriginalNOW = NOW;
            if (weekly.Day.Time.StartTime.TimeOfDay > weekly.Day.Time.EndTime.TimeOfDay)
            {
                //has overnight, if we are running for next day, set now one day back
                if (NOW.TimeOfDay <= weekly.Day.Time.EndTime.TimeOfDay)
                    NOW = NOW.AddDays(-1);
            }

            if (weekly.Day.DayName.ToString() != NOW.DayOfWeek.ToString())
                return false;

            int weekOfMonth = DateTimeExtensions.GetWeekOfMonth(NOW);

            if (weekly.Day.WeekRecurrence != WeekRecurrence.Every)
            {
                if (weekly.Day.WeekRecurrence == WeekRecurrence.Last)
                {
                    if (NOW.Day <= (DateTime.DaysInMonth(NOW.Year, NOW.Month) - 7))
                        return false;
                }

                else if (weekOfMonth != (int)weekly.Day.WeekRecurrence)
                    return false;
            }

            //week related checks are done, just need to check if time is fine now
            weekly.Day.Time.StartTime = new DateTime(NOW.Year, NOW.Month, NOW.Day).Add(weekly.Day.Time.StartTime.TimeOfDay);
            weekly.Day.Time.EndTime = new DateTime(NOW.Year, NOW.Month, NOW.Day).Add(weekly.Day.Time.EndTime.TimeOfDay);

            return IsTimeToRunNow(AdjustIfOvernight(weekly.Day.Time), OriginalNOW);
        }

        private static Time GetDailyTime(JobScheduleDaily dailySchedule, DateTime NOW)
        {
            if (dailySchedule.Time.StartTime.TimeOfDay > dailySchedule.Time.EndTime.TimeOfDay)
            {
                //has overnight, if we are running for next day, set now one day back
                if (NOW.TimeOfDay <= dailySchedule.Time.EndTime.TimeOfDay)
                    NOW = NOW.AddDays(-1);
            }
            dailySchedule.Time.StartTime = new DateTime(NOW.Year, NOW.Month, NOW.Day).Add(dailySchedule.Time.StartTime.TimeOfDay);
            dailySchedule.Time.EndTime = new DateTime(NOW.Year, NOW.Month, NOW.Day).Add(dailySchedule.Time.EndTime.TimeOfDay);
            return AdjustIfOvernight(dailySchedule.Time);
        }
        private static Time GetMonthlyTime(JobScheduleMonthly monthly, DateTime NOW)
        {
            if (monthly.Day.Time.StartTime.TimeOfDay > monthly.Day.Time.EndTime.TimeOfDay)
            {
                //has overnight, if we are running for next day, set now one day back
                if (NOW.TimeOfDay <= monthly.Day.Time.EndTime.TimeOfDay)
                    NOW = NOW.AddDays(-1);
            }

            if (DateTime.DaysInMonth(NOW.Year, NOW.Month) < monthly.Day.Day)
                return null;
            monthly.Day.Time.StartTime = new DateTime(NOW.Year, NOW.Month, monthly.Day.Day).Add(monthly.Day.Time.StartTime.TimeOfDay);
            monthly.Day.Time.EndTime = new DateTime(NOW.Year, NOW.Month, monthly.Day.Day).Add(monthly.Day.Time.EndTime.TimeOfDay);
            return AdjustIfOvernight(monthly.Day.Time);
        }

        private static Time GetYearlyTime(JobScheduleYearly yearly, DateTime NOW)
        {
            yearly.Month.Time.StartTime = new DateTime(NOW.Year, (int)yearly.Month.MonthName + 1, yearly.Month.Day).Add(yearly.Month.Time.StartTime.TimeOfDay);
            yearly.Month.Time.EndTime = new DateTime(NOW.Year, (int)yearly.Month.MonthName + 1, yearly.Month.Day).Add(yearly.Month.Time.EndTime.TimeOfDay);
            return AdjustIfOvernight(yearly.Month.Time);
        }
        private static bool IsTimeToRunNow(Time time, DateTime NOW)
        {
            if (time == null)
                return false;
            //LogWarning("StarTime: " + time.StartTime.ToString() + "\tEndTime: " + time.EndTime.ToString());
            if ((NOW >= time.StartTime) && (NOW <= time.EndTime))
                return true;
            else
                return false;
        }
    }
}