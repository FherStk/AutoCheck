/*
    Copyright © 2020 Fernando Porrino Serrano
    Third party software licenses can be found at /docs/credits/thirdparties.md

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
using System.ComponentModel;
using System.Collections.Generic;
using Npgsql;
using ToolBox.Bridge;
using AutoCheck.Core;
using AutoCheck.Exceptions;


namespace AutoCheck.Connectors{  
    //TODO: look for an abstraction layer like EF but dynamic (with no need of building the schema manually). This should replace all the custom methods created here (limited and quickly developed for a hurry need).
    
    /// <summary>
    /// Allows in/out operations and/or data validations with a PostgreSQL instance.
    /// </summary>     
    public class Postgres: Core.Connector{     
#region "Auxiliar Classes"        
        /// <summary>
        /// Allows the source selection for an SQL operation.
        /// </summary>
        public class Source{     
            /// <summary>
            /// The source schema.
            /// </summary>
            /// <value></value>       
            public string Schema {get; set;}

            /// <summary>
            /// The source table
            /// </summary>
            /// <value></value>
            public string Table {get; set;}

            /// <summary>
            /// Creates a new instance.
            /// </summary>
            /// <param name="schema">The source schema.</param>
            /// <param name="table">The source table</param>
            public Source(string schema, string table){
                this.Schema = schema;
                this.Table = table;
            }

            /// <summary>
            /// Creates a new instance.
            /// </summary>
            /// <param name="source">The source as 'schema.table'.</param>
            public Source(string source){
                if(string.IsNullOrEmpty(source)) throw new ArgumentNullException("source");
                if(!source.Contains(".")) throw new ArgumentInvalidException("The source argument must be an SQL source item like 'schema.table'.");

                var s = source.Split(".");
                this.Schema = s[0];
                this.Table = s[1];
            }

            /// <summary>
            /// Converts the current instance to an SQL compatible string representation
            /// </summary>
            /// <returns></returns>
            public override string ToString(){
                if(string.IsNullOrEmpty(Schema)) throw new ArgumentNullException("Source.Schema");
                if(string.IsNullOrEmpty(Table)) throw new ArgumentNullException("Source.Table");

                return string.Format("{0}.{1}", Schema, Table);
            }
        }

        /// <summary>
        /// Allows the destination selection for an SQL operation.
        /// </summary>
        public class Destination : Source{                 
            /// <summary>
            /// Creates a new instance.
            /// </summary>
            /// <param name="schema">The source schema.</param>
            /// <param name="table">The source table</param>
            public Destination(string schema, string table) : base(schema, table){                
            }
        }

        /// <summary>
        /// Allows filtering the data source over an SQL operation.
        /// </summary>
        public class Filter{
            /// <summary>
            /// The filed name which will be used for filtering.
            /// </summary>
            /// <value></value>
            public string Field {get; set;}
            
            /// <summary>
            /// The field value which will be used for filtering.
            /// </summary>
            /// <value></value>
            public object Value {get; set;}
            
            /// <summary>
            /// The operation between the field name and its value, which result will be used for filtering.
            /// </summary>
            /// <value></value>
            public Operator Operator {get; set;}

            /// <summary>
            /// Creates a new instance.
            /// </summary>
            /// <param name="field">The filed name which will be used for filtering.</param>
            /// <param name="op">The operation between the field name and its value, which result will be used for filtering.</param>
            /// <param name="value">The field value which will be used for filtering.</param>
            public Filter(string field, Operator op, object value){
                this.Field = field;
                this.Value = value;
                this.Operator = op;
            }

            public override string ToString(){
                if(Value == null) throw new ArgumentNullException("Filter.Value");
                if(Value.GetType() == typeof(string) && string.IsNullOrEmpty(Value.ToString())) throw new ArgumentNullException("Filter.Value");
                if(string.IsNullOrEmpty(Field)) throw new ArgumentNullException("Filter.Field");          

                var op = this.Operator switch  
                {  
                    Operator.LIKE => " LIKE ",  
                    Operator.LOWEREQUALS => " <= ",  
                    Operator.GREATEREQUALS => " >= ",  
                    Operator.NOTEQUALS => " != ",  
                    _ => ((char)this.Operator).ToString()
                }; 
                
                return string.Format("{0}{1}{2}", this.Field, op, ParseItemForSql(this.Value, this.Operator == Operator.LIKE));
            }               
        }        
#endregion
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
        
        /// <summary>
        /// The student name wich is the original database creator.
        /// </summary>
        /// <value></value>    
        public string Student{
            get{
                return Core.Utils.DataBaseNameToStudentName(this.Database);
            }
        }          
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
                throw new QueryInvalidException(string.Format("Unable to run te given SQL query '{0}'. Please, check the inner exception for further details.", query), ex);
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
                throw new QueryInvalidException(string.Format("Unable to run te given SQL query '{0}'. Please, check the inner exception for further details.", query), ex);
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
                throw new QueryInvalidException(string.Format("Unable to run te given SQL query '{0}'. Please, check the inner exception for further details.", query), ex);
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
                this.Conn.Open();
                return true;
            }   
            catch(Exception e){                    
                if(e.Message.StartsWith("3D000")) return false;
                else throw e;
            } 
            finally{
                this.Conn.Close();
            }
        }             

        /// <summary>
        /// Creates a new database using a SQL Dump file.
        /// </summary>
        /// <param name="filePath">The SQL Dump file path.</param>
        /// <param name="binPath">The path to the bin folder [only needed for windows systems].</param>
        [Obsolete("This overload has been deprecated, use other overloads and set the binPath (if needed) using the constructor.")]
        public void CreateDataBase(string filePath, string binPath)
        { 
            if(string.IsNullOrEmpty(filePath)) throw new ArgumentNullException("filePath");
            CreateDataBase(binPath);
            ImportSqlFile(filePath);
        }

        /// <summary>
        /// Creates a new database instance using an SQL Dump file.
        /// </summary>
        /// <param name="filePath">The SQL Dump file path.</param>
        public void CreateDataBase(string filePath)
        { 
            if(string.IsNullOrEmpty(filePath)) throw new ArgumentNullException("filePath");
            CreateDataBase();
            ImportSqlFile(filePath);
        } 

        /// <summary>
        /// Creates a new and empty database.
        /// </summary>
        public void CreateDataBase()
        { 
            string cmdPassword = string.Format("PGPASSWORD={0}", this.Password);
            string cmdCreate = string.Format("createdb -h {0} -U {1} -T template0 {2}", this.Host, this.User, this.Database);
            Response resp = null;
            
            using(LocalShell ls = new LocalShell()){
                switch (CurrentOS)
                {
                    //Once path is ok on windows and unix the almost same code will be used.
                    case OS.WIN:
                        resp = ls.Shell.Term(string.Format("SET \"{0}\" && {1}", cmdPassword, cmdCreate), ToolBox.Bridge.Output.Hidden, this.BinPath);
                        if(resp.code > 0) throw new Exception(resp.stderr.Replace("\n", ""));                                                
                        break;

                    case OS.MAC:
                    case OS.GNU:
                        resp = ls.Shell.Term(string.Format("{0} {1}", cmdPassword, cmdCreate));
                        if(resp.code > 0) throw new Exception(resp.stderr.Replace("\n", ""));
                        break;
                } 
            }
        }

        /// <summary>
        /// Imports an SQL into the current database.
        /// </summary>
        /// <param name="filePath"></param>
        public void ImportSqlFile(string filePath)
        { 
            if(string.IsNullOrEmpty(filePath)) throw new ArgumentNullException("filePath");
            if(!File.Exists(filePath)) throw new FileNotFoundException("filePath");
            
            string cmdPassword = string.Format("PGPASSWORD={0}", this.Password);
            string cmdRestore = string.Format("psql -h {0} -U {1} {2} < \"{3}\"", this.Host, this.User, this.Database, filePath);            
            Response resp = null;
            
            using(LocalShell ls = new LocalShell()){
                switch (CurrentOS)
                {
                    //Once path is ok on windows and unix the almost same code will be used.
                    case OS.WIN:
                        resp = ls.Shell.Term(string.Format("SET \"{0}\" && {1}", cmdPassword, cmdRestore), ToolBox.Bridge.Output.Hidden, this.BinPath);
                        if(resp.code > 0) throw new Exception(resp.stderr.Replace("\n", ""));
                        
                        break;

                    case OS.MAC:
                    case OS.GNU:
                        resp = ls.Shell.Term(string.Format("{0} {1}", cmdPassword, cmdRestore.Replace("\"", "'")));
                        if(resp.code > 0) throw new Exception(resp.stderr.Replace("\n", ""));
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
                ExecuteNonQuery(string.Format("SELECT pg_terminate_backend(pg_stat_activity.pid) FROM pg_stat_activity WHERE pg_stat_activity.datname = '{0}' AND pid <> pg_backend_pid();", this.Database));                
                      
                //Step 2: drop the database
                string cmdPassword = string.Format("PGPASSWORD={0}", this.Password);
                string cmdDrop = string.Format("dropdb -h {0} -U {1} {2}", this.Host, this.User, this.Database);         
                Response resp = null;
                
                using(LocalShell ls = new LocalShell()){
                    switch (CurrentOS)
                    {
                        //Once path is ok on windows and unix the almost same code will be used.
                        case OS.WIN:                  
                            resp = ls.Shell.Term(string.Format("SET \"{0}\" && {1}", cmdPassword, cmdDrop), ToolBox.Bridge.Output.Hidden, this.BinPath);
                            if(resp.code > 0) throw new Exception(resp.stderr.Replace("\n", ""));                    
                            break;

                        case OS.MAC:
                        case OS.GNU:
                            resp = ls.Shell.Term(string.Format("{0} {1}", cmdPassword, cmdDrop));
                            if(resp.code > 0) throw new Exception(resp.stderr.Replace("\n", ""));
                            break;
                    }   
                }
            }   
            finally{
                this.Conn = new NpgsqlConnection(GetConnectionString(this.Host, this.Database, this.User, this.Password));

                /*
                //TODO: TEST this, has no sense with the new changes
                //Step 3: restore the original connection (must be open, otherwise the first query will be aborted... why?).
                this.Conn = new NpgsqlConnection(GetConnectionString(this.DBHost, this.DBName, this.DBUser, this.DBPassword));
                this.Conn.Open();
                */
            }  
        } 

        //TODO: create table
        //TODO: create schema
