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
            EvalQuestion(db.CheckIfViewMatchesDefinition("gerencia", "responsables", @"
                SELECT  e.id AS id_responsable,
                        e.nom AS nom_responsable,
                        e.cognoms AS cognoms_responsable,
                        f.id AS id_fabrica,
                        f.nom AS nom_fabrica
                FROM rrhh.empleats e
                LEFT JOIN produccio.fabriques f ON e.id = f.id_responsable;
            ")); 
            
            CloseQuestion();   

            OpenQuestion("Question 2: ");       //Note: No real question, just the caption (because the subquestions will be scored individually).                                                         
            //db.ExecuteNonQuery(string.Format("INSERT INTO gerencia.responsables (nom_fabrica, nom_responsable, cognoms_responsable) VALUES ('{0}', '{1}', '{2}');", "NEW FACTORY NAME 1", "NEW EMPLOYEE NAME 1", "NEW EMPLOYEE SURNAME 1"));
            db.InsertData("gerencia", "responsables", new Dictionary<string, object>(){{"nom_fabrica", "NEW FACTORY NAME 1"}, {"nom_responsable", "NEW EMPLOYEE NAME 1"},{"cognoms_responsable","NEW EMPLOYEE SURNAME 1"}}, "id_fabrica");
            int id_fabrica = db.GetLastID("produccio", "fabriques");
            int id_empleat = db.GetLastID("rrhh", "empleats");
            
            OpenQuestion("Question 2.1: ", 1);  //Note: This question cancels the previous one, so no need to close the main question, just the subquestions (which ones we will to score individually).
            EvalQuestion(db.CheckIfTableMatchesData("gerencia", "responsables", new Dictionary<string, object>(){{"nom_responsable", "NEW EMPLOYEE NAME 1"},{"cognoms_responsable","NEW EMPLOYEE SURNAME 1"}}, id_empleat, "id_responsable"));
            EvalQuestion(db.CheckIfTableMatchesData("rrhh", "empleats", new Dictionary<string, object>(){{"nom", "NEW EMPLOYEE NAME 1"}, {"cognoms", "NEW EMPLOYEE SURNAME 1"}, {"id_cap", 1}, {"id_departament", 1}}, id_empleat));
            CloseQuestion();      

            OpenQuestion("Question 2.2: ", 1);  
            EvalQuestion(db.CheckIfTableMatchesData("gerencia", "responsables", new Dictionary<string, object>(){{"nom_fabrica", "NEW FACTORY NAME 1"}}, id_fabrica, "id_fabrica"));            
            EvalQuestion(db.CheckIfTableMatchesData("produccio", "fabriques", new Dictionary<string, object>(){{"nom", "NEW FACTORY NAME 1"}, {"pais", "SPAIN"}, {"direccio", "NONE"}, {"telefon", "+3493391000"}, {"id_responsable", id_empleat}}, id_fabrica));
            CloseQuestion();   

            OpenQuestion("Question 3: ");
            //Do not assume that INSERT on view is working, this question must be avaluated individually            
            id_empleat = db.InsertData("rrhh", "empleats", new Dictionary<string, object>(){{"id", "@(SELECT MAX(id)+1 FROM rrhh.empleats)"}, {"nom", "NEW EMPLOYEE NAME 2"}, {"cognoms", "NEW EMPLOYEE SURNAME 2"}, {"email", "NEW EMPLOYEE EMAIL 2"}, {"id_cap", 1}, {"id_departament", 1}});
            id_fabrica = db.InsertData("produccio", "fabriques", new Dictionary<string, object>(){{"id", "@(SELECT MAX(id)+1 FROM produccio.fabriques)"}, {"nom", "NEW FACTORY NAME 2"}, {"pais", "NEW FACTORY COUNTRY 2"}, {"direccio", "NEW FACTORY ADDRESS 2"}, {"telefon", "NEW FACT. PHONE 2"}, {"id_responsable", id_empleat}});

            OpenQuestion("Question 3.1: ", 1);
            EvalQuestion(db.CheckIfTableUpdatesData("gerencia", "responsables", new Dictionary<string, object>(){{"nom_responsable", "UPDATED EMPLOYEE NAME 2"}, {"cognoms_responsable", "UPDATED EMPLOYEE SURNAME 2"}}, id_empleat, "id_responsable"));
            EvalQuestion(db.CheckIfTableMatchesData("gerencia", "responsables", new Dictionary<string, object>(){{"nom_fabrica", "NEW FACTORY NAME 2"}, {"nom_responsable", "UPDATED EMPLOYEE NAME 2"}, {"cognoms_responsable","UPDATED EMPLOYEE SURNAME 2"}}, id_empleat, "id_responsable"));
            EvalQuestion(db.CheckIfTableMatchesData("rrhh", "empleats", new Dictionary<string, object>(){{"nom", "UPDATED EMPLOYEE NAME 2"}, {"cognoms", "UPDATED EMPLOYEE SURNAME 2"}, {"email", "NEW EMPLOYEE EMAIL 2"}, {"id_cap", 1}, {"id_departament", 1}}, id_empleat));

            CloseQuestion();
            OpenQuestion("Question 3.2: ", 1);
            EvalQuestion(db.CheckIfTableUpdatesData("gerencia", "responsables", new Dictionary<string, object>(){{"nom_fabrica", "UPDATED FACTORY NAME 2"}}, id_fabrica, "id_fabrica"));
            EvalQuestion(db.CheckIfTableMatchesData("gerencia", "responsables", new Dictionary<string, object>(){{"nom_fabrica", "UPDATED FACTORY NAME 2"}, {"nom_responsable", "UPDATED EMPLOYEE NAME 2"}, {"cognoms_responsable","UPDATED EMPLOYEE SURNAME 2"}}, id_fabrica, "id_fabrica"));
            EvalQuestion(db.CheckIfTableMatchesData("produccio", "fabriques", new Dictionary<string, object>(){{"nom", "UPDATED FACTORY NAME 2"}, {"pais", "NEW FACTORY COUNTRY 2"}, {"direccio", "NEW FACTORY ADDRESS 2"}, {"telefon", "NEW FACT. PHONE 2"}, {"id_responsable", id_empleat}}, id_fabrica));
            CloseQuestion();

            
            
            /*                                    
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