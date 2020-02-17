using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator.Core{
    public abstract class Script<T> where T: Core.CopyDetector, new(){    
        protected string Path {get; set;}   
        protected float CpThresh {get; set;}
        private float Success {get; set;}
        private float Fails {get; set;}
        private List<string> Errors {get; set;}
        private float Points {get; set;}
        public float Score {get; private set;}   
        public bool IsQuestionOpen  {
            get{
                return this.Errors != null;
            }
        }  

        public Script(string[] args){
            DefaultArguments();
            LoadArguments(args);            
        }
        protected virtual void DefaultArguments(){
            this.CpThresh = 1.0f;
        }        
        private void LoadArguments(string[] args){          
            string[] ignore = new string[]{"script", "target"};
            for(int i = 0; i < args.Length; i++){
                if(args[i].StartsWith("--") && args[i].Contains("=")){
                    string[] data = args[i].Split("=");
                    string name = data[0].ToLower().Trim().Replace("\"", "").Substring(2);
                    string value = data[1].Trim().Replace("\"", "");
                    
                    if(!ignore.Contains(name)){
                        try{
                            this.GetType().GetProperty(name, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, value);
                        }
                        catch{
                            throw new Exception(string.Format("The parameter '{0}' could not be binded with any '{1}' property.", name, this.GetType().Name));
                        }                    
                    }
                }                                
            }
        }        

        public virtual void Batch(){    
            if(string.IsNullOrEmpty(Path)) 
                Output.Instance.WriteLine(string.Format("A 'path' argument must be provided when using --target='batch'.", Path), ConsoleColor.Red);               

            else if(!Directory.Exists(Path)) 
                Output.Instance.WriteLine(string.Format("The provided path '{0}' does not exist.", Path), ConsoleColor.Red);   
                
            else{ 
                //Step 1: Unzip all the files within the students folders                            
                UnZip();

                //Step 2: Load all the students data in order to detect potential copies
                T cd = CopyDetection();
                            
                //Step 3: Loop through the students folders
                string batchPath = this.Path;
                foreach(string f in Directory.EnumerateDirectories(Path))
                {                   
                    try{            
                        //Step 3.1: Reset score data
                        this.Path = f;
                        Clean();
                        
                        //Step 3.2: Run if no copy detected, otherwise display the copies
                       if(cd.CopyDetected(f, CpThresh)){
                            PrintCopies(cd, f);
                            Output.Instance.WriteLine("Script execution aborted by the copy detector! ", ConsoleColor.Red);                            
                        } 
                        else{
                            Output.Instance.Write("No copy detected for the student ", ConsoleColor.DarkGreen);     
                            Output.Instance.WriteLine(string.Format("~{0}", Utils.FolderNameToStudentName(f)), ConsoleColor.Yellow);                            
                            Run();
                        } 
                        
                        //Step 3.2: Run the script (with pre and post events)                                                                                           
                        this.Path = batchPath;
                    }
                    catch (Exception e){
                        Output.Instance.WriteResponse(string.Format("ERROR {0}", e.Message));
                    }
                    finally{    
                        //Output.Instance.UnIndent();  

                        //Step 3.3: Wait
                        Output.Instance.BreakLine();
                        Output.Instance.WriteLine("Press any key to continue...");
                        Console.ReadLine();                          
                    }
                }  
            } 
        }
        /// <summary>
        /// This method contains the main script to run for a single student.
        /// It will be automatically invoked, so avoid manual calls and just implement the method within your script.
        /// </summary>          
        public virtual void Run(){
            Output.Instance.WriteLine(string.Format("Running ~{0}: ", this.GetType().Name), ConsoleColor.DarkYellow);
        }   

        /// <summary>
        /// This method can be used in order to clean data before running a script for a single student.
        /// It will be automatically invoked (only if needed), so avoid manual calls and just implement the method within your script.
        /// </summary>
        protected virtual void Clean(){
            this.Success = 0;
            this.Fails = 0;       
            this.Points = 0;   
            this.Score = 0;  
            this.Errors = new List<string>();
        }

        /// <summary>
        /// Opens a new question, so all the computed score within "EvalQuestion" method will belong to this one.
        /// Warning: It will cancell any previous question if it's open, so its computed score will be lost.
        /// </summary>
        /// <param name="caption"></param>
        /// <param name="score"></param>   
        protected void OpenQuestion(string caption, float score=0){
           OpenQuestion(caption, string.Empty, score);
        }
        /// <summary>
        /// Opens a new question, so all the computed score within "EvalQuestion" method will belong to this one.
        /// Warning: It will cancell any previous question if it's open, so its computed score will be lost.
        /// </summary>
        /// <param name="caption"></param>
        /// <param name="score"></param>   
        protected void OpenQuestion(string caption, string description, float score=0){
            if(IsQuestionOpen){
                CancelQuestion();
                Output.Instance.BreakLine();
            } 

            if(score > 0) caption = string.Format("{0} [{1} {2}]", caption, score, (score > 1 ? "points" : "point"));
            if(!string.IsNullOrEmpty(description)) caption = string.Format("{0} - {1}", caption, description);            
            Output.Instance.WriteLine(string.Format("{0}:", caption), ConsoleColor.Cyan);
            Output.Instance.Indent();                        
                       
            this.Errors = new List<string>();                
            this.Points = score;
        }
        protected void CancelQuestion(){
            this.Errors = null;
        }
        /// <summary>
        /// Closes the currently open question.
        /// </summary>
        /// <param name="caption"></param>
        protected void CloseQuestion(string caption = null){       
            if(!string.IsNullOrEmpty(caption)) Output.Instance.WriteLine(caption);                             
            Output.Instance.UnIndent();            
            
            if(IsQuestionOpen){
                Output.Instance.BreakLine();
                                
                if(this.Errors.Count == 0) this.Success += this.Points;
                else this.Fails += this.Points;
                
                this.Errors = null;
                
                float total = Success + Fails;
                this.Score = (total > 0 ? (Success / total)*10 : 0);      
            }            
        }
        protected void EvalQuestion(List<string> errors){
            if(IsQuestionOpen){
                this.Errors.AddRange(errors);
                Output.Instance.WriteResponse(errors);
            }
        }        
        protected void PrintScore(){
            Output.Instance.Write("TOTAL SCORE: ", ConsoleColor.Cyan);
            Output.Instance.Write(Math.Round(Score, 2).ToString(), (Score < 5 ? ConsoleColor.Red : ConsoleColor.Green));
            Output.Instance.BreakLine();
        }  
        private void UnZip(){
            Output.Instance.WriteLine("Unzipping files: ");
            Output.Instance.Indent();

            foreach(string f in Directory.EnumerateDirectories(Path))
            {
                try{
                    Output.Instance.WriteLine(string.Format("Unzipping files for the student ~{0}: ", Utils.FolderNameToStudentName(f)), ConsoleColor.DarkYellow);
                    Output.Instance.Indent();

                    string zip = Directory.GetFiles(f, "*.zip", SearchOption.AllDirectories).FirstOrDefault();    
                    if(string.IsNullOrEmpty(zip)) Output.Instance.WriteLine("Done!");
                    else{
                        try{
                            Output.Instance.Write("Unzipping the zip file... ");
                            Connectors.Zip.ExtractFile(zip);
                            Output.Instance.WriteResponse();
                        }
                        catch(Exception e){
                            Output.Instance.WriteResponse(string.Format("ERROR {0}", e.Message));                           
                            continue;
                        }
                                                
                        try{
                            Output.Instance.Write("Removing the zip file... ");
                            File.Delete(zip);
                            Output.Instance.WriteResponse();
                        }
                        catch(Exception e){
                            Output.Instance.WriteResponse(string.Format("ERROR {0}", e.Message));
                            continue;
                        }                                                                    
                    }                    
                }
                catch (Exception e){
                    Output.Instance.WriteResponse(string.Format("ERROR {0}", e.Message));
                }
                finally{    
                    Output.Instance.UnIndent();
                    Output.Instance.BreakLine();
                }
            }
            
            if(Directory.EnumerateDirectories(Path).Count() == 0){
                Output.Instance.WriteLine("Done!");
                Output.Instance.BreakLine();
            } 
                
            Output.Instance.UnIndent();            
        }
        private T CopyDetection(){           
            //TODO: This can be incompatible with some custom scripts (for example, one which no uses files at all...).
            //      Do virtual and move this behaviour to the CopyDetector code, so each script will use its own.

            T cd = new T();            
            Output.Instance.WriteLine("Loading the copy detector: ");
            Output.Instance.Indent();
            
            foreach(string f in Directory.EnumerateDirectories(Path))
            {
                try{
                    Output.Instance.Write(string.Format("Loading files for the student ~{0}... ", Utils.FolderNameToStudentName(f)), ConsoleColor.DarkYellow);                    
                    cd.Load(f);
                    Output.Instance.WriteResponse();
                }
                catch (Exception e){
                    Output.Instance.WriteResponse(e.Message);
                }                
            }            
            
            try{
                Output.Instance.Write("Comparing files... ");                    
                                
                if(cd.Count > 0) cd.Compare();
                Output.Instance.WriteResponse();
            }
            catch (Exception e){
                Output.Instance.WriteResponse(string.Format("ERROR {0}", e.Message));
            }
            
            Output.Instance.UnIndent();
            Output.Instance.BreakLine();             
            
            return cd;
        }                           
        private void PrintCopies(T cd, string folder){
            Output.Instance.Write("Potential copy detected for the student ", ConsoleColor.Red);                            
            Output.Instance.WriteLine(string.Format("~{0}: ", Utils.FolderNameToStudentName(folder)), ConsoleColor.Yellow);                            
            Output.Instance.Indent();

            foreach(var item in cd.GetDetails(folder)){
                Output.Instance.Write(string.Format("Matching with ~{0}~ from the student ~{1}~: ", System.IO.Path.GetFileName(item.source), item.student), ConsoleColor.Yellow);     
                Output.Instance.WriteLine(string.Format("~{0:P2} ", item.match), (item.match < CpThresh ? ConsoleColor.Green : ConsoleColor.Red));
            }
            
            Output.Instance.UnIndent();
            Output.Instance.BreakLine();
        }
    }
}