using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Technisient;

namespace TestTask
{
    public class Task2 : TaskBase
    {
        public override object Run(object input)
        {
            LogDebug("Input to Task2: " + input.ToString());// TRACE <  DEBUG < INFO < WARN < ERROR < FATAL

            long  op = (long)input * Number1;

            LogInfo(input.ToString() + " * " + Number1.ToString() + " = " + Convert.ToString(op));
         
            return op;
           
        }

        [SamayParameter(LabelText = "Num 1",Help = "num1", DefaultValue = "5", Index = 1, IsRequired = true)]
        public long Number1 { get; set; }
    }
}
