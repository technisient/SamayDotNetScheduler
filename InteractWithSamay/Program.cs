using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Technisient;
using Technisient.SamayConfig;

namespace InteractWithSamay
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                SamaySharedLib samay = new SamaySharedLib();
                string c = samay.GetConfig();
                Console.WriteLine(c);

                //  Engine engine = JsonConvert.DeserializeObject<Engine>(c);

               // samay.RemoveJob("New Task 2");

                Job j = GetNewJob();
                samay.AddJob(JsonConvert.SerializeObject(j), "Ading a new job for xyz");

              
                samay.ReloadConfig();
            }
            catch(Exception ex)
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

            //Optional - Task chaining

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
