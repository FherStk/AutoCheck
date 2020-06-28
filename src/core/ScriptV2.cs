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
using System.Net;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using YamlDotNet.RepresentationModel;
using AutoCheck.Exceptions;


namespace AutoCheck.Core{
    //TODO: This will be the new Script (without V2)
    public class ScriptV2{
#region Attributes
        /// <summary>
        /// The current script name defined within the YAML file, otherwise the YAML file name.
        /// </summary>
        public string ScriptName {
            get{
                return GetVar("script_name").ToString();
            }

            private set{
                UpdateVar("script_name", value);                
            }
        }

        /// <summary>
        /// The current script execution folder defined within the YAML file, otherwise the YAML file's folder.
        /// </summary>
        public string ExecutionFolder {
            get{
                return GetVar("execution_folder").ToString();
            }

            private set{
                UpdateVar("execution_folder", value);               
            }
        }

        /// <summary>
        /// The current script folder for single-typed scripts (the same as "folder"); can change during the execution for batch-typed scripts with the folder used to extract, restore a database, etc.
        /// </summary>
        public string CurrentFolder {
            get{
                return GetVar("current_folder").ToString();
            }

            private set{
                UpdateVar("current_folder", value);               
            }
        }

        /// <summary>
        /// The current script file for single-typed scripts; can change during the execution for batch-typed scripts with the file used to extract, restore a database, etc.
        /// </summary>
        public string CurrentFile {
            get{
                return GetVar("current_file").ToString();
            }

            private set{
                UpdateVar("current_file", value);                
            }
        }

        /// <summary>
        /// The current question (and subquestion) number (1, 2, 2.1, etc.)
        /// </summary>
        public string CurrentQuestion {
            get{
                return GetVar("current_question").ToString();
            }

            private set{
                UpdateVar("current_question", value);                
            }
        }
        
        /// <summary>
        /// Last executed command's result.
        /// </summary>
        /// <value></value>
        public string Result {
            get{ 
                var res = GetVar("result");
                return res == null ? null : res.ToString();
            }

            private set{
                UpdateVar("result", value);                
            }
        }

        /// <summary>
        /// The current datetime.  
        /// </summary>
        public string Now {
            get{
                return DateTime.Now.ToString();
            }
        }

        /// <summary>
        /// Returns if there's an open question in progress.
        /// </summary>
        public bool IsQuestionOpen  {
            get{
                return this.Errors != null;
            }
        } 

        /// <summary>
        /// The current question (and subquestion) score
        /// </summary>
        public float CurrentScore {
            get{
                return (float)GetVar("current_score");
            }

            private set{
                UpdateVar("current_score", value);                
            }
        }

        /// <summary>
        /// Maximum score possible
        /// </summary>
        public float MaxScore {
            get{
                return (float)GetVar("max_score");
            }

            private set{
                UpdateVar("max_score", value);                
            }
        }
        
        /// <summary>
        /// The accumulated score (over 10 points), which will be updated on each CloseQuestion() call.
        /// </summary>
        public float TotalScore {
            get{
                return (float)GetVar("total_score");
            }

            private set{
                UpdateVar("total_score", value);                
            }
        }

        private float Success {get; set;}
        
        private float Fails {get; set;}

        private List<string> Errors {get; set;}

        private Dictionary<string, object> Vars {get; set;}

        public Stack<Dictionary<string, object>> Checkers {get; private set;}  //Checkers and Connectors are the same within a YAML script, each of them in their scope
        
        /// <summary>
        /// Returns the requested var value.
        /// </summary>
        /// <param name="key">Var name</param>
        /// <returns>Var value</returns>
        public object GetVar(string key){
            if(Vars.ContainsKey(key)) return Vars[key];
            else return null;
        }

