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
#region Attributes
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
#endregion
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
                
                value = ComputeVarValue(item.Key.ToString(), item.Value.ToString());

                if(Vars.ContainsKey(name)) throw new VariableInvalidException($"Repeated variables defined with name '{name}'.");
                else Vars.Add(name, value);
            }
        }

        private string ComputeVarValue(string name, string value){
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
                    else if(!Vars.ContainsKey(replace.ToLower())) throw new VariableInvalidException($"Undefined variable {replace} has been requested within '{name}'.");                            

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
            
            return value;
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
                            var db_name =  (mapping.Children.ContainsKey("db_name") ? mapping.Children["db_name"].ToString() : Vars["script_name"].ToString());
                            var db_override =  (mapping.Children.ContainsKey("override") ? bool.Parse(mapping.Children["override"].ToString()) : false);
                            var db_remove =  (mapping.Children.ContainsKey("remove") ? bool.Parse(mapping.Children["remove"].ToString()) : false);
                            var db_recursive =  (mapping.Children.ContainsKey("recursive") ? bool.Parse(mapping.Children["recursive"].ToString()) : false);
                            RestoreDB(db_file, db_host,  db_user, db_pass, db_name, db_override, db_remove, db_recursive);
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
                        Vars.Add("file_name", Path.GetFileName(zip));

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

                        if(Vars.ContainsKey("file_name")) Vars.Remove("file_name");
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
        private void RestoreDB(string file, string dbhost, string dbuser, string dbpass, string dbname, bool @override, bool remove, bool recursive){
            Output.Instance.WriteLine("Restoring databases: ");
            Output.Instance.Indent();
           
            try{
                string[] files = Directory.GetFiles(Folder, file, (recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));                    
                if(files.Length == 0) Output.Instance.WriteLine("Done!");                    
                else{
                    foreach(string sql in files){
                        Vars.Add("file_name", Path.GetFileName(sql));

                        try{                            
                            //TODO: parse DB name to avoid forbidden chars.
                            var parsedDbName = Path.GetFileName(ComputeVarValue("dbname", dbname)).Replace(" ", "_").Replace(".", "_");
                            Output.Instance.WriteLine($"Checking the database ~{parsedDbName}: ", ConsoleColor.DarkYellow);      
                            Output.Instance.Indent();

                            using(var db = new Connectors.Postgres(dbhost, parsedDbName, dbuser, dbpass)){
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

                                    try{
                                        Output.Instance.Write($"Restoring the database using the file {sql}... ", ConsoleColor.DarkYellow);
                                        db.CreateDataBase(sql);
                                        Output.Instance.WriteResponse();
                                    }
                                    catch(Exception ex){
                                        Output.Instance.WriteResponse(ex.Message);
                                    }
                                }
                            }
                        }
                        catch(Exception e){
                            Output.Instance.WriteResponse($"ERROR {e.Message}");
                            continue;
                        }

                        if(remove){                        
                            try{
                                Output.Instance.Write($"Removing the file ~{sql}... ", ConsoleColor.DarkYellow);
                                File.Delete(sql);
                                Output.Instance.WriteResponse();
                            }
                            catch(Exception e){
                                Output.Instance.WriteResponse($"ERROR {e.Message}");
                                continue;
                            }
                        }

                        if(Vars.ContainsKey("file_name")) Vars.Remove("file_name");
                        Output.Instance.UnIndent();
                        Output.Instance.BreakLine();
                    }                                                                  
                }                    
            }
            catch (Exception e){
                Output.Instance.WriteResponse(string.Format("ERROR {0}", e.Message));
            }
            finally{    
                Output.Instance.UnIndent();
            }    
        } 
#endregion
#region Google Drive
        // private void UploadGDrive(string source, string user, string secret, string remoteFolder, bool @override, bool remove){            
        //     //TODO: New behaviour (sorry, no time for implementation)
        //     //          1. Create a new GDrive folder for the current student (within the current one)
        //     //          2. Copy there all the files shared (sometimes, the student shared an entire folder)
        //     //          3. Repeat for all the GDrive links found within the txt file.

        //     using(var drive = new Connectors.GDrive(secret, user)){
        //         Output.Instance.WriteLine("Checking the hosted Google Drive files: ", ConsoleColor.DarkYellow); 
        //         Output.Instance.Indent();
                
        //         var p = System.IO.Path.GetDirectoryName(this.GDriveFolder);
        //         var f = System.IO.Path.GetFileName(this.GDriveFolder);
        //         if(drive.GetFolder(p, f) == null){                
        //             try{
        //                 Output.Instance.Write(string.Format("Creating folder structure in '{0}': ", this.GDriveFolder)); 
        //                 drive.CreateFolder(p, f);
        //                 Output.Instance.WriteResponse();
        //             }
        //             catch(Exception ex){
        //                 Output.Instance.WriteResponse(ex.Message);
        //             } 
        //         } 

        //         var uri = string.Empty;
        //         try{
        //             Output.Instance.Write("Retreiving remote file URI from student's assignment: "); 
        //             var file = Directory.GetFiles(this.Path, "*.txt", SearchOption.AllDirectories).FirstOrDefault();    
        //             uri = File.ReadAllLines(file).Where(x => x.Length > 0 && x.StartsWith("http")).FirstOrDefault(); 

        //             if(string.IsNullOrEmpty(uri)) Output.Instance.WriteResponse("Unable to read any URI from the current file.");              
        //             else Output.Instance.WriteResponse();
        //         }
        //         catch(Exception ex){
        //             Output.Instance.WriteResponse(ex.Message);
        //         }

        //         if(!string.IsNullOrEmpty(uri)){            
        //             try{
        //                 Output.Instance.Write("Copying student's remote file to Google Drive's storage: ");                         
        //                 drive.CopyFile(new Uri(uri), this.GDriveFolder, this.Student);                    
        //                 Output.Instance.WriteResponse();
        //             }
        //             catch(Exception ex){
        //                 Output.Instance.WriteResponse(ex.Message);
        //             }    
        //         }             

        //         Output.Instance.UnIndent(); 
        //         Output.Instance.BreakLine();    
        //     }   
        // }
#endregion    
    }
}