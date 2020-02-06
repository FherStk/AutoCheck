using System;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator.Scripts{
    public class ASIX_M02UF3_ViewsAssignment: Core.ScriptBaseForDataBase<CopyDetectors.SqlLog>{                       
        public ASIX_M02UF3_ViewsAssignment(string[] args): base(args){        
        }                

        public override void Script(){
            base.Script();            
            
            Output.Indent();
            Utils.DataBase db = new Utils.DataBase(this.Host, this.DataBase, "postgres", "postgres", this.Output);            
            
            OpenQuestion("Question 1: ", 1);
            EvalQuestion(db.CheckIfTableExists("gerencia", "responsables"));
            EvalQuestion(db.CheckViewDefinition("gerencia", "responsables", @"
                SELECT  e.id AS id_responsable,
                        e.nom AS nom_responsable,
                        e.cognoms AS cognoms_responsable,
                        f.id AS id_fabrica,
                        f.nom AS nom_fabrica
                FROM rrhh.empleats e
                LEFT JOIN produccio.fabriques f ON e.id = f.id_responsable;
            ")); 
            
            CloseQuestion();   

            OpenQuestion("Question 2: ", 1);
            OpenQuestion("Question 2.1: ", 1);
            CloseQuestion();   
            CloseQuestion();   
            
            
            /*             
            //question 2         
            //Must do: 
                INSERT into view [db.DoExecuteNonQuery for INSERT + db.DoExecuteQuery for getting the IDs]

                Check view data for employee [new method for comparing rows: schema, table, pkfield, pkvalue, expected_values<key, value>]
                Check table data (with view data + default one) for employee [as previous]
                Score +1

                Check view data for factory
                Check table data (with view data + default one) for factory
                Score +1
                

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