        private void UpdateVar(string key, object value){
            if(Vars.ContainsKey(key)) Vars.Remove(key);
            if(value != null) Vars.Add(key, value);
        }
#endregion
#region Constructor
        /// <summary>
        /// Creates a new script instance using the given script file.
        /// </summary>
        /// <param name="path">Path to the script file (yaml).</param>
        public ScriptV2(string path){
            if(!File.Exists(path)) throw new FileNotFoundException(path);
            
            Vars = new Dictionary<string, object>();
            Checkers = new Stack<Dictionary<string, object>>();
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
       
            var root = (YamlMappingNode)yaml.Documents[0].RootNode;
            ValidateEntries(root, "root", new string[]{"name", "folder", "inherits", "vars", "pre", "post", "body"});
            
            Result = null;                                   
            MaxScore = 0f;
            TotalScore = 0f;
            CurrentScore = 0f;
            CurrentQuestion = "0";            
            CurrentFile = Path.GetFileName(path);
            CurrentFolder = ParseNode(root, "folder", Path.GetDirectoryName(path));                        
            ScriptName = ParseNode(root, "name", Regex.Replace(Path.GetFileNameWithoutExtension(path), "[A-Z]", " $0"));            
            ExecutionFolder = AppContext.BaseDirectory; 
            
            ParseVars(root);
            ParsePre(root);
            ParseBody(root);                                    
        }
        
        private void ParseVars(YamlMappingNode root, string node="vars"){
            if(root.Children.ContainsKey(node)){
                root = (YamlMappingNode)root.Children[new YamlScalarNode(node)];

                foreach (var item in root.Children){
                    var name = item.Key.ToString();                   
                    var reserved = new string[]{"script_name", "execution_folder", "current_folder", "current_file", "result", "now"};

                    if(reserved.Contains(name)) throw new VariableInvalidException($"The variable name {name} is reserved and cannot be declared.");                
                    if(Vars.ContainsKey(name)) throw new VariableInvalidException($"Repeated variables defined with name '{name}'.");
                    
                    UpdateVar(name, ParseNode(item));
                }
            } 
        }  

        private void ParsePre(YamlMappingNode root, string node="pre"){
            ForEach(root, node, new string[]{"extract", "restore_db", "upload_gdrive"}, new Action<string, YamlMappingNode>((name, node) => {
                switch(name){
                    case "extract":
                        ValidateEntries(node, name, new string[]{"file", "remove", "recursive"});  

                        var ex_file = ParseNode(node, "file", "*.zip");
                        var ex_remove =  ParseNode(node, "remove", false);
                        var ex_recursive =  ParseNode(node, "recursive", false);
                                                   
                        Extract(ex_file, ex_remove,  ex_recursive);                        
                        break;

                    case "restore_db":
                        ValidateEntries(node, name, new string[]{"file", "db_host", "db_user", "db_pass", "db_name", "override", "remove", "recursive"});     

                        var db_file = ParseNode(node, "file", "*.sql");
                        var db_host = ParseNode(node, "db_host", "localhost");
                        var db_user = ParseNode(node, "db_user", "postgres");
                        var db_pass = ParseNode(node, "db_pass", "postgres");
                        var db_name = ParseNode(node, "db_name", ScriptName);
                        var db_override = ParseNode(node, "override", false);
                        var db_remove = ParseNode(node, "remove", false);
                        var db_recursive = ParseNode(node, "recursive", false);

                        RestoreDB(db_file, db_host,  db_user, db_pass, db_name, db_override, db_remove, db_recursive);
                        break;

                    case "upload_gdrive":
                        ValidateEntries(node, name, new string[]{"source", "username", "secret", "remote_path", "link", "copy", "remove", "recursive"});     

                        var gd_source = ParseNode(node, "source", "*");
                        var gd_user = ParseNode(node, "username", "");
                        var gd_secret = ParseNode(node, "secret", AutoCheck.Core.Utils.ConfigFile("gdrive_secret.json"));
                        var gd_remote = ParseNode(node, "remote_path",  "\\AutoCheck\\scripts\\{$SCRIPT_NAME}\\");
                        var gd_link = ParseNode(node, "link", false);
                        var gd_copy = ParseNode(node, "copy", true);
                        var gd_remove = ParseNode(node, "remove", false);
                        var gd_recursive = ParseNode(node, "recursive", false);

                        if(string.IsNullOrEmpty(gd_user)) throw new ArgumentInvalidException("The 'username' argument must be provided when using the 'upload_gdrive' feature.");                        
                        UploadGDrive(gd_source, gd_user, gd_secret, gd_remote, gd_link, gd_copy, gd_remove, gd_recursive);
                        break;
                } 
            }));
        }    
        
