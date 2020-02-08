using Npgsql;
using System;
using System.Data;
using ToolBox.Bridge;
using ToolBox.Platform;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator.Utils{        
    public partial class DataBase{        
        private Output Output {get; set;}
        public string DBAddress {get; private set;}
        public string DBName {get; private set;}
        public NpgsqlConnection Conn {get; private set;}
        public string Student{
            get{
                if(!this.DBName.Contains("_")) throw new Exception("The current database name does not follows the Moodle's batch download folders name convetion.");
                return this.DBName.Substring(this.DBName.IndexOf("_")+1).Replace("_", " ");
            }
        }

        public DataBase(string host, string database, string username, string password, Output output = null){
            this.Output = output;
            this.DBAddress = host;
            this.DBName = database;
            this.Conn = new NpgsqlConnection(string.Format("Server={0};User Id={1};Password={2};Database={3};", host, "postgres", "postgres", database));
        }         
        public void Dispose()
        {                        
            this.Conn.Dispose();            
        }   
#region Checkers        
        /// <summary>
        /// Compares a set of expected privileges with the current table's ones.
        /// </summary>
        /// <param name="role">The role which privileges will be checked.</param>
        /// <param name="schema">The schema containing the table to check.</param>
        /// <param name="table">The table which privileges will be checked against the role's ones.</param>
        /// <param name="expectedPrivileges">ACL letters as appears on PostgreSQL documentation: https://www.postgresql.org/docs/11/sql-grant.html</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfTableMatchesPrivileges(string role, string schema, string table, string expectedPrivileges){
            List<string> errors = new List<string>();                         
            
            try{
                if(Output != null) Output.Write(string.Format("Getting the permissions for the role '{0}' on table ~{1}.{2}... ", role, schema, table), ConsoleColor.Yellow);
                
                int count = 0;
                string currentPrivileges = "";

                foreach(DataRow dr in GetTablePrivileges(role, schema, table).Tables[0].Rows){                        
                    //ACL letters: https://www.postgresql.org/docs/9.3/sql-grant.html                            
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
                     
                    if(count == 0) errors.Add(string.Format("Unable to find any privileges for the table '{0}'", table));
                    else if(!currentPrivileges.Equals(expectedPrivileges)) errors.Add(string.Format("Privileges missmatch over the table '{0}.{1}': expected->'{2}' found->'{3}'.", schema, table, expectedPrivileges, currentPrivileges));
                }
            }
            catch(Exception e){
                errors.Add(e.Message);
            }            

            return errors;
        }     
        /// <summary>
        /// Looks for a privilege within the current table's ones.
        /// </summary>
        /// <param name="role">The role which privileges will be checked.</param>
        /// <param name="schema">The schema containing the table to check.</param>
        /// <param name="table">The table which privileges will be checked against the role's ones.</param>
        /// <param name="privilege">ACL letter as appears on PostgreSQL documentation: https://www.postgresql.org/docs/11/sql-grant.html</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfTableContainsPrivileges(string role, string schema, string table, char privilege){
            List<string> errors = new List<string>();                         
            
             try{
                if(Output != null) Output.Write(string.Format("Getting the permissions for the role '{0}' on table ~{1}.{2}... ", role, schema, table), ConsoleColor.Yellow);

                int count = 0;
                string currentPrivileges = "";

                foreach(DataRow dr in GetTablePrivileges(role, schema, table).Tables[0].Rows){
                    //ACL letters: https://www.postgresql.org/docs/9.3/sql-grant.html
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

                    if(count == 0) errors.Add(string.Format("Unable to find any privileges for the table '{0}'", table));
                    else if(!currentPrivileges.Contains(privilege)) errors.Add(string.Format("Unable to find the requested privilege '{0}' over the table '{1}': found->'{2}'.", privilege, string.Format("{0}.{1}", schema, table), currentPrivileges));  
                }                 
            }
            catch(Exception e){
                errors.Add(e.Message);
            }           

            return errors;
        } 
        /// <summary>
        /// Compares a set of expected privileges with the current schema's ones.
        /// </summary>
        /// <param name="role">The role which privileges will be checked.</param>
        /// <param name="schema">The schema containing the table to check.</param>
        /// <param name="expectedPrivileges">ACL letters as appears on PostgreSQL documentation: https://www.postgresql.org/docs/11/sql-grant.html</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfSchemaMatchesPrivileges(string role, string schema, string expectedPrivileges){
           List<string> errors = new List<string>();    

            try{                     
                if(Output != null) Output.Write(string.Format("Getting the permissions for the role '{0}' on schema ~{1}... ", role, schema), ConsoleColor.Yellow);                                
                string currentPrivileges = "";
                int count = 0;

                foreach(DataRow dr in GetSchemaPrivileges(role, schema).Tables[0].Rows){
                    count++;

                    //ACL letters: https://www.postgresql.org/docs/9.3/sql-grant.html
                    if((bool)dr["usage_grant"]) currentPrivileges += "U";
                    if((bool)dr["create_grant"]) currentPrivileges += "C";                                
                }
                
                if(count == 0) errors.Add(string.Format("Unable to find any privileges for the role '{0}' on schema '{1}'.", role, schema));
                if(!currentPrivileges.Equals(expectedPrivileges)) errors.Add(string.Format("Privileges missmatch over the schema '{0}': expected->'{1}' found->'{2}'.", schema, expectedPrivileges, currentPrivileges));                    
            }
            catch(Exception e){
                errors.Add(e.Message);
            }            

            return errors;
        } 
        /// <summary>
        /// Looks for a privilege within the current schema's ones.
        /// </summary>
        /// <param name="role">The role which privileges will be checked.</param>
        /// <param name="schema">The schema containing the table to check.</param>
        /// <param name="privilege">ACL letter as appears on PostgreSQL documentation: https://www.postgresql.org/docs/11/sql-grant.html</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfSchemaContainsPrivilege(string role, string schema, char privilege){
            List<string> errors = new List<string>();                         

            try{
                if(Output != null) Output.Write(string.Format("Getting the permissions for the role '{0}' on schema ~{1}... ", role, schema), ConsoleColor.Yellow);                 

                int count = 0;
                foreach(DataRow dr in GetSchemaPrivileges(role, schema).Tables[0].Rows){
                    count ++;
                    
                    //ACL letters: https://www.postgresql.org/docs/9.3/sql-grant.html
                    switch(privilege){
                        case 'U':
                        if(!(bool)dr["usage_grant"]) errors.Add(string.Format("Unable to find the USAGE privilege for the role '{0}' on schema '{1}'.", role, schema));
                        break;

                        case 'C':
                        if(!(bool)dr["create_grant"]) errors.Add(string.Format("Unable to find the CREATE privilege for the role '{0}' on schema '{1}'.", role, schema));
                        break;
                    }                        
                }                

                if(count == 0) errors.Add(string.Format("Unable to find any privileges for the role '{0}' on schema '{1}'.", role, schema));                
            }
            catch(Exception e){
                errors.Add(e.Message);
            }

            return errors;
        } 
        /// <summary>
        /// Checks if the given role is part of all the given groups.
        /// </summary>
        /// <param name="role">The role to check.</param>
        /// <param name="groups">The groups where the role should belong.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckRoleMembership(string role, string[] groups){
            List<string> errors = new List<string>();
            Dictionary<string, bool> matches = new Dictionary<string, bool>();

            foreach(string g in groups)
                matches.Add(g, false);

            try{
                if(Output != null) Output.Write(string.Format("Getting the membership for the role ~{0}... ", role), ConsoleColor.Yellow);
                foreach(DataRow dr in GetRoleMembership(role).Tables[0].Rows){
                    if(matches.ContainsKey(dr["memberOf"].ToString()))
                        matches[dr["memberOf"].ToString()] = true;
                }                                
               
                foreach(string g in groups){
                    if(!matches[g]) 
                        errors.Add(string.Format("The role '{0}' does not belongs to the group '{1}'.", role, g));
                }
            }
            catch(Exception e){
                errors.Add(e.Message);
            }           

            return errors;
        } 
        /// <summary>
        /// Checks if a table's columns has been stablished as foreign key to another table's column.
        /// </summary>
        /// <param name="schemaFrom">Foreign key's origin schema.</param>
        /// <param name="tableFrom">Foreign key's origin table.</param>
        /// <param name="columnFrom">Foreign key's origin column.</param>
        /// <param name="schemaTo">Foreign key's destination schema.</param>
        /// <param name="tableTo">Foreign key's destination table.</param>
        /// <param name="columnTo">Foreign key's destination schema.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckForeignKey(string schemaFrom, string tableFrom, string columnFrom, string schemaTo, string tableTo, string columnTo){    
            List<string> errors = new List<string>();                             

            try{
                if(Output != null) Output.Write(string.Format("Getting the foreign key for ~{0}.{1}.{2} -> {2}.{3}.{4}... ", schemaFrom,tableFrom, columnFrom, schemaTo, tableTo, columnTo), ConsoleColor.Yellow);            
                                
                int count = 0;
                bool found = false;                    

                foreach(DataRow dr in GetForeignKeys(schemaFrom, tableFrom).Tables[0].Rows){  
                    count++;               
                    if( dr["columnFrom"].ToString().Equals(columnFrom) && 
                        dr["schemaTo"].ToString().Equals(schemaTo) && 
                        dr["tableTo"].ToString().Equals(tableTo) && 
                        dr["columnTo"].ToString().Equals(columnTo)
                    ) found = true;                        
                }

                if(count == 0) errors.Add(string.Format("Unable to find any FOREIGN KEY for the table '{0}'", tableFrom));
                else if(!found) errors.Add(string.Format("Unable to find the FOREIGN KEY from '{0}' to '{1}'", string.Format("{0}.{1}", schemaFrom, tableFrom), string.Format("{0}.{1}", schemaTo, tableTo))); 
            }
            catch(Exception e){
                errors.Add(e.Message);
            }                     

            return errors;
        }  
        /// <summary>
        /// Checks if a new item has been added to a table, looking for a greater ID (pkField > lastPkValue).
        /// </summary>
        /// <param name="schema">The schema containing the table to check.</param>
        /// <param name="table">The table to check.</param>        
        /// <param name="pkField">The primary key field.</param>
        /// <param name="lastPkValue">The last primary key value, so the new element must have a higher one.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfEntryAdded(string schema, string table, string pkField, int lastPkValue){    
            List<string> errors = new List<string>();            
            
            try{
                if(Output != null) Output.Write(string.Format("Checking if a new item has been added to the table ~{0}.{1}... ", schema, table), ConsoleColor.Yellow);      
                long count = (long)CountRegisters(schema, table, pkField, lastPkValue, '>');
                if(count == 0) errors.Add(string.Format("Unable to find any new item on table '{0}.{1}'", schema, table));                
            }
            catch(Exception e){
                errors.Add(e.Message);
            }           

            return errors;
        }
        /// <summary>
        /// Checks if an item has been removed from a table, looking for its  ID (pkField = lastPkValue).
        /// </summary>
        /// <param name="schema">The schema containing the table to check.</param>
        /// <param name="table">The table to check.</param>        
        /// <param name="pkField">The primary key field.</param>
        /// <param name="lastPkValue">The primary key value, so the element must have been erased.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfEntryRemoved(string schema, string table, string pkField, int removedPkValue){    
            List<string> errors = new List<string>();            

            try{
                if(Output != null) Output.Write(string.Format("Checking if an item has been removed from the table ~{0}.{1}... ", schema, table), ConsoleColor.Yellow);
                long count = (long)CountRegisters(schema, table, pkField, removedPkValue);
                if(count > 0) errors.Add(string.Format("An existing item was find for the {0}={1} on table '{2}.{3}'", pkField, removedPkValue, schema, table));                               
            }
            catch(Exception e){
                errors.Add(e.Message);
            }            

            return errors;
        }
        /// <summary>
        /// Compares if the given entry data matches with the current one stored in the database.
        /// </summary>
        /// <param name="schema">The schema containing the table to check.</param>
        /// <param name="table">The table to check.</param>
        /// <param name="fields">A set of [field-name, field-value] pairs which will be used to check the entry data.</param>
        /// <param name="filterField">The field name which be used to find the registry.</param>
        /// <param name="filterValue">The field value which be used to find the registry.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfTableMatchesData(string schema, string table, Dictionary<string, object> fields, string filterField, object filterValue){    
            List<string> errors = new List<string>();            
            
            try{
                if(Output != null) Output.Write(string.Format("Checking the entry data for ~{0}={1}~ on ~{2}.{3}... ", filterField, filterValue, schema, table), ConsoleColor.Yellow);                                      
                return MatchesData(SelectData(schema, table, fields, filterField, filterValue), fields);                                     
            }
            catch(Exception e){
                errors.Add(e.Message);
            }

            return errors;
        }        
        /// <summary>
        /// Checks if a table or view exists.
        /// </summary>
        /// <param name="schema">The schema containing the table to check.</param>
        /// <param name="table">The table to check.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfTableExists(string schema, string table){    
            List<string> errors = new List<string>();                             

            try{                
                if(Output != null) Output.Write(string.Format("Checking the creation of the table ~{0}.{1}... ", schema, table), ConsoleColor.Yellow);
                //If not exists, an exception will be thrown                    
                CountRegisters(schema, table);                                                             
            }
            catch{
                errors.Add("The table does not exists.");
                return errors;
            }               

            return errors;
        }    
        /// <summary>
        /// Given a view, executes its select query and compares the result with the given definition.
        /// </summary>
        /// <param name="schema">The schema containing the table to check.</param>
        /// <param name="table">The table to check.</param>
        /// <param name="definition">The SQL select query which result should produce the same result as the view.</param>        
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfViewMatchesDefinition(string schema, string view, string definition){
           List<string> errors = new List<string>();            

            try{                
                if(Output != null) Output.Write(string.Format("Checking the SQL definition of the view ~{0}.{1}... ", schema, view), ConsoleColor.Yellow);                                                                                          
                if(!CompareSelects(GetViewDefinition(schema, view), definition)) errors.Add("The view definition does not match with the expected one.");                                   
            }
            catch(Exception e){
                errors.Add(e.Message);
            }             

            return errors;
        }
        /// <summary>
        /// Checks if new data can be inserted into the table.
        /// </summary>
        /// <param name="schema">Schema where the table is.</param>
        /// <param name="table">The table where the data will be added.</param>        
        /// <param name="pkField">The primary key field name.</param>
        /// <param name="fields">Key-value pairs of data [field, value], subqueries as values must start with @.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfTableInsertsData(string schema, string table, Dictionary<string, object> fields, string pkField){
           List<string> errors = new List<string>();            

            try{       
                if(Output != null) Output.Write(string.Format("Checking if a new item can be inserted into the table ~{0}.{1}... ", schema, table), ConsoleColor.Yellow);               
                InsertData(schema, table, fields, pkField);
            }
            catch(Exception e){
                errors.Add(e.Message);
            } 

            return errors;
        }
        /// <summary>
        /// Checks if old data can be updated into the table.
        /// </summary>
        /// <param name="schema">Schema where the table is.</param>
        /// <param name="table">The table where the data will be added.</param>
        /// <param name="fields">Key-value pairs of data [field, value], subqueries as values must start with @.</param>        
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfTableUpdatesData(string schema, string table, Dictionary<string, object> fields){
            return CheckIfTableUpdatesData(schema, table, fields, null, 0);
        }
        /// <summary>
        /// Checks if old data can be updated into the table.
        /// </summary>
        /// <param name="schema">Schema where the table is.</param>
        /// <param name="table">The table where the data will be added.</param>
        /// <param name="fields">Key-value pairs of data [field, value], subqueries as values must start with @.</param>        
        /// <param name="filterField">The field name used to find the affected registries.</param>
        /// <param name="filterValue">The field value used to find the affected registries.</param> 
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfTableUpdatesData(string schema, string table, Dictionary<string, object> fields, string filterField, object filterValue){
           List<string> errors = new List<string>();            

            try{       
                if(Output != null) Output.Write(string.Format("Checking if a new item can be updated into the table ~{0}.{1}... ", schema, table), ConsoleColor.Yellow);               
                UpdateData(schema, table, fields, filterField, filterValue);
            }
            catch(Exception e){
                errors.Add(e.Message);
            } 

            return errors;
        }
        /// <summary>
        /// Checks if old data can be removed from the table.
        /// </summary>
        /// <param name="schema">Schema where the table is.</param>
        /// <param name="table">The table where the data will be added.</param>        
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfTableDeletesData(string schema, string table){
            return CheckIfTableDeletesData(schema, table, null, 0);
        }
        /// <summary>
        /// Checks if old data can be removed from the table.
        /// </summary>
        /// <param name="schema">Schema where the table is.</param>
        /// <param name="table">The table where the data will be added.</param>        
        /// <param name="filterField">The field name used to find the affected registries.</param>
        /// <param name="filterValue">The field value used to find the affected registries.</param> 
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfTableDeletesData(string schema, string table, string filterField, object filterValue){
           List<string> errors = new List<string>();            

            try{       
                if(Output != null) Output.Write(string.Format("Checking if an old item can be removed from the table ~{0}.{1}... ", schema, table), ConsoleColor.Yellow);               
                DeleteData(schema, table, filterField, filterValue);
            }
            catch(Exception e){
                errors.Add(e.Message);
            } 

            return errors;
        }
        /// <summary>
        /// Checks if old data can be removed from the table.
        /// </summary>
        /// <param name="schema">Schema where the table is.</param>
        /// <param name="table">The table where the data will be added.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfTableMatchesAmountOfRegisters(string schema, string table, int amount){
           return CheckIfTableMatchesAmountOfRegisters(schema, table, null, 0, amount);
        }
        /// <summary>
        /// Checks if old data can be removed from the table.
        /// </summary>
        /// <param name="schema">Schema where the table is.</param>
        /// <param name="table">The table where the data will be added.</param>
        /// <param name="filterField">The field name used to find the affected registries.</param>
        /// <param name="filterValue">The field value used to find the affected registries.</param> 
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfTableMatchesAmountOfRegisters(string schema, string table, string filterField,  object filterValue, int amount){
           List<string> errors = new List<string>();            

            try{       
                if(Output != null) Output.Write(string.Format("Checking the amount of items in table ~{0}.{1}... ", schema, table), ConsoleColor.Yellow);                               
                long count = CountRegisters(schema, table, filterField, filterValue);
                if(!count.Equals(amount)) errors.Add(string.Format("Amount of registers missmatch over the table '{0}.{1}': expected->'{2}' found->'{3}'.", schema, table, amount, count));
            }
            catch(Exception e){
                errors.Add(e.Message);
            } 

            return errors;
        }
