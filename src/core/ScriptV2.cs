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
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using YamlDotNet.RepresentationModel;
using AutoCheck.Exceptions;


namespace AutoCheck.Core{
    //TODO: This will be the new Script (without V2)
    public class ScriptV2{
        public string Name {
            get{
                return Vars["script_name"].ToString();
            }
        }

        public string Folder {
            get{
                return Vars["current_folder"].ToString();
            }
        }

        public Dictionary<string, object> Vars {get; private set;}

#region Constructor    
        /// <summary>
        /// Creates a new script instance using the given script file.
        /// </summary>
        /// <param name="path">Path to the script file (yaml).</param>
        public ScriptV2(string path){
            if(!File.Exists(path)) throw new FileNotFoundException(path);
            
            Vars = new Dictionary<string, object>();
            ParseScript(path);
        }
#endregion
#region Parsing   
        private void ParseScript(string path){            
            var yaml = new YamlStream();

            try{
                yaml.Load(new StringReader(File.ReadAllText(path)));
            }
            catch(Exception ex){
                throw new DocumentInvalidException("Unable to parse the YAML document, see inner exception for further details.", ex);
            }
                        
            var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

            Vars.Add("script_name", (mapping.Children.ContainsKey("name") ? mapping.Children["name"].ToString() : Regex.Replace(Path.GetFileNameWithoutExtension(path).Replace("_", " "), "[A-Z]", " $0")));
            Vars.Add("current_folder", (mapping.Children.ContainsKey("folder") ? mapping.Children["folder"].ToString() : AppContext.BaseDirectory));
            
            if(mapping.Children.ContainsKey("vars")) ParseVars((YamlMappingNode)mapping.Children[new YamlScalarNode("vars")]);
            if(mapping.Children.ContainsKey("pre")) ParsePre((YamlSequenceNode)mapping.Children[new YamlScalarNode("pre")]);
            
            //Validation
            var expected = new string[]{"name", "folder", "inherits", "vars", "pre", "post", "body"};
            foreach (var entry in mapping.Children)
            {                
                var current = entry.Key.ToString().ToLower();
                if(!expected.Contains(current)) throw new DocumentInvalidException($"Unexpected value '{current}' found.");              
            }
        }
        
        private void ParseVars(YamlMappingNode root){
            foreach (var item in root.Children){
                var name = item.Key.ToString();
                var value = item.Value.ToString();

                var reserved = new string[]{"script_name", "current_folder", "now"};
                if(reserved.Contains(name)) throw new VariableInvalidException($"The variable name {name} is reserved and cannot be declared.");

                foreach(Match match in Regex.Matches(value, "{(.*?)}")){
                    var replace = match.Value.TrimStart('{').TrimEnd('}');                    
                    
                    if(replace.StartsWith("#") || replace.StartsWith("$")){                        
                        //Check if the regex is valid and/or also the referred var exists.
                        var regex = string.Empty;
                        if(replace.StartsWith("#")){
                            var error = $"The regex {replace} must start with '#' and end with a '$' followed by variable name.";
                            
                            if(!replace.Contains("$")) throw new RegexInvalidException(error);
                            regex = replace.Substring(1, replace.LastIndexOf("$")-1);
                            replace = replace.Substring(replace.LastIndexOf("$"));
                            if(string.IsNullOrEmpty(replace)) throw new RegexInvalidException(error);
                        }

                        replace = replace.TrimStart('$');
                        if(replace.Equals("NOW")) replace = DateTime.Now.ToString();
                        else if(!Vars.ContainsKey(replace.ToLower())) throw new VariableInvalidException($"Unable to apply a regular expression over an undefined variable {replace} as requested within the variable '{name}'.");                            

                        if(string.IsNullOrEmpty(regex)) replace = Vars[replace.ToLower()].ToString();
                        else {
                            try{
                                replace = Regex.Match(replace, regex).Value;
                            }
                            catch{
                                throw new RegexInvalidException($"Invalid regular expression defined inside the variable '{name}'.");
                            }
                        }
                    }
                    
                    value = value.Replace(match.Value, replace);
                }
                
                if(Vars.ContainsKey(name)) throw new VariableInvalidException($"Repeated variables defined with name '{name}'.");
                else Vars.Add(name, value);
            }
        }
        
