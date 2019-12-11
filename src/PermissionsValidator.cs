using System;
using Npgsql;
using System.Linq;
using ToolBox.Platform;
using ToolBox.Bridge;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator{
    class PermissionsValidator{       
        public static bool CreateDataBase(string server, string database, string sqlDump)
        {
            Utils.Write("Creating database for the student ");
            Utils.Write(database.Substring(database.IndexOf("_")+1).Replace("_", " "), ConsoleColor.DarkYellow);
            Utils.Write(": ");

            string defaultWinPath = "C:\\Program Files\\PostgreSQL\\10\\bin";   //TODO: this must be configurable (no time, sorry).
            string cmdPassword = "PGPASSWORD=postgres";
            string cmdCreate = string.Format("createdb -h {0} -U postgres -T template0 {1}", server, database);
            string cmdRestore = string.Format("psql -h {0} -U postgres {1} < {2}", server, database, sqlDump);            
            Response resp = null;
            List<string> errors = new List<string>();

            switch (OS.GetCurrent())
            {
                  case "win":                  
                    resp = Utils.Shell.Term(string.Format("SET \"{0}\" && {1}", cmdPassword, cmdCreate), Output.Hidden, defaultWinPath);
                    if(resp.code > 0) errors.Add(resp.stderr);

                    resp = Utils.Shell.Term(string.Format("SET \"{0}\" && {1}", cmdPassword, cmdRestore), Output.Hidden, defaultWinPath);
                    if(resp.code > 0) errors.Add(resp.stderr);
                    
                    break;

                case "mac":
                case "gnu":
                    resp = Utils.Shell.Term(string.Format("{0} {1}", cmdPassword, cmdCreate));
                    if(resp.code > 0) errors.Add(resp.stderr);

                    resp = Utils.Shell.Term(string.Format("{0} {1}", cmdPassword, cmdRestore));
                    if(resp.code > 0) errors.Add(resp.stderr);
                    break;
            }   

            Utils.PrintResults(errors);
            return (errors.Count == 0);
            

            /*
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C copy /b Image1.jpg + Archive.rar Image2.jpg";
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            */
             //TODO: automated import of SQL files:
                    //  From a computer with postgres installed and a folder containing the pg_dump files, run:
                    //      PGPASSWORD=postgres createdb -h IP -U postgres -T template0 empresa_NOM_COGNOM
                    //      PGPASSWORD=postgres psql -h IP -U postgres empresa_NOM_COGNOM < empresa_NOM_COGNOM.sql

                    //Requisites: install PostgreSQL client
                    //  UBUNTU: sudo apt install postgresql-client
                    //  WINDOWS: 
                    //      Download from: https://www.postgresql.org/download/windows/
                    //      Add the install path (default: "C:\Program Files\PostgreSQL\10\bin") to the variable enviroments (through control panel / system / advanced configuration)
        }
        public static void ValidateDataBase(string server, string database, bool oldVersion=true)
        {                 
            WriteHeaderForDatabasePermissions(database);                            
            using (NpgsqlConnection conn = new NpgsqlConnection(string.Format("Server={0};User Id={1};Password={2};Database={3};", server, "postgres", "postgres", database))){
                conn.Open();
                
                int score = 0;
                List<string> currentErrors;
                List<string> globalErrors;

                //question 1
                if(oldVersion) score+=1;    //Yes... I know... At least I removed it on the new version...

                //question 2
                WriteHeaderForForeignKey("rrhh.empleats", "rrhh.empleats");
                globalErrors = CheckForeginKey(conn, "rrhh", "empleats", "id_cap", "rrhh", "empleats", "id");                
                Utils.PrintResults(globalErrors);
                
                if(globalErrors.Count() == 0) score+=1;
                else globalErrors.Clear();

                //question 3
                WriteHeaderForForeignKey("rrhh.empleats", "rrhh.departaments");                
                globalErrors = CheckForeginKey(conn, "rrhh", "empleats", "id_departament", "rrhh", "departaments", "id");
                Utils.PrintResults(globalErrors);
                
                if(globalErrors.Count() == 0) score+=1;
                else globalErrors.Clear();

                //question 4               
                WriteHeaderForTableContent("rrhh.empleats");
                currentErrors = CheckInsertOnEmpleats(conn);                
                globalErrors.AddRange(currentErrors);
                Utils.PrintResults(currentErrors);

                WriteHeaderForTablePermissions("rrhhadmin", "rrhh.empleats");                                                            
                currentErrors = CheckTableContainsPrivilege(conn, "rrhhadmin", "rrhh", "empleats", 'a');                
                globalErrors.AddRange(currentErrors);
                Utils.PrintResults(currentErrors);

                if(globalErrors.Count() == 0) score+=1;
                else globalErrors.Clear();

                //question 5
                //NONE

                //question 6            
                WriteHeaderForForeignKey("produccio.fabricacio", "produccio.fabriques");                
                currentErrors = CheckForeginKey(conn, "produccio", "fabricacio", "id_fabrica", "produccio", "fabriques", "id");                
                globalErrors.AddRange(currentErrors); 
                Utils.PrintResults(currentErrors);

                WriteHeaderForForeignKey("produccio.fabricacio", "produccio.productes");                
                currentErrors = CheckForeginKey(conn, "produccio", "fabricacio", "id_producte", "produccio", "productes", "id");                
                globalErrors.AddRange(currentErrors); 
                Utils.PrintResults(currentErrors);

                WriteHeaderForTablePermissions("prodadmin", "produccio.productes");
                currentErrors = CheckTableContainsPrivilege(conn, "prodadmin", "produccio", "productes", 'x');                                       
                globalErrors.AddRange(currentErrors); 
                Utils.PrintResults(currentErrors);   

                if(globalErrors.Count() == 0) score+=1;
                else globalErrors.Clear();

                //question 7
                WriteHeaderForForeignKey("produccio.fabriques", "rrhh.empleats");                
                currentErrors = CheckForeginKey(conn, "produccio", "fabriques", "id_responsable", "rrhh", "empleats", "id");                
                globalErrors.AddRange(currentErrors);                               
                Utils.PrintResults(currentErrors);  
                
                WriteHeaderForSchemaPermissions("rrhh");
                currentErrors = CheckSchemaContainsPrivilege(conn, "prodadmin", "rrhh", 'U');                  
                globalErrors.AddRange(currentErrors); 
                Utils.PrintResults(currentErrors);

                WriteHeaderForTablePermissions("prodadmin", "rrhh.empleats");
                currentErrors = CheckTableMatchPrivileges(conn, "prodadmin", "rrhh", "empleats", "x");                  
                globalErrors.AddRange(currentErrors); 
                Utils.PrintResults(currentErrors);

                if(globalErrors.Count() == 0) score+=2;
                else globalErrors.Clear();
                
                //question 8 
                WriteHeaderForTableContent("rrhh.empleats");
                currentErrors = CheckDeleteOnEmpleats(conn);
                globalErrors.AddRange(currentErrors); 
                Utils.PrintResults(currentErrors);                

                WriteHeaderForTablePermissions("rrhhadmin", "rrhh.empleats");         
                currentErrors = CheckTableMatchPrivileges(conn, "rrhhadmin", "rrhh", "empleats", "arwxt");  
                globalErrors.AddRange(currentErrors); 
                Utils.PrintResults(currentErrors);                

                WriteHeaderForTablePermissions("rrhhadmin", "rrhh.departaments");         
                currentErrors = CheckTableMatchPrivileges(conn, "rrhhadmin", "rrhh", "departaments", "arwxt");  
                globalErrors.AddRange(currentErrors); 
                Utils.PrintResults(currentErrors);

                WriteHeaderForTablePermissions("prodadmin", "produccio.fabriques");         
                currentErrors = CheckTableMatchPrivileges(conn, "prodadmin", "produccio", "fabriques", "arwxt");  
                globalErrors.AddRange(currentErrors); 
                Utils.PrintResults(currentErrors);

                WriteHeaderForTablePermissions("prodadmin", "produccio.productes");         
                currentErrors = CheckTableMatchPrivileges(conn, "prodadmin", "produccio", "productes", "arwxt"); 
                globalErrors.AddRange(currentErrors);  
                Utils.PrintResults(currentErrors);

                WriteHeaderForTablePermissions("prodadmin", "produccio.fabricacio");         
                currentErrors = CheckTableMatchPrivileges(conn, "prodadmin", "produccio", "fabricacio", "arwxt");  
                globalErrors.AddRange(currentErrors); 
                Utils.PrintResults(currentErrors);

                if(globalErrors.Count() == 0) score+=1;
                else globalErrors.Clear();

                //question 9
                WriteHeaderForRoleMembership("dbadmin");
                currentErrors = CheckRoleMembership(conn, "dbadmin");
                globalErrors.AddRange(currentErrors); 
                Utils.PrintResults(currentErrors);

                WriteHeaderForTablePermissions("dbadmin", "rrhh.empleats");         
                currentErrors = CheckTableMatchPrivileges(conn, "dbadmin", "rrhh", "empleats", "dD");  
                globalErrors.AddRange(currentErrors); 
                Utils.PrintResults(currentErrors);                

                WriteHeaderForTablePermissions("dbadmin", "rrhh.departaments");         
                currentErrors = CheckTableMatchPrivileges(conn, "dbadmin", "rrhh", "departaments", "dD");  
                globalErrors.AddRange(currentErrors); 
                Utils.PrintResults(currentErrors);

                WriteHeaderForTablePermissions("dbadmin", "produccio.fabriques");         
                currentErrors = CheckTableMatchPrivileges(conn, "dbadmin", "produccio", "fabriques", "dD");  
                globalErrors.AddRange(currentErrors); 
                Utils.PrintResults(currentErrors);

                WriteHeaderForTablePermissions("dbadmin", "produccio.productes");         
                currentErrors = CheckTableMatchPrivileges(conn, "dbadmin", "produccio", "productes", "dD"); 
                globalErrors.AddRange(currentErrors);  
                Utils.PrintResults(currentErrors);

                WriteHeaderForTablePermissions("dbadmin", "produccio.fabricacio");         
                currentErrors = CheckTableMatchPrivileges(conn, "dbadmin", "produccio", "fabricacio", "dD");  
                globalErrors.AddRange(currentErrors); 
                Utils.PrintResults(currentErrors);

                if(globalErrors.Count() == 0) score+=(oldVersion ? 2 : 3);
                else globalErrors.Clear();
                
                //no more questions, your grace
                Utils.BreakLine();
                Utils.Write("   TOTAL SCORE: ", ConsoleColor.Cyan);
                Utils.Write(score.ToString(), (score < 5 ? ConsoleColor.Red : ConsoleColor.Green));
                Utils.BreakLine();
            }        
        }         
        private static List<string> CheckForeginKey(NpgsqlConnection conn, string schemaFrom, string tableFrom, string columnFrom, string schemaTo, string tableTo, string columnTo){    
            List<string> errors = new List<string>();                             
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@" SELECT tc.constraint_name, tc.table_schema AS schemaFrom, tc.table_name AS tableFrom, kcu.column_name AS columnFrom, ccu.table_schema AS schemaTo, ccu.table_name AS tableTo, ccu.column_name AS columnTo
                                                                            FROM information_schema.table_constraints AS tc 
                                                                            JOIN information_schema.key_column_usage AS kcu ON tc.constraint_name = kcu.constraint_name
                                                                            JOIN information_schema.constraint_column_usage AS ccu ON ccu.constraint_name = tc.constraint_name
                                                                            WHERE constraint_type = 'FOREIGN KEY' AND tc.table_schema='{0}' AND tc.table_name='{1}'", schemaFrom, tableFrom), conn)){
                
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
        private static List<string> CheckInsertOnEmpleats(NpgsqlConnection conn){    
            List<string> errors = new List<string>();            
            string schema = "rrhh";
            string table = "empleats";           

            //REGISTER
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT COUNT(id) FROM {0}.{1} WHERE id > 9", schema, table), conn)){
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
        private static List<string> CheckDeleteOnEmpleats(NpgsqlConnection conn){    
            List<string> errors = new List<string>();            
            string schema = "rrhh";
            string table = "empleats";           

            //REGISTER
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT COUNT(id) FROM {0}.{1} WHERE id=9", schema, table), conn)){
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
        private static List<string> CheckTableContainsPrivilege(NpgsqlConnection conn, string role, string schema, string table, char privilege){
            List<string> errors = new List<string>();                         

            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT grantee, privilege_type 
                                                                        FROM information_schema.role_table_grants 
                                                                        WHERE table_schema='{0}' AND table_name='{1}'", schema, table), conn)){

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
                        else if(!currentPrivileges.Contains(privilege)) errors.Add(String.Format("Unabel to find the requested privilege '{0}' over the table '{1}': found->'{2}'.", privilege, string.Format("{0}.{1}", schema, table), currentPrivileges));
                    }
                }
                catch(Exception e){
                    errors.Add(e.Message);
                }
            }

            return errors;
        } 
        private static List<string> CheckTableMatchPrivileges(NpgsqlConnection conn, string role, string schema, string table, string expectedPrivileges){
            List<string> errors = new List<string>();                         

            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT grantee, privilege_type 
                                                                        FROM information_schema.role_table_grants 
                                                                        WHERE table_schema='{0}' AND table_name='{1}'", schema, table), conn)){

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
        private static List<string> CheckSchemaContainsPrivilege(NpgsqlConnection conn, string role, string schema, char privilege){
            List<string> errors = new List<string>();                         

            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT nspname as schema_name, r.rolname as role_name, pg_catalog.has_schema_privilege(r.rolname, nspname, 'CREATE') as create_grant, pg_catalog.has_schema_privilege(r.rolname, nspname, 'USAGE') as usage_grant
                                                                        FROM pg_namespace pn,pg_catalog.pg_roles r
                                                                        WHERE array_to_string(nspacl,',') like '%'||r.rolname||'%' AND nspowner > 1 AND nspname='{0}' AND r.rolname='{1}'", schema, role), conn)){

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
        private static List<string> CheckRoleMembership(NpgsqlConnection conn, string role){
            List<string> errors = new List<string>();                         

            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT c.rolname AS rolname, b.rolname AS memberOf 
	                                                                        FROM pg_catalog.pg_auth_members m
	                                                                        JOIN pg_catalog.pg_roles b ON (m.roleid = b.oid)
	                                                                        JOIN pg_catalog.pg_roles c ON (c.oid = m.member)
	                                                                        WHERE c.rolname='{0}'", role), conn)){
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
        private static void WriteHeaderForDatabasePermissions(string database){
            Utils.Write("   Getting the permissions for the database ");
            Utils.Write(database, ConsoleColor.Yellow);
            Utils.Write(": ");           
            Utils.BreakLine();            
        }
        private static void WriteHeaderForSchemaPermissions(string schema){
            Utils.Write("       Getting the permissions for the schema ");
            Utils.Write(schema, ConsoleColor.Yellow);
            Utils.Write(": ");                     
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
        private static void WriteHeaderForRoleMembership(string role){
            Utils.Write("       Getting the membership for the role ");
            Utils.Write(role, ConsoleColor.Yellow);            
            Utils.Write(": ");
        }
        private static void WriteHeaderForForeignKey(string tableFrom, string tableTo){
            Utils.Write("       Getting the foreign key for ");
            Utils.Write(string.Format("{0} -> {1}", tableFrom, tableTo), ConsoleColor.Yellow);
            Utils.Write(": ");
        }              
    }
}