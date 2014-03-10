using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Technisient;
using Technisient.SamayConfig;
using TestTask;


namespace SamayTestHarness
{
    class TestHarness
    {
        static void Main(string[] args)
        {
            AddRemoveJobsInEngine();

           // RunTasks();

            Console.WriteLine("Done!");
            Console.Read();
        }

        private static void RunTasks()
        {
            Console.WriteLine("Testing a Single Task");
            //Testing a single Task
            Task1 t1 = new Task1();

            // TRACE <  DEBUG < INFO < WARN < ERROR < FATAL
            t1.LoggingLevel = LogLevel.Info; //optional setting. Default is 'Trace' logging

            t1.Number1 = 4;
            t1.Number2 = 5;
            t1.Run(null);


            Console.WriteLine("\nTesting Task Chaining");
            //Task Chaining
            Task2 t2 = new Task2();
            t2.Number1 = 10;
            t2.LoggingLevel = LogLevel.Trace;
            t2.Run(t1.Run(null)); //passes output of t1 to t2. You can have any number of tasks in Task Chaining
        }



        private static void AddRemoveJobsInEngine()
        {
            try
            {
                SamaySharedLib samay = new SamaySharedLib();

                //**********  Read Current Engine Settings  *****************
                Console.WriteLine("Reading Config File: ");
                string c = samay.GetConfig();
                Console.WriteLine(c);
                Engine engine = JsonConvert.DeserializeObject<Engine>(c);

                //***********************************************************



                //**********  Removing a Job  *********************
                                
                 samay.RemoveJob("New Task 2");
                Console.WriteLine("Job Removed");

                //*************************************************


                //**********  Adding a Job  *********************
                                
                Job j = GetNewJob();
                samay.AddJob(JsonConvert.SerializeObject(j), "Ading a new job for xyz");
                Console.WriteLine("Job Added");

                samay.ReloadConfig(); //Very important step! Won't use the new settings until next engine restart otherwise
                Console.WriteLine("Reload Config complete");
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
