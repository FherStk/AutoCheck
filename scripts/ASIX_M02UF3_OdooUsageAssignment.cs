using System;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator.Scripts{
    public class ASIX_M02UF3_OdooUsageAssignment: Core.ScriptDB<CopyDetectors.None>{                       
        public ASIX_M02UF3_OdooUsageAssignment(string[] args): base(args){        
        }                

        public override void Run(){
            base.Run();            
            
            Output.Indent();
            string companyName = string.Format("Samarretes Frikis {0}", this.Student);  
            Checkers.Odoo odoo = new Checkers.Odoo(companyName, this.Host, this.DataBase, "postgres", "postgres", this.Output);            
            
            OpenQuestion("Question 1: ");                                     
                EvalQuestion(odoo.CheckIfCompanyMatchesData(new Dictionary<string, object>(){{"id", 1}, {"name", companyName}, {"logo", true}}));
                CloseQuestion();   
            CloseQuestion();

            string providerName = string.Format("Samarretes Frikis {0}", this.Student); 
            OpenQuestion("Question 2: ");                                                 
                EvalQuestion(odoo.CheckIfProviderMatchesData(new Dictionary<string, object>(){{"name", providerName}, {"is_company", true}, {"logo", true}}));
                CloseQuestion();   
            CloseQuestion();      
              

            PrintScore();
            Output.UnIndent();
        }
    }
}