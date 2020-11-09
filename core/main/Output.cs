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
using System.Collections.Generic;

namespace AutoCheck.Core{
    /// <summary>
    /// This class is in charge of writing the output into the terminal.    
    /// </summary>
    public class Output{     
        //Note: should be a singletone but cannot be due testing...
        public enum Mode {
            SILENT,
            VERBOSE
        }

        public const string SingleIndent = "   ";
        public string CurrentIndent {get; private set;}
        private bool NewLine {get; set;}      
        private List<List<string>> FullLog {get; set;}              //The log can be splitted into separate files        
        private List<string> Log {                                  //The current log will be always the last one
            get {
                return FullLog.LastOrDefault();
            }
        }         
                                
        public Output(){                                         
            FullLog = new List<List<string>>();            
            BreakLog();
        }
        
        /// <summary>
        /// Changes the output mode.
        /// </summary>
        /// <param name="Mode">Requested output mode</param>
        public static void SetMode(Mode mode){            
            switch(mode){
                case Mode.VERBOSE:
                    var standardOutput = new StreamWriter(Console.OpenStandardOutput());
                    standardOutput.AutoFlush = true;
                    Console.SetOut(standardOutput);

                    var standardError = new StreamWriter(Console.OpenStandardError());
                    standardError.AutoFlush = true;
                    Console.SetError(standardError);
                    break;
                
                case Mode.SILENT:
                    Console.SetOut(new StringWriter());
                    Console.SetError(new StringWriter());                    
                    break;
            }
        }

        /// <summary>
        /// Gets the current output mode.
        /// </summary>
        /// <returns></returns>
        public static Mode GetMode(){ 
            //Note: IsOutputRedirected does not work properly when tests are run directly from terminal            
            return (Console.IsErrorRedirected ? Mode.SILENT : Mode.VERBOSE);            
        }

        /// <summary>
        /// Returns the Output history as an string array, where each string represents a separated log file using \r\n as breaklines.
        /// </summary>
        /// <returns></returns>
        public string[] ToArray(){
            List<string> result = new List<string>();

            foreach(var log in FullLog){
                string output = string.Empty;
                
                foreach(string line in log)
                    output = $"{output}{line}\r\n";
                
                result.Add(output.TrimEnd("\r\n".ToCharArray()));
            }
        
            return result.ToArray();
        }

        /// <summary>
        /// Returns the Output history as an string, using \r\n as breaklines.
        /// </summary>
        /// <returns></returns>
        public new string ToString(){
            return string.Join("\r\n", ToArray()).TrimEnd("\r\n".ToCharArray()).TrimEnd(' ');
        }            
        
        // /// <summary>
        // /// Returns the Output history as an string, using HTML notation.
        // /// </summary>
        // /// <returns></returns>
        // public string ToHTML(string title = ""){
        //     string output = string.Empty;
        //     foreach(string line in Log)
        //         output = $"{output}{line}<br/>";

        //     return $"<html lang='en'><head><meta charset='utf-8'><title>{title}</title></head><body><p>{output.Trim()}</p></body></html>";
        // }          

        /// <summary>
        /// Send new text to the output, no breakline will be added to the end.
        /// The text will be printed in gray, and everything between '~' symbols will be printed using a secondary color (or till the last ':' or '...' symbols).
        /// </summary>
        /// <param name="text">The text to display</param>
        /// <param name="color">The color used to print the whole text or just the secondary one.</param>
        public void Write(string text, ConsoleColor color = ConsoleColor.Gray){
            WriteColor(text, color, false);
        }  
        
        /// <summary>
        /// Send new text to the output, a breakline will be added to the end.
        /// The text will be printed in gray, and everything between '~' symbols will be printed using a secondary color (or till the last ':' or '...' symbols).
        /// </summary>
        /// <param name="text">The text to display</param>
        /// <param name="color">The color used to print the whole text or just the secondary one.</param>
        public void WriteLine(string text, ConsoleColor color = ConsoleColor.Gray){
            WriteColor(text, color, true);
        } 
        
