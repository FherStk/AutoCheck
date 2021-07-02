/*
    Copyright © 2021 Fernando Porrino Serrano
    Third party software licenses can be found at /docs/credits/credits.md

    This file is part of AutoCheck.

    AutoCheck is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.Fwrite

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
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using YamlDotNet.RepresentationModel;
using AutoCheck.Core.Exceptions;
using CopyDetector = AutoCheck.Core.CopyDetectors.Base;
using Operator = AutoCheck.Core.Connectors.Operator;
using OS = AutoCheck.Core.Utils.OS;


namespace AutoCheck.Core{        
    public class Script{
#region Classes
    private class Remote: Local{
        public OS OS {get; set;}
        public string Host {get; set;}
        public string User {get; set;}
        public string Password {get; set;}
        public int Port {get; set;}        

        public Remote(OS os, string host, string user, string password, int port, string[] folders, Dictionary<string, object> vars): base(folders, vars){
            OS = os;
            Host = host;
            User = user;
            Password = password;
            Port = port;            
        }
    }

    private class Local{        
        public string[] Folders {get; set;}
        public Dictionary<string, object> Vars {get; set;}

        public Local(string[] folders, Dictionary<string, object> vars){            
            Folders = folders;
            Vars = vars;
        }
    }

    private enum LogFormatType {
        TEXT,
        JSON
    }
#endregion
#region Vars
        /// <summary>
        /// The current script name defined within the YAML file, otherwise the YAML file name.
        /// </summary>
        protected string ScriptName {
            get{
                return GetVar("script_name", AutoComputeVarValues).ToString();
            }

            private set{
                UpdateVar("script_name", value);                
            }
        }

        /// <summary>
        /// The current script version defined within the YAML file, otherwise the YAML file name.
        /// </summary>
        protected string ScriptVersion {
            get{
                return GetVar("script_version", AutoComputeVarValues).ToString();
            }

            private set{
                UpdateVar("script_version", value);                
            }
        }

        /// <summary>
        /// The current script caption defined within the YAML file.
        /// </summary>
        protected string ScriptCaption {
            get{
                return GetVar("script_caption", AutoComputeVarValues).ToString();
            }

            private set{
                UpdateVar("script_caption", value);                
            }
        }

        /// <summary>
        /// The current script caption defined within the YAML file.
        /// </summary>
        protected string SingleCaption {
            get{
                return GetVar("single_caption", AutoComputeVarValues).ToString();
            }

            private set{
                UpdateVar("single_caption", value);                
            }
        }

        /// <summary>
        /// The current script caption defined within the YAML file.
        /// </summary>
        protected string BatchCaption {
            get{
                return GetVar("batch_caption", AutoComputeVarValues).ToString();
            }

            private set{
                UpdateVar("batch_caption", value);                
            }
        }
        
        /// <summary>
        /// The root app execution folder path.
        /// </summary>
        protected string AppFolderPath {
            get{
                return GetVar("app_folder_path", AutoComputeVarValues).ToString();
            }

            private set{                
                try{
                    //Read only
                    GetVar("app_folder_path", AutoComputeVarValues);
                    throw new NotSupportedException("Read only");
                }
                catch (ItemNotFoundException){
                    UpdateVar("app_folder_path", value); 
                    UpdateVar("app_folder_name", Path.GetFileName(value) ?? string.Empty);    
                } 
            }
        }

        /// <summary>
        /// The root app execution folder name.
        /// </summary>
        protected string AppFolderName {
            get{
                return GetVar("app_folder_name", AutoComputeVarValues).ToString();
            }           
        }

        /// <summary>
        /// The current script execution folder path defined within the YAML file, otherwise the YAML file's folder.
        /// </summary>
        protected string ExecutionFolderPath {
            get{
                return GetVar("execution_folder_path", AutoComputeVarValues).ToString();
            }

            private set{
                UpdateVar("execution_folder_path", value);  
                UpdateVar("execution_folder_name", Path.GetFileName(value) ?? string.Empty);             
            }
        }

        /// <summary>
        /// The current script execution folder name defined within the YAML file, otherwise the YAML file's folder.
        /// </summary>
        protected string ExecutionFolderName {
            get{
                return GetVar("execution_folder_name", AutoComputeVarValues).ToString();
            }
        }
        
        /// <summary>
        /// The script folder path.
        /// </summary>
        protected string ScriptFolderPath {
            get{
                return GetVar("script_folder_path", AutoComputeVarValues).ToString();
            }

            private set{    
                //Current folder values                            
                UpdateVar("script_folder_path", value);
                UpdateVar("script_folder_name", Path.GetFileName(value) ?? string.Empty); 

                //Setting up the folder path resets the file path
                UpdateVar("script_file_path", string.Empty);     
                UpdateVar("script_file_name", string.Empty);    
            }
        }

        /// <summary>
        /// The script folder name.
        /// </summary>
        protected string ScriptFolderName {
            get{
                return GetVar("script_folder_name", AutoComputeVarValues).ToString();
            }
        }
        
        /// <summary>
        /// The script file path.
        /// </summary>
        protected string ScriptFilePath {
            get{
                return GetVar("script_file_path", AutoComputeVarValues).ToString();
            }

            private set{
                //Setting up the file path forces the folder path
                ScriptFolderPath = Path.GetDirectoryName(value) ?? string.Empty;                         

                //Current file path values                
                UpdateVar("script_file_path", value);     
                UpdateVar("script_file_name", Path.GetFileName(value) ?? string.Empty);                                
            }
        }

        /// <summary>
        /// The script file name.
        /// </summary>
        protected string ScriptFileName {
            get{
                return GetVar("script_file_name", AutoComputeVarValues).ToString();
            }
        }     
        
        /// <summary>
        /// The current folder path where the script is targeting right now (local or remote); can change during the execution for batch-typed.
        /// </summary>
        protected string CurrentFolderPath {
            get{
                return GetVar("current_folder_path", AutoComputeVarValues).ToString();
            }

            private set{    
                //Current folder values                            
                UpdateVar("current_folder_path", value);
                UpdateVar("current_folder_name", Path.GetFileName(value) ?? string.Empty); 

                //Setting up the folder path resets the file path
                UpdateVar("current_file_path", string.Empty);     
                UpdateVar("current_file_name", string.Empty);    
            }
        }

        /// <summary>
        /// The current folder name where the script is targeting right now (local or remote); can change during the execution for batch-typed.
        /// </summary>
        protected string CurrentFolderName {
            get{
                return GetVar("current_folder_name", AutoComputeVarValues).ToString();
            }
        }
        
        /// <summary>
        /// The current file path where the script is targeting right now (local or remote); can change during the execution for batch-typed.
        /// </summary>
        protected string CurrentFilePath {
            get{
                return GetVar("current_file_path", AutoComputeVarValues).ToString();
            }

            private set{
                //Setting up the file path forces the folder path
                CurrentFolderPath = Path.GetDirectoryName(value) ?? string.Empty;                         

                //Current file path values                
                UpdateVar("current_file_path", value);     
                UpdateVar("current_file_name", Path.GetFileName(value) ?? string.Empty);                                
            }
        }

        /// <summary>
        /// The current file name where the script is targeting right now (local or remote); can change during the execution for batch-typed.
        /// </summary>
        protected string CurrentFileName {
            get{
                return GetVar("current_file_name").ToString();
            }
        }
        
        /// <summary>
        /// The current log folder (the entire path)
        /// </summary>
        protected string LogFolderPath {
            get{
                string path = (string)GetVar("log_folder_path", AutoComputeVarValues);
                return (AutoComputeVarValues ? CleanPathInvalidChars(path) : path);                
            }

            private set{    
                //Current folder values                            
                UpdateVar("log_folder_path", value);
                UpdateVar("log_folder_name", Path.GetFileName(value) ?? string.Empty); 

                //Setting up the folder path resets the file path
                UpdateVar("log_file_path", string.Empty);     
                UpdateVar("log_file_name", string.Empty);    
            }
        }

        /// <summary>
        /// The current log folder (the folder name)
        /// </summary>
        protected string LogFolderName {
            get{
                string path = (string)GetVar("log_folder_name", AutoComputeVarValues);
                return (AutoComputeVarValues ? CleanPathInvalidChars(path) : path);                
            }
        }

        /// <summary>
        /// The current log file (the entire path)
        /// </summary>
        protected string LogFilePath {
            get{
                string path = (string)GetVar("log_file_path", AutoComputeVarValues);
                return (AutoComputeVarValues ? CleanPathInvalidChars(path) : path);                
            }

            private set{
                //Setting up the file path forces the folder path
                LogFolderPath = Path.GetDirectoryName(value) ?? string.Empty;                         

                //Current file path values                
                UpdateVar("log_file_path", value);     
                UpdateVar("log_file_name", Path.GetFileName(value) ?? string.Empty);          
            }
        }

        /// <summary>
        /// The current log file (the file name)
        /// </summary>
        protected string LogFileName {
            get{
                string path = (string)GetVar("log_file_name", AutoComputeVarValues);
                return (AutoComputeVarValues ? CleanPathInvalidChars(path) : path);
            }
        }

        /// <summary>
        /// Only for batch mode: returns the kind of the current batch execution: `none`, `local` or `remote`.
        /// </summary>
        protected string CurrentTarget {
            get{
                return GetVar("current_target", AutoComputeVarValues).ToString();
            }

            private set{
                UpdateVar("current_target", value);               
            }
        }

        /// <summary>
        /// The current OS family.
        /// </summary>
        protected OS CurrentOS {
            get{
                return (OS)GetVar("current_os", AutoComputeVarValues);
            }

            private set{
                UpdateVar("current_os", value);               
            }
        }

        /// <summary>
        /// The host name or IP address for the current execution.
        /// </summary>
        protected string CurrentHost {
            get{
                return GetVar("current_host", AutoComputeVarValues).ToString();
            }

            private set{
                UpdateVar("current_host", value);               
            }
        }

        /// <summary>
        /// The username for the current execution.
        /// </summary>
        protected string CurrentUser {
            get{
                return GetVar("current_user", AutoComputeVarValues).ToString();
            }

            private set{
                UpdateVar("current_user", value);               
            }
        }

        /// <summary>
        /// The password for the current execution.
        /// </summary>
        protected string CurrentPassword {
            get{
                return GetVar("current_password", AutoComputeVarValues).ToString();
            }

            private set{
                UpdateVar("current_password", value);               
            }
        } 

        /// <summary>
        /// The port for the current execution.
        /// </summary>
        protected int CurrentPort {
            get{
                return (int)GetVar("current_port", AutoComputeVarValues);
            }

            private set{
                UpdateVar("current_port", value);               
            }
        }         

        /// <summary>
        /// The current question (and subquestion) number (1, 2, 2.1, etc.)
        /// </summary>
        protected string CurrentQuestion {
            get{
                return GetVar("current_question", AutoComputeVarValues).ToString();
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
                var res = GetVar("result", AutoComputeVarValues);
                return res == null ? null : res.ToString();
            }

            private set{                
                try{
                    //Only on upper scope (global)
                    GetVar("result", AutoComputeVarValues);
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
                return DateTimeOffset.UtcNow.ToString("o");
            }
        }

        /// <summary>
        /// The current question (and subquestion) score
        /// </summary>
        protected float CurrentScore {
            get{
                return (float)GetVar("current_score", AutoComputeVarValues);
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
                return (float)GetVar("max_score", AutoComputeVarValues);
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
                return (float)GetVar("total_score", AutoComputeVarValues);
            }

            private set{
                UpdateVar("total_score", value);                
            }
        }
#endregion
#region Attributes
        /// <summary>
        /// Output instance used to display messages.
        /// </summary>
        public Output Output {get; private set;} 

        /// <summary>
        /// Output instance used to display messages.
        /// </summary>
        public List<string> LogFiles {get; private set;}          

        private Stack<Dictionary<string, object>> Vars {get; set;}  //Variables are scope-delimited

        private Stack<Dictionary<string, object>> Connectors {get; set;}  //Connectors are scope-delimited

        private float Success {get; set;}
        
        private float Fails {get; set;}         

        private List<string> Errors {get; set;}

        private bool Abort {get; set;}
        
        private bool Skip {get; set;}

        private bool AutoComputeVarValues {get; set;}

        private bool BatchPauseEnabled {get; set;}

        private bool LogFilesEnabled {get; set;}   

        private LogFormatType LogFormat {get; set;}   

        private bool IsQuestionOpen {
            get{
                return Errors != null;
            }
        }        
#endregion
#region Constructor
        /// <summary>
        /// Creates a new script instance using the given script file.
        /// </summary>
        /// <param name="path">Path to the script file (yaml).</param>
        public Script(string path){    
            Output = new Output();     
            LogFiles = new List<string>();                                                           
            Connectors = new Stack<Dictionary<string, object>>();          
            Vars = new Stack<Dictionary<string, object>>();            
            
            //Scope in              
            Vars.Push(new Dictionary<string, object>());

            //Setup default vars (must be ALL declared before the caption (or any other YAML var) could be used, because the user can customize it using any of this vars)
            AutoComputeVarValues = false;
            Abort = false;
            Skip = false;
            Result = null;                                   
            MaxScore = 10f;  
            TotalScore = 0f;
            CurrentScore = 0f;
            CurrentQuestion = "0";     

            //Setup default folders, each property will set also the related 'name' property                              
            AppFolderPath = Utils.AppFolder;            
            ExecutionFolderPath = Utils.ExecutionFolder;
            ScriptFolderPath = Utils.PathToCurrentOS(Path.GetDirectoryName(path));
            ScriptFilePath = path;
            CurrentFolderPath = string.Empty;
            CurrentFilePath = string.Empty; 

            //Setup local/remote batch mode vars
            CurrentTarget = "none";
            SetupDefaultHostVars();

            //Setup the remaining vars            
            ScriptVersion = "1.0.0.0";
            ScriptName = Regex.Replace(Path.GetFileNameWithoutExtension(path), "[A-Z]", " $0");
            ScriptCaption = "Running script ~{$SCRIPT_NAME} (v{$SCRIPT_VERSION}):~";
            SingleCaption = "Running on single mode:";
            BatchCaption = "Running on batch mode:";
            BatchPauseEnabled = true;

            //Setup log data before starting
            SetupLog(
                Path.Combine("{$APP_FOLDER_PATH}", "logs"), 
                LogFormatType.TEXT,
                "{$SCRIPT_NAME}_{$NOW}", 
                false
            );                 
        
            //Load the YAML file
            var root = (YamlMappingNode)LoadYamlFile(path).Documents[0].RootNode;
            ValidateChildren(root, "root", new string[]{"inherits", "version", "caption", "name", "single", "batch", "output", "vars", "body", "max-score"});
                    
            //YAML header overridable vars 
            CurrentFolderPath = Utils.PathToCurrentOS(ParseChild(root, "folder", CurrentFolderPath, false));            
            ScriptVersion = ParseChild(root, "version", ScriptVersion, false);
            ScriptCaption = ParseChild(root, "caption", ScriptCaption, false);
            ScriptName = ParseChild(root, "name", ScriptName, false);                        
            MaxScore = ParseChild(root, "max-score", MaxScore, false);                                
                        
            //Preparing script body execution
            AutoComputeVarValues = true;
            var script = new Action(() => {   
                //This data must be cleared for each script body execution (batch mode)  
                Success = 0;
                Fails = 0;

                //Running script body                
                if(root.Children.ContainsKey("body")) ParseBody(root.Children["body"]);
                
                //Storing the log file path (it will be used to generate it later). 
                //WARNING: this is based on log index, could fail if multithreading is implemented
                var logFile = ComputeVarValue(LogFilePath);
                LogFiles.Add(logFile); 
            });                        

            //Storing script execution into log
            Output.WriteLine(ScriptCaption, Output.Style.HEADER);
            Output.CloseLog(Output.Type.HEADER);            

            //Vars are shared along, but pre, body and post must be run once for single-typed scripts or N times for batch-typed scripts    
            if(root.Children.ContainsKey("output")) ParseOutput(root.Children["output"]);
            if(root.Children.ContainsKey("vars")) ParseVars(root.Children["vars"]);
            if(root.Children.ContainsKey("single")) ParseSingle(root.Children["single"], script);
            if(root.Children.ContainsKey("batch")) ParseBatch(root.Children["batch"], script);   

            //If no batch and no single, force just an execution (usefull for simple script like test\samples\script\vars\vars_ok5.yaml)   
            if(!root.Children.ContainsKey("single") && !root.Children.ContainsKey("batch")){                
                Output.Indent();
                script.Invoke();
                Output.UnIndent();
                
                //Storing script execution log
                Output.CloseLog(Output.Type.SCRIPT);  
            }

            //Log files export (once all the teardown has been executed)
            ExportLog();
            
            //Scope out
            Vars.Pop();
            
        }
#endregion
#region Parsing
        private void ParseOutput(YamlNode node, string current="output", string parent="root", string[] children = null, string[] mandatory = null){
            children ??= new string[]{"terminal", "pause", "log"};
            if(node == null || !node.GetType().Equals(typeof(YamlMappingNode))) return;
            
            AutoComputeVarValues = false;
            ValidateChildren(node, current, children, mandatory);
            ForEachChild(node, new Action<string, YamlScalarNode>((name, value) => {
                switch(name){
                    case "terminal":
                        if(!ParseNode<bool>(value, true)) Output.SetMode(Output.Mode.SILENT); //Just alter default value (terminal) because testing system uses silent and should not be replaced here                       
                        break;

                    case "pause":
                        BatchPauseEnabled = ParseNode<bool>(value, BatchPauseEnabled);
                        break;
                }               
            }));  

            ForEachChild(node, new Action<string, YamlMappingNode>((name, value) => {
                switch(name){
                    case "log":
                        SetupLog(
                            ParseChild(value, "folder", LogFolderPath, false), 
                            ParseChild(value, "format", LogFormat, false), 
                            ParseChild(value, "name", LogFileName, false),
                            ParseChild(value, "enabled", LogFilesEnabled, false)
                        );                       
                        break;
                }               
            }));  
            AutoComputeVarValues = true;                 
        } 

        private void ParseVars(YamlNode node, string current="vars", string parent="root", string[] reserved = null, Stack<Dictionary<string, object>> stack = null){
            reserved ??= new string[]{"script_name", "execution_folder_path", "current_ip", "current_folder_path", "current_file_path", "result", "now"};
            if(node == null || !node.GetType().Equals(typeof(YamlMappingNode))) return;
            
            ForEachChild(node, new Action<string, YamlScalarNode>((name, value) => {
                if(reserved.Contains(name)) throw new VariableInvalidException($"The variable name {name} is reserved and cannot be declared.");                                    
                UpdateVar(name, ParseNode(value, false), stack);
            }));                     
        }  
        
        private void ParseSetup(YamlNode node, string current="setup", string parent="root", string[] children = null, string[] mandatory = null){
            children ??= new string[]{"vars", "connector", "run", "echo"};

            //The same as ParseBody but with no questions                        
            ParseBody(node, current, parent, children, mandatory);
        }    
        
        private void ParseTeardown(YamlNode node, string current="teardown", string parent="root", string[] children = null, string[] mandatory = null){
            //The same as ParseSetup
            ParseSetup(node, current, parent, children, mandatory);
        }

        private void ParsePre(YamlNode node, string current="pre", string parent="batch", string[] children = null, string[] mandatory = null){
            children ??= new string[]{"vars", "connector", "run", "echo"};

            //The same as ParseBody but with no questions                        
            ParseBody(node, current, parent, children, mandatory);
        }

        private void ParsePost(YamlNode node, string current="post", string parent="batch", string[] children = null, string[] mandatory = null){
            //The same as ParsePre
            ParsePre(node, current, parent, children, mandatory);
        }
        
        //TODO: can be single and batch simplified where one uses onther?
        private void ParseSingle(YamlNode node, Action action, string current="single", string parent="root", string[] children = null, string[] mandatory = null){  
            children ??= new string[]{"caption", "setup", "teardown", "local", "remote"};
            if(node == null || !node.GetType().Equals(typeof(YamlMappingNode))) action.Invoke();
            else{                                    
                //Parsing caption (scalar)
                AutoComputeVarValues = false;
                ValidateChildren(node, current, children, mandatory);                
                ForEachChild(node, new Action<string, YamlScalarNode>((name, node) => { 
                    switch(name){                       
                        case "caption":                            
                            SingleCaption = ParseNode(node, SingleCaption, false);
                            break;
                    }
                }));
                AutoComputeVarValues = true;

                //Parsing local / remote targets      
                Local local = null;
                Remote remote = null;
                ForEachChild(node, new Action<string, YamlMappingNode>((name, node) => { 
                    switch(name){                        
                        case "local":                        
                            local = ParseLocal(node, name, current, new string[]{"folder", "vars"});
                            if(local.Folders.Length > 1) throw new DocumentInvalidException("Only one folder is allowed when running on single mode.");
                            break;

                        case "remote":                        
                            remote = ParseRemote(node, name, current);
                            break;
                    }
                }));

                //Parsing pre content, it must run for each target before the body and the copy detector execution
                ForEachChild(node, new Action<string, YamlMappingNode>((name, node) => {  
                    if(Abort) return;                                              
                    switch(name){                       
                        case "setup":                            
                            ParseSetup(node, name, current);
                            Output.BreakLine();                 
                            break;
                    }                    
                })); 

                //Storing log for the setup data
                Output.CloseLog(Output.Type.SETUP);
                
                //Execution abort could be requested from any "setup"
                if(Abort) return;

                //Both local and remote will run exactly the same code
                var script = new Action(() => {
                    Output.WriteLine(SingleCaption, Output.Style.HEADER);
                    Output.Indent();
                    action.Invoke();
                    Output.UnIndent();

                    //Storing log for the script data
                    Output.CloseLog(Output.Type.SCRIPT);
                });

                if(local != null){
                    ForEachLocalTarget(new Local[]{local}, (folder) => {
                        //ForEachLocalTarget method setups the global vars
                        script.Invoke();
                    });
                }

                if(remote != null){
                    ForEachRemoteTarget(new Remote[]{remote}, (os, host, username, password, port, folder) => {
                        //ForEachLocalTarget method setups the global vars
                        script.Invoke();
                    });
                }

                //Parsing post content, it must run for each target after all the bodies and the copy detector execution
                ForEachChild(node, new Action<string, YamlMappingNode>((name, node) => {  
                    if(Abort) return;                      
                    switch(name){                       
                        case "teardown":                            
                            ParseTeardown(node, name, current);
                            Output.BreakLine();  
                            break;
                    }                    
                })); 

                //Storing log for the teardown data
                Output.CloseLog(Output.Type.TEARDOWN);
            }
        }

        private void ParseBatch(YamlNode node, Action action, string current="batch", string parent="root", string[] children = null, string[] mandatory = null){   
            children ??= new string[]{"caption", "setup", "teardown", "pre", "post", "copy_detector", "local", "remote"};     
            if(node == null || !node.GetType().Equals(typeof(YamlSequenceNode))) action.Invoke();
            else{    
                //Running in batch mode            
                var originalFolder = CurrentFolderPath;
                var originalIP = CurrentHost;                                          
                                
                //Collecting all the folders and IPs      
                ValidateChildren(node, current, children, mandatory);
                
                //Parsing caption (scalar)
                AutoComputeVarValues = false;
                ForEachChild(node, new Action<string, YamlScalarNode>((name, node) => { 
                    switch(name){                       
                        case "caption":                            
                            BatchCaption = ParseNode(node, BatchCaption, false);
                            break;
                    }
                }));
                AutoComputeVarValues = true;

                //Parsing local / remote targets      
                var local = new List<Local>();  
                var remote = new List<Remote>();                        
                ForEachChild(node, new Action<string, YamlSequenceNode>((name, node) => { 
                    switch(name){                            
                        case "local":                        
                            local.Add(ParseLocal(node, name, current));
                            break;

                        case "remote":                        
                            remote.Add(ParseRemote(node, name, current));
                            break;
                    }
                }));

                //Parsing setup content, it must run for each target before the body and the copy detector execution
                Output.Indent();      
                ForEachLocalTarget(local.ToArray(), (folder) => {                    
                    ForEachChild(node, new Action<string, YamlSequenceNode>((name, node) => {                                             
                        if(Abort) return;
                        switch(name){                       
                            case "setup":                            
                                ParseSetup(node, name, current);
                                Output.BreakLine();                                
                                break;
                        }                    
                    })); 
                }); 

                ForEachRemoteTarget(remote.ToArray(), (os, host, username, password, port, folder) => {
                    ForEachChild(node, new Action<string, YamlSequenceNode>((name, node) => {
                        if(Abort) return;   
                        switch(name){                       
                            case "setup":                            
                                ParseSetup(node, name, current);
                                Output.BreakLine();
                                break;
                        }                    
                    })); 
                });               
                
                //Execution abort could be requested from any "setup"
                if(Abort) return;
                                    
                //Note: the copy detector cannot be parsed till the local and remote has been loaded and, also, all pre has been executed.
                var cpydet = new List<CopyDetector>();
                ForEachChild(node, new Action<string, YamlMappingNode>((name, node) => {                                         
                    switch(name){                       
                        case "copy_detector":                            
                            cpydet.AddRange(ParseCopyDetector(node, local.ToArray(), remote.ToArray(), name, current));
                            break;
                    }                    
                })); 
                Output.UnIndent();                   
                if(cpydet.Count > 0) Output.BreakLine();

                //Storing log for the setup data
                Output.CloseLog(Output.Type.SETUP);
                
                //Both local and remote will run exactly the same code
                var script = new Action<string>((folder) => {
                    //Printing script caption
                    Output.WriteLine(BatchCaption, Output.Style.HEADER);
                    
                    //Running copy detectors and script body
                    new Action(() => {
                        //Pre content
                        Output.Indent();      
                        ForEachChild(node, new Action<string, YamlSequenceNode>((name, node) => {                                             
                            if(Abort) return;
                            switch(name){                       
                                case "pre":                            
                                    ParsePre(node, name, current);
                                    Output.BreakLine();                                
                                    break;
                            }                    
                        }));                                        
                        
                        var match = false;
                        try{                        
                            foreach(var cd in cpydet){                            
                                if(cd != null){
                                    match = match || cd.CopyDetected(folder);                        
                                    if(match) PrintCopies(cd, folder);                            
                                }
                            }                        

                            if(!match){
                                action.Invoke();  
                                Output.BreakLine();
                            } 
                        }
                        catch(Exception ex){
                            Output.WriteLine($"ERROR: {ExceptionToOutput(ex)}", Output.Style.ERROR);
                        }

                        //Post content
                        ForEachChild(node, new Action<string, YamlSequenceNode>((name, node) => {                                             
                            if(Abort) return;
                            switch(name){                       
                                case "post":                            
                                    ParsePost(node, name, current);
                                    Output.BreakLine();
                                    break;
                            }                    
                        }));                    
                        Output.UnIndent();                        
                        
                        //Pausing if needed, but should not be logged...
                        if(!BatchPauseEnabled || Output.GetMode() != Output.Mode.VERBOSE) Output.BreakLine();
                        else {                               
                            Console.WriteLine("Press any key to continue...");
                            Console.ReadKey();
                            Output.BreakLine(2);
                        }

                        //Storing log for the script data
                        Output.CloseLog(Output.Type.SCRIPT);
                                                
                    }).Invoke();
                });

                //Executing body for each local target                
                ForEachLocalTarget(local.ToArray(), (folder) => {
                    //ForEachLocalTarget method setups the global vars                    
                    script.Invoke(folder);
                });                                  

                //Executing body for each remote target                
                ForEachRemoteTarget(remote.ToArray(), (os, host, username, password, port, folder) => {
                    //ForEachLocalTarget method setups the global vars
                    script.Invoke(folder);
                }); 

                //Parsing teardown content, it must run for each target after all the bodies and the copy detector execution
                Output.Indent();
                ForEachLocalTarget(local.ToArray(), (folder) => {
                    ForEachChild(node, new Action<string, YamlSequenceNode>((name, node) => {    
                        if(Abort) return;                    
                        switch(name){                       
                            case "teardown":                            
                                ParseTeardown(node, name, current);
                                Output.BreakLine();
                                break;
                        }                    
                    })); 
                }); 

                ForEachRemoteTarget(remote.ToArray(), (os, host, username, password, port, folder) => {
                    ForEachChild(node, new Action<string, YamlSequenceNode>((name, node) => {    
                        if(Abort) return;                    
                        switch(name){                       
                            case "teardown":                            
                                ParseTeardown(node, name, current);
                                Output.BreakLine();
                                break;
                        }                    
                    })); 
                }); 
                Output.UnIndent(); 

                //Storing log for the teardown data
                Output.CloseLog(Output.Type.TEARDOWN);                                
            }            
        }

        private Local ParseLocal(YamlNode node, string current="local", string parent="single", string[] children = null, string[] mandatory = null){  
            children ??= new string[]{"path", "folder", "vars"};
            
            var folders = new List<string>();  
            Dictionary<string, object> vars = new Dictionary<string, object>();

            //Setting up default localhost data, can be overriden using the "vars" property within a yaml script.
            SetupDefaultHostVars();

            var parse = new Action<string, YamlNode>((string name, YamlNode node) => {
                //Prepare the local folder/path parsing mechanism for mapping/sequence definition (within single/batch)
                switch(name){                        
                    case "folder":
                        folders.Add(Utils.PathToCurrentOS(ParseNode((YamlScalarNode)node, CurrentFolderPath)));
                        break;

                    case "path":                            
                        foreach(var folder in Directory.GetDirectories(Utils.PathToCurrentOS(ParseNode((YamlScalarNode)node, CurrentFolderPath))).OrderBy(x => x)) 
                            folders.Add(folder);     

                        break;

                    case "vars":
                        var stack = new Stack<Dictionary<string, object>>();
                        stack.Push(vars);

                        //NOTE: vars should be stored within the current remote execution, not remotelly
                        ParseVars(node, name, current, null, stack);
                        break;
                }
            });

            //Load local folder/path data
            if(node != null){
                if(node.GetType().Equals(typeof(YamlSequenceNode))){
                    ValidateChildren((YamlSequenceNode)node, current, children, mandatory);
                    ForEachChild((YamlSequenceNode)node, parse);
                }
                else if(node.GetType().Equals(typeof(YamlMappingNode))){
                    ValidateChildren((YamlMappingNode)node, current, children, mandatory);
                    ForEachChild((YamlMappingNode)node, parse);
                }
            }

            if(folders.Count == 0) throw new ArgumentNullException("Some 'folder' or 'path' must be defined when using 'local' batch mode.");
            return new Local(folders.ToArray(), vars);
        }

        private Remote ParseRemote(YamlNode node, string current="remote", string parent="single", string[] children = null, string[] mandatory = null){  
            children ??= new string[]{"os", "host", "user", "password", "port", "path", "folder", "vars"};
            mandatory ??= new string[]{"host", "user"};

            var os = OS.GNU;
            var host = string.Empty;
            var user = string.Empty;
            var password = string.Empty;
            var port = 22;
            var folders = new List<string>();
            Dictionary<string, object> vars = new Dictionary<string, object>();

            //NOTE: Could be MappingNode or SequenceNode (within remote and single respectively)
            if(node == null) return new Remote(os, host, user, password, port, folders.ToArray(), vars);
            
            //Load the current data
            AutoComputeVarValues = false;
            ValidateChildren(node, current, children, mandatory);
            ForEachChild(node, new Action<string, YamlScalarNode>((name, node) => { 
                switch(name){
                    case "os":                            
                        os = ParseNode(node, CurrentOS);
                        break;

                    case "host":                            
                        host = ParseNode(node, CurrentHost);
                        break;

                    case "user":                            
                        user = ParseNode(node, CurrentHost);
                        break;

                    case "password":                            
                        password = ParseNode(node, CurrentHost);
                        break;

                    case "port":                            
                        port = ParseNode(node, CurrentPort);
                        break;                                   
                }
            }));

            ForEachChild(node, new Action<string, YamlNode>((name, node) => { 
                switch(name){                   
                    case "folder":                            
                        folders.Add(Utils.PathToRemoteOS(ParseNode((YamlScalarNode)node, CurrentFolderPath), os));
                        break;

                    case "path":                            
                        var remote = new AutoCheck.Core.Connectors.Shell(os, host, user, password, port);
                        foreach(var folder in remote.GetFolders(Utils.PathToRemoteOS(ParseNode((YamlScalarNode)node, CurrentFolderPath), os), "*", false).OrderBy(x => x)) 
                            folders.Add(folder);    

                        break;

                    case "vars":                     
                        var stack = new Stack<Dictionary<string, object>>();
                        stack.Push(vars);

                        //NOTE: vars should be stored within the current remote execution, not remotelly
                        ParseVars(node, name, current, null, stack);
                        break;
                }
            }));
            AutoComputeVarValues = true;

            return new Remote(os, host, user, password, port, folders.ToArray(), vars);
        }
        
        private CopyDetector[] ParseCopyDetector(YamlNode node, Local[] local, Remote[] remote, string current="copy_detector", string parent="batch", string[] children = null, string[] mandatory = null){                        
            children ??= new string[]{"type", "caption", "threshold", "file"};
            mandatory ??= new string[]{"type"};

            //Validations
            var cds = new List<CopyDetector>();            
            if(node == null || !node.GetType().Equals(typeof(YamlMappingNode))) return cds.ToArray();                             
            ValidateChildren(node, current, children, mandatory);                        

            //Loading data
            var threshold = ParseChild(node, "threshold", 1f, false);
            var file = ParseChild(node, "file", "*", false);
            var caption = ParseChild(node, "caption", "Looking for potential copies within ~{$CURRENT_FOLDER_NAME}... ", false);                    
            var type = ParseChild(node, "type", string.Empty);                                    
            if(string.IsNullOrEmpty(type)) throw new ArgumentNullException(type);

            //Running the copy detector
            Output.WriteLine($"Starting the copy detector for ~{type}:", Output.Style.HEADER);                 
            Output.Indent();
            cds.Add(LoadCopyDetector(type, caption, threshold, file, local, remote));
            Output.UnIndent();                        

            return cds.ToArray();
        }

        private void ParseBody(YamlNode node, string current="body", string parent="root", string[] children = null, string[] mandatory = null){
            children ??= new string[]{"vars", "connector", "run", "question", "echo"};

            var question = false;
            if(node == null || !node.GetType().Equals(typeof(YamlSequenceNode))) return;

            //Scope in
            Vars.Push(new Dictionary<string, object>());
            Connectors.Push(new Dictionary<string, object>());
            
            ValidateChildren(node, current, children, mandatory);
            ForEachChild(node, new Action<string, YamlNode>((name, node) => {
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

                     case "echo":
                        ParseEcho(node, name, current);
                        break;                    
                } 
            }));          
            
            if(Abort){
                Output.BreakLine();
                Output.WriteLine("Aborting execution!", Output.Style.ERROR);
                Output.BreakLine();
                TotalScore = 0;
            }            

            //Body ends, so total score can be displayed
            if(question){
                Output.Write("TOTAL SCORE: ", Output.Style.SCORE);
                Output.Write($"{Math.Round(TotalScore, 2).ToString(CultureInfo.InvariantCulture)} / {Math.Round(MaxScore, 2).ToString(CultureInfo.InvariantCulture)}", (TotalScore < MaxScore/2 ? Output.Style.ERROR :Output.Style.SUCCESS));
                Output.BreakLine(2);
            }

            //Scope out
            Vars.Pop();
            Connectors.Pop();
        }

        private void ParseConnector(YamlNode node, string current="connector", string parent="root", string[] children = null, string[] mandatory = null){
            children ??= new string[]{"type", "name", "arguments", "onexception", "caption", "success", "error"};
            if(node == null || !node.GetType().Equals(typeof(YamlMappingNode))) return;

            //Validation before continuing
            var conn = (YamlMappingNode)node;
            ValidateChildren(conn, current, children, mandatory);                 
           
            //Loading connector data
            var type = ParseChild(conn, "type", "SHELL");
            var name = ParseChild(conn, "name", type).ToLower();
            var caption = ParseChild(conn, "caption", string.Empty);            
            var success = ParseChild(conn, "success", "OK");
            var error = ParseChild(conn, "error", "ERROR"); 
            
            //onexcepttion needs a caption
            var onexception = ParseChildWithRequiredCaption(conn, "onexception", "ERROR");

            //Storing instance        
            var scope = Connectors.Peek();
            if(!scope.ContainsKey(name)) scope.Add(name, null);      
            
            //Caption and result
            var exceptions = new List<string>();
            if(!string.IsNullOrEmpty(caption)) Output.Write(caption, Output.Style.DEFAULT); 
            
            try{    
                //Getting the connector's assembly (unable to use name + baseType due inheritance between connectors, for example Odoo -> Postgres)
                Assembly assembly = Assembly.GetExecutingAssembly();
                var assemblyType = assembly.GetTypes().FirstOrDefault(t => t.FullName.Equals($"AutoCheck.Core.Connectors.{type}", StringComparison.InvariantCultureIgnoreCase));
                if(assemblyType == null) throw new ConnectorInvalidException($"Unable to create a connector of type '{type}'");
                                
                //Creating the instance    
                var arguments = conn.Children.ContainsKey("arguments") ? ParseArguments(conn.Children["arguments"]) : null;                           
                var constructor = GetMethod(assemblyType, assemblyType.Name, arguments);                   
                var instance = Activator.CreateInstance(assemblyType, constructor.args);                
                scope[name] = instance;                
            }   
            catch (Exception ex){
                //Some connector instance can fail on execution due to wrong data (depends on users files) like XML because it could try to parse a wrong file.
                //If fails, the exception will be stored so the script will know what failed on creation if the connector is called through a "run".
                switch(onexception){
                    case "ABORT":                        
                    case "SKIP":                    
                    case "ERROR":
                        if(onexception.Equals("ABORT")) Abort = true; 
                        else if(onexception.Equals("SKIP")) Skip = true; 

                        scope[name] = (ex.InnerException == null ? ex : ex.InnerException);
                        exceptions.Add(ExceptionToOutput(scope[name] as Exception));
                        
                        if(IsQuestionOpen) Errors.AddRange(exceptions);
                        break;

                    case "SUCCESS":
                        scope[name] = (ex.InnerException == null ? ex : ex.InnerException);
                        break;

                    default:
                        throw new ArgumentInvalidException($"Invalid value '{onexception}' for the 'onexception' item within 'connector'.");

                }                
            }

            if(!string.IsNullOrEmpty(caption)) Output.WriteResponse(exceptions, success, error);          
        }  

        private void ParseRun(YamlNode node, string current="run", string parent="body", string[] children = null, string[] mandatory = null){
            children ??= new string[]{"connector", "command", "arguments", "expected", "caption", "success", "error", "onexception", "onerror", "store"};
            mandatory ??= new string[]{"command"};

            if(node == null || !node.GetType().Equals(typeof(YamlMappingNode))) return;

            //Validation before continuing
            var run = (YamlMappingNode)node;            
            ValidateChildren(run, current, children, mandatory);                                                     
                       
            //Data is loaded outside the try statement to rise exception on YAML errors
            var name = ParseChild(run, "connector", "LOCALSHELL");     
            var caption = ParseChild(run, "caption", string.Empty);         
            var expected = ParseChild(run, "expected", (object)null);                          
            var command = ParseChild(run, "command", string.Empty);
            var store = ParseChild(run, "store", string.Empty);            
            var error = false;            

            //onexcepttion and onerror needs a caption
            var onexception = ParseChildWithRequiredCaption(run, "onexception", "ERROR");
            var onerror = ParseChildWithRequiredCaption(run, "onerror", "CONTINUE");            

            //Running the command over the connector with the given arguments   
            (object result, bool shellExecuted) data;
            try{                         
                var connector = GetConnector(name); //Could throw an exception if the connector has not been instantiated correctly
                var arguments = (run.Children.ContainsKey("arguments") ? ParseArguments(run.Children["arguments"]) : null); //Could throw an exception if an argument is a connector
                data = InvokeCommand(connector, command, arguments);             
            }
            catch(ArgumentInvalidException){
                //Exception when trying to run the command (command not executed) with invalid arguments, so YAML script is no correct
                throw;
            }
            catch(Exception ex){  
                //Exception on command execution (command executed)                         
                data = (string.Empty, false);

                //processing                
                switch(onexception){
                    case "ERROR":                    
                    case "ABORT":
                    case "SKIP":
                        if(onexception.Equals("ERROR")) error = true;
                        else if(onexception.Equals("ABORT")) Abort = true; 
                        else if(onexception.Equals("SKIP")) Skip = true; 
                        data.result = ExceptionToOutput(ex);                                
                        break;

                    case "SUCCESS":
                        data.result = expected;     //forces match
                        break;  

                    default:
                        throw new NotSupportedException();
                }
            }

            //Parsing the result
            if(data.shellExecuted) Result = ((ValueTuple<int, string>)data.result).Item2;
            else if (data.result == null) Result = "NULL";
            else if(data.result.GetType().IsArray) Result = $"[{string.Join(",", ((Array)data.result).Cast<object>().ToArray())}]";
            else Result = data.result.ToString();                                

            //Storing the result into "store" and into the global var "Result"
            Result = Result.TrimEnd();
            if(!string.IsNullOrEmpty(store)) UpdateVar(store, Result);

            //Expected needed castings
            if(expected != null && expected.GetType().Equals(typeof(float))) expected = ((float)expected).ToString(CultureInfo.InvariantCulture); 

            //Run with no caption will work as silent but will throw an exception on expected missamtch, if no exception wanted, do not use expected. 
            //Run with no caption wont compute within question, computing hidden results can be confuse when reading a report.
            //Running with caption/no-caption but no expected, means all the results will be assumed as OK and will be computed and displayed ONLY if caption is used (excluding unexpected exceptions).
            //Array.ConvertAll<object, string>(data.result, Convert.ToString)
            var info = (Abort || Skip || error ? Result : $"Expected -> {expected}; Found -> {Result}");     
            var match = (!error && !Abort && !Skip);                                    

            if(match) match = (expected == null ? true : 
                (data.result == null ? MatchesExpected(Result, expected.ToString()) : 
                    (data.result.GetType().IsArray ? MatchesExpected((Array)data.result, expected.ToString()) : MatchesExpected(Result, expected.ToString()))
                )
            );

            if(string.IsNullOrEmpty(caption) && !match) throw new ResultMismatchException(info);
            else if(!string.IsNullOrEmpty(caption)){                                                          
                var successCaption = ParseChild(run, "success", "OK");
                var errorCaption = ParseChild(run, "error", "ERROR"); 

                List<string> errors = null;
                if(!match){                                                            
                    //Computing errors when within a question
                    errors = new List<string>(){info}; 
                    if(IsQuestionOpen) Errors.AddRange(errors);

                    switch(onerror){
                        case "ABORT":
                            Abort = true; 
                            break;

                        case "SKIP":
                            Skip = true; 
                            break;

                        case "CONTINUE":
                            break;  

                        default:
                            throw new NotSupportedException();
                    }
                }
                
                Output.Write(caption, Output.Style.DEFAULT);
                Output.WriteResponse(errors, successCaption, errorCaption); 
            }                                   
        }

        private void ParseQuestion(YamlNode node, string current="question", string parent="root", string[] children = null, string[] mandatory = null){
            children ??= new string[]{"score", "caption", "description", "content"};
            mandatory ??= new string[]{"content"};

            if(node == null || !node.GetType().Equals(typeof(YamlMappingNode))) return;

            //Validation before continuing
            var question = (YamlMappingNode)node;
            ValidateChildren(question, current, children, mandatory);     
                        
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
            Errors = new List<string>();

            //Loading question data                        
            CurrentScore = ComputeQuestionScore(question);
            var caption = ParseChild(question, "caption", $"Question {CurrentQuestion} [{Math.Round(CurrentScore, 2).ToString(CultureInfo.InvariantCulture)} {(CurrentScore == 1 ? "point" : "points")}]");
            var description = ParseChild(question, "description", string.Empty);  
            
            //Displaying question caption
            caption = (string.IsNullOrEmpty(description) ? $"{caption}:" : $"{caption} - {description}:");            
            Output.WriteLine(caption, Output.Style.QUESTION);   
            Output.Indent();                        

            //Parse and run question content
            var content = "content";
            if(question.Children.ContainsKey(content)) ParseContent(question.Children[content], content, current);           

            //Compute scores
            float total = Success + Fails;
            TotalScore = (total > 0 ? (Success / total) * MaxScore : 0);      
            Errors = null;   

            //Closing the question (breaklining is performed within content, in order to check for subquestions)                           
            Output.UnIndent();                                    
        }

        private void ParseContent(YamlNode node, string current="content", string parent="question", string[] children = null, string[] mandatory = null){
            children ??= new string[]{"vars", "connector", "run", "question", "echo"};
            if(node == null || !node.GetType().Equals(typeof(YamlSequenceNode))) return;

            //Scope in
            Vars.Push(new Dictionary<string, object>());
            Connectors.Push(new Dictionary<string, object>());

            //Subquestion detection
            var subquestion = false;
            ValidateChildren((YamlSequenceNode)node, current, children, mandatory);
            ForEachChild((YamlSequenceNode)node, new Action<string, YamlNode>((name, node) => {
                switch(name){                   
                    case "question":
                        subquestion = true;
                        return;                        
                } 
            }));
            
            //Recursive content processing
            ForEachChild((YamlSequenceNode)node, new Action<string, YamlNode>((name, node) => {
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

                    case "echo":
                        ParseEcho(node, name, current);
                        break;                            
                } 
            }));                  
            
            //Processing score            
            if(!subquestion || Skip){
                Skip = false;
                if(Errors.Count == 0) Success += CurrentScore;
                else Fails += CurrentScore;

                 //Only breaklining the line within subquestions (otherwise prints an accumulation)
                Output.BreakLine();
            } 

            //Scope out
            Vars.Pop();
            Connectors.Pop(); 
        }

        private void ParseEcho(YamlNode node, string current="echo", string parent="body"){
            if(node == null || !node.GetType().Equals(typeof(YamlScalarNode))) return;
            
            var echo = node.ToString().Trim();
            Output.WriteLine(ComputeVarValue(echo), Output.Style.ECHO);
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
                    if(!input.StartsWith("--")) throw new ArgumentInvalidException("Provided arguments must be as '--argName argValue', avoid using spaces within argument values or surround those with double quotes or single quotes.");
                                    
                    input = input.TrimStart('-');
                    var name = input.Substring(0, input.IndexOf(' '));
                    input = input.Substring(input.IndexOf(' ')+1);
                    
                    var value = string.Empty;
                    char separator = (input.StartsWith('"') ? '"' : (input.StartsWith('\'') ? '\'' : ' '));
                    if(input.Contains(separator)){
                        input = input.TrimStart(separator);                        
                        value = input.Substring(0, input.IndexOf(separator));
                        input = input.Substring(input.IndexOf(separator)+1).TrimEnd(separator);
                    }
                    else{
                        value = input;
                        input = string.Empty;
                    }                    
                    
                    arguments.Add(name, ComputeVarValue(name, value));
                }
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
        private T ParseChild<T>(YamlNode node, string child, T @default, bool compute=true){                 
            var current = GetChildren(node).Where(x => x.Key.ToString().Equals(child)).SingleOrDefault();
            if(current.Key == null){
                if(@default == null || !@default.GetType().Equals(typeof(string))) return @default;
                else return (T)ParseNode(new YamlScalarNode(@default.ToString()), @default, compute); 
            }
            else{
                if(!current.Value.GetType().Equals(typeof(YamlScalarNode))) throw new NotSupportedException("This method only supports YamlScalarNode child nodes.");
                return (T)ParseNode((YamlScalarNode)current.Value,  @default, compute);           
            }           
        }
                 
        private T ParseNode<T>(YamlScalarNode node, T @default, bool compute=true){
            try{
                var parsed = ParseNode(node, compute);

                if(typeof(T).IsEnum) return (T)Enum.Parse(typeof(T), parsed.ToString());
                else if(typeof(T).Equals(typeof(Single)) && parsed.GetType().Equals(typeof(Int32))) parsed = (float)(int)parsed;
                return (T)parsed;
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

        private void ValidateChildren(YamlNode node, string current, string[] expected, string[] mandatory = null){                                 
            //ValidateChildren(GetChildren(node), current, expected, mandatory);

            var found = new List<string>();

            foreach (var entry in GetChildren(node)){
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

        private void ForEachChild<T>(YamlNode node, Action<string, T> action, bool parseEmpty = true) where T: YamlNode{                              
            foreach(var child in GetChildren(node)){
                //Continue if the node type matches or if it's the generic YamlNode
                if(child.Value.GetType().Equals(typeof(T)) || child.Value.GetType().BaseType.Equals(typeof(T))) action.Invoke(child.Key.ToString(), (T)child.Value);
                else if(parseEmpty){
                    //Empty nodes can be treated as the requested type (for example, empty 'extract' will be treated as a YamlMappingNode)
                    if(child.Value.GetType().Equals(typeof(YamlScalarNode)) && string.IsNullOrEmpty(((YamlScalarNode)child.Value).Value)){                        
                        if(typeof(T).Equals(typeof(YamlScalarNode))) action.Invoke(child.Key.ToString(), (T)(YamlNode)(new YamlScalarNode()));  
                        else if(typeof(T).Equals(typeof(YamlMappingNode))) action.Invoke(child.Key.ToString(), (T)(YamlNode)(new YamlMappingNode()));  
                        else if(typeof(T).Equals(typeof(YamlSequenceNode))) action.Invoke(child.Key.ToString(), (T)(YamlNode)(new YamlSequenceNode()));
                        else throw new NotSupportedException();   
                    }                        
                }                
            }
        }

        private IEnumerable<KeyValuePair<YamlNode, YamlNode>> GetChildren(YamlNode node){
            //Note: it's important to add .ToList() at the end to return a copy of the data, otherwise the pointer to this data can change
            if(node.GetType().Equals(typeof(YamlSequenceNode))) return ((YamlSequenceNode)node).Children.SelectMany(x => ((YamlMappingNode)x).Children).ToList();
            else if(node.GetType().Equals(typeof(YamlMappingNode))) return ((YamlMappingNode)node).Children.Select(x => x).ToList();
            else throw new InvalidOperationException("Only YamlMappingNode and YamlSequenceNode can be requested for looping through its children.");
        }
#endregion
#region Helpers
        private void SetupDefaultHostVars(){
            CurrentOS = Utils.CurrentOS;
            CurrentHost = "localhost";
            CurrentPort = 22;
            CurrentUser = string.Empty;
            CurrentPassword = string.Empty;
        }

        private string ParseChildWithRequiredCaption(YamlMappingNode node, string child, string @default){
            var caption = ParseChild(node, "caption", string.Empty);   
            var value = ParseChild(node, child, string.Empty);            
            if(IsQuestionOpen && string.IsNullOrEmpty(caption) && !string.IsNullOrEmpty(value)) throw new DocumentInvalidException($"The '{child}' argument cannot be used without the 'caption' argument within a question.");
            if(string.IsNullOrEmpty(value)) value = @default;

            return value;
        }

        private string ExceptionToOutput(Exception ex){
            if(ex.GetType().Equals(typeof(TargetInvocationException))) ex = ex.InnerException;
            
            var output = ($"{ex.Message.Replace("\n",  $"\n{Output.CurrentIndent}{Output.SingleIndent}")}").TrimEnd(); 
            while(ex.InnerException != null){
                ex = ex.InnerException;
                output += ($" \r\n{Output.CurrentIndent}{Output.SingleIndent}---> {ex.Message.Replace("\n",  $" \n{Output.CurrentIndent}{Output.SingleIndent}")}").TrimEnd();
            }

            return output;
        }  
        
        private void ForEachLocalTarget(Local[] local, Action<string> action){
            var originalFolder = CurrentFolderPath;

            foreach(var l in local){
                //local target vars should be loaded
                Vars.Push(l.Vars);
                foreach(var folder in l.Folders){
                    CurrentFolderPath = folder;
                    action.Invoke(folder);
                }    
                Vars.Pop();
            }

            CurrentFolderPath = originalFolder;
        }

        private void ForEachRemoteTarget(Remote[] remote, Action<OS, string, string, string, int, string> action){
            var originalHost = CurrentHost;
            var originalUser = CurrentUser;
            var originalPort = CurrentPort;
            var originalPassword = CurrentPassword;            
            var originalFolder = CurrentFolderPath;            

            foreach(var r in remote){
                CurrentHost = r.Host;
                CurrentUser = r.User;
                CurrentPort = r.Port;
                CurrentPassword = r.Password;    

                //local target vars should be loaded
                Vars.Push(r.Vars);            

                if(r.Folders.Count() == 0) action.Invoke(r.OS, r.Host, r.User, r.Password, r.Port, null);
                else{
                    foreach(var folder in r.Folders){
                        CurrentFolderPath = folder;
                        action.Invoke(r.OS, r.Host, r.User, r.Password, r.Port, folder);
                    }
                }

                Vars.Pop();
            }    

            CurrentFolderPath = originalFolder;
            CurrentPassword = originalPassword;
            CurrentUser = originalUser;
            CurrentHost = originalHost;
            CurrentPort = originalPort;
        }
        
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
                if(args != null && pending.Count == 0){
                    if(info.IsGenericMethodDefinition) return (((MethodInfo)info).MakeGenericMethod(typeof(object)), args.ToArray());
                    else return (info, args.ToArray());   
                } 
            }
            
            //No bind has been found
            throw new ArgumentInvalidException($"Unable to find any {(constructor ? "constructor" : $"method called '{method}'")} for the Connector '{type.Name}' that matches with the given set of arguments.");
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
                shellExecuted = connector.GetType().Equals(typeof(Connectors.Shell)) || connector.GetType().IsSubclassOf(typeof(Connectors.Shell));  
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
                    if(replace.Equals("NOW")) replace = Now;
                    else{                                                 
                        replace = string.Format(CultureInfo.InvariantCulture, "{0}", GetVar(replace.ToLower()));
                        if(!string.IsNullOrEmpty(regex)){
                            try{
                                if(Utils.CurrentOS != Utils.OS.WIN) regex = regex.Replace("\\\\", "/"); //TODO: this is a workaround to get the last folder of a path on WIN and UNIX... think something less dirty...
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
            if(string.IsNullOrEmpty(tag)){
                bool boolValue;
                if(bool.TryParse(value, out boolValue)) return boolValue;

                int intValue;
                if(int.TryParse(value, out intValue)) return intValue;

                float floatValue;
                if(value.ToCharArray().Where(x => x.Equals('.')).Count() == 1 && float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out floatValue)) return floatValue;

                return value;
            } 
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

            if(!subquestion) return ParseChild<float>(root, "score", 1f, false);
            else return score;
        }

        private bool MatchesExpected(Array current, string expected){
            var match = false;
            //expected = expected.ToUpper().TrimStart();
            expected = expected.TrimStart();

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
                var source = new List<string>((IEnumerable<string>)current).OrderBy(x => x);
                expected = expected.Substring(15).Trim();
                match = source.SequenceEqual(expected.TrimStart('[').TrimEnd(']').Split(",").OrderBy(x => x));

            }
            else{                
                var source = new List<string>((IEnumerable<string>)current);
                if(expected.StartsWith("ORDEREDEQUALS")) expected = expected.Substring(13).Trim();
                match = source.SequenceEqual(expected.TrimStart('[').TrimEnd(']').Split(","));
            }            
            
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
                var file = Path.Combine(Path.GetDirectoryName(path), Utils.PathToCurrentOS(inherits));
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

        private void SetupLog(string logFolderPath, LogFormatType logFormatType, string logFileName, bool enabled){            
            LogFolderPath =  logFolderPath;                    
            LogFormat = logFormatType;
            LogFilePath = Path.Combine(logFolderPath, $"{Path.GetFileNameWithoutExtension(logFileName)}.log");
            LogFilesEnabled = enabled;
        }

        private string GetLog(){
            switch(LogFormat){
                case LogFormatType.TEXT:
                    return Output.ToText().LastOrDefault();

                case LogFormatType.JSON:
                    return Output.ToJson().LastOrDefault();

                default:
                    return null;
            }
        }
        
        private void ExportLog(){           
            //Preparing the output files and folders if enabled                                               
            if(LogFilesEnabled){  
                string[] logs = null;

                 switch(LogFormat){
                    case LogFormatType.TEXT:
                        logs = Output.ToText();
                        break;

                    case LogFormatType.JSON:
                        logs = Output.ToJson();
                        break;

                    default:
                        logs = new string[0];
                        break;
                }

                for(int i=0; i<LogFiles.Count; i++){
                    //WARNING: this could fail if multithreading is implemented
                    var logContent = logs[i];
                    var logFile = LogFiles[i];
                    var logFolder = Path.GetDirectoryName(logFile);

                    //Writing log output if needed                                        
                    if(!Directory.Exists(logFolder)) Directory.CreateDirectory(logFolder);
                    if(File.Exists(logFile)) File.Delete(logFile);

                    //Retry if the log file is bussy
                    Utils.RunWithRetry<IOException>(new Action(() => {                    
                        File.WriteAllText(logFile, logContent);
                    }));
                }                              
            }
        }
        private string CleanPathInvalidChars(string path){
            var file = string.Empty;
            var folder = string.Empty;
            
            if(string.IsNullOrEmpty(Path.GetExtension(path))) folder = path;           
            else
            {
                file = Path.GetFileName(path);
                folder = Path.GetDirectoryName(path);
            }

            foreach(char c in Path.GetInvalidPathChars())
                folder = folder.Replace(c.ToString(), "");

            foreach(char c in Path.GetInvalidFileNameChars())
                file = file.Replace(c.ToString(), "");

            if(string.IsNullOrEmpty(file)) return folder;
            else return Path.Combine(folder, file);
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
            catch (ItemNotFoundException ex){
                throw new VariableNotFoundException($"Undefined variable {name} has been requested.", ex);
            }            
        }

        private void UpdateVar(string name, object value, Stack<Dictionary<string, object>> stack = null){
            name = name.ToLower();
            stack ??= Vars;

            if(name.StartsWith("$")){
                //Only update var within upper scopes
                var current = stack.Pop();  
                name = name.TrimStart('$');
                
                try{ 
                    var found = FindScope(stack, name);
                    found[name] = value;
                }
                catch (ItemNotFoundException){
                    throw new VariableNotFoundException($"Undefined upper-scope variable {name} has been requested.");
                }  
                finally{ 
                    stack.Push(current); 
                }  
            }
            else{
                //Create or update var within current scope
                var current = stack.Peek();
                if(!current.ContainsKey(name)) current.Add(name, null);
                current[name] = value;
            }           
        }       

        private object GetConnector(string name, bool @default = true){     
            try{
                var conn = FindItemWithinScope(Connectors, name);
                if(conn.GetType().IsSubclassOf(typeof(Core.Connectors.Base))) return conn;
                else throw new ConnectorInvalidException($"Unable to use the connector named '{name}' because it couldn't be instantiated.", (Exception)conn);
            }      
            catch(ItemNotFoundException){
                if(@default) return new Connectors.Shell();
                else throw new ConnectorNotFoundException($"Unable to find any connector named '{name}'.");
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
        private CopyDetector LoadCopyDetector(string type, string caption, float threshold, string filePattern, Local[] local, Remote[] remote){                        
            Assembly assembly = Assembly.GetExecutingAssembly();
            if(assembly == null) throw new ArgumentInvalidException("type");

            //Loading documents
            var assemblyType = assembly.GetTypes().Where(t => t.Name.Equals(type, StringComparison.InvariantCultureIgnoreCase) && t.IsSubclassOf(typeof(CopyDetector))).FirstOrDefault();                       
            if(assemblyType == null) throw new ArgumentInvalidException("type");

            var cd = (CopyDetector)Activator.CreateInstance(assemblyType, new object[]{threshold, filePattern}); 

            //Compute for each local folder
            ForEachLocalTarget(local, (folder) => {
                try{
                    Output.Write(ComputeVarValue(caption), Output.Style.DETAILS);                    
                    cd.Load(folder);                    
                    Output.WriteResponse();
                }
                catch (Exception e){
                    Output.WriteResponse(e.Message);
                } 
            });

            //Compute for each remote local folder
            ForEachRemoteTarget(remote, (os, host, username, password, port, folder) => {
                try{
                    Output.Write(ComputeVarValue(caption), Output.Style.DETAILS);                    
                    cd.Load(os, host, username, password, port, folder);                    
                    Output.WriteResponse();
                }
                catch (Exception e){
                    Output.WriteResponse(e.Message);
                } 
            });                               

            //Compare
            if(cd.Count > 0) cd.Compare();
            return cd;
        }                          
        
        private void PrintCopies(CopyDetector cd, string folder){                        
            var details = cd.GetDetails(folder);
            folder = Path.GetDirectoryName(folder);
            folder = details.file.Substring(folder.Length).TrimStart(Path.DirectorySeparatorChar);

            Output.WriteLine($"Potential copy detected for ~{folder}:", Output.Style.CRITICAL);                                                      
            Output.Indent();

            foreach(var item in details.matches){  
                folder = Path.GetDirectoryName(item.folder);
                folder = item.file.Substring(folder.Length).TrimStart(Path.DirectorySeparatorChar);

                Output.Write($"Match score with ~{folder}... ", Output.Style.DETAILS);     
                Output.WriteLine(string.Format("{0:P2} ", item.match), (item.match < cd.Threshold ?Output.Style.SUCCESS : Output.Style.ERROR));
            }
            
            Output.UnIndent();
            Output.BreakLine();
        }
#endregion
    }
}