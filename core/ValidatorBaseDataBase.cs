using Npgsql;
using System;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator{
    public abstract class ValidatorBaseDataBase : ValidatorBase{
        public string Server {get; private set;}
        public string DataBase {get; private set;}
        public NpgsqlConnection Conn {get; private set;}

        protected ValidatorBaseDataBase(string server, string database): base(){
            this.Server = server;
            this.DataBase = database;
            this.Conn = new NpgsqlConnection(string.Format("Server={0};User Id={1};Password={2};Database={3};", server, "postgres", "postgres", database));
        }  
        public new void Dispose()
        {                        
            this.Conn.Dispose();            
            base.Dispose();
        }   
        protected List<string> CheckTableMatchPrivileges(string role, string schema, string table, string expectedPrivileges, bool autoOpenTest = true){
            List<string> errors = new List<string>();                         

            
            if(autoOpenTest) OpenTest(string.Format("Getting the permissions for the role '{0}' on table ~{1}.{2}... ", role, schema, table), ConsoleColor.Yellow);
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
                        else if(!currentPrivileges.Equals(expectedPrivileges)) errors.Add(String.Format("Privileges missmatch over the table '{0}.{1}': expected->'{2}' found->'{3}'.", schema, table, expectedPrivileges, currentPrivileges));
                    }
                }
                catch(Exception e){
                    errors.Add(e.Message);
                }
            }

            return errors;
        }     
        protected List<string> CheckTableContainsPrivilege(string role, string schema, string table, char privilege, bool autoOpenTest = true){
            List<string> errors = new List<string>();                         

            if(autoOpenTest) OpenTest(string.Format("Getting the permissions for the role '{0}' on table ~{1}.{2}... ", role, schema, table), ConsoleColor.Yellow);
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
        protected List<string> CheckSchemaMatchPrivileges(string role, string schema, string expectedPrivileges, bool autoOpenTest = true){
           List<string> errors = new List<string>();                         

            if(autoOpenTest) OpenTest(string.Format("Getting the permissions for the schema ~{0}... ", schema), ConsoleColor.Yellow);
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT nspname as schema_name, r.rolname as role_name, pg_catalog.has_schema_privilege(r.rolname, nspname, 'CREATE') as create_grant, pg_catalog.has_schema_privilege(r.rolname, nspname, 'USAGE') as usage_grant
                                                                        FROM pg_namespace pn,pg_catalog.pg_roles r
                                                                        WHERE array_to_string(nspacl,',') like '%'||r.rolname||'%' AND nspowner > 1 AND nspname='{0}' AND r.rolname='{1}'", schema, role), this.Conn)){
                
                string currentPrivileges = "";
                try{
                     using (NpgsqlDataReader dr = cmd.ExecuteReader()){                   
                        //ACL letters: https://www.postgresql.org/docs/9.3/sql-grant.html
                        if(!dr.Read()) errors.Add(String.Format("Unable to find any privileges for the role '{0}' on schema '{1}'.", role, schema));
                        else{
                            if((bool)dr["usage_grant"]) currentPrivileges += "U";
                            if((bool)dr["create_grant"]) currentPrivileges += "C";                
                        }
                    }

                    if(!currentPrivileges.Equals(expectedPrivileges))
                        errors.Add(String.Format("Privileges missmatch over the schema '{0}': expected->'{1}' found->'{2}'.", schema, expectedPrivileges, currentPrivileges));
                }
                catch(Exception e){
                    errors.Add(e.Message);
                }
            }

            return errors;
        } 
        protected List<string> CheckSchemaContainsPrivilege(string role, string schema, char privilege, bool autoOpenTest = true){
            List<string> errors = new List<string>();                         

            if(autoOpenTest) OpenTest(string.Format("Getting the permissions for the schema ~{0}... ", schema), ConsoleColor.Yellow);
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
        protected List<string> CheckRoleMembership(string role, bool autoOpenTest = true){
            List<string> errors = new List<string>();                         

            if(autoOpenTest) OpenTest(string.Format("Getting the membership for the role ~{0}... ", role), ConsoleColor.Yellow);   
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