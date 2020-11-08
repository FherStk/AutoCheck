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
using AutoCheck.Core.Exceptions;
using CopyDetector = AutoCheck.Core.CopyDetectors.Base;
using Operator = AutoCheck.Core.Connectors.Operator;


namespace AutoCheck.Core{    
    public class Script{
#region Vars
        /// <summary>
        /// The current script name defined within the YAML file, otherwise the YAML file name.
        /// </summary>
        protected string ScriptName {
            get{
                return GetVar("script_name").ToString();
            }

            private set{
                UpdateVar("script_name", value);                
            }
        }

        /// <summary>
        /// The current script caption defined within the YAML file.
        /// </summary>
        protected string ScriptCaption {
            get{
                return GetVar("script_caption").ToString();
            }

            private set{
                UpdateVar("script_caption", value);                
            }
        }

        /// <summary>
        /// The current batch caption defined within the YAML file.
        /// </summary>
        protected string BatchCaption {
            get{
                return GetVar("batch_caption").ToString();
            }

            private set{
                UpdateVar("batch_caption", value);                
            }
        }

        /// <summary>
        /// The root app execution folder.
        /// </summary>
        protected string AppFolder {
            get{
                return GetVar("app_folder").ToString();
            }

            private set{                
                try{
                    //Read only
                    GetVar("app_folder");
                    throw new NotSupportedException("Read only");
                }
                catch (ItemNotFoundException){
                    UpdateVar("app_folder", value);     
                }                          
            }
        }

