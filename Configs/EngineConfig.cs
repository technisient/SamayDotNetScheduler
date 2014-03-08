using System;
using System.Collections.Generic;

namespace Technisient.SamayConfig
{
    public enum DayName
    {
        Sunday,
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday,
    }

    public enum JobScheduleTypeEnum
    {
        JobScheduleDaily,
        JobScheduleWeekly,
        JobScheduleMonthly,
        JobScheduleYearly,
        JobScheduleOneTimeOnly,
    }

    public enum MonthName
    {
        January,
        February,
        March,
        April,
        May,
        June,
        July,
        August,
        September,
        October,
        November,
        December,
    }

    public enum WeekRecurrence
    {
        Every,
        First,
        Second,
        Third,
        Fourth,
        Fifth,
        Sixth,
        Last,
    }

    public class Engine
    {
        private EngineSettings engineConfigField;
        private List<Job> jobsField;
        private List<Task> taskDefaultsField;
        public Engine()
        {
            jobsField = new List<Job>();
            taskDefaultsField = new List<Task>();
            engineConfigField = new EngineSettings();
        }

        public EngineSettings EngineConfig
        {
            get
            {
                return this.engineConfigField;
            }
            set
            {
                this.engineConfigField = value;
            }
        }

        public List<Job> Jobs
        {
            get
            {
                return this.jobsField;
            }
            set
            {
                this.jobsField = value;
            }
        }

        public List<Task> TaskDefaults
        {
            get
            {
                return this.taskDefaultsField;
            }
            set
            {
                this.taskDefaultsField = value;
            }
        }
    }

    public class EngineSettings
    {
        private bool enabledField;
        private List<GlobalExcludeDate> globalExcludeDatesField;
        private LogLevel logLevelField;
        private int logRetentionDaysField;
        private int maxCPUField;
        private int sleepTicksField;
        public EngineSettings()
        {
            this.logLevelField = LogLevel.Error;
            this.sleepTicksField = 10000;
            this.enabledField = true;
            this.globalExcludeDatesField = new List<GlobalExcludeDate>();
            this.logRetentionDaysField = 60;
        }

        public bool Enabled
        {
            get
            {
                return this.enabledField;
            }
            set
            {
                this.enabledField = value;
            }
        }

        public List<GlobalExcludeDate> GlobalExcludeDates
        {
            get
            {
                return this.globalExcludeDatesField;
            }
            set
            {
                this.globalExcludeDatesField = value;
            }
        }

        public LogLevel LogLevel
        {
            get
            {
                return this.logLevelField;
            }
            set
            {
                this.logLevelField = value;
            }
        }

        public int LogRetentionDays
        {
            get
            {
                return this.logRetentionDaysField;
            }
            set
            {
                this.logRetentionDaysField = value;
            }
        }

        public int MaxCPU
        {
            get
            {
                return this.maxCPUField;
            }
            set
            {
                this.maxCPUField = value;
            }
        }

        public int SleepTicks
        {
            get
            {
                return this.sleepTicksField;
            }
            set
            {
                this.sleepTicksField = value;
            }
        }
    }

    public class GlobalExcludeDate
    {
        private System.DateTime dateField;
        private string idField;
        private string noteField;

        public GlobalExcludeDate()
        {
            idField = Guid.NewGuid().ToString();
        }

        public System.DateTime Date
        {
            get
            {
                return this.dateField;
            }
            set
            {
                this.dateField = value;
            }
        }

        public string Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
        public string Note
        {
            get
            {
                return this.noteField;
            }
            set
            {
                this.noteField = value;
            }
        }
    }

    public class Job : ICloneable
    {
        private bool doNotRunOnGlobalExcludeDatesField;
        private bool enabledField;
        private List<JobExcludeDates> excludeDatesField;
        private int executeTimesField;
        private string idField;
        private JobInterval intervalField;
        private string jobNameField;
        private List<JobSchedule> schedulesField;
        private TaskChain taskChainField;

        public Job()
        {
            this.executeTimesField = -1;
            this.doNotRunOnGlobalExcludeDatesField = true;
            this.idField = Guid.NewGuid().ToString();
            this.enabledField = true;
            this.schedulesField = new List<JobSchedule>();
            this.intervalField = new JobInterval();
            this.intervalField.Interval_msec = 60000; //default
        }

