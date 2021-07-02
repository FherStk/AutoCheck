/*
    Copyright © 2021 Fernando Porrino Serrano
    Third party software licenses can be found at /docs/credits/credits.md

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
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using AutoCheck.Core.Exceptions;
using ExCSS;

namespace AutoCheck.Core{
    /// <summary>
    /// This class is in charge of writing the output into the terminal.    
    /// </summary>
    /// <remarks>Should be a singletone but cannot be due testing...</remarks>
    public class Output{ 
        private List<Content> HeaderLog {get; set;}
        private List<Content> SetupLog {get; set;}
        private List<List<Content>> ScriptLog {get; set;}
        private List<Content> TeardownLog {get; set;}          
               
        //Contains the data stored as needed in order to generate diferent outputs (txt, json, etc)
        private class Content{            
            public string Indent {get; set;}
            public string Text {get; set;}
            public string Style {get; set;}
            public bool BreakLine {get; set;}
        }

        public enum Mode {
            SILENT,
            VERBOSE
        }
        
        public enum Style {
            INFO,
            PROMPT,
            HEADER,
            CRITICAL,
            DETAILS,
            QUESTION,
            SCORE,
            SUCCESS,
            ERROR,
            DEFAULT,
            ECHO,
            WARNING
        }

        public enum Type{
            HEADER,
            SETUP,
            SCRIPT,
            TEARDOWN
        }
        
        public const string SingleIndent = "   ";
        
        public string CurrentIndent {get; private set;}
        
        private bool IsNewLine {get; set;}              
        
        private Stylesheet CssDoc {get; set;} 
                
        private List<Content> CurrentLog {get; set;} 
        
        private int CurrentLogIndex {get; set;}                         //The last CurrentLog entry that has been send to the terminal output                

        public Output(){                                        
            HeaderLog = new List<Content>(); 
            SetupLog = new List<Content>(); 
            TeardownLog = new List<Content>(); 
            ScriptLog = new List<List<Content>>();
            CurrentLog = new List<Content>();

            ResetLog();        

            //Load the styles
            var cssPath = Utils.ConfigFile("output.css");                                            
            if(!File.Exists(cssPath)) throw new FileNotFoundException(cssPath);
            else{
                StylesheetParser parser = new StylesheetParser();    
                CssDoc = parser.Parse(File.ReadAllText(cssPath));
            } 
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
        /// Returns the Output history as an string array, where each string represents a separated plain log file using \r\n as breaklines.
        /// </summary>
        /// <returns></returns>
        public string[] ToText(){
            List<string> result = new List<string>();

            foreach(var log in GetFullLog()){
                string output = string.Empty;
                
                foreach(var content in log){                   
                    output = $"{output}{content.Indent}{content.Text}";
                    if(content.BreakLine) output = $"{output}\r\n";                         
                }

                output = output.TrimEnd("\r\n".ToCharArray()).TrimEnd(' ');
                if(!string.IsNullOrEmpty(output)) result.Add(output);
            }
        
            return result.ToArray();
        }

        /// <summary>
        /// Returns the Output history as a json array, where each string represents a separated json log file.
        /// </summary>
        /// <returns></returns>
        public string[] ToJson(){
            List<string> result = new List<string>();

            foreach(var log in GetFullLog(true)){
                result.Add(JsonSerializer.Serialize(log, new JsonSerializerOptions(){
                    ReferenceHandler = ReferenceHandler.Preserve
                }));
            }

            return result.ToArray();
        }

        /// <summary>
        /// Returns the Output history as an string, using \r\n as breaklines.
        /// </summary>
        /// <returns></returns>
        public new string ToString(){
            return string.Join("\r\n\r\n", ToText());
        }            
               
        /// <summary>
        /// Send new text to the output, no breakline will be added to the end.
        /// The text will be printed in gray, and everything between '~' symbols will be printed using a secondary color (or till the last ':' or '...' symbols).
        /// </summary>
        /// <param name="text">The text to display</param>
        /// <param name="style">Which stuyle will be applied in order to print using colors.</param>
        public void Write(string text, Style style = Style.DEFAULT){
            WriteColor(text, style, false);
        }  
        
        /// <summary>
        /// Send new text to the output, a breakline will be added to the end.
        /// The text will be printed in gray, and everything between '~' symbols will be printed using a secondary color (or till the last ':' or '...' symbols).
        /// </summary>
        /// <param name="text">The text to display</param>
        /// <param name="style">Which stuyle will be applied in order to print using colors.</param>
        public void WriteLine(string text, Style style = Style.DEFAULT){
            WriteColor(text, style, true);
        } 
        
        /// <summary>
        /// Given a list of strings containing the errors found during a script execution (usually the return of a checker's method), the output will be as follows:
        /// 'OK' in green if the errors list is empty; 'ERROR' in red, followed by all the errors descriptions (as a list), otherwise.
        /// </summary>
        /// <param name="errors">A list of errors, usually the return of a checker's method.</param>            
        public void WriteResponse(List<string> errors = null, string captionOk="OK", string captionError="ERROR"){
            if(errors == null || errors.Count == 0) WriteLine(captionOk, Style.SUCCESS);
            else if(errors.Where(x => x.Length > 0).Count() == 0) WriteLine(captionError, Style.ERROR);
            else{
                Indent();
                string prefix = $"\n{CurrentIndent}-";
                UnIndent();
                WriteLine($"{captionError}:{prefix}{string.Join(prefix, errors)}", Style.ERROR);
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
           IsNewLine = true;
        }
        
        /// <summary>
        /// Writes a set of breakline into the output.
        /// </summary>
        /// <param name="lines">The amount of breaklines.</param>
        public void BreakLine(int lines = 1){            
            for(int i=0; i < lines; i++){
                WriteColor("", Style.DEFAULT, true);
            }
        }
        
        /// <summary>
        /// The current log content will be stored using the provided type
        /// </summary>
        public void CloseLog(Type type){ 
            switch(type){
                case Type.HEADER:
                    HeaderLog = CurrentLog;
                    break;

                case Type.SETUP:
                    SetupLog = CurrentLog;
                    break;

                case Type.TEARDOWN:
                    TeardownLog = CurrentLog;
                    break;

                case Type.SCRIPT:
                    ScriptLog.Add(CurrentLog);                    
                    break;
            }

            ResetLog();
        }

        /// <summary>
        /// Closes the current log and setups a new one, usefull for batch mode script execution
        /// </summary>
        private void ResetLog(){ 
            CurrentIndent = "";
            CurrentLogIndex = 0;    
            IsNewLine = true;  
            CurrentLog = new List<Content>(); 
        }           

        private ConsoleColor CssToConsoleColor(StyleRule cssRule){
            var color = cssRule.Style.Color;
            color = color.Substring(color.IndexOf("(")+1);
            color = color.Substring(0, color.IndexOf(")"));

            var rgb = color.Split(",");
            return ClosestConsoleColor(byte.Parse(rgb[0].Trim()), byte.Parse(rgb[1].Trim()), byte.Parse(rgb[2].Trim()));
        }

        private ConsoleColor ClosestConsoleColor(byte r, byte g, byte b)
        {
            //Source: https://stackoverflow.com/a/12340136
            ConsoleColor ret = 0;
            double rr = r, gg = g, bb = b, delta = double.MaxValue;

            foreach (ConsoleColor cc in Enum.GetValues(typeof(ConsoleColor)))
            {
                var n = Enum.GetName(typeof(ConsoleColor), cc);
                var c = System.Drawing.Color.FromName(n == "DarkYellow" ? "Orange" : n); // bug fix
                var t = Math.Pow(c.R - rr, 2.0) + Math.Pow(c.G - gg, 2.0) + Math.Pow(c.B - bb, 2.0);
                if (t == 0.0)
                    return cc;
                if (t < delta)
                {
                    delta = t;
                    ret = cc;
                }
            }
            return ret;
        }
        
        /// <summary>
        /// The text will be printed in gray, and everything between the '~' symbol will be printed using a secondary color (or till the last ':' or '...' symbols).
        /// </summary>
        /// <param name="text">The text to display, use ~TEXT~ to print this "text" with a secondary color (the symbols ':' or '...' can also be used as terminators).</param>
        /// <param name="style">Which stuyle will be applied in order to print using colors.</param>
        /// <param name="newLine">If true, a breakline will be added at the end.</param>
        private void WriteColor(string text, Style style, bool newLine){             
            if(!text.Contains("~")){
                //Single color text
                CurrentLog.Add(new Content(){
                    Indent = (IsNewLine && !string.IsNullOrEmpty(text) ? CurrentIndent : ""),
                    Text = text,
                    Style = $"{style.ToString().ToLower()}-primary", //GetCssRule($"{style.ToString().ToLower()}-primary"),
                    BreakLine = newLine
                });
            }
            else{
                //Multi-color
                while(text.Contains("~")){
                    int i = text.IndexOf("~");

                    //First color
                    CurrentLog.Add(new Content(){
                        Indent = (IsNewLine ? CurrentIndent : ""),
                        Text = text.Substring(0, i),
                        Style = $"{style.ToString().ToLower()}-primary" //GetCssRule($"{style.ToString().ToLower()}-primary")
                    });            
                    
                    text = text.Substring(i+1);
                    i = (text.Contains("~") ? text.IndexOf("~") : text.Contains("...") ? text.IndexOf("...") : text.IndexOf(":"));
                    if(i == -1) i = text.Length;

                    //Second color
                    CurrentLog.Add(new Content(){
                        Text = text.Substring(0, i),
                        Style = $"{style.ToString().ToLower()}-secondary" //GetCssRule($"{style.ToString().ToLower()}-secondary")
                    });
                    
                    text = text.Substring(i).TrimStart('~');                               
                }

                //Last part of the text
                if(!string.IsNullOrEmpty(text)){
                     CurrentLog.Add(new Content(){
                        Text = text,
                        Style = $"{style.ToString().ToLower()}-primary" //GetCssRule($"{style.ToString().ToLower()}-primary")
                    });
                }

                //Breakline at the end (if needed)
                CurrentLog[CurrentLog.Count-1].BreakLine = newLine;                
            }

            IsNewLine = newLine;
            WriteIntoTerminal();
        }

        private void WriteIntoTerminal(){
            for(int i=CurrentLogIndex; i<CurrentLog.Count; i++){                
                Console.Write(CurrentLog[i].Indent);
                Console.ForegroundColor = CssToConsoleColor(GetCssRule(CurrentLog[i].Style));

                if(CurrentLog[i].BreakLine) Console.WriteLine(CurrentLog[i].Text);
                else Console.Write(CurrentLog[i].Text);
                Console.ResetColor();               
            }

            CurrentLogIndex = CurrentLog.Count;
        }

        private StyleRule GetCssRule(string style){
            try{
                var rule = CssDoc.StyleRules.Cast<StyleRule>().Where(x => x.SelectorText.Equals($".{style}")).SingleOrDefault();
                if(rule != null) return rule;
                else throw new StyleInvalidException($"Unable to apply the requested style '{style}'.");  
            }
            catch(Exception){
                if(style.Contains("-")) return GetCssRule(style.Substring(0, style.IndexOf("-")));
                else throw;
            } 
        }

        private List<Content> Trim(List<Content> content){
            var copy = content.ToArray().ToList();                
            
            while(copy.FirstOrDefault() != null && copy.FirstOrDefault().BreakLine && string.IsNullOrEmpty(copy.FirstOrDefault().Text.Trim()))
                copy.RemoveAt(0);

            while(copy.LastOrDefault() != null && copy.LastOrDefault().BreakLine && string.IsNullOrEmpty(copy.LastOrDefault().Text.Trim()))
                copy.RemoveAt(copy.Count-1);            

            if(copy.LastOrDefault() != null && !string.IsNullOrEmpty(copy.LastOrDefault().Text))
                copy.LastOrDefault().Text = copy.LastOrDefault().Text.TrimEnd();

            return copy;
        }

        private List<List<Content>> GetFullLog(bool trim = false) {
            var header = trim ? Trim(HeaderLog) : HeaderLog;
            var setup =  trim ? Trim(SetupLog) : SetupLog;
            var teardwon =  trim ? Trim(TeardownLog) : TeardownLog;
            List<List<Content>> log = new List<List<Content>>();

            foreach(var script in ScriptLog){                
                log.Add(header.Concat(setup).Concat((trim ? Trim(script) : script).Concat(teardwon)).ToList());
            }
            
            return log;
        } 
    }
}