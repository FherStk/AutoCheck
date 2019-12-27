using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator{
    class Terminal{    
        private static string _indentation = "";
        private static bool _newLine = true;
        public static void Write(string text, ConsoleColor color = ConsoleColor.Gray){
            WriteColor(text, color, false);
        }  
        public static void WriteLine(string text, ConsoleColor color = ConsoleColor.Gray){
            WriteColor(text, color, true);
        }             
        public static void WriteResponse(List<string> errors = null){                        
            if(errors == null || errors.Count == 0) WriteLine("OK", ConsoleColor.DarkGreen);
            else if(errors.Where(x => x.Length > 0).Count() == 0) WriteLine("ERROR", ConsoleColor.Red);
            else{
                Indent();
                string prefix = string.Format("\n{0}-", _indentation);
                UnIndent();

                WriteLine(string.Format("ERROR: {0}{1}", prefix, string.Join(prefix, errors)), ConsoleColor.Red);
            } 
        }        
        public static void WriteResponse(string error){
            WriteResponse(new List<string>(){error});
        }            
        public static void BreakLine(int lines = 1){
            for(int i=0; i < lines; i++)
               Console.WriteLine();
        }                   
        public static void Indent(){
            _indentation = string.Format("{0}{1}", _indentation, "   ");
        }
        public static void UnIndent(){
            if(_indentation.Length > 0) _indentation = _indentation.Substring(0, _indentation.Length-3);
        }
        public static void ResetIndent(){
           _indentation = "";
           _newLine = true;
        }
        /// <summary>
        /// The text will be printed in gray, and everything after the '~' symbol will be printed using a secondary color till the last ':' symbol.
        /// </summary>
        /// <param name="text">The text to display, use ~TEXT: to print this "text" with a secondary color.</param>
        /// <param name="color">The secondary color to use.</param>
        /// <param name="newLine">If true, a breakline will be added at the end.</param>
        private static void WriteColor(string text, ConsoleColor color, bool newLine){            
            if(_newLine && !string.IsNullOrEmpty(text)) Console.Write(_indentation);            
            
            Console.ResetColor();
            if(!text.Contains("~")) Console.ForegroundColor = color;     
            else{
                int i = text.IndexOf("~");
                Console.Write(text.Substring(0, i));

                Console.ForegroundColor = color;     
                text = text.Substring(i+1);
                i = text.IndexOf(":");
                Console.Write(text.Substring(0, i), color);

                Console.ResetColor();
                text = text.Substring(i);                    
            }                    

            _newLine = newLine;
            if(newLine) Console.WriteLine(text);
            else Console.Write(text);

            Console.ResetColor();   
        }  
    }
}