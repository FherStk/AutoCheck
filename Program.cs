using System;
using System.Linq;
using System.Reflection;
using AutomatedAssignmentValidator.Core;

/// <summary>
/// Main
/// </summary>
namespace AutomatedAssignmentValidator
{
    //TODO: Write this into developer's documentation
    /*
        Script -> Behaviour about what and how to check. Uses the Checkers, can also uses Connectors. Designed to be easy to build.
        Checker -> Computes score and shows messages to the terminal. Uses the Connectors. Desgined to be resusable.
        Connectors -> Interface to get data (SQL, HTML, etc.) with no terminal output. Desgined to be resusable.        
    */

    //TODO: rename the app with a nice-looking name, options:
    //      Automator   (because automates assignement checking, validation and scoring)                    <- too serious
    //      Automata    (because automates assignement checking, validation and scoring)                    <- too serious
    //      RoboTeacher (because automates assignement checking, validation and scoring)                    <- too serious
    //      Bulldozer   (because can handle a lot of ward work with less effort)                            <- funny
    //      Prometheus  (because is the greec god which gived the fire as a present for helping humanity)   <- presumptuous
    //      Seth        (because is the egiptian deity of brute force, of the tumultuous, the unstoppable)  <- funny
    //      AutoCheck   (because automates assignement checking, validation and scoring)                    <- too serious

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
            Output.Instance.WriteLine("v2.0.0.0");
            Output.Instance.Write(String.Format("Copyright © {0}: ", DateTime.Now.Year), ConsoleColor.Yellow);            
            Output.Instance.WriteLine("Fernando Porrino Serrano.");
            Output.Instance.Write(String.Format("Under the AGPL license: ", DateTime.Now.Year), ConsoleColor.Yellow);            
            Output.Instance.WriteLine("https://github.com/FherStk/AutomatedAssignmentValidator/blob/master/LICENSE");
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
