using System;
using Npgsql;
using System.Linq;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator{
    class PermissionsValidator{
        public static void ValidateDataBase(string server, string database)
        {                 
            WriteHeaderForDatabasePermissions(database);                            
            using (NpgsqlConnection conn = new NpgsqlConnection(string.Format("Server={0};User Id={1};Password={2};Database={3};", server, "postgres", "postgres", database))){
                conn.Open();
                
                int score = 0;

                //question 1
                //NONE

                //question 2
                WriteHeaderForForeignKey("rrhh.empleats", "rrhh.empleats");
                List<string> results_fk1 = CheckForeginKey(conn, "rrhh", "empleats", "id_cap", "rrhh", "empleats", "id");                
                Utils.PrintResults(results_fk1);
                if(results_fk1.Count() == 0) score+=1;

                //question 3
                WriteHeaderForForeignKey("rrhh.empleats", "rrhh.departaments");                
                results_fk1 = CheckForeginKey(conn, "rrhh", "empleats", "id_departament", "rrhh", "departaments", "id");
                Utils.PrintResults(results_fk1);
                if(results_fk1.Count() == 0) score+=1;

                //question 4
                WriteHeaderForTableContent("rrhh.empleats");
                List<string> results_co = CheckInsertOnEmpleats(conn);
                Utils.PrintResults(results_co);

                WriteHeaderForTablePermissions("rrhhAdmin", "rrhh.empleats");                                                            
                List<string> results_pr1 = CheckTableContainsPrivilege(conn, "rrhhAdmin", "rrhh", "empleats", 'a');
                Utils.PrintResults(results_pr1);                

                if(results_co.Count() == 0 && results_pr1.Count() == 0) score+=1;

                //question 5
                //NONE

                //question 6            
                WriteHeaderForForeignKey("produccio.fabricacio", "produccio.fabriques");                
                results_fk1 = CheckForeginKey(conn, "produccio", "fabricacio", "id_fabrica", "produccio", "fabriques", "id");
                Utils.PrintResults(results_fk1);

                WriteHeaderForForeignKey("produccio.fabricacio", "produccio.productes");                
                List<string> results_fk2 = CheckForeginKey(conn, "produccio", "fabricacio", "id_producte", "produccio", "productes", "id");
                Utils.PrintResults(results_fk2);

                WriteHeaderForTablePermissions("prodAdmin", "produccio.productes");
                results_pr1 = CheckTableContainsPrivilege(conn, "prodAdmin", "produccio", "productes", 'x');                       
                Utils.PrintResults(results_pr1);                            

                if(results_fk1.Count() == 0 && results_fk2.Count() == 0 && results_pr1.Count() == 0) 
                    score+=1;

                //question 7
                WriteHeaderForForeignKey("produccio.fabriques", "rrhh.empleats");                
                results_fk1 = CheckForeginKey(conn, "produccio", "fabriques", "id_responsable", "rrhh", "empleats", "id");
                Utils.PrintResults(results_fk1);                                

                
                WriteHeaderForSchemaPermissions("rrhh");
                //FALTA: GRANT USAGE ON SCHEMA rrhh TO prodAdmin; 


                WriteHeaderForTablePermissions("prodAdmin", "rrhh.empleats");
                results_pr1 = CheckTableMatchPrivileges(conn, "prodAdmin", "rrhh", "empleats", "x");  
                Utils.PrintResults(results_pr1);

                if(results_fk1.Count() == 0 && results_pr1.Count() == 0) score+=2;
                
                //question 8
                //HASTA AQUI NO SE DEBEN COMPROBAR LOS PRIVILEGIOS ESTRICTOS.
                //USAR EL "CONTAINS" ANTES DE AQUI, Y LLEGADOS A ESTE PUNTO COMPROBAR LOS PRIVILEGIOS EXACTOS
                //List<string> results_pr1 = CheckTableMatchPrivileges(conn, "rrhhAdmin", "rrhh", "empleats", "arwxt");   
                /*
                WriteHeaderForTablePermissions("rrhhAdmin", "rrhh.departaments");
                List<string> results_pr2 = CheckTableMatchPrivileges(conn, "rrhhAdmin", "rrhh", "departaments", "arwxt");                                               
                Utils.PrintResults(results_pr2);

                  WriteHeaderForTablePermissions("prodAdmin", "produccio.productes");
                results_pr1 = CheckTableMatchPrivileges(conn, "prodAdmin", "produccio", "productes", "arwxt");                       
                Utils.PrintResults(results_pr1);

                WriteHeaderForTablePermissions("prodAdmin", "produccio.fabriques");
                results_pr2 = CheckTableMatchPrivileges(conn, "prodAdmin", "produccio", "fabriques", "arwxt");                       
                Utils.PrintResults(results_pr2);

                  WriteHeaderForTablePermissions("prodAdmin", "produccio.fabricacio");
                List<string> results_pr3 = CheckTableMatchPrivileges(conn, "prodAdmin", "produccio", "fabricacio", "arwxt");                       
                Utils.PrintResults(results_pr3);
                */

                Utils.BreakLine();
                Utils.Write("   TOTAL SCORE: ", ConsoleColor.Cyan);
                Utils.Write(score.ToString(), (score < 5 ? ConsoleColor.Red : ConsoleColor.Green));
                Utils.BreakLine();
            }

            /*
                For getting role membership (look for another simples query).
                SELECT r.rolname, r.rolsuper, r.rolinherit,
                r.rolcreaterole, r.rolcreatedb, r.rolcanlogin,
                r.rolconnlimit, r.rolvaliduntil,
                ARRAY(SELECT b.rolname
                        FROM pg_catalog.pg_auth_members m
                        JOIN pg_catalog.pg_roles b ON (m.roleid = b.oid)
                        WHERE m.member = r.oid) as memberof
                , r.rolreplication
                , r.rolbypassrls
                FROM pg_catalog.pg_roles r
                WHERE r.rolname !~ '^pg_'
                ORDER BY 1;
            */
        }         

        private static List<string> CheckForeginKey(NpgsqlConnection conn, string schemaFrom, string tableFrom, string columnFrom, string schemaTo, string tableTo, string columnTo){    
            List<string> errors = new List<string>();                             
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@" SELECT tc.constraint_name, tc.table_schema AS schemaFrom, tc.table_name AS tableFrom, kcu.column_name AS columnFrom, ccu.table_schema AS schemaTo, ccu.table_name AS tableTo, ccu.column_name AS columnTo
                                                                            FROM information_schema.table_constraints AS tc 
                                                                            JOIN information_schema.key_column_usage AS kcu ON tc.constraint_name = kcu.constraint_name
                                                                            JOIN information_schema.constraint_column_usage AS ccu ON ccu.constraint_name = tc.constraint_name
                                                                            WHERE constraint_type = 'FOREIGN KEY' AND tc.table_schema='{0}' AND tc.table_name='{1}'", schemaFrom, tableFrom), conn)){
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

            return errors;
        }             
        private static List<string> CheckInsertOnEmpleats(NpgsqlConnection conn){    
            List<string> errors = new List<string>();            
            string schema = "rrhh";
            string table = "empleats";           

            //REGISTER
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT COUNT(id) FROM {0}.{1} WHERE id > 9", schema, table), conn)){
                long count = (long)cmd.ExecuteScalar();
                if(count == 0) errors.Add(String.Format("Unable to find any new employee on table '{0}'", string.Format("{0}.{1}", schema, table)));
            }

            return errors;
        }        
        private static List<string> CheckTableContainsPrivilege(NpgsqlConnection conn, string role, string schema, string table, char privilege){
            List<string> errors = new List<string>();                         

            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT grantee, privilege_type 
                                                                        FROM information_schema.role_table_grants 
                                                                        WHERE table_schema='{0}' AND table_name='{1}'", schema, table), conn)){




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
                    else if(!currentPrivileges.Contains(privilege)) errors.Add(String.Format("Unabel to find the requested privilege '{0}' over the table '{1}': found->'{2}'.", privilege, string.Format("{0}.{1}", schema, table), currentPrivileges));
                }
            }

            return errors;
        } 
        private static List<string> CheckTableMatchPrivileges(NpgsqlConnection conn, string role, string schema, string table, string expectedPrivileges){
            List<string> errors = new List<string>();                         

            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT grantee, privilege_type 
                                                                        FROM information_schema.role_table_grants 
                                                                        WHERE table_schema='{0}' AND table_name='{1}'", schema, table), conn)){




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

            return errors;
        } 
        private static List<string> CheckSchemaContainsPrivilege(NpgsqlConnection conn, string role, string schema, char privilege){
            List<string> errors = new List<string>();                         

            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT nspname as schema_name, r.rolname as role_name, pg_catalog.has_schema_privilege(r.rolname, nspname, 'CREATE') as create_grant, pg_catalog.has_schema_privilege(r.rolname, nspname, 'USAGE') as usage_grant
                                                                        FROM pg_namespace pn,pg_catalog.pg_roles r
                                                                        WHERE array_to_string(nspacl,',') like '%'||r.rolname||'%' AND nspowner > 1 AND schema_name='{0}' AND role_name='{1}'", schema, role), conn)){




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

            return errors;
        } 
        private static void WriteHeaderForDatabasePermissions(string database){
            Utils.Write("   Getting the permissions for the database ");
            Utils.Write(database, ConsoleColor.Yellow);
            Utils.Write(": ");           
            Utils.BreakLine();            
        }
        private static void WriteHeaderForSchemaPermissions(string schema){
            Utils.Write("   Getting the permissions for the schema ");
            Utils.Write(schema, ConsoleColor.Yellow);
            Utils.Write(": ");           
            Utils.BreakLine();            
        }
         private static void WriteHeaderForTableContent(string table){
            Utils.Write("       Getting the content of the table ");
            Utils.Write(table, ConsoleColor.Yellow);
            Utils.Write(": ");
        }
        private static void WriteHeaderForTablePermissions(string role, string table){
            Utils.Write("       Getting the permissions for the role ");
            Utils.Write(role, ConsoleColor.Yellow);
            Utils.Write(" on table ");
            Utils.Write(table, ConsoleColor.Yellow);
            Utils.Write(": ");
        }
        private static void WriteHeaderForForeignKey(string tableFrom, string tableTo){
            Utils.Write("       Getting the foreign key for ");
            Utils.Write(string.Format("{0} -> {1}", tableFrom, tableTo), ConsoleColor.Yellow);
            Utils.Write(": ");
        }      
    }
}