        public bool DoNotRunOnGlobalExcludeDates
        {
            get
            {
                return this.doNotRunOnGlobalExcludeDatesField;
            }
            set
            {
                this.doNotRunOnGlobalExcludeDatesField = value;
            }
        }

        public bool Enabled
        {
            get
            {
                return this.enabledField;
            }
            set
            {
                this.enabledField = value;
            }
        }

        public List<JobExcludeDates> ExcludeDates
        {
            get
            {
                return this.excludeDatesField;
            }
            set
            {
                this.excludeDatesField = value;
            }
        }

        public int ExecuteTimes
        {
            get
            {
                return this.executeTimesField;
            }
            set
            {
                this.executeTimesField = value;
            }
        }

        public string Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        public JobInterval Interval
        {
            get
            {
                return this.intervalField;
            }
            set
            {
                this.intervalField = value;
            }
        }

        public string JobName
        {
            get
            {
                return this.jobNameField;
            }
            set
            {
                this.jobNameField = value;
            }
        }
        public List<JobSchedule> Schedules
        {
            get
            {
                return this.schedulesField;
            }
            set
            {
                this.schedulesField = value;
            }
        }
        public TaskChain TaskChain
        {
            get
            {
                return this.taskChainField;
            }
            set
            {
                this.taskChainField = value;
            }
        }

        public object Clone()
        {
            // return this.MemberwiseClone();
            throw new NotImplementedException();
        }
    }

    public class JobExcludeDates
    {
        private System.DateTime dateField;

        public System.DateTime Date
        {
            get
            {
                return this.dateField;
            }
            set
            {
                this.dateField = value;
            }
        }
    }

    public class JobInterval
    {
        //EITHER msec is set or ontheclock

        private List<int> clockTimeField;

        private int interval_msecField;

        public JobInterval()
        {
            interval_msecField = -1;
            clockTimeField = null;
        }
        public List<int> ClockTime
        {
            get
            {
                return this.clockTimeField;
            }
            set
            {
                this.clockTimeField = value;
            }
        }

        public int Interval_msec
        {
            get
            {
                return this.interval_msecField;
            }
            set
            {
                this.interval_msecField = value;
            }
        }
    }

    public class JobSchedule
    {
        private string idField;
        private JobScheduleTypeEnum jobScheduleTypeField;
        private object scheduleConfigField;
        public JobSchedule()
        {
            idField = Guid.NewGuid().ToString();
        }

        public string ID
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        public JobScheduleTypeEnum JobScheduleType
        {
            get
            {
                return this.jobScheduleTypeField;
            }
            set
            {
                this.jobScheduleTypeField = value;
            }
        }

        public object ScheduleConfig
        {
            get
            {
                return this.scheduleConfigField;
            }
            set
            {
                this.scheduleConfigField = value;
            }
        }
    }

    public class JobScheduleDaily
    {
        private Time timeField;

        public Time Time
        {
            get
            {
                return this.timeField;
            }
            set
            {
                this.timeField = value;
            }
        }
    }

    public class JobScheduleMonthly
    {
        private JobScheduleMonthlyDay dayField;

        public JobScheduleMonthlyDay Day
        {
            get
            {
                return this.dayField;
            }
            set
            {
                this.dayField = value;
            }
        }
    }

    public class JobScheduleMonthlyDay
    {
        private int dayField;
        private Time timeField;

        public int Day
        {
            get
            {
                return this.dayField;
            }
            set
            {
                this.dayField = value;
            }
        }

        public Time Time
        {
            get
            {
                return this.timeField;
            }
            set
            {
                this.timeField = value;
            }
        }
    }

    public class JobScheduleOneTimeOnly
    {
        private System.DateTime endDateTimeField;
        private bool endDateTimeFieldSpecified;
        private System.DateTime startDateTimeField;
        public System.DateTime EndDateTime
        {
            get
            {
                return this.endDateTimeField;
            }
            set
            {
                this.endDateTimeField = value;
            }
        }

        public bool EndDateTimeSpecified
        {
            get
            {
                return this.endDateTimeFieldSpecified;
            }
            set
            {
                this.endDateTimeFieldSpecified = value;
            }
        }

        public System.DateTime StartDateTime
        {
            get
            {
                return this.startDateTimeField;
            }
            set
            {
                this.startDateTimeField = value;
            }
        }
    }

