/*
    Copyright © 2020 Fernando Porrino Serrano
    Third party software licenses can be found at /docs/credits/thirdparties.md

    This file is part of AutoCheck.

    AutoCheck is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    AutoCheck is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with AutoCheck.  If not, see <https://www.gnu.org/licenses/>.
*/

using AutoCheck.Core;

namespace AutoCheck.Scripts{
    public class ASIX_M02UF3_PermissionsAssignment: Core.ScriptDB<CopyDetectors.SqlLog>{                       
        public ASIX_M02UF3_PermissionsAssignment(string[] args): base(args){        
        }     

        protected override void Clean(){
            base.Clean();

            Checkers.Postgres db = new Checkers.Postgres(this.Host, this.DataBase, this.Username, this.Password);
            db.Connector.RevokeRole("dbadmin", "prodadmin");
            db.Connector.RevokeRole("prodadmin", "prodadmin");
        }

        public override void Run(){
            base.Run();        

            Output.Instance.Indent();
            Checkers.Postgres db = new Checkers.Postgres(this.Host, this.DataBase, this.Username, this.Password);
                        
            OpenQuestion("Question 1");
            CloseQuestion("This questions does not score.");            

            OpenQuestion("Question 2", 1);
            EvalQuestion(db.CheckForeignKey("rrhh", "empleats", "id_cap", "rrhh", "empleats", "id"));
            CloseQuestion();

            OpenQuestion("Question 3", 1);
            EvalQuestion(db.CheckForeignKey("rrhh", "empleats", "id_departament", "rrhh", "departaments", "id"));
            CloseQuestion();

            OpenQuestion("Question 4", 1);
            EvalQuestion(db.CheckIfEntryAdded("rrhh", "empleats", "id", 9));
            EvalQuestion(db.CheckIfTableContainsPrivileges("rrhhadmin", "rrhh", "empleats", 'a'));
            CloseQuestion();

            OpenQuestion("Question 5");
            CloseQuestion("This questions does not score."); 

            OpenQuestion("Question 6", 1);
            EvalQuestion(db.CheckForeignKey("produccio", "fabricacio", "id_fabrica", "produccio", "fabriques", "id"));
            EvalQuestion(db.CheckForeignKey("produccio", "fabricacio", "id_producte", "produccio", "productes", "id"));
            EvalQuestion(db.CheckIfTableContainsPrivileges("prodadmin", "produccio", "fabricacio", 'x'));
            CloseQuestion();

            OpenQuestion("Question 7", 2);
            EvalQuestion(db.CheckForeignKey("produccio", "fabriques", "id_responsable", "rrhh", "empleats", "id"));
            EvalQuestion(db.CheckIfSchemaContainsPrivilege("prodadmin", "rrhh", 'U'));
            EvalQuestion(db.CheckIfTableContainsPrivileges("prodadmin", "rrhh", "empleats", 'x'));
            CloseQuestion();

            OpenQuestion("Question 8", 1);
            EvalQuestion(db.CheckIfEntryRemoved("rrhh", "empleats", "id", 9));
            EvalQuestion(db.CheckIfTableMatchesPrivileges("rrhhadmin", "rrhh", "empleats", "arwxt"));
            EvalQuestion(db.CheckIfTableMatchesPrivileges("rrhhadmin", "rrhh", "departaments", "arwxt"));
            EvalQuestion(db.CheckIfTableMatchesPrivileges("prodadmin", "produccio", "fabriques", "arwxt"));
            EvalQuestion(db.CheckIfTableMatchesPrivileges("prodadmin", "produccio", "productes", "arwxt"));
            EvalQuestion(db.CheckIfTableMatchesPrivileges("prodadmin", "produccio", "fabricacio", "arwxt"));
            CloseQuestion();

            OpenQuestion("Question 9", 3);
            EvalQuestion(db.CheckRoleMembership("dbadmin", new string[]{"prodadmin", "rrhhadmin"}));
            EvalQuestion(db.CheckIfTableMatchesPrivileges("dbadmin", "rrhh", "empleats", "dD"));
            EvalQuestion(db.CheckIfTableMatchesPrivileges("dbadmin", "rrhh", "departaments", "dD"));
            EvalQuestion(db.CheckIfTableMatchesPrivileges("dbadmin", "produccio", "fabriques", "dD"));
            EvalQuestion(db.CheckIfTableMatchesPrivileges("dbadmin", "produccio", "productes", "dD"));
            EvalQuestion(db.CheckIfTableMatchesPrivileges("dbadmin", "produccio", "fabricacio", "dD"));
            CloseQuestion();
            
            PrintScore();   
            Output.Instance.UnIndent();        
        }
    }
}