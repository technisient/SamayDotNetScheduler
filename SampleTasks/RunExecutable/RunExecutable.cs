using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Technisient;

namespace JobsNS
{
    class RunExecutable : TaskBase
    {
        string arg;
        [SamayParameter(LabelText="Arguments", Index=1)]
        public string Arg
        {
            get { return arg; }
            set { arg = value; }
        }

        string executable;
        [SamayParameter(LabelText="Executable Path", Help="Execuatble to run",Regex=@"\b\w+(.exe\b)",
            ValidationMessage="Please enter a valid path for executable ending with .exe", IsRequired=true,Index=0)]
        public string Executable
        {
            get { return executable; }
            set { executable = value; }
        }

       
        bool createNoWindow;
          [SamayParameter(LabelText = "Create No Window")]
        public bool CreateNoWindow
        {
            get { return createNoWindow; }
            set { createNoWindow = value; }
        }

        bool useShellExecute;
          [SamayParameter(LabelText = "Use Shell Execute")]
        public bool UseShellExecute
        {
            get { return useShellExecute; }
            set { useShellExecute = value; }
        }

        string workingDirectory;
        [SamayParameter(LabelText = "Working Directory")]
        public string WorkingDirectory
        {
            get { return workingDirectory; }
            set { workingDirectory = value; }
        }


        string temp;

        public string Temp
        {
            get { return temp; }
            set { temp = value; }
        }

        public override object Run(object input)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.Arguments = arg;
            psi.CreateNoWindow = createNoWindow;
            psi.UseShellExecute = useShellExecute;

            if (!string.IsNullOrEmpty(workingDirectory))
                psi.WorkingDirectory = workingDirectory;
            psi.FileName = executable;

            Process p = Process.Start(psi);
            return true;
        }

    }
}
