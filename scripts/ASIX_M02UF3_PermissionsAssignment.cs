using System;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator.Scripts{
    public abstract class ASIX_M02UF3_PermissionsAssignment: Core.ScriptBaseForDataBase<CopyDetectors.SqlLog>{                       
        public ASIX_M02UF3_PermissionsAssignment(string[] args): base(args){        
        }                

        public override void Single(){
            base.Single();
            Output.Indent();
            
            //TODO: simplify the script at maximum
            //  Each item from Utils will print what is doing on Output (if it is not null, no it can be used with no output also)
            //  The OpenQuestion and CloseQuestion will print also (only if its Output is not null)
            //  This allows to simplify scripting and also improve compatibility with other parts working as silent (helpers, unit tests, whatever...)

            Utils.DataBase db = new Utils.DataBase(this.Host, this.DataBase);
            OpenQuestion("Question 1: ", 0);
            Output.WriteLine("This questions does not score.");
            CloseQuestion();
            

            OpenQuestion("Question 2: ", 1);
            EvalQuestion(CheckForeignKey(db, "rrhh", "empleats", "id_cap", "rrhh", "empleats", "id"));
            CloseQuestion();

            OpenQuestion("Question 3: ", 1);
            EvalQuestion(CheckForeignKey(db, "rrhh", "empleats", "id_departament", "rrhh", "departaments", "id"));
            CloseQuestion();
            
           /*
               
                //question 4                                               
                AppendTest(CheckInsertOnEmpleats());                

                CloseTest(CheckTableContainsPrivilege("rrhhadmin", "rrhh", "empleats", 'a'));
                
                //question 5
                //NONE

                //question 6                   
                AppendTest(CheckForeginKey("produccio", "fabricacio", "id_fabrica", "produccio", "fabriques", "id"));
                AppendTest(CheckForeginKey("produccio", "fabricacio", "id_producte", "produccio", "productes", "id"));
                CloseTest(CheckTableContainsPrivilege("prodadmin", "produccio", "fabricacio", 'x'));                

                //question 7                
                AppendTest(CheckForeginKey("produccio", "fabriques", "id_responsable", "rrhh", "empleats", "id"));                                              
                AppendTest(CheckSchemaContainsPrivilege("prodadmin", "rrhh", 'U'));
                CloseTest(CheckTableMatchPrivileges("prodadmin", "rrhh", "empleats", "x"), 2);                
                
                //question 8                                 
                AppendTest(CheckDeleteOnEmpleats());                

                AppendTest(CheckTableMatchPrivileges("rrhhadmin", "rrhh", "empleats", "arwxt"));                
                AppendTest(CheckTableMatchPrivileges("rrhhadmin", "rrhh", "departaments", "arwxt"));
                AppendTest(CheckTableMatchPrivileges("prodadmin", "produccio", "fabriques", "arwxt"));
                AppendTest(CheckTableMatchPrivileges("prodadmin", "produccio", "productes", "arwxt"));
                CloseTest(CheckTableMatchPrivileges("prodadmin", "produccio", "fabricacio", "arwxt"));

                //question 9
                AppendTest(CheckRoleMembership("dbadmin"));
                AppendTest(CheckTableMatchPrivileges("dbadmin", "rrhh", "empleats", "dD"));                
                AppendTest(CheckTableMatchPrivileges("dbadmin", "rrhh", "departaments", "dD"));                
                AppendTest(CheckTableMatchPrivileges("dbadmin", "produccio", "fabriques", "dD"));                  
                AppendTest(CheckTableMatchPrivileges("dbadmin", "produccio", "productes", "dD")); 
                CloseTest(CheckTableMatchPrivileges("dbadmin", "produccio", "fabricacio", "dD"), 3);

                 PrintScore();
           */
                  

            Output.UnIndent();
            Output.BreakLine();
        }

        

        private List<string> CheckForeignKey(Utils.DataBase db, string schemaFrom, string tableFrom, string columnFrom, string schemaTo, string tableTo, string columnTo){            
            Output.Write(string.Format("Getting the foreign key for ~{0}.{1}.{2} -> {2}.{3}.{4}... ", schemaFrom,tableFrom, columnFrom, schemaTo, tableTo, columnTo), ConsoleColor.Yellow);            
            
            List<string> resp = db.CheckForeignKey(schemaFrom,tableFrom, columnFrom, schemaTo, tableTo, columnTo);
            Output.WriteResponse(resp);
            return resp;
        }
    }
}