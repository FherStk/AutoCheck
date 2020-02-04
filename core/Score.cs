using System;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator.Core{
    public class Score{
            private float Success {get; set;}
            private float Fails {get; set;}
            private List<string> Errors {get; set;}
            private float Points {get; set;}
            public float Value {get; private set;}     

            public Score(){
            }

            public void OpenQuestion(float score){
                if(this.Errors != null) throw new Exception("Close the question before opening a new one.");
                this.Errors = new List<string>();
                this.Success = 0;
                this.Fails = 0;                      
                this.Value = 0;      
                this.Points = score;    
            }

            public void CloseQuestion(){
                if(this.Errors == null) throw new Exception("Open the question before closing the current one.");
                if(this.Errors.Count == 0) this.Success += this.Points;
                else this.Fails += this.Points;
                
                this.Errors = null;
                
                float div = Success + Fails;
                this.Value = (div > 0 ? (Success / div)*10 : 0);
            }

            public void EvalQuestion(List<string> errors){     
                if(this.Errors == null) throw new Exception("Open the question before evaluating the current one.");          
                this.Errors.AddRange(errors);
            }
        }
}