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
    public class ASIX_M02UF3_ViewsExtendedAssignment: Core.ScriptDB<CopyDetectors.SqlLog>{                       
        public ASIX_M02UF3_ViewsExtendedAssignment(string[] args): base(args){        
        }                

        public override void Run(){
            base.Run();            
            
            Output.Instance.Indent();
            Checkers.Postgres db = new Checkers.Postgres(this.Host, this.DataBase, this.Username, this.Password);            
            
            OpenQuestion("Question 1", "View creation", 2);               
                EvalQuestion(db.CheckIfTableExists("gerencia", "report"));
                EvalQuestion(db.CheckIfViewMatchesDefinition("gerencia", "report", @"
                    SELECT  e.id AS id_responsable,
                            e.nom AS nom_responsable,                                
                            f.id AS id_fabrica,
                            f.nom AS nom_fabrica,
                            p.id AS id_producte,
                            p.nom AS nom_producte                                
                    FROM produccio.fabricacio x
                        LEFT JOIN produccio.productes p ON p.id = x.id_producte                            
                        LEFT JOIN produccio.fabriques f ON f.id = x.id_fabrica
                        LEFT JOIN rrhh.empleats e ON e.id = f.id_responsable;"
                ));                             
            CloseQuestion();   

            OpenQuestion("Question 2", "Insert rule", 3);       
                EvalQuestion(db.CheckIfTableInsertsData("gerencia", "report", new Dictionary<string, object>(){
                    {"nom_fabrica", "NEW FACTORY NAME 1"}, 
                    {"nom_responsable", "NEW EMPLOYEE NAME 1"},
                    {"nom_producte","NEW PRODUCT NAME 1"}
                }));

                int id_empleat = db.Connector.GetLastID("rrhh", "empleats", "id");       
                int id_fabrica = db.Connector.GetLastID("produccio", "fabriques", "id");
                int id_producte = db.Connector.GetLastID("produccio", "productes", "id");                         
            
                EvalQuestion(db.CheckIfTableMatchesData("gerencia", "report", "id_responsable", id_empleat, new Dictionary<string, object>(){
                    {"nom_responsable", "NEW EMPLOYEE NAME 1"},
                }));
                
                EvalQuestion(db.CheckIfTableMatchesData("rrhh", "empleats", "id", id_empleat, new Dictionary<string, object>(){
                    {"nom", "NEW EMPLOYEE NAME 1"}, 
                    {"cognoms", "NONE"}, 
                    {"email", "email@email.cat"},
                    {"id_cap", 4}, 
                    {"id_departament", 4}
                }));

                EvalQuestion(db.CheckIfTableMatchesData("gerencia", "report", "id_fabrica", id_fabrica, new Dictionary<string, object>(){
                    {"nom_fabrica", "NEW FACTORY NAME 1"}
                }));
                
                EvalQuestion(db.CheckIfTableMatchesData("produccio", "fabriques", "id", id_fabrica, new Dictionary<string, object>(){
                    {"nom", "NEW FACTORY NAME 1"}, 
                    {"pais", "SPAIN"}, 
                    {"direccio", "C. Anselm Rius 10. Santa Coloma de Gramenet"}, 
                    {"telefon", "+3493391000"}, 
                    {"id_responsable", id_empleat}
                }));

                EvalQuestion(db.CheckIfTableMatchesData("gerencia", "report", "id_producte", id_producte, new Dictionary<string, object>(){
                    {"nom_producte", "NEW PRODUCT NAME 1"}
                }));
                
                EvalQuestion(db.CheckIfTableMatchesData("produccio", "productes", "id", id_producte, new Dictionary<string, object>(){
                    {"nom", "NEW PRODUCT NAME 1"}, 
                    {"codi", "NONE"}, 
                    {"descripcio", "NONE"}
                }));
            CloseQuestion();    //Not mandatory for score computation, but restores the output indentation 

            OpenQuestion("Question 3", "Update rule", 2);
                //Do not assume that INSERT on view is working, this question must be avaluated individually            
                id_empleat = db.Connector.InsertData("rrhh", "empleats", "id", new Dictionary<string, object>(){
                    {"id", "@(SELECT MAX(id)+1 FROM rrhh.empleats)"}, 
                    {"nom", "NEW EMPLOYEE NAME 2"}, 
                    {"cognoms", "NEW EMPLOYEE SURNAME 2"}, 
                    {"email", "NEW EMPLOYEE EMAIL 2"}, 
                    {"id_cap", 4}, 
                    {"id_departament", 4}
                });

                id_fabrica = db.Connector.InsertData("produccio", "fabriques", "id", new Dictionary<string, object>(){
                    {"id", "@(SELECT MAX(id)+1 FROM produccio.fabriques)"}, 
                    {"nom", "NEW FACTORY NAME 2"}, 
                    {"pais", "NEW FACTORY COUNTRY 2"}, 
                    {"direccio", "NEW FACTORY ADDRESS 2"}, 
                    {"telefon", "NEW FACT. PHONE 2"}, 
                    {"id_responsable", id_empleat}
                });

                id_producte = db.Connector.InsertData("produccio", "productes", "id", new Dictionary<string, object>(){
                    {"id", "@(SELECT MAX(id)+1 FROM produccio.productes)"}, 
                    {"nom", "NEW PRODUCT NAME 2"}, 
                    {"codi", "NEW PRODUCT CODE 2"}, 
                    {"descripcio", "NEW FACTORY DESCRIPTION 2"}
                });

                EvalQuestion(db.CheckIfTableUpdatesData("gerencia", "report", "id_responsable", id_empleat, new Dictionary<string, object>(){
                    {"nom_responsable", "UPDATED EMPLOYEE NAME 2"}, 
                }));

                EvalQuestion(db.CheckIfTableMatchesData("gerencia", "report", "id_responsable", id_empleat, new Dictionary<string, object>(){
                    {"nom_fabrica", "NEW FACTORY NAME 2"}, 
                    {"nom_responsable", "UPDATED EMPLOYEE NAME 2"}, 
                    {"nom_producte","NEW PRODUCT NAME 2"}
                }));

                EvalQuestion(db.CheckIfTableMatchesData("rrhh", "empleats", "id", id_empleat, new Dictionary<string, object>(){
                    {"nom", "UPDATED EMPLOYEE NAME 2"}, 
                    {"cognoms", "NEW EMPLOYEE SURNAME 2"}, 
                    {"email", "NEW EMPLOYEE EMAIL 2"}, 
                    {"id_cap", 4}, 
                    {"id_departament", 4}
                }));

                EvalQuestion(db.CheckIfTableUpdatesData("gerencia", "report", "id_fabrica", id_fabrica, new Dictionary<string, object>(){
                    {"nom_fabrica", "UPDATED FACTORY NAME 2"}
                }));

                EvalQuestion(db.CheckIfTableMatchesData("gerencia", "report", "id_fabrica", id_fabrica, new Dictionary<string, object>(){
                    {"nom_fabrica", "UPDATED FACTORY NAME 2"}, 
                    {"nom_responsable", "UPDATED EMPLOYEE NAME 2"}, 
                    {"nom_producte","NEW PRODUCT NAME 2"}
                }));

                EvalQuestion(db.CheckIfTableMatchesData("produccio", "fabriques", "id", id_fabrica, new Dictionary<string, object>(){
                    {"nom", "UPDATED FACTORY NAME 2"}, 
                    {"pais", "NEW FACTORY COUNTRY 2"}, 
                    {"direccio", "NEW FACTORY ADDRESS 2"}, 
                    {"telefon", "NEW FACT. PHONE 2"}, 
                    {"id_responsable", id_empleat}
                }));

                EvalQuestion(db.CheckIfTableUpdatesData("gerencia", "report", "id_producte", id_producte, new Dictionary<string, object>(){
                    {"nom_producte", "UPDATED PRODUCT NAME 2"}
                }));

                EvalQuestion(db.CheckIfTableMatchesData("gerencia", "report", "id_producte", id_producte, new Dictionary<string, object>(){
                    {"nom_fabrica", "UPDATED FACTORY NAME 2"}, 
                    {"nom_responsable", "UPDATED EMPLOYEE NAME 2"}, 
                    {"nom_producte","UPDATED PRODUCT NAME 2"}
                }));

                EvalQuestion(db.CheckIfTableMatchesData("produccio", "productes", "id_producte", id_producte, new Dictionary<string, object>(){
                    {"nom", "UPDATED PRODUCT NAME 2"}, 
                    {"codi", "NEW PRODUCT CODE 2"}, 
                    {"descripcio", "NEW FACTORY DESCRIPTION 2"}
                }));
            CloseQuestion();

            OpenQuestion("Question 4");
                //Do not assume that INSERT on view is working, this question must be avaluated individually            
                id_empleat = db.Connector.InsertData("rrhh", "empleats", "id", new Dictionary<string, object>(){
                    {"id", "@(SELECT MAX(id)+1 FROM rrhh.empleats)"}, 
                    {"nom", "NEW EMPLOYEE NAME 3"}, 
                    {"cognoms", "NEW EMPLOYEE SURNAME 3"}, 
                    {"email", "NEW EMPLOYEE EMAIL 3"}, 
                    {"id_cap", 4}, 
                    {"id_departament", 4}
                });

                int id_empleatNoDel = db.Connector.InsertData("rrhh", "empleats", "id", new Dictionary<string, object>(){
                    {"id", "@(SELECT MAX(id)+1 FROM rrhh.empleats)"}, 
                    {"nom", "NEW EMPLOYEE NAME 4"}, 
                    {"cognoms", "NEW EMPLOYEE SURNAME 4"}, 
                    {"email", "NEW EMPLOYEE EMAIL 4"}, 
                    {"id_cap", 4}, 
                    {"id_departament", 4}
                });

                id_fabrica = db.Connector.InsertData("produccio", "fabriques", "id", new Dictionary<string, object>(){
                    {"id", "@(SELECT MAX(id)+1 FROM produccio.fabriques)"}, 
                    {"nom", "NEW FACTORY NAME 3"}, 
                    {"pais", "NEW FACTORY COUNTRY 3"}, 
                    {"direccio", "NEW FACTORY ADDRESS 3"}, 
                    {"telefon", "NEW FACT. PHONE 3"}, 
                    {"id_responsable", id_empleat}
                });

                int id_fabricaNoDel = db.Connector.InsertData("produccio", "fabriques", "id", new Dictionary<string, object>(){
                    {"id", "@(SELECT MAX(id)+1 FROM produccio.fabriques)"}, 
                    {"nom", "NEW FACTORY NAME 4"}, 
                    {"pais", "NEW FACTORY COUNTRY 4"}, 
                    {"direccio", "NEW FACTORY ADDRESS 4"}, 
                    {"telefon", "NEW FACT. PHONE 4"}, 
                    {"id_responsable", id_empleatNoDel}
                });

                id_producte = db.Connector.InsertData("produccio", "productes", "id", new Dictionary<string, object>(){
                    {"id", "@(SELECT MAX(id)+1 FROM produccio.productes)"}, 
                    {"nom", "NEW PRODUCT NAME 3"}, 
                    {"codi", "NEW PRODUCT CODE 3"}, 
                    {"descripcio", "NEW FACTORY DESCRIPTION 3"}
                });

                int id_producteNoDel = db.Connector.InsertData("produccio", "productes", "id", new Dictionary<string, object>(){
                    {"id", "@(SELECT MAX(id)+1 FROM produccio.productes)"}, 
                    {"nom", "NEW PRODUCT NAME 3"}, 
                    {"codi", "NEW PRODUCT CODE 3"}, 
                    {"descripcio", "NEW FACTORY DESCRIPTION 3"}
                });

                //Delete from factories
                EvalQuestion(db.CheckIfTableDeletesData("gerencia", "report", "id_fabrica", id_fabrica));
                EvalQuestion(db.CheckIfTableMatchesData("produccio", "fabriques", "id", id_fabrica, new Dictionary<string, object>(){
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

                EvalQuestion(db.CheckIfTableMatchesAmountOfRegisters("produccio", "fabricacio", "id_fabrica", id_fabrica, 0));

                //Delete from products
                EvalQuestion(db.CheckIfTableDeletesData("gerencia", "report", "id_producte", id_producte));
                EvalQuestion(db.CheckIfTableMatchesData("produccio", "productes", "id", id_producte, new Dictionary<string, object>(){
                    {"nom", "NEW PRODUCT NAME 3"}, 
                    {"codi", "NEW PRODUCT CODE 3"}, 
                    {"descripcio", "NEW FACTORY DESCRIPTION 3"}
                }));   

                EvalQuestion(db.CheckIfTableMatchesData("produccio", "productes", "id", id_producteNoDel, new Dictionary<string, object>(){
                    {"nom", "NEW PRODUCT NAME 4"}, 
                    {"codi", "NEW PRODUCT CODE 4"}, 
                    {"descripcio", "NEW FACTORY DESCRIPTION 4"}
                }));             

                EvalQuestion(db.CheckIfTableMatchesAmountOfRegisters("produccio", "fabricacio", "id_producte", id_producte, 0));

                //Delete from employees
                EvalQuestion(db.CheckIfTableDeletesData("gerencia", "report", "id_responsable", id_empleat));
                EvalQuestion(db.CheckIfTableMatchesData("rrhh", "empleats", "id", id_empleat, new Dictionary<string, object>(){
                    {"nom", "NEW EMPLOYEE NAME 3"}, 
                    {"cognoms", "NEW EMPLOYEE SURNAME 3"}, 
                    {"email", "NEW EMPLOYEE EMAIL 3"}, 
                    {"id_cap", 4}, 
                    {"id_departament", 4}
                }));

                EvalQuestion(db.CheckIfTableMatchesData("rrhh", "empleats", "id", id_empleatNoDel, new Dictionary<string, object>(){
                    {"nom", "NEW EMPLOYEE NAME 4"}, 
                    {"cognoms", "NEW EMPLOYEE SURNAME 4"}, 
                    {"email", "NEW EMPLOYEE EMAIL 4"}, 
                    {"id_cap", 4}, 
                    {"id_departament", 4}
                }));

                EvalQuestion(db.CheckIfTableMatchesAmountOfRegisters("produccio", "fabriques", "id_responsable", id_empleat, 0));                

                //Delete with no condition
                EvalQuestion(db.CheckIfTableDeletesData("gerencia", "report"));
                EvalQuestion(db.CheckIfTableMatchesAmountOfRegisters("gerencia", "report", 0));
            CloseQuestion();

            OpenQuestion("Question 5", "Permissions", 1);
                EvalQuestion(db.CheckIfTableMatchesPrivileges("it", "gerencia", "report", "r"));
                EvalQuestion(db.CheckIfSchemaMatchesPrivileges("it", "gerencia", "U"));
            CloseQuestion();                   

            PrintScore();
            Output.Instance.UnIndent();
        }
    }
}