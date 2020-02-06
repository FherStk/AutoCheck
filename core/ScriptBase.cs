using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator.Core{
    public abstract class ScriptBase<T> where T: Core.CopyDetectorBase, new(){
        public class SingleEventArgs: EventArgs
        {
            public SingleEventArgs(string p) { Path = p; }
            public String Path { get; } // readonly
        }
        public event EventHandler BeforeBatchStarted;
        public event EventHandler AfterBatchFinished;
        public event EventHandler<SingleEventArgs> BeforeSingleStarted;
        public event EventHandler<SingleEventArgs> AfterSingleFinished;

        protected Output Output {get; set;}
        protected Score Score {get; set;}
        protected string Path {get; set;}   
        protected float CpThresh {get; set;}

        public ScriptBase(string[] args){
            this.Output = new Output();
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
                Output.WriteLine(string.Format("A 'path' argument must be provided when using --target='batch'.", Path), ConsoleColor.Red);               

            else if(!Directory.Exists(Path)) 
                Output.WriteLine(string.Format("The provided path '{0}' does not exist.", Path), ConsoleColor.Red);   
                
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
                        Script();
                        AfterSingleFinished?.Invoke(this, new SingleEventArgs(f));                                                

                        //Step 3.3: Wait
                        Output.BreakLine();
                        Output.WriteLine("Press any key to continue...");
                        Console.ReadLine();                                            
                    }
                    catch (Exception e){
                        Output.WriteResponse(string.Format("ERROR {0}", e.Message));
                    }
                    finally{    
                        Output.UnIndent();                       
                    }
                }  
            } 

            AfterBatchFinished?.Invoke(this, new EventArgs());
        }          
        public virtual void Script(){
            Output.WriteLine(string.Format("Running ~{0}: ", this.GetType().Name), ConsoleColor.DarkYellow);
        }    
        /// <summary>
        /// Opens a new question, so all the computed score within "EvalQuestion" method will belong to this one.
        /// Warning: It will cancell any previous question if it's open, so its computed score will be lost.
        /// </summary>
        /// <param name="caption"></param>
        /// <param name="score"></param>   
        protected void OpenQuestion(string caption, float score=0){
            Output.WriteLine(caption, ConsoleColor.Cyan);
            Output.Indent();
            
            if(Score.IsOpen) Score.CancelQuestion();
            Score.OpenQuestion(score);
            
        }
        /// <summary>
        /// Closes the currently open question.
        /// </summary>
        /// <param name="caption"></param>
        protected void CloseQuestion(string caption = null){       
            if(!string.IsNullOrEmpty(caption)) Output.WriteLine(caption);     
            
            Output.BreakLine();
            Output.UnIndent();            
            
            Score.CloseQuestion();            
        }
        protected void EvalQuestion(List<string> errors){
            Score.EvalQuestion(errors);
            Output.WriteResponse(errors);
        }
        protected void PrintScore(){
            Output.Write("TOTAL SCORE: ", ConsoleColor.Cyan);
            Output.Write(Math.Round(Score.Value, 2).ToString(), (Score.Value < 5 ? ConsoleColor.Red : ConsoleColor.Green));
            Output.BreakLine();
        }  
        private void UnZip(){
            Output.WriteLine("Unzipping files: ");
            Output.Indent();

            foreach(string f in Directory.EnumerateDirectories(Path))
            {
                try{
                    Output.WriteLine(string.Format("Unzipping files for the student ~{0}: ", Utils.MoodleFolderToStudentName(f)), ConsoleColor.DarkYellow);
                    Output.Indent();

                    string zip = Directory.GetFiles(f, "*.zip", SearchOption.AllDirectories).FirstOrDefault();    
                    if(string.IsNullOrEmpty(zip)) Output.WriteLine("Done!");
                    else{
                        try{
                            Output.Write("Unzipping the zip file... ");
                            Utils.ExtractZipFile(zip);
                            Output.WriteResponse();
                        }
                        catch(Exception e){
                            Output.WriteResponse(string.Format("ERROR {0}", e.Message));                           
                            continue;
                        }
                                                
                        try{
                            Output.Write("Removing the zip file... ");
                            File.Delete(zip);
                            Output.WriteResponse();
                        }
                        catch(Exception e){
                            Output.WriteResponse(string.Format("ERROR {0}", e.Message));
                            continue;
                        }                                                                    
                    }                    
                }
                catch (Exception e){
                    Output.WriteResponse(string.Format("ERROR {0}", e.Message));
                }
                finally{    
                    Output.UnIndent();
                    Output.BreakLine();
                }
            }
            
            if(Directory.EnumerateDirectories(Path).Count() == 0){
                Output.WriteLine("Done!");
                Output.BreakLine();
            } 
                
            Output.UnIndent();            
        }
        private T CopyDetection(){           
            T cd = new T();            
            Output.WriteLine("Loading files for validation: ");
            Output.Indent();
            
            foreach(string f in Directory.EnumerateDirectories(Path))
            {
                try{
                    Output.Write(string.Format("Loading files for the student ~{0}... ", Utils.MoodleFolderToStudentName(f)), ConsoleColor.DarkYellow);                    
                    cd.LoadFile(f);    //TODO: must be empty on generic/base class
                    Output.WriteResponse();
                }
                catch (Exception e){
                    Output.WriteResponse(e.Message);
                }                
            }            
            
            if(cd.Count == 0) Output.WriteLine("Done!");
            else{
                try{
                    Output.BreakLine();       
                    Output.Write("Validating files... ");                    

                    cd.Compare();
                    Output.WriteResponse();
                }
                catch (Exception e){
                    Output.WriteResponse(string.Format("ERROR {0}", e.Message));
                }
            }                        
            
            Output.UnIndent();
            Output.BreakLine();             
            
            return cd;
        }                           
        private void PrintCopies(T cd, string folder){
            Output.Write(string.Format("Skipping script for the student ~{0}: ", Utils.MoodleFolderToStudentName(folder)), ConsoleColor.DarkYellow);                            
            Output.WriteLine("Potential copy detected!", ConsoleColor.Red);
            Output.Indent();

            foreach(var item in cd.GetDetails(folder)){
                string file = System.IO.Path.GetFileName(item.file);
                string student = Utils.MoodleFolderToStudentName(item.file.Split("\\")[this.Path.Split("\\").Count()]);

                Output.Write(string.Format("Matching with ~{0}~ from the student ~{1}~: ", file, student), ConsoleColor.Yellow);     
                Output.WriteLine(string.Format("~{0:P2} ", item.match), (item.match < CpThresh ? ConsoleColor.Green : ConsoleColor.Red));
            }
            
            Output.UnIndent();
            Output.BreakLine();
        }
    }
}