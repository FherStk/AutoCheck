using System;
using System.IO;
using System.Linq;

namespace AutomatedAssignmentValidator.Core{
    public abstract class ScriptBaseForDataBase<T>: ScriptBase<T> where T: Core.CopyDetectorBase, new(){
        protected string Host {get; set;}                          
        protected string DataBase {get; set;}        
        protected string Username {get; set;}
        protected string Password {get; set;}
        protected string Student {get; set;}

        public ScriptBaseForDataBase(string[] args): base(args){        
            this.BeforeSingleStarted += BeforeSingleStartedEventHandler;
            this.AfterSingleFinished += AfterSingleFinishedEventHandler;
        }

        protected override void LoadArgument(string name, string value){        
            switch(name){
                case "host":
                    this.Host = value;
                    break;

                 case "database":
                    this.DataBase = value;
                    break;

                case "username":
                    this.Username = value;
                    break;

                case "password":
                    this.Password = value;
                    break;
                
                default:
                    base.LoadArgument(name, value);
                    break;
            }  
        }
        private void AfterSingleFinishedEventHandler(Object sender, SingleEventArgs e)
        {
            //Reset DB data (only avaialble within Single execution)
            this.DataBase = null;
        }
        private void BeforeSingleStartedEventHandler(Object sender, SingleEventArgs e)
        {
            //Proceed to DB creation if needed
            this.DataBase = Utils.FolderNameToDataBase(e.Path);
            this.Student = Utils.DataBaseToStudentName(this.DataBase);
            AutomatedAssignmentValidator.Utils.DataBase dbUtils = new AutomatedAssignmentValidator.Utils.DataBase(this.Host, this.DataBase, this.Username, this.Password, this.Output);

            Output.WriteLine(string.Format("Checking the ~{0}~ for the student ~{1}: ", this.DataBase, this.Student), ConsoleColor.DarkYellow); 
            Output.Indent();            
            Output.Write(string.Format("The database exists on server: ", DataBase)); 
            if(dbUtils.ExistsDataBase()) Output.WriteResponse();
            else{
                Output.WriteResponse(string.Empty);
                Output.Write(string.Format("Creating the database: ", DataBase)); 
                try{
                    dbUtils.CreateDataBase(Directory.GetFiles(e.Path, "*.sql", SearchOption.AllDirectories).FirstOrDefault());
                    Output.WriteResponse();
                }
                catch(Exception ex){
                    Output.WriteResponse(ex.Message);
                }                
            } 

            Output.UnIndent(); 
            Output.BreakLine();           
        }         
        public override void Script(){
            Output.WriteLine(string.Format("Running ~{0}~ for the student ~{1}: ", this.GetType().Name, this.Student), ConsoleColor.DarkYellow);
        }                               
    }
}