    public class JobScheduleWeekly
    {
        private JobScheduleWeeklyDay dayField;

        public JobScheduleWeeklyDay Day
        {
            get
            {
                return this.dayField;
            }
            set
            {
                this.dayField = value;
            }
        }
    }

    public class JobScheduleWeeklyDay
    {
        private DayName dayNameField;
        private Time timeField;
        private WeekRecurrence weekRecurrenceField;

        public DayName DayName
        {
            get
            {
                return this.dayNameField;
            }
            set
            {
                this.dayNameField = value;
            }
        }

        public Time Time
        {
            get
            {
                return this.timeField;
            }
            set
            {
                this.timeField = value;
            }
        }

        public WeekRecurrence WeekRecurrence
        {
            get
            {
                return this.weekRecurrenceField;
            }
            set
            {
                this.weekRecurrenceField = value;
            }
        }
    }

    public class JobScheduleYearly
    {
        private JobScheduleYearlyMonth monthField;

        public JobScheduleYearlyMonth Month
        {
            get
            {
                return this.monthField;
            }
            set
            {
                this.monthField = value;
            }
        }
    }

    public class JobScheduleYearlyMonth
    {
        private int dayField;
        private MonthName monthNameField;
        private Time timeField;

        public int Day
        {
            get
            {
                return this.dayField;
            }
            set
            {
                this.dayField = value;
            }
        }

        public MonthName MonthName
        {
            get
            {
                return this.monthNameField;
            }
            set
            {
                this.monthNameField = value;
            }
        }
        public Time Time
        {
            get
            {
                return this.timeField;
            }
            set
            {
                this.timeField = value;
            }
        }
    }

    public class Task
    {
        private string classNameField;
        private string idField;
        private LogLevel logLevelField;
        private List<TaskParameter> paramField;
        private TaskRetryOnError retryOnErrorField;

        public Task()
        {
            this.logLevelField = LogLevel.Error;
            this.idField = Guid.NewGuid().ToString();
        }

        public string ClassName
        {
            get
            {
                return this.classNameField;
            }
            set
            {
                this.classNameField = value;
            }
        }

        public string Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
        public LogLevel LogLevel
        {
            get
            {
                return this.logLevelField;
            }
            set
            {
                this.logLevelField = value;
            }
        }

        public List<TaskParameter> param
        {
            get
            {
                return this.paramField;
            }
            set
            {
                this.paramField = value;
            }
        }

        public TaskRetryOnError RetryOnError
        {
            get
            {
                return this.retryOnErrorField;
            }
            set
            {
                this.retryOnErrorField = value;
            }
        }
    }

    public class TaskChain
    {
        private bool continueOnErrorField;
        private List<Task> taskField;

        public bool ContinueOnError
        {
            get
            {
                return this.continueOnErrorField;
            }
            set
            {
                this.continueOnErrorField = value;
            }
        }

        public List<Task> Task
        {
            get
            {
                return this.taskField;
            }
            set
            {
                this.taskField = value;
            }
        }
    }

    public class TaskParameter
    {
        private string nameField;
        private List<string> valueField;

        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        public List<string> Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    public class TaskRetryOnError
    {
        private int retryDelay_msecField;
        private int retryTimesField;
        public TaskRetryOnError()
        {
            this.retryTimesField = 3;
            this.retryDelay_msecField = 100;
        }

        public int RetryDelay_msec
        {
            get
            {
                return this.retryDelay_msecField;
            }
            set
            {
                this.retryDelay_msecField = value;
            }
        }

        public int RetryTimes
        {
            get
            {
                return this.retryTimesField;
            }
            set
            {
                this.retryTimesField = value;
            }
        }
    }
    public class Time
    {
        private System.DateTime endTimeField;
        private bool endTimeFieldSpecified;
        private System.DateTime startTimeField;
        public System.DateTime EndTime
        {
            get
            {
                return this.endTimeField;
            }
            set
            {
                this.endTimeField = value;
            }
        }

        public bool EndTimeSpecified
        {
            get
            {
                return this.endTimeFieldSpecified;
            }
            set
            {
                this.endTimeFieldSpecified = value;
            }
        }

        public System.DateTime StartTime
        {
            get
            {
                return this.startTimeField;
            }
            set
            {
                this.startTimeField = value;
            }
        }
    }
}