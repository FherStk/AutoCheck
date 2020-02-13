using System.Collections.Generic;
using AutomatedAssignmentValidator.Core;

namespace AutomatedAssignmentValidator.Scripts{
    public class DAM_M04UF1_Html5Assignment: Core.ScriptFiles<CopyDetectors.Html>{                       
        public DAM_M04UF1_Html5Assignment(string[] args): base(args){        
        }                

        public override void Run(){
            base.Run();            
            
            Output.Instance.Indent();      
            Checkers.Web web = new Checkers.Web(this.Path, "*.html");
            web.Connector.ValidateHTML5AgainstW3C();    //exception if fails, so no score will be computed

            OpenQuestion("Question 1", "Index");
                OpenQuestion("Question 1.1", "Validating the headers", 1);
                    EvalQuestion(web.CheckIfNodeMatchesMinimum("h1", 1));
                    EvalQuestion(web.CheckIfNodeMatchesMinimum("h2", 1));
                CloseQuestion();
            CloseQuestion();

            PrintScore();
            Output.Instance.UnIndent();
        }
    }
}