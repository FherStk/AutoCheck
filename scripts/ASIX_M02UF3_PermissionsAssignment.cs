using System;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator.Scripts{
    public class ASIX_M02UF3_PermissionsAssignment: Core.ScriptDB<CopyDetectors.SqlLog>{                       
        public ASIX_M02UF3_PermissionsAssignment(string[] args): base(args){        
        }     

        protected override void Clean(){
            base.Clean();
            Checkers.Postgres db = new Checkers.Postgres(this.Host, this.DataBase, "postgres", "postgres", this.Output);
            db.Connector.RevokeRole("dbadmin", "prodadmin");
            db.Connector.RevokeRole("prodadmin", "prodadmin");
        }

        public override void Run(){
            base.Run();            
            Output.Indent();

            Checkers.Postgres db = new Checkers.Postgres(this.Host, this.DataBase, "postgres", "postgres", this.Output);
                        
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
            EvalQuestion(db.CheckIfTableContainsPrivileges('a', "rrhhadmin", "rrhh", "empleats"));
            CloseQuestion();

            OpenQuestion("Question 5");
            CloseQuestion("This questions does not score."); 

            OpenQuestion("Question 6", 1);
            EvalQuestion(db.CheckForeignKey("produccio", "fabricacio", "id_fabrica", "produccio", "fabriques", "id"));
            EvalQuestion(db.CheckForeignKey("produccio", "fabricacio", "id_producte", "produccio", "productes", "id"));
            EvalQuestion(db.CheckIfTableContainsPrivileges('x', "prodadmin", "produccio", "fabricacio"));
            CloseQuestion();

            OpenQuestion("Question 7", 2);
            EvalQuestion(db.CheckForeignKey("produccio", "fabriques", "id_responsable", "rrhh", "empleats", "id"));
            EvalQuestion(db.CheckIfSchemaContainsPrivilege('U', "prodadmin", "rrhh"));
            EvalQuestion(db.CheckIfTableContainsPrivileges('x', "prodadmin", "rrhh", "empleats"));
            CloseQuestion();

            OpenQuestion("Question 8", 1);
            EvalQuestion(db.CheckIfEntryRemoved("rrhh", "empleats", "id", 9));
            EvalQuestion(db.CheckIfTableMatchesPrivileges("arwxt", "rrhhadmin", "rrhh", "empleats"));
            EvalQuestion(db.CheckIfTableMatchesPrivileges("arwxt", "rrhhadmin", "rrhh", "departaments"));
            EvalQuestion(db.CheckIfTableMatchesPrivileges("arwxt", "prodadmin", "produccio", "fabriques"));
            EvalQuestion(db.CheckIfTableMatchesPrivileges("arwxt", "prodadmin", "produccio", "productes"));
            EvalQuestion(db.CheckIfTableMatchesPrivileges("arwxt", "prodadmin", "produccio", "fabricacio"));
            CloseQuestion();

            OpenQuestion("Question 9", 3);
            EvalQuestion(db.CheckRoleMembership("dbadmin", new string[]{"prodadmin", "rrhhadmin"}));
            EvalQuestion(db.CheckIfTableMatchesPrivileges("dD", "dbadmin", "rrhh", "empleats"));
            EvalQuestion(db.CheckIfTableMatchesPrivileges("dD", "dbadmin", "rrhh", "departaments"));
            EvalQuestion(db.CheckIfTableMatchesPrivileges("dD", "dbadmin", "produccio", "fabriques"));
            EvalQuestion(db.CheckIfTableMatchesPrivileges("dD", "dbadmin", "produccio", "productes"));
            EvalQuestion(db.CheckIfTableMatchesPrivileges("dD", "dbadmin", "produccio", "fabricacio"));
            CloseQuestion();
            
            PrintScore();   
            Output.UnIndent();        
        }
    }
}