#endregion
#region "SELECT and COUNT"
        //INFO: The Source, Destination and Filter methods has been created in order to avoid conflicts between overloads with the same signature.
        //      By using this new types more flexibility is allowed (but complexity also...), the method with only string arguments is the "advanced one" and
        //      the rest are the "assisted ones". Just one "advanced" is created to decrease the complexity (nulls are accepted in this one).

        /// <summary>
        /// Selects some data from the database.
        /// </summary>
        /// <param name="source">The unique schema and table from which the data will be loaded.</param>
        /// <param name="field">The field's data to load (a single one, or comma-separated set).</param>
        /// <returns>A dataset containing the requested data.</returns>
        /// <remarks>Use the overload with only string parameters for complex queries.</remarks>
        public DataSet Select(Source source, string field = "*"){
            if(source == null) throw new ArgumentNullException("source");            
            return Select(source.ToString(), string.Empty, (string.IsNullOrEmpty(field) ? null : new string[]{field}));
        }
        
        /// <summary>
        /// Selects some data from the database.
        /// </summary>
        /// <param name="source">The unique schema and table from which the data will be loaded.</param>
        /// <param name="fields">The set of field's data to load.</param>
        /// <returns>A dataset containing the requested data.</returns>
        /// <remarks>Use the overload with only string parameters for complex queries.</remarks>
        public DataSet Select(Source source, string[] fields){
            if(source == null) throw new ArgumentNullException("source");            
            return Select(source.ToString(), string.Empty, fields);
        }                       
        
        /// <summary>
        /// Selects some data from the database.
        /// </summary>
        /// <param name="source">The unique schema and table from which the data will be loaded.</param>
        /// <param name="filter">A filter over a single field which will be used to screen the data, subqueries are allowed but must start with '@' and surrounded by parenthesis like '@(SELECT MAX(id)+1 FROM t)'.</param>
        /// <param name="field">The field's data to load (a single one, or comma-separated set).</param>
        /// <returns>A dataset containing the requested data.</returns>   
        /// <remarks>Use the overload with only string parameters for complex queries.</remarks>
        public DataSet Select(Source source, Filter filter, string field = "*"){
            if(source == null) throw new ArgumentNullException("source");            
            if(filter == null) throw new ArgumentNullException("filter");            

            return Select(source.ToString(), filter.ToString(), (string.IsNullOrEmpty(field) ? null : new string[]{field}));
        }
        
        /// <summary>
        /// Selects some data from the database.
        /// </summary>
        /// <param name="source">The unique schema and table from which the data will be loaded.</param>
        /// <param name="filter">A filter over a single field which will be used to screen the data, subqueries are allowed but must start with '@' and surrounded by parenthesis like '@(SELECT MAX(id)+1 FROM t)'.</param>
        /// <param name="fields">The set of field's data to load.</param>
        /// <returns>A dataset containing the requested data.</returns>
        /// <remarks>Use the overload with only string parameters for complex queries.</remarks>
        public DataSet Select(Source source, Filter filter, string[] fields){
            if(source == null) throw new ArgumentNullException("source");          
            if(filter == null) throw new ArgumentNullException("filter");

            return Select(source.ToString(), filter.ToString(), fields);
        } 
        
        /// <summary>
        /// Selects some data from the database.
        /// </summary>
        /// <param name="source">The set of schemas and tables from which the data will be loaded, should be an SQL FROM sentence (without FROM) allowing joins and alisases.</param>
        /// <param name="filter">The set of filters which will be used to screen the data, should be an SQL WHERE sentence (without WHERE).</param>
        /// <param name="field">The field's data to load (a single one, or comma-separated set).</param>
        /// <returns>A dataset containing the requested data.</returns> 
        public DataSet Select(string source, string filter, string field = "*"){
            //TODO: allow ordering
            return Select(source, filter, (string.IsNullOrEmpty(field) ? null : new string[]{field}));
        }  
        
        /// <summary>
        /// Selects some data from the database.
        /// </summary>
        /// <param name="source">The set of schemas and tables from which the data will be loaded, should be an SQL FROM sentence (without FROM) allowing joins and alisases.</param>
        /// <param name="filter">The set of filters which will be used to screen the data, should be an SQL WHERE sentence (without WHERE).</param>
        /// <param name="fields">The set of field's data to load.</param>
        /// <returns>A dataset containing the requested data.</returns>  
        public DataSet Select(string source, string filter, string[] fields){
            //TODO: allow ordering
            if(string.IsNullOrEmpty(source)) throw new ArgumentNullException("source");

            string columns = (fields == null || fields.Length == 0 ? "*" : string.Join(",", fields));
            string query = string.Format("SELECT {0} FROM {1}", columns, source);
            if(!string.IsNullOrEmpty(filter)) query += string.Format(" WHERE {0}", filter);
            return ExecuteQuery(query);    
        }        
        
        /// <summary>
        /// Returns the requested field's value for the first found item.
        /// </summary>
        /// <param name="source">The unique schema and table from which the data will be loaded.</param>
        /// <param name="field">The wanted field's name.</param>
        /// <param name="sort">Defines how to order the list, so the max value will be returned when "descending" and min value when "ascending"..</param>
        /// <returns>The item's field value, NULL if not found.</returns>
        /// <remarks>Use the overload with only string parameters for complex queries.</remarks>
        public T GetField<T>(Source source, string field, ListSortDirection sort = ListSortDirection.Descending){
            if(source == null) throw new ArgumentNullException("source");          
            return GetField<T>(source.ToString(), string.Empty, field, sort);
        }        
        
        /// <summary>
        /// Returns the requested field's value for the first found item.
        /// </summary>
        /// <param name="source">The unique schema and table from which the data will be loaded.</param>
        /// <param name="field">The wanted field's name.</param>
        /// <param name="filter">A filter over a single field which will be used to screen the data, subqueries are allowed but must start with '@' and surrounded by parenthesis like '@(SELECT MAX(id)+1 FROM t)'.</param>
        /// <param name="sort">Defines how to order the list, so the max value will be returned when "descending" and min value when "ascending"..</param>
        /// <returns>The item's field value, NULL if not found.</returns>
        /// <remarks>Use the overload with only string parameters for complex queries.</remarks>
        public T GetField<T>(Source source, Filter filter, string field, ListSortDirection sort = ListSortDirection.Descending){
            if(source == null) throw new ArgumentNullException("source");          
            if(filter == null) throw new ArgumentNullException("filter");
            return GetField<T>(source.ToString(), filter.ToString(), field, sort);
        }

        /// <summary>
        /// Returns the requested field's value for the first found item.
        /// </summary>
        /// <param name="source">The set of schemas and tables from which the data will be loaded, should be an SQL FROM sentence (without FROM) allowing joins and alisases.</param>
        /// <param name="field">The wanted field's name.</param>
        /// <param name="filter">The set of filters which will be used to screen the data, should be an SQL WHERE sentence (without WHERE).</param>
        /// <param name="sort">Defines how to order the list, so the max value will be returned when "descending" and min value when "ascending"..</param>
        /// <returns>The item's field value, NULL if not found.</returns>
        public T GetField<T>(string source, string filter, string field, ListSortDirection sort = ListSortDirection.Descending){
            //TODO: sort must be a string too in this method to allow complex orders
            if(string.IsNullOrEmpty(source)) throw new ArgumentNullException("source");
            if(string.IsNullOrEmpty(field)) throw new ArgumentNullException("field");
            if(!string.IsNullOrEmpty(filter)) filter = string.Format("WHERE {0}", filter);
            
            return ExecuteScalar<T>((string.Format("SELECT {0} FROM {1} {2} ORDER BY {0} {3} LIMIT 1;", field, source, filter, (sort == ListSortDirection.Descending ? "DESC" : "ASC"))));           
        }
        
        /// <summary>
        /// Counts how many registers appears in a table.
        /// </summary>
        /// <param name="source">The unique schema and table from which the data will be loaded.</param>
        /// <returns>Amount of registers found.</returns>
        public long CountRegisters(Source source){
            if(source == null) throw new ArgumentNullException("source");          
            return CountRegisters(source.ToString(), string.Empty);
        }
        
        /// <summary>
        /// Counts how many registers appears in a table.
        /// </summary>
        /// <param name="source">The unique schema and table from which the data will be loaded.</param>
        /// <param name="filter">A filter over a single field which will be used to screen the data, subqueries are allowed but must start with '@' and surrounded by parenthesis like '@(SELECT MAX(id)+1 FROM t)'.</param>
        /// <returns>Amount of registers found.</returns>
        public long CountRegisters(Source source, Filter filter){
            if(source == null) throw new ArgumentNullException("source");          
            if(filter == null) throw new ArgumentNullException("filter");
            return CountRegisters(source.ToString(), filter.ToString());
        }
        
        /// <summary>
        /// Counts how many registers appears in a table.
        /// </summary>
        /// <param name="source">The set of schemas and tables from which the data will be loaded, should be an SQL FROM sentence (without FROM) allowing joins and alisases.</param>
        /// <param name="filter">The set of filters which will be used to screen the data, should be an SQL WHERE sentence (without WHERE).</param>
        /// <returns>Amount of registers found.</returns>
        public long CountRegisters(string source, string filter = null){
            if(string.IsNullOrEmpty(source)) throw new ArgumentNullException("source");

            string query = string.Format("SELECT COUNT(*) FROM {0}", source);
            if(!string.IsNullOrEmpty(filter)) query += string.Format(" WHERE {0}", filter);
            return ExecuteScalar<long>(query);
        }

        //TODO: do not buld count tables and schemas, replace all this custom code with an existing library (look at the top of this file).
