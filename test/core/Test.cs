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
using NUnit.Framework;
using AutoCheck.Core;

namespace AutoCheck.Test.Core
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
            Output.Instance.SetMode(Output.Mode.SILENT);

            //Compute samples paths
            this.SamplesRootFolder = Path.Combine(Utils.AppFolder, "samples"); 
            this.SamplesScriptFolder = GetSamplePath(this.GetType().Name.ToLower()); 
            
            //Fresh start needed!
            CleanUp();            
        }

        [OneTimeTearDown]
        public virtual void OneTimeTearDown(){     
            //Clean before exit :)          
            CleanUp();                                

            //Restore output     
            Output.Instance.SetMode(Output.Mode.TERMINAL);
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
            return Path.Combine(this.SamplesRootFolder, script); 
        }

        /// <summary>
        /// Retrieves a sample file path for the current test.
        /// </summary>
        /// <param name="file">The file to retrieve.</param>
        /// <returns>A file path.</returns>
        protected string GetSampleFile(string file) 
        {
            if(string.IsNullOrEmpty(this.SamplesScriptFolder)) throw new ArgumentNullException("The global samples path value is empty, use another overload or set up the SamplesPath parameter.");
            return Path.Combine(this.SamplesScriptFolder, file); 
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
    }
}
