using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Technisient;

namespace Task
{
    public class Task1:TaskBase
    {
        public override object Run(object input)
        {
            // TRACE <  DEBUG < INFO < WARN < ERROR < FATAL

            LogDebug("Starting Task 1!");
            long op = Number1 + Number2;
            LogInfo(Number1.ToString() + " + " + Number2.ToString() + " = " + Convert.ToString(op));
            return op;
        }

        [SamayParameter(Help = "num1", LabelText = "Num 1")]
        public long Number1 { get; set; }

        [SamayParameter(Help = "num2", LabelText = "Num 2")]
        public long Number2 { get; set; }
    }
}
