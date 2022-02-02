/*
    Copyright © 2022 Fernando Porrino Serrano
    Third party software licenses can be found at /docs/credits/credits.md

    This file is part of AutoCheck.

    AutoCheck is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    AutoCheck is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with AutoCheck.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using Npgsql;
using AutoCheck.Core.Exceptions;


namespace AutoCheck.Core.Connectors{      
    /// <summary>
    /// Allows in/out operations and/or data validations with a PostgreSQL instance.
    /// </summary>     
    public class Postgres: Base{     

#region "Attributes"        
        /// <summary>
        /// The connection used for communication between PostgreSQL and the current application.
        /// </summary>
        /// <value></value>
        public NpgsqlConnection Conn {get; private set;}         
        
        /// <summary>
        /// PostgreSQL host address.
        /// </summary>
        /// <value></value>
        public string Host {get; private set;}      
        
        /// <summary>
        /// The PostgreSQL database host address, with a running instance allowing remote connections.
        /// </summary>
        /// <value></value>  
        public string Database {get; private set;}        
        
        /// <summary>
        /// The PostgreSQL database username, which will be used to perform operations.
        /// </summary>
        /// <value></value>  
        public string User  {get; private set;}    
        
        /// <summary>
        /// 
        /// The PostgreSQL database password, which will be used to perform operations.
        /// </summary>
        /// <value></value>    
        protected string Password {get; private set;}

        /// <summary>
        /// The path to the bin folder [only needed for windows systems].
        /// </summary>
        /// <value></value>    
        public string BinPath {get; private set;}    
                      
#endregion        
#region "Constructor / Destructor"
        /// <summary>
        /// Creates a new connector instance.
        /// </summary>
        /// <param name="host">Host address in order to connect with the running PostgreSQL service.</param>
        /// <param name="database">The PostgreSQL database name.</param>
        /// <param name="username">The PostgreSQL database username, which will be used to perform operations.</param>
        /// <param name="password">The PostgreSQL database password, which will be used to perform operations.</param>
        /// <param name="binPath">The path to the bin folder [only needed for windows systems].</param>
        public Postgres(string host, string database, string username, string password=null, string binPath = "C:\\Program Files\\PostgreSQL\\10\\bin"){       
            if(string.IsNullOrEmpty(host)) throw new ArgumentNullException("host");
            if(string.IsNullOrEmpty(database)) throw new ArgumentNullException("database");
            if(string.IsNullOrEmpty(username)) throw new ArgumentNullException("username");
            if(database.Contains(" ")) throw new ArgumentInvalidException($"Invalid database name '{database}', spaces are not allowed.");
            //TODO: check for other forbidden chars when creating a bbdd

            this.Host = host;
            this.Database = database;
            this.User = username;
            this.Password = password;
            this.BinPath = binPath;
            this.Conn = new NpgsqlConnection(GetConnectionString(host, database, username, password));           
        }       
        
        /// <summary>
        /// Cleans and releases memory for unnatended objects.
        /// </summary>
        public override void Dispose()
        {                        
            this.Conn.Dispose();            
        }   

        /// <summary>
        /// Test the connection to the database, so an exception will be thrown if any problem occurs.
        /// </summary>
        public void TestConnection(){
            try{
                this.Conn.Open();
                this.Conn.Close();
            }
            catch(Exception ex){
                throw new ConnectionInvalidException("Invalid connection string data has been provided, check the inner exception for further details.", ex);
            } 
        }  
#endregion        
#region "Native Query"        
        /// <summary>
        /// Runs a query that produces an output as a set of data.
        /// </summary>
        /// <param name="query">The query to run.</param>
        /// <returns>The dataset containing all the output.</returns>
        public DataSet ExecuteQuery(string query){
            if(string.IsNullOrEmpty(query)) throw new ArgumentNullException("query");

            try{                
                this.Conn.Open();                
                DataSet ds = new DataSet();
                query = CleanSqlQuery(query);

                using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(query, this.Conn)){    
                    da.Fill(ds);

                    if(query.StartsWith("SELECT")){
                        string temp = query.Substring(query.IndexOf("FROM")+4).Trim();
                        if(temp.Contains(" ")) temp = temp.Substring(0, temp.IndexOf(" ")).Trim();

                        string[] names = temp.Split(".");
                        if(names.Length == 1) ds.Tables[0].TableName = names[0];                        
                        else{
                            ds.Tables[0].Namespace = names[0];
                            ds.Tables[0].TableName = names[1];
                        }
                    }

                    return ds;                      
                }
            }
            catch(Exception ex){
                throw new QueryInvalidException($"Unable to run te given SQL query '{query}'. Please, check the inner exception for further details.", ex);
            }             
            finally{
                this.Conn.Close();
            }
        }

        /// <summary>
        /// Runs a query that produces no output.
        /// </summary>
        /// <param name="query">The query to run.</param>
        public void ExecuteNonQuery(string query){
            if(string.IsNullOrEmpty(query)) throw new ArgumentNullException("query");

            try{
                this.Conn.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, this.Conn)){
                    cmd.ExecuteNonQuery();                                            
                }
            } 
            catch(Exception ex){
                throw new QueryInvalidException($"Unable to run te given SQL query '{query}'. Please, check the inner exception for further details.", ex);
            }          
            finally{
                this.Conn.Close();
            }
        }

        /// <summary>
        /// Runs a query that produces an output as a single data.
        /// </summary>
        /// <param name="query">The query to run.</param>
        /// <returns>The requested item.</returns>
        public T ExecuteScalar<T>(string query){
            if(string.IsNullOrEmpty(query)) throw new ArgumentNullException("query");

            try{
                this.Conn.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, this.Conn)){
                    return (T)cmd.ExecuteScalar();                                            
                }
            }  
            catch(Exception ex){
                throw new QueryInvalidException($"Unable to run te given SQL query '{query}'. Please, check the inner exception for further details.", ex);
            }             
            finally{
                this.Conn.Close();
            }
        }  
#endregion
#region "CREATE and DROP"
        /// <summary>
        /// Checks if the database exists.
        /// </summary>
        /// <returns>True if the database exists.</returns>
        public bool ExistsDataBase()
        {            
            try{
                //The connection succeeeds on recently removed BBDD.
                var count = ExecuteScalar<long>($"SELECT COUNT(*) FROM pg_database WHERE datname='{this.Database}'");
                return count > 0;
            }   
            catch(Exception e){                    
                if(e.InnerException.Message.StartsWith("3D000")) return false;
                else throw;
            } 
        }                     

        /// <summary>
        /// Creates a new database instance using an SQL Dump file.
        /// </summary>
        /// <param name="filePath">The SQL Dump file path.</param>
        /// <param name="replace">Existing database will be drop and replaced by the new one.</param>
        public void CreateDataBase(string dumpPath, bool replace = false)
        { 
            if(string.IsNullOrEmpty(dumpPath)) throw new ArgumentNullException("filePath");
            if(replace && ExistsDataBase()) DropDataBase();

            CreateDataBase(replace);
            ImportSqlDump(dumpPath);
        } 

        /// <summary>
        /// Creates a new and empty database.
        /// </summary>
        /// <param name="replace">Existing database will be drop and replaced by the new one.</param>
        public void CreateDataBase(bool replace = false)
        { 
            //TODO: this should work with remote...
            string cmdPassword = $"PGPASSWORD={this.Password}";
            string cmdCreate = $"createdb -h {this.Host} -U {this.User} -T template0 {this.Database}";
            
            using(var ls = new Shell()){
                switch (Utils.CurrentOS)
                {
                    //Once path is ok on windows and unix the almost same code will be used.
                    case Utils.OS.WIN:
                        var win = ls.RunCommand($"SET \"{cmdPassword}\" && {cmdCreate}", this.BinPath);
                        if(win.code > 0) throw new Exception(win.response.Replace("\n", ""));                                                
                        break;

                    case Utils.OS.MAC:
                    case Utils.OS.GNU:
                        var gnu = ls.RunCommand($"{cmdPassword} {cmdCreate}");
                        if(gnu.code > 0) throw new Exception(gnu.response.Replace("\n", ""));
                        break;
                } 
            }
        }

        /// <summary>
        /// Imports an SQL into the current database.
        /// </summary>
        /// <param name="dumpPath"></param>
        public void ImportSqlDump(string dumpPath)
        { 
            if(string.IsNullOrEmpty(dumpPath)) throw new ArgumentNullException("filePath");
            if(!File.Exists(dumpPath)) throw new FileNotFoundException("filePath");
            
            string cmdPassword = $"PGPASSWORD={this.Password}";
            string cmdRestore = $"psql -h {this.Host} -U {this.User} {this.Database} < \"{dumpPath}\"";
            
            using(var ls = new Shell()){
                switch (Utils.CurrentOS)
                {
                    //Once path is ok on windows and unix the almost same code will be used.
                    case Utils.OS.WIN:
                        var win = ls.RunCommand($"SET \"{cmdPassword}\" && {cmdRestore}", this.BinPath);
                        if(win.code > 0) throw new Exception(win.response.Replace("\n", ""));
                        
                        break;

                    case Utils.OS.MAC:
                    case Utils.OS.GNU:
                        var gnu = ls.RunCommand($"{cmdPassword} {cmdRestore}");
                        if(gnu.code > 0) throw new Exception(gnu.response.Replace("\n", ""));
                        break;
                } 
            }  
        }            
       
        /// <summary>
        /// Drops the current database.
        /// </summary>
        public void DropDataBase()
        {                         
            try{      
                //Step 1: close all open connections from a connection to another DB
                this.Conn = new NpgsqlConnection(GetConnectionString(this.Host, "postgres", this.User, this.Password));
                ExecuteNonQuery($"SELECT pg_terminate_backend(pg_stat_activity.pid) FROM pg_stat_activity WHERE pg_stat_activity.datname = '{this.Database}' AND pid <> pg_backend_pid();");                
                      
                //Step 2: drop the database
                string cmdPassword = $"PGPASSWORD={this.Password}";
                string cmdDrop = $"dropdb -h {this.Host} -U {this.User} {this.Database}";
                
                using(var ls = new Shell()){
                    switch (Utils.CurrentOS)
                    {
                        //Once path is ok on windows and unix the almost same code will be used.
                        case Utils.OS.WIN:                  
                            var win = ls.RunCommand($"SET \"{cmdPassword}\" && {cmdDrop}", this.BinPath);
                            if(win.code > 0) throw new Exception(win.response.Replace("\n", ""));                    
                            break;

                        case Utils.OS.MAC:
                        case Utils.OS.GNU:
                            var gnu = ls.RunCommand($"{cmdPassword} {cmdDrop}");
                            if(gnu.code > 0) throw new Exception(gnu.response.Replace("\n", ""));
                            break;
                    }   
                }
            }   
            finally{
                this.Conn = new NpgsqlConnection(GetConnectionString(this.Host, this.Database, this.User, this.Password));
                                           
                try{
                    //The first query after a drop alwais fails (even from other connectors), so it will be forced     
                    ExecuteNonQuery($"SELECT COUNT(*) FROM pg_database WHERE datname='{this.Database}';");
                }
                catch{
                    //none
                }                
            }  
        } 
       
#endregion
#region "Users"
        /// <summary>
        /// Requests for all the users created.
        /// </summary>
        /// <returns>A dataset containing the requested data ('username', 'attributes').</returns>
        public DataSet GetUsers(){
            return ExecuteQuery(@"SELECT u.usename AS username,
                                    CASE WHEN u.usesuper AND u.usecreatedb THEN 
                                        CAST('superuser, create database' AS pg_catalog.text)
                                    WHEN u.usesuper THEN 
                                        CAST('superuser' AS pg_catalog.text)
                                    WHEN u.usecreatedb 
                                        THEN CAST('create database' AS pg_catalog.text)
                                    ELSE CAST('' AS pg_catalog.text)
                                    END AS attributes
                                FROM pg_catalog.pg_user u
                                ORDER BY 1;");
        }

        /// <summary>
        /// Counts how many user accounts are in the database.
        /// </summary>
        /// <returns>A dataset containing the requested data.</returns>
        public long CountUsers(){
            return ExecuteScalar<long>(@"SELECT COUNT (*) FROM pg_catalog.pg_user;");
        }

        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="role">The user name to create.</param>
        public void CreateUser(string user, string password = ""){
            if(string.IsNullOrEmpty(user)) throw new ArgumentNullException("role");
            string query = $"CREATE USER {user}";

            if(!string.IsNullOrEmpty(password)) query += $" WITH PASSWORD '{password}'";
            ExecuteNonQuery(query);      
        }

        /// <summary>
        /// Removes an existing user.
        /// </summary>
        /// <param name="role">The user name to remove.</param>
        public void DropUser(string user){
            if(string.IsNullOrEmpty(user)) throw new ArgumentNullException("role");            
            DropUserMembership(user);            
            ExecuteNonQuery($"DROP USER {user};");  
        }                

        /// <summary>
        /// Looks if a user exists.
        /// </summary>
        /// <param name="user">The user name to find.</param>
        /// <returns>True if the role exists.</return>
        public bool ExistsUser(string user){
            foreach(DataRow dr in GetUsers().Tables[0].Rows)
                if(dr["username"].ToString().Equals(user)) return true;

            return false;
        }

        /// <summary>
        /// Drops the user from its current memberhips
        /// </summary>
        /// <param name="user">The user to clean.</param>
        public void DropUserMembership(string user){
            if(!ExistsUser(user)) return;
            DropMembership(user);       
        }

#endregion
#region "Roles"
        /// <summary>
        /// Requests for all the roles created.
        /// </summary>
        /// <returns>A dataset containing the requested data ('rolname').</returns>
        public DataSet GetRoles(){
            return ExecuteQuery(@"SELECT rolname FROM pg_catalog.pg_roles b WHERE rolcanlogin = false ORDER BY 1;");
        }

        /// <summary>
        /// Counts how many roles are in the database.
        /// </summary>
        /// <returns>A dataset containing the requested data.</returns>
        public long CountRoles(){
            return ExecuteScalar<long>(@"SELECT COUNT (*) FROM pg_catalog.pg_roles b WHERE rolcanlogin = false;");
        }

        /// <summary>
        /// Creates a new role.
        /// </summary>
        /// <param name="role">The role name to create.</param>
        public void CreateRole(string role){
            if(string.IsNullOrEmpty(role)) throw new ArgumentNullException("role");            
            ExecuteNonQuery($"CREATE ROLE {role};");
        }

        /// <summary>
        /// Removes an existing role.
        /// </summary>
        /// <param name="role">The role name to remove.</param>
        public void DropRole(string role){
            if(string.IsNullOrEmpty(role)) throw new ArgumentNullException("role");            
            DropRoleMembership(role);            
            ExecuteNonQuery($"DROP ROLE {role};");            
        }

        /// <summary>
        /// Looks if a role exists.
        /// </summary>
        /// <param name="role">The role name to find.</param>
        /// <returns>True if the role exists.</return>
        public bool ExistsRole(string role){
            foreach(DataRow dr in GetRoles().Tables[0].Rows)
                if(dr["rolname"].ToString().Equals(role)) return true;

            return false;
        }

        /// <summary>
        /// Drops the role from its current memberhips
        /// </summary>
        /// <param name="role">The rol to clean.</param>
        public void DropRoleMembership(string role){
            if(!ExistsRole(role)) return;
            DropMembership(role);       
        }
#endregion
#region "Permissions"         
        /// <summary>
        /// Get a list of groups and roles where the given item (user, role or group) belongs.
        /// </summary>
        /// <param name="item">The role to check.</param>
        /// <returns>A set of groups and roles where the given item (user, role or group) belongs ordered by name.</returns>
        public string[] GetMembership(string item){
            if(string.IsNullOrEmpty(item)) throw new ArgumentNullException("item");
            var ds = ExecuteQuery($"SELECT c.rolname AS rolname, b.rolname AS memberOf FROM pg_catalog.pg_auth_members m JOIN pg_catalog.pg_roles b ON (m.roleid = b.oid) JOIN pg_catalog.pg_roles c ON (c.oid = m.member) WHERE c.rolname='{item}' ORDER BY b.rolname ASC");
            
            var memberOf = new List<string>();
            foreach(DataRow dr in ds.Tables[0].Rows)
                memberOf.Add(dr["memberOf"].ToString());

            return memberOf.ToArray();
        }  
        
        /// <summary>
        /// Returns the table privileges.
        /// </summary>
        /// <param name="schema">The schema where to lookup for the table.</param>
        /// <param name="table">The table which permissions will be requested.</param>
        /// <param name="role">The role which privileges will be checked.</param>        
        /// <returns>The table privileges as ACL (https://www.postgresql.org/docs/9.3/sql-grant.html).</returns>
        public string GetTablePrivileges(string schema, string table, string role){
            if(schema == null) throw new ArgumentNullException("schema");
            if(table == null) throw new ArgumentNullException("table");
            if(role == null) throw new ArgumentNullException("role");

            string query = $"SELECT grantee, privilege_type AS privilege FROM information_schema.role_table_grants WHERE table_schema='{schema}' AND table_name='{table}' AND grantee='{role}'";
            string currentPrivileges = "";
            
            foreach(DataRow dr in ExecuteQuery(query).Tables[0].Rows){
                //ACL letters: https://www.postgresql.org/docs/9.3/sql-grant.html           
                if(dr["grantee"].ToString().Equals(role, StringComparison.CurrentCultureIgnoreCase)){                            
                    if(dr["privilege"].ToString().Equals("SELECT")) currentPrivileges = currentPrivileges + "r";
                    if(dr["privilege"].ToString().Equals("UPDATE")) currentPrivileges = currentPrivileges + "w";
                    if(dr["privilege"].ToString().Equals("INSERT")) currentPrivileges = currentPrivileges + "a";
                    if(dr["privilege"].ToString().Equals("DELETE")) currentPrivileges = currentPrivileges + "d";
                    if(dr["privilege"].ToString().Equals("TRUNCATE")) currentPrivileges = currentPrivileges + "D";
                    if(dr["privilege"].ToString().Equals("REFERENCES")) currentPrivileges = currentPrivileges + "x";
                    if(dr["privilege"].ToString().Equals("TRIGGER")) currentPrivileges = currentPrivileges + "t";                        
                }                    
            }

            return currentPrivileges;              
        }       
                         
        /// <summary>
        /// Returns the schema privileges.
        /// </summary>
        /// <param name="role">The role which privileges will be checked.</param>
        /// <param name="schema">The schema containing the table to check.</param>
        /// <returns>The schema privileges as ACL (https://www.postgresql.org/docs/9.3/sql-grant.html).</returns>
        public string GetSchemaPrivileges(string schema, string role = null){
            if(string.IsNullOrEmpty(schema)) throw new ArgumentNullException("schema");

            string query = $"SELECT r.rolname as grantee, pg_catalog.has_schema_privilege(r.rolname, nspname, 'CREATE') as create, pg_catalog.has_schema_privilege(r.rolname, nspname, 'USAGE') as usage FROM pg_namespace pn,pg_catalog.pg_roles r WHERE array_to_string(nspacl,',') like '%'||r.rolname||'%' AND nspowner > 1 AND nspname='{schema}'";
            if(!string.IsNullOrEmpty(role)) query += $" AND r.rolname='{role}'";

            string currentPrivileges = "";
            foreach(DataRow dr in ExecuteQuery(query).Tables[0].Rows){               
                //ACL letters: https://www.postgresql.org/docs/9.3/sql-grant.html
                if((bool)dr["usage"]) currentPrivileges += "U";
                if((bool)dr["create"]) currentPrivileges += "C";                                
            }
            
            return currentPrivileges;
        }       
#endregion
#region "Utils"               
        /// <summary>
        /// Given a view, executes its select query and compares the result with the given select query (does not compare the view definiton).
        /// </summary>
        /// <param name="schema">The schema containing the requested table..</param>
        /// <param name="view">The view which data will be requested.</param>
        /// <param name="query">The SQL select query which result should produce the same result as the view.</param>        
        /// <returns>True if matches.</returns>
        public bool CompareSelectWithView(string schema, string view, string query){
            return CompareSelects(query, GetViewDefinition(schema, view));
        }

        /// <summary>
        /// Compares two select queries, executing them and comparing the exact amount of rows and its data (doesn't compare the column names).
        /// </summary>
        /// <param name="expected">The left-side select query.</param>
        /// <param name="compared">The right-side select query.</param>
        /// <returns>True if both select queries are equivalent (returns exactly the same rows).</returns>
        public bool CompareSelects(string expected, string compared){
            if(string.IsNullOrEmpty(expected)) throw new ArgumentNullException("expected");
            if(string.IsNullOrEmpty(compared)) throw new ArgumentNullException("compared");

            expected = CleanSqlQuery(expected);
            compared = CleanSqlQuery(compared);

            return (0 == ExecuteScalar<long>($@"
                SELECT COUNT(*) FROM (
                    (({expected}) EXCEPT ({compared}))
                    UNION
                    (({compared}) EXCEPT ({expected}))
                ) AS result;"));
        }                                     
        
        /// <summary>
        /// Given a view, return its definition as a select query.
        /// </summary>
        /// <param name="schema">The schema containing the requested table..</param>
        /// <param name="view">The view which data will be requested.</param>
        /// <returns>The view definition as an SQL SELECT query.</returns>
        public string GetViewDefinition(string schema, string view){
            if(string.IsNullOrEmpty(schema)) throw new ArgumentNullException("schema");
            if(string.IsNullOrEmpty(view)) throw new ArgumentNullException("view");

            return ExecuteScalar<string>($"SELECT view_definition FROM information_schema.views WHERE table_schema='{schema}' AND table_name='{view}'");
        }        

        /// <summary>
        /// Returns the information about all the foreign keys defined over a table.
        /// </summary>
        /// <param name="schema">The schema containing the requested table..</param>
        /// <param name="table">The view which data will be requested.</param>
        /// <returns>The view definition as an SQL SELECT query.</returns>
        public DataSet GetForeignKeys(string schema, string table){
            if(string.IsNullOrEmpty(schema)) throw new ArgumentNullException("schema");
            if(string.IsNullOrEmpty(table)) throw new ArgumentNullException("table");

            return ExecuteQuery($@" SELECT tc.constraint_name AS name, tc.table_schema AS schemaFrom, tc.table_name AS tableFrom, kcu.column_name AS columnFrom, ccu.table_schema AS schemaTo, ccu.table_name AS tableTo, ccu.column_name AS columnTo
                                    FROM information_schema.table_constraints AS tc 
                                        JOIN information_schema.key_column_usage AS kcu ON tc.constraint_name = kcu.constraint_name
                                        JOIN information_schema.constraint_column_usage AS ccu ON ccu.constraint_name = tc.constraint_name
                                    WHERE constraint_type = 'FOREIGN KEY' AND tc.table_schema='{schema}' AND tc.table_name='{table}'");
        }

        /// <summary>
        /// Determines if a table's columns has been stablished as foreign key to another table's column.
        /// </summary>
        /// <param name="schemaFrom">Foreign key's origin schema.</param>
        /// <param name="tableFrom">Foreign key's origin table.</param>
        /// <param name="columnFrom">Foreign key's origin column.</param>
        /// <param name="schemaTo">Foreign key's destination schema.</param>
        /// <param name="tableTo">Foreign key's destination table.</param>
        /// <param name="columnTo">Foreign key's destination schema.</param>
        /// <returns>True if found.</returns>
        public bool ExistsForeignKey(string schemaFrom, string tableFrom, string columnFrom, string schemaTo, string tableTo, string columnTo){    
            foreach(DataRow dr in GetForeignKeys(schemaFrom, tableFrom).Tables[0].Rows){  
                if( dr["columnFrom"].ToString().Equals(columnFrom) && 
                    dr["schemaTo"].ToString().Equals(schemaTo) && 
                    dr["tableTo"].ToString().Equals(tableTo) && 
                    dr["columnTo"].ToString().Equals(columnTo)
                ) return true;
            }
                              
            return false;
        }

        /// <summary>
        /// Checks if the table or view exists.
        /// </summary>
        /// <param name="schema">The schema that contains the table.</param>
        /// <param name="table">The wanted table.</param>
        /// <returns>True if the table exists.</returns>
        public bool ExistsTable(string schema, string table)
        {                        
            return ExecuteScalar<bool>($"SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_schema='{schema}' AND  table_name='{table}');");            
        }        
#endregion
#region "Private"   
        /// <summary>
        /// Drops the item (user, role or group) from its current memberhips
        /// </summary>
        /// <param name="item">The role, group or user to clean.</param>
        private void DropMembership(string item){
            if(string.IsNullOrEmpty(item)) throw new ArgumentNullException("item");

            ExecuteNonQuery($"REASSIGN OWNED BY {item} TO postgres;");
            ExecuteNonQuery($"DROP OWNED BY {item}");

            foreach(var member in GetMembership(item))
                ExecuteNonQuery($"REVOKE {member} FROM {item}");           
        }

        private string CleanSqlQuery(string sql){
            sql = sql.Replace("\r\n", "").Replace("\n", "");            
            do sql = sql.Replace("  ", " ").Trim();
            while(sql.Contains("  "));

            return sql.TrimEnd(';');
        }       
               
        private static string ParseItemForSql(object item, bool like = false){
            if(item == null) return "NULL";
            else{            
                bool quotes = (item.GetType() == typeof(string) && (string.IsNullOrEmpty(item.ToString()) || item.ToString().Substring(0, 1) != "@"));                
                return (quotes ? string.Format("'{1}{0}{1}'", item, (like ? "%" : "")) : item.ToString().TrimStart('@'));
            }
        }         
        
        private string GetConnectionString(string host, string database, string username, string password){
            return $"Server={host};User Id={username};Password={password};Database={database};";
        }     
#endregion
    }
}