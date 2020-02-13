using System;
using System.IO;
using System.Linq;

namespace AutomatedAssignmentValidator.Core{
    public abstract class ScriptFiles<T>: Script<T> where T: Core.CopyDetector, new(){       
        protected string Student {get; set;}

        public ScriptFiles(string[] args): base(args){                   
        }       
        protected override void DefaultArguments(){  
            //Note: this cannot be on constructor, because the arguments introduced on command line should prevail:
            //  1. Default base class values
            //  2. Inherited class values
            //  3. Command line argument values
            
            this.CpThresh = 0.75f;           
        }                
               
        public override void Run(){   
            this.Student = Core.Utils.FolderNameToStudentName(this.Path); //this.Path will be loaded by argument (single) or by batch (folder name).
            Output.Instance.WriteLine(string.Format("Running ~{0}~ for the student ~{1}: ", this.GetType().Name, this.Student), ConsoleColor.DarkYellow);
        }                               
    }
}