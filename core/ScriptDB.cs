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
        protected string Student {get; set;}

        public ScriptDB(string[] args): base(args){        
            this.BeforeBatchCallToSingle += BeforeBatchCallToSingleEventHandler;
            this.AfterBatchCallToSingle += AfterBatchCalledToSingleEventHandler;
            this.DBPrefix = this.GetType().Name.Split("_").Last().ToLower();
        }       
        protected override void DefaultArguments(){  
            //Note: this cannot be on constructor, because the arguments introduced on command line should prevail:
            //  1. Default base class values
            //  2. Inherited class values
            //  3. Command line argument values
            
            this.CpThresh = 0.75f;
            this.Username = "postgres";
            this.Password = "postgres";
        }
                
        private void AfterBatchCalledToSingleEventHandler(Object sender, EventArgs e)
        {
            //Reset DB data (only avaialble within Script() execution)
            this.DataBase = null;
        }
        private void BeforeBatchCallToSingleEventHandler(Object sender, EventArgs e)
        {            
            //Proceed to DB creation if needed
            this.DataBase = Utils.FolderNameToDataBase(this.Path, this.DBPrefix);            
            Connectors.Postgres db = new Connectors.Postgres(this.Host, this.DataBase, this.Username, this.Password);            
        
            Output.Instance.WriteLine(string.Format("Checking the ~{0}~ for the student ~{1}: ", this.DataBase, db.Student), ConsoleColor.DarkYellow); 
            Output.Instance.Indent();
            
            try{
                Output.Instance.Write(string.Format("Cleaning data from previous executions: ", DataBase));                         
                Clean();
                Output.Instance.WriteResponse();
            }
            catch(Exception ex){
                Output.Instance.WriteResponse(ex.Message);
            } 
            
            if(db.ExistsDataBase()){                
                try{
                    Output.Instance.Write(string.Format("Dropping the existing database: ", DataBase)); 
                    db.DropDataBase();
                    Output.Instance.WriteResponse();
                }
                catch(Exception ex){
                    Output.Instance.WriteResponse(ex.Message);
                } 
            } 
                        
            try{
                Output.Instance.Write(string.Format("Creating the database: ", DataBase)); 
                db.CreateDataBase(Directory.GetFiles(this.Path, "*.sql", SearchOption.AllDirectories).FirstOrDefault());
                Output.Instance.WriteResponse();
            }
            catch(Exception ex){
                Output.Instance.WriteResponse(ex.Message);
            }                 

            Output.Instance.UnIndent(); 
            Output.Instance.BreakLine();           
        }         
        public override void Run(){   
            this.Student = Core.Utils.DataBaseNameToStudentName(this.DataBase); //this.DataBase will be loaded by argument (single) or by batch (folder name).
            Output.Instance.WriteLine(string.Format("Running ~{0}~ for the student ~{1}: ", this.GetType().Name, this.Student), ConsoleColor.DarkYellow);
        }                               
    }
}