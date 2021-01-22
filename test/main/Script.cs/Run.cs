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
    public class Run : Test
    {                
        [OneTimeSetUp]
        public virtual void StartUp() 
        {
            SamplesScriptFolder = GetSamplePath(Path.Combine("script", "body", Name));            
        }

        protected override void CleanUp(){
            //Clean temp files
            var dir = Path.Combine(GetSamplePath("script"), "temp", Name);
            if(Directory.Exists(dir)) Directory.Delete(dir, true);       

            //Clean logs
            var logs = Path.Combine(AutoCheck.Core.Utils.AppFolder, "logs", Name);
            if(Directory.Exists(logs)) Directory.Delete(logs, true);                      
        }

        [Test, Category("Run")]
        public void ParseBody_RUN_ECHO()
        {  
            Assert.DoesNotThrow(() => new AutoCheck.Core.Script(GetSampleFile("run_ok1.yaml")));
        }
 
        [Test, Category("Run")]
        public void ParseBody_RUN_FIND()
        {          
            Assert.DoesNotThrow(() => new AutoCheck.Core.Script(GetSampleFile("run_ok2.yaml")));
        }

        [Test, Category("Run")]
        public void ParseBody_RUN_CAPTION_OK()
        {          
            var s = new AutoCheck.Core.Script(GetSampleFile("run_ok3.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Running script run_ok3 (v1.0.0.1):\r\n   Checking if file exists... OK", log);
        }

        [Test, Category("Run")]
        public void ParseBody_RUN_CAPTION_ERROR()
        {          
            var s = new AutoCheck.Core.Script(GetSampleFile("run_ok4.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Running script run_ok4 (v1.0.0.1):\r\n   Checking if file exists... OK\r\n   Counting folders... ERROR:\n      -Expected -> Wanted ERROR!; Found -> 0", log);
        }

        [Test, Category("Run")]
        public void ParseBody_RUN_CAPTION_EXCEPTION()
        {   
            Assert.Throws<ArgumentInvalidException>(() => new AutoCheck.Core.Script(GetSampleFile("run_ko3.yaml")));                
        }

        [Test, Category("Run")]
        public void ParseBody_RUN_NOCAPTION_EXCEPTION()
        {   
            Assert.Throws<ResultMismatchException>(() => new AutoCheck.Core.Script(GetSampleFile("run_ko4.yaml")));                
        } 

        [Test, Category("Run")]
        public void ParseBody_RUN_EMPTY()
        {  
            var s = new AutoCheck.Core.Script(GetSampleFile("run_ok6.yaml"));
            Assert.AreEqual("Running script run_ok6 (v1.0.0.0):", s.Output.ToString());
        }

        [Test, Category("Run")]
        public void ParseBody_RUN_INVALID_TYPED_ARGS()
        {  
            Assert.Throws<ArgumentInvalidException>(() => new AutoCheck.Core.Script(GetSampleFile("run_ko2.yaml")));            
        }

        [Test, Category("Run")]
        public void ParseBody_RUN_CONFLICTIVE_ARGS()
        {  
            Assert.DoesNotThrow(() => new AutoCheck.Core.Script(GetSampleFile("run_ok5.yaml")));            
        }
    }
}