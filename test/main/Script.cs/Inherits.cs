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

using System.IO;
using NUnit.Framework;
using AutoCheck.Core.Exceptions;

namespace AutoCheck.Test
{    
    [Parallelizable(ParallelScope.All)]    
    public class Inherits : Test
    {                
        [OneTimeSetUp]
        public virtual void StartUp() 
        {
            SamplesScriptFolder = GetSamplePath(Path.Combine("script", Name));            
        }

        protected override void CleanUp(){
            //Clean temp files
            var dir = Path.Combine(GetSamplePath("script"), "temp", Name);
            if(Directory.Exists(dir)) Directory.Delete(dir, true);       

            //Clean logs
            var logs = Path.Combine(AutoCheck.Core.Utils.AppFolder, "logs", Name);
            if(Directory.Exists(logs)) Directory.Delete(logs, true);                      
        }

        [Test, Category("Echo")]
        public void ParseBody_ECHO_RUN()
        {  
            var s = new AutoCheck.Core.Script(GetSampleFile("echo_ok1.yaml"));
            Assert.AreEqual("Running script echo_ok1 (v1.0.0.0):\r\n   ECHO", s.Output.ToString());
        }

        [Test, Category("Echo")]
        public void ParseBody_ECHO_CONTENT()
        {  
            var s = new AutoCheck.Core.Script(GetSampleFile("echo_ok2.yaml"));
            Assert.AreEqual("Running script echo_ok2 (v1.0.0.0):\r\n   ECHO 1\r\n   Question 1 [1 point]:\r\n      ECHO 2\r\n\r\n   TOTAL SCORE: 10 / 10", s.Output.ToString());
        }

         [Test, Category("Inherits")]
        public void ParseBody_INHERITS_VARS_REPLACE()
        {        
            try{  
                var s = new AutoCheck.Core.Script(GetSampleFile("inherits\\inherits_vars_ok1.yaml"));
            }   
            catch(ResultMismatchException ex){
                Assert.AreEqual("Expected -> Fer; Found -> New Fer", ex.Message);
            }                                       
        }

        //TODO: test to override other level-1 nodes (only 'vars' has been tested)

        [Test, Category("Inherits")]
        public void ParseBody_INHERITS_RUN_FOLDER()
        {       
            var dest = Path.Combine(GetSamplePath("script"), "temp", "inherits", "test2");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);                                 

            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest, "nopass.zip"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.zip")));
            var s = new AutoCheck.Core.Script(GetSampleFile("inherits\\inherits_run_ok1.yaml"));            
            
            Assert.AreEqual($"Running script inherits_run_ok1 (v1.0.0.1):\r\nRunning script inherits_run_ok1 (v1.0.0.1) in single mode for {dest}:", s.Output.ToString());
            Directory.Delete(dest, true);
        }
    }
}