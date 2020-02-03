using System;
using Npgsql;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator{
    class PermissionsValidator: ValidatorBaseDataBase{     
        //TODO: this can be converted to the new system in order to check if it's worth the effort to change everything.
        
        public PermissionsValidator(string server, string database): base(server, database){                        
        } 

        public override List<TestResult> Validate()
        {               
            Terminal.WriteLine(string.Format("Getting the permissions for the database ~{0}:", this.DataBase), ConsoleColor.Yellow);
            Terminal.Indent();
                                 
            using (this.Conn){
                this.Conn.Open();                                           
                ClearResults();
                
                //question 1
                //NONE

                //question 2                                          
                CloseTest(CheckForeginKey("rrhh", "empleats", "id_cap", "rrhh", "empleats", "id")); 

                //question 3
                CloseTest(CheckForeginKey("rrhh", "empleats", "id_departament",  "rrhh", "departaments", "id"));

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
            }        

            //no more questions, your grace            
            PrintScore();
            Terminal.UnIndent();
            
            return GlobalResults;
        }  
        private new void ClearResults(){
            base.ClearResults();

            //TODO: create the roles (rrhhadmin, prodadmin, dbadmin) and the user (it) if them not exists; also set the default permissions
            using (NpgsqlCommand cmd = new NpgsqlCommand("REVOKE rrhhadmin FROM dbadmin; REVOKE prodadmin FROM dbadmin;", this.Conn)){
                cmd.ExecuteNonQuery();                                            
            }  
        }              
        private List<string> CheckForeginKey(string schemaFrom, string tableFrom, string columnFrom, string schemaTo, string tableTo, string columnTo){    
            List<string> errors = new List<string>();                             

            OpenTest(string.Format("Getting the foreign key for ~{0}.{1} -> {2}.{3}... ", schemaFrom, tableFrom, schemaTo, tableTo), ConsoleColor.Yellow);
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT tc.constraint_name, tc.table_schema AS schemaFrom, tc.table_name AS tableFrom, kcu.column_name AS columnFrom, ccu.table_schema AS schemaTo, ccu.table_name AS tableTo, ccu.column_name AS columnTo
                                                                            FROM information_schema.table_constraints AS tc 
                                                                            JOIN information_schema.key_column_usage AS kcu ON tc.constraint_name = kcu.constraint_name
                                                                            JOIN information_schema.constraint_column_usage AS ccu ON ccu.constraint_name = tc.constraint_name
                                                                            WHERE constraint_type = 'FOREIGN KEY' AND tc.table_schema='{0}' AND tc.table_name='{1}'", schemaFrom, tableFrom), this.Conn)){
                
                try{
                    using (NpgsqlDataReader dr = cmd.ExecuteReader()){
                        int count = 0;
                        bool found = false;                    

                        while(dr.Read()){         
                            count++;               
                            if(dr["columnFrom"].ToString().Equals(columnFrom) && dr["schemaTo"].ToString().Equals(schemaTo) && dr["tableTo"].ToString().Equals(tableTo) && dr["columnTo"].ToString().Equals(columnTo)) found = true;                        
                        }

                        if(count == 0) errors.Add(String.Format("Unable to find any FOREIGN KEY for the table '{0}'", tableFrom));
                        else if(!found) errors.Add(String.Format("Unable to find the FOREIGN KEY from '{0}' to '{1}'", string.Format("{0}.{1}", schemaFrom, tableFrom), string.Format("{0}.{1}", schemaTo, tableTo)));
                    } 
                }
                catch(Exception e){
                    errors.Add(e.Message);
                }                               
            }           

            return errors;
        }             
        private List<string> CheckInsertOnEmpleats(){    
            List<string> errors = new List<string>();            
            string schema = "rrhh";
            string table = "empleats";     
                        
            //REGISTER
            OpenTest(string.Format("Getting the content of the table ~{0}.{1}... ", schema, table), ConsoleColor.Yellow);      
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT COUNT(id) FROM {0}.{1} WHERE id > 9", schema, table), this.Conn)){
                try{
                    long count = (long)cmd.ExecuteScalar();
                    if(count == 0) errors.Add(String.Format("Unable to find any new employee on table '{0}'", string.Format("{0}.{1}", schema, table)));
                }
                catch(Exception e){
                    errors.Add(e.Message);
                }
            }

            return errors;
        }   
        private List<string> CheckDeleteOnEmpleats(){    
            List<string> errors = new List<string>();            
            string schema = "rrhh";
            string table = "empleats";           

            OpenTest(string.Format("Getting the content of the table ~{0}.{1}... ", schema, table), ConsoleColor.Yellow);                
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT COUNT(id) FROM {0}.{1} WHERE id=9", schema, table), this.Conn)){
                try{
                    long count = (long)cmd.ExecuteScalar();
                    if(count > 0) errors.Add(String.Format("An existing employee was find for the id=9 on table '{0}'", string.Format("{0}.{1}", schema, table)));
                }
                catch(Exception e){
                    errors.Add(e.Message);
                }                
            }

            return errors;
        }                            
    }
}