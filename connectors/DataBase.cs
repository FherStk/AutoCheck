using Npgsql;
using System;
using System.Data;
using ToolBox.Bridge;
using ToolBox.Platform;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator.Connectors{        
    public partial class DataBase{        
        public string DBAddress {get; private set;}
        public string DBName {get; private set;}
        public NpgsqlConnection Conn {get; private set;}
        public string Student{
            get{
                return Core.Utils.DataBaseNameToStudentName(this.DBName);
            }
        }

        public DataBase(string host, string database, string username, string password){
            this.DBAddress = host;
            this.DBName = database;
            this.Conn = new NpgsqlConnection(string.Format("Server={0};User Id={1};Password={2};Database={3};", host, "postgres", "postgres", database));
        }         
        public void Dispose()
        {                        
            this.Conn.Dispose();            
        }   
        /// <summary>
        /// Selects data from a single table, the 'ExecuteNonQuery' method can be used for complex selects (union, join, etc.).
        /// </summary>
        /// <param name="schema">Schema where the table is.</param>
        /// <param name="table">The table where the data will be added.</param>
        /// <param name="fields">Key-value pairs of data [field, value], subqueries as values must start with @.</param>
        /// <param name="filterField">The field name used to find the affected registries.</param>
        /// <param name="filterValue">The field value used to find the affected registries.</param> 
        /// <param name="filterOperator">The operator to use, % for LIKE.</param>     
        /// <returns>The data selected.</returns>
        public DataSet SelectData(string[] fields, string schema, string table, string filterField, object filterValue, char filterOperator='='){           
            string query = string.Format("SELECT {0} FROM {1}.{2}", string.Join(",", fields), schema, table);
            if(!string.IsNullOrEmpty(filterField))
                query += string.Format(" WHERE {0}{1}{2}", filterField, filterOperator, ParseObjectForSQL(filterValue));

            return ExecuteQuery(query);           
        }
        /// <summary>
        /// Inserts new data into a table.
        /// </summary>
        /// <param name="schema">Schema where the table is.</param>
        /// <param name="table">The table where the data will be added.</param>
        /// <param name="fields">Key-value pairs of data [field, value], subqueries as values must start with @.</param>
        /// <param name="pkField">The primary key field name.</param>
        /// <returns>The primary key of the new item.</returns>
        public int InsertData(Dictionary<string, object> fields, string schema, string table, string pkField){
            string query = string.Format("INSERT INTO {0}.{1} ({2}) VALUES (", schema, table, string.Join(',', fields.Keys));
            foreach(string field in fields.Keys)
                query += string.Format("{0},", ParseObjectForSQL(fields[field]));
            
            query = string.Format("{0})", query.TrimEnd(','));
            ExecuteNonQuery(query);

            return GetLastID(schema, table, pkField);            
        }
        /// <summary>
        /// Updates all the data from a table.
        /// </summary>
        /// <param name="schema">Schema where the table is.</param>
        /// <param name="table">The table where the data will be updated.</param>
        /// <param name="fields">Key-value pairs of data [field, value], subqueries as values must start with @.</param>        
        public void UpdateData(Dictionary<string, object> fields, string schema, string table){
            UpdateData(fields, schema, table, null, 0);
        }
        /// <summary>
        /// Update some data from a table, the 'ExecuteNonQuery' method can be used for complex filters (and, or, etc.).
        /// </summary>
        /// <param name="schema">Schema where the table is.</param>
        /// <param name="table">The table where the data will be updated.</param>
        /// <param name="fields">Key-value pairs of data [field, value], subqueries as values must start with @.</param>
        /// <param name="filterField">The field name used to find the affected registries.</param>
        /// <param name="filterValue">The field value used to find the affected registries.</param> 
        /// <param name="filterOperator">The operator to use, % for LIKE.</param>
        public void UpdateData(Dictionary<string, object> fields, string schema, string table, string filterField, object filterValue, char filterOperator='='){                             
            string query = string.Format("UPDATE {0}.{1} SET", schema, table);
            foreach(string field in fields.Keys){
                bool quotes = (fields[field].GetType() == typeof(string) && fields[field].ToString().Substring(0, 1) != "@");
                query += (quotes ? string.Format(" {0}{1}'{2}',", field, filterOperator, fields[field]) : string.Format(" {0}{1}'{2}',", field, filterOperator, fields[field].ToString().TrimStart('@')));
            }
            
            query = query.TrimEnd(',');
            
            if(!string.IsNullOrEmpty(filterField))
                query += string.Format(" WHERE {0}={1};", filterField, ParseObjectForSQL(filterValue));

            ExecuteNonQuery(query);
        }
        /// <summary>
        /// Delete all the data from a table.
        /// </summary>
        /// <param name="schema">Schema where the table is.</param>
        /// <param name="table">The table where the data will be updated.</param>        
        public void DeleteData(string schema, string table){
            DeleteData(schema, table, null, 0);
        }
        /// <summary>
        /// Delete some data from a table, the 'ExecuteNonQuery' method can be used for complex filters (and, or, etc.).
        /// </summary>
        /// <param name="schema">Schema where the table is.</param>
        /// <param name="table">The table where the data will be updated.</param>        
        /// <param name="filterField">The field name used to find the affected registries.</param>
        /// <param name="filterValue">The field value used to find the affected registries.</param> 
        /// <param name="filterOperator">The operator to use, % for LIKE.</param>
        public void DeleteData(string schema, string table, string filterField, object filterValue, char filterOperator='='){           
            string query = string.Format("DELETE FROM {0}.{1}", schema, table);
            if(!string.IsNullOrEmpty(filterField)) query += string.Format(" WHERE {0}{1}{2};", filterField, filterOperator, ParseObjectForSQL(filterValue));

            ExecuteNonQuery(query);            
        }        
        /// <summary>
        /// Revokes a role from a group or role or user.
        /// </summary>
        /// <param name="role">The role to revoke.</param>
        /// <param name="item">The group, role or user which role will be revoked.</param>
        public void RevokeRole(string role, string item){
            ExecuteNonQuery(string.Format("REVOKE {0} FROM {1};", role, item));            
        }        
        /// <summary>
        /// Determines if the database exists or not in the server.
        /// </summary>
        /// <returns>True if the database exists, False otherwise.</returns>
        /// <summary>
        /// Compares two select queries.
        /// </summary>
        /// <param name="expected">The left-side select query.</param>
        /// <param name="compared">The right-side select query.</param>
        /// <returns>True if both select queries are equivalent.</returns>
        public bool CompareSelects(string expected, string compared){
            return (0 == (long)ExecuteScalar(string.Format("SELECT COUNT(*) FROM (({0}) EXCEPT ({1})) AS result;", CleanSqlQuery(expected), CleanSqlQuery(compared))));
        }
        public bool ExistsDataBase()
        {            
            try{
                this.Conn.Open();               
                return true;
            }   
            catch(Exception e){                    
                if(e.Message.Contains(string.Format("database \"{0}\" does not exist", this.DBName))) return false;
                else throw e;
            } 
            finally{
                this.Conn.Close();
            }
        }  
        /// <summary>
        /// Creates a new database using a SQL Dump file.
        /// </summary>
        /// <param name="sqlDumpFilePath">The SQL Dump file path.</param>
        public void CreateDataBase(string sqlDumpFilePath)
        {
            string defaultWinPath = "C:\\Program Files\\PostgreSQL\\10\\bin";   
            string cmdPassword = "PGPASSWORD=postgres";
            string cmdCreate = string.Format("createdb -h {0} -U postgres -T template0 {1}", this.DBAddress, this.DBName);
            string cmdRestore = string.Format("psql -h {0} -U postgres {1} < \"{2}\"", this.DBAddress, this.DBName, sqlDumpFilePath);            
            Response resp = null;
            
            switch (OS.GetCurrent())
            {
                //Once path is ok on windows and unix the almost same code will be used.
                case "win":                  
                    resp = Shell.Instance.Term(string.Format("SET \"{0}\" && {1}", cmdPassword, cmdCreate), ToolBox.Bridge.Output.Hidden, defaultWinPath);
                    if(resp.code > 0) throw new Exception(resp.stderr.Replace("\n", ""));

                    resp = Shell.Instance.Term(string.Format("SET \"{0}\" && {1}", cmdPassword, cmdRestore), ToolBox.Bridge.Output.Hidden, defaultWinPath);
                    if(resp.code > 0) throw new Exception(resp.stderr.Replace("\n", ""));
                    
                    break;

                case "mac":                
                case "gnu":
                    resp = Shell.Instance.Term(string.Format("{0} {1}", cmdPassword, cmdCreate));
                    if(resp.code > 0) throw new Exception(resp.stderr.Replace("\n", ""));

                    resp = Shell.Instance.Term(string.Format("{0} {1}", cmdPassword, cmdRestore.Replace("\"", "'")));
                    if(resp.code > 0) throw new Exception(resp.stderr.Replace("\n", ""));
                    break;
            }   
        } 
        /// <summary>
        /// Counts how many registers appears in a table.
        /// </summary>
        /// <param name="schema">The schema containing the table to check.</param>
        /// <param name="table">The table to check.</param>
        /// <returns>Number of items.</returns>
        public long CountRegisters(string schema, string table){
            return CountRegisters(schema, table, null, 0);
        }
        /// <summary>
        /// Counts how many registers appears in a table using the primary key as a filter, the 'ExecuteNonQuery' method can be used for complex filters (and, or, etc.).
        /// </summary>
        /// <param name="schema">The schema containing the table to check.</param>
        /// <param name="table">The table to check.</param>        
        /// <param name="filterField">The field name used to find the affected registries.</param>
        /// <param name="filterValue">The field value used to find the affected registries.</param>        
        /// <param name="filterOperator">The operator to use, % for LIKE.</param>
        /// <returns>Number of items.</returns>
        public long CountRegisters(string schema, string table, string filterField, object filterValue, char filterOperator='='){
            string query = string.Format("SELECT COUNT(*) FROM {0}.{1}", schema, table);
            if(!string.IsNullOrEmpty(filterField)) query = string.Format("{0} WHERE {1}{2}{3}", query, filterField, filterOperator, ParseObjectForSQL(filterValue));
            
            return (long)ExecuteScalar(query);
        }    
        /// <summary>
        /// Returns the highest registry ID.
        /// </summary>
        /// <param name="schema">The schema containing the table to check.</param>
        /// <param name="table">The table to check.</param>
        /// <param name="pkField">The primary key field name.</param>
        /// <returns></returns>
        public int GetLastID(string schema, string table, string pkField){
            return (int)ExecuteScalar(string.Format(@"SELECT MAX({0}) FROM {1}.{2};", pkField, schema, table));            
        }  
        /// <summary>
        /// Returns the lowest registry ID.
        /// </summary>
        /// <param name="schema">The schema containing the table to check.</param>
        /// <param name="table">The table to check.</param>
        /// <param name="pkField">The primary key field name.</param>
        /// <returns></returns>
        public int GetFirstID(string schema, string table, string pkField){
            return (int)ExecuteScalar(string.Format(@"SELECT MIN({0}) FROM {1}.{2};", pkField, schema, table));                
        }   
        /// <summary>
        /// Returns the selected registry ID, the 'ExecuteNonQuery' method can be used for complex filters (and, or, etc.).
        /// </summary>
        /// <param name="schema">The schema containing the table to check.</param>
        /// <param name="table">The table to check.</param>
        /// <param name="pkField">The primary key field name.</param>
        /// <param name="filterField">The field name used to find the affected registries.</param>
        /// <param name="filterValue">The field value used to find the affected registries.</param>
        /// <param name="filterOperator">The operator to use, % for LIKE.</param>
        /// <returns>The first ID found.</returns>
        public int GetID(string schema, string table, string pkField, string filterField, object filterValue, char filterOperator='='){
            return (int)ExecuteScalar((string.Format("SELECT {0} FROM {1}.{2} WHERE {3}{4}{5} LIMIT 1;", pkField, schema, table, filterField, (filterOperator == '%' ? " LIKE " : filterOperator.ToString()), ParseObjectForSQL(filterValue))));
        }        
        /// <summary>
        /// Given a view, return its definition as a select query.
        /// </summary>
        /// <param name="schema">The schema containing the table to check.</param>
        /// <param name="table">The table to check.</param>
        /// <returns>A select query.</returns>
        public string GetViewDefinition(string schema, string view){
            return (string)ExecuteScalar(string.Format("SELECT view_definition FROM information_schema.views WHERE table_schema='{0}' AND table_name='{1}'", schema, view));
        }
        /// <summary>
        /// Returns the table privileges.
        /// </summary>
        /// <param name="role">The role which privileges will be checked.</param>
        /// <param name="schema">The schema containing the table to check.</param>
        /// <param name="table">The table which privileges will be checked against the role's ones.</param>
        /// <returns>The table privileges.</returns>
        public DataSet GetTablePrivileges(string role, string schema, string table){
            return ExecuteQuery(string.Format("SELECT grantee, privilege_type FROM information_schema.role_table_grants WHERE table_schema='{0}' AND table_name='{1}'", schema, table));            
        } 
        /// <summary>
        /// Returns the schema privileges.
        /// </summary>
        /// <param name="role">The role which privileges will be checked.</param>
        /// <param name="schema">The schema containing the table to check.</param>
        /// <returns>The schema privileges.</returns>
        public DataSet GetSchemaPrivileges(string role, string schema){
            return ExecuteQuery(string.Format("SELECT nspname as schema_name, r.rolname as role_name, pg_catalog.has_schema_privilege(r.rolname, nspname, 'CREATE') as create_grant, pg_catalog.has_schema_privilege(r.rolname, nspname, 'USAGE') as usage_grant FROM pg_namespace pn,pg_catalog.pg_roles r WHERE array_to_string(nspacl,',') like '%'||r.rolname||'%' AND nspowner > 1 AND nspname='{0}' AND r.rolname='{1}'", schema, role));            
        } 
        /// <summary>
        /// Get a list of the groups and/or roles where the fiven role belongs.
        /// </summary>
        /// <param name="role">The role to check.</param>
        /// <returns>A set of groups and/or roles</returns>
        public DataSet GetRoleMembership(string role){
            return ExecuteQuery(string.Format("SELECT c.rolname AS rolname, b.rolname AS memberOf FROM pg_catalog.pg_auth_members m JOIN pg_catalog.pg_roles b ON (m.roleid = b.oid) JOIN pg_catalog.pg_roles c ON (c.oid = m.member) WHERE c.rolname='{0}'", role));            
        } 
        /// <summary>
        /// Returns the information about all the foreign keys defined over a table.
        /// </summary>
        /// <param name="schemaFrom">The schema where the foreign has been defined.</param>
        /// <param name="tableFrom">The table where the foreign has been defined.</param>
        /// <returns>The foreign keys data.</returns>
        public DataSet GetForeignKeys(string schemaFrom, string tableFrom){
            return ExecuteQuery(string.Format(@"SELECT tc.constraint_name, tc.table_schema AS schemaFrom, tc.table_name AS tableFrom, kcu.column_name AS columnFrom, ccu.table_schema AS schemaTo, ccu.table_name AS tableTo, ccu.column_name AS columnTo
                                                                            FROM information_schema.table_constraints AS tc 
                                                                            JOIN information_schema.key_column_usage AS kcu ON tc.constraint_name = kcu.constraint_name
                                                                            JOIN information_schema.constraint_column_usage AS ccu ON ccu.constraint_name = tc.constraint_name
                                                                            WHERE constraint_type = 'FOREIGN KEY' AND tc.table_schema='{0}' AND tc.table_name='{1}'", schemaFrom, tableFrom));            
        } 
        /// <summary>
        /// Runs a query that produces no output.
        /// </summary>
        /// <param name="query">The query to run.</param>
        public void ExecuteNonQuery(string query){
            try{
                this.Conn.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, this.Conn)){
                    cmd.ExecuteNonQuery();                                            
                }
            }   
            finally{
                this.Conn.Close();
            }
        }
        /// <summary>
        /// Runs a query that produces an output as a set of data.
        /// </summary>
        /// <param name="query">The query to run.</param>
        /// <returns>The dataset containing all the output.</returns>
        public DataSet ExecuteQuery(string query){
            //TODO: this must return a DATASET
            try{
                this.Conn.Open();
                DataSet ds = new DataSet();
                using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(query, this.Conn)){                        
                    da.Fill(ds);                    
                    return ds;                      
                }
            }   
            finally{
                this.Conn.Close();
            }
        }
        /// <summary>
        /// Runs a query that produces an output as a single data.
        /// </summary>
        /// <param name="query">The query to run.</param>
        /// <returns>The dataset containing all the output.</returns>
        public object ExecuteScalar(string query){
            //TODO: this must return a DATASET
            try{
                this.Conn.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, this.Conn)){
                    return cmd.ExecuteScalar();                                            
                }
            }   
            finally{
                this.Conn.Close();
            }
        }         
        /// <summary>
        /// Given a SQL query, removes the extra spaces, breaklines and also the last ';'.
        /// </summary>
        /// <param name="sql">The original SQL query.</param>
        /// <returns>The clean SQL query.</returns>
        private string CleanSqlQuery(string sql){
            sql = sql.Replace("\r\n", "").Replace("\n", "");            
            do sql = sql.Replace("  ", " ").Trim();
            while(sql.Contains("  "));

            return sql.TrimEnd(';');
        }       
        /// <summary>
        /// Given an object, determines if the item needs quotes for being used into a query.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>The item ready to be used int an SQL query.</returns>
        private string ParseObjectForSQL(object item){
            bool quotes = (item.GetType() == typeof(string) && item.ToString().Substring(0, 1) != "@");
            return (quotes ? string.Format(" '{0}'", item) : string.Format(" {0}", item.ToString().TrimStart('@')));
        }
    }
}