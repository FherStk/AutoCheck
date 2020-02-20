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
                        {"active", true}, 
                        {"customer", false}, 
                        {"supplier", true}, 
                        {"employee", false}
                    }));
                CloseQuestion();  
            CloseQuestion();              
            
            PrintScore();
            Output.Instance.UnIndent();
        }
    }
}