#endregion
#region Actions
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
        public DataSet SelectData(string schema, string table, Dictionary<string, object> fields, string filterField, object filterValue, char filterOperator='='){           
            string query = string.Format("SELECT {0} FROM {1}.{2}", string.Join(",", fields.Keys), schema, table);
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
        public int InsertData(string schema, string table, Dictionary<string, object> fields, string pkField){
            string query = string.Format("INSERT INTO {0}.{1} ({2}) VALUES (", schema, table, string.Join(',', fields.Keys));
            foreach(string field in fields.Keys)
                query += ParseObjectForSQL(fields[field]);
            
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
        public void UpdateData(string schema, string table, Dictionary<string, object> fields){
            UpdateData(schema, table, fields, null, 0);
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
        public void UpdateData(string schema, string table, Dictionary<string, object> fields, string filterField, object filterValue, char filterOperator='='){                             
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
            this.Conn.Open();
            string query = string.Format("DELETE FROM {0}.{1}", schema, table);
            if(!string.IsNullOrEmpty(filterField)) query += string.Format(" WHERE {0}{1}{2};", filterField, filterOperator, ParseObjectForSQL(filterValue));

            ExecuteNonQuery(query);            
        }
        /// <summary>
        /// Compares if the given entry data matches with the current one stored in the database.
        /// </summary>
        /// <param name="select">The select query to be executed in order to get the data and compare.</param>       
        /// <param name="fields">A set of [field-name, field-value] pairs which will be used to check the entry data.</param>        
        /// <returns>The list of mismmatches found (the list will be empty it there's no errors).</returns>
        public List<string> MatchesData(string select, Dictionary<string, object> fields){  
            return MatchesData(ExecuteQuery(select), fields);              
        }
        /// <summary>
        /// Compares if the given entry data matches with the current one stored in the database.
        /// </summary>
        /// <param name="ds">The data that will be compared.</param>       
        /// <param name="fields">A set of [field-name, field-value] pairs which will be used to check the entry data.</param>        
        /// <returns>The list of mismmatches found (the list will be empty it there's no errors).</returns>
        public List<string> MatchesData(DataSet ds, Dictionary<string, object> fields){    
            List<string> errors = new List<string>();            
            
            int count = 0;
            foreach(DataRow dr in ds.Tables[0].Rows){    
                count++;
                                                
                foreach(string k in fields.Keys){
                    if(!dr[k].Equals(fields[k])) 
                        errors.Add(string.Format("Incorrect data found on '{0}.{1}': expected->'{2}' found->'{3}'.", ds.Tables[0].Namespace, ds.Tables[0].TableName, fields[k], dr[k]));
                }
            }  

            if(count == 0) errors.Add(string.Format("Unable to find any data for the given query on ~{0}.{1}... ", ds.Tables[0].Namespace, ds.Tables[0].TableName));                      
            
            return errors;
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
        /// <param name="selectLeft">The left-side select query.</param>
        /// <param name="selectRight">The right-side select query.</param>
        /// <returns>True if both select queries are equivalent.</returns>
        public bool CompareSelects(string selectLeft, string selectRight){
            return (0 == (long)ExecuteScalar(string.Format("SELECT COUNT(*) FROM (({0}) EXCEPT ({1})) AS result;", CleanSqlQuery(selectLeft), CleanSqlQuery(selectRight))));
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
#endregion
#region Static
        /// <summary>
        /// Extracts the student's name from de database's name, but only if it follows the naming convention 'prefix_STUDENT'.
        /// </summary>
        /// <param name="database">The database name, it must follows the naming convention 'prefix_STUDENT'.</param>
        /// <returns>The student's name.</returns>
        public static string ToStudentName(string database){
            if(!database.Contains("_")) throw new Exception("The current database name does not follows the naming convetion 'prefix_STUDENT'.");
            return database.Substring(database.IndexOf("_")+1).Replace("_", " ");
        }
#endregion
#region Private       
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
            return (quotes ? string.Format(" '{0}',", item) : string.Format(" {0},", item.ToString().TrimStart('@')));
        }
#endregion
    }
}