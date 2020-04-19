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
using System.Data;
using System.Linq;
using System.Collections.Generic;
using AutoCheck.Core;
using Source = AutoCheck.Connectors.Postgres.Source;
using Destination = AutoCheck.Connectors.Postgres.Destination;
using Filter = AutoCheck.Connectors.Postgres.Filter;

namespace AutoCheck.Checkers{        
    /// <summary>
    /// Allows data validations over a PostgreSQL instance.
    /// </summary>
    public class Postgres: Core.Checker{    
        /// <summary>
        /// The main connector, can be used to perform direct operations over the data source.
        /// </summary>
        /// <value></value>    
        public Connectors.Postgres Connector {get; private set;}
        /// <summary>
        /// PostgreSQL host address.
        /// </summary>
        /// <value></value>
        public string Host {
            get{
                return this.Connector.Host;
            }
        }
        /// <summary>
        /// The PostgreSQL database username, which will be used to perform operations.
        /// </summary>
        /// <value></value>
        public string Name {
            get{
                return this.Connector.Database;
            }
        }   
        /// <summary>
        /// The student name wich is the original database creator.
        /// </summary>
        /// <value></value>        
        public string Student{
            get{
               return this.Connector.Student;
            }
        }
        /// <summary>
        ///  Creates a new checker instance.
        /// </summary>
        /// <param name="host">Host address in order to connect with the running PostgreSQL service, wich contains the Odoo database.</param>
        /// <param name="database">The PostgreSQL database name.</param>
        /// <param name="username">The PostgreSQL database username, which will be used to perform operations.</param>
        /// <param name="password">The PostgreSQL database password, which will be used to perform operations.</param>
        public Postgres(string host, string database, string username, string password): base(){
            this.Connector = new Connectors.Postgres(host, database, username, password);
        } 
        /// <summary>
        /// Disposes the object releasing its unmanaged properties.
        /// </summary>
        public override void Dispose(){
            this.Connector.Dispose();
        }   
        /// <summary>
        /// Compares a set of expected privileges with the current table's ones.
        /// </summary>        
        /// <param name="role">The role which privileges will be checked.</param>
        /// <param name="schema">The schema containing the table to check.</param>
        /// <param name="table">The table which privileges will be checked against the role's ones.</param>        
        /// <param name="expected">ACL letters as appears on PostgreSQL documentation: https://www.postgresql.org/docs/11/sql-grant.html</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfTableMatchesPrivileges(string role, string schema, string table, string expected){
            List<string> errors = new List<string>();                         
            