        /// <summary>
        /// Given a list of strings containing the errors found during a script execution (usually the return of a checker's method), the output will be as follows:
        /// 'OK' in green if the errors list is empty; 'ERROR' in red, followed by all the errors descriptions (as a list), otherwise.
        /// </summary>
        /// <param name="errors">A list of errors, usually the return of a checker's method.</param>            
        public void WriteResponse(List<string> errors = null, string captionOk="OK", string captionError="ERROR"){
            if(errors == null || errors.Count == 0) WriteLine(captionOk, ConsoleColor.DarkGreen);
            else if(errors.Where(x => x.Length > 0).Count() == 0) WriteLine(captionError, ConsoleColor.Red);
            else{
                Indent();
                string prefix = $"\n{CurrentIndent}-";
                UnIndent();

                WriteLine($"{captionError}:{prefix}{string.Join(prefix, errors)}", ConsoleColor.Red);
            }
        }
        
        /// <summary>
        /// Given a string containing an error description, the output will be as follows:
        /// 'ERROR' in red, followed by the errors description if the error is not empty; just 'ERROR' in red otherwise.
        /// </summary>
        /// <param name="errors">A list of errors, usually the return of a checker's method.</param>          
        public void WriteResponse(string error, string captionOk="OK", string captionError="ERROR"){
            WriteResponse(new List<string>(){error}, captionOk, captionError);
        }   
        
        /// <summary>
        /// Adds an indentation (3 whitespaces) to the output.
        /// </summary>                                   
        public void Indent(){
            CurrentIndent = $"{CurrentIndent}{SingleIndent}";
        }
        
        /// <summary>
        /// Removes an indentation (3 whitespaces) from the output.
        /// </summary>    
        public void UnIndent(){
            if(CurrentIndent.Length > 0) CurrentIndent = CurrentIndent.Substring(0, CurrentIndent.Length-SingleIndent.Length);
        }
        
        /// <summary>
        /// Resets the output indentation.
        /// </summary>    
        public void ResetIndent(){
           CurrentIndent = "";
           NewLine = true;
        }
        
        /// <summary>
        /// Writes a set of breakline into the output.
        /// </summary>
        /// <param name="lines">The amount of breaklines.</param>
        public void BreakLine(int lines = 1){            
            for(int i=0; i < lines; i++){
                Console.WriteLine();
                Log.Add(string.Empty);
            }               
        }  
        
        /// <summary>
        /// New log content will be stored into a new log space
        /// </summary>
        public void BreakLog(){
            FullLog.Add(new List<string>());
            CurrentIndent = "";
            NewLine = true;       
        }

        /// <summary>
        /// The text will be printed in gray, and everything between the '~' symbol will be printed using a secondary color (or till the last ':' or '...' symbols).
        /// </summary>
        /// <param name="text">The text to display, use ~TEXT~ to print this "text" with a secondary color (the symbols ':' or '...' can also be used as terminators).</param>
        /// <param name="color">The secondary color to use.</param>
        /// <param name="newLine">If true, a breakline will be added at the end.</param>
        private void WriteColor(string text, ConsoleColor color, bool newLine){    
            if(NewLine && !string.IsNullOrEmpty(text)){                
                Console.Write(CurrentIndent);
                Log.Add(string.Empty);
                Log[Log.Count-1] += CurrentIndent;
            } 
            
            Console.ResetColor();
            if(!text.Contains("~")) Console.ForegroundColor = color;     
            else{
                do{
                    int i = text.IndexOf("~");
                    string output = text.Substring(0, i);
                    Console.Write(output);
                    Log[Log.Count-1] += output;

                    Console.ForegroundColor = color;     
                    text = text.Substring(i+1);
                    i = (text.Contains("~") ? text.IndexOf("~") : text.Contains(":") ? text.IndexOf(":") : text.IndexOf("..."));
                    if(i == -1) i = text.Length;

                    output = text.Substring(0, i);
                    Console.Write(output, color);     
                    Log[Log.Count-1] += output;               
                    Console.ResetColor();

                    text = text.Substring(i).TrimStart('~');                                    
                }
                while(text.Contains("~"));
            }                    

            NewLine = newLine;
            Log[Log.Count-1] += text;   

            if(!newLine) Console.Write(text);    
            else Console.WriteLine(text);                

            Console.ResetColor();   
        }        
    }
}