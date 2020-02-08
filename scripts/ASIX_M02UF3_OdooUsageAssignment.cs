using System;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator.Scripts{
    public class ASIX_M02UF3_OdooUsageAssignment: Core.ScriptDB<CopyDetectors.SqlLog>{                       
        public ASIX_M02UF3_OdooUsageAssignment(string[] args): base(args){        
        }                

        public override void Run(){
            base.Run();            
            
            Output.Indent();
            string companyName = string.Format("Samarretes Frikis {0}", this.Username);  
            Checkers.Odoo odoo = new Checkers.Odoo(companyName, this.Host, this.DataBase, "postgres", "postgres", this.Output);            
            
            OpenQuestion("Question 1: ");                                     
                EvalQuestion(odoo.CheckIfCompanyMatchesData(companyName));
                CloseQuestion();   
            CloseQuestion();   
              

            PrintScore();
            Output.UnIndent();
        }
    }
}