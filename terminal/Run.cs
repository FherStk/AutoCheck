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
using System.Collections.Generic;
using AutoCheck.Core;
using AutoCheck.Core.Connectors;

namespace AutoCheck.Terminal
{
    class Run
    {     
        static void Main(string[] args)
        {
            //TODO: check for updates and ask to the user if wants to update the app before continuing (use git to check for updates)

            var output = new Output();
            output.BreakLine();        
            output.WriteLine($"AutoCheck: ~v{typeof(AutoCheck.Terminal.Run).Assembly.GetName().Version} (Core v{typeof(AutoCheck.Core.Script).Assembly.GetName().Version})", ConsoleColor.Yellow, ConsoleColor.White);            
            output.WriteLine($"Copyright © {DateTime.Now.Year}: ~Fernando Porrino Serrano.", ConsoleColor.Yellow, ConsoleColor.White);            
            output.WriteLine("Under the AGPL license: ~https://github.com/FherStk/AutoCheck/blob/master/LICENSE~", ConsoleColor.Yellow, ConsoleColor.White);            
            output.BreakLine();              

            switch(args[0]){
                case "update":
                    Update(output, false);        
                    break;

                default:
                    Update(output, true);
                    output.BreakLine();
                    Script(args[0], output);                    
                    break;
            }  

            output.BreakLine(); 
        }

        private static void Update(Output output, bool prompt){
            var shell = new LocalShell();
            output.WriteLine("Checking for updates:", ConsoleColor.Blue);
            output.Indent();

            output.Write("Retrieving the list of changes... ");
            var result = shell.RunCommand("git remote update");
            if(result.code == 0) output.WriteResponse(new List<string>());
            else
            {
                output.WriteResponse(result.response);
                return;
            } 

            output.Write("Looking for new versions... ");
            result = shell.RunCommand("git status -uno");
            if(result.code == 0) output.WriteResponse(new List<string>());
            else{
                output.WriteResponse(result.response);
                return;
            } 
            
            output.UnIndent();
            output.BreakLine();            
            if(string.IsNullOrEmpty(result.response)){
                output.WriteLine("AutoCheck is up to date.", ConsoleColor.Green);
                return;
            } 

            var update = true;
            if(prompt){
                output.WriteLine("A new verions of AutoCheck is available, do you want to update before continue [Y/n]?:", ConsoleColor.Magenta);
                update = (Console.ReadLine() is "Y" or "y" or "");
                output.BreakLine();     
            }

            if(!update) {
                output.WriteLine("AutoCheck has not been updated.", ConsoleColor.Red);
                return;
            }

            output.WriteLine("Starting update:", ConsoleColor.Blue);
            output.Indent();

            output.Write("Updating local database... ");
            //result = shell.RunCommand("git fetch --all");
            if(result.code == 0) output.WriteResponse(new List<string>());
            else
            {
                output.WriteResponse(result.response);
                return;
            } 

            output.Write("Removing local changes... ");
            //result = shell.RunCommand("git reset --hard origin/master");
            if(result.code == 0) output.WriteResponse(new List<string>());
            else
            {
                output.WriteResponse(result.response);
                return;
            } 

            output.Write("Downloading updates... ");
            //result = shell.RunCommand("git pull");
            if(result.code == 0) output.WriteResponse(new List<string>());
            else
            {
                output.WriteResponse(result.response);
                return;
            } 

            output.UnIndent();
            output.BreakLine();                            
            output.WriteLine("AutoCheck has been updated", ConsoleColor.Green);
        }
        
        private static void Script(string script, Output output){
            script = Utils.PathToCurrentOS(script);            

            if(string.IsNullOrEmpty(script)) output.WriteLine("ERROR: A path to a 'script' file must be provided.", ConsoleColor.Red);
            else if(!File.Exists(script)) output.WriteLine("ERROR: Unable to find any 'script' file using the provided path.", ConsoleColor.Red);
            else{
                try{
                    new Script(script);
                }
                catch(Exception ex){
                    output.BreakLine();
                    output.WriteLine($"ERROR: {ex.Message}", ConsoleColor.Red);   
                    
                    while(ex.InnerException != null){
                        ex = ex.InnerException;
                        output.WriteLine($"{Output.SingleIndent}---> {ex.Message}", ConsoleColor.Red);   
                    }

                    output.BreakLine();
                }
            }      
        }
    }
}
