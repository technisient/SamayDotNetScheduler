using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.Reflection;
using Technisient;

namespace JobsNS
{  
    public class EchoJob : TaskBase
    {
        public override object Run(object input)
        {
            if (input != null)
            {
                LogInfo(input.ToString());
                return input.ToString();
            }
            else
                return "No Task Input Found!";
        }

        //example of Task with no properties!

    }
}