        private void ParsePost(YamlMappingNode root){
            //Maybe something diferent will be done in a near future? Who knows... :p
            ParsePre(root, "post");
        }

        private void ParseBody(YamlMappingNode root, string node="body"){
            Checkers.Push(new Dictionary<string, object>());

            ForEach(root, node, new string[]{"connector", "run", "question"}, new Action<string, YamlMappingNode>((name, node) => {
                switch(name){
                    case "connector":
                        ParseConnector(node);                            
                        break;

                    case "run":
                        ParseRun(node);
                        break;

                    case "question":
                        ParseQuestion(node);
                        break;
                } 
            }));
        }

        private void ParseConnector(YamlMappingNode root){
            //Validation before continuing
            ValidateEntries(root, "connector", new string[]{"type", "name", "arguments"});     

            var type = ParseNode(root, "type", "LOCALSHELL");
            var name = ParseNode(root, "name", type);
                    
            //Compute the loaded connector arguments (typed or not) and store them as variables, allowing requests within the script (can be useful).
            var arguments = ParseArguments(root);
            foreach(var key in arguments.Keys.ToList())                                
                UpdateVar($"{name}.{key}", arguments[key]); 
           
            //Getting the connector's assembly (unable to use name + baseType due checker's dynamic connector type)
            Assembly assembly = Assembly.GetExecutingAssembly();
            var assemblyType = assembly.GetTypes().First(t => t.FullName.Equals($"AutoCheck.Checkers.{type}", StringComparison.InvariantCultureIgnoreCase));
            var constructor = GetMethod(assemblyType, assemblyType.Name, arguments);            
            Checkers.Peek().Add(name, Activator.CreateInstance(assemblyType, constructor.args));   
        }        

        private void ParseRun(YamlMappingNode root, string parent="body"){
            //Validation before continuing
            var validation = new List<string>(){"connector", "command", "arguments"};
            if(!parent.Equals("body")) validation.AddRange(new string[]{"expected", "success", "error"});
            ValidateEntries(root, "run", validation.ToArray());     

            //Loading command data
            var name = ParseNode(root, "connector", "LOCALSHELL");
            var checker = GetChecker(name);            
            var command = ParseNode(root, "command", string.Empty);
            if(string.IsNullOrEmpty(command)) throw new ArgumentNullException("A 'command' argument must be specified within 'run'.");  
            
            //Binding with an existing connector command
            var arguments = ParseArguments(root);
            (MethodBase method, object[] args, bool checker) data;
            try{
                //Regular bind (directly to the checker or its inner connector)
                data = GetMethod(checker.GetType(), command, arguments);
            }
            catch(ArgumentInvalidException){                
                if(checker.GetType().Equals(typeof(Checkers.LocalShell)) || checker.GetType().BaseType.Equals(typeof(Checkers.LocalShell))){                 
                    //If LocalShell (implicit or explicit) is being used, shell commands can be used directly as "command" attributes.
                    if(!arguments.ContainsKey("path")) arguments.Add("path", string.Empty); 
                    arguments.Add("command", command);
                    command = "RunCommand";
                }
                
                //Retry
                data = GetMethod(checker.GetType(), command, arguments);
            }            

            //Loading expected execution behaviour
            var expected = ParseNode(root, "expected", string.Empty);
            var success = ParseNode(root, "success", "OK");
            var error = ParseNode(root, "error", "ERROR\n{$RESULT}");
            
            //Running the command over the connector with the given arguments
            try{
                var result = data.method.Invoke((data.checker ? checker : checker.GetType().GetProperty("Connector").GetValue(checker)), data.args); 
                if(result.GetType().Equals(typeof(ValueTuple<int, string>))) Result = ((ValueTuple<int, string>)result).Item2; //Comming from LocalShell's RunCommand
                else if(result.GetType().Equals(typeof(List<string>))) Result = string.Join("\r\n", (List<string>)result); //Comming from some Checker's method
                else Result = result.ToString();
                
                if(!parent.Equals("body")){
                    //TODO: 
                    //  1. comparisson type: regex for strings or SQL like (direct value means equals)                 
                    //  2. Compare the output with the expected one   
                    //  3. Print success or error (EvalQuestion content)
                }                
            }
            catch(Exception ex){
                if(!parent.Equals("body")){
                    Output.Instance.WriteResponse(ex.Message);
                }
            }
        }

