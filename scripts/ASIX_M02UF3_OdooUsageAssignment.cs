using System;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator.Scripts{
    public class ASIX_M02UF3_OdooUsageAssignment: Core.ScriptBaseForDataBase<CopyDetectors.SqlLog>{                       
        public ASIX_M02UF3_OdooUsageAssignment(string[] args): base(args){        
        }                

        public override void Script(){
            base.Script();            
            
            Output.Indent();
            string companyName = string.Format("Samarretes Frikis {0}", this.Username);  
            Utils.Odoo odoo = new Utils.Odoo(companyName, this.Host, this.DataBase, "postgres", "postgres", this.Output);            
            
            OpenQuestion("Question 1: ");                                     
                EvalQuestion(odoo.CheckCompanyData(companyName));
                CloseQuestion();   
            CloseQuestion();   
              

            PrintScore();
            Output.UnIndent();
        }
    }
}