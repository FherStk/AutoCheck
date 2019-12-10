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
                
                WriteHeaderForTablePermissions("rrhh.empleats");                
                Utils.PrintResults(CheckTableEmpleats(conn));
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

        private static void WriteHeaderForDatabasePermissions(string database){
            WriteHeader("Getting the permissions for the database", database, 0);
            Utils.BreakLine();            
        }
        private static void WriteHeaderForTablePermissions(string table){
            WriteHeader("Getting the permissions for the table", table, 1);            
        }
        private static void WriteHeader(string info, string item, int tabs){
            string prefix = "";
            for(int i=0; i<tabs; i++)
                prefix+="   ";

            Utils.Write(string.Format("{0}{1}", prefix, info));
            Utils.Write(string.Format("'{0}'", item), ConsoleColor.Yellow);
            Utils.Write(": ");
        }

         private static List<string> CheckTableEmpleats(NpgsqlConnection conn){    
            List<string> errors = new List<string>();            
            string schema = "rrhh";
            string table = "empleats";
            string role = "rrhhAdmin";

            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(@"SELECT grantee, privilege_type 
                                                                        FROM information_schema.role_table_grants 
                                                                        WHERE table_schema='{0}' AND table_name='{1}'", schema, table), conn)){




                using (NpgsqlDataReader dr = cmd.ExecuteReader()){
                    int count = 0;
                    bool insert = false;
                    bool select = false;
                    bool update = false;
                    bool delete = true;
                    bool truncate = true;
                    bool references = false;
                    bool trigger = false;

                    while(dr.Read()){         
                        count++;               
                        if(dr["grantee"].ToString().Equals(role, StringComparison.CurrentCultureIgnoreCase)){
                            if(dr["privilege_type"].ToString().Equals(nameof(insert), StringComparison.CurrentCultureIgnoreCase)) insert = true;
                            if(dr["privilege_type"].ToString().Equals(nameof(select), StringComparison.CurrentCultureIgnoreCase)) select = true;
                            if(dr["privilege_type"].ToString().Equals(nameof(update), StringComparison.CurrentCultureIgnoreCase)) update = true;
                            if(dr["privilege_type"].ToString().Equals(nameof(delete), StringComparison.CurrentCultureIgnoreCase)) delete = false;
                            if(dr["privilege_type"].ToString().Equals(nameof(truncate), StringComparison.CurrentCultureIgnoreCase)) truncate = false;
                            if(dr["privilege_type"].ToString().Equals(nameof(references), StringComparison.CurrentCultureIgnoreCase)) references = true;
                            if(dr["privilege_type"].ToString().Equals(nameof(trigger), StringComparison.CurrentCultureIgnoreCase)) trigger = true;
                        }
                    }

                    if(count == 0) errors.Add(String.Format("Unable to find any privileges for the table '{0}'", table));
                    else{
                        if(!insert) errors.Add(String.Format("Unable to find the {0} privilege for the role '{1}' in the table '{2}'", nameof(insert).ToUpper(), role, string.Format("{0}.{1}", schema, table)));
                        if(!select) errors.Add(String.Format("Unable to find the {0} privilege for the role '{1}' in the table '{2}'", nameof(select).ToUpper(), role, string.Format("{0}.{1}", schema, table)));
                        if(!update) errors.Add(String.Format("Unable to find the {0} privilege for the role '{1}' in the table '{2}'", nameof(update).ToUpper(), role, string.Format("{0}.{1}", schema, table)));
                        if(!delete) errors.Add(String.Format("Found out the {0} privilege for the role '{1}' in the table '{2}'", nameof(delete).ToUpper(), role, string.Format("{0}.{1}", schema, table)));
                        if(!truncate) errors.Add(String.Format("Found out the {0} privilege for the role '{1}' in the table '{2}'", nameof(truncate).ToUpper(), role, string.Format("{0}.{1}", schema, table)));
                        if(!references) errors.Add(String.Format("Unable to find the {0} privilege for the role '{1}' in the table '{2}'", nameof(references).ToUpper(), role, string.Format("{0}.{1}", schema, table)));
                        if(!trigger) errors.Add(String.Format("Unable to find the {0} privilege for the role '{1}' in the table '{2}'", nameof(trigger).ToUpper(), role, string.Format("{0}.{1}", schema, table)));
                    }                    
                }
            }

            return errors;
        }     
    }
}