        private T ParseNode<T>(YamlMappingNode root, string node, T @default, bool compute = false){
            if(!root.Children.ContainsKey(node)) return @default;    
            return (T)ParseNode(root.Children.Where(x => x.Key.ToString().Equals(node)).FirstOrDefault(), compute);                    
        }

        private object ParseNode(KeyValuePair<YamlNode, YamlNode> node, bool compute = false){            
            var name = node.Key.ToString();
            object value = node.Value.ToString();

            value = ComputeTypeValue(node.Value.Tag, node.Value.ToString());
            if(compute && value.GetType().Equals(typeof(string))) value = ComputeVarValue(node.Key.ToString(), value.ToString());
            return value;
        }

        private void ParseQuestion(YamlMappingNode root){
            //Validation before continuing
            var validation = new List<string>(){"score", "caption", "description", "content"};            
            ValidateEntries(root, "question", validation.ToArray());     
            
            this.Errors = new List<string>();
            if(IsQuestionOpen){
                //Opening a subquestion               
                CurrentQuestion += ".1";                
                Output.Instance.BreakLine();
            }
            else{
                //Opening a main question
                var parts = CurrentQuestion.Split('.');
                var last = int.Parse(parts.LastOrDefault());
                parts[parts.Length-1] = (last+1).ToString();
            } 

            //Loading question data            
            CurrentScore = ParseNode(root, "score", 1f);  
            var caption = ParseNode(root, "caption", $"Question {CurrentQuestion} [{CurrentScore} {(CurrentScore > 1 ? "points" : "point")}]");
            var description = ParseNode(root, "description", string.Empty);  
            
            //Displaying question caption
            caption = (string.IsNullOrEmpty(description) ? $"{caption}:" : $"{caption} - {description}:");            
            Output.Instance.WriteLine(string.Format("{0}:", caption), ConsoleColor.Cyan);
            Output.Instance.Indent();                        

            //Running question content
            ForEach(root, "content", new string[]{"connector", "run", "question"}, new Action<string, YamlMappingNode>((name, node) => {
                switch(name){
                    case "connector":
                        ParseConnector(node);                            
                        break;

                    case "run":
                        ParseRun(node, "question");
                        break;

                    case "question":
                        ParseQuestion(node);
                        break;
                } 
            }));

            //Closing the question                            
            Output.Instance.UnIndent();                                    
            Output.Instance.BreakLine();
                            
            if(this.Errors.Count == 0) this.Success += this.CurrentScore;
            else this.Fails += this.CurrentScore;
                                    
            float total = Success + Fails;
            this.TotalScore = (total > 0 ? (Success / total)*this.MaxScore : 0);      
            this.Errors = null;
        }
        
        private Dictionary<string, object> ParseArguments(YamlMappingNode root){            
            var arguments =  new Dictionary<string, object>();

            //Load the connector argument list (typed or not)
            if(root.Children.ContainsKey("arguments")){
                if(root.Children["arguments"].GetType() == typeof(YamlScalarNode)){                    
                    foreach(var item in root.Children["arguments"].ToString().Split("--").Skip(1)){
                        var input = item.Trim(' ').Split(" ");   
                        var name = input[0].TrimStart('-');
                        var value = ComputeVarValue(name, input[1]);
                        arguments.Add(name, value);                        
                    }
                }
                else{
                    ForEach(root, "arguments", new Action<string, YamlScalarNode>((name, node) => {
                        var value = ComputeTypeValue(node.Tag, node.Value);
                        if(value.GetType().Equals(typeof(string))) value = ComputeVarValue(name, value.ToString());
                        arguments.Add(name, value);
                    }));
                } 
            }

            return arguments;
        }