#endregion
#region "INSERT"
        /// <summary>
        /// Inserts new data into a table.
        /// </summary>
        /// <param name="destination">The unique schema and table where the data will be added.</param>
        /// <param name="primaryKey">This primary key field name.</param>
        /// <param name="fields">Key-value pairs of data [field, value], subqueries as values must start with @.</param>
        /// <returns>The primary key value.</rereturns>
        /// <remarks>Use the overload with only string parameters for complex queries.</remarks>
        public T Insert<T>(Destination destination, string primaryKey, Dictionary<string, object> fields){
            Insert(destination, fields);
            return GetField<T>(destination, primaryKey, ListSortDirection.Descending);
        }

        /// <summary>
        /// Inserts new data into a table.
        /// </summary>
        /// <param name="destination">The schema and table where the data will be added, should be an SQL INTO sentence (schema.table).</param>
        /// <param name="primaryKey">This primary key field name.</param>
        /// <param name="fields">Key-value pairs of data [field, value], subqueries as values must start with @.</param>
        /// <returns>The primary key value.</rereturns>
        public T Insert<T>(string destination, string primaryKey, Dictionary<string, object> fields){
            Insert(destination, fields);
            return GetField<T>(destination, string.Empty, primaryKey, ListSortDirection.Descending);
        }
        
        /// <summary>
        /// Inserts new data into a table.
        /// </summary>
        /// <param name="destination">The unique schema and table where the data will be added.</param>
        /// <param name="fields">Key-value pairs of data [field, value], subqueries as values must start with @.</param>
        /// <remarks>Use the overload with only string parameters for complex queries.</remarks>
        public void Insert(Destination destination, Dictionary<string, object> fields){
            if(destination == null) throw new ArgumentNullException("source"); 

            Insert(destination.ToString(), fields);
        }
        
        /// <summary>
        /// Inserts new data into a table.
        /// </summary>
        /// <param name="destination">The schema and table where the data will be added, should be an SQL INTO sentence (schema.table).</param>
        /// <param name="fields">Key-value pairs of data [field, value], subqueries are allowed but must start with '@' and surrounded by parenthesis like '@(SELECT MAX(id)+1 FROM t)'.</param>
        public void Insert(string destination, Dictionary<string, object> fields){
            if(string.IsNullOrEmpty(destination)) throw new ArgumentNullException("source");
            if(fields == null || fields.Count == 0) throw new ArgumentNullException("fields");

            string query = string.Format("INSERT INTO {0} ({1}) VALUES (", destination, string.Join(',', fields.Keys));
            foreach(string field in fields.Keys)
                query += string.Format("{0},", ParseItemForSql(fields[field]));
            
            query = string.Format("{0})", query.TrimEnd(','));
            ExecuteNonQuery(query);                   
        }
