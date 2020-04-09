/*
    Copyright © 2020 Fernando Porrino Serrano
    Third party software licenses can be found at /docs/credits/thirdparties.md

    This file is part of AutoCheck.

    AutoCheck is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    AutoCheck is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with AutoCheck.  If not, see <https://www.gnu.org/licenses/>.
*/

using AutoCheck.Core;

namespace AutoCheck.Scripts{
    public class DAM_M04UF1_Html5Assignment: Core.ScriptFiles<CopyDetectors.Html>{                       
        public DAM_M04UF1_Html5Assignment(string[] args): base(args){        
        }                

        public override void Run(){
            base.Run();            
            
            Output.Instance.Indent();                  

            OpenQuestion("Question 1", "Index");
                var index = new Checkers.Html(this.Path, "index.html");
                index.Connector.ValidateHTML5AgainstW3C();    //exception if fails, so no score will be computed

                OpenQuestion("Question 1.1", "Validating headers", 1);
                    EvalQuestion(index.CheckIfNodesMatchesAmount("//h1", 1, Connector.Operator.LOWER));
                    EvalQuestion(index.CheckIfNodesMatchesAmount("//h2", 1, Connector.Operator.LOWER));
                CloseQuestion();

                OpenQuestion("Question 1.2", "Validating paragraphs", 1);
                    EvalQuestion(index.CheckIfNodesMatchesAmount("//p", 2, Connector.Operator.LOWER));
                    EvalQuestion(index.CheckIfNodesContentMatchesAmount("//p", 1500, Connector.Operator.LOWER));
                CloseQuestion();

                OpenQuestion("Question 1.3", "Validating breaklines", 1);
                    EvalQuestion(index.CheckIfNodesMatchesAmount("//p/br", 1, Connector.Operator.LOWER));
                CloseQuestion();

                OpenQuestion("Question 1.4", "Validating images", 1);
                    EvalQuestion(index.CheckIfNodesMatchesAmount("//img", 1, Connector.Operator.LOWER));
                CloseQuestion();
            CloseQuestion();

            OpenQuestion("Question 2", "Contacte");
                var contacte = new Checkers.Html(this.Path, "contacte.html");
                contacte.Connector.ValidateHTML5AgainstW3C();    //exception if fails, so no score will be computed

                OpenQuestion("Question 2.1", "Validating text fields", 1);
                    EvalQuestion(contacte.CheckIfNodesMatchesAmount("//input[@type='text']", 2, Connector.Operator.LOWER));
                CloseQuestion();

                OpenQuestion("Question 2.2", "Validating number fields", 1);
                    EvalQuestion(contacte.CheckIfNodesMatchesAmount("//input[@type='number']", 1, Connector.Operator.LOWER));
                CloseQuestion();

                OpenQuestion("Question 2.3", "Validating email fields", 1);
                    EvalQuestion(contacte.CheckIfNodesMatchesAmount("//input[@type='email']", 1, Connector.Operator.LOWER));
                CloseQuestion();

                OpenQuestion("Question 2.4", "Validating radio buttons", 1);
                    EvalQuestion(contacte.CheckIfNodesMatchesAmount("//input[@type='radio']", 3, Connector.Operator.LOWER));
                    EvalQuestion(contacte.CheckIfNodesMatchesAmount("//input[@type='radio'][@name=(//input[@type='radio']/@name)]", 3));
                    EvalQuestion(contacte.CheckIfNodesMatchesAmount("//input[@type='radio'][@checked]", 1, Connector.Operator.EQUALS, true));
                CloseQuestion();

                OpenQuestion("Question 2.5", "Validating select lists", 1);
                    EvalQuestion(contacte.CheckIfNodesMatchesAmount("//select", 1, Connector.Operator.LOWER));
                    EvalQuestion(contacte.CheckIfNodesMatchesAmount("//select/option", 3, Connector.Operator.LOWER, true));
                    EvalQuestion(contacte.CheckIfNodesMatchesAmount("//select/option[@selected]", 1, Connector.Operator.EQUALS, true));
                CloseQuestion();

                OpenQuestion("Question 2.6", "Validating checkboxes", 1);
                    EvalQuestion(contacte.CheckIfNodesMatchesAmount("//input[@type='checkbox']", 3, Connector.Operator.LOWER, false));
                    EvalQuestion(contacte.CheckIfNodesMatchesAmount("//input[@type='checkbox'][@name=(//input[@type='checkbox']/@name)]", 3));
                    EvalQuestion(contacte.CheckIfNodesMatchesAmount("//input[@type='checkbox'][@checked]", 1, Connector.Operator.EQUALS, true));
                CloseQuestion();

                OpenQuestion("Question 2.7", "Validating textarea fields", 1);
                    EvalQuestion(contacte.CheckIfNodesMatchesAmount("//textarea", 1, Connector.Operator.LOWER));
                CloseQuestion();

                OpenQuestion("Question 2.8", "Validating placeholders", 1);
                    EvalQuestion(contacte.CheckIfNodesMatchesAmount("//input[@type='text'][@placeholder]", 2));
                    EvalQuestion(contacte.CheckIfNodesMatchesAmount("//input[@type='email'][@placeholder]", 1));
                    EvalQuestion(contacte.CheckIfNodesMatchesAmount("//input[@type='number'][@placeholder]", 1));
                    EvalQuestion(contacte.CheckIfNodesMatchesAmount("//textarea[@placeholder]", 1));
                CloseQuestion();

                OpenQuestion("Question 2.9", "Validating labels", 1);
                    EvalQuestion(contacte.CheckIfNodesRelatedLabelsMatchesAmount("//input[@type='text']", 1));
                    EvalQuestion(contacte.CheckIfNodesRelatedLabelsMatchesAmount("//input[@type='number']", 1));     
                    EvalQuestion(contacte.CheckIfNodesRelatedLabelsMatchesAmount("//input[@type='email']", 1));    
                    EvalQuestion(contacte.CheckIfNodesRelatedLabelsMatchesAmount("//input[@type='radio']", 1));   
                    EvalQuestion(contacte.CheckIfNodesRelatedLabelsMatchesAmount("//select", 1)); 
                    EvalQuestion(contacte.CheckIfNodesRelatedLabelsMatchesAmount("//input[@type='checkbox']", 1)); 
                    EvalQuestion(contacte.CheckIfNodesRelatedLabelsMatchesAmount("//textarea", 1));                   
                CloseQuestion();

                OpenQuestion("Question 2.10", "Validating table", 1);
                    EvalQuestion(contacte.CheckIfNodesMatchesAmount("//tr[1]/td", 4, Connector.Operator.LOWER));
                    EvalQuestion(contacte.CheckIfNodesMatchesAmount("//td[@colspan=3]", 1, Connector.Operator.LOWER));
                    EvalQuestion(contacte.CheckIfNodesMatchesAmount("//tr/td[1]/label", 8, Connector.Operator.LOWER));
                    EvalQuestion(contacte.CheckIfNodesMatchesAmount("//tr/td[2]/label", 1, Connector.Operator.LOWER));
                    EvalQuestion(contacte.CheckIfNodesMatchesAmount("//tr/td[3]/label", 5, Connector.Operator.LOWER));
                    EvalQuestion(contacte.CheckIfTablesIsConsistent("//table"));                                              
                CloseQuestion();

                OpenQuestion("Question 2.11", "Validating form reset", 1);
                    EvalQuestion(contacte.CheckIfNodesMatchesAmount("//input[@type='reset'] | //button[@type='reset']", 1));
                CloseQuestion();   

                OpenQuestion("Question 2.12", "Validating form submit", 1);                    
                    EvalQuestion(contacte.CheckIfNodesMatchesAmount("(//input[@type !='submit' and @type !='reset'] | //select | //textarea)[not(@name)]", 0));
                    EvalQuestion(contacte.CheckIfNodesMatchesAmount("//input[@type='submit'] | //button[@type='submit']", 1));
                    EvalQuestion(contacte.CheckIfNodesMatchesAmount("//form[@action='formResult.html'] | //button[@formaction='formResult.html']", 1));
                CloseQuestion();
            CloseQuestion();

            OpenQuestion("Question 3", "Index (menu)");                                
                OpenQuestion("Question 3.1", "Validating lists", 1);
                    EvalQuestion(index.CheckIfNodesMatchesAmount("//ul", 1));
                    EvalQuestion(index.CheckIfNodesMatchesAmount("//ul/li", 2, Connector.Operator.EQUALS, true));
                CloseQuestion();

                OpenQuestion("Question 3.2", "Validating links", 1);
                    EvalQuestion(index.CheckIfNodesMatchesAmount("//ul/li/a", 2));
                    EvalQuestion(index.CheckIfNodesMatchesAmount("//ul/li/a[@href='index.html' or @href='contacte.html']", 2));
                CloseQuestion();
            CloseQuestion();

            PrintScore();
            Output.Instance.UnIndent();
        }
    }
}