
using System;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator{
    public abstract class ValidatorBase: IDisposable{
        private int Success {get; set;}
        private int Errors {get; set;}        
        protected List<TestResult> GlobalResults {get; private set;}
        private TestResult CurrentResult {get; set;}        
        private List<string> History  {get; set;}  
        public abstract List<TestResult> Validate();
        protected ValidatorBase(){
           ClearResults();
        } 
        public void Dispose()
        {            
            //Just for IDisposable elements (see ValidatorBaseDataBase.cs for an example.)
            //The GC will do the rest :)
        }
        protected void OpenTest(string caption, ConsoleColor color = ConsoleColor.Gray, bool print = true){
            if(this.CurrentResult != null) throw new Exception("Close the current test before opening a new one.");
            else{
                CurrentResult = new TestResult(caption);            
                if(print) Terminal.WriteCaption(caption, color);
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
            
            Terminal.BreakLine(); 
            Terminal.Write("   TOTAL SCORE: ", ConsoleColor.Cyan);
            Terminal.Write(Math.Round(score, 2).ToString(), (score < 5 ? ConsoleColor.Red : ConsoleColor.Green));
            Terminal.BreakLine();
        }                 
        private void PrintTestResults(){
            if(CurrentResult.Errors == null || CurrentResult.Errors.Count == 0) Terminal.WriteOK();
            else Terminal.WriteError(CurrentResult.Errors);
        }              
    }
}