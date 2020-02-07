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
            
            OpenQuestion("Question 1: ");
                OpenQuestion("Question 1.1: ", 1);
                    EvalQuestion(db.CheckIfTableExists("gerencia", "responsables"));
                CloseQuestion();   

                OpenQuestion("Question 1.2: ", 1);
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
            CloseQuestion();   

            OpenQuestion("Question 2: ");       //Note: No real question, just the caption (because the subquestions will be scored individually).                                                         
                db.InsertData("gerencia", "responsables", new Dictionary<string, object>(){{"nom_fabrica", "NEW FACTORY NAME 1"}, {"nom_responsable", "NEW EMPLOYEE NAME 1"},{"cognoms_responsable","NEW EMPLOYEE SURNAME 1"}}, "id_fabrica");
                int id_fabricaDel = db.GetLastID("produccio", "fabriques", "id");
                int id_empleatDel = db.GetLastID("rrhh", "empleats", "id");
            
                OpenQuestion("Question 2.1: ", 1);  //Note: This question cancels the previous one, so the subquestions will score individually.
                    EvalQuestion(db.CheckIfTableMatchesData("gerencia", "responsables", new Dictionary<string, object>(){{"nom_responsable", "NEW EMPLOYEE NAME 1"},{"cognoms_responsable","NEW EMPLOYEE SURNAME 1"}}, "id_responsable", id_empleatDel));
                    EvalQuestion(db.CheckIfTableMatchesData("rrhh", "empleats", new Dictionary<string, object>(){{"nom", "NEW EMPLOYEE NAME 1"}, {"cognoms", "NEW EMPLOYEE SURNAME 1"}, {"id_cap", 1}, {"id_departament", 1}}, "id", id_empleatDel));
                CloseQuestion();      

                OpenQuestion("Question 2.2: ", 1);  
                    EvalQuestion(db.CheckIfTableMatchesData("gerencia", "responsables", new Dictionary<string, object>(){{"nom_fabrica", "NEW FACTORY NAME 1"}}, "id_fabrica", id_fabricaDel));
                    EvalQuestion(db.CheckIfTableMatchesData("produccio", "fabriques", new Dictionary<string, object>(){{"nom", "NEW FACTORY NAME 1"}, {"pais", "SPAIN"}, {"direccio", "NONE"}, {"telefon", "+3493391000"}, {"id_responsable", id_empleatDel}}, "id", id_fabricaDel));
                CloseQuestion();   
            CloseQuestion();    //Not mandatory for score computation, but restores the output indentation 

            OpenQuestion("Question 3: ");
                //Do not assume that INSERT on view is working, this question must be avaluated individually            
                id_empleatDel = db.InsertData("rrhh", "empleats", new Dictionary<string, object>(){{"id", "@(SELECT MAX(id)+1 FROM rrhh.empleats)"}, {"nom", "NEW EMPLOYEE NAME 2"}, {"cognoms", "NEW EMPLOYEE SURNAME 2"}, {"email", "NEW EMPLOYEE EMAIL 2"}, {"id_cap", 1}, {"id_departament", 1}}, "id");
                id_fabricaDel = db.InsertData("produccio", "fabriques", new Dictionary<string, object>(){{"id", "@(SELECT MAX(id)+1 FROM produccio.fabriques)"}, {"nom", "NEW FACTORY NAME 2"}, {"pais", "NEW FACTORY COUNTRY 2"}, {"direccio", "NEW FACTORY ADDRESS 2"}, {"telefon", "NEW FACT. PHONE 2"}, {"id_responsable", id_empleatDel}}, "id");

                OpenQuestion("Question 3.1: ", 1);
                    EvalQuestion(db.CheckIfTableUpdatesData("gerencia", "responsables", new Dictionary<string, object>(){{"nom_responsable", "UPDATED EMPLOYEE NAME 2"}, {"cognoms_responsable", "UPDATED EMPLOYEE SURNAME 2"}}, "id_responsable", id_empleatDel));
                    EvalQuestion(db.CheckIfTableMatchesData("gerencia", "responsables", new Dictionary<string, object>(){{"nom_fabrica", "NEW FACTORY NAME 2"}, {"nom_responsable", "UPDATED EMPLOYEE NAME 2"}, {"cognoms_responsable","UPDATED EMPLOYEE SURNAME 2"}}, "id_responsable", id_empleatDel));
                    EvalQuestion(db.CheckIfTableMatchesData("rrhh", "empleats", new Dictionary<string, object>(){{"nom", "UPDATED EMPLOYEE NAME 2"}, {"cognoms", "UPDATED EMPLOYEE SURNAME 2"}, {"email", "NEW EMPLOYEE EMAIL 2"}, {"id_cap", 1}, {"id_departament", 1}}, "id", id_empleatDel));
                CloseQuestion();

                OpenQuestion("Question 3.2: ", 1);
                    EvalQuestion(db.CheckIfTableUpdatesData("gerencia", "responsables", new Dictionary<string, object>(){{"nom_fabrica", "UPDATED FACTORY NAME 2"}}, "id_fabrica", id_fabricaDel));
                    EvalQuestion(db.CheckIfTableMatchesData("gerencia", "responsables", new Dictionary<string, object>(){{"nom_fabrica", "UPDATED FACTORY NAME 2"}, {"nom_responsable", "UPDATED EMPLOYEE NAME 2"}, {"cognoms_responsable","UPDATED EMPLOYEE SURNAME 2"}}, "id_fabrica", id_fabricaDel));
                    EvalQuestion(db.CheckIfTableMatchesData("produccio", "fabriques", new Dictionary<string, object>(){{"nom", "UPDATED FACTORY NAME 2"}, {"pais", "NEW FACTORY COUNTRY 2"}, {"direccio", "NEW FACTORY ADDRESS 2"}, {"telefon", "NEW FACT. PHONE 2"}, {"id_responsable", id_empleatDel}}, "id", id_fabricaDel));
                CloseQuestion();
            CloseQuestion();

            OpenQuestion("Question 4: ");
                //Do not assume that INSERT on view is working, this question must be avaluated individually            
                id_empleatDel = db.InsertData("rrhh", "empleats", new Dictionary<string, object>(){{"id", "@(SELECT MAX(id)+1 FROM rrhh.empleats)"}, {"nom", "NEW EMPLOYEE NAME 3"}, {"cognoms", "NEW EMPLOYEE SURNAME 3"}, {"email", "NEW EMPLOYEE EMAIL 3"}, {"id_cap", 1}, {"id_departament", 1}}, "id");
                int id_empleatNoDel = db.InsertData("rrhh", "empleats", new Dictionary<string, object>(){{"id", "@(SELECT MAX(id)+1 FROM rrhh.empleats)"}, {"nom", "NEW EMPLOYEE NAME 4"}, {"cognoms", "NEW EMPLOYEE SURNAME 4"}, {"email", "NEW EMPLOYEE EMAIL 4"}, {"id_cap", 1}, {"id_departament", 1}}, "id");
                id_fabricaDel = db.InsertData("produccio", "fabriques", new Dictionary<string, object>(){{"id", "@(SELECT MAX(id)+1 FROM produccio.fabriques)"}, {"nom", "NEW FACTORY NAME 3"}, {"pais", "NEW FACTORY COUNTRY 3"}, {"direccio", "NEW FACTORY ADDRESS 3"}, {"telefon", "NEW FACT. PHONE 3"}, {"id_responsable", id_empleatDel}}, "id");
                int id_fabricaNoDel = db.InsertData("produccio", "fabriques", new Dictionary<string, object>(){{"id", "@(SELECT MAX(id)+1 FROM produccio.fabriques)"}, {"nom", "NEW FACTORY NAME 4"}, {"pais", "NEW FACTORY COUNTRY 4"}, {"direccio", "NEW FACTORY ADDRESS 4"}, {"telefon", "NEW FACT. PHONE 4"}, {"id_responsable", id_empleatNoDel}}, "id");

                OpenQuestion("Question 4.1: ", 1);
                    //Delete from factories
                    EvalQuestion(db.CheckIfTableDeletesData("gerencia", "responsables", "id_fabrica", id_fabricaDel));
                    EvalQuestion(db.CheckIfTableMatchesData("produccio", "fabriques", new Dictionary<string, object>(){{"nom", "NEW FACTORY NAME 3"}, {"pais", "NEW FACTORY COUNTRY 3"}, {"direccio", "NEW FACTORY ADDRESS 3"}, {"telefon", "NEW FACT. PHONE 3"}, {"id_responsable", DBNull.Value}}, "id", id_fabricaDel));
                    EvalQuestion(db.CheckIfTableMatchesData("produccio", "fabriques", new Dictionary<string, object>(){{"nom", "NEW FACTORY NAME 4"}, {"pais", "NEW FACTORY COUNTRY 4"}, {"direccio", "NEW FACTORY ADDRESS 4"}, {"telefon", "NEW FACT. PHONE 4"}, {"id_responsable", id_empleatNoDel}}, "id", id_fabricaNoDel));
                CloseQuestion();

                OpenQuestion("Question 4.2: ", 1);
                    //Delete from employees
                    EvalQuestion(db.CheckIfTableDeletesData("gerencia", "responsables", "id_responsable", id_empleatDel));
                    EvalQuestion(db.CheckIfTableMatchesData("rrhh", "empleats", new Dictionary<string, object>(){{"nom", "NEW EMPLOYEE NAME 3"}, {"cognoms", "NEW EMPLOYEE SURNAME 3"}, {"email", "NEW EMPLOYEE EMAIL 3"}, {"id_cap", 1}, {"id_departament", 1}}, "id", id_empleatDel));
                    EvalQuestion(db.CheckIfTableMatchesData("rrhh", "empleats", new Dictionary<string, object>(){{"nom", "NEW EMPLOYEE NAME 4"}, {"cognoms", "NEW EMPLOYEE SURNAME 4"}, {"email", "NEW EMPLOYEE EMAIL 4"}, {"id_cap", 1}, {"id_departament", 1}}, "id", id_empleatNoDel));
                    EvalQuestion(db.CheckIfTableMatchesAmountOfRegisters("produccio", "fabriques", "id_responsable", id_empleatDel, 0));
                CloseQuestion();

                OpenQuestion("Question 4.3: ", 1);
                    //Delete with no condition
                    EvalQuestion(db.CheckIfTableDeletesData("gerencia", "responsables"));
                    EvalQuestion(db.CheckIfTableMatchesAmountOfRegisters("gerencia", "responsables", 0));
                CloseQuestion();
            CloseQuestion();

            OpenQuestion("Question 5: ", 1);
                EvalQuestion(db.CheckIfTableMatchesPrivileges("it", "gerencia", "responsables", "r"));
                EvalQuestion(db.CheckIfSchemaMatchesPrivileges("it", "gerencia", "U"));
            CloseQuestion();                   

            PrintScore();
            Output.UnIndent();
        }
    }
}