using System;
using System.IO;
using System.Linq;
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
        protected float CopyThreshold {get; set;}

        public ScriptBase(string[] args){
            this.Output = new Output();
            this.Score = new Score();   

            LoadArguments(args);            
        }
        private void LoadArguments(string[] args){
            //Default values
            this.CopyThreshold = 1.0f;

            //Load from arguments
            for(int i = 0; i < args.Length; i++){
                if(args[i].StartsWith("--") && args[i].Contains("=")){
                    string[] data = args[i].Split("=");
                    string name = data[0].ToLower().Trim().Replace("\"", "").Substring(2);
                    string value = data[1].Trim().Replace("\"", "");
                    
                    LoadArgument(name, value);
                }                                
            }
        }
        protected virtual void LoadArgument(string name, string value){
            switch(name){
                case "path":
                    this.Path = value;
                    break;

                 case "cpthresh":
                    this.CopyThreshold = float.Parse(value);
                    break;
            }  
        }

        public virtual void Batch(){    
            BeforeBatchStarted?.Invoke(this, new EventArgs());

            if(string.IsNullOrEmpty(Path)) 
                Output.WriteLine(string.Format("A 'path' argument must be provided when using --target='batch'.", Path), ConsoleColor.Red);               

            else if(!Directory.Exists(Path)) 
                Output.WriteLine(string.Format("The provided path '{0}' does not exist.", Path), ConsoleColor.Red);   
                
            else{                             
                UnZip();
                T cd = CopyDetection();
                            
                foreach(string f in Directory.EnumerateDirectories(Path))
                {                   
                    try{            
                        //Some items must be reseted for every student folder
                        this.Score = new Score();   

                        if(!cd.CopyDetected(f, CopyThreshold)){
                            BeforeSingleStarted?.Invoke(this, new SingleEventArgs(f));
                            Single();
                            AfterSingleFinished?.Invoke(this, new SingleEventArgs(f));

                            Output.BreakLine();
                            Output.WriteLine("Press any key to continue...");
                            Console.ReadLine();
                        } 
                        else{
                            Output.WriteLine(string.Format("Skipping script for the student ~{0}: ", Utils.MoodleFolderToStudentName(f)), ConsoleColor.DarkYellow);                            
                            Output.Write("Potential copy detected!", ConsoleColor.DarkRed);
                            Output.Indent();

                            foreach(var item in cd.GetDetails(f)){
                                Terminal.Write(string.Format("Matching with ~{0}~ from the student ~{1}~: ", item.file, Utils.MoodleFolderToStudentName(item.file)), ConsoleColor.Yellow);     
                                Terminal.WriteLine(string.Format("~{0:P2} ", item.match), (item.match < CopyThreshold ? ConsoleColor.Green : ConsoleColor.Red));
                            }

                            Output.UnIndent();
                        }                                                
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
        public virtual void Single(){
            Output.WriteLine(string.Format("Running ~{0}: ", this.GetType().Name), ConsoleColor.DarkYellow);
        }       
        protected void OpenQuestion(string caption, float score){
            Output.WriteLine(caption);
            Output.Indent();
            Score.OpenQuestion(score);                
        }

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
    }
}