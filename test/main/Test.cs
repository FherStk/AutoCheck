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
using NUnit.Framework;
using AutoCheck.Core;
using OS = AutoCheck.Core.Utils.OS;

namespace AutoCheck.Test
{
    public abstract class Test
    {
        protected string SamplesRootFolder {get; set;}
        protected string SamplesScriptFolder {get; set;}
        protected const string _FAKE = "fake";

        [OneTimeSetUp]
        public virtual void OneTimeSetUp() 
        {        
            //Hide the output    
            Output.SetMode(Output.Mode.SILENT);

            //Compute samples paths
            SamplesRootFolder = Path.Combine(Utils.AppFolder, "samples"); 
            SamplesScriptFolder = GetSamplePath(GetType().Name.ToCamelCase()); 
            
            //Fresh start needed!
            CleanUp();            
        }

        [OneTimeTearDown]
        public virtual void OneTimeTearDown(){     
            //Clean before exit :)          
            CleanUp();                                

            //Restore output     
            Output.SetMode(Output.Mode.VERBOSE);
        }         

        /// <summary>
        /// This method will be automatically called by the engine in order to cleanup a test enviroment on start and on ends.
        /// </summary>
        protected virtual void CleanUp(){
        }                   

        /// <summary>
        /// Retrieves the samples path for the requested script.
        /// </summary>
        /// <param name="script">Script name.</param>
        /// <returns>A folder path.</returns>
        protected string GetSamplePath(string script) 
        {
            return Path.Combine(SamplesRootFolder, script); 
        }

        /// <summary>
        /// Retrieves a sample file path for the current test.
        /// </summary>
        /// <param name="file">The file to retrieve.</param>
        /// <returns>A file path.</returns>
        protected string GetSampleFile(string file) 
        {
            if(string.IsNullOrEmpty(SamplesScriptFolder)) throw new ArgumentNullException("The global samples path value is empty, use another overload or set up the SamplesPath parameter.");
            return Path.Combine(SamplesScriptFolder, file); 
        }

        /// <summary>
        /// Retrieves a sample file path for the requested script.
        /// </summary>
        /// <param name="script">The script folder used to search into.</param>
        /// <param name="file">The file to retrieve.</param>
        /// <returns>A file path.</returns>
        protected string GetSampleFile(string script, string file) 
        {
            return Path.Combine(GetSamplePath(script), file); 
        }

        /// <summary>
        /// Converts an absolute local Win path to a remote one under WSL (Windows Subsystem Linux).
        /// </summary>
        /// <example>"C:\folder\file.ext" -> "/mnt/c/folder/file.ext"</example>
        /// <param name="localPath">Absolute local path to convert.</param>        
        /// <returns>The absolute remote path.</returns>
        protected string LocalPathToWsl(string localPath){            
            var drive = localPath.Substring(0, localPath.IndexOf(":"));            
            if(Core.Utils.CurrentOS == OS.WIN) localPath = localPath.Replace($"{drive}:\\", $"/mnt/{drive}/", StringComparison.InvariantCultureIgnoreCase).Replace("\\", "/");    
            return localPath;
        }
    }
}
