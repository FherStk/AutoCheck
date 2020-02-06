using Npgsql;
using System;
using System.Linq;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator.Utils{   
    //Experimental methods or functionalities for the regular Utilities

    public partial class DataBase
    {
        private class Select{
            public List<Field> Fields {get; set;}
            public Field From {get; set;}
            public List<Join> Joins {get; set;}

            public int Length{
                get{
                    return this.ToString().Length;
                }
            }

            public Select(){
                this.Fields = new List<Field>();
                this.Joins = new List<Join>();
            }

            public bool Equals(Select s){
                if(!this.From.Equals(s.From)) return false;
                if(!this.Fields.Count.Equals(s.Fields.Count)) return false;
                if(!this.Joins.Count.Equals(s.Joins.Count)) return false;

                List<Field> leftF = this.Fields.OrderBy(x => x.Name).ToList();
                List<Field> rightF = s.Fields.OrderBy(x => x.Name).ToList();
                for(int i = 0; i < leftF.Count; i++)
                    if(!leftF[i].Equals(rightF[i])) return false;

                List<Join> leftJ = this.Joins.OrderBy(x => x.Field.Name).ToList();
                List<Join> rightJ = s.Joins.OrderBy(x => x.Field.Name).ToList();
                for(int i = 0; i < leftJ.Count; i++)
                    if(!leftJ[i].Equals(rightJ[i])) return false;               
                
                return true;
            }

            public override string ToString(){
                return string.Format("SELECT {0} FROM {1} {2}", this.Fields.ToString(), this.From.ToString(), this.Joins.ToString());
            }
        }
        private class Field {
            public string Qualification {get; set;}
            public string Name {get; set;}
            public string Alias {get; set;}

            public int Length{
                get{
                    return this.ToString().Length;
                }
            }

            public override string ToString(){
                return this.ToString(false);
            }

            public string ToString(bool ignoreAlias){
                string sql = string.IsNullOrEmpty(this.Qualification) ?  this.Name : string.Format("{0}.{1}", this.Qualification, this.Name);
                return (ignoreAlias ? sql : string.Format("{0}{1}", sql, this.Alias));                
            }

            public bool Equals(Field f, bool ignoreAlias = false){
                bool result = this.Qualification.Equals(f.Qualification) && this.Name.Equals(f.Name) && (ignoreAlias || this.Alias.Equals(f.Alias));                
                return result;
            }
        }
        private class Join {            
            public string Type {get; set;}
            public Field Field {get; set;}
            public string LeftQualification {get; set;}
            public string LeftName {get; set;}
            public string Operator {get; set;}
            public string RightQualification {get; set;}
            public string RightName {get; set;}

            public int Length{
                get{
                    return this.ToString().Length;
                }
            }

            public override string ToString(){
                return string.Format("{0} {1} ON {2}.{3} {4} {5} {6}", this.Type, this.Field.ToString(true), this.LeftQualification, this.LeftName, this.Operator, this.RightQualification, this.RightName);                
            }

            public bool Equals(Join j){
                if(!this.Type.Equals(j.Type) && this.Field.Equals(j.Field, true) && this.Operator.Equals(j.Operator)) return false;                
                if(this.Operator != "=") throw new NotImplementedException();   //TODO: implement > and <!
                else{
                    bool same = (this.LeftQualification.Equals(j.LeftQualification) && this.LeftName.Equals(j.LeftName) && this.RightQualification.Equals(j.RightQualification) && this.RightName.Equals(j.RightName));
                    bool swap = (this.LeftQualification.Equals(j.RightQualification) && this.LeftName.Equals(j.RightName) && this.RightQualification.Equals(j.LeftQualification) && this.RightName.Equals(j.LeftName));

                    return same || swap;
                }                    
            }
        }
        /// <summary>
        /// Parses the SQL select view definition in order to check if it has been defined correctly.
        /// </summary>
        /// <param name="schema">The schema containing the table to check.</param>
        /// <param name="table">The table to check.</param>
        /// <param name="definition">The SQL select query which result should produce the same result as the view.</param>        
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> _EXPERIMENTAL_CheckIfViewDefinitionMatches(string schema, string view, string definition){               
            List<string> errors = new List<string>();            

            try{                
                this.Conn.Open();
                if(Output != null) Output.Write(string.Format("Checking the definition of the view ~{0}.{1}... ", schema, view), ConsoleColor.Yellow);

                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format("SELECT view_definition FROM information_schema.views WHERE table_schema='{0}' AND table_name='{1}'", schema, view), this.Conn)){                                                            
                    bool equals = false;
                    string query = cmd.ExecuteScalar().ToString();

                    if(new string[] { " OR ", " AND ", "UNION", "GROUP BY", "ORDER BY"}.Any(s => definition.Contains(s))){
                        //Option 1: To select from the view should be equals to execute the given definition. Pros: Works for all definitions. Problem: not 100% sure in all the cases.
                        throw new NotImplementedException();                        
                        
                        //Alternate behaviour proposed:
                        //return CheckSelectQueryMatches(query, definition);
                    }
                    else{
                        //Option 2: To compare the _RETURN rule definition with the given one. Pros: No data missmatch possible. Problem: Complex and tricky algorithm, only compatible with simple queries (no ANDs, no ORs, no UNIONS...) and implement more options does not compensate the effort.
                        equals = ParseSelectQuery(definition).Equals(ParseSelectQuery(query));                        
                    }

                    if(!equals) errors.Add("The view definition does not match with the expected one.");                    
                }
            }
            catch(Exception e){
                errors.Add(e.Message);
            } 
            finally{
                this.Conn.Close();
            }

            return errors;
        }
        /// <summary>
        /// Parses the given SQL SELECT query into a Select object.
        /// </summary>
        /// <param name="sql">The SQL SELECT query.</param>
        /// <returns>A Select object.</returns>
        private Select ParseSelectQuery(string sql){   
            Select query = new Select();

            //STEP 1: Clean the query
            sql = sql.Replace("\r\n", "").Replace("\n", "").Replace("AS", "");            
            do sql = sql.Replace("  ", " ").Trim();
            while(sql.Contains("  "));

            //STEP 2: check the columns
            string temp = sql.Substring(0, sql.IndexOf("FROM")).Replace("SELECT", "");
            foreach(string c in temp.Split(","))                                         
                query.Fields.Add(ParseQueryTableField(c));
            
            //STEP 3: check the from
            temp = sql.Substring(sql.IndexOf("FROM") + 4);
            query.From = ParseQueryTableField(temp); 

            //STEP 4: check the joins
            temp = temp.Substring(temp.IndexOf(string.Format(" {0} ", query.From.Alias))+3).Trim();
            if(temp.Contains(" AND ") || temp.Contains(" OR ")) throw new NotImplementedException();    //too complex to implement...
            else{                
                string[] values = temp.Split(" ");                     
                values[5] = values[5].Replace("(", "");
                values[7] = values[7].Replace(")", "").Replace(";", "");

                query.Joins.Add(new Join(){
                    Type = string.Format("{0} {1}", values[0], values[1]),
                    Field = ParseQueryTableField (temp.Substring(temp.IndexOf("JOIN")+4, temp.IndexOf(" ON ")).Trim()),
                    LeftQualification = values[5].Split(".")[0],
                    LeftName = values[5].Split(".")[1],
                    Operator = values[6],
                    RightQualification = values[7].Split(".")[0],
                    RightName = values[7].Split(".")[1]
                });
            }
            
            //STEP 5: check the where  
            //TODO: implement when needed, sorry, not enought time. It could be easy for conditions like "WHEREW field = value" with no ORs or ANDs
            if(sql.Contains("WHERE")) throw new NotImplementedException();
        
            //STEP 6: replace the alias qualificators for fully ones
            ReplaceAliasesForFullQualification(query.From, query);
            
            foreach(Join j in query.Joins)
                ReplaceAliasesForFullQualification(j.Field, query);

            //NOTE: It could not work properly if the view has no qualification for the columns
            //      So extra work should be performed in order to find the original table's column if needed.

            return query;
        }        
        /// <summary>
        /// Parses the given SQL field definition (like "qualification.field alias") into a Field object.
        /// </summary>
        /// <param name="sql">The SQL field definition (like "qualification.field alias").</param>
        /// <returns>A Field object.</returns>
        private Field ParseQueryTableField(string sql){
            string[] values = sql.Trim().Split(" ");                
            
            string alias = values[1];
            string qualification = string.Empty;
            string field = string.Empty;                

            values[0] = values[0].Replace("(", "");
            if(!values[0].Contains(".")) field = values[0];
            else{
                qualification = values[0].Split(".")[0];
                field = values[0].Split(".")[1];
            }                

            return new Field(){
                Qualification = qualification, 
                Name = field, 
                Alias = alias
            };
        }
        /// <summary>
        /// Replaces the alias used as a qualification in the query object for the SELECT, FROM and JOIN fields.
        /// </summary>
        /// <param name="field">The field containing alias that should be converted to the full qualified info.</param>
        /// <param name="query">The query to modify.</param>        
        private void ReplaceAliasesForFullQualification(Field field, Select query){
            string qualification = string.IsNullOrEmpty(field.Qualification) ? field.Name : string.Format("{0}.{1}", field.Qualification, field.Name);
            
            foreach(Field f in query.Fields)
                if(f.Qualification == field.Alias) f.Qualification = qualification;

            foreach(Join j in query.Joins){
                if(j.LeftQualification == field.Alias) j.LeftQualification= qualification;
                if(j.RightQualification == field.Alias) j.RightQualification= qualification;
            }
        }
    }
}