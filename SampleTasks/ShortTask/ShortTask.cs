using System;
using System.Collections.Generic;
using System.Text;
using Technisient;

namespace JobsNS
{
    class ShortTask : TaskBase
    {
        public override object Run(object input)
        {
            //Random rnd = new Random(DateTime.Now.Millisecond);
            //int a = rnd.Next(1,100);
            //int b = rnd.Next(1, 100);
            //int c = a * b;
            //return c;

            decimal number3=0;

            switch (operation)
            {
                case OP.Add:
                    number3 = number1 + number2;
                    break;
                case OP.Subtract:
                    number3 = number1 - number2;
                    break;
                case OP.Multiply:
                    number3 = number1 * number2;
                    break;
                case OP.Divide:
                    number3 = number1 / number2;
                    break;
                default:
                    break;
            }

            LogInfo(number1.ToString() + " " + operation.ToString() + " " + Number2.ToString() + " = "  + number3.ToString());
           
            return number3;
        }

          decimal number1;
        [SamayParameter(LabelText="Number 1", Index=1,IsRequired=true,
            MinValue="1", MaxValue="1000")]
          public decimal Number1
          {
              get { return number1; }
              set { number1 = value; }
          }

          decimal number2;
          [SamayParameter(LabelText = "Number 2", Index = 3, IsRequired = true,
            MinValue = "1", MaxValue = "1000")]
          public decimal Number2
          {
              get { return number2; }
              set { number2 = value; }
          }

          OP operation;

        [SamayParameter(LabelText="Operation", Index=2, IsRequired = true,DefaultValue="Multiply")]
          public OP Operation
          {
              get { return operation; }
              set { operation = value; }
          }

          public enum OP
          {
              Add,
              Subtract,
              Multiply,
              Divide
          }
    }
}
