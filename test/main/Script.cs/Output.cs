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

namespace AutoCheck.Test
{    
    [Parallelizable(ParallelScope.All)]    
    public class Output : Test
    {                
        [OneTimeSetUp]
        public virtual void StartUp() 
        {
            SamplesScriptFolder = GetSamplePath(Path.Combine("script", Name));            
        }
       
        [Test, Category("Output"), Category("Local")]
        public void Output_SINGLE_FILE_1()
        {             
            var log = Path.Combine(LogsScriptFolder, "OUTPUT SINGLE 1_Student Name 1.log");
            Assert.IsFalse(File.Exists(log));
            
            var s = new AutoCheck.Core.Script(GetSampleFile("output_single_1.yaml"));
            Assert.IsTrue(File.Exists(log));        
            Assert.IsTrue(File.ReadAllText(log).Equals(s.Output.ToString()));                
        } 

        [Test, Category("Output"), Category("Local")]
        public void Output_BATCH_FILE_1()
        {             
            var logs = new string[]{
                Path.Combine(LogsScriptFolder, "OUTPUT BATCH 1_Student Name 1.log"),
                Path.Combine(LogsScriptFolder, "OUTPUT BATCH 1_Student Name 2.log"),
                Path.Combine(LogsScriptFolder, "OUTPUT BATCH 1_Student Name 3.log")
            };

            foreach(var l in logs)
                Assert.IsFalse(File.Exists(l));
            
            var i = 0;
            var s = new AutoCheck.Core.Script(Path.Combine(GetSamplePath("script"), "output", "output_batch_1.yaml"));
                    
            foreach(var l in logs){
                Assert.IsTrue(File.Exists(l));        
                Assert.IsTrue(File.ReadAllText(l).Equals(s.Output.ToArray()[i++]));
            }
        } 
    }
}