        private object GetChecker(string name){
            /*
                Connector scope definition
                1. body
                    1.1 question
                        1.1.1 content
                        1.1.2 question (recursive)
            */            
            
            object chcker = null;
            var visited = new Stack<Dictionary<string, object>>();

            //Search the checker by name within scopes
            while(chcker == null && Checkers.Count > 0){
                if(Checkers.Peek().ContainsKey(name)) chcker = Checkers.Peek()[name];
                else visited.Push(Checkers.Pop());
            }

            //Undo scope search
            while(visited.Count > 0){
                Checkers.Push(visited.Pop());
            }

            if(chcker == null) chcker = new Checkers.LocalShell();
            return chcker;
        }

        private (MethodBase method, object[] args, bool checker) GetMethod(Type type, string method, Dictionary<string, object> arguments, bool checker = true){            
            List<object> args = null;
            var constructor = method.Equals(type.Name);                        
            var methods = (constructor ? (MethodBase[])type.GetConstructors() : (MethodBase[])type.GetMethods());            

            //Getting the constructor parameters in order to bind them with the YAML script ones
            foreach(var info in methods.Where(x => x.Name.Equals((constructor ? ".ctor" : method), StringComparison.InvariantCultureIgnoreCase) && x.GetParameters().Count() == arguments.Count)){                                
                args = new List<object>();
                foreach(var param in info.GetParameters()){
                    if(arguments.ContainsKey(param.Name) && arguments[param.Name].GetType() == param.ParameterType) args.Add(arguments[param.Name]);
                    else{
                        args = null;
                        break;
                    } 
                }

                //Not null means that all the constructor parameters has been succesfully binded
                if(args != null) return (info, args.ToArray(), checker);
            }
            
            //If ends is because no bind has been found, look for the inner Checker's Connector instance (if possible).
            if(!checker) throw new ArgumentInvalidException($"Unable to find any {(constructor ? "constructor" : "method")} for the Connector '{type.Name}' that matches with the given set of arguments.");                                                
            else{
                //Warning: due polimorfism, there's not only a single property called "Connector" within a Checker
                var conns = type.GetProperties().Where(x => x.Name.Equals("Connector")).ToList();
                var connType = conns.FirstOrDefault().PropertyType;
                if(conns.Count() > 1) connType = conns.Where(x => x.DeclaringType.Equals(x.ReflectedType)).SingleOrDefault().PropertyType;  //SingleOrDefault to rise exception if not unique (should never happen?)

                return GetMethod(connType, method, arguments, false);                     
            } 
        }

