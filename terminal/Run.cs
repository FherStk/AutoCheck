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

namespace AutoCheck.Terminal
{
    class Run
    {     
        static void Main(string[] args)
        {
            //TODO: check for updates and ask to the user if wants to update the app before continuing (use git to check for updates)

            var output = new Output();
            output.BreakLine();        
            output.Write("AutoCheck: ", ConsoleColor.Yellow);
            output.WriteLine($"v{typeof(AutoCheck.Terminal.Run).Assembly.GetName().Version} (Core v{typeof(AutoCheck.Core.Script).Assembly.GetName().Version})");
            output.Write($"Copyright © {DateTime.Now.Year}: ", ConsoleColor.Yellow);            
            output.WriteLine("Fernando Porrino Serrano.");
            output.Write("Under the AGPL license: ", ConsoleColor.Yellow);            
            output.WriteLine("https://github.com/FherStk/AutoCheck/blob/master/LICENSE");
            output.BreakLine();
            
            var arguments = new Dictionary<string, string>(); 
            for(int i = 0; i < args.Length; i++){
                if(args[i].StartsWith("--") && args[i].Contains("=")){
                    string[] data = args[i].Split("=");
                    string param = data[0].ToLower().Trim().Replace("\"", "").Substring(2);
                    string value = data[1].Trim().Replace("\"", "");
                    arguments.Add(param, value);                    
                }
            }

            if(!arguments.ContainsKey("script"))  output.WriteLine("ERROR: The 'script' argument must be provided.", ConsoleColor.Red);
            else{            
                try{
                    string script = Utils.PathToCurrentOS(arguments["script"]);
                    if(!File.Exists(script)) output.WriteLine("ERROR: Unable to find the provided script.", ConsoleColor.Red);                    
                    else new AutoCheck.Core.Script(script);
                }
                catch(FileNotFoundException){
                    output.WriteLine("ERROR: The 'script' argument must be a valid file path.", ConsoleColor.Red);   
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
