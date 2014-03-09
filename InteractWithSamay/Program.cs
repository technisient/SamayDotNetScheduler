using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Technisient;
using Technisient.SamayConfig;

namespace InteractWithSamay
{
    public class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                SamaySharedLib samay = new SamaySharedLib();

                //**********  Read Current Engine Settings  *****************

                string c = samay.GetConfig();
                Console.WriteLine(c);
                Engine engine = JsonConvert.DeserializeObject<Engine>(c);

                //***********************************************************



                //**********  Removing a Job  *********************

                // samay.RemoveJob("New Task 2");

                //*************************************************


                //**********  Adding a Job  *********************

                Job j = GetNewJob();
                samay.AddJob(JsonConvert.SerializeObject(j), "Ading a new job for xyz");

                samay.ReloadConfig(); //Very important step! Won't use the new settings until next engine restart otherwise

                //******************************************

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine("Done!");
            Console.ReadKey();
        }

        private static Job GetNewJob()
        {
            Job job2 = new Job();
            job2.JobName = "New Task 2";

            job2.Interval = new JobInterval { Interval_msec = 10000 };
            job2.Schedules.Add(new JobSchedule
            {
                JobScheduleType = JobScheduleTypeEnum.JobScheduleDaily,
                ScheduleConfig = new JobScheduleDaily
                {
                    Time = new Time
                    {
                        StartTime = new DateTime(1, 1, 1, 0, 10, 0),
                        EndTime = new DateTime(1, 1, 1, 23, 55, 0),
                        EndTimeSpecified = true
                    }
                }
            });

            Task taskA = new Task
            {
                ClassName = "Task.Task1",
                LogLevel = Technisient.LogLevel.Trace,
                param = new List<TaskParameter>{
                    new TaskParameter{
                        Name= "Number1",
                        Value = new List<string>{"8"}
                    },
                    new TaskParameter{
                        Name= "Number2",
                        Value = new List<string>{"25"}
                    }
                }
            };

            // job2.TaskChain = new TaskChain { Task = new List<Task> { taskA } };

            //*******************************
            //    Optional - Task chaining
            //*******************************

            Task taskB = new Task
            {
                ClassName = "Task.Task2",
                LogLevel = Technisient.LogLevel.Trace,
                param = new List<TaskParameter>{
                    new TaskParameter{
                        Name= "Number1",
                        Value = new List<string>{"58"}
                    }
                }
            };

            job2.TaskChain = new TaskChain { Task = new List<Task> { taskA, taskB } };

            return job2;
        }

    }
}