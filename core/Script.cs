/*
    Copyright © 2020 Fernando Porrino Serrano
    Third party software licenses can be found at /docs/credits/thirdparties.md

    This file is part of AutoCheck.

    AutoCheck is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    AutoCheck is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with AutoCheck.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace AutoCheck.Core{
    /// <summary>
    /// This class must be inherited in order to develop a generic custom script.
    /// The script is the main container for a set of instructions, which will test the correctness of an assignement.
    /// </summary>      
    /// <typeparam name="T">The copy detector that will be automatically used within the script.</typeparam>
    public abstract class Script<T> where T: Core.CopyDetector, new(){            
        /// <summary>
        /// Current path being used within an execution, automatically updated and mantained.
        /// </summary>
        /// <value></value>
        protected string Path {get; set;}   
        /// <summary>
        /// The copy thresshold value, a copy will be detected if its matching value is equal or higher to this one.
        /// It must be set up on DefaultArguments().
        /// </summary>
        /// <value></value>
        protected float CpThresh {get; set;}
        private float Success {get; set;}
        private float Fails {get; set;}
        private List<string> Errors {get; set;}
        private float Points {get; set;}
        /// <summary>
        /// Maximum score possible
        /// </summary>
        /// <value></value>
        protected float MaxScore {get; set;}
        /// <summary>
        /// The accumulated score (over 10 points), which will be updated on each CloseQuestion() call.
        /// </summary>
        /// <value></value>
        public float Score {get; private set;}   
        /// <summary>
        /// Returns if there's an open question in progress.
        /// </summary>
        /// <value></value>
        public bool IsQuestionOpen  {
            get{
                return this.Errors != null;
            }
        }  
        /// <summary>
        /// Creates a new script instance.
        /// </summary>
        /// <param name="args">Argument list, loaded from the command line, on which one will be stored into its equivalent local property.</param>
        public Script(string[] args){
            DefaultArguments();
            LoadArguments(args);            
        }
        /// <summary>
        /// Sets up the default arguments values, can be overwrited if custom arguments are needed.
        /// </summary>
        protected virtual void DefaultArguments(){
            this.CpThresh = 1.0f;
            this.MaxScore = 10f;
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
        /// <summary>
        /// This method will loop through a set of students assignments, running the script for all of them.
        /// All the assignments will be unziped, loaded into the local copy detector, and all the data from previos executions will be restored prior the script execution.
        /// If a potential copy is detected, the script execution will be skipped.
        /// </summary>
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
                        Output.Instance.WriteResponse(e.Message);
                        Output.Instance.ResetIndent();
                    }
                    finally{    
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
        /// </summary>          
        public virtual void Run(){
            Output.Instance.WriteLine(string.Format("Running ~{0}: ", this.GetType().Name), ConsoleColor.DarkYellow);
        }   

        /// <summary>
        /// This method can be used in order to clean data before running a script for a single student.
        /// It will be automatically invoked if needed, so forced calls should be avoided.
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
        /// <param name="caption">The question caption to display.</param>
        /// <param name="score">The current question score, no errors must be found when evaluating in order to compute.</param>   
        protected void OpenQuestion(string caption, float score=0){
           OpenQuestion(caption, string.Empty, score);
        }
        /// <summary>
        /// Opens a new question, so all the computed score within "EvalQuestion" method will belong to this one.
        /// Warning: It will cancell any previous question if it's open, so its computed score will be lost.
        /// </summary>
        /// <param name="caption">The question caption to display.</param>
        /// <param name="description">The question description to display.</param>
        /// <param name="score">The current question score, no errors must be found when evaluating in order to compute.</param>   
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
        /// <summary>
        /// Cancels the current question, so no score will be computed.
        /// </summary>
        protected void CancelQuestion(){
            this.Errors = null;
        }
        /// <summary>
        /// Closes the currently open question and computes the score, which has been setup when opening the question.
        /// </summary>
        /// <param name="caption">The closing caption to display.</param>
        protected void CloseQuestion(string caption = null){       
            if(!string.IsNullOrEmpty(caption)) Output.Instance.WriteLine(caption);                             
            Output.Instance.UnIndent();            
            
            if(IsQuestionOpen){
                Output.Instance.BreakLine();
                                
                if(this.Errors.Count == 0) this.Success += this.Points;
                else this.Fails += this.Points;
                
                this.Errors = null;
                
                float total = Success + Fails;
                this.Score = (total > 0 ? (Success / total)*this.MaxScore : 0);      
            }            
        }
        /// <summary>
        /// Adds a correct execution result (usually a checker's method one) for the current opened question, so its value will be computed once the question is closed.
        /// </summary>
        protected void EvalQuestion(){
            EvalQuestion(new List<string>());
        }
        /// <summary>
        /// Adds an execution result (usually a checker's method one) for the current opened question, so its value will be computed once the question is closed.
        /// </summary>
        /// <param name="errors">A list of errors, an empty one will be considered as correct, otherwise it will be considered as a incorrect.</param>
        protected void EvalQuestion(List<string> errors){
            if(IsQuestionOpen){
                this.Errors.AddRange(errors);
                Output.Instance.WriteResponse(errors);
            }
        }   
        /// <summary>
        /// Prints the score to the output.
        /// </summary>     
        protected void PrintScore(){
            Output.Instance.Write("TOTAL SCORE: ", ConsoleColor.Cyan);
            Output.Instance.Write(Math.Round(Score, 2).ToString(), (Score < MaxScore/2 ? ConsoleColor.Red : ConsoleColor.Green));
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

                    string[] files = Directory.GetFiles(f, "*.zip", SearchOption.AllDirectories);                    
                    if(files.Length == 0) Output.Instance.WriteLine("Done!");                    
                    else{
                        foreach(string zip in files){
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