        private void ValidateEntries(YamlMappingNode root, string parent, string[] expected){
            foreach (var entry in root.Children)
            {                
                var current = entry.Key.ToString().ToLower();
                if(!expected.Contains(current)) throw new DocumentInvalidException($"Unexpected value '{current}' found within '{parent}'.");              
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

                    if(string.IsNullOrEmpty(regex)) replace = GetVar(replace.ToLower()).ToString();
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

        private object ComputeTypeValue(string tag, string value){
            if(string.IsNullOrEmpty(tag)) return value;
            else{
                //Source: https://yaml.org/spec/1.2/spec.html#id2804923
                var type = tag.Split(':').LastOrDefault();
                return type switch
                {
                    "int"   => int.Parse(value, CultureInfo.InvariantCulture),
                    "float" => float.Parse(value, CultureInfo.InvariantCulture),
                    "bool"  => bool.Parse(value),
                    "str"   => value,                    
                    _       => throw new InvalidCastException($"Unable to cast the value '{value}' using the YAML tag '{tag}'."),
                };
            }            
        }

        private void ForEach<T>(YamlMappingNode root, string node, Action<string, T> action) where T: YamlNode{
            ForEach(root, node, null, new Action<string, T>((name, node) => {
                action.Invoke(name, (T)node);
            }));
        }

        private void ForEach<T>(YamlMappingNode root, string node, string[] expected, Action<string, T> action) where T: YamlNode{
            //TODO: loop through YAML nodes (like Pre and Body) and execute the given delegate for each children found.        
            if(root.Children.ContainsKey(node)){ 
                var tmp = root.Children[new YamlScalarNode(node)];
                var list = new List<YamlMappingNode>();

                if(tmp.GetType() == typeof(YamlSequenceNode)) list = ((YamlSequenceNode)tmp).Cast<YamlMappingNode>().ToList();
                else if(tmp.GetType() == typeof(YamlMappingNode)) list.Add((YamlMappingNode)tmp);
                else if(tmp.GetType() == typeof(YamlScalarNode)) return;    //no children to loop through

                //Loop through found items and childs
                foreach (var item in list)
                {
                    if(expected != null && expected.Length > 0) 
                        ValidateEntries(item, node, expected);

                    foreach (var child in item.Children){  
                        var name = child.Key.ToString();   
                        
                        try{
                            if(typeof(T) == typeof(YamlMappingNode)) action.Invoke(name, (T)item.Children[new YamlScalarNode(name)]);
                            else if(typeof(T) == typeof(YamlScalarNode)) action.Invoke(name, (T)child.Value);
                            else throw new InvalidCastException();
                        }
                        catch(InvalidCastException){
                            action.Invoke(name, (T)Activator.CreateInstance(typeof(T)));
                        }
                    }
                }
            }
        }
#endregion
#region ZIP
        private void Extract(string file, bool remove, bool recursive){
            Output.Instance.WriteLine("Extracting files: ");
            Output.Instance.Indent();

            //CurrentFolder and CurrentFile may be modified during execution
            var originalCurrentFile = CurrentFile;
            var originalCurrentFolder = CurrentFolder;

            try{
                string[] files = Directory.GetFiles(CurrentFolder, file, (recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));                    
                if(files.Length == 0) Output.Instance.WriteLine("Done!");                    
                else{
                    foreach(string zip in files){                        
                        CurrentFile = Path.GetFileName(zip);
                        CurrentFolder = Path.GetDirectoryName(zip);

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

                //Restoring original values
                CurrentFile = originalCurrentFile;
                CurrentFolder = originalCurrentFolder;
            }            
        }
#endregion
#region BBDD
        private void RestoreDB(string file, string dbhost, string dbuser, string dbpass, string dbname, bool @override, bool remove, bool recursive){
            Output.Instance.WriteLine("Restoring databases: ");
            Output.Instance.Indent();

            //CurrentFolder and CurrentFile may be modified during execution
            var originalCurrentFile = CurrentFile;
            var originalCurrentFolder = CurrentFolder;

            try{
                string[] files = Directory.GetFiles(CurrentFolder, file, (recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));                    
                if(files.Length == 0) Output.Instance.WriteLine("Done!");                    
                else{
                    foreach(string sql in files){
                        CurrentFile =  Path.GetFileName(sql);
                        CurrentFolder = Path.GetDirectoryName(sql);

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
                
                //Restoring original values
                CurrentFile = originalCurrentFile;
                CurrentFolder = originalCurrentFolder;
            }    
        } 
#endregion
#region Google Drive
        private void UploadGDrive(string source, string user, string secret, string remoteFolder, bool link, bool copy, bool remove, bool recursive){            
            Output.Instance.WriteLine("Uploading files to Google Drive: ");
            Output.Instance.Indent();

            //CurrentFolder and CurrentFile may be modified during execution
            var originalCurrentFile = CurrentFile;
            var originalCurrentFolder = CurrentFolder;
                
            //Option 1: Only files within a searchpath, recursive or not, will be uploaded into the same remote folder.
            //Option 2: Non-recursive folders within a searchpath, including its files, will be uploaded into the same remote folder.
            //Option 3: Recursive folders within a searchpath, including its files, will be uploaded into the remote folder, replicating the folder tree.
           
            try{     
                remoteFolder = ComputeVarValue("remoteFolder", remoteFolder.TrimEnd('\\'));
                using(var drive = new Connectors.GDrive(secret, user)){                        
                    if(string.IsNullOrEmpty(Path.GetExtension(source))) UploadGDriveFolder(drive, CurrentFolder, source, remoteFolder, link, copy, recursive, remove);
                    else{
                        var files = Directory.GetFiles(CurrentFolder, source, (recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
                        if(files.Length == 0) Output.Instance.WriteLine("Done!");         

                        foreach(var file in files)
                            UploadGDriveFile(drive, file, remoteFolder, link, copy, remove);
                    }
                }                                 
            }
            catch (Exception e){
                Output.Instance.WriteResponse(string.Format("ERROR {0}", e.Message));
            }
            finally{    
                Output.Instance.UnIndent();

                //Restoring original values
                CurrentFile = originalCurrentFile;
                CurrentFolder = originalCurrentFolder;
            }    
        }
        
        private void UploadGDriveFile(Connectors.GDrive drive, string localFile, string remoteFolder, bool link, bool copy, bool remove){
            //CurrentFolder and CurrentFile may be modified during execution
            var originalCurrentFile = CurrentFile;
            var originalCurrentFolder = CurrentFolder;

            try{                            
                CurrentFile =  Path.GetFileName(localFile);
                CurrentFolder = Path.GetDirectoryName(localFile);

                Output.Instance.WriteLine($"Checking the local file ~{Path.GetFileName(localFile)}: ", ConsoleColor.DarkYellow);      
                Output.Instance.Indent();                

                var fileName = string.Empty;
                var filePath = string.Empty;                                
                if(string.IsNullOrEmpty(Path.GetExtension(remoteFolder))) filePath = remoteFolder;
                else{
                    fileName = Path.GetFileName(remoteFolder);
                    filePath = Path.GetDirectoryName(remoteFolder);                        
                }                
                
                //Remote GDrive folder structure                    
                var fileFolder = Path.GetFileName(filePath);
                filePath = Path.GetDirectoryName(remoteFolder);     
                if(drive.GetFolder(filePath, fileFolder) == null){                
                    Output.Instance.Write($"Creating folder structure in ~'{remoteFolder}': ", ConsoleColor.Yellow); 
                    drive.CreateFolder(filePath, fileFolder);
                    Output.Instance.WriteResponse();                
                } 
                filePath = Path.Combine(filePath, fileFolder);

                if(link){
                    var content = File.ReadAllText(localFile);
                    //Regex source: https://stackoverflow.com/a/6041965
                    foreach(Match match in Regex.Matches(content, "(http|ftp|https)://([\\w_-]+(?:(?:\\.[\\w_-]+)+))([\\w.,@?^=%&:/~+#-]*[\\w@?^=%&/~+#-])?")){
                        var uri = new Uri(match.Value);

                        if(copy){
                            try{
                                Output.Instance.Write($"Copying the file from external Google Drive's account to the own one... ");
                                drive.CopyFile(uri, filePath, fileName);
                                Output.Instance.WriteResponse();
                            }
                            catch{
                                Output.Instance.WriteResponse(string.Empty);
                                copy = false;   //retry with download-reload method if fails
                            }
                        }

                        if(!copy){
                            //download and reupload       
                            Output.Instance.Write($"Downloading the file from external sources and uploading to the own Google Drive's account... ");

                            string local = string.Empty;
                            if(match.Value.Contains("drive.google.com")) local = drive.Download(uri, Path.Combine(AppContext.BaseDirectory, "tmp"));                                        
                            else{
                                using (var client = new WebClient())
                                {                                    
                                    local = Path.Combine(AppContext.BaseDirectory, "tmp");
                                    if(!Directory.Exists(local)) Directory.CreateDirectory(local);

                                    local = Path.Combine(local, uri.Segments.Last());
                                    client.DownloadFile(uri, local);
                                }
                            }
                            
                            drive.CreateFile(local, filePath, fileName);
                            File.Delete(local);
                            Output.Instance.WriteResponse();
                        }                                                       
                    }
                }
                else{
                    Output.Instance.Write($"Uploading the local file to the own Google Drive's account... ");
                    drive.CreateFile(localFile, filePath, fileName);
                    Output.Instance.WriteResponse();                        
                }

                if(remove){
                    Output.Instance.Write($"Removing the local file... ");
                    File.Delete(localFile);
                    Output.Instance.WriteResponse();       
                } 
            }
            catch (Exception ex){
                Output.Instance.WriteResponse(ex.Message);
            } 
            finally{    
                Output.Instance.UnIndent();

                //Restoring original values
                CurrentFile = originalCurrentFile;
                CurrentFolder = originalCurrentFolder;
            }              
        }

        private void UploadGDriveFolder(Connectors.GDrive drive, string localPath, string localSource, string remoteFolder, bool link, bool copy, bool recursive, bool remove){           
            //CurrentFolder and CurrentFile may be modified during execution
            var originalCurrentFile = CurrentFile;
            var originalCurrentFolder = CurrentFolder;

            try{                
                CurrentFolder =  localPath;

                var files = Directory.GetFiles(localPath, localSource, SearchOption.TopDirectoryOnly);
                var folders = (recursive ? Directory.GetDirectories(localPath, localSource, SearchOption.TopDirectoryOnly) : new string[]{});
                
                if(files.Length == 0 && folders.Length == 0) Output.Instance.WriteLine("Done!");                       
                else{
                    foreach(var file in files){
                        //This will setup CurrentFolder and CurrentFile
                        UploadGDriveFile(drive, file, remoteFolder, link, copy, remove);
                    }
                                    
                    if(recursive){
                        foreach(var folder in folders){
                            var folderName = Path.GetFileName(folder);
                            drive.CreateFolder(remoteFolder, folderName);
                            
                            //This will setup CurrentFolder and CurrentFile
                            UploadGDriveFolder(drive, folder, localSource, Path.Combine(remoteFolder, folderName), link, copy, recursive, remove);
                        }

                        if(remove){
                            //Only removes if recursive (otherwise not uploaded data could be deleted).
                            Output.Instance.Write($"Removing the local folder... ");
                            Directory.Delete(localPath);    //not-recursive delete request, should be empty, otherwise something went wrong!
                            Output.Instance.WriteResponse();       
                        } 
                    }
                }                               
            }
            catch (Exception e){
                Output.Instance.WriteResponse(string.Format("ERROR {0}", e.Message));
            }
            finally{    
                Output.Instance.UnIndent();

                //Restoring original values
                CurrentFile = originalCurrentFile;
                CurrentFolder = originalCurrentFolder;
            }    
        }                
#endregion    
#region Scoring                  
        
        
        
        /// <summary>
        /// Adds a correct execution result (usually a checker's method one) for the current opened question, so its value will be computed once the question is closed.
        /// </summary>
        protected void EvalQuestion(){
            EvalQuestion(new List<string>());
        }
        
        /// <summary>
        /// Adds an execution result (usually a checker's method one) for the current opened question, so its value will be computed once the question is closed.
        /// </summary>
        /// <param name="errors">A list of errors, an empty one will be considered as correct, otherwise it will be considered as a incorrect.</param>
        protected void EvalQuestion(List<string> errors){
            if(IsQuestionOpen){
                this.Errors.AddRange(errors);
                Output.Instance.WriteResponse(errors);
            }
        }   
        
        /// <summary>
        /// Prints the score to the output.
        /// </summary>     
        protected void PrintScore(){
            Output.Instance.Write("TOTAL SCORE: ", ConsoleColor.Cyan);
            Output.Instance.Write(Math.Round(TotalScore, 2).ToString(), (TotalScore < MaxScore/2 ? ConsoleColor.Red : ConsoleColor.Green));
            Output.Instance.BreakLine();
        }  
#endregion
    }
}