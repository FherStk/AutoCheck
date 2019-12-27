using System;
using Npgsql;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator{
    class PermissionsValidator: ValidatorBaseDataBase{     
        public PermissionsValidator(string server, string database): base(server, database){                        
        } 

        public override List<TestResult> Validate()
        {   
            ClearResults();
            Terminal.WriteCaption(string.Format("   Getting the permissions for the database ~{0}:", this.DataBase), ConsoleColor.Yellow);
                                 
            using (this.Conn){
                this.Conn.Open();                                           
                
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
                AppendTest(CheckSchemaContainsPrivilege("rrhhadmin", "rrhh", 'U'));
                CloseTest(CheckTableMatchPrivileges("rrhhadmin", "rrhh", "empleats", "x"), 2);                
                
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

            OpenTest(string.Format("   Getting the foreign key for ~{0}.{1} -> {2}.{3}:", schemaFrom, tableFrom, schemaTo, tableTo), ConsoleColor.Yellow);
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@" SELECT tc.constraint_name, tc.table_schema AS schemaFrom, tc.table_name AS tableFrom, kcu.column_name AS columnFrom, ccu.table_schema AS schemaTo, ccu.table_name AS tableTo, ccu.column_name AS columnTo
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
            OpenTest(string.Format("   Getting the content of the table ~{0}.{1}:", schema, table), ConsoleColor.Yellow);      
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

            OpenTest(string.Format("   Getting the content of the table ~{0}.{1}:", schema, table), ConsoleColor.Yellow);                
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
        private List<string> CheckTableContainsPrivilege(string role, string schema, string table, char privilege){
            List<string> errors = new List<string>();                         

            OpenTest(string.Format("   Getting the permissions for the role '{0}' on table ~{1}.{2}:", role, schema, table), ConsoleColor.Yellow);
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT grantee, privilege_type 
                                                                        FROM information_schema.role_table_grants 
                                                                        WHERE table_schema='{0}' AND table_name='{1}'", schema, table), this.Conn)){

                try{
                    using (NpgsqlDataReader dr = cmd.ExecuteReader()){
                        int count = 0;
                        string currentPrivileges = "";
                        
                        //ACL letters: https://www.postgresql.org/docs/9.3/sql-grant.html
                        while(dr.Read()){         
                            count++;               
                            if(dr["grantee"].ToString().Equals(role, StringComparison.CurrentCultureIgnoreCase)){                            
                                if(dr["privilege_type"].ToString().Equals("SELECT")) currentPrivileges = currentPrivileges + "r";
                                if(dr["privilege_type"].ToString().Equals("UPDATE")) currentPrivileges = currentPrivileges + "w";
                                if(dr["privilege_type"].ToString().Equals("INSERT")) currentPrivileges = currentPrivileges + "a";
                                if(dr["privilege_type"].ToString().Equals("DELETE")) currentPrivileges = currentPrivileges + "d";
                                if(dr["privilege_type"].ToString().Equals("TRUNCATE")) currentPrivileges = currentPrivileges + "D";
                                if(dr["privilege_type"].ToString().Equals("REFERENCES")) currentPrivileges = currentPrivileges + "x";
                                if(dr["privilege_type"].ToString().Equals("TRIGGER")) currentPrivileges = currentPrivileges + "t";                        
                            }
                        }

                        if(count == 0) errors.Add(String.Format("Unable to find any privileges for the table '{0}'", table));
                        else if(!currentPrivileges.Contains(privilege)) errors.Add(String.Format("Unable to find the requested privilege '{0}' over the table '{1}': found->'{2}'.", privilege, string.Format("{0}.{1}", schema, table), currentPrivileges));
                    }
                }
                catch(Exception e){
                    errors.Add(e.Message);
                }
            }

            return errors;
        } 
        private List<string> CheckTableMatchPrivileges(string role, string schema, string table, string expectedPrivileges){
            List<string> errors = new List<string>();                         

            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT grantee, privilege_type 
                                                                        FROM information_schema.role_table_grants 
                                                                        WHERE table_schema='{0}' AND table_name='{1}'", schema, table), this.Conn)){

                try{
                    using (NpgsqlDataReader dr = cmd.ExecuteReader()){
                        int count = 0;
                        string currentPrivileges = "";
                        
                        //ACL letters: https://www.postgresql.org/docs/9.3/sql-grant.html
                        while(dr.Read()){         
                            count++;               
                            if(dr["grantee"].ToString().Equals(role, StringComparison.CurrentCultureIgnoreCase)){                            
                                if(dr["privilege_type"].ToString().Equals("SELECT")) currentPrivileges = currentPrivileges + "r";
                                if(dr["privilege_type"].ToString().Equals("UPDATE")) currentPrivileges = currentPrivileges + "w";
                                if(dr["privilege_type"].ToString().Equals("INSERT")) currentPrivileges = currentPrivileges + "a";
                                if(dr["privilege_type"].ToString().Equals("DELETE")) currentPrivileges = currentPrivileges + "d";
                                if(dr["privilege_type"].ToString().Equals("TRUNCATE")) currentPrivileges = currentPrivileges + "D";
                                if(dr["privilege_type"].ToString().Equals("REFERENCES")) currentPrivileges = currentPrivileges + "x";
                                if(dr["privilege_type"].ToString().Equals("TRIGGER")) currentPrivileges = currentPrivileges + "t";                        
                            }
                        }

                        if(count == 0) errors.Add(String.Format("Unable to find any privileges for the table '{0}'", table));
                        else if(!currentPrivileges.Equals(expectedPrivileges)) errors.Add(String.Format("Privileges missmatch over the table '{0}': expected->'{1}' found->'{2}'.", string.Format("{0}.{1}", schema, table), expectedPrivileges, currentPrivileges));
                    }
                }
                catch(Exception e){
                    errors.Add(e.Message);
                }
            }

            return errors;
        } 
        private List<string> CheckSchemaContainsPrivilege(string role, string schema, char privilege){
            List<string> errors = new List<string>();                         

            OpenTest(string.Format("       Getting the permissions for the schema ~{0}:", schema), ConsoleColor.Yellow);
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT nspname as schema_name, r.rolname as role_name, pg_catalog.has_schema_privilege(r.rolname, nspname, 'CREATE') as create_grant, pg_catalog.has_schema_privilege(r.rolname, nspname, 'USAGE') as usage_grant
                                                                        FROM pg_namespace pn,pg_catalog.pg_roles r
                                                                        WHERE array_to_string(nspacl,',') like '%'||r.rolname||'%' AND nspowner > 1 AND nspname='{0}' AND r.rolname='{1}'", schema, role), this.Conn)){

                try{
                     using (NpgsqlDataReader dr = cmd.ExecuteReader()){                   
                        //ACL letters: https://www.postgresql.org/docs/9.3/sql-grant.html
                        if(!dr.Read()) errors.Add(String.Format("Unable to find any privileges for the role '{0}' on schema '{1}'.", role, schema));
                        else{
                            switch(privilege){
                                case 'U':
                                if(!(bool)dr["usage_grant"]) errors.Add(String.Format("Unable to find the USAGE privilege for the role '{0}' on schema '{1}'.", role, schema));
                                break;

                                case 'C':
                                if(!(bool)dr["create_grant"]) errors.Add(String.Format("Unable to find the CREATE privilege for the role '{0}' on schema '{1}'.", role, schema));
                                break;
                            }                        
                        }
                    }
                }
                catch(Exception e){
                    errors.Add(e.Message);
                }
            }

            return errors;
        } 
        private List<string> CheckRoleMembership(string role){
            List<string> errors = new List<string>();                         

            OpenTest(string.Format("       Getting the membership for the role ~{0}:", role), ConsoleColor.Yellow);   
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT c.rolname AS rolname, b.rolname AS memberOf 
	                                                                        FROM pg_catalog.pg_auth_members m
	                                                                        JOIN pg_catalog.pg_roles b ON (m.roleid = b.oid)
	                                                                        JOIN pg_catalog.pg_roles c ON (c.oid = m.member)
	                                                                        WHERE c.rolname='{0}'", role), this.Conn)){
                try{
                    using (NpgsqlDataReader dr = cmd.ExecuteReader()){   
                        bool prodadmin = false;
                        bool rrhhadmin = false;

                        while(dr.Read()){         
                            if(dr["memberOf"].ToString().Equals("prodadmin")) prodadmin = true;
                            if(dr["memberOf"].ToString().Equals("rrhhadmin")) rrhhadmin = true;                        
                        }

                        if(!prodadmin) errors.Add(String.Format("The role '{0}' does not belongs to the role 'prodadmin'.", role));
                        if(!rrhhadmin) errors.Add(String.Format("The role '{0}' does not belongs to the role 'rrhhadmin'.", role));
                    }
                }
                catch(Exception e){
                    errors.Add(e.Message);
                }                
            }

            return errors;
        }                     
    }
}