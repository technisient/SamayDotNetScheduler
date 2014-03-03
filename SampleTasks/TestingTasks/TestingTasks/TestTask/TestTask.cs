using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Technisient;


namespace Task
{
    class TestTask
    {
        static void Main(string[] args)
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

            Console.WriteLine("Done!");
            Console.Read();
        }
      
    }
}
