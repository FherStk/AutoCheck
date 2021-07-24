/*
    Copyright © 2021 Fernando Porrino Serrano
    Third party software licenses can be found at /docs/credits/credits.md

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
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using AutoCheck.Core;
using AutoCheck.Core.Connectors;

namespace AutoCheck.Cli
{
    class Run
    { 
        private static bool _NO_PAUSE = false;

        static void Main(string[] args)
        {
            //TODO: check for updates and ask to the user if wants to update the app before continuing (use git to check for updates)                
            var output = new Output(true);

            output.BreakLine();        
            output.WriteLine($"AutoCheck's Command Line Interface: ~v{GetProductVersion(Assembly.GetExecutingAssembly())} (Core v{GetProductVersion(typeof(AutoCheck.Core.Script).Assembly)})", Output.Style.INFO);            
            output.WriteLine($"Copyright © {DateTime.Now.Year}: ~Fernando Porrino Serrano.", Output.Style.INFO);            
            output.WriteLine("Under the AGPL license: ~https://github.com/FherStk/AutoCheck/blob/master/LICENSE~", Output.Style.INFO);            
            output.BreakLine();                                   

            var u = false;
            var nu = false;
            var script = string.Empty;

            foreach(var arg in args){
                switch(arg){
                    case "--update":
                    case "-u":
                        u = true;
                        break;

                    case "--no-update":
                    case "-nu":
                        nu = true;
                        break;

                    case "--no-pause":
                    case "-np":
                        _NO_PAUSE = true;
                        break;

                    default:
                        script = arg;
                        break;
                }  
            }

            if(args.Length == 0) Info("Please, provide the correct arguments in order to run AutoCheck.", output);                
            else if(nu && string.IsNullOrEmpty(script)) Info("Please, provide a YAML script path in order to run AutoCheck.", output);                
            else if(!string.IsNullOrEmpty(script) && !File.Exists(script)) Info("Unable to find the provided YAML script, please provide a correct path in order to run AutoCheck.", output);                
            else{
                var update = (u || (!nu && !string.IsNullOrEmpty(script)));
                if(update){
                    var updated = Update(output);
                    output.BreakLine();
                    
                    if(updated && !string.IsNullOrEmpty(script)){
                        //If correctly updated and a script has been provided, its necessary to restart the script in order to build and use the new version; otherwise the old one (current one) would be used. 
                        var file = Path.Combine("utils", (Core.Utils.CurrentOS == Utils.OS.WIN ? "restart.bat" : "restart.sh"));

                        if(Core.Utils.CurrentOS == Utils.OS.GNU){
                            //On Ubuntu, the sh file needs execution permissions
                            var chmod = new Process
                            {
                                StartInfo = new ProcessStartInfo
                                {
                                    RedirectStandardOutput = false,
                                    UseShellExecute = false,
                                    WorkingDirectory = Environment.CurrentDirectory,
                                    FileName = "/bin/bash",
                                    Arguments = $"-c \"chmod +x {file}\""
                                }
                            }; 

                            chmod.Start();
                            chmod.WaitForExit();
                        }

                        var restart = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                RedirectStandardOutput = false,
                                UseShellExecute = false,
                                WorkingDirectory = Environment.CurrentDirectory,
                                FileName = file,
                                Arguments = script
                            }
                        };                                                                                                

                        output.BreakLine();
                        output.WriteLine("Restarting, please wait...", Output.Style.PROMPT);
                        output.BreakLine();
                        restart.Start();
                        return;                   
                    }
                }            
                
                if(!string.IsNullOrEmpty(script)){
                    //Just script exec
                    Script(script, output);    
                    output.BreakLine(); 
                } 
            }  
        }

        private static bool Update(Output output){
            var shell = new Shell();
            output.WriteLine("Checking for updates:", Output.Style.HEADER);
            output.Indent();

            output.Write("Retrieving the list of changes... ");
            var result = shell.RunCommand("git remote update");
            if(result.code == 0) output.WriteResponse(new List<string>());
            else
            {
                output.WriteResponse(result.response);
                return false;
            } 

            output.Write("Looking for new versions... ");            
            result = shell.RunCommand((Core.Utils.CurrentOS == Utils.OS.WIN ? "set LC_ALL=C.UTF-8 & git status -uno" : "LC_ALL=C git status -uno"));
            if(result.code == 0) output.WriteResponse(new List<string>());
            else{
                output.WriteResponse(result.response);
                return false;
            } 
            
            output.UnIndent();
            output.BreakLine();            
            if(result.response.Contains("Your branch is up to date with 'origin/master'")){
                output.WriteLine("AutoCheck is up to date.", Output.Style.SUCCESS);
                return false;
            } 

            output.WriteLine("A new version of AutoCheck is available, YAML script files within 'AutoCheck\\scripts\\custom\' folder will be preserved but all other changes you made will be reverted. Do you still want to update [Y/n]?:", Output.Style.PROMPT);
            
            var update = (Console.ReadLine() is "Y" or "y" or "");
            output.BreakLine();                 

            if(!update) {
                output.WriteLine("AutoCheck has not been updated.", Output.Style.ERROR);
                return false;
            }

            output.WriteLine("Starting update:", Output.Style.HEADER);
            output.Indent();

            output.Write("Updating local database... ");
            result = shell.RunCommand("git fetch --all");
            if(result.code == 0) output.WriteResponse(new List<string>());
            else
            {
                output.WriteResponse(result.response);
                return false;
            } 

            output.Write("Removing local changes... ");
            result = shell.RunCommand("git reset --hard origin/master");
            if(result.code == 0) output.WriteResponse(new List<string>());
            else
            {
                output.WriteResponse(result.response);
                return false;
            } 

            output.Write("Downloading updates... ");
            result = shell.RunCommand("git pull");
            if(result.code == 0) output.WriteResponse(new List<string>());
            else
            {
                output.WriteResponse(result.response);
                return false;
            } 

            output.UnIndent();
            output.BreakLine();                            
            output.WriteLine("AutoCheck has been updated.", Output.Style.SUCCESS);

            return true;
        }
        
        private static void Script(string script, Output output){
            script = Utils.PathToCurrentOS(script);            

            if(string.IsNullOrEmpty(script)) output.WriteLine("ERROR: A path to a 'script' file must be provided.", Output.Style.ERROR);
            else if(!File.Exists(script)) output.WriteLine("ERROR: Unable to find any 'script' file using the provided path.", Output.Style.ERROR);
            else{
                try{
                    new Script(script, OnLogGenerated);
                }
                catch(Exception ex){
                    output.BreakLine();
                    output.WriteLine($"ERROR: {ex.Message}", Output.Style.ERROR);   
                    
                    while(ex.InnerException != null){
                        ex = ex.InnerException;
                        output.WriteLine($"{Output.SingleIndent}---> {ex.Message}", Output.Style.ERROR);   
                    }

                    output.BreakLine();
                }
            }      
        }

        private static void OnLogGenerated(object sender, Script.LogGeneratedEventArgs e){
            var script = (Script)sender;
            script.Output.SendToTerminal(e.Log);

            if(e.Type == Output.Type.SCRIPT && e.ExecutionMode == Core.Script.ExecutionModeType.BATCH && !_NO_PAUSE){
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.WriteLine();
                Console.WriteLine();
            }            
        }

        private static void Info(string message, Output output){
            output.WriteLine(message, Output.Style.ERROR);
            output.BreakLine(); 

            output.WriteLine("dotnet run [arguments] FILE_PATH: ", Output.Style.HEADER);
            output.WriteLine("Allowed arguments: ", Output.Style.HEADER);
            output.Indent();

            output.WriteLine("-u, --update: ~updates the application.", Output.Style.DETAILS);
            output.WriteLine("-nu, --no-update: ~disables the update mechanism.", Output.Style.DETAILS);
            output.WriteLine("-np, --no-pause: ~disables the pause between batch script executions.", Output.Style.DETAILS);
            output.WriteLine("-no, --no-output: ~disables the terminal output.", Output.Style.DETAILS);
            output.WriteLine("FILE_PATH: ~updated the application and executes the given YAML script.", Output.Style.DETAILS);                

            output.BreakLine(); 
        }

        private static string GetProductVersion(Assembly assembly){
            return ((AssemblyInformationalVersionAttribute)assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false).FirstOrDefault()).InformationalVersion;
        }
    }
}