        private void ParsePre(YamlSequenceNode root){
            //Loop through because the order matters
            foreach (YamlMappingNode current in root)
            {
                foreach (var item in current.Children){  
                    var name = item.Key.ToString();   
                    
                    YamlMappingNode mapping;
                    try{
                        mapping = (YamlMappingNode)current.Children[new YamlScalarNode(name)];
                    }
                    catch{
                        mapping = new YamlMappingNode();
                    }

                    switch(name){
                        case "extract":
                            var ex_file =  (mapping.Children.ContainsKey("file") ? mapping.Children["file"].ToString() : "*.zip");
                            var ex_remove =  (mapping.Children.ContainsKey("remove") ? bool.Parse(mapping.Children["remove"].ToString()) : false);
                            var ex_recursive =  (mapping.Children.ContainsKey("recursive") ? bool.Parse(mapping.Children["recursive"].ToString()) : false);
                            Extract(ex_file, ex_remove,  ex_recursive);                        
                            break;

                        case "restore_db":
                            var db_file =  (mapping.Children.ContainsKey("file") ? mapping.Children["file"].ToString() : "*.sql");
                            var db_host =  (mapping.Children.ContainsKey("db_host") ? mapping.Children["db_host"].ToString() : "localhost");
                            var db_user =  (mapping.Children.ContainsKey("db_user") ? mapping.Children["db_user"].ToString() : "postgres");
                            var db_pass =  (mapping.Children.ContainsKey("db_pass") ? mapping.Children["db_pass"].ToString() : "postgres");
                            var db_name =  (mapping.Children.ContainsKey("db_name") ? mapping.Children["db_name"].ToString() : "public");
                            var db_override =  (mapping.Children.ContainsKey("override") ? bool.Parse(mapping.Children["override"].ToString()) : false);
                            var db_remove =  (mapping.Children.ContainsKey("remove") ? bool.Parse(mapping.Children["remove"].ToString()) : false);
                            RestoreDB(db_file, db_host,  db_user, db_pass, db_name, db_override, db_remove);
                            break;

                        case "upload_gdrive":
                            break;

                        default:
                            throw new DocumentInvalidException($"Unexpected value '{name}' found.");
                    }                    
                }
            }
        }
#endregion
#region ZIP        
        private void Extract(string file, bool remove, bool recursive){
            Output.Instance.WriteLine("Extracting files: ");
            Output.Instance.Indent();
           
            try{
                string[] files = Directory.GetFiles(Folder, file, (recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));                    
                if(files.Length == 0) Output.Instance.WriteLine("Done!");                    
                else{
                    foreach(string zip in files){
                        try{
                            Output.Instance.Write($"Extracting the file ~{zip}... ", ConsoleColor.DarkYellow);
                            Utils.ExtractFile(zip);
                            Output.Instance.WriteResponse();
                        }
                        catch(Exception e){
                            Output.Instance.WriteResponse($"ERROR {e.Message}");
                            continue;
                        }

                        if(remove){                        
                            try{
                                Output.Instance.Write($"Removing the file ~{zip}... ", ConsoleColor.DarkYellow);
                                File.Delete(zip);
                                Output.Instance.WriteResponse();
                                Output.Instance.BreakLine();
                            }
                            catch(Exception e){
                                Output.Instance.WriteResponse($"ERROR {e.Message}");
                                continue;
                            }  
                        }
                    }                                                                  
                }                    
            }
            catch (Exception e){
                Output.Instance.WriteResponse(string.Format("ERROR {0}", e.Message));
            }
            finally{    
                Output.Instance.UnIndent();
                if(!remove) Output.Instance.BreakLine();
            }            
        }
#endregion
#region BBDD                
        private void RestoreDB(string file, string dbhost, string dbuser, string dbpass, string dbname, bool @override, bool remove){
            using(var db = new Connectors.Postgres(dbhost, dbname, dbuser, dbpass)){        
                Output.Instance.WriteLine($"Checking the database ~{dbname}: ", ConsoleColor.DarkYellow); 
                Output.Instance.Indent();
                
                if(!@override && db.ExistsDataBase()) Output.Instance.WriteLine("The database already exists, skipping!"); 
                else{
                    if(@override && db.ExistsDataBase()){                
                        try{
                            Output.Instance.Write("Dropping the existing database: "); 
                            db.DropDataBase();
                            Output.Instance.WriteResponse();
                        }
                        catch(Exception ex){
                            Output.Instance.WriteResponse(ex.Message);
                        } 
                    } 

                    var sql = Directory.GetFiles(Folder, file, SearchOption.AllDirectories).FirstOrDefault();
                    try{
                        Output.Instance.Write($"Creating the database using the file ~{file}... ", ConsoleColor.DarkYellow);
                        db.CreateDataBase(sql);
                        Output.Instance.WriteResponse();
                    }
                    catch(Exception ex){
                        Output.Instance.WriteResponse(ex.Message);
                    } 

                    if(remove){                        
                        try{
                            Output.Instance.Write($"Removing the file ~{sql}... ", ConsoleColor.DarkYellow);
                            File.Delete(sql);
                            Output.Instance.WriteResponse();
                            Output.Instance.BreakLine();
                        }
                        catch(Exception e){
                            Output.Instance.WriteResponse($"ERROR {e.Message}");
                        }  
                    } 
                }            

                Output.Instance.UnIndent(); 
                Output.Instance.BreakLine();      
            }       
        } 
#endregion
    }
}