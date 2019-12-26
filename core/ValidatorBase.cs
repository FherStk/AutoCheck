
using System;
using System.Linq;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator{
    public abstract class ValidatorBase{
        public int Success {get; private set;}
        public int Errors {get; private set;}        
        public List<TestResult> GlobalResults {get; private set;}
        public TestResult CurrentResult {get; private set;}        
        private List<string> History  {get; set;}  
        public abstract void Validate();

        protected ValidatorBase(){
           ClearResults();
        } 
        protected void OpenTest(string caption, ConsoleColor color = ConsoleColor.Gray, bool print = true){
            if(this.CurrentResult != null) throw new Exception("Close the current test before opening a new one.");
            else{
                CurrentResult = new TestResult(caption);            
                if(print) PrintTestCaption(caption, color);
            }             
        }                     
        protected void AppendTest(List<string> errors, bool print = true){
            if(this.CurrentResult == null) throw new Exception("Open a new test before appending a new one.");
            else{                
                //Closing the current test
                CurrentResult.Errors.AddRange(errors);
                if(print) PrintTestResults();

                //Storing the historical data for compute errors on closing
                GlobalResults.Add(CurrentResult);                
                History.AddRange(errors);                                
                CurrentResult = null;
            } 
        } 
        protected void CloseTest(List<string> errors, int score = 1, bool print = true){
            AppendTest(errors, print);            
            errors.AddRange(History);

            if(errors.Count == 0) Success += score;
            else this.Errors += score;
            
            History.Clear();                      
        }                   
        protected void ClearResults(){
            this.Success = 0;
            this.Errors = 0;            
            this.CurrentResult = null;
            this.GlobalResults = new List<TestResult>();                        
            this.History = new List<string>();
        }
        protected void PrintScore(){
            float div = (float)(Success + Errors);
            float score = (div > 0 ? ((float)Success / div)*10 : 0);
            
            Utils.BreakLine(); 
            Utils.Write("   TOTAL SCORE: ", ConsoleColor.Cyan);
            Utils.Write(Math.Round(score, 2).ToString(), (score < 5 ? ConsoleColor.Red : ConsoleColor.Green));
            Utils.BreakLine();
        } 
        /// <summary>
        /// The caption will be printed in gray, and everything after the '~' symbol will be printed using a secondary color till the last ':' symbol.
        /// </summary>
        /// <param name="caption">The text to display, use ~TEXT: to print this "text" with a secondary color.</param>
        /// <param name="color">The secondary color to use.</param>
        private void PrintTestCaption(string caption, ConsoleColor color = ConsoleColor.Gray){                
            if(caption.Contains("~")){
                int i = caption.IndexOf("~");
                Utils.Write(caption.Substring(0, i));

                caption = caption.Substring(i+1);
                i = caption.IndexOf(":");
                Utils.Write(caption.Substring(0, i), color);

                caption = caption.Substring(i+1);                    
            }
            
            Utils.Write(caption);                
        }
        private void PrintTestResults(){
            string prefix = "\n\t-";
            if(CurrentResult.Errors == null || CurrentResult.Errors.Count == 0) Utils.WriteLine("OK", ConsoleColor.DarkGreen);
            else{
                if(CurrentResult.Errors.Where(x => x.Length > 0).Count() == 0) Utils.WriteLine("ERROR", ConsoleColor.Red);
                else Utils.WriteLine(string.Format("ERROR: {0}{1}", prefix, string.Join(prefix, CurrentResult.Errors)), ConsoleColor.Red);
            }
        }              
    }
}