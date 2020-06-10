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

using NUnit.Framework;
using AutoCheck.Core;

namespace AutoCheck.Test.Connectors
{
    [Parallelizable(ParallelScope.All)]    
    public class LocalShell : Core.Test
    {
        [SetUp]
        public void Setup() 
        {
            base.Setup("localShell");
        }

        [Test]
        public void Constructor()
        {                      
            Assert.DoesNotThrow(() => new AutoCheck.Connectors.LocalShell());
        }

        [Test]
        public void GetFolder()
        {            
            using(var conn = new AutoCheck.Connectors.LocalShell()){
                Assert.IsNull(conn.GetFolder(this.SamplesPath, "testSubFolder11", false));
                Assert.IsNotNull(conn.GetFolder(this.SamplesPath, "testSubFolder11", true));
                Assert.IsNotNull(conn.GetFolder(this.SamplesPath, "testFolder1", false));                
                Assert.IsNotNull(conn.GetFolder(this.SamplesPath, "testFolder1", true));
            }                
        }

        [Test]
        public void GetFile()
        {            
            using(var conn = new AutoCheck.Connectors.LocalShell()){                
                Assert.IsNull(conn.GetFile(this.SamplesPath, "testFile11.txt", false));
                Assert.IsNotNull(conn.GetFile(this.SamplesPath, "testFile11.txt", true));
                Assert.IsNull(conn.GetFile(this.SamplesPath, "testFile11.txt", false));                
                Assert.IsNotNull(conn.GetFile(this.SamplesPath, "testFile11.txt", true));
            }                
        }
        
        [Test]
        public void CountFolders()
        {            
            using(var conn = new AutoCheck.Connectors.LocalShell()){
                Assert.AreEqual(2, conn.CountFolders(this.SamplesPath, false));
                Assert.AreEqual(6, conn.CountFolders(this.SamplesPath, true));
            }                
        }

        [Test]
        public void CountFiles()
        {            
            using(var conn = new AutoCheck.Connectors.LocalShell()){
                Assert.AreEqual(0, conn.CountFiles(this.SamplesPath, false));
                Assert.AreEqual(2, conn.CountFiles(this.SamplesPath, true));
            }                
        }

        [Test]
        public void RunCommand()
        {                        
            using(var conn = new AutoCheck.Connectors.LocalShell()){                
                string command = "ls";
                if(conn.CurrentOS == OS.WIN) 
                    command = string.Format("wsl {0}", command);
                
                //TODO: on windows, test if the  wsl is installed because wsl -e will be used to test linux commands and windows ones in one step if don't, throw an exception

                var result = conn.RunCommand(command);
                Assert.AreEqual(0, result.code);
                Assert.IsNotNull(result.response);

                result = conn.RunCommand("fake");
                Assert.AreNotEqual(0, result.code);
                Assert.IsNotNull(result.response);
            }                
        }
    }
}