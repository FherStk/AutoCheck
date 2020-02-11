using System;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator.Scripts{
    public class ASIX_M02UF3_OdooUsageAssignment: Core.ScriptDB<CopyDetectors.None>{                       
        public ASIX_M02UF3_OdooUsageAssignment(string[] args): base(args){        
        }                

        public override void Run(){
            base.Run();            
            
            Output.Indent();             
            Checkers.Odoo odoo = new Checkers.Odoo(1, this.Host, this.DataBase, "postgres", "postgres", this.Output);            
            
            string companyName = string.Format("Samarretes Frikis {0}", this.Student); 
            OpenQuestion("Question 1: ");                                     
                EvalQuestion(odoo.CheckIfCompanyMatchesData(new Dictionary<string, object>(){{"id", 1}, {"name", companyName}, {"logo", true}}));
            CloseQuestion();   

            string providerName = string.Format("Samarretes Frikis {0}", this.Student); 
            OpenQuestion("Question 2: ");                                                 
                EvalQuestion(odoo.CheckIfProviderMatchesData(new Dictionary<string, object>(){{"name", providerName}, {"is_company", true}, {"logo", true}}));
            CloseQuestion();  

            string productName = string.Format("Samarretes Frikis {0}", this.Student); 
            int providerID = odoo.Connector.GetProviderID(providerName);
            OpenQuestion("Question 2: ");                                                 
                EvalQuestion(odoo.CheckIfProductMatchesData(new Dictionary<string, object>(){{"name", providerName}, {"type", "product"}, {"attribute", "Talla"}, {"supplier_id", providerID}, {"purchase_price", 9.99m}, {"sell_price", 19.99m}}, new string[]{"S", "M", "L", "XL"}));
            CloseQuestion();      
              

            PrintScore();
            Output.UnIndent();
        }
    }
}