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
using System.Text;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
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
        /// The current script IP for single-typed scripts (the same as "îp"); can change during the execution for batch-typed scripts.
        /// </summary>
        public string CurrentIP {
            get{
                return GetVar("current_ip").ToString();
            }

            private set{
                UpdateVar("current_ip", value);               
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

        private bool IsQuestionOpen  {
            get{
                return this.Errors != null;
            }
        } 

        private List<string> Errors {get; set;}

        private Stack<Dictionary<string, object>> Vars {get; set;}  //Variables are scope-delimited

        private Stack<Dictionary<string, object>> Checkers {get; set;}  //Checkers and Connectors are the same within a YAML script, each of them in their scope                       

        public OutputV2 Output {get; private set;}     
#endregion
#region Constructor
        /// <summary>
        /// Creates a new script instance using the given script file.
        /// </summary>
        /// <param name="path">Path to the script file (yaml).</param>
        public ScriptV2(string path){            
            Output = new OutputV2();
            Vars = new Stack<Dictionary<string, object>>();
            Checkers = new Stack<Dictionary<string, object>>();
            ParseScript(path);
        }
#endregion
#region Scope
        /// <summary>
        /// Returns the requested var value.
        /// </summary>
        /// <param name="key">Var name</param>
        /// <returns>Var value</returns>
        private object GetVar(string name, bool compute = true){
            try{
                var value = FindItemWithinScope(Vars, name);                
                return (compute && value != null && value.GetType().Equals(typeof(string)) ? ComputeVarValue(name, value.ToString()) : value);
            }
            catch (ItemNotFoundException){
                throw new VariableNotFoundException($"Undefined variable {name} has been requested.");
            }            
        }

        private void UpdateVar(string name, object value){
            name = name.ToLower();

            if(name.StartsWith("$")){
                //Only update var within upper scopes
                var current = Vars.Pop();  
                name = name.TrimStart('$');
                
                try{ 
                    var found = FindScope(Vars, name);
                    found[name] = value;
                }
                catch (ItemNotFoundException){
                    throw new VariableNotFoundException($"Undefined upper-scope variable {name} has been requested.");
                }  
                finally{ 
                    Vars.Push(current); 
                }  
            }
            else{
                //Create or update var within current scope
                var current = Vars.Peek();
                if(!current.ContainsKey(name)) current.Add(name, null);
                current[name] = value;
            }
           
        }

        private object GetChecker(string name){     
            try{
                return FindItemWithinScope(Checkers, name);
            }      
            catch{
                return new Checkers.LocalShell();
            }            
        }

        private Dictionary<string, object> FindScope(Stack<Dictionary<string, object>> scope, string key){
            object item = null;            
            var visited = new Stack<Dictionary<string, object>>();            

            try{
                //Search the checker by name within scopes
                key = key.ToLower();
                while(item == null && scope.Count > 0){
                    if(scope.Peek().ContainsKey(key)) return scope.Peek();
                    else visited.Push(scope.Pop());
                }

                //Not found
                throw new ItemNotFoundException();
            }
            finally{
                //Undo scope search
                while(visited.Count > 0){
                    scope.Push(visited.Pop());
                }
            }            
        } 

        private object FindItemWithinScope(Stack<Dictionary<string, object>> scope, string key){            
            var found = FindScope(scope, key);
            return found[key.ToLower()];          
        }                
#endregion
#region Parsing
        private void ParseScript(string path){                     
            var root = (YamlMappingNode)LoadYamlFile(path).Documents[0].RootNode;
            ValidateEntries(root, "root", new string[]{"inherits", "name", "ip", "folder", "batch", "vars", "pre", "post", "body"});

            //Scope in            
            Vars.Push(new Dictionary<string, object>());
            
            //Default vars
            Result = null;                                   
            MaxScore = 10f;
            TotalScore = 0f;
            CurrentScore = 0f;
            CurrentQuestion = "0";                        
            CurrentFile = Path.GetFileName(path);
            ExecutionFolder = AppContext.BaseDirectory; 
            CurrentFolder = ParseNode(root, "folder", Path.GetDirectoryName(path), false);
            CurrentIP = ParseNode(root, "ip", "localhost", false);
            ScriptName = ParseNode(root, "name", Regex.Replace(Path.GetFileNameWithoutExtension(path), "[A-Z]", " $0"), false);                        
            
            //Script parsing
            ParseVars(root);
            ParsePre(root);
            ParseBody(root);  
            ParsePost(root);  

            //Scope out
            Vars.Pop();
        }
        
        private void ParseVars(YamlMappingNode root, string node="vars"){           
            if(root.Children.ContainsKey(node)) root = (YamlMappingNode)root.Children[new YamlScalarNode(node)];
            foreach (var item in root.Children){
                var name = item.Key.ToString();                   
                var reserved = new string[]{"script_name", "execution_folder", "current_ip", "current_folder", "current_file", "result", "now"};

                if(reserved.Contains(name)) throw new VariableInvalidException($"The variable name {name} is reserved and cannot be declared.");                                    
                UpdateVar(name, ParseNode(item, false));
            }
        }  

        private void ParsePre(YamlMappingNode root, string node="pre"){
            ForEach(root, node, new string[]{"extract", "restore_db", "upload_gdrive"}, new Action<string, YamlMappingNode>((name, node) => {
                switch(name){
                    case "extract":
                        ValidateEntries(node, name, new string[]{"file", "remove", "recursive"});  

                        var ex_file = ParseNode(node, "file", "*.zip", false);
                        var ex_remove =  ParseNode(node, "remove", false, false);
                        var ex_recursive =  ParseNode(node, "recursive", false, false);
                                                   
                        Extract(ex_file, ex_remove,  ex_recursive);                        
                        break;

                    case "restore_db":
                        ValidateEntries(node, name, new string[]{"file", "db_host", "db_user", "db_pass", "db_name", "override", "remove", "recursive"});     

                        var db_file = ParseNode(node, "file", "*.sql", false);
                        var db_host = ParseNode(node, "db_host", "localhost", false);
                        var db_user = ParseNode(node, "db_user", "postgres", false);
                        var db_pass = ParseNode(node, "db_pass", "postgres", false);
                        var db_name = ParseNode(node, "db_name", ScriptName, false);
                        var db_override = ParseNode(node, "override", false, false);
                        var db_remove = ParseNode(node, "remove", false, false);
                        var db_recursive = ParseNode(node, "recursive", false, false);

                        RestoreDB(db_file, db_host,  db_user, db_pass, db_name, db_override, db_remove, db_recursive);
                        break;

                    case "upload_gdrive":
                        ValidateEntries(node, name, new string[]{"source", "username", "secret", "remote_path", "link", "copy", "remove", "recursive"});     

                        var gd_source = ParseNode(node, "source", "*", false);
                        var gd_user = ParseNode(node, "username", "", false);
                        var gd_secret = ParseNode(node, "secret", AutoCheck.Core.Utils.ConfigFile("gdrive_secret.json"), false);
                        var gd_remote = ParseNode(node, "remote_path",  "\\AutoCheck\\scripts\\{$SCRIPT_NAME}\\", false);
                        var gd_link = ParseNode(node, "link", false, false);
                        var gd_copy = ParseNode(node, "copy", true, false);
                        var gd_remove = ParseNode(node, "remove", false, false);
                        var gd_recursive = ParseNode(node, "recursive", false, false);

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
            //Scope in
            Vars.Push(new Dictionary<string, object>());
            Checkers.Push(new Dictionary<string, object>());

            //Parse the entire body
            ForEach(root, node, new string[]{"vars", "connector", "run", "question"}, new Action<string, YamlMappingNode>((name, node) => {
                switch(name){
                    case "vars":
                        ParseVars(node);                            
                        break;

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

            //Body ends, so total score can be displayed
            Output.Write("TOTAL SCORE: ", ConsoleColor.Cyan);
            Output.Write(Math.Round(TotalScore, 2).ToString(CultureInfo.InvariantCulture), (TotalScore < MaxScore/2 ? ConsoleColor.Red : ConsoleColor.Green));
            Output.BreakLine();

            //Scope out
            Vars.Pop();
            Checkers.Pop();
        }

        private void ParseConnector(YamlMappingNode root){
            //Validation before continuing
            ValidateEntries(root, "connector", new string[]{"type", "name", "arguments"});     

            var type = ParseNode(root, "type", "LOCALSHELL");
            var name = ParseNode(root, "name", type);
                               
            //Getting the connector's assembly (unable to use name + baseType due checker's dynamic connector type)
            Assembly assembly = Assembly.GetExecutingAssembly();
            var assemblyType = assembly.GetTypes().First(t => t.FullName.Equals($"AutoCheck.Checkers.{type}", StringComparison.InvariantCultureIgnoreCase));
            var constructor = GetMethod(assemblyType, assemblyType.Name, ParseArguments(root));            
            Checkers.Peek().Add(name.ToLower(), Activator.CreateInstance(assemblyType, constructor.args));   
        }        

        private void ParseRun(YamlMappingNode root, string parent="body"){
            //Validation before continuing
            var validation = new List<string>(){"connector", "command", "arguments", "expected"};
            if(!parent.Equals("body")) validation.AddRange(new string[]{"caption", "success", "error"});
            ValidateEntries(root, "run", validation.ToArray());     

            //Loading command data            
            var name = ParseNode(root, "connector", "LOCALSHELL");
            var checker = GetChecker(name);
            var command = ParseNode(root, "command", string.Empty);
            if(string.IsNullOrEmpty(command)) throw new ArgumentNullException("command", new Exception("A 'command' argument must be specified within 'run'."));  
            
            //Binding with an existing connector command
            var shellExecuted = false;            
            var arguments = ParseArguments(root);            
            (MethodBase method, object[] args, bool checker) data;
            try{
                //Regular bind (directly to the checker or its inner connector)
                data = GetMethod(checker.GetType(), command, arguments);
            }
            catch(ArgumentInvalidException){       
                //If LocalShell (implicit or explicit) is being used, shell commands can be used directly as "command" attributes.
                shellExecuted = checker.GetType().Equals(typeof(Checkers.LocalShell)) || checker.GetType().BaseType.Equals(typeof(Checkers.LocalShell));  
                if(shellExecuted){                                     
                    if(!arguments.ContainsKey("path")) arguments.Add("path", string.Empty); 
                    arguments.Add("command", command);
                    command = "RunCommand";
                }
                
                //Retry the execution
                data = GetMethod(checker.GetType(), command, arguments);
            }            
                        
            try{
                //Running the command over the connector with the given arguments                
                var result = data.method.Invoke((data.checker ? checker : GetConnectorProperty(checker.GetType()).GetValue(checker)), data.args);                                                 
                var checkerExecuted = result.GetType().Equals(typeof(List<string>));
                
                //Storing the result into the global var
                if(shellExecuted) Result = ((ValueTuple<int, string>)result).Item2; 
                else if(checkerExecuted) Result = string.Join("\r\n", (List<string>)result);
                else Result = result.ToString();
                Result = Result.TrimEnd();  //Remove trailing breaklines...  

                //Matching the data
                var expected = ParseNode(root, "expected", (object)null);  
                var match = (expected == null ? true : MatchesExpected(Result, expected.ToString()));
                var info = $"Expected -> {expected}; Found -> {Result}";                

                //Displaying matching messages
                if(parent.Equals("body") && !match) throw new ResultMismatchException(info);
                else if(!parent.Equals("body")){  
                    var caption = ParseNode(root, "caption", string.Empty);
                    var success = ParseNode(root, "success", "OK");
                    var error = ParseNode(root, "error", "ERROR");
                    
                    if(IsQuestionOpen){                        
                        if(string.IsNullOrEmpty(caption)) throw new ArgumentNullException("caption", new Exception("A 'caption' argument must be provided when running a 'command' using 'expected' within a 'quesion'."));
                        Output.Write($"{caption} ");
                        
                        List<string> errors = null;
                        if(!match){
                            if(shellExecuted || !checkerExecuted) errors = new List<string>(){info}; 
                            else errors = (List<string>)result;
                        }
                       
                        if(errors != null) Errors.AddRange(errors);
                        Output.WriteResponse(errors, success, error);                      
                    }                    
                }              
            }
            catch(ResultMismatchException ex){
                if(parent.Equals("body")) throw;
                else Output.WriteResponse(ex.Message);
            }            
        }

        private void ParseQuestion(YamlMappingNode root){
            //Validation before continuing
            var validation = new List<string>(){"score", "caption", "description", "content"};            
            ValidateEntries(root, "question", validation.ToArray());     
                        
            if(IsQuestionOpen){
                //Opening a subquestion               
                CurrentQuestion += ".1";                
                Output.BreakLine();
            }
            else{
                //Opening a main question
                var parts = CurrentQuestion.Split('.');
                var last = int.Parse(parts.LastOrDefault());
                parts[parts.Length-1] = (last+1).ToString();
                CurrentQuestion = string.Join('.', parts);
            } 

            //Cleaning previous errors
            this.Errors = new List<string>();

            //Loading question data                        
            CurrentScore = ComputeQuestionScore(root);
            var caption = ParseNode(root, "caption", $"Question {CurrentQuestion} [{CurrentScore} {(CurrentScore > 1 ? "points" : "point")}]");
            var description = ParseNode(root, "description", string.Empty);  
            
            //Displaying question caption
            caption = (string.IsNullOrEmpty(description) ? $"{caption}:" : $"{caption} - {description}:");            
            Output.WriteLine(caption, ConsoleColor.Cyan);
            Output.Indent();                        

            //Scope in
            Vars.Push(new Dictionary<string, object>());
            Checkers.Push(new Dictionary<string, object>());

            //Running question content
            var subquestion = ContainsSubquestion(root);
            ForEach(root, "content", new string[]{"connector", "run", "question"}, new Action<string, YamlMappingNode>((name, node) => {
                switch(name){
                    case "connector":
                        ParseConnector(node);                            
                        break;

                    case "run":
                        ParseRun(node, (subquestion ? "body" : "question"));
                        break;

                    case "question":                        
                        ParseQuestion(node);
                        break;
                } 
            }));

            //Scope out
            Vars.Pop();
            Checkers.Pop();

            //Closing the question                            
            Output.UnIndent();                                    
            Output.BreakLine();

            if(!subquestion){                
                if(Errors.Count == 0) Success += CurrentScore;
                else Fails += CurrentScore;
            }    

            float total = Success + Fails;
            TotalScore = (total > 0 ? (Success / total)*MaxScore : 0);      
            Errors = null;           
        }

        private T ParseNode<T>(YamlMappingNode root, string node, T @default, bool compute=true){
            try{
                if(!root.Children.ContainsKey(node)){
                    if(@default == null) return @default;
                    else if(@default.GetType().Equals(typeof(string))) return (T)ParseNode(new KeyValuePair<YamlNode, YamlNode>(node, @default.ToString()), compute); 
                    else return @default;
                }
                return (T)ParseNode(root.Children.Where(x => x.Key.ToString().Equals(node)).FirstOrDefault(), compute);                    
            }
            catch(InvalidCastException){
                return @default;
            }            
        }

        private object ParseNode(KeyValuePair<YamlNode, YamlNode> node, bool compute=true){            
            var name = node.Key.ToString();
            object value = node.Value.ToString();

            value = ComputeTypeValue(node.Value.Tag, node.Value.ToString());
            
            if(value.GetType().Equals(typeof(string))){                
                //Always check if the computed value requested is correct, otherwise throws an exception
                var computed = ComputeVarValue(node.Key.ToString(), value.ToString());
                if(compute) value = computed;
            } 
            return value;
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
        
        private (MethodBase method, object[] args, bool checker) GetMethod(Type type, string method, Dictionary<string, object> arguments, bool checker = true){            
            List<object> args = null;
            var constructor = method.Equals(type.Name);                        
            var methods = (constructor ? (MethodBase[])type.GetConstructors() : (MethodBase[])type.GetMethods());            

            //Getting the constructor parameters in order to bind them with the YAML script ones
            foreach(var info in methods.Where(x => x.Name.Equals((constructor ? ".ctor" : method), StringComparison.InvariantCultureIgnoreCase) && x.GetParameters().Count() == arguments.Count)){                                
                args = new List<object>();
                foreach(var param in info.GetParameters()){
                    if(arguments.ContainsKey(param.Name) && (arguments[param.Name].GetType() == param.ParameterType)) args.Add(arguments[param.Name]);
                    else if(arguments.ContainsKey(param.Name) && param.ParameterType.IsEnum && arguments[param.Name].GetType().Equals(typeof(string))) args.Add(Enum.Parse(param.ParameterType, arguments[param.Name].ToString()));
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
            else return GetMethod(GetConnectorProperty(type).PropertyType, method, arguments, false);                     
        }

        private PropertyInfo GetConnectorProperty(Type input){
            //Warning: due polimorfism, there's not only a single property called "Connector" within a Checker
            var conns = input.GetProperties().Where(x => x.Name.Equals("Connector")).ToList();
            if(conns.Count == 0) throw new PorpertyNotFoundException($"Unable to find the property 'Connector' for the given '{input.Name}'");

            //SingleOrDefault to rise exception if not unique (should never happen?)
            if(conns.Count() > 1) return conns.Where(x => x.DeclaringType.Equals(x.ReflectedType)).SingleOrDefault();  
            else return conns.FirstOrDefault();
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
                    else{                         
                        replace = string.Format(CultureInfo.InvariantCulture, "{0}", GetVar(replace.ToLower()));
                        if(!string.IsNullOrEmpty(regex)){
                            try{
                                replace = Regex.Match(replace, regex).Value;
                            }
                            catch{
                                throw new RegexInvalidException($"Invalid regular expression defined inside the variable '{name}'.");
                            }
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

        private float ComputeQuestionScore(YamlMappingNode root){        
            var score = 0f;
            var subquestion = false;

            ForEach(root, "content", new string[]{"connector", "run", "question"}, new Action<string, YamlMappingNode>((name, node) => {
                switch(name){                   
                    case "question":
                        subquestion = true;
                        score += ComputeQuestionScore(node);
                        break;
                } 
            }));

            if(!subquestion) return ParseNode(root, "score", 1f, false);
            else return score;
        }

        private bool ContainsSubquestion(YamlMappingNode root){                    
            var subquestion = false;

            ForEach(root, "content", new string[]{"connector", "run", "question"}, new Action<string, YamlMappingNode>((name, node) => {
                switch(name){                   
                    case "question":
                        subquestion = true;
                        return;                        
                } 
            }));

            return subquestion;
        }

        private bool MatchesExpected(string current, string expected){
            var match = false;
            var comparer = Core.Operator.EQUALS;                        

            if(expected.StartsWith("<=")){ 
                comparer = Operator.LOWEREQUALS;
                expected = expected.Substring(2).Trim();
            }
            else if(expected.StartsWith(">=")){
                comparer = Operator.GREATEREQUALS;
                expected = expected.Substring(2).Trim();
            }
            else if(expected.StartsWith("<")){ 
                comparer = Operator.LOWER;
                expected = expected.Substring(1).Trim();
            }
            else if(expected.StartsWith(">")){
                comparer = Operator.GREATER;                        
                expected = expected.Substring(1).Trim();
            }
            else if(expected.StartsWith("LIKE")){
                comparer = Operator.LIKE;
                expected = expected.Substring(4).Trim();
            }
            else if(expected.StartsWith("%") || expected.EndsWith("%")){
                comparer = Operator.LIKE;
            }
            else if(expected.StartsWith("<>") || expected.StartsWith("!=")){ 
                comparer = Operator.NOTEQUALS;
                expected = expected.Substring(2).Trim();
            }

            if(comparer == Operator.LIKE){
                if(expected.StartsWith('%') && expected.EndsWith('%')){
                    expected = expected.Trim('%');
                    match = current.Contains(expected);
                }
                else if(expected.StartsWith('%')){
                    expected = expected.Trim('%');
                    match = current.EndsWith(expected);
                }
                else if(expected.EndsWith('%')){
                    expected = expected.Trim('%');
                    match = current.StartsWith(expected);
                }
            }
            else{
                match = comparer switch
                {
                    Operator.EQUALS => current.Equals(expected),
                    Operator.NOTEQUALS => !current.Equals(expected),
                    Operator.LOWEREQUALS => (float.Parse(current) <= float.Parse(expected)),
                    Operator.GREATEREQUALS => (float.Parse(current) >= float.Parse(expected)),
                    Operator.LOWER => (float.Parse(current) < float.Parse(expected)),
                    Operator.GREATER => (float.Parse(current) > float.Parse(expected)),
                    _ => throw new NotSupportedException()
                };
            }
            
            
            return match;
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

        private YamlStream LoadYamlFile(string path){     
            if(!File.Exists(path)) throw new FileNotFoundException(path);
            
            var yaml = new YamlStream();            
            try{
                yaml.Load(new StringReader(File.ReadAllText(path)));
            }
            catch(Exception ex){
                throw new DocumentInvalidException("Unable to parse the YAML document, see inner exception for further details.", ex);
            }

            var root = (YamlMappingNode)yaml.Documents[0].RootNode;
            var inherits = ParseNode(root, "inherits", string.Empty);

            if(string.IsNullOrEmpty(inherits)) return yaml;
            else {
                var file = Path.Combine(Path.GetDirectoryName(path), inherits);
                var parent = LoadYamlFile(file);
                return MergeYamlFiles(parent, yaml);
            }            
        }     

        private YamlStream MergeYamlFiles(YamlStream original, YamlStream inheritor){
            //Source: https://stackoverflow.com/a/53414534
            
            var left = (YamlMappingNode)original.Documents[0].RootNode;
            var right = (YamlMappingNode)inheritor.Documents[0].RootNode; 

            foreach(var child in right.Children){
                if(!left.Children.ContainsKey(child.Key.ToString())) left.Children.Add(child.Key, child.Value);
                else left.Children[child.Key] = child.Value;                    
            }

            return original;            
        }
#endregion
#region ZIP
        private void Extract(string file, bool remove, bool recursive){
            Output.WriteLine("Extracting files: ");
            Output.Indent();

            //CurrentFolder and CurrentFile may be modified during execution
            var originalCurrentFile = CurrentFile;
            var originalCurrentFolder = CurrentFolder;

            try{
                string[] files = Directory.GetFiles(CurrentFolder, file, (recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));                    
                if(files.Length == 0) Output.WriteLine("Done!");                    
                else{
                    foreach(string zip in files){                        
                        CurrentFile = Path.GetFileName(zip);
                        CurrentFolder = Path.GetDirectoryName(zip);

                        try{
                            Output.Write($"Extracting the file ~{zip}... ", ConsoleColor.DarkYellow);
                            Utils.ExtractFile(zip);
                            Output.WriteResponse();
                        }
                        catch(Exception e){
                            Output.WriteResponse($"ERROR {e.Message}");
                            continue;
                        }

                        if(remove){                        
                            try{
                                Output.Write($"Removing the file ~{zip}... ", ConsoleColor.DarkYellow);
                                File.Delete(zip);
                                Output.WriteResponse();
                                Output.BreakLine();
                            }
                            catch(Exception e){
                                Output.WriteResponse($"ERROR {e.Message}");
                                continue;
                            }  
                        }
                    }                                                                  
                }                    
            }
            catch (Exception e){
                Output.WriteResponse(string.Format("ERROR {0}", e.Message));
            }
            finally{    
                Output.UnIndent();
                if(!remove) Output.BreakLine();

                //Restoring original values
                CurrentFile = originalCurrentFile;
                CurrentFolder = originalCurrentFolder;
            }            
        }
#endregion
#region BBDD
        private void RestoreDB(string file, string dbhost, string dbuser, string dbpass, string dbname, bool @override, bool remove, bool recursive){
            Output.WriteLine("Restoring databases: ");
            Output.Indent();

            //CurrentFolder and CurrentFile may be modified during execution
            var originalCurrentFile = CurrentFile;
            var originalCurrentFolder = CurrentFolder;

            try{
                string[] files = Directory.GetFiles(CurrentFolder, file, (recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));                    
                if(files.Length == 0) Output.WriteLine("Done!");                    
                else{
                    foreach(string sql in files){
                        CurrentFile =  Path.GetFileName(sql);
                        CurrentFolder = Path.GetDirectoryName(sql);

                        try{                            
                            //TODO: parse DB name to avoid forbidden chars.
                            var parsedDbName = Path.GetFileName(ComputeVarValue("dbname", dbname)).Replace(" ", "_").Replace(".", "_");
                            Output.WriteLine($"Checking the database ~{parsedDbName}: ", ConsoleColor.DarkYellow);      
                            Output.Indent();

                            using(var db = new Connectors.Postgres(dbhost, parsedDbName, dbuser, dbpass)){
                                if(!@override && db.ExistsDataBase()) Output.WriteLine("The database already exists, skipping!"); 
                                else{
                                    if(@override && db.ExistsDataBase()){                
                                        try{
                                            Output.Write("Dropping the existing database: "); 
                                            db.DropDataBase();
                                            Output.WriteResponse();
                                        }
                                        catch(Exception ex){
                                            Output.WriteResponse(ex.Message);
                                        } 
                                    } 

                                    try{
                                        Output.Write($"Restoring the database using the file {sql}... ", ConsoleColor.DarkYellow);
                                        db.CreateDataBase(sql);
                                        Output.WriteResponse();
                                    }
                                    catch(Exception ex){
                                        Output.WriteResponse(ex.Message);
                                    }
                                }
                            }
                        }
                        catch(Exception e){
                            Output.WriteResponse($"ERROR {e.Message}");
                            continue;
                        }

                        if(remove){                        
                            try{
                                Output.Write($"Removing the file ~{sql}... ", ConsoleColor.DarkYellow);
                                File.Delete(sql);
                                Output.WriteResponse();
                            }
                            catch(Exception e){
                                Output.WriteResponse($"ERROR {e.Message}");
                                continue;
                            }
                        }

                        Output.UnIndent();
                        Output.BreakLine();
                    }                                                                  
                }                    
            }
            catch (Exception e){
                Output.WriteResponse(string.Format("ERROR {0}", e.Message));
            }
            finally{    
                Output.UnIndent();
                
                //Restoring original values
                CurrentFile = originalCurrentFile;
                CurrentFolder = originalCurrentFolder;
            }    
        } 
#endregion
#region Google Drive
        private void UploadGDrive(string source, string user, string secret, string remoteFolder, bool link, bool copy, bool remove, bool recursive){            
            Output.WriteLine("Uploading files to Google Drive: ");
            Output.Indent();

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
                        if(files.Length == 0) Output.WriteLine("Done!");         

                        foreach(var file in files)
                            UploadGDriveFile(drive, file, remoteFolder, link, copy, remove);
                    }
                }                                 
            }
            catch (Exception e){
                Output.WriteResponse(string.Format("ERROR {0}", e.Message));
            }
            finally{    
                Output.UnIndent();

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

                Output.WriteLine($"Checking the local file ~{Path.GetFileName(localFile)}: ", ConsoleColor.DarkYellow);      
                Output.Indent();                

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
                    Output.Write($"Creating folder structure in ~'{remoteFolder}': ", ConsoleColor.Yellow); 
                    drive.CreateFolder(filePath, fileFolder);
                    Output.WriteResponse();                
                } 
                filePath = Path.Combine(filePath, fileFolder);

                if(link){
                    var content = File.ReadAllText(localFile);
                    //Regex source: https://stackoverflow.com/a/6041965
                    foreach(Match match in Regex.Matches(content, "(http|ftp|https)://([\\w_-]+(?:(?:\\.[\\w_-]+)+))([\\w.,@?^=%&:/~+#-]*[\\w@?^=%&/~+#-])?")){
                        var uri = new Uri(match.Value);

                        if(copy){
                            try{
                                Output.Write($"Copying the file from external Google Drive's account to the own one... ");
                                drive.CopyFile(uri, filePath, fileName);
                                Output.WriteResponse();
                            }
                            catch{
                                Output.WriteResponse(string.Empty);
                                copy = false;   //retry with download-reload method if fails
                            }
                        }

                        if(!copy){
                            //download and reupload       
                            Output.Write($"Downloading the file from external sources and uploading to the own Google Drive's account... ");

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
                            Output.WriteResponse();
                        }                                                       
                    }
                }
                else{
                    Output.Write($"Uploading the local file to the own Google Drive's account... ");
                    drive.CreateFile(localFile, filePath, fileName);
                    Output.WriteResponse();                        
                }

                if(remove){
                    Output.Write($"Removing the local file... ");
                    File.Delete(localFile);
                    Output.WriteResponse();       
                } 
            }
            catch (Exception ex){
                Output.WriteResponse(ex.Message);
            } 
            finally{    
                Output.UnIndent();

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
                
                if(files.Length == 0 && folders.Length == 0) Output.WriteLine("Done!");                       
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
                            Output.Write($"Removing the local folder... ");
                            Directory.Delete(localPath);    //not-recursive delete request, should be empty, otherwise something went wrong!
                            Output.WriteResponse();       
                        } 
                    }
                }                               
            }
            catch (Exception e){
                Output.WriteResponse(string.Format("ERROR {0}", e.Message));
            }
            finally{    
                Output.UnIndent();

                //Restoring original values
                CurrentFile = originalCurrentFile;
                CurrentFolder = originalCurrentFolder;
            }    
        }                
#endregion    
    }
}