        /// <summary>
        /// The current script execution folder defined within the YAML file, otherwise the YAML file's folder.
        /// </summary>
        protected string ExecutionFolder {
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
        protected string CurrentHost {
            get{
                return GetVar("current_host").ToString();
            }

            private set{
                UpdateVar("current_host", value);               
            }
        }

        /// <summary>
        /// The current script folder for single-typed scripts (the same as "folder"); can change during the execution for batch-typed scripts with the folder used to extract, restore a database, etc.
        /// </summary>
        protected string CurrentFolder {
            get{
                return GetVar("current_folder").ToString();
            }

            private set{
                UpdateVar("current_folder", value);               
            }
        }

        /// <summary>
        /// The host or folder where the script is running on batch mode (CurrentHost or CurrentFolder)
        /// </summary>
        protected string CurrentTarget {
            get{
                return GetVar("current_target").ToString();
            }

            private set{
                UpdateVar("current_target", value);               
            }
        }

        /// <summary>
        /// The current script file for single-typed scripts; can change during the execution for batch-typed scripts with the file used to extract, restore a database, etc.
        /// </summary>
        protected string CurrentFile {
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
        protected string CurrentQuestion {
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
        protected string Result {
            get{ 
                var res = GetVar("result");
                return res == null ? null : res.ToString();
            }

            private set{                
                try{
                    //Only on upper scope (global)
                    GetVar("result");
                    UpdateVar("$result", value);
                }
                catch (ItemNotFoundException){
                    UpdateVar("result", value);
                }       
            }
        }

        /// <summary>
        /// The current datetime.  
        /// </summary>
        protected string Now {
            get{
                return DateTime.Now.ToString();
            }
        }

        /// <summary>
        /// The current question (and subquestion) score
        /// </summary>
        protected float CurrentScore {
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
        protected float MaxScore {
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
        protected float TotalScore {
            get{
                return (float)GetVar("total_score");
            }

            private set{
                UpdateVar("total_score", value);                
            }
        }

        /// <summary>
        /// The current log folder
        /// </summary>
        protected string LogFolder {
            get{
                return (string)GetVar("log_folder");
            }

            private set{
                UpdateVar("log_folder", value);                
            }
        }

        /// <summary>
        /// The current log file name
        /// </summary>
        protected string LogName {
            get{
                return (string)GetVar("log_name");
            }

            private set{
                UpdateVar("log_name", value);                
            }
        }

#endregion
#region Attributes
        /// <summary>
        /// Output instance used to display messages.
        /// </summary>
        public Output Output {get; private set;}   

        private Stack<Dictionary<string, object>> Vars {get; set;}  //Variables are scope-delimited

        private Stack<Dictionary<string, object>> Connectors {get; set;}  //Connectors are scope-delimited

        private float Success {get; set;}
        
        private float Fails {get; set;}         

        private List<string> Errors {get; set;}

        private bool Abort {get; set;}
        
        private bool Skip {get; set;}

        private bool BatchPauseEnabled {get; set;}
        private bool LogFilesEnabled {get; set;}       
#endregion
#region Constructor
        /// <summary>
        /// Creates a new script instance using the given script file.
        /// </summary>
        /// <param name="path">Path to the script file (yaml).</param>
        public Script(string path){            
            Output = new Output();                                    
            Connectors = new Stack<Dictionary<string, object>>();          
            Vars = new Stack<Dictionary<string, object>>();
            
            //Scope in              
            Vars.Push(new Dictionary<string, object>());

            //Default vars (must be ALL declared before the caption could be used, because the user can customize the caption with any of this vars)
            Abort = false;
            Skip = false;
            Result = null;                                   
            MaxScore = 10f;  
            TotalScore = 0f;
            CurrentScore = 0f;
            CurrentQuestion = "0";                       
            AppFolder = Utils.AppFolder;
            ExecutionFolder = AppContext.BaseDirectory.TrimEnd('\\'); 
            CurrentFolder = Path.GetDirectoryName(path);
            CurrentHost = "localhost";
            CurrentTarget = string.Empty;   //NONE till batch mode is running
            CurrentFile = Path.GetFileName(path);
            ScriptName = Regex.Replace(Path.GetFileNameWithoutExtension(path), "[A-Z]", " $0");
            ScriptCaption = "Executing script ~{$SCRIPT_NAME}:";
            BatchCaption = "Running on batch mode for ~{$CURRENT_TARGET}:";
            BatchPauseEnabled = true;
            LogFilesEnabled = false;
            LogFolder =  "{$APP_FOLDER}\\logs\\";
            LogName = "{$SCRIPT_NAME}_{#[^\\\\]+$$CURRENT_FOLDER}";


            //Load the YAML file
            var root = (YamlMappingNode)LoadYamlFile(path).Documents[0].RootNode;
            ValidateChildren(root, "root", new string[]{"inherits", "name", "caption", "host", "folder", "batch", "output", "vars", "pre", "post", "body"});
                    
            //YAML header overridable vars 
            CurrentFolder = ParseChild(root, "folder", CurrentFolder, false);
            CurrentHost = ParseChild(root, "host", CurrentHost, false);
            ScriptName = ParseChild(root, "name", ScriptName, false);
            ScriptCaption = ParseChild(root, "caption", ScriptCaption, false);
            
            //Printing script caption
            Output.WriteLine(ComputeVarValue(ScriptCaption), ConsoleColor.Yellow);
            
            //Preparing script execution
            var script = new Action(() => {   
                //This data must be cleared for each script body execution (batch mode)  
                Success = 0;
                Fails = 0;

                //Running script parts
                if(root.Children.ContainsKey("pre")) ParsePre(root.Children["pre"]);
                if(root.Children.ContainsKey("body")) ParseBody(root.Children["body"]);
                if(root.Children.ContainsKey("post")) ParsePost(root.Children["post"]);
                
                //Preparing the output files and folders
                var lf = LogFolder;
                foreach(char c in Path.GetInvalidPathChars())
                    lf = lf.Replace(c.ToString(), "");

                var ln = LogName;
                foreach(char c in Path.GetInvalidFileNameChars())
                    ln = ln.Replace(c.ToString(), "");
                
                //Writing log output if needed
                string logFile = Path.Combine(lf, $"{ln}.txt");
                if(!Directory.Exists(lf)) Directory.CreateDirectory(lf);
                if(File.Exists(logFile)) File.Delete(logFile);
                
                //Retry if the log file is bussy
                int max = 5;
                int step = 0;                
                Action write = null;
                write = new Action(() => {                    
                    try{
                        File.WriteAllText(logFile, Output.ToArray().LastOrDefault());
                    }
                    catch(IOException){
                        if(step >= max) throw;
                        else {
                            System.Threading.Thread.Sleep((step++)*1000);
                            write.Invoke();
                        }                        
                    }
                });
                
                write.Invoke();
            });
            
            //Vars are shared along, but pre, body and post must be run once for single-typed scripts or N times for batch-typed scripts    
            if(root.Children.ContainsKey("output")) ParseOutput(root.Children["output"]);
            if(root.Children.ContainsKey("vars")) ParseVars(root.Children["vars"]);
            if(root.Children.ContainsKey("batch")) ParseBatch(root.Children["batch"], script);
            else script.Invoke();
            
            //Scope out
            Vars.Pop();
            
        }
#endregion
#region Parsing
        private void ParseOutput(YamlNode node, string current="output", string parent="root"){
            if(node == null || !node.GetType().Equals(typeof(YamlMappingNode))) return;
            
            ValidateChildren((YamlMappingNode)node, current, new string[]{"terminal", "pause", "files"});
            ForEachChild((YamlMappingNode)node, new Action<string, YamlScalarNode>((name, value) => {
                switch(name){
                    case "terminal":
                        if(!ParseNode<bool>(value, true)) Output.SetMode(Output.Mode.SILENT); //Just alter default value (terminal) because testing system uses silent and should not be replaced here                       
                        break;

                    case "pause":
                        BatchPauseEnabled = ParseNode<bool>(value, BatchPauseEnabled);
                        break;
                }               
            }));  

            ForEachChild((YamlMappingNode)node, new Action<string, YamlMappingNode>((name, value) => {
                switch(name){
                    case "files":
                        LogFilesEnabled = ParseChild(value, "enabled", LogFilesEnabled, false);
                        LogFolder = ParseChild(value, "folder", LogFolder, false);
                        LogName = ParseChild(value, "name", LogName, false);
                        break;
                }               
            }));                   
        } 

        private void ParseVars(YamlNode node, string current="vars", string parent="root"){
            if(node == null || !node.GetType().Equals(typeof(YamlMappingNode))) return;
            
            var reserved = new string[]{"script_name", "execution_folder", "current_ip", "current_folder", "current_file", "result", "now"};
            ForEachChild((YamlMappingNode)node, new Action<string, YamlScalarNode>((name, value) => {
                if(reserved.Contains(name)) throw new VariableInvalidException($"The variable name {name} is reserved and cannot be declared.");                                    
                UpdateVar(name, ParseNode(value, false));
            }));                     
        }  
        
        private void ParsePre(YamlNode node, string current="pre", string parent="root"){
            if(node == null || !node.GetType().Equals(typeof(YamlSequenceNode))) return;

            ValidateChildren((YamlSequenceNode)node, current, new string[]{"extract", "restore_db", "upload_gdrive"});            
            ForEachChild((YamlSequenceNode)node, new Action<string, YamlMappingNode>((name, node) => {               
                switch(name){
                    case "extract":
                        ValidateChildren(node, name, new string[]{"file", "remove", "recursive"});                                                                      
                        Extract(
                            ParseChild(node, "file", "*.zip", false), 
                            ParseChild(node, "remove", false, false),  
                            ParseChild(node, "recursive", false, false)
                        );                        
                        break;

                    case "restore_db":
                        ValidateChildren(node, name, new string[]{"file", "db_host", "db_user", "db_pass", "db_name", "override", "remove", "recursive"});     
                        RestoreDB(
                            ParseChild(node, "file", "*.sql", false), 
                            ParseChild(node, "db_host", "localhost", false),  
                            ParseChild(node, "db_user", "postgres", false), 
                            ParseChild(node, "db_pass", "postgres", false), 
                            ParseChild(node, "db_name", ScriptName, false), 
                            ParseChild(node, "override", false, false), 
                            ParseChild(node, "remove", false, false), 
                            ParseChild(node, "recursive", false, false)
                        );
                        break;

                    case "upload_gdrive":
                        ValidateChildren(node, name, new string[]{"source", "account", "secret", "remote_path", "link", "copy", "remove", "recursive"});     
                        UploadGDrive(
                            ParseChild(node, "source", "*", false), 
                            ParseChild(node, "account", AutoCheck.Core.Utils.ConfigFile("gdrive_account.txt"), false), 
                            ParseChild(node, "secret", AutoCheck.Core.Utils.ConfigFile("gdrive_secret.json"), false), 
                            ParseChild(node, "remote_path",  "\\AutoCheck\\scripts\\{$SCRIPT_NAME}\\", false), 
                            ParseChild(node, "link", false, false), 
                            ParseChild(node, "copy", true, false), 
                            ParseChild(node, "remove", false, false), 
                            ParseChild(node, "recursive", false, false)
                        );
                        break;
                } 
            }));
        }    
        
        private void ParsePost(YamlNode node, string current="post", string parent="root"){
            //Maybe something diferent will be done in a near future? Who knows... :p
            ParsePre(node, current, parent);
        }
        
        private void ParseBatch(YamlNode node, Action action, string current="batch", string parent="root"){        
            if(node == null || !node.GetType().Equals(typeof(YamlSequenceNode))) action.Invoke(); 
            else{    
                //Running in batch mode            
                var originalFolder = CurrentFolder;
                var originalIP = CurrentHost;                                          
                var folders = new List<string>();
                var hosts = new List<string>();
                var cpydet = new List<CopyDetector>();

                //Collecting all the folders and IPs
                var batch = (YamlSequenceNode)node;                
                ValidateChildren(batch, current, new string[]{"caption", "copy_detector", "target"}, new string[]{"target"});
                
                //Parsing caption (scalar)
                ForEachChild(batch, new Action<string, YamlScalarNode>((name, node) => { 
                    switch(name){                       
                        case "caption":                            
                            BatchCaption = ParseNode(node, BatchCaption, false);
                            break;
                    }
                }));

                //Parsing targets (seqüence)
                ForEachChild(batch, new Action<string, YamlSequenceNode>((name, node) => { 
                    switch(name){                        
                        case "target":
                            var target = ParseTarget(node, name, current);
                            folders.AddRange(target.folders);
                            hosts.AddRange(target.hosts);
                            break;
                    }
                }));

                //Parsing copy detectors (mapping nodes) which cannot be parsed till all the folders have been requested                
                var br = false;
                Output.Indent();
                ForEachChild(batch, new Action<string, YamlMappingNode>((name, node) => {                     
                    br = true;
                    switch(name){                       
                        case "copy_detector":                            
                            cpydet.AddRange(ParseCopyDetector(node, folders.ToArray(), name, current));
                            break;
                    }                    
                })); 
                Output.UnIndent();                   
                if(br) Output.BreakLine();
                                
                //Executing for each target
                foreach(var f in folders){
                    CurrentFolder = f;
                    CurrentTarget = CurrentFolder;

                    //Printing script caption
                    Output.Indent();
                    Output.WriteLine(ComputeVarValue(BatchCaption), ConsoleColor.Yellow);
                    
                    //Running copy detectors and script body
                    new Action(() => {
                        Output.Indent();
                        
                        var match = false;
                        foreach(var cd in cpydet){                            
                            if(cd != null){
                                match = match || cd.CopyDetected(f);                        
                                if(match) PrintCopies(cd, f);                            
                            }
                        }                        

                        if(!match) action.Invoke();                            

                        Output.UnIndent();
                        Output.BreakLine();

                        //Breaking log into a new file
                        Output.BreakLog();
                        
                        //Pausing if needed, but should not be logged...
                        if(BatchPauseEnabled && Output.GetMode() == Output.Mode.VERBOSE){                               
                            Console.WriteLine("Press any key to continue...");
                            Console.ReadKey();
                            Output.BreakLine();
                        }

                    }).Invoke();

                    Output.UnIndent();
                }

                foreach(var h in hosts){                    
                    CurrentHost = h;
                    CurrentTarget = CurrentHost;
                    
                    //TODO: Run for the given hosts (note that folders and hosts are mutually exclusive).
                }           

                //Restore global data
                CurrentFolder = originalFolder;
                CurrentHost = originalIP;   
                CurrentTarget = string.Empty;   //NONE once batch has ended
                
            }            
        }

        private (string[] folders, string[] hosts) ParseTarget(YamlNode node, string current="target", string parent="batch"){  
            var folders = new List<string>();
            var hosts = new List<string>();
            if(node == null || !node.GetType().Equals(typeof(YamlSequenceNode))) return (folders.ToArray(), hosts.ToArray());
            
            ValidateChildren((YamlSequenceNode)node, current, new string[]{"host", "path", "folder"});
            ForEachChild((YamlSequenceNode)node, new Action<string, YamlScalarNode>((name, node) => { 
                switch(name){
                    case "host":                            
                        //TODO: set of IPs (using mask) same as path but with IPs
                        throw new NotImplementedException();

                    case "folder":
                        folders.Add(ParseNode(node, CurrentFolder));                                                  
                        break;

                    case "path":                            
                        foreach(var folder in Directory.GetDirectories(ParseNode(node, CurrentFolder))) 
                            folders.Add(folder);                                                                                            
                        break;
                }
            }));

            if(folders.Count + hosts.Count == 0) throw new ArgumentNullException("Some targets ('folder', 'path', 'ip') must be defined when using 'batch' mode.");
            return (folders.ToArray(), hosts.ToArray());
        }

        private CopyDetector[] ParseCopyDetector(YamlNode node, string[] folders, string current="copy_detector", string parent="batch"){                        
            var cds = new List<CopyDetector>();            
            if(node == null || !node.GetType().Equals(typeof(YamlMappingNode))) return cds.ToArray();

            var copy = (YamlMappingNode)node;
            ValidateChildren(copy, current, new string[]{"type", "caption", "threshold", "file"}, new string[]{"type"});            

            var threshold = ParseChild(copy, "threshold", 1f, false);
            var file = ParseChild(copy, "file", "*", false);
            var caption = ParseChild(copy, "caption", "Looking for potential copies within ~{#[^\\\\]+$$CURRENT_FOLDER}... ", false);                    
            var type = ParseChild(copy, "type", string.Empty);                                    
            if(string.IsNullOrEmpty(type)) throw new ArgumentNullException(type);

            cds.Add(LoadCopyDetector(type, caption, threshold, file, folders.ToArray()));
            

            return cds.ToArray();
        }

        private void ParseBody(YamlNode node, string current="body", string parent="root"){
            var question = false;
            if(node == null || !node.GetType().Equals(typeof(YamlSequenceNode))) return;

            //Scope in
            Vars.Push(new Dictionary<string, object>());
            Connectors.Push(new Dictionary<string, object>());
            
            ValidateChildren((YamlSequenceNode)node, current, new string[]{"vars", "connector", "run", "question"});
            ForEachChild((YamlSequenceNode)node, new Action<string, YamlMappingNode>((name, node) => {
                if(Abort) return;

                switch(name){
                    case "vars":
                        ParseVars(node, name, current);                            
                        break;

                    case "connector":
                        ParseConnector(node, name, current);                            
                        break;

                    case "run":
                        ParseRun(node, name, current);
                        break;

                    case "question":
                        question = true;
                        ParseQuestion(node, name, current);
                        break;
                } 
            }));
            
            if(Abort){
                Output.BreakLine();
                Output.WriteLine("Aborting execution!", ConsoleColor.Red);
                Output.BreakLine();
                TotalScore = 0;
            }            

            //Body ends, so total score can be displayed
            if(question){
                Output.Write("TOTAL SCORE: ", ConsoleColor.Cyan);
                Output.Write(Math.Round(TotalScore, 2).ToString(CultureInfo.InvariantCulture), (TotalScore < MaxScore/2 ? ConsoleColor.Red : ConsoleColor.Green));
                Output.BreakLine();
            }

            //Scope out
            Vars.Pop();
            Connectors.Pop();
        }

        private void ParseConnector(YamlNode node, string current="connector", string parent="root"){
            if(node == null || !node.GetType().Equals(typeof(YamlMappingNode))) return;

            //Validation before continuing
            var conn = (YamlMappingNode)node;
            ValidateChildren(conn, current, new string[]{"type", "name", "arguments"});                 
            //Loading connector data
            var type = ParseChild(conn, "type", "LOCALSHELL");
            var name = ParseChild(conn, "name", type).ToLower();
                               
            //Getting the connector's assembly (unable to use name + baseType due inheritance between connectors, for example Odoo -> Postgres)
            Assembly assembly = Assembly.GetExecutingAssembly();
            var assemblyType = assembly.GetTypes().First(t => t.FullName.Equals($"AutoCheck.Core.Connectors.{type}", StringComparison.InvariantCultureIgnoreCase));
            var arguments = conn.Children.ContainsKey("arguments") ? ParseArguments(conn.Children["arguments"]) : null;
            var constructor = GetMethod(assemblyType, assemblyType.Name, arguments);   
            
            //Storing instance
            var instance = Activator.CreateInstance(assemblyType, constructor.args);
            var scope = Connectors.Peek();
            if(!scope.ContainsKey(name)) scope.Add(name, null);         
            scope[name] = instance;
        }        
        
        private void ParseRun(YamlNode node, string current="run", string parent="body"){
            if(node == null || !node.GetType().Equals(typeof(YamlMappingNode))) return;
            Exception exception = null;

            //Validation before continuing
            var run = (YamlMappingNode)node;            
            ValidateChildren(run, current, new string[]{"connector", "command", "arguments", "expected", "caption", "success", "error", "onexception", "store"}, new string[]{"command"});                                                     
                       
            //Data is loaded outside the try statement to rise exception on YAML errors
            var name = ParseChild(run, "connector", "LOCALSHELL");     
            var caption = ParseChild(run, "caption", string.Empty);         
            var expected = ParseChild(run, "expected", (object)null);                          
            var command = ParseChild(run, "command", string.Empty);
            var store = ParseChild(run, "store", string.Empty);
            var connector = GetConnector(name);
            var arguments = (run.Children.ContainsKey("arguments") ? ParseArguments(run.Children["arguments"]) : null);                                    

            //Running the command over the connector with the given arguments   
            (object result, bool shellExecuted) data;
            try{                         
                data = InvokeCommand(connector, command, arguments);             
            }
            catch(ArgumentInvalidException){
                //Exception when trying to run the command (command not executed) with invalid arguments, so YAML script is no correct
                throw;
            }
            catch(Exception ex){  
                //Exception on command execution (command executed)                         
                data = (string.Empty, false);
                var onexcept = ParseChild(run, "onexception", string.Empty);

                //onexcept needs caption
                if(string.IsNullOrEmpty(caption) && !string.IsNullOrEmpty(onexcept)) throw new DocumentInvalidException("The 'onexception' argument cannot be used without the 'caption' argument.");
                else if(!string.IsNullOrEmpty(caption) && string.IsNullOrEmpty(onexcept)) onexcept = "ERROR";

                //processing
                switch(onexcept){
                    case "ERROR":                    
                    case "ABORT":
                    case "SKIP":
                        if(onexcept.Equals("ABORT")) Abort = true;
                        else if(onexcept.Equals("SKIP")) Skip = true;

                        //Reflection-call exception is not usefull
                        if(ex.GetType().Equals(typeof(TargetInvocationException))) ex = ex.InnerException;
                        
                        exception = ex;       
                        data.result = ex.Message;
                        expected = string.Empty;

                        while(ex.InnerException != null){
                            ex = ex.InnerException;
                            data.result += $" --> {ex.Message}";
                        }

                        break;

                    case "SUCCESS":
                        data.result = expected;     //forces match
                        break;  

                    case "":
                        throw;

                    default:
                        throw new NotSupportedException();
                }
            }

            //Parsing the result
            if(data.shellExecuted) Result = ((ValueTuple<int, string>)data.result).Item2;
            else if (data.result == null) Result = string.Empty;
            else if(data.result.GetType().IsArray) Result = $"[{string.Join(",", ((Array)data.result).Cast<object>().ToArray())}]";
            else Result = data.result.ToString();            
            
            //Storing the result into "store" and into the global var "Result"
            Result = Result.TrimEnd();
            if(!string.IsNullOrEmpty(store)) UpdateVar(store, Result);

            //Run with no caption will work as silent but will throw an exception on expected missamtch, if no exception wanted, do not use expected. 
            //Run with no caption wont compute within question, computing hidden results can be confuse when reading a report.
            //Running with caption/no-caption but no expected, means all the results will be assumed as OK and will be computed and displayed ONLY if caption is used (excluding unexpected exceptions).
            //Array.ConvertAll<object, string>(data.result, Convert.ToString)
            var info = (exception == null ? $"Expected -> {expected}; Found -> {Result}": $"{exception.GetType().Name}: {Result}" );
            var match = (expected == null ? true : 
                (data.result.GetType().IsArray ? MatchesExpected((Array)data.result, expected.ToString()) : MatchesExpected(Result, expected.ToString()))
            );

            if(string.IsNullOrEmpty(caption) && !match) throw new ResultMismatchException(info);
            else if(!string.IsNullOrEmpty(caption)){                                                          
                var success = ParseChild(run, "success", "OK");
                var error = ParseChild(run, "error", "ERROR"); 

                List<string> errors = null;
                if(!match){                                                            
                    //Computing errors when within a question
                    errors = new List<string>(){info}; 
                    if(Errors != null) Errors.AddRange(errors);                    
                }
                
                Output.Write($"{caption} ");
                Output.WriteResponse(errors, success, error); 
            }                                   
        }

        private void ParseQuestion(YamlNode node, string current="question", string parent="root"){
            if(node == null || !node.GetType().Equals(typeof(YamlMappingNode))) return;

            //Validation before continuing
            var question = (YamlMappingNode)node;
            ValidateChildren(question, current, new string[]{"score", "caption", "description", "content"}, new string[]{"content"});     
                        
            if(Errors != null){
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
            Errors = new List<string>();

            //Loading question data                        
            CurrentScore = ComputeQuestionScore(question);
            var caption = ParseChild(question, "caption", $"Question {CurrentQuestion} [{CurrentScore} {(CurrentScore > 1 ? "points" : "point")}]");
            var description = ParseChild(question, "description", string.Empty);  
            
            //Displaying question caption
            caption = (string.IsNullOrEmpty(description) ? $"{caption}:" : $"{caption} - {description}:");            
            Output.WriteLine(caption, ConsoleColor.Cyan);
            Output.Indent();                        

            //Parse and run question content
            var content = "content";
            if(question.Children.ContainsKey(content)) ParseContent(question.Children[content], content, current);           

            //Compute scores
            float total = Success + Fails;
            TotalScore = (total > 0 ? (Success / total)*MaxScore : 0);      
            Errors = null;   

            //Closing the question                            
            Output.UnIndent();                                    
            Output.BreakLine();   
        }

        private void ParseContent(YamlNode node, string current="content", string parent="question"){
            if(node == null || !node.GetType().Equals(typeof(YamlSequenceNode))) return;

            //Scope in
            Vars.Push(new Dictionary<string, object>());
            Connectors.Push(new Dictionary<string, object>());

            //Subquestion detection
            var subquestion = false;
            ValidateChildren((YamlSequenceNode)node, current, new string[]{"vars", "connector", "run", "question"});
            ForEachChild((YamlSequenceNode)node, new Action<string, YamlMappingNode>((name, node) => {
                switch(name){                   
                    case "question":
                        subquestion = true;
                        return;                        
                } 
            }));
            
            //Recursive content processing
            ForEachChild((YamlSequenceNode)node, new Action<string, YamlMappingNode>((name, node) => {
                if(Abort || Skip) return;
                
                switch(name){
                    case "vars":
                        ParseVars(node, name, current);                            
                        break;

                    case "connector":
                        ParseConnector(node, name, current);                            
                        break;

                    case "run":
                        ParseRun(node, name, current);
                        break;

                    case "question":                        
                        ParseQuestion(node, name, current);
                        break;
                } 
            }));            
            
            //Processing score            
            if(!subquestion || Skip){
                Skip = false;
                if(Errors.Count == 0) Success += CurrentScore;
                else Fails += CurrentScore;
            } 

            //Scope out
            Vars.Pop();
            Connectors.Pop();  
        }
        
        private Dictionary<string, object> ParseArguments(YamlNode node){                        
            var arguments =  new Dictionary<string, object>();
            if(node == null) return arguments;

            //Load the connector argument list
            if(node.GetType() == typeof(YamlScalarNode)){                    
                //Inline arguments                
                var input = node.ToString().Trim();
                while(input.Length > 0){
                    //NOTE: trim over "--" cannot be used due arguemnts like "--regex <!--[\\s\\S\n]*?-->" so it will be processed sequentially
                    if(!input.StartsWith("--")) throw new ArgumentInvalidException("Provided arguments must be as '--argName argValue'");
                                    
                    input = input.TrimStart('-');
                    var name = input.Substring(0, input.IndexOf(' '));
                    input = input.Substring(input.IndexOf(' ')+1);
                    
                    var value = string.Empty;
                    char separator = (input.StartsWith('"') ? '"' : ' ');                    
                    if(input.Contains(separator)){
                        value = input.Substring(0, input.IndexOf(separator));
                        input = input.TrimStart(separator).Substring(input.IndexOf(separator)+1).TrimEnd(separator);
                    }
                    else{
                        value = input;
                        input = string.Empty;
                    }                    
                    
                    arguments.Add(name, ComputeVarValue(name, value));
                }

                // foreach(var item in node.ToString().Split("--").Skip(1)){                    
                //     var clean = item.Trim(' ');
                //     var name = string.Empty;
                //     var value = string.Empty;                    
                //     char separator = (clean.Contains('"') ? '"' : ' ');

                //     name = clean.Substring(0, clean.IndexOf(separator)).TrimStart('-').Trim();
                //     value = clean.Substring(clean.IndexOf(separator)+1).TrimEnd('"').Trim();
                //     arguments.Add(name, ComputeVarValue(name, value));
                // }
            }
            else{
                //Typed arguments               
                ForEachChild((YamlMappingNode)node, new Action<string, YamlNode>((name, node) => {                    
                    if(node.GetType().Equals(typeof(YamlScalarNode))){
                        //Scalar typed argument
                        var scalar = (YamlScalarNode)node;    
                        arguments.Add(name, ComputeArgument(name, scalar));                                           
                    }
                    else if(node.GetType().Equals(typeof(YamlSequenceNode))){
                        //NOT scalar typed argument
                        var sequence = (YamlSequenceNode)node;
                        var dict = new Dictionary<object, object>();

                        ForEachChild(sequence, new Action<string, YamlNode>((name, node) => {
                            if(node.GetType().Equals(typeof(YamlScalarNode))){
                               //Array typed argument
                               var scalar = (YamlScalarNode)node;      
                               dict.Add(ComputeArgument(name, scalar), null);
                            }
                            else if(node.GetType().Equals(typeof(YamlMappingNode))){                                
                                //Dictionary typed argument
                                //TODO: test this
                                var map = (YamlMappingNode)node; 
                                ForEachChild(map, new Action<string, YamlScalarNode>((name, node) => {
                                    dict.Add(name, ComputeArgument(name, node));
                                }));
                            }
                            else throw new NotSupportedException();
                        }));
                                                
                        if(dict.Values.Count(x => x != null) > 0){
                            //Dictionary, but needs type casting
                            //TODO: cast the dictionary to the correct types
                            arguments.Add(name, dict);
                        } 
                        else {
                            //Array, but needs type casting
                            var items = dict.Keys.ToArray();
                            if(items.GroupBy(x => x.GetType()).Count() > 1) arguments.Add(name, items);
                            else{
                                //All the items are of the same type, so casting can be done :)
                                Type t = items.FirstOrDefault().GetType();                                
                                Array casted = Array.CreateInstance(t, items.Length);
                                Array.Copy(items, casted, items.Length);
                                arguments.Add(name, casted);
                            }
                            
                        }
                    }
                    else throw new NotSupportedException();
                }));
            } 

            return arguments;
        }
#endregion
#region Nodes
        private T ParseChild<T>(YamlMappingNode node, string child, T @default, bool compute=true){           
            if(node.Children.ContainsKey(child)){
                var current = node.Children.Where(x => x.Key.ToString().Equals(child)).FirstOrDefault().Value;
                if(!current.GetType().Equals(typeof(YamlScalarNode))) throw new NotSupportedException("This method only supports YamlScalarNode child nodes.");
                return (T)ParseNode((YamlScalarNode)current,  @default, compute);                            
            } 
            else{
                if(@default == null || !@default.GetType().Equals(typeof(string))) return @default;
                else return (T)ParseNode(new YamlScalarNode(@default.ToString()), @default, compute); 
            }
        }
                 
        private T ParseNode<T>(YamlScalarNode node, T @default, bool compute=true){
            try{                                
                return (T)ParseNode(node, compute);                    
            }
            catch(InvalidCastException){
                return @default;
            }            
        }

        private object ParseNode(YamlScalarNode node, bool compute=true){    
            object  value = ComputeTypeValue(node.Tag, node.Value);

            if(value.GetType().Equals(typeof(string))){                
                //Always check if the computed value requested is correct, otherwise throws an exception
                var computed = ComputeVarValue(value.ToString());
                if(compute) value = computed;
            } 

            return value;
        }        

        private IEnumerable<KeyValuePair<YamlNode, YamlNode>> GetChildren(YamlSequenceNode node){
            return node.Children.SelectMany(x => ((YamlMappingNode)x).Children);
        }

        private IEnumerable<KeyValuePair<YamlNode, YamlNode>> GetChildren(YamlMappingNode node){
            return node.Children.Select(x => x);
        }

        private void ValidateChildren(YamlSequenceNode node, string current, string[] expected, string[] mandatory = null){            
            ValidateChildren(GetChildren(node), current, expected, mandatory);
        }

        private void ValidateChildren(YamlMappingNode node, string current, string[] expected, string[] mandatory = null){
            ValidateChildren(GetChildren(node), current, expected, mandatory);
        }

        private void ValidateChildren(IEnumerable<KeyValuePair<YamlNode, YamlNode>> nodes, string current, string[] expected, string[] mandatory){
            var found = new List<string>();

            foreach (var entry in nodes){
                var name = entry.Key.ToString();
                found.Add(name);
                if(expected != null && !expected.Contains(name)) throw new DocumentInvalidException($"Unexpected value '{name}' found within '{current}'.");                        
            }

            if(mandatory != null){
                foreach (var name in mandatory){
                    if(!found.Contains(name)) throw new DocumentInvalidException($"Mandatory value '{name}' not found within '{current}'.");                        
                }
            }
        }

        private void ForEachChild<T>(YamlSequenceNode node, Action<string, T> action) where T: YamlNode{
            ForEachChild(GetChildren(node), action);
        }

        private void ForEachChild<T>(YamlMappingNode node, Action<string, T> action) where T: YamlNode{
            ForEachChild(GetChildren(node), action);
        }

        private void ForEachChild<T>(IEnumerable<KeyValuePair<YamlNode, YamlNode>> nodes, Action<string, T> action) where T: YamlNode{
            foreach(var child in nodes){
                if(child.Value.GetType().Equals(typeof(T)) || typeof(T).Equals(typeof(YamlNode))) action.Invoke(child.Key.ToString(), (T)child.Value);                                
                else if(typeof(T).Equals(typeof(YamlScalarNode))) action.Invoke(child.Key.ToString(), (T)(YamlNode)(new YamlScalarNode()));  
                else if(typeof(T).Equals(typeof(YamlMappingNode))) action.Invoke(child.Key.ToString(), (T)(YamlNode)(new YamlMappingNode()));  
                else if(typeof(T).Equals(typeof(YamlSequenceNode))) action.Invoke(child.Key.ToString(), (T)(YamlNode)(new YamlSequenceNode()));
                else throw new NotSupportedException();                
            }
        }

#endregion
#region Helpers
        private (MethodBase method, object[] args) GetMethod(Type type, string method, Dictionary<string, object> arguments = null){            
            List<object> args = null;
            var constructor = method.Equals(type.Name);                        
            var methods = (constructor ? (MethodBase[])type.GetConstructors() : (MethodBase[])type.GetMethods());                        

            //Getting the constructor parameters in order to bind them with the YAML script ones (important to order by argument count)            
            if(arguments == null) arguments = new Dictionary<string, object>();            
            foreach(var info in methods.Where(x => x.Name.Equals((constructor ? ".ctor" : method), StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(x => x.GetParameters().Count())){
                var pending = (arguments == null ? new List<string>() : arguments.Keys.ToList());
                args = new List<object>();
                
                foreach(var param in info.GetParameters()){
                    pending.Remove(param.Name);

                    if(arguments.ContainsKey(param.Name) && (arguments[param.Name].GetType() == param.ParameterType)) args.Add(arguments[param.Name]);
                    else if(arguments.ContainsKey(param.Name) && param.ParameterType.IsEnum && arguments[param.Name].GetType().Equals(typeof(string))) args.Add(Enum.Parse(param.ParameterType, arguments[param.Name].ToString()));
                    else if(param.IsOptional) args.Add(param.DefaultValue); //adding default values for optional arguments
                    else{
                        args = null;
                        break;
                    } 
                }

                //Not null means that all the constructor parameters has been succesfully binded, but unused arguments are not allowed
                if(args != null && pending.Count == 0) return (info, args.ToArray());
            }
            
            //No bind has been found
            throw new ArgumentInvalidException($"Unable to find any {(constructor ? "constructor" : "method")} for the Connector '{type.Name}' that matches with the given set of arguments.");
        }
               
        private (object result, bool shellExecuted) InvokeCommand(object connector, string command, Dictionary<string, object> arguments = null){
            //Loading command data                        
            if(string.IsNullOrEmpty(command)) throw new ArgumentNullException("command", new Exception("A 'command' argument must be specified within 'run'."));  
            
            //Binding with an existing connector command
            var shellExecuted = false;                    

            (MethodBase method, object[] args) data;
            try{
                //Regular bind (directly to the connector or its inner connector)
                if(arguments == null) arguments = new Dictionary<string, object>();
                data = GetMethod(connector.GetType(), command, arguments);                
            }
            catch(ArgumentInvalidException){       
                //If LocalShell (implicit or explicit) is being used, shell commands can be used directly as "command" attributes.
                shellExecuted = connector.GetType().Equals(typeof(Connectors.LocalShell)) || connector.GetType().IsSubclassOf(typeof(Connectors.LocalShell));  
                if(shellExecuted){                                     
                    if(!arguments.ContainsKey("path")) arguments.Add("path", string.Empty); 
                    arguments.Add("command", command);
                    command = "RunCommand";
                }
                
                //Retry the execution
                data = GetMethod(connector.GetType(), command, arguments);                
            }

            var result = data.method.Invoke(connector, data.args);                                                 
            return (result, shellExecuted);
        }

        private object ComputeArgument(string name, YamlScalarNode node){
            var value = ComputeTypeValue(node.Tag, node.Value);            
            if(value.GetType().Equals(typeof(string))) value = ComputeVarValue(name, value.ToString());
            return value;
        }
        
        private string ComputeVarValue(string value){
            return ComputeVarValue(nameof(value), value);
        }

        private string ComputeVarValue(string name, string value){
            foreach(Match match in Regex.Matches(value, "{(.*?)}")){    
                //The match must be checked, because double keys can fail, for example: awk "BEGIN {print {$NUM1}+{$NUM2}+{$NUM3}; exit}"                   
                var original = match.Value;
                if(original.TrimStart('{').Contains('{')) original = original.Substring(original.LastIndexOf('{'));

                var replace = original.TrimStart('{').TrimEnd('}');                 
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
                            catch (Exception ex){
                                throw new RegexInvalidException($"Invalid regular expression defined inside the variable '{name}'.", ex);
                            }
                        }
                    }
                }
                
                value = value.Replace(original, replace);
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
                    "Connector" => GetConnector(value, false),
                    _       => throw new InvalidCastException($"Unable to cast the value '{value}' using the YAML tag '{tag}'."),
                };
            }            
        }

        private float ComputeQuestionScore(YamlMappingNode root){        
            var score = 0f;
            var subquestion = false;
            
            if(root.Children.ContainsKey("content")){            
                ForEachChild((YamlSequenceNode)root.Children["content"], new Action<string, YamlMappingNode>((name, node) => {
                    switch(name){                   
                        case "question":
                            subquestion = true;

                            score += ComputeQuestionScore(node);
                            break;
                    } 
                }));
            }

            if(!subquestion) return ParseChild(root, "score", 1f, false);
            else return score;
        }

        private bool MatchesExpected(Array current, string expected){
            var match = false;
            expected = expected.ToUpper().TrimStart();

            if(expected.StartsWith("LENGTH")){
                expected = expected.Substring(6).Trim();
                return MatchesExpected(current.Length.ToString(), expected);                
            }
            else if(expected.StartsWith("CONTAINS")){
                expected = expected.Substring(8).Trim();

                foreach(var item in current){
                    match = MatchesExpected(item.ToString(), expected);
                    if(match) break;
                }
            }
            else if(expected.StartsWith("UNORDEREDEQUALS")){
                //TODO
                throw new NotImplementedException();

            }
            else if(expected.StartsWith("ORDEREDEQUALS")){
                //TODO
                throw new NotImplementedException();
            }
            else throw new NotSupportedException();    
            
            return match;
        }

        private bool MatchesExpected(string current, string expected){
            var match = false;
            var comparer = Operator.EQUALS;                        

            if(expected.StartsWith("=")) expected = expected.Substring(1);
            else if(expected.StartsWith("<=")){ 
                comparer = Operator.LOWEREQUALS;
                expected = expected.Substring(2);
            }
            else if(expected.StartsWith(">=")){
                comparer = Operator.GREATEREQUALS;
                expected = expected.Substring(2);
            }
            else if(expected.StartsWith("<")){ 
                comparer = Operator.LOWER;
                expected = expected.Substring(1);
            }
            else if(expected.StartsWith(">")){
                comparer = Operator.GREATER;                        
                expected = expected.Substring(1);
            }
            else if(expected.StartsWith("LIKE")){
                comparer = Operator.LIKE;
                expected = expected.Substring(4);
            }
            else if(expected.StartsWith("%") || expected.EndsWith("%")){
                comparer = Operator.LIKE;
            }
            else if(expected.StartsWith("<>") || expected.StartsWith("!=")){ 
                comparer = Operator.NOTEQUALS;
                expected = expected.Substring(2);
            }
            
            expected = expected.Trim();
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
            var inherits = ParseChild(root, "inherits", string.Empty);

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

        private object GetConnector(string name, bool @default = true){     
            try{
                return FindItemWithinScope(Connectors, name);
            }      
            catch{
                if(@default) return new Connectors.LocalShell();
                else throw new ConnectorNotFoundException($"Unable to find any connector named '{name}'");
            }            
        }

        private Dictionary<string, object> FindScope(Stack<Dictionary<string, object>> scope, string key){
            object item = null;            
            var visited = new Stack<Dictionary<string, object>>();            

            try{
                //Search the connector by name within scopes
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
#region Copy Detection        
        private CopyDetector LoadCopyDetector(string type, string caption, float threshold, string filePattern, string[] folders){                        
            Assembly assembly = Assembly.GetExecutingAssembly();
            var assemblyType = assembly.GetTypes().Where(t => t.Name.Equals(type, StringComparison.InvariantCultureIgnoreCase) && t.IsSubclassOf(typeof(CopyDetector))).FirstOrDefault();
            if(assembly == null) throw new ArgumentInvalidException("type");            

            //Loading documents
            var originalFolder = CurrentFolder;
            var cd = (CopyDetector)Activator.CreateInstance(assemblyType, new object[]{threshold, filePattern});              
            foreach(string f in folders)
            {                
                try{
                    CurrentFolder = f;
                    Output.Write(ComputeVarValue(caption) , ConsoleColor.DarkYellow);                    
                    cd.Load(f);                    
                    Output.WriteResponse();
                }
                catch (Exception e){
                    Output.WriteResponse(e.Message);
                }                
            }

            //Resotre
            CurrentFolder = originalFolder;

            //Compare
            if(cd.Count > 0) cd.Compare();
            return cd;
        }                          
        
        private void PrintCopies(CopyDetector cd, string folder){                        
            var details = cd.GetDetails(folder);
            folder = Path.GetDirectoryName(folder);
            folder = details.file.Substring(folder.Length).TrimStart('\\');

            Output.Write("Potential copy detected for ", ConsoleColor.Red);                                          
            Output.Write(folder, ConsoleColor.Yellow);
            Output.WriteLine("!", ConsoleColor.Red);
            Output.Indent();

            foreach(var item in details.matches){  
                folder = Path.GetDirectoryName(item.folder);
                folder = item.file.Substring(folder.Length).TrimStart('\\');

                Output.Write($"Match score with ~{folder}: ", ConsoleColor.Yellow);     
                Output.WriteLine(string.Format("~{0:P2} ", item.match), (item.match < cd.Threshold ? ConsoleColor.Green : ConsoleColor.Red));
            }
            
            Output.UnIndent();
            Output.BreakLine();
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
                            var parsedDbName = Path.GetFileName(ComputeVarValue(dbname)).Replace(" ", "_").Replace(".", "_");
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
        private void UploadGDrive(string source, string account, string secret, string remoteFolder, bool link, bool copy, bool remove, bool recursive){                        
            if(string.IsNullOrEmpty(account)) throw new ArgumentNullException("The 'username' argument must be provided when using the 'upload_gdrive' feature.");                        

            Output.WriteLine("Uploading files to Google Drive: ");
            Output.Indent();

            //CurrentFolder and CurrentFile may be modified during execution
            var originalCurrentFile = CurrentFile;
            var originalCurrentFolder = CurrentFolder;
                
            //Option 1: Only files within a searchpath, recursive or not, will be uploaded into the same remote folder.
            //Option 2: Non-recursive folders within a searchpath, including its files, will be uploaded into the same remote folder.
            //Option 3: Recursive folders within a searchpath, including its files, will be uploaded into the remote folder, replicating the folder tree.
           
            try{     
                remoteFolder = ComputeVarValue(remoteFolder.TrimEnd('\\'));
                using(var drive = new Connectors.GDrive(account, secret)){                        
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