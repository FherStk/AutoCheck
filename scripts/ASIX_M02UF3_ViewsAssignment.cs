using System;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator.Scripts{
    public class ASIX_M02UF3_ViewsAssignment: Core.ScriptBaseForDataBase<CopyDetectors.SqlLog>{                       
        public ASIX_M02UF3_ViewsAssignment(string[] args): base(args){        
        }                

        public override void Single(){
            base.Single();            
            Output.Indent();

            Utils.DataBase db = new Utils.DataBase(this.Host, this.DataBase, "postgres", "postgres", this.Output);
            
            
            OpenQuestion("Question 1: ", 1);
            EvalQuestion(db.CheckIfTableExists("gerencia", "responsables"));
            //TODO: Continue
            //EvalQuestion(CheckViewIsCorrect()); 
            CloseQuestion("This questions does not score.");            
            
            /*             
            //question 2         
            CloseTest(CheckInsertRuleForEmployee()); 
            CloseTest(CheckInsertRuleForFactory()); 
            
            //question 3                     
            CloseTest(CheckUpdateRuleForEmployee());  
            CloseTest(CheckUpdateRuleForFactory());  

            //question 4                  
            CloseTest(CheckDeleteRuleForFabrica());    
            CloseTest(CheckDeleteRuleForResponsable());    
            CloseTest(CheckDeleteRuleWithNoFilter());    

            //question 5      
            OpenTest("Checking the permissions for the user ~it... ", ConsoleColor.Yellow);
            AppendTest(CheckTableMatchPrivileges("it", "gerencia", "responsables", "r", false), false);
            CloseTest(CheckSchemaMatchPrivileges("it", "gerencia", "U", false), 1);
            */

            PrintScore();   
            Output.UnIndent();        
        }
    }
}