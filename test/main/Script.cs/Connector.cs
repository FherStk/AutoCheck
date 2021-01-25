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
    public class Connector : Test
    {                
        [OneTimeSetUp]
        public virtual void StartUp() 
        {
            SamplesScriptFolder = GetSamplePath(Path.Combine("script", "body", Name));            
        }       

        [Test, Category("Connector"), Category("Local")]
        public void Script_CONNECTOR_EMPTY()
        {  
            Assert.DoesNotThrow(() => new AutoCheck.Core.Script(GetSampleFile("connector_ok1.yaml")));                                         
        }

        [Test, Category("Connector"), Category("Local")]
        public void Script_CONNECTOR_INLINE_ARGS()
        {              
            Assert.DoesNotThrow(() => new AutoCheck.Core.Script(GetSampleFile("connector_ok2.yaml")));            
        }

        [Test, Category("Connector"), Category("Local")]
        public void Script_CONNECTOR_TYPED_ARGS()
        {                          
            Assert.DoesNotThrow(() => new AutoCheck.Core.Script(GetSampleFile("connector_ok3.yaml")));                                    
        }

        [Test, Category("Connector"), Category("Local")]
        public void Script_CONNECTOR_MULTI_LOAD()
        {                          
            Assert.DoesNotThrow(() => new AutoCheck.Core.Script(GetSampleFile("connector_ok4.yaml")));                          
        }

        [Test, Category("Connector"), Category("Remote")]
        public void Script_CONNECTOR_REMOTE_HOST()
        {                          
            //Needs a remote GNU user called usuario@usuario
            Assert.DoesNotThrow(() => new AutoCheck.Core.Script(GetSampleFile("connector_ok5.yaml")));                          
        }

        [Test, Category("Connector"), Category("Local")]
        public void Script_CONNECTOR_IMPLICIT_INVALID_INLINE_ARGS()
        {  
            var s = new AutoCheck.Core.Script(GetSampleFile("connector_ko1.yaml"));
            Assert.AreEqual("Running script connector_ko1 (v1.0.0.0):\r\n   Testing connector... ERROR:\n      -Unable to find any constructor for the Connector 'Shell' that matches with the given set of arguments.", s.Output.ToString());        
        }

        [Test, Category("Connector"), Category("Local")]
        public void Script_CONNECTOR_EXPLICIT_INVALID_INLINE_ARGS()
        {   
            var s = new AutoCheck.Core.Script(GetSampleFile("connector_ko2.yaml"));
            Assert.AreEqual("Running script connector_ko2 (v1.0.0.0):\r\n   Testing connector... ERROR:\n      -Unable to find any constructor for the Connector 'Css' that matches with the given set of arguments.\r\n\r\n   Aborting execution!", s.Output.ToString());
        }

        [Test, Category("Connector"), Category("Local")]
        public void Script_CONNECTOR_EXPLICIT_INVALID_TYPED_ARGS()
        {  
            var s = new AutoCheck.Core.Script(GetSampleFile("connector_ko3.yaml"));
            Assert.AreEqual("Running script connector_ko3 (v1.0.0.0):\r\n   Testing connector... ERROR:\n      -Unable to find any constructor for the Connector 'Odoo' that matches with the given set of arguments.\r\n\r\n   Aborting execution!", s.Output.ToString());                           
        }

        [Test, Category("Connector"), Category("Local")]
        public void Script_CONNECTOR_EXPLICIT_INVALID_ONEXCEPTION()
        {  
            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.Script(GetSampleFile("connector_ko4.yaml")));
        }

        [Test, Category("Connector"), Category("Local")]
        public void Script_CONNECTOR_EXPLICIT_INVALID_SILENT()
        {  
            Assert.DoesNotThrow(() => new AutoCheck.Core.Script(GetSampleFile("connector_ko5.yaml")));
        }
    }
}