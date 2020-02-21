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

using System.Collections.Generic;
using AutoCheck.Core;

namespace AutoCheck.Scripts{
    public class DAM_M10UF2_OdooCsvAssignment: Core.ScriptDB<CopyDetectors.None>{                       
        public DAM_M10UF2_OdooCsvAssignment(string[] args): base(args){        
        }

        protected override void DefaultArguments(){
            base.DefaultArguments();            
            this.MaxScore = 5f;
        }                

        public override void Run(){
            base.Run();            
            
            Output.Instance.Indent();      
                        
            OpenQuestion("Question 1", "CSV data");                                                     
                OpenQuestion("Question 1.1", "The file has been created", 0.5f);             
                    Output.Instance.Write("Checking the CSV file... ");
                    var csv = new Checkers.Csv(this.Path, "*.csv"); //Exception if wont parse                    
                    EvalQuestion();
                CloseQuestion();   

                OpenQuestion("Question 1.2", "The file has been modified", 1);             
                    EvalQuestion(csv.CheckIfRegistriesMatchesAmount(1));
                CloseQuestion();  

                OpenQuestion("Question 1.3", "The file has the correct data", 1);             
                    EvalQuestion(csv.CheckIfRegistriesMatchesData(1, new Dictionary<string, object>(){
                        {"name", this.Student}, 
                        {"email", "@elpuig.xeill.net"}, 
                        {"active", true}, 
                        {"customer", false}, 
                        {"supplier", true}, 
                        {"employee", false}
                    }));
                CloseQuestion();   
            CloseQuestion();   

            OpenQuestion("Question 2", "Odoo's database data");
                Checkers.Odoo odoo = new Checkers.Odoo(1, this.Host, this.DataBase, this.Username, this.Password);
                OpenQuestion("Question 2.1", "Some data loaded", 1.5f);
                    EvalQuestion(odoo.CheckIfTableMatchesData("public", "res_partner", new Dictionary<string, object>(){
                        {"name", csv.Connector.CsvDoc.GetLine(1)["name"]}
                    }));
                CloseQuestion();   

                OpenQuestion("Question 2.2", "All data loaded correctly", 1); 
                    int providerID = odoo.Connector.GetProviderID(csv.Connector.CsvDoc.GetLine(1)["name"]);            
                    EvalQuestion(odoo.CheckIfProviderMatchesData(providerID, new Dictionary<string, object>(){
                        {"name", csv.Connector.CsvDoc.GetLine(1)["name"]}, 
                        {"email", csv.Connector.CsvDoc.GetLine(1)["email"]}, 
                        {"active", csv.Connector.CsvDoc.GetLine(1)["active"]}, 
                        {"customer", csv.Connector.CsvDoc.GetLine(1)["customer"]}, 
                        {"supplier", csv.Connector.CsvDoc.GetLine(1)["supplier"]}, 
                        {"employee", csv.Connector.CsvDoc.GetLine(1)["employee"]}
                    }));
                CloseQuestion();  
            CloseQuestion();              
            
            PrintScore();
            Output.Instance.UnIndent();
        }
    }
}