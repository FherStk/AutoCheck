using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator.Core{
    public abstract class Script<T> where T: Core.CopyDetector, new(){
        public class SingleEventArgs: EventArgs
        {
            public SingleEventArgs(string p) { Path = p; }
            public String Path { get; } // readonly
        }
        public event EventHandler BeforeBatchStarted;
        public event EventHandler AfterBatchFinished;
        public event EventHandler<SingleEventArgs> BeforeSingleStarted;
        public event EventHandler<SingleEventArgs> AfterSingleFinished;

        protected Score Score {get; set;}
        protected string Path {get; set;}   
        protected float CpThresh {get; set;}

        public Script(string[] args){
            this.Score = new Score();                                              

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
            BeforeBatchStarted?.Invoke(this, new EventArgs());

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
                foreach(string f in Directory.EnumerateDirectories(Path))
                {                   
                    try{            
                        //Step 3.1: Reset data and write copy matches (if detected)
                        this.Score = new Score();   
                        if(cd.CopyDetected(f, CpThresh)){ 
                            PrintCopies(cd, f);
                        }
                        
                        //Step 3.2: Run the script (with pre and post events)
                        BeforeSingleStarted?.Invoke(this, new SingleEventArgs(f));
                        Run();
                        AfterSingleFinished?.Invoke(this, new SingleEventArgs(f));                                                                                                               
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

            AfterBatchFinished?.Invoke(this, new EventArgs());
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
            if(Score.IsOpen){
                Score.CancelQuestion();
                Output.Instance.BreakLine();
            } 

            if(score > 0) caption = string.Format("{0} [{1} {2}]", caption, score, (score > 1 ? "points" : "point"));
            if(!string.IsNullOrEmpty(description)) caption = string.Format("{0} - {1}", caption, description);
            
            Output.Instance.WriteLine(string.Format("{0}:", caption), ConsoleColor.Cyan);
            Output.Instance.Indent();                        
            Score.OpenQuestion(score);            
        }
        /// <summary>
        /// Closes the currently open question.
        /// </summary>
        /// <param name="caption"></param>
        protected void CloseQuestion(string caption = null){       
            if(!string.IsNullOrEmpty(caption)) Output.Instance.WriteLine(caption);     
                        
            Output.Instance.UnIndent();            
            
            if(Score.IsOpen){
                Output.Instance.BreakLine();
                Score.CloseQuestion();            
            }            
        }
        protected void EvalQuestion(List<string> errors){
            Score.EvalQuestion(errors);
            Output.Instance.WriteResponse(errors);
        }
        protected void PrintScore(){
            Output.Instance.Write("TOTAL SCORE: ", ConsoleColor.Cyan);
            Output.Instance.Write(Math.Round(Score.Value, 2).ToString(), (Score.Value < 5 ? ConsoleColor.Red : ConsoleColor.Green));
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
            T cd = new T();            
            Output.Instance.WriteLine("Loading files for validation: ");
            Output.Instance.Indent();
            
            foreach(string f in Directory.EnumerateDirectories(Path))
            {
                try{
                    Output.Instance.Write(string.Format("Loading files for the student ~{0}... ", Utils.FolderNameToStudentName(f)), ConsoleColor.DarkYellow);                    
                    cd.LoadFile(f);    //TODO: must be empty on generic/base class
                    Output.Instance.WriteResponse();
                }
                catch (Exception e){
                    Output.Instance.WriteResponse(e.Message);
                }                
            }            
            
            if(cd.Count == 0) Output.Instance.WriteLine("Done!");
            else{
                try{
                    Output.Instance.BreakLine();       
                    Output.Instance.Write("Validating files... ");                    

                    cd.Compare();
                    Output.Instance.WriteResponse();
                }
                catch (Exception e){
                    Output.Instance.WriteResponse(string.Format("ERROR {0}", e.Message));
                }
            }                        
            
            Output.Instance.UnIndent();
            Output.Instance.BreakLine();             
            
            return cd;
        }                           
        private void PrintCopies(T cd, string folder){
            Output.Instance.Write(string.Format("Skipping script for the student ~{0}: ", Utils.FolderNameToStudentName(folder)), ConsoleColor.DarkYellow);                            
            Output.Instance.WriteLine("Potential copy detected!", ConsoleColor.Red);
            Output.Instance.Indent();

            foreach(var item in cd.GetDetails(folder)){
                string file = System.IO.Path.GetFileName(item.file);
                string student = Utils.FolderNameToStudentName(item.file.Split("\\")[this.Path.Split("\\").Count()]);

                Output.Instance.Write(string.Format("Matching with ~{0}~ from the student ~{1}~: ", file, student), ConsoleColor.Yellow);     
                Output.Instance.WriteLine(string.Format("~{0:P2} ", item.match), (item.match < CpThresh ? ConsoleColor.Green : ConsoleColor.Red));
            }
            
            Output.Instance.UnIndent();
            Output.Instance.BreakLine();
        }
    }
}