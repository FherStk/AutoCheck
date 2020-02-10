
namespace AutomatedAssignmentValidator.Core{       
    public partial class Checker {  
        protected Output Output {get; private set;}

        public Checker(Output output){
            this.Output = output;
        }
    }
}
