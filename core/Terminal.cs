using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator{
    class Terminal{
        /// <summary>
        /// The caption will be printed in gray, and everything after the '~' symbol will be printed using a secondary color till the last ':' symbol.
        /// </summary>
        /// <param name="caption">The text to display, use ~TEXT: to print this "text" with a secondary color.</param>
        /// <param name="color">The secondary color to use.</param>
        public static void WriteCaption(string caption, ConsoleColor color = ConsoleColor.Gray){                
            if(caption.Contains("~")){
                int i = caption.IndexOf("~");
                Write(caption.Substring(0, i));

                caption = caption.Substring(i+1);
                i = caption.IndexOf(":");
                Write(caption.Substring(0, i), color);

                caption = caption.Substring(i+1);                    
            }
        
            Write(caption);                
        }                                                                
        public static void Write(string text, ConsoleColor color = ConsoleColor.Gray){
            WriteColor(false, text, color);
        }  
        public static void WriteLine(string text, ConsoleColor color = ConsoleColor.Gray){
            WriteColor(true, text, color);
        }             
        public static void WriteOK(){
            WriteLine("OK", ConsoleColor.DarkGreen);
        }
        public static void WriteError(List<string> errors = null, string prefix = "\n\t-"){
            if(errors == null || errors.Where(x => x.Length > 0).Count() == 0) WriteLine("ERROR", ConsoleColor.Red);
            else WriteLine(string.Format("ERROR: {0}{1}", prefix, string.Join(prefix, errors)), ConsoleColor.Red);
        }
        public static void WriteError(string error, string prefix = "\n\t-"){
            WriteError(new List<string>(){error});
        }            
        public static void BreakLine(int lines = 1){
            for(int i=0; i < lines; i++)
                WriteLine("");
        }                   
        private static void WriteColor(bool newLine, string text, ConsoleColor color){
            Console.ResetColor();   
            Console.ForegroundColor = color;     
            if(newLine) Console.WriteLine(text);
            else Console.Write(text);
            Console.ResetColor();   
        }  
    }
}