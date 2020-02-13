using System.Collections.Generic;
using AutomatedAssignmentValidator.Core;

namespace AutomatedAssignmentValidator.Scripts{
    public class DAM_M04UF1_Html5Assignment: Core.ScriptFiles<CopyDetectors.Html>{                       
        public DAM_M04UF1_Html5Assignment(string[] args): base(args){        
        }                

        public override void Run(){
            base.Run();            
            
            Output.Instance.Indent();                  

            OpenQuestion("Question 1", "Index");
                Checkers.Web web = new Checkers.Web(this.Path, "index.html");
                //web.Connector.ValidateHTML5AgainstW3C();    //exception if fails, so no score will be computed

                OpenQuestion("Question 1.1", "Validating headers", 1);
                    EvalQuestion(web.CheckIfNodeAmountMatchesMinimum("//h1", 1));
                    EvalQuestion(web.CheckIfNodeAmountMatchesMinimum("//h2", 1));
                CloseQuestion();

                OpenQuestion("Question 1.2", "Validating paragraphs", 1);
                    EvalQuestion(web.CheckIfNodeAmountMatchesMinimum("//p", 2));
                    EvalQuestion(web.CheckIfNodeContentMatchesMinimum("//p", 1500));
                CloseQuestion();

                OpenQuestion("Question 1.3", "Validating breaklines", 1);
                    EvalQuestion(web.CheckIfNodeAmountMatchesMinimum("//p/br", 1));
                CloseQuestion();

                OpenQuestion("Question 1.4", "Validating images", 1);
                    EvalQuestion(web.CheckIfNodeAmountMatchesMinimum("//img", 1));
                CloseQuestion();

                OpenQuestion("Question 1.5", "Validating lists", 1);
                    EvalQuestion(web.CheckIfNodeAmountMatchesMinimum("//ul", 1));
                    EvalQuestion(web.CheckIfNodeAmountMatchesMinimum("//ul/li", 2, true));
                CloseQuestion();

                OpenQuestion("Question 1.6", "Validating links", 1);
                    EvalQuestion(web.CheckIfNodeAmountMatchesMinimum("//ul/li/a", 2));
                    EvalQuestion(web.CheckIfNodeAttributeMatchesData("//ul/li/a", "href", new string[]{
                        "index.html", 
                        "contacte.html"
                    }));
                CloseQuestion();
            CloseQuestion();

            PrintScore();
            Output.Instance.UnIndent();
        }
    }
}