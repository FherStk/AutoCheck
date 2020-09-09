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
using System.Collections.Concurrent;

namespace AutoCheck.Core{
    /// <summary>
    /// This class is in charge of writing the output into the terminal.    
    /// </summary>
    public class Output{     
        public enum Mode {
            SILENT,
            FILE, 
            TERMINAL
        }
    
        private string Indentation {get; set;}
        private bool NewLine {get; set;}      
        private List<string> Log {get; set;}
        private ConcurrentStack<bool> Status {get; set;}    //just for parallel tests (this class has not been designed to be thread-safe)
        
        /// <summary>
        /// Returns if the current instance is disabled, so all output will be ignored.
        /// </summary>
        /// <value></value>
        public bool Disabled {
            get{
                //Status contains the enabled/disabled history (true=disabled)
                bool result;
                Status.TryPeek(out result);

                return result;
            }
        }        
        
        public Output(){            
            this.Indentation = "";
            this.NewLine = true;            
            this.Log = new List<string>();
            this.Log.Add(string.Empty);            
            this.Status = new ConcurrentStack<bool>();
            this.Status.Push(false); 
        }
        
        /// <summary>
        /// Changes the output mode.
        /// </summary>
        /// <param name="Mode">Requested output mode</param>
        public void SetMode(Mode mode){
            switch(mode){
                case Mode.TERMINAL:
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

                case Mode.FILE:
                    //TODO: the log file path must be loaded from the script file.
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Enables the current instance, so all output will be processed.
        /// WARNING: Enabled state will be added to the status stack, use UndoStatus() in order to revert.
        /// </summary>
        public void Enable(){
            this.Status.Push(false);
        }
        
        /// <summary>
        /// Disables the current instance, so no output will be processed.
        /// WARNING: Disabled state will be added to the status stack, use UndoStatus() in order to revert.
        /// </summary>
        public void Disable(){
            this.Status.Push(true);
        }
        
        /// <summary>
        /// Reverts the Enabled/Disabled status (enabled is the default).
        /// </summary>
        public void UndoStatus(){
            //Allows restoring the previous status, even if it was the same as the current one.
            bool item;
            this.Status.TryPop(out item);
        }
        
        /// <summary>
        /// Returns the Output history as an string, using \r\n as breaklines.
        /// </summary>
        /// <returns></returns>
        public new string ToString(){
            string output = string.Empty;
            foreach(string line in this.Log)
                output = $"{output}{line}\r\n";

            return output.Trim();
        }
        
        /// <summary>
        /// Returns the Output history as an string, using HTML notation.
        /// </summary>
        /// <returns></returns>
        public string ToHTML(){
            string output = string.Empty;
            foreach(string line in this.Log)
                output = $"{output}{line}<br/>";

            return $"<p>{output.Trim()}</p>";
        }          

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
                string prefix = $"\n{Indentation}-";
                UnIndent();

                WriteLine($"{captionError}:{prefix}{string.Join(prefix, errors)}");
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
            Indentation = $"{Indentation}   ";
        }
        
        /// <summary>
        /// Removes an indentation (3 whitespaces) from the output.
        /// </summary>    
        public void UnIndent(){
            if(Indentation.Length > 0) Indentation = Indentation.Substring(0, Indentation.Length-3);
        }
        
        /// <summary>
        /// Resets the output indentation.
        /// </summary>    
        public void ResetIndent(){
           Indentation = "";
           NewLine = true;
        }
        
        /// <summary>
        /// The text will be printed in gray, and everything between the '~' symbol will be printed using a secondary color (or till the last ':' or '...' symbols).
        /// </summary>
        /// <param name="text">The text to display, use ~TEXT~ to print this "text" with a secondary color (the symbols ':' or '...' can also be used as terminators).</param>
        /// <param name="color">The secondary color to use.</param>
        /// <param name="newLine">If true, a breakline will be added at the end.</param>
        private void WriteColor(string text, ConsoleColor color, bool newLine){    
            if(this.Disabled) return;

            if(NewLine && !string.IsNullOrEmpty(text)){                
                Console.Write(Indentation);
                this.Log[this.Log.Count-1] += Indentation;
            } 
            
            Console.ResetColor();
            if(!text.Contains("~")) Console.ForegroundColor = color;     
            else{
                do{
                    int i = text.IndexOf("~");
                    string output = text.Substring(0, i);
                    Console.Write(output);
                    this.Log[this.Log.Count-1] += output;

                    Console.ForegroundColor = color;     
                    text = text.Substring(i+1);
                    i = (text.Contains("~") ? text.IndexOf("~") : text.Contains(":") ? text.IndexOf(":") : text.IndexOf("..."));
                    if(i == -1) i = text.Length;

                    output = text.Substring(0, i);
                    Console.Write(output, color);     
                    this.Log[this.Log.Count-1] += output;               
                    Console.ResetColor();

                    text = text.Substring(i).TrimStart('~');                                    
                }
                while(text.Contains("~"));
            }                    

            this.NewLine = newLine;
            this.Log[this.Log.Count-1] += text;   

            if(!newLine) Console.Write(text);    
            else{
                Console.WriteLine(text);
                this.Log.Add(string.Empty);
            } 

            Console.ResetColor();   
        }   
        
        /// <summary>
        /// Writes a set of breakline into the output.
        /// </summary>
        /// <param name="lines">The amount of breaklines.</param>
        public void BreakLine(int lines = 1){
            if(this.Disabled) return;
            
            for(int i=0; i < lines; i++){
                Console.WriteLine();
                this.Log.Add(string.Empty);
            }               
        }       
    }
}