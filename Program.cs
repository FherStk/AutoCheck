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
using System.Linq;
using System.Reflection;
using AutoCheck.Core;

/// <summary>
/// Main
/// </summary>
namespace AutoCheck
{
    class Program
    {     
        private enum ScriptTarget{
            SINGLE,
            BATCH,
            NONE
        }         

        static void Main(string[] args)
        {            
            Output.Instance.BreakLine();
            Output.Instance.Write("Automated Assignment Validator: ", ConsoleColor.Yellow);                        
            Output.Instance.WriteLine("v2.3.0.0 (alpha)");
            Output.Instance.Write(String.Format("Copyright © {0}: ", DateTime.Now.Year), ConsoleColor.Yellow);            
            Output.Instance.WriteLine("Fernando Porrino Serrano.");
            Output.Instance.Write(String.Format("Under the AGPL license: ", DateTime.Now.Year), ConsoleColor.Yellow);            
            Output.Instance.WriteLine("https://github.com/FherStk/AutoCheck/blob/master/LICENSE");
            Output.Instance.BreakLine();

            LaunchScript(args);
        }  
        private static void LaunchScript(string[] args){
            object script = null;
            Type type = null;
            ScriptTarget target = ScriptTarget.NONE;

            for(int i = 0; i < args.Length; i++){
                if(args[i].StartsWith("--") && args[i].Contains("=")){
                    string[] data = args[i].Split("=");
                    string param = data[0].ToLower().Trim().Replace("\"", "").Substring(2);
                    string value = data[1].Trim().Replace("\"", "");
                    
                    switch(param){
                        case "script":
                            Assembly assembly = Assembly.GetExecutingAssembly();
                            type = assembly.GetTypes().First(t => t.Name == value);
                            script = Activator.CreateInstance(type, new string[][]{args});                        
                            break;
                        
                        case "target":
                            target = (ScriptTarget)Enum.Parse(typeof(ScriptTarget), value, true);
                            break;
                    }                                  
                }                                
            }

            if(target == ScriptTarget.NONE)
                Output.Instance.WriteLine("Unable to launch the script: a 'target' parameter was expected or its value is not correct.", ConsoleColor.Red);

            else if(script == null)
                Output.Instance.WriteLine("Unable to launch the script: none has been found with the given name.", ConsoleColor.Red);

            else{                
                MethodInfo methodInfo = null;
                if(target == ScriptTarget.BATCH) methodInfo = type.GetMethod("Batch");
                else if(target == ScriptTarget.SINGLE) methodInfo = type.GetMethod("Run");                
                
                try{                    
                    methodInfo.Invoke(script, null);
                }
                catch(Exception ex){
                    Output.Instance.BreakLine();
                    Output.Instance.BreakLine();
                    Output.Instance.WriteLine(string.Format("UNHANDLED EXCEPTION: {0}", ex), ConsoleColor.Red);
                }                
            }
        }                                                      
    }
}