#endregion
#region "UPDATE"
        /// <summary>
        /// Updates some data from a table, the 'ExecuteNonQuery' method can be used for complex filters (and, or, etc.).
        /// </summary>
        /// <param name="destination">The unique schema and table where the data will be added.</param>
        /// <param name="fields">Key-value pairs of data [field, value], subqueries are allowed but must start with '@' and surrounded by parenthesis like '@(SELECT MAX(id)+1 FROM t)'.</param>
        /// <remarks>Use the overload with only string parameters for complex queries.</remarks>
        public void Update(Destination destination, Dictionary<string, object> fields){
            if(destination == null) throw new ArgumentNullException("destination"); 
            
            Update(destination.ToString(), string.Empty, string.Empty, fields);
        }
               
        /// <summary>
        /// Updates some data from a table, the 'ExecuteNonQuery' method can be used for complex filters (and, or, etc.).
        /// </summary>
        /// <param name="destination">The unique schema and table where the data will be added.</param>        
        /// <param name="filter">A filter over a single field which will be used to screen the data, subqueries are allowed but must start with '@' and surrounded by parenthesis like '@(SELECT MAX(id)+1 FROM t)'.</param>
        /// <param name="fields">Key-value pairs of data [field, value], subqueries are allowed but must start with '@' and surrounded by parenthesis like '@(SELECT MAX(id)+1 FROM t)'.</param>
        /// <remarks>Use the overload with only string parameters for complex queries.</remarks>
        public void Update(Destination destination, Filter filter, Dictionary<string, object> fields){
            if(destination == null) throw new ArgumentNullException("destination"); 
            if(filter == null) throw new ArgumentNullException("filter"); 

            Update(destination.ToString(), string.Empty, filter.ToString(), fields);
        }

        /// <summary>
        /// Updates some data from a table, the 'ExecuteNonQuery' method can be used for complex filters (and, or, etc.).
        /// </summary>
        /// <param name="destination">The unique schema and table where the data will be added.</param>
        /// <param name="source">The set of schemas and tables from which the data will be loaded, should be an SQL FROM sentence (without FROM) allowing joins and alisases.</param>        
        /// <param name="filter">A filter over a single field which will be used to screen the data, subqueries are allowed but must start with '@' and surrounded by parenthesis like '@(SELECT MAX(id)+1 FROM t)'.</param>
        /// <param name="fields">Key-value pairs of data [field, value], subqueries are allowed but must start with '@' and surrounded by parenthesis like '@(SELECT MAX(id)+1 FROM t)'.</param>
        /// <remarks>Use the overload with only string parameters for complex queries.</remarks>
        public void Update(Destination destination, Source source, Filter filter, Dictionary<string, object> fields){
            if(destination == null) throw new ArgumentNullException("destination"); 
            if(source == null) throw new ArgumentNullException("source"); 
            if(filter == null) throw new ArgumentNullException("filter"); 

            Update(destination.ToString(), source.ToString(), filter.ToString(), fields);
        }

        /// <summary>
        /// Updates some data from a table, the 'ExecuteNonQuery' method can be used for complex filters (and, or, etc.).
        /// </summary>
        /// <param name="destination">The unique schema and table where the data will be added.</param>
        /// <param name="source">The set of schemas and tables from which the data will be loaded, should be an SQL FROM sentence (without FROM) allowing joins and alisases.</param>        
        /// <param name="filter">The set of filters which will be used to screen the data, should be an SQL WHERE sentence (without WHERE).</param>
        /// <param name="fields">Key-value pairs of data [field, value], subqueries are allowed but must start with '@' and surrounded by parenthesis like '@(SELECT MAX(id)+1 FROM t)'.</param>
        public void Update(string destination, string source, string filter, Dictionary<string, object> fields){
            if(string.IsNullOrEmpty(destination)) throw new ArgumentNullException("destination");
            if(fields == null || fields.Count == 0) throw new ArgumentNullException("fields");

            string query = string.Format("UPDATE {0} SET", destination);
            foreach(string field in fields.Keys) 
                query += string.Format(" {0}={1},", field, ParseItemForSql(fields[field]));
            
            query = query.TrimEnd(',');

            if(!string.IsNullOrEmpty(source)) query += string.Format(" FROM {0}", source);
            if(!string.IsNullOrEmpty(filter)) query += string.Format(" WHERE {0};", filter);

            ExecuteNonQuery(query);
        }
