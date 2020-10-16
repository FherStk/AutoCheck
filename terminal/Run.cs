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
using System.Reflection;
using System.Collections.Generic;
using AutoCheck.Core;

/// <summary>
/// Main
/// </summary>
namespace AutoCheck.Terminal
{
    class Run
    {     
        // private enum ScriptTarget{
        //     SINGLE,
        //     BATCH,
        //     NONE
        // }         

        static void Main(string[] args)
        {
            var output = new Output();
            output.BreakLine();
            output.Write("AutoCheck: ", ConsoleColor.Yellow);                        
            output.WriteLine("v1.0.0.0 (alpha-2)");
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

            if(!arguments.ContainsKey("script"))  output.WriteLine("The 'script' argument must be provided.", ConsoleColor.Red);
            else{
                string script = arguments["script"].ToString();
                
                try{
                    if(!File.Exists(script)) output.WriteLine("Unable to find the provided script.", ConsoleColor.Red);                    
                    else new AutoCheck.Core.Script(script);
                }
                catch{
                    output.WriteLine("The 'script' argument must be a valid file path.", ConsoleColor.Red);   
                }
            }            
        }                                                              
    }
}
