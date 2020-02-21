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

using System;
using System.Collections.Generic;
using AutoCheck.Core;

namespace AutoCheck.Scripts{
    public class ASIX_M02UF3_ViewsAssignment: Core.ScriptDB<CopyDetectors.SqlLog>{                       
        public ASIX_M02UF3_ViewsAssignment(string[] args): base(args){        
        }                

        public override void Run(){
            base.Run();            
            
            Output.Instance.Indent();
            Checkers.Postgres db = new Checkers.Postgres(this.Host, this.DataBase, this.Username, this.Password);            
            
            OpenQuestion("Question 1", "View creation");
                OpenQuestion("Question 1.1", 1);
                    EvalQuestion(db.CheckIfTableExists("gerencia", "responsables"));
                CloseQuestion();   

                OpenQuestion("Question 1.2", 1);
                    EvalQuestion(db.CheckIfViewMatchesDefinition("gerencia", "responsables", @"
                        SELECT  e.id AS id_responsable,
                                e.nom AS nom_responsable,
                                e.cognoms AS cognoms_responsable,
                                f.id AS id_fabrica,
                                f.nom AS nom_fabrica
                        FROM rrhh.empleats e
                        LEFT JOIN produccio.fabriques f ON e.id = f.id_responsable;"
                    ));             
                CloseQuestion();   
            CloseQuestion();   

            OpenQuestion("Question 2", "Insert rule");       //Note: No real question, just the caption (because the subquestions will be scored individually).                                                         
                EvalQuestion(db.CheckIfTableInsertsData("gerencia", "responsables", new Dictionary<string, object>(){
                    {"nom_fabrica", "NEW FACTORY NAME 1"}, 
                    {"nom_responsable", "NEW EMPLOYEE NAME 1"},
                    {"cognoms_responsable","NEW EMPLOYEE SURNAME 1"}
                }));

                int id_fabrica = db.Connector.GetLastID("produccio", "fabriques", "id");
                int id_empleat = db.Connector.GetLastID("rrhh", "empleats", "id");
            
                OpenQuestion("Question 2.1", 1);  //Note: This question cancels the previous one, so the subquestions will score individually.
                    EvalQuestion(db.CheckIfTableMatchesData("gerencia", "responsables", "id_responsable", id_empleat, new Dictionary<string, object>(){
                        {"nom_responsable", "NEW EMPLOYEE NAME 1"},
                        {"cognoms_responsable","NEW EMPLOYEE SURNAME 1"}
                    }));
                    
                    EvalQuestion(db.CheckIfTableMatchesData("rrhh", "empleats", "id", id_empleat, new Dictionary<string, object>(){
                        {"nom", "NEW EMPLOYEE NAME 1"}, 
                        {"cognoms", "NEW EMPLOYEE SURNAME 1"}, 
                        {"id_cap", 1}, 
                        {"id_departament", 1}
                    }));
                CloseQuestion();      

                OpenQuestion("Question 2.2", 1);  
                    EvalQuestion(db.CheckIfTableMatchesData("gerencia", "responsables", "id_fabrica", id_fabrica, new Dictionary<string, object>(){
                        {"nom_fabrica", "NEW FACTORY NAME 1"}
                    }));
                    
                    EvalQuestion(db.CheckIfTableMatchesData("produccio", "fabriques", "id", id_fabrica, new Dictionary<string, object>(){
                        {"nom", "NEW FACTORY NAME 1"}, 
                        {"pais", "SPAIN"}, 
                        {"direccio", "NONE"}, 
                        {"telefon", "+3493391000"}, 
                        {"id_responsable", id_empleat}
                    }));
                CloseQuestion();   
            CloseQuestion();    //Not mandatory for score computation, but restores the output indentation 

            OpenQuestion("Question 3", "Update rule");
                //Do not assume that INSERT on view is working, this question must be avaluated individually            
                id_empleat = db.Connector.InsertData("rrhh", "empleats", "id", new Dictionary<string, object>(){
                    {"id", "@(SELECT MAX(id)+1 FROM rrhh.empleats)"}, 
                    {"nom", "NEW EMPLOYEE NAME 2"}, 
                    {"cognoms", "NEW EMPLOYEE SURNAME 2"}, 
                    {"email", "NEW EMPLOYEE EMAIL 2"}, 
                    {"id_cap", 1}, 
                    {"id_departament", 1}
                });

                id_fabrica = db.Connector.InsertData("produccio", "fabriques", "id", new Dictionary<string, object>(){
                    {"id", "@(SELECT MAX(id)+1 FROM produccio.fabriques)"}, 
                    {"nom", "NEW FACTORY NAME 2"}, 
                    {"pais", "NEW FACTORY COUNTRY 2"}, 
                    {"direccio", "NEW FACTORY ADDRESS 2"}, 
                    {"telefon", "NEW FACT. PHONE 2"}, 
                    {"id_responsable", id_empleat}
                });

                OpenQuestion("Question 3.1", 1);
                    EvalQuestion(db.CheckIfTableUpdatesData("gerencia", "responsables", "id_responsable", id_empleat, new Dictionary<string, object>(){
                        {"nom_responsable", "UPDATED EMPLOYEE NAME 2"}, 
                        {"cognoms_responsable", "UPDATED EMPLOYEE SURNAME 2"}
                    }));

                    EvalQuestion(db.CheckIfTableMatchesData("gerencia", "responsables", "id_responsable", id_empleat, new Dictionary<string, object>(){
                        {"nom_fabrica", "NEW FACTORY NAME 2"}, 
                        {"nom_responsable", "UPDATED EMPLOYEE NAME 2"}, 
                        {"cognoms_responsable","UPDATED EMPLOYEE SURNAME 2"}
                    }));

                    EvalQuestion(db.CheckIfTableMatchesData("rrhh", "empleats", "id", id_empleat, new Dictionary<string, object>(){
                        {"nom", "UPDATED EMPLOYEE NAME 2"}, 
                        {"cognoms", "UPDATED EMPLOYEE SURNAME 2"}, 
                        {"email", "NEW EMPLOYEE EMAIL 2"}, 
                        {"id_cap", 1}, 
                        {"id_departament", 1}
                    }));
                CloseQuestion();

                OpenQuestion("Question 3.2", 1);
                    EvalQuestion(db.CheckIfTableUpdatesData("gerencia", "responsables", "id_fabrica", id_fabrica, new Dictionary<string, object>(){
                        {"nom_fabrica", "UPDATED FACTORY NAME 2"}
                    }));

                    EvalQuestion(db.CheckIfTableMatchesData("gerencia", "responsables", "id_fabrica", id_fabrica, new Dictionary<string, object>(){
                        {"nom_fabrica", "UPDATED FACTORY NAME 2"}, 
                        {"nom_responsable", "UPDATED EMPLOYEE NAME 2"}, 
                        {"cognoms_responsable","UPDATED EMPLOYEE SURNAME 2"}
                    }));

                    EvalQuestion(db.CheckIfTableMatchesData("produccio", "fabriques", "id", id_fabrica, new Dictionary<string, object>(){
                        {"nom", "UPDATED FACTORY NAME 2"}, 
                        {"pais", "NEW FACTORY COUNTRY 2"}, 
                        {"direccio", "NEW FACTORY ADDRESS 2"}, 
                        {"telefon", "NEW FACT. PHONE 2"}, 
                        {"id_responsable", id_empleat}
                    }));
                CloseQuestion();
            CloseQuestion();

            OpenQuestion("Question 4", "Delete rule");
                //Do not assume that INSERT on view is working, this question must be avaluated individually            
                int id_empleatDel = db.Connector.InsertData("rrhh", "empleats", "id", new Dictionary<string, object>(){
                    {"id", "@(SELECT MAX(id)+1 FROM rrhh.empleats)"}, 
                    {"nom", "NEW EMPLOYEE NAME 3"}, 
                    {"cognoms", "NEW EMPLOYEE SURNAME 3"}, 
                    {"email", "NEW EMPLOYEE EMAIL 3"}, 
                    {"id_cap", 1}, 
                    {"id_departament", 1}
                });

                int id_empleatNoDel = db.Connector.InsertData("rrhh", "empleats", "id", new Dictionary<string, object>(){
                    {"id", "@(SELECT MAX(id)+1 FROM rrhh.empleats)"}, 
                    {"nom", "NEW EMPLOYEE NAME 4"}, 
                    {"cognoms", "NEW EMPLOYEE SURNAME 4"}, 
                    {"email", "NEW EMPLOYEE EMAIL 4"}, 
                    {"id_cap", 1}, 
                    {"id_departament", 1}
                });

                int id_fabricaDel = db.Connector.InsertData("produccio", "fabriques", "id", new Dictionary<string, object>(){
                    {"id", "@(SELECT MAX(id)+1 FROM produccio.fabriques)"}, 
                    {"nom", "NEW FACTORY NAME 3"}, 
                    {"pais", "NEW FACTORY COUNTRY 3"}, 
                    {"direccio", "NEW FACTORY ADDRESS 3"}, 
                    {"telefon", "NEW FACT. PHONE 3"}, 
                    {"id_responsable", id_empleatDel}
                });

                int id_fabricaNoDel = db.Connector.InsertData("produccio", "fabriques", "id", new Dictionary<string, object>(){
                    {"id", "@(SELECT MAX(id)+1 FROM produccio.fabriques)"},
                    {"nom", "NEW FACTORY NAME 4"}, 
                    {"pais", "NEW FACTORY COUNTRY 4"}, 
                    {"direccio", "NEW FACTORY ADDRESS 4"}, 
                    {"telefon", "NEW FACT. PHONE 4"}, 
                    {"id_responsable", id_empleatNoDel}
                });

                OpenQuestion("Question 4.1", 1);
                    //Delete from factories
                    EvalQuestion(db.CheckIfTableDeletesData("gerencia", "responsables", "id_fabrica", id_fabricaDel));
                    EvalQuestion(db.CheckIfTableMatchesData("produccio", "fabriques", "id", id_fabricaDel, new Dictionary<string, object>(){
                        {"nom", "NEW FACTORY NAME 3"}, 
                        {"pais", "NEW FACTORY COUNTRY 3"}, 
                        {"direccio", "NEW FACTORY ADDRESS 3"}, 
                        {"telefon", "NEW FACT. PHONE 3"}, 
                        {"id_responsable", DBNull.Value}
                    }));

                    EvalQuestion(db.CheckIfTableMatchesData("produccio", "fabriques", "id", id_fabricaNoDel, new Dictionary<string, object>(){
                        {"nom", "NEW FACTORY NAME 4"}, 
                        {"pais", "NEW FACTORY COUNTRY 4"}, 
                        {"direccio", "NEW FACTORY ADDRESS 4"}, 
                        {"telefon", "NEW FACT. PHONE 4"}, 
                        {"id_responsable", id_empleatNoDel}
                    }));
                CloseQuestion();

                OpenQuestion("Question 4.2", 1);
                    //Delete from employees
                    EvalQuestion(db.CheckIfTableDeletesData("gerencia", "responsables", "id_responsable", id_empleatDel));
                    EvalQuestion(db.CheckIfTableMatchesData("rrhh", "empleats", "id", id_empleatDel, new Dictionary<string, object>(){
                        {"nom", "NEW EMPLOYEE NAME 3"}, 
                        {"cognoms", "NEW EMPLOYEE SURNAME 3"}, 
                        {"email", "NEW EMPLOYEE EMAIL 3"}, 
                        {"id_cap", 1}, 
                        {"id_departament", 1}
                    }));

                    EvalQuestion(db.CheckIfTableMatchesData("rrhh", "empleats", "id", id_empleatNoDel, new Dictionary<string, object>(){
                        {"nom", "NEW EMPLOYEE NAME 4"}, 
                        {"cognoms", "NEW EMPLOYEE SURNAME 4"}, 
                        {"email", "NEW EMPLOYEE EMAIL 4"}, 
                        {"id_cap", 1}, 
                        {"id_departament", 1}
                    }));

                    EvalQuestion(db.CheckIfTableMatchesAmountOfRegisters("produccio", "fabriques", "id_responsable", id_empleatDel, 0));
                CloseQuestion();

                OpenQuestion("Question 4.3", 1);
                    //Delete with no condition
                    EvalQuestion(db.CheckIfTableDeletesData("gerencia", "responsables"));
                    EvalQuestion(db.CheckIfTableMatchesAmountOfRegisters("gerencia", "responsables", 0));
                CloseQuestion();
            CloseQuestion();

            OpenQuestion("Question 5", "Permissions", 1);
                EvalQuestion(db.CheckIfTableMatchesPrivileges("it", "gerencia", "responsables", "r"));
                EvalQuestion(db.CheckIfSchemaMatchesPrivileges("it", "gerencia", "U"));
            CloseQuestion();                   

            PrintScore();
            Output.Instance.UnIndent();
        }
    }
}