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
using System.Collections.Concurrent;
using NUnit.Framework;
using AutoCheck.Core;
using OS = AutoCheck.Core.Utils.OS;

namespace AutoCheck.Test
{
    public abstract class Test
    { 
        private string _PATH {get; set;}
        protected const string _FAKE = "fake"; 

        private ConcurrentDictionary<string, string> FolderPool = new ConcurrentDictionary<string, string>();

        protected string SamplesRootFolder {
            get {
                return Utils.PathToCurrentOS(Path.Combine(Utils.AppFolder, "samples")); 
            }
        } 
        protected string TempRootFolder {
            get {
                return Utils.PathToCurrentOS(Path.Combine(Utils.AppFolder, "temp")); 
            }
        } 
        protected string LogRootFolder {
            get {
                return Utils.PathToCurrentOS(Path.Combine(Utils.AppFolder, "logs")); 
            }
        } 

        protected string SamplesScriptFolder  {
            get {
                return Utils.PathToCurrentOS(Path.Combine(SamplesRootFolder, _PATH)); 
            }
        } 

        protected string TempScriptFolder {
            get {
                return Utils.PathToCurrentOS(Path.Combine(TempRootFolder, _PATH, FolderPool[TestContext.CurrentContext.Test.ID])); 
            }
        } 
        protected string LogScriptFolder{
            get {
                return Utils.PathToCurrentOS(Path.Combine(LogRootFolder, _PATH, FolderPool[TestContext.CurrentContext.Test.ID])); 
            }
        }         
        
        protected string Name { 
            get {
                return GetType().Name.ToCamelCase();
            }
        } 

        public Test(){
            _PATH = Name;
        }

        public Test(string folderScaffold){            
            _PATH = Path.Combine(folderScaffold, Name);
        }                   

        [OneTimeSetUp]
        public virtual void OneTimeSetUp() 
        {        
            //Hide the output    
            AutoCheck.Core.Output.SetMode(AutoCheck.Core.Output.Mode.SILENT);
           
            //Mandatory folders
            if(!Directory.Exists(SamplesRootFolder)) Directory.CreateDirectory(SamplesRootFolder);
            if(!Directory.Exists(TempRootFolder)) Directory.CreateDirectory(TempRootFolder);
            if(!Directory.Exists(LogRootFolder)) Directory.CreateDirectory(LogRootFolder);

            //Fresh start needed!
            CleanUp();            
        }

        [SetUp]
        public virtual void SetUp() 
        {
            //Each test instance has its own folder for logs and temp in order to avoid collisions and ensure cleaning when done.
            var added = false;
            do added = FolderPool.TryAdd(TestContext.CurrentContext.Test.ID, Guid.NewGuid().ToString());
            while(!added);                
        }

        [OneTimeTearDown]
        public virtual void OneTimeTearDown(){     
            //Clean before exit :)          
            CleanUp();                                

            //Temp and logs
            if(Directory.Exists(LogRootFolder)) Directory.Delete(LogRootFolder, true);
            if(Directory.Exists(TempRootFolder)) Directory.Delete(TempRootFolder, true);

            //Restore output     
            AutoCheck.Core.Output.SetMode(AutoCheck.Core.Output.Mode.VERBOSE);
        } 

        /// <summary>
        /// This method will be automatically called by the engine in order to cleanup a test enviroment before any test starts and after all tests ends.
        /// </summary>
        protected virtual void CleanUp(){            
        }        

        /// <summary>
        /// Retrieves a sample file path for the current test.
        /// </summary>
        /// <param name="file">The file to retrieve.</param>
        /// <returns>A file path.</returns>
        protected string GetSampleFile(string file) 
        {            
            return GetSampleFile(SamplesScriptFolder, file); 
        }

        /// <summary>
        /// Retrieves a sample file path for the requested script.
        /// </summary>
        /// <param name="script">The script folder used to search into.</param>
        /// <param name="file">The file to retrieve.</param>
        /// <returns>A file path.</returns>
        protected string GetSampleFile(string script, string file) 
        {
            return Utils.PathToCurrentOS(Path.Combine(SamplesRootFolder, script, file)); 
        }

        /// <summary>
        /// Converts an absolute local Win path to a remote one under WSL (Windows Subsystem Linux).
        /// </summary>
        /// <example>"C:\folder\file.ext" -> "/mnt/c/folder/file.ext"</example>
        /// <param name="localPath">Absolute local path to convert.</param>        
        /// <returns>The absolute remote path.</returns>
        protected string LocalPathToWsl(string localPath){            
            var drive = localPath.Substring(0, localPath.IndexOf(":")).ToLower();            
            if(Core.Utils.CurrentOS == OS.WIN) localPath = localPath.Replace($"{drive}:\\", $"/mnt/{drive}/", StringComparison.InvariantCultureIgnoreCase).Replace("\\", "/");    
            return localPath;
        }
    }
}
