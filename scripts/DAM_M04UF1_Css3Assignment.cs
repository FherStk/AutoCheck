using AutomatedAssignmentValidator.Core;

namespace AutomatedAssignmentValidator.Scripts{
    public class DAM_M04UF1_Css3Assignment: Core.ScriptFiles<CopyDetectors.Css>{                       
        public DAM_M04UF1_Css3Assignment(string[] args): base(args){        
        }                

        public override void Run(){
            base.Run();            
            
            Output.Instance.Indent();                  

            OpenQuestion("Question 1", "Index");
                Checkers.Web index = new Checkers.Web(this.Path, "index.html", "index.css");
                index.Connector.ValidateHTML5AgainstW3C();    //exception if fails, so no score will be computed

                //TODO: compute question numbers (1, 2, 2.1, 2.2...)
                OpenQuestion("Question 1.1", "Validating inline CSS", 1);
                    EvalQuestion(index.CheckIfNodesMatchesAmount("//style | //*[@style]", 0));
                CloseQuestion();

                OpenQuestion("Question 1.2", "Validating divs", 1);
                    EvalQuestion(index.CheckIfNodesMatchesAmount("//div", 1, Checkers.Web.Operator.MIN));
                CloseQuestion();

                OpenQuestion("Question 1.2", "Validating video", 1);
                    EvalQuestion(index.CheckIfNodesMatchesAmount("//video | //iframe[@src] | //object[@data]", 1, Checkers.Web.Operator.MIN));
                CloseQuestion();
            CloseQuestion();

            OpenQuestion("Question 2", "CSS");
                index.Connector.ValidateCSS3AgainstW3C();    //exception if fails, so no score will be computed

                OpenQuestion("Question 2.1", 1);
                    EvalQuestion(index.CheckIfCssPropertyHasBeenApplied("font"));
                CloseQuestion();

                OpenQuestion("Question 2.2", 1);
                    EvalQuestion(index.CheckIfCssPropertyHasBeenApplied("border"));
                CloseQuestion();

                OpenQuestion("Question 2.3", 1);
                    EvalQuestion(index.CheckIfCssPropertyHasBeenApplied("text"));
                CloseQuestion();

                OpenQuestion("Question 2.4", 1);
                    EvalQuestion(index.CheckIfCssPropertyHasBeenApplied("color"));
                CloseQuestion();

                OpenQuestion("Question 2.5", 1);
                    EvalQuestion(index.CheckIfCssPropertyHasBeenApplied("background"));
                CloseQuestion();

                OpenQuestion("Question 2.6", 1);
                    EvalQuestion(index.CheckIfCssPropertyHasBeenApplied("float", "left"));
                CloseQuestion();

                OpenQuestion("Question 2.7", 1);
                    EvalQuestion(index.CheckIfCssPropertyHasBeenApplied("float", "right"));
                CloseQuestion();

                OpenQuestion("Question 2.8", 1);
                    EvalQuestion(index.CheckIfCssPropertyHasBeenApplied("position", "absolute"));
                CloseQuestion();

                OpenQuestion("Question 2.9", 1);
                    EvalQuestion(index.CheckIfCssPropertyHasBeenApplied("position", "relative"));
                CloseQuestion();

                OpenQuestion("Question 2.10", 1);
                    EvalQuestion(index.CheckIfCssPropertyHasBeenApplied("clear"));
                CloseQuestion();

                OpenQuestion("Question 2.11", 1);
                    EvalQuestion(index.CheckIfCssPropertyHasBeenApplied("width"));
                CloseQuestion();

                OpenQuestion("Question 2.12", 1);
                    EvalQuestion(index.CheckIfCssPropertyHasBeenApplied("height"));
                CloseQuestion();

                OpenQuestion("Question 2.13", 1);
                    EvalQuestion(index.CheckIfCssPropertyHasBeenApplied("margin"));
                CloseQuestion();

                OpenQuestion("Question 2.14", 1);
                    EvalQuestion(index.CheckIfCssPropertyHasBeenApplied("padding"));
                CloseQuestion();

                OpenQuestion("Question 2.15", 1);
                    EvalQuestion(index.CheckIfCssPropertyHasBeenApplied("list"));
                CloseQuestion();

                OpenQuestion("Question 2.16", 1);
                    EvalQuestion(index.CheckIfCssPropertiesApplied(new string[]{
                        "top",
                        "right",
                        "bottom",
                        "left"
                    }, 1, Checkers.Web.Operator.MIN));
                CloseQuestion();
            CloseQuestion();

            PrintScore();
            Output.Instance.UnIndent();
        }
    }
}