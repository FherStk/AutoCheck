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
    public abstract class ScriptDB<T>: Script<T> where T: Core.CopyDetector, new(){
        /// <summary>
        /// The host address where the database server is listening.
        /// </summary>
        /// <value></value>
        protected string Host {get; set;}  
        
        /// <summary>
        /// The database name which the script will connect with.
        /// </summary>
        /// <value></value>                        
        protected string DataBase {get; set;}        
        
        /// <summary>
        /// The database username which the script will connect with.
        /// </summary>
        /// <value></value>
        protected string Username {get; set;}
        
        /// <summary>
        /// The database password which the script will connect with.
        /// </summary>
        /// <value></value>
        protected string Password {get; set;}      
        
        /// <summary>
        /// The current student's name.
        /// </summary>
        /// <value></value>
        protected string Student {get; private set;}
        
        private string DBPrefix {get; set;}        
        
        /// <summary>
        /// Creates a new script instance.
        /// </summary>
        /// <param name="args">Argument list, loaded from the command line, on which one will be stored into its equivalent local property.</param>
        /// <returns></returns>
        public ScriptDB(string[] args): base(args){        
            this.DBPrefix = this.GetType().Name.Split("_").Last().ToLower();
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
            this.CpThresh = 0.75f;
            this.Username = "postgres";
            this.Password = "postgres";
        }
               
        /// This method can be used in order to perform any action before running a script for a single student.
        /// Cleans any previous student execution's data, and re-creates a database if needed.
        /// <remarks>It will be automatically invoked when needed, so forced calls should be avoided.</remarks>
        /// </summary>
        protected override void SetUp(){
            base.SetUp();

            this.DataBase = Utils.FolderNameToDataBase(this.Path, this.DBPrefix);
            using(var db = new Connectors.Postgres(this.Host, this.DataBase, this.Username, this.Password)){        
                Output.Instance.WriteLine(string.Format("Checking the ~{0}~ database for the student ~{1}: ", this.DataBase, db.Student), ConsoleColor.DarkYellow); 
                Output.Instance.Indent();
                
                if(db.ExistsDataBase()){                
                    try{
                        Output.Instance.Write("Dropping the existing database: "); 
                        db.DropDataBase();
                        Output.Instance.WriteResponse();
                    }
                    catch(Exception ex){
                        Output.Instance.WriteResponse(ex.Message);
                    } 
                } 
                            
                try{
                    Output.Instance.Write("Creating the database: "); 
                    db.CreateDataBase(Directory.GetFiles(this.Path, "*.sql", SearchOption.AllDirectories).FirstOrDefault());
                    Output.Instance.WriteResponse();
                }
                catch(Exception ex){
                    Output.Instance.WriteResponse(ex.Message);
                }                 

                Output.Instance.UnIndent(); 
                Output.Instance.BreakLine();      
            }       
        }  
        
        /// <summary>
        /// This method contains the main script to run for a single student.
        /// </summary>                             
        public override void Run(){   
            this.Student = Core.Utils.DataBaseNameToStudentName(this.DataBase); //this.DataBase will be loaded by argument (single) or by batch (folder name).
            Output.Instance.WriteLine(string.Format("Running ~{0}~ for the student ~{1}: ", this.GetType().Name, this.Student), ConsoleColor.DarkYellow);
        }                               
    }
}