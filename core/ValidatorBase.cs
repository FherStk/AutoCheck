
using System;
using System.Linq;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator{
    public abstract class ValidatorBase{
        public int success {get; private set;}
        public int errors {get; private set;}
        public List<TestResult> testResults {get; private set;}
        public TestResult currentResult {get; private set;}

        //TODO: two base validators: for files and for BBDD
        //protected abstract void ValidateAssignment();

        protected ValidatorBase(){
           ClearResults();
        } 
        protected void OpenTest(string caption, ConsoleColor color = ConsoleColor.Gray, bool print = true){
            if(this.currentResult != null) throw new Exception("Close the current test before opening a new one.");
            else{
                currentResult = new TestResult(caption);            
                if(print) PrintTestCaption(caption, color);
            }             
        } 
        protected void CloseTest(List<string> errors, int score = 1, bool print = true){
            if(this.currentResult == null) throw new Exception("Open a new test before closing one.");
            else{
                //Closing the current test
                currentResult.errors.AddRange(errors);
                testResults.Add(currentResult);
                currentResult = null;

                //Scoring the current test
                if(errors.Count == 0) success += score;
                else this.errors += score;
                
                //Printing results
                if(print) PrintTestResults(errors);
            } 
            
            
        }                
        protected void ClearResults(){
            this.success = 0;
            this.errors = 0;
            this.testResults = new List<TestResult>();
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
        private void PrintTestResults(List<string> errors = null){
            string prefix = "\n\t-";
            if(errors == null || errors.Count == 0) Utils.WriteLine("OK", ConsoleColor.DarkGreen);
            else{
                if(errors.Where(x => x.Length > 0).Count() == 0) Utils.WriteLine("ERROR", ConsoleColor.Red);
                else Utils.WriteLine(string.Format("ERROR: {0}{1}", prefix, string.Join(prefix, errors)), ConsoleColor.Red);
            }
        }       
        protected void PrintScore(){
            float div = (float)(success + errors);
            float score = (div > 0 ? ((float)success / div)*10 : 0);
            
            Utils.BreakLine(); 
            Utils.Write("   TOTAL SCORE: ", ConsoleColor.Cyan);
            Utils.Write(Math.Round(score, 2).ToString(), (score < 5 ? ConsoleColor.Red : ConsoleColor.Green));
            Utils.BreakLine();
        } 
    }
}