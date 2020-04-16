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
    public class DAM_M04UF1_Css3Assignment: Core.ScriptFiles<CopyDetectors.Css>{                       
        public DAM_M04UF1_Css3Assignment(string[] args): base(args){        
        }                

        public override void Run(){
            base.Run();            
            
            Output.Instance.Indent();                  

            OpenQuestion("Question 1", "Index");
                var html = new Checkers.Html(this.Path, "index.html");                
                html.Connector.ValidateHTML5AgainstW3C();    //exception if fails, so no score will be computed

                //TODO: compute question numbers (1, 2, 2.1, 2.2...)
                OpenQuestion("Question 1.1", "Validating inline CSS", 1);
                    EvalQuestion(html.CheckIfNodesMatchesAmount("//style | //*[@style]", 0));
                CloseQuestion();

                OpenQuestion("Question 1.2", "Validating divs", 1);
                    EvalQuestion(html.CheckIfNodesMatchesAmount("//div", 1, Connector.Operator.LOWER));
                CloseQuestion();

                OpenQuestion("Question 1.2", "Validating video", 1);
                    EvalQuestion(html.CheckIfNodesMatchesAmount("//video | //iframe[@src] | //object[@data]", 1, Connector.Operator.LOWER));
                CloseQuestion();
            CloseQuestion();

            OpenQuestion("Question 2", "CSS");
                var css = new Checkers.Css(this.Path, "index.css");
                css.Connector.ValidateCSS3AgainstW3C();    //exception if fails, so no score will be computed

                OpenQuestion("Question 2.1", 1);
                    EvalQuestion(css.CheckIfPropertyApplied(html.Connector.HtmlDoc, "font"));
                CloseQuestion();

                OpenQuestion("Question 2.2", 1);
                    EvalQuestion(css.CheckIfPropertyApplied(html.Connector.HtmlDoc, "border"));
                CloseQuestion();

                OpenQuestion("Question 2.3", 1);
                    EvalQuestion(css.CheckIfPropertyApplied(html.Connector.HtmlDoc, "text"));
                CloseQuestion();

                OpenQuestion("Question 2.4", 1);
                    EvalQuestion(css.CheckIfPropertyApplied(html.Connector.HtmlDoc, "color"));
                CloseQuestion();

                OpenQuestion("Question 2.5", 1);
                    EvalQuestion(css.CheckIfPropertyApplied(html.Connector.HtmlDoc, "background"));
                CloseQuestion();

                OpenQuestion("Question 2.6", 1);
                    EvalQuestion(css.CheckIfPropertyApplied(html.Connector.HtmlDoc, "float", "left"));
                CloseQuestion();

                OpenQuestion("Question 2.7", 1);
                    EvalQuestion(css.CheckIfPropertyApplied(html.Connector.HtmlDoc, "float", "right"));
                CloseQuestion();

                OpenQuestion("Question 2.8", 1);
                    EvalQuestion(css.CheckIfPropertyApplied(html.Connector.HtmlDoc, "position", "absolute"));
                CloseQuestion();

                OpenQuestion("Question 2.9", 1);
                    EvalQuestion(css.CheckIfPropertyApplied(html.Connector.HtmlDoc, "position", "relative"));
                CloseQuestion();

                OpenQuestion("Question 2.10", 1);
                    EvalQuestion(css.CheckIfPropertyApplied(html.Connector.HtmlDoc, "clear"));
                CloseQuestion();

                OpenQuestion("Question 2.11", 1);
                    EvalQuestion(css.CheckIfPropertyApplied(html.Connector.HtmlDoc, "width"));
                CloseQuestion();

                OpenQuestion("Question 2.12", 1);
                    EvalQuestion(css.CheckIfPropertyApplied(html.Connector.HtmlDoc, "height"));
                CloseQuestion();

                OpenQuestion("Question 2.13", 1);
                    EvalQuestion(css.CheckIfPropertyApplied(html.Connector.HtmlDoc, "margin"));
                CloseQuestion();

                OpenQuestion("Question 2.14", 1);
                    EvalQuestion(css.CheckIfPropertyApplied(html.Connector.HtmlDoc, "padding"));
                CloseQuestion();

                OpenQuestion("Question 2.15", 1);
                    EvalQuestion(css.CheckIfPropertyApplied(html.Connector.HtmlDoc, "list"));
                CloseQuestion();

                OpenQuestion("Question 2.16", 1);
                    EvalQuestion(css.CheckIfPropertyApplied(html.Connector.HtmlDoc, new string[]{
                        "top",
                        "right",
                        "bottom",
                        "left"
                    }, 1));
                CloseQuestion();
            CloseQuestion();

            PrintScore();
            Output.Instance.UnIndent();
        }
    }
}