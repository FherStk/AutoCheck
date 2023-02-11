/*
    Copyright © 2023 Fernando Porrino Serrano
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

using System.IO;
using NUnit.Framework;
using AutoCheck.Core.Exceptions;

namespace AutoCheck.Test
{    
    [Parallelizable(ParallelScope.All)]    
    public class Inherits : Test
    {
        //WARNING:  Parametrization not allowed, because the temp folder would be shared. 
        // 
        
        public Inherits(): base("script"){
        }              
       
        [Test, Category("Inherits"), Category("Local")]
        public void Script_INHERITS_VARS_REPLACE()
        {        
            try{  
                var s = new AutoCheck.Core.Script(GetSampleFile("inherits_vars_ok1.yaml"));
            }   
            catch(ResultMismatchException ex){
                Assert.AreEqual("Expected -> Fer; Found -> New Fer", ex.Message);
            }                                       
        }

        //TODO: test to override other level-1 nodes (only 'vars' has been tested)

        [Test, Category("Inherits"), Category("Local")]
        public void Script_INHERITS_RUN_FOLDER()
        {       
            var dest =  Path.Combine(Path.GetDirectoryName(TempScriptFolder), "test2"); //the script will use this folder, so no random path can be used
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);                                 

            File.Copy(GetSampleFile("compressed", "nopass.zip"), GetSampleFile(dest, "nopass.zip"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.zip")));
            var s = new AutoCheck.Core.Script(GetSampleFile("inherits_run_ok1.yaml"));            
            
            Assert.AreEqual($"Running script inherits_run_ok1 (v1.0.0.1):\r\nRunning script inherits_run_ok1 (v1.0.0.1) in single mode for {dest}:", s.Output.ToString());
            Directory.Delete(dest, true);
        }
    }
}