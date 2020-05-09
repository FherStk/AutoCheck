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
        public ScriptGDrive(string[] args): base(args){                                
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
            this.GDriveFolder = System.IO.Path.Combine("AutoCheck", "scripts", this.GetType().Name.Split("_").Last().ToLower());
        } 

        /// This method can be used in order to perform any action before running a script for a single student.
        /// Cleans any previous student execution's data, and re-creates a database if needed.
        /// <remarks>It will be automatically invoked when needed, so forced calls should be avoided.</remarks>
        /// </summary>
        protected override void SetUp(){           
            this.Student = Core.Utils.FolderNameToStudentName(this.Path); 
            using(var drive = new Connectors.GDrive(this.Secret, this.Username)){
                Output.Instance.WriteLine(string.Format("Checking the hosted Google Drive file for the student ~{0}: ", this.Student), ConsoleColor.DarkYellow); 
                Output.Instance.Indent();
               
                try{
                    Output.Instance.Write("Cleaning data from previous executions: ");                         
                    base.SetUp();
                    Output.Instance.WriteResponse();
                }
                catch(Exception ex){
                    Output.Instance.WriteResponse(ex.Message);
                } 
                
                if(!drive.ExistsFolder(this.GDriveFolder)){                
                    try{
                        Output.Instance.Write(string.Format("Creating folder structure in '{0}': ", this.GDriveFolder)); 
                        drive.CreateFolder(this.GDriveFolder);
                        Output.Instance.WriteResponse();
                    }
                    catch(Exception ex){
                        Output.Instance.WriteResponse(ex.Message);
                    } 
                } 
                            
                try{
                    Output.Instance.Write("Downloading the file to local storage: "); 
                    var file = Directory.GetFiles(this.Path, "*.txt", SearchOption.AllDirectories).FirstOrDefault();
                    var uri = File.ReadAllLines(file).Where(x => x.Length > 0 && x.StartsWith("http")).FirstOrDefault();
                    file = drive.Download(new Uri(uri), System.IO.Path.Combine(AutoCheck.Core.Utils.AppFolder(), "temp"));                    
                    Output.Instance.WriteResponse();

                    Output.Instance.Write("Uploading the file to Google Drive's storage: "); 
                    drive.CreateFile(file, string.Format("{0}.{1}", System.IO.Path.Combine(this.GDriveFolder, this.Student), System.IO.Path.GetExtension(file)));
                    Output.Instance.WriteResponse();

                    Output.Instance.Write("Removing the file from local storage: "); 
                    System.IO.File.Delete(file);
                    Output.Instance.WriteResponse();
                }
                catch(Exception ex){
                    Output.Instance.WriteResponse(ex.Message);
                }                 

                Output.Instance.UnIndent(); 
                Output.Instance.BreakLine();    
            }   
        }               
    }
}