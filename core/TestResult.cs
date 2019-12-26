using System.Collections.Generic;
namespace AutomatedAssignmentValidator{
    public class TestResult{
        public string caption {get; private set;}
        public List<string> errors {get; private set;}

        public TestResult(string caption){
            this.caption = caption;
            this.errors = new List<string>();
        }
    }
}