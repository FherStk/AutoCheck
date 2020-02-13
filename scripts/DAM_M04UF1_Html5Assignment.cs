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
                Checkers.Web index = new Checkers.Web(this.Path, "index.html");
                //web.Connector.ValidateHTML5AgainstW3C();    //exception if fails, so no score will be computed

                OpenQuestion("Question 1.1", "Validating headers", 1);
                    EvalQuestion(index.CheckIfNodeAmountMatchesMinimum("//h1", 1));
                    EvalQuestion(index.CheckIfNodeAmountMatchesMinimum("//h2", 1));
                CloseQuestion();

                OpenQuestion("Question 1.2", "Validating paragraphs", 1);
                    EvalQuestion(index.CheckIfNodeAmountMatchesMinimum("//p", 2));
                    EvalQuestion(index.CheckIfNodeContentMatchesMinimum("//p", 1500));
                CloseQuestion();

                OpenQuestion("Question 1.3", "Validating breaklines", 1);
                    EvalQuestion(index.CheckIfNodeAmountMatchesMinimum("//p/br", 1));
                CloseQuestion();

                OpenQuestion("Question 1.4", "Validating images", 1);
                    EvalQuestion(index.CheckIfNodeAmountMatchesMinimum("//img", 1));
                CloseQuestion();

                OpenQuestion("Question 1.5", "Validating lists", 1);
                    EvalQuestion(index.CheckIfNodeAmountMatchesMinimum("//ul", 1));
                    EvalQuestion(index.CheckIfNodeAmountMatchesMinimum("//ul/li", 2, true));
                CloseQuestion();

                OpenQuestion("Question 1.6", "Validating links", 1);
                    EvalQuestion(index.CheckIfNodeAmountMatchesMinimum("//ul/li/a", 2));
                    EvalQuestion(index.CheckIfNodeAttributeMatchesData("//ul/li/a", "href", new string[]{
                        "index.html", 
                        "contacte.html"
                    }));
                CloseQuestion();
            CloseQuestion();

            OpenQuestion("Question 2", "Contacte");
                Checkers.Web contacte = new Checkers.Web(this.Path, "contacte.html");
                //contacte.Connector.ValidateHTML5AgainstW3C();    //exception if fails, so no score will be computed

                OpenQuestion("Question 2.1", "Validating text fields", 1);
                    EvalQuestion(contacte.CheckIfNodeAmountMatchesMinimum("//input[@type='text']", 2));
                    //TODO: check labels related for all fields...
                CloseQuestion();

                OpenQuestion("Question 2.2", "Validating number fields", 1);
                    EvalQuestion(contacte.CheckIfNodeAmountMatchesMinimum("//input[@type='number']", 1));                    
                CloseQuestion();

                OpenQuestion("Question 2.3", "Validating email fields", 1);
                    EvalQuestion(contacte.CheckIfNodeAmountMatchesMinimum("//input[@type='email']", 1));                    
                CloseQuestion();

                OpenQuestion("Question 2.4", "Validating radio buttons", 1);
                    EvalQuestion(contacte.CheckIfNodeAmountMatchesMinimum("//input[@type='radio']", 3));
                    EvalQuestion(contacte.CheckIfNodeAttributeSharesData("//input[@type='radio']", "name"));
                    EvalQuestion(contacte.CheckIfNodeAttributeMatchesAmount("//input[@type='radio']", "checked", 1));
                CloseQuestion();

                OpenQuestion("Question 2.5", "Validating select lists", 1);
                    EvalQuestion(contacte.CheckIfNodeAmountMatchesMinimum("//select", 1));
                    EvalQuestion(contacte.CheckIfNodeAmountMatchesMinimum("//select/option", 3, true));
                    EvalQuestion(contacte.CheckIfNodeAttributeMatchesAmount("//select/option", "selected", 1));
                CloseQuestion();

                OpenQuestion("Question 2.6", "Validating checkboxes", 1);
                    EvalQuestion(contacte.CheckIfNodeAmountMatchesMinimum("//input[@type='checkbox']", 3));
                    EvalQuestion(contacte.CheckIfNodeAttributeSharesData("//input[@type='checkbox']", "name"));
                    EvalQuestion(contacte.CheckIfNodeAttributeMatchesAmount("//input[@type='checkbox']", "checked", 1));
                CloseQuestion();

                OpenQuestion("Question 2.7", "Validating textarea fields", 1);
                    EvalQuestion(contacte.CheckIfNodeAmountMatchesMinimum("//textarea", 1));                    
                CloseQuestion();

                OpenQuestion("Question 2.8", "Validating placeholders", 1);
                    EvalQuestion(contacte.CheckIfNodeHasMandatoryAttribute("//input[@type='text']", "placeholder"));
                    EvalQuestion(contacte.CheckIfNodeHasMandatoryAttribute("//input[@type='email']", "placeholder"));
                    EvalQuestion(contacte.CheckIfNodeHasMandatoryAttribute("//input[@type='number']", "placeholder"));
                    EvalQuestion(contacte.CheckIfNodeHasMandatoryAttribute("//textarea", "placeholder"));
                CloseQuestion();

                //TODO: check table (rows, cols, rowspans, colspans...)


            CloseQuestion();

            PrintScore();
            Output.Instance.UnIndent();
        }
    }
}