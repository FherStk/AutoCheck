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
    /// This class must be inherited in order to develop a database oriented custom script.
    /// The script is the main container for a set of instructions, which will test the correctness of an assignement.
    /// </summary>      
    /// <typeparam name="T">The copy detector that will be automatically used within the script.</typeparam>
    public abstract class ScriptGDrive<T>: ScriptFiles<T> where T: Core.CopyDetector, new(){
        /// <summary>
        /// The json file (client_secret.json) containing the credentials needed to communicate with the Google Drive's API.
        /// </summary>
        /// <value></value>
        protected string Secret {get; set;}  
                
        /// <summary>
        /// The Google Drive's API account
        /// </summary>
        /// <value></value>
        protected string Username {get; set;}                       

        protected string GDriveFolder {get; private set;}
               
        /// <summary>
        /// Creates a new script instance.
        /// </summary>
        /// <param name="args">Argument list, loaded from the command line, on which one will be stored into its equivalent local property.</param>
        /// <returns></returns>
        public ScriptGDrive(Dictionary<string, string> args): base(args){                                
        } 
        
        /// <summary>
        /// Sets up the default arguments values, can be overwrited if custom arguments are needed.
        /// </summary>      
        protected override void DefaultArguments(){  
            //Note: this cannot be on constructor, because the arguments introduced on command line should prevail:
            //  1. Default base class values
            //  2. Inherited class values
            //  3. Command line argument values
            
            base.DefaultArguments();
            this.GDriveFolder = System.IO.Path.Combine("\\AutoCheck", "scripts", this.GetType().Name.Split("_").Last().ToLower());
        } 

        /// This method can be used in order to perform any action before running a script for a single student.
        /// Cleans any previous student execution's data, and uploads the student's Google Drive file to the teacher's one.
        /// <remarks>It will be automatically invoked when needed, so forced calls should be avoided.</remarks>
        /// </summary>
        protected override void SetUp(){          
            base.SetUp();

            //TODO: New behaviour (sorry, no time for implementation)
            //          1. Create a new GDrive folder for the current student (within the current one)
            //          2. Copy there all the files shared (sometimes, the student shared an entire folder)
            //          3. Repeat for all the GDrive links found within the txt file.

            this.Student = Core.Utils.FolderNameToStudentName(this.Path); 
            using(var drive = new Connectors.GDrive(this.Secret, this.Username)){
                Output.Instance.WriteLine(string.Format("Checking the hosted Google Drive file for the student ~{0}: ", this.Student), ConsoleColor.DarkYellow); 
                Output.Instance.Indent();
                
                var p = System.IO.Path.GetDirectoryName(this.GDriveFolder);
                var f = System.IO.Path.GetFileName(this.GDriveFolder);
                if(drive.GetFolder(p, f) == null){                
                    try{
                        Output.Instance.Write(string.Format("Creating folder structure in '{0}': ", this.GDriveFolder)); 
                        drive.CreateFolder(p, f);
                        Output.Instance.WriteResponse();
                    }
                    catch(Exception ex){
                        Output.Instance.WriteResponse(ex.Message);
                    } 
                } 

                var uri = string.Empty;
                try{
                    Output.Instance.Write("Retreiving remote file URI from student's assignment: "); 
                    var file = Directory.GetFiles(this.Path, "*.txt", SearchOption.AllDirectories).FirstOrDefault();    
                    uri = File.ReadAllLines(file).Where(x => x.Length > 0 && x.StartsWith("http")).FirstOrDefault(); 

                    if(string.IsNullOrEmpty(uri)) Output.Instance.WriteResponse("Unable to read any URI from the current file.");              
                    else Output.Instance.WriteResponse();
                }
                catch(Exception ex){
                    Output.Instance.WriteResponse(ex.Message);
                }

                if(!string.IsNullOrEmpty(uri)){            
                    try{
                        Output.Instance.Write("Copying student's remote file to Google Drive's storage: ");                         
                        drive.CopyFile(new Uri(uri), this.GDriveFolder, this.Student);                    
                        Output.Instance.WriteResponse();
                    }
                    catch(Exception ex){
                        Output.Instance.WriteResponse(ex.Message);
                    }    
                }             

                Output.Instance.UnIndent(); 
                Output.Instance.BreakLine();    
            }   
        }               
    }
}