            try{
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Getting the permissions for the role '{0}' on table ~{1}.{2}... ", role, schema, table), ConsoleColor.Yellow);
                
                int count = 0;
                string currentPrivileges = "";

                foreach(DataRow dr in this.Connector.GetTablePrivileges(new Source(schema, table), role).Tables[0].Rows){                        
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
                }

                if(count == 0) errors.Add(string.Format("Unable to find any privileges for the table '{0}.{1}'", schema, table));
                else if(!currentPrivileges.Equals(expected)) errors.Add(string.Format("Privileges missmatch over the table '{0}.{1}': expected->'{2}' found->'{3}'.", schema, table, expected, currentPrivileges));
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
        /// <param name="expected">ACL letter as appears on PostgreSQL documentation: https://www.postgresql.org/docs/11/sql-grant.html</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfTableContainsPrivileges(string role, string schema, string table, char expected){
            List<string> errors = new List<string>();                         
            
             try{
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Getting the permissions for the role '{0}' on table ~{1}.{2}... ", role, schema, table), ConsoleColor.Yellow);

                int count = 0;
                string currentPrivileges = "";

                foreach(DataRow dr in this.Connector.GetTablePrivileges(new Source(schema, table), role).Tables[0].Rows){
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
                }    
                
                if(count == 0) errors.Add(string.Format("Unable to find any privileges for the table '{0}.{1}'", schema, table));
                else if(!currentPrivileges.Contains(expected)) errors.Add(string.Format("Privileges missmatch over the table '{0}.{1}': expected->'{2}' found->'{3}'.", schema, table, expected, currentPrivileges));                    
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
        /// <param name="expected">ACL letters as appears on PostgreSQL documentation: https://www.postgresql.org/docs/11/sql-grant.html</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfSchemaMatchesPrivileges(string role, string schema, string expected){
           List<string> errors = new List<string>();    

            try{                     
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Getting the permissions for the role '{0}' on schema ~{1}... ", role, schema), ConsoleColor.Yellow);                                
                string currentPrivileges = "";
                int count = 0;

                foreach(DataRow dr in this.Connector.GetSchemaPrivileges(role, schema).Tables[0].Rows){
                    count++;

                    //ACL letters: https://www.postgresql.org/docs/9.3/sql-grant.html
                    if((bool)dr["usage_grant"]) currentPrivileges += "U";
                    if((bool)dr["create_grant"]) currentPrivileges += "C";                                
                }
                
                if(count == 0) errors.Add(string.Format("Unable to find any privileges for the role '{0}' on schema '{1}'.", role, schema));
                if(!currentPrivileges.Equals(expected)) errors.Add(string.Format("Privileges missmatch over the schema '{0}': expected->'{1}' found->'{2}'.", schema, expected, currentPrivileges));                    
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
        /// <param name="expected">ACL letter as appears on PostgreSQL documentation: https://www.postgresql.org/docs/11/sql-grant.html</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfSchemaContainsPrivilege(string role, string schema, char expected){
            List<string> errors = new List<string>();                         

            try{
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Getting the permissions for the role '{0}' on schema ~{1}... ", role, schema), ConsoleColor.Yellow);                 

                int count = 0;
                foreach(DataRow dr in this.Connector.GetSchemaPrivileges(role, schema).Tables[0].Rows){
                    count ++;
                    
                    //ACL letters: https://www.postgresql.org/docs/9.3/sql-grant.html
                    switch(expected){
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
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Getting the membership for the role ~{0}... ", role), ConsoleColor.Yellow);
                foreach(DataRow dr in this.Connector.GetMembership(role).Tables[0].Rows){
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
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Getting the foreign key for ~{0}.{1}.{2} -> {2}.{3}.{4}... ", schemaFrom,tableFrom, columnFrom, schemaTo, tableTo, columnTo), ConsoleColor.Yellow);            
                                
                int count = 0;
                bool found = false;                    

                foreach(DataRow dr in this.Connector.GetForeignKeys(new Source(schemaFrom, tableFrom)).Tables[0].Rows){  
                    count++;               
                    if( dr["columnFrom"].ToString().Equals(columnFrom) && 
                        dr["schemaTo"].ToString().Equals(schemaTo) && 
                        dr["tableTo"].ToString().Equals(tableTo) && 
                        dr["columnTo"].ToString().Equals(columnTo)
                    ) found = true;                        
                }

                if(count == 0) errors.Add(string.Format("Unable to find any FOREIGN KEY for the table '{0}.{1}'", schemaFrom, tableFrom));
                else if(!found) errors.Add(string.Format("Unable to find the FOREIGN KEY from '{0}.{1}' to '{2}.{2}'", schemaFrom, tableFrom, schemaTo, tableTo)); 
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
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Checking if a new item has been added to the table ~{0}.{1}... ", schema, table), ConsoleColor.Yellow);      
                long count = (long)this.Connector.CountRegisters(new Source(schema, table), new Filter(pkField, Operator.GREATER, lastPkValue));
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
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Checking if an item has been removed from the table ~{0}.{1}... ", schema, table), ConsoleColor.Yellow);
                long count = (long)this.Connector.CountRegisters(new Source(schema, table), new Filter(pkField, Operator.EQUALS, removedPkValue));
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
        /// <param name="table">The table to check.</param>        
        /// <param name="expected">A set of [field-name, field-value] pairs which will macthed with the table data.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfTableMatchesData(DataTable table, Dictionary<string, object> expected){    
            List<string> errors = new List<string>();            
            
            try{
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Checking the entry data for ~{0}.{1}... ", table.Namespace, table.TableName), ConsoleColor.Yellow);

                int count = 0;
                foreach(DataRow dr in table.Rows){    
                    count++;
                                                    
                    foreach(string k in expected.Keys){
                        if(!dr[k].Equals(expected[k])) 
                            errors.Add(string.Format("Incorrect data found for {0} in {1}.{2}: expected->'{3}' found->'{4}'.", k, table.Namespace, table.TableName, expected[k], dr[k]));
                    }
                }  

                if(count == 0) errors.Add(string.Format("Unable to find any data for the given query for {0}.{1}... ", table.Namespace, table.TableName));        
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
        /// <param name="filterField">The field name which be used to find the registry.</param>
        /// <param name="filterValue">The field value which be used to find the registry.</param>
        /// <param name="expected">A set of [field-name, field-value] pairs which will be used to check the entry data.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfTableMatchesData(string schema, string table, string filterField, object filterValue, Dictionary<string, object> expected){    
            List<string> errors = new List<string>();                                                
                            
            try{
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Checking the entry data for ~{0}={1}~ on ~{2}.{3}... ", filterField, filterValue, schema, table), ConsoleColor.Yellow);                                      
                Output.Instance.Disable();
                return CheckIfTableMatchesData(this.Connector.Select(new Source(schema, table), new Filter(filterField, Operator.EQUALS, filterValue), expected.Keys.ToArray()).Tables[0], expected);                    
            }  
            catch(Exception ex){
                errors.Add(ex.Message);
                return errors;
            }         
            finally{
                Output.Instance.UndoStatus();
            }
        }  
        /// <summary>
        /// Compares if the given entry data matches with the current one stored in the database.
        /// </summary>        
        /// <param name="schema">The schema containing the table to check.</param>
        /// <param name="table">The table to check.</param>                
        /// <param name="expected">A set of [field-name, field-value] pairs which will be used to check the entry data.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfTableMatchesData(string schema, string table, Dictionary<string, object> expected){    
            List<string> errors = new List<string>();                                                
                            
            try{
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Checking the entry data on ~{0}.{1}... ", schema, table), ConsoleColor.Yellow);                                      
                                
                List<string> conditions = new List<string>();
                foreach(string k in expected.Keys.ToArray()){
                    if(expected[k].GetType() == typeof(string)) conditions.Add(string.Format("{0} = '{1}'", k, expected[k]));
                    else conditions.Add(string.Format("{0} = {1}", k, expected[k]));
                }

                Output.Instance.Disable();
                return CheckIfTableMatchesData(this.Connector.Select(new Source(schema, table).ToString(), string.Join(" AND ", conditions), expected.Keys.ToArray()).Tables[0], expected);                    
            }  
            catch(Exception ex){
                errors.Add(ex.Message);
                return errors;
            }         
            finally{
                Output.Instance.UndoStatus();
            }
        }
        /// <summary>
        /// Compares if the given entry data matches with the current one stored in the database.
        /// </summary>        
        /// <param name="select">The select query to perform.</param>
        /// <param name="expected">A set of [field-name, field-value] pairs which will be used to check the entry data.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfSelectMatchesData(string select, Dictionary<string, object> expected){    
            List<string> errors = new List<string>();                                                        
            return CheckIfTableMatchesData(this.Connector.ExecuteQuery(select).Tables[0], expected);
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
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Checking the creation of the table ~{0}.{1}... ", schema, table), ConsoleColor.Yellow);
                //If not exists, an exception will be thrown                    
                this.Connector.CountRegisters(new Source(schema, table));
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
        /// <param name="expected">The SQL select query which result should produce the same result as the view.</param>        
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfViewMatchesDefinition(string schema, string view, string expected){
           List<string> errors = new List<string>();            

            try{                
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Checking the SQL definition of the view ~{0}.{1}... ", schema, view), ConsoleColor.Yellow);                                                                                          
                if(!this.Connector.CompareSelects(expected, this.Connector.GetViewDefinition(new Source(schema, view)))) errors.Add("The view definition does not match with the expected one.");                                   
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
        public List<string> CheckIfTableInsertsData(string schema, string table, Dictionary<string, object> fields){
           List<string> errors = new List<string>();            

            try{       
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Checking if a new item can be inserted into the table ~{0}.{1}... ", schema, table), ConsoleColor.Yellow);               
                this.Connector.Insert(new Destination(schema, table), fields);
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
            return CheckIfTableUpdatesData(schema, table, null, null, fields);
        }
        /// <summary>
        /// Checks if old data can be updated into the table, the filter operator '=' will be used.
        /// </summary>
        /// <param name="schema">Schema where the table is.</param>
        /// <param name="table">The table where the data will be added.</param>
        /// <param name="fields">Key-value pairs of data [field, value], subqueries as values must start with @.</param>        
        /// <param name="filterField">The field name used to find the affected registries.</param>
        /// <param name="filterValue">The field value used to find the affected registries.</param> 
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfTableUpdatesData(string schema, string table, string filterField, object filterValue, Dictionary<string, object> fields){
            return CheckIfTableUpdatesData(schema, table, filterField, filterValue, Operator.EQUALS, fields);
        }
        /// <summary>
        /// Checks if old data can be updated into the table.
        /// </summary>
        /// <param name="schema">Schema where the table is.</param>
        /// <param name="table">The table where the data will be added.</param>
        /// <param name="fields">Key-value pairs of data [field, value], subqueries as values must start with @.</param>        
        /// <param name="filterField">The field name used to find the affected registries.</param>
        /// <param name="filterValue">The field value used to find the affected registries.</param> 
        /// <param name="filterOperator">The operator to use, % for LIKE.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfTableUpdatesData(string schema, string table, string filterField, object filterValue, Operator filterOperator, Dictionary<string, object> fields){
           List<string> errors = new List<string>();            

            try{       
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Checking if a new item can be updated into the table ~{0}.{1}... ", schema, table), ConsoleColor.Yellow);               
                this.Connector.Update(new Destination(schema, table), new Filter(filterField, filterOperator, filterValue), fields);
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
            return CheckIfTableDeletesData(schema, table, null, null);
        }
        /// <summary>
        /// Checks if old data can be removed from the table.
        /// </summary>
        /// <param name="schema">Schema where the table is.</param>
        /// <param name="table">The table where the data will be added.</param>        
        /// <param name="filterField">The field name used to find the affected registries.</param>
        /// <param name="filterValue">The field value used to find the affected registries.</param> 
        /// <param name="filterOperator">The operator to use, % for LIKE.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfTableDeletesData(string schema, string table, string filterField, object filterValue, Operator filterOperator=Operator.EQUALS){
           List<string> errors = new List<string>();            

            try{       
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Checking if an old item can be removed from the table ~{0}.{1}... ", schema, table), ConsoleColor.Yellow);               
                if(string.IsNullOrEmpty(filterField)) this.Connector.Delete(new Destination(schema, table));
                else this.Connector.Delete(new Destination(schema, table), new Filter(filterField, filterOperator, filterValue));
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
        /// <param name="expected">Amount of data expected to be found.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfTableMatchesAmountOfRegisters(string schema, string table, int expected){
           return CheckIfTableMatchesAmountOfRegisters(schema, table, null, null, expected);
        }
        /// <summary>
        /// Checks if old data can be removed from the table.
        /// </summary>        
        /// <param name="schema">Schema where the table is, the filter operator '=' will be used.</param>
        /// <param name="table">The table where the data will be added.</param>
        /// <param name="filterField">The field name used to find the affected registries.</param>
        /// <param name="filterValue">The field value used to find the affected registries.</param> 
        /// <param name="expected">Amount of data expected to be found.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfTableMatchesAmountOfRegisters(string schema, string table, string filterField,  object filterValue, int expected){
            return CheckIfTableMatchesAmountOfRegisters(schema, table, filterField, filterValue, Operator.EQUALS, expected);
        }
        /// <summary>
        /// Checks if old data can be removed from the table.
        /// </summary>        
        /// <param name="schema">Schema where the table is.</param>
        /// <param name="table">The table where the data will be added.</param>
        /// <param name="filterField">The field name used to find the affected registries.</param>
        /// <param name="filterValue">The field value used to find the affected registries.</param> 
        /// <param name="filterOperator">The operator to use, % for LIKE.</param>        
        /// <param name="expected">Amount of data expected to be found.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfTableMatchesAmountOfRegisters(string schema, string table, string filterField,  object filterValue, Operator filterOperator, int expected){
           List<string> errors = new List<string>();            

            try{       
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Checking the amount of items in table ~{0}.{1}... ", schema, table), ConsoleColor.Yellow);                                               
                long count = (filterField == null ?  this.Connector.CountRegisters(new Source(schema, table)) : this.Connector.CountRegisters(new Source(schema, table), new Filter(filterField, filterOperator, filterValue)));
                if(!count.Equals(expected)) errors.Add(string.Format("Amount of registers missmatch over the table '{0}.{1}': expected->'{2}' found->'{3}'.", schema, table, expected, count));
            }
            catch(Exception e){
                errors.Add(e.Message);
            } 

            return errors;
        }
    }
}