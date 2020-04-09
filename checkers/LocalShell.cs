
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
using System.Collections.Generic;
using AutoCheck.Core;

namespace AutoCheck.Checkers{     
    /// <summary>
    /// Allows data validations over a local shell (running local commands).
    /// </summary>  
    public class LocalShell : Checker{  
        /// <summary>
        /// The main connector, can be used to perform direct operations over the data source.
        /// </summary>
        /// <value></value>    
        public Connectors.LocalShell Connector {get; private set;}        
        /// <summary>
        /// Creates a new checker instance.
        /// </summary>
        public LocalShell(){
            this.Connector = new Connectors.LocalShell();            
        }  
        /// <summary>
        /// Disposes the object releasing its unmanaged properties.
        /// </summary>
        public override void Dispose(){
            this.Connector.Dispose();
        }  
        /// <summary>
        /// Checks if a folder exists within the given path.
        /// </summary>
        /// <param name="path">Path where the folder will be searched into.</param>
        /// <param name="folder">The folder to search.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfFolderExists(string path, string folder, bool recursive = true){  
            List<string> errors = new List<string>();

            try{
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Looking for the folder ~{0}... ", System.IO.Path.Combine(path, folder)), ConsoleColor.Yellow);
                if(string.IsNullOrEmpty(this.Connector.GetFolder(path, folder, recursive))) errors.Add("Unable to find the folder.");
            }
            catch(Exception e){
                errors.Add(e.Message);
            }            

            return errors;
        }             
        /// <summary>
        /// Checks if a file exists within the given path.
        /// </summary>
        /// <param name="path">Path where the file will be searched into.</param>
        /// <param name="file">The file to search.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfFileExists(string path, string file, bool recursive = true){  
            List<string> errors = new List<string>();

            try{
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Looking for the file ~{0}... ", System.IO.Path.Combine(path, file)), ConsoleColor.Yellow);
                if(string.IsNullOrEmpty(this.Connector.GetFile(path, file, recursive))) errors.Add("Unable to find the file.");
            }
            catch(Exception e){
                errors.Add(e.Message);
            }            

            return errors;
        }    
        /// <summary>
        /// Checks the amount of expected folders.
        /// </summary>
        /// <param name="path">Path where the folder will be searched into.</param>
        /// <param name="expected">The expected amount.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <param name="op">The comparation operator to use when matching the result.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfFoldersMatchesAmount(string path, int expected, bool recursive = true, Connector.Operator op = Core.Connector.Operator.EQUALS){  
            List<string> errors = new List<string>();

            try{
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Looking for the amount of folders within ~{0}... ", path), ConsoleColor.Yellow);
                int count = this.Connector.CountFolders(path, recursive);
                errors.AddRange(CompareItems("Amount of folders missmatch:", expected, count, op));               
            }
            catch(Exception e){
                errors.Add(e.Message);
            }            

            return errors;
        } 
        /// <summary>
        /// Checks the amount of expected files.
        /// </summary>
        /// <param name="path">Path where the files will be searched into.</param>
        /// <param name="expected">The expected amount.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <param name="op">The comparation operator to use when matching the result.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfFilesMatchesAmount(string path, int expected, bool recursive = true, Connector.Operator op = Core.Connector.Operator.EQUALS){  
            List<string> errors = new List<string>();

            try{
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Looking for the amount of files within ~{0}... ", path), ConsoleColor.Yellow);
                int count = this.Connector.CountFiles(path, recursive);
                errors.AddRange(CompareItems("Amount of files missmatch:", expected, count, op));               
            }
            catch(Exception e){
                errors.Add(e.Message);
            }            

            return errors;
        } 
         /// <summary>
        /// Checks if the executed command produces the expected result.
        /// </summary>
        /// <param name="command">The command to run.</param>
        /// <param name="expected">The expected result.</param>
        /// <param name="op">The comparation operator to use when matching the result.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfCommandMatchesResult(string command, int expected, Connector.Operator op = Core.Connector.Operator.EQUALS){  
            return CheckIfCommandMatchesResult(command, null, expected, op);
        }
        /// <summary>
        /// Checks if the executed command produces the expected result.
        /// </summary>
        /// <param name="command">The command to run.</param>
        /// <param name="path">The path where the command executable is located.</param>
        /// <param name="expected">The expected result.</param>
        /// <param name="op">The comparation operator to use when matching the result.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfCommandMatchesResult(string command, string path, int expected, Connector.Operator op = Core.Connector.Operator.EQUALS){  
            List<string> errors = new List<string>();

            try{
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Running the command ~{0}... ", command), ConsoleColor.Yellow);
                var r = this.Connector.RunCommand(command, path);
                errors.AddRange(CompareItems("Amount of files missmatch:", expected, r.response, op));               
            }
            catch(Exception e){
                errors.Add(e.Message);
            }            

            return errors;
        }

        
        private List<string> CompareItems(string caption, object expected, object current, Connector.Operator op){
            //TODO: must be reusable by other checkers (this is the good one!!!)
            List<string> errors = new List<string>();
            string info = string.Format("expected->'{0}' found->'{1}'.", expected, current);

            double expectedNum = 0, currentNum = 0;
            if(op != AutoCheck.Core.Connector.Operator.EQUALS){
                if (!double.TryParse(expected.ToString(), out expectedNum)) throw new InvalidOperationException("Expected value is not a number.");             
                if (!double.TryParse(current.ToString(), out currentNum)) throw new InvalidOperationException("Current value is not a number.");             
            }

            switch(op){
                case AutoCheck.Core.Connector.Operator.EQUALS:
                    if(current != expected) errors.Add(string.Format("{0} {1}.", caption, info));
                    break;

                case AutoCheck.Core.Connector.Operator.GREATER:       
                    if(currentNum > expectedNum) errors.Add(string.Format("{0} maximum {1}.", caption, info));
                    break;

                case AutoCheck.Core.Connector.Operator.LOWER:
                    if(currentNum < expectedNum) errors.Add(string.Format("{0} minimum {1}.", caption, info));
                    break;
            }

            return errors;
        }       
    }    
}