#endregion
#region "DELETE"
        /// <summary>
        /// Deletes some data from a table, the 'ExecuteNonQuery' method can be used for complex filters (and, or, etc.).
        /// </summary>
        /// <param name="destination">The unique schema and table where the data will be added.</param>
        public void Delete(Destination destination){
            if(destination == null) throw new ArgumentNullException("destination"); 
            Delete(destination.ToString(), string.Empty, string.Empty);  
        }

        /// <summary>
        /// Deletes some data from a table, the 'ExecuteNonQuery' method can be used for complex filters (and, or, etc.).
        /// </summary>
        /// <param name="destination">The unique schema and table where the data will be added.</param>
        /// <param name="filter">A filter over a single field which will be used to screen the data, subqueries are allowed but must start with '@' and surrounded by parenthesis like '@(SELECT MAX(id)+1 FROM t)'.</param>
        public void Delete(Destination destination, Filter filter){
            if(destination == null) throw new ArgumentNullException("destination"); 
            if(filter == null) throw new ArgumentNullException("filter"); 

            Delete(destination.ToString(), string.Empty, filter.ToString());  
        }

        /// <summary>
        /// Deletes some data from a table, the 'ExecuteNonQuery' method can be used for complex filters (and, or, etc.).
        /// </summary>
        /// <param name="destination">The unique schema and table where the data will be added.</param>
        /// <param name="source">The set of schemas and tables from which the data will be loaded, should be an SQL FROM sentence (without FROM) allowing joins and alisases.</param>        
        /// <param name="filter">A filter over a single field which will be used to screen the data, subqueries are allowed but must start with '@' and surrounded by parenthesis like '@(SELECT MAX(id)+1 FROM t)'.</param>
        public void Delete(Destination destination, Source source, Filter filter){
            if(destination == null) throw new ArgumentNullException("destination"); 
            if(source == null) throw new ArgumentNullException("source"); 
            if(filter == null) throw new ArgumentNullException("filter"); 

            Delete(destination.ToString(), source.ToString(), filter.ToString());  
        } 

        /// <summary>
        /// Delete some data from a table, the 'ExecuteNonQuery' method can be used for complex filters (and, or, etc.).
        /// </summary>
        /// <param name="destination">The unique schema and table where the data will be added.</param>
        /// <param name="source">The set of schemas and tables from which the data will be loaded, should be an SQL FROM sentence (without FROM) allowing joins and alisases.</param>        
        /// <param name="filter">The set of filters which will be used to screen the data, should be an SQL WHERE sentence (without WHERE).</param>
        public void Delete(string destination, string source, string filter){
            //TODO: allow CASCADE option
            if(string.IsNullOrEmpty(destination)) throw new ArgumentNullException("destination");
            string query = string.Format("DELETE FROM {0}", destination);

            if(!string.IsNullOrEmpty(source)) query += string.Format(" USING {0}", source);
            if(!string.IsNullOrEmpty(filter)) query += string.Format(" WHERE {0};", filter);

            ExecuteNonQuery(query);            
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
            
            string query = string.Format("CREATE USER {0}", user);
            if(!string.IsNullOrEmpty(password)) query += string.Format(" WITH PASSWORD '{0}'", password);
            
            ExecuteNonQuery(query);      
        }

        /// <summary>
        /// Removes an existing user.
        /// </summary>
        /// <param name="role">The user name to remove.</param>
        public void DropUser(string user){
            if(string.IsNullOrEmpty(user)) throw new ArgumentNullException("role");            
            ExecuteNonQuery(string.Format("DROP USER {0}", user));      
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
            
            ExecuteNonQuery(string.Format("CREATE ROLE {0};", role));
        }

        /// <summary>
        /// Removes an existing role.
        /// </summary>
        /// <param name="role">The role name to remove.</param>
        public void DropRole(string role){
            if(string.IsNullOrEmpty(role)) throw new ArgumentNullException("role");
            
            ExecuteNonQuery(string.Format("DROP OWNED BY {0}", role));
            ExecuteNonQuery(string.Format("DROP ROLE {0};", role));
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
#endregion
#region "Permissions"
        /// <summary>
        /// Grants an item (role, group or permission) to a destination (role, group or user).
        /// </summary>
        /// <param name="what">What to grant (permission).</param>        
        /// <param name="where">Where to grant (table).</param>
        /// <param name="who">Who to grant (role, group or user).</param>
        public void Grant(string what, Destination where, string who){           
            if(where == null) throw new ArgumentNullException("where");                        
            Grant(what, where.ToString(), who);
        }

        /// <summary>
        /// Grants an item (role, group or permission) to a destination (role, group or user).
        /// </summary>
        /// <param name="what">What to grant (permission).</param>
        /// <param name="where">Where to grant (schema).</param>
        /// <param name="who">Who to grant (role, group or user).</param>        
        public void Grant(string what, string where, string who){
            if(string.IsNullOrEmpty(what)) throw new ArgumentNullException("what");
            if(string.IsNullOrEmpty(where)) throw new ArgumentNullException("where");
            if(string.IsNullOrEmpty(who)) throw new ArgumentNullException("who");            

            Grant(string.Format("{0} ON {1} {2}", what, (where.Contains(".") ? "TABLE" : "SCHEMA"), where), who);
        }

        /// <summary>
        /// Grants an item (role, group or permissio) to a destination (role, group or user).
        /// </summary>
        /// <param name="what">Who to grant (role, group or user) or the permission to grant as an SQL compatible statement like 'permission ON schema.table'.</param>
        /// <param name="where">Where to grant (destination: role, group or user).</param>
        public void Grant(string what, string where){
            if(string.IsNullOrEmpty(what)) throw new ArgumentNullException("item");
            if(string.IsNullOrEmpty(where)) throw new ArgumentNullException("destination");

            ExecuteNonQuery(string.Format("GRANT {0} TO {1};", what, where));
        }

        /// <summary>
        /// Revokes an item (role, group or permission) from a destination (role, group or user).
        /// </summary>
        /// <param name="what">What to revoke (permission).</param>
        /// <param name="where">Where to revoke (schema).</param>
        /// <param name="who">Who to revoke (role, group or user).</param>      
        public void Revoke(string what, Destination where, string who){           
            if(where == null) throw new ArgumentNullException("where");                        
            Revoke(what, where.ToString(), who);
        }

        /// <summary>
        /// Revokes an item (role, group or permission) from a destination (role, group or user).
        /// </summary>
        /// <param name="what">What to revoke (permission).</param>
        /// <param name="where">Where to revoke (schema).</param>
        /// <param name="who">Who to revoke (role, group or user).</param>        
        public void Revoke(string what, string where, string who){
            if(string.IsNullOrEmpty(what)) throw new ArgumentNullException("what");
            if(string.IsNullOrEmpty(where)) throw new ArgumentNullException("where");
            if(string.IsNullOrEmpty(who)) throw new ArgumentNullException("who");            

            Revoke(string.Format("{0} ON {1} {2}", what, (where.Contains(".") ? "TABLE" : "SCHEMA"), where), who);
        }

        /// <summary>
        /// Revokes an item (role, group or permissio) from a destination (role, group or user).
        /// </summary>
        /// <param name="what">Who to revoke (role, group or user) or the permission to revoke as an SQL compatible statement like 'permission FROM schema.table'.</param>
        /// <param name="where">Where to revoke (destination: role, group or user).</param>
        public void Revoke(string what, string where){
            if(string.IsNullOrEmpty(what)) throw new ArgumentNullException("item");
            if(string.IsNullOrEmpty(where)) throw new ArgumentNullException("destination");

            ExecuteNonQuery(string.Format("REVOKE {0} FROM {1};", what, where));
        }

        /// <summary>
        /// Get a list of groups and roles where the given item (user, role or group) belongs.
        /// </summary>
        /// <param name="item">The role to check.</param>
        /// <returns>The requested data (rolname, memberOf)</returns>
        public DataSet GetMembership(string item){
            if(string.IsNullOrEmpty(item)) throw new ArgumentNullException("item");
            return ExecuteQuery(string.Format("SELECT c.rolname AS rolname, b.rolname AS memberOf FROM pg_catalog.pg_auth_members m JOIN pg_catalog.pg_roles b ON (m.roleid = b.oid) JOIN pg_catalog.pg_roles c ON (c.oid = m.member) WHERE c.rolname='{0}'", item));            
        }  
        
        /// <summary>
        /// Returns the table privileges.
        /// </summary>
        /// <param name="role">The role which privileges will be checked.</param>
        /// <param name="source">The source which permissions will be requested.</param>
        /// <returns>The requested data (grantee, privilege).</returns>
        public DataSet GetTablePrivileges(Source source, string role = null){
            if(source == null) throw new ArgumentNullException("source");            
            source.ToString();  //throws an exception if a mandatory source argument is null or empty.

            string query = string.Format("SELECT grantee, privilege_type AS privilege FROM information_schema.role_table_grants WHERE table_schema='{0}' AND table_name='{1}'", source.Schema, source.Table);
            if(!string.IsNullOrEmpty(role)) query += string.Format(" AND grantee='{0}'", role);

            return ExecuteQuery(query);            
        }

        /// <summary>
        /// Returns the table privileges.
        /// </summary>
        /// <param name="role">The role which privileges will be checked.</param>
        /// <param name="source">The table which permissions will be requested as 'schema.table'.</param>
        /// <returns>The table privileges.</returns>
        public DataSet GetTablePrivileges(string source, string role = null){            
            return GetTablePrivileges(new Source(source, role));
        }  
        
        /// <summary>
        /// Returns the schema privileges.
        /// </summary>
        /// <param name="role">The role which privileges will be checked.</param>
        /// <param name="schema">The schema containing the table to check.</param>
        /// <returns>The requested data (grantee, privilege).</returns>
        public DataSet GetSchemaPrivileges(string schema, string role = null){
            if(string.IsNullOrEmpty(schema)) throw new ArgumentNullException("schema");

            string query = string.Format("SELECT r.rolname as grantee, pg_catalog.has_schema_privilege(r.rolname, nspname, 'CREATE') as create, pg_catalog.has_schema_privilege(r.rolname, nspname, 'USAGE') as usage FROM pg_namespace pn,pg_catalog.pg_roles r WHERE array_to_string(nspacl,',') like '%'||r.rolname||'%' AND nspowner > 1 AND nspname='{0}'", schema);
            if(!string.IsNullOrEmpty(role)) query += string.Format(" AND r.rolname='{0}'", role);

            return ExecuteQuery(query);            
        }       
#endregion
#region "Utils"        
        /// <summary>
        /// Compares two select queries, executing them and comparing the exact amount of rows and its data (doesn't compare the column names).
        /// </summary>
        /// <param name="expected">The left-side select query.</param>
        /// <param name="compared">The right-side select query.</param>
        /// <returns>True if both select queries are equivalent (returns exactly the same rows).</returns>
        public bool CompareSelects(string expected, string compared){
            if(string.IsNullOrEmpty(expected)) throw new ArgumentNullException("expected");
            if(string.IsNullOrEmpty(compared)) throw new ArgumentNullException("compared");

            return (0 == ExecuteScalar<long>(string.Format(@"
                SELECT COUNT(*) FROM (
                    ({0}) EXCEPT ({1})
                    UNION
                    ({1}) EXCEPT ({0})
                ) AS result;", CleanSqlQuery(expected), CleanSqlQuery(compared))));
        }                  

        /// <summary>
        /// Given a view, return its definition as a select query.
        /// </summary>
        /// <param name="source">The unique schema and table from which the data will be loaded, as 'schema.table'.</param>
        /// <returns>The view definition as an SQL SELECT query.</returns>
        public string GetViewDefinition(string source){
            return GetViewDefinition(new Source(source));
        }        
        
        /// <summary>
        /// Given a view, return its definition as a select query.
        /// </summary>
        /// <param name="source">The unique schema and table from which the data will be loaded.</param>
        /// <returns>The view definition as an SQL SELECT query.</returns>
        public string GetViewDefinition(Source source){
            if(source == null) throw new ArgumentNullException("source");
            source.ToString();  //throws an exception if the mandatory arguments are empty.

            return ExecuteScalar<string>(string.Format("SELECT view_definition FROM information_schema.views WHERE table_schema='{0}' AND table_name='{1}'", source.Schema, source.Table));
        }

        /// <summary>
        /// Returns the information about all the foreign keys defined over a table.
        /// </summary>
        /// <param name="source">The unique schema and table from which the foreign keys will be loaded, as 'schema.table'.</param>
        /// <returns>The foreign keys data.</returns>
        public DataSet GetForeignKeys(string source){
            return GetForeignKeys(new Source(source));
        }

        /// <summary>
        /// Returns the information about all the foreign keys defined over a table.
        /// </summary>
        /// <param name="source">The unique schema and table from which the foreign keys will be loaded.</param>
        /// <returns>The view definition as an SQL SELECT query.</returns>
        public DataSet GetForeignKeys(Source source){
            if(source == null) throw new ArgumentNullException("source");
            source.ToString();  //throws an exception if the mandatory arguments are empty.

            return ExecuteQuery(string.Format(@"SELECT tc.constraint_name AS name, tc.table_schema AS schemaFrom, tc.table_name AS tableFrom, kcu.column_name AS columnFrom, ccu.table_schema AS schemaTo, ccu.table_name AS tableTo, ccu.column_name AS columnTo
                                                                            FROM information_schema.table_constraints AS tc 
                                                                            JOIN information_schema.key_column_usage AS kcu ON tc.constraint_name = kcu.constraint_name
                                                                            JOIN information_schema.constraint_column_usage AS ccu ON ccu.constraint_name = tc.constraint_name
                                                                            WHERE constraint_type = 'FOREIGN KEY' AND tc.table_schema='{0}' AND tc.table_name='{1}'", source.Schema, source.Table));            
        }        
#endregion
#region "Private"       
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
                return (quotes ? string.Format("'{1}{0}{1}'", item, (like ? "%" : "")) : string.Format("{0}", item.ToString().TrimStart('@')));
            }
        }         
        
        private string GetConnectionString(string host, string database, string username, string password){
            return string.Format("Server={0};User Id={1};Password={2};Database={3};", host, username, password, database);
        }     
#endregion
    }
}