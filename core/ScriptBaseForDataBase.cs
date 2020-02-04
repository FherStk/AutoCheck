using System;
using System.IO;
using System.Linq;

namespace AutomatedAssignmentValidator.Core{
    public abstract class ScriptBaseForDataBase<T>: ScriptBase<T> where T: Core.CopyDetectorBase, new(){
        protected string Host {get; set;}                          
        protected string DataBase {get; set;}
        protected string Username {get; set;}
        protected string Password {get; set;}

        public ScriptBaseForDataBase(string[] args): base(args){        
            this.BeforeSingleStarted += BeforeSingleStartedEventHandler;
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
        
        private void BeforeSingleStartedEventHandler(Object sender, SingleEventArgs e)
        {
            //DB Creation
            AutomatedAssignmentValidator.Utils.DataBase dbUtils = new AutomatedAssignmentValidator.Utils.DataBase(this.Host, this.DataBase, this.Username, this.Password, this.Output);
            Output.WriteLine(string.Format("Checking the ~{0}~ for the student ~{1}: ", DataBase, Utils.MoodleFolderToStudentName(e.Path)), ConsoleColor.DarkYellow); 
            Output.Indent();            
            Output.Write(string.Format("The database exists on server: ", DataBase), ConsoleColor.DarkYellow); 
            if(dbUtils.ExistsDataBase()) Output.WriteResponse();
            else{
                Output.WriteResponse(string.Empty);
                Output.Write(string.Format("Creating the database: ", DataBase), ConsoleColor.DarkYellow); 
                try{
                    dbUtils.CreateDataBase(Directory.GetFiles(e.Path, "*.sql", SearchOption.AllDirectories).FirstOrDefault());
                    Output.WriteResponse();
                }
                catch(Exception ex){
                    Output.WriteResponse(ex.Message);
                }                
            } 

            Output.UnIndent();            
        }         
        public override void Single(){
            base.Single();
        }                               
    }
}