using System;
using System.IO;
using System.Linq;

namespace AutomatedAssignmentValidator.Core{
    public abstract class ScriptDB<T>: Script<T> where T: Core.CopyDetector, new(){
        protected string Host {get; set;}                          
        protected string DataBase {get; set;}        
        protected string Username {get; set;}
        protected string Password {get; set;}
        protected string DBPrefix {get; set;}

        public ScriptDB(string[] args): base(args){        
            this.BeforeSingleStarted += BeforeSingleStartedEventHandler;
            this.AfterSingleFinished += AfterSingleFinishedEventHandler;
            this.DBPrefix = this.GetType().Name.Split("_").Last().ToLower();
        }       
        protected override void DefaultArguments(){  
            //Note: this cannot be on constructor, because the arguments introduced on command line should prevail:
            //  1. Default base class values
            //  2. Inherited class values
            //  3. Command line argument values
            
            this.CpThresh = 0.75f;
        }
        private void AfterSingleFinishedEventHandler(Object sender, SingleEventArgs e)
        {
            //Reset DB data (only avaialble within Script() execution)
            this.DataBase = null;
        }
        private void BeforeSingleStartedEventHandler(Object sender, SingleEventArgs e)
        {
            //Proceed to DB creation if needed
            this.DataBase = Utils.FolderNameToDataBase(e.Path, this.DBPrefix);
            Connectors.DataBase db = new Connectors.DataBase(this.Host, this.DataBase, this.Username, this.Password);

            Output.WriteLine(string.Format("Checking the ~{0}~ for the student ~{1}: ", this.DataBase, db.Student), ConsoleColor.DarkYellow); 
            Output.Indent();            
            Output.Write(string.Format("The database exists on server: ", DataBase)); 
            if(db.ExistsDataBase()) Output.WriteResponse();
            else{
                Output.WriteResponse(string.Empty);
                Output.Write(string.Format("Creating the database: ", DataBase)); 
                try{
                    db.CreateDataBase(Directory.GetFiles(e.Path, "*.sql", SearchOption.AllDirectories).FirstOrDefault());
                    Output.WriteResponse();
                }
                catch(Exception ex){
                    Output.WriteResponse(ex.Message);
                }                
            } 

            Output.UnIndent(); 
            Output.BreakLine();           
        }         
        public override void Run(){
            Output.WriteLine(string.Format("Running ~{0}~ for the student ~{1}: ", this.GetType().Name, Utils.DataBaseNameToStudentName(this.DataBase)), ConsoleColor.DarkYellow);
        }                               
    }
}