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
    public class Echo : Test
    {  
        public Echo(): base(Path.Combine("script")){
        }        
      
        [Test, Category("Echo"), Category("Local")]
        public void Script_ECHO_RUN()
        {  
            var s = new AutoCheck.Core.Script(GetSampleFile("echo_ok1.yaml"));
            Assert.AreEqual("Running script echo_ok1 (v1.0.0.0):\r\n   ECHO", s.Output.ToString());
        }

        [Test, Category("Echo"), Category("Local")]
        public void Script_ECHO_CONTENT()
        {  
            var s = new AutoCheck.Core.Script(GetSampleFile("echo_ok2.yaml"));
            Assert.AreEqual("Running script echo_ok2 (v1.0.0.0):\r\n   ECHO 1\r\n   Question 1 [1 point]:\r\n      ECHO 2\r\n\r\n   TOTAL SCORE: 10 / 10", s.Output.ToString());
        }
    }
}