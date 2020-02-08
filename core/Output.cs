using System;
using System.Linq;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator{
    public class Output{
        //This will be instantiated by any Script class... no singleton needed at the moment.
        private string Indentation {get; set;}
        private bool NewLine {get; set;}
        private List<string> Log {get; set;}

        public Output(){
            this.Indentation = "";
            this.NewLine = true;
            this.Log = new List<string>();
            this.Log.Add(string.Empty);
        }
        public new string ToString(){
            string output = string.Empty;
            foreach(string line in this.Log)
                output = string.Format("{0}{1}\r\n", output, line);

            return output;
        }
        public string ToHTML(){
            string output = string.Empty;
            foreach(string line in this.Log)
                output = string.Format("{0}{1}<br/>", output, line);

            return string.Format("<p>{0}</p>", output);
        }
        public void Write(string text, ConsoleColor color = ConsoleColor.Gray){
            WriteColor(text, color, false);
        }  
        public void WriteLine(string text, ConsoleColor color = ConsoleColor.Gray){
            WriteColor(text, color, true);
        }             
        public void WriteResponse(List<string> errors = null){                        
            if(errors == null || errors.Count == 0) WriteLine("OK", ConsoleColor.DarkGreen);
            else if(errors.Where(x => x.Length > 0).Count() == 0) WriteLine("ERROR", ConsoleColor.Red);
            else{
                Indent();
                string prefix = string.Format("\n{0}-", Indentation);
                UnIndent();

                WriteLine(string.Format("ERROR: {0}{1}", prefix, string.Join(prefix, errors)), ConsoleColor.Red);
            } 
        }        
        public void WriteResponse(string error){
            WriteResponse(new List<string>(){error});
        }            
        public void BreakLine(int lines = 1){
            for(int i=0; i < lines; i++){
                Console.WriteLine();
                this.Log.Add(string.Empty);
            }               
        }                   
        public void Indent(){
            Indentation = string.Format("{0}{1}", Indentation, "   ");
        }
        public void UnIndent(){
            if(Indentation.Length > 0) Indentation = Indentation.Substring(0, Indentation.Length-3);
        }
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
    }
}