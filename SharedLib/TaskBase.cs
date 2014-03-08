using System;
using System.Collections.Generic;
using System.Reflection;
using System.ServiceModel;

namespace Technisient
{
    // http://msdn.microsoft.com/en-us/library/scsyfw1d(VS.71).aspx
    //if you anticipate creating multiple versions of your component, create an abstract class.
    //Abstract classes provide a simple and easy way to version your components.
    //By updating the base class, all inheriting classes are automatically updated with the change.
    //Interfaces, on the other hand, cannot be changed once created. If a new version of an interface is required, you must create a whole new interface.

    /// <summary>
    ///
    /// </summary>
    ///
    [Serializable]
    public abstract class TaskBase : MarshalByRefObject
    {
        //FUTURE: Provide for Impersonate thru config website

        private ISamayEngineListener samayEngineListenerProxy;

        private TaskContext taskContext;

        public TaskBase()
        {
            samayEngineListenerProxy = new TestListener();
            LoggingLevel = LogLevel.Trace;
        }

        public TaskContext _taskContext
        {
            get { return taskContext; }
            set { taskContext = value; }
        }

        public LogLevel LoggingLevel
        {
            set { samayEngineListenerProxy.LoggerLevel(value); }
        }

        public void AssignParameters(Dictionary<string, List<string>> taskParams)
        {
            foreach (string p in taskParams.Keys)
            {
                try
                {
                    PropertyInfo pI = this.GetType().GetProperty(p);
                    switch (pI.PropertyType.FullName)
                    {
                        //FUTURE: Other cases
                        case "System.Boolean":
                            pI.SetValue(this, Convert.ToBoolean(taskParams[p][0]), null);
                            break;
                        //   case "System.Byte":
                        //     break;
                        // case "System.SByte":
                        //   break;
                        case "System.Char":
                            pI.SetValue(this, Convert.ToChar(taskParams[p][0]), null);
                            break;

                        case "System.Decimal":
                            pI.SetValue(this, Convert.ToDecimal(taskParams[p][0]), null);
                            break;

                        case "System.Double":
                            pI.SetValue(this, Convert.ToBoolean(taskParams[p][0]), null);
                            break;

                        case "System.Single":
                            pI.SetValue(this, Convert.ToSingle(taskParams[p][0]), null);
                            break;

                        case "System.Int32":
                            pI.SetValue(this, Convert.ToInt32(taskParams[p][0]), null);
                            break;

                        case "System.UInt32":
                            pI.SetValue(this, Convert.ToUInt32(taskParams[p][0]), null);
                            break;

                        case "System.Int64":
                            pI.SetValue(this, Convert.ToInt64(taskParams[p][0]), null);
                            break;

                        case "System.UInt64":
                            pI.SetValue(this, Convert.ToUInt64(taskParams[p][0]), null);
                            break;
                        //case "System.Object":
                        //  break;
                        case "System.Int16":
                            pI.SetValue(this, Convert.ToInt16(taskParams[p][0]), null);
                            break;

                        case "System.UInt16":
                            pI.SetValue(this, Convert.ToUInt16(taskParams[p][0]), null);
                            break;

                        case "System.String":
                            pI.SetValue(this, taskParams[p][0], null);
                            break;

                        default:
                            if (pI.PropertyType.BaseType == typeof(System.Enum))
                            {
                                pI.SetValue(this, Enum.Parse(this.GetType().GetProperty(p).PropertyType, taskParams[p][0], false), null);
                                break;
                            }
                            else
                                throw new Exception("Unsupported type defined: " + pI.PropertyType.FullName +
                                    "\n" + @"Permitted Types are: Boolean, Char, Decimal, Double, Single,
                            Int32, UInt32, Int64, UInt64, Int16, UInt16, String, ENUM");
                    }
                }
                catch
                {
                    throw new Exception("'" + this.GetType().FullName + "' Class does not have the Property '" + p + "' defined in the configuration.");
                }
            }
        }

        public virtual string Description()
        {
            return this.GetType().FullName;
        }

        public void InitListener(string listener)
        {
            NetNamedPipeBinding npb = new NetNamedPipeBinding();

            npb.ReaderQuotas.MaxStringContentLength = 2048000;
            ChannelFactory<ISamayEngineListener> samayEngineListener = new ChannelFactory<ISamayEngineListener>(
                npb, new EndpointAddress(listener));

            samayEngineListenerProxy = samayEngineListener.CreateChannel();
        }
        public abstract object Run(object input);
        //String/Char will show up as a textbox
        //Enums: Dropdownbox
        //Boolean: Checkbox
        //DateTime: Calendar
        //Decimal/Double/Single/Int32/UInt32/Int64/UInt64/Int16/UInt16: Numeric textbox
        //No support for arrays

        //TODO: In the Web UI, disable and highlight Jobs who has tasks which are not Valid - eg. has some property TYPE which we do no support, or has some Attribute which is not correct for given TYPE (eg. incorrect max min value for datetime)

        [System.AttributeUsage(System.AttributeTargets.Property)]
        public class SamayParameter : System.Attribute
        {
            private string defaultValue;
            private string help;

            private int index;

            private bool isRequired;

            private string labelText;

            private int maxlength;

            private string maxValue;

            private int minlength;

            private string minValue;

            private string regex;

            private string validationMessage;

            public SamayParameter()
            {
            }

            /// <summary>
            /// The value field will be pre-filled with the DefaultValue of appropriate type based on property
            /// </summary>
            public string DefaultValue
            {
                get { return defaultValue; }
                set { defaultValue = value; }
            }

            /// <summary>
            /// The text to be shown on the tooltip help icon
            /// </summary>
            public string Help
            {
                get { return help; }
                set { help = value; }
            }
            /// <summary>
            /// The order in which the controls will be displayed. As this is optional, it will first place the Properties which have it set and then place the other properties
            /// </summary>
            public int Index
            {
                get { return index; }
                set { index = value; }
            }

            /// <summary>
            /// If your task needs to have this value to function, set to true. If it is optional, set it to false.
            /// </summary>
            public bool IsRequired
            {
                get { return isRequired; }
                set { isRequired = value; }
            }

            /// <summary>
            /// The text on the label for the given property
            /// </summary>
            public string LabelText
            {
                get { return labelText; }
                set { labelText = value; }
            }
            ////////////////////////////////////////////////////////////
            /// <summary>
            /// Maximum number of characters acceptable for the value
            /// </summary>
            public int Maxlength
            {
                get { return maxlength; }
                set { maxlength = value; }
            }

            /// <summary>
            /// Maximum value for this field. Format varies based on property type
            /// </summary>
            public string MaxValue
            {
                get { return maxValue; }
                set { maxValue = value; }
            }

            /// <summary>
            /// Minimum number of characters acceptable for the value
            /// </summary>
            public int Minlength
            {
                get { return minlength; }
                set { minlength = value; }
            }

            /// <summary>
            /// Minimum value for this field. Format varies based on property type
            /// </summary>
            public string MinValue
            {
                get { return minValue; }
                set { minValue = value; }
            }

            /// <summary>
            /// Regular Expression (Javascript Regex) for validation
            /// </summary>
            public string Regex
            {
                get { return regex; }
                set { regex = value; }
            }
            /// <summary>
            /// Validation message to be shown to end user in case one of the above validations fails
            /// </summary>
            public string ValidationMessage
            {
                get { return validationMessage; }
                set { validationMessage = value; }
            }
            //public enum ControlType
            //{
            //    Calendar,
            //    CheckBox,
            //    Label,
            //    NumericTextBox,  //http://msdn.microsoft.com/en-us/library/ad548tzy.aspx
            //   // RadioButton,
            //    TextBox,
            //    //CheckBoxList, //??
            //    DropDownList//,  //??
            //    //ListBox,  //??
            //    //RadioButtonList //??
            //}
        }

        #region LOG

        public void LogDebug(string logMsg)
        {
            samayEngineListenerProxy.Log(logMsg, taskContext.jobName, this.GetType().FullName, taskContext.taskId, LogLevel.Debug);
        }

        public void LogError(string logMsg)
        {
            samayEngineListenerProxy.Log(logMsg, taskContext.jobName, this.GetType().FullName, taskContext.taskId, LogLevel.Error);
        }
        public void LogFatal(string logMsg)
        {
            samayEngineListenerProxy.Log(logMsg, taskContext.jobName, this.GetType().FullName, taskContext.taskId, LogLevel.Fatal);
        }

        public void LogInfo(string logMsg)
        {
            samayEngineListenerProxy.Log(logMsg, taskContext.jobName, this.GetType().FullName, taskContext.taskId, LogLevel.Info);
        }

        public void LogTrace(string logMsg)
        {
            samayEngineListenerProxy.Log(logMsg, taskContext.jobName, this.GetType().FullName, taskContext.taskId, LogLevel.Trace);
        }

        public void LogWarning(string logMsg)
        {
            samayEngineListenerProxy.Log(logMsg, taskContext.jobName, this.GetType().FullName, taskContext.taskId, LogLevel.Warn);
        }

        #endregion LOG
    }
}