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

using NUnit.Framework;
using AutoCheck.Core.Exceptions;
using OS = AutoCheck.Core.Utils.OS;

namespace AutoCheck.Test.Connectors
{
    [Parallelizable(ParallelScope.All)]    
    public class Atom : Test
    {         
        [Test]
        [TestCase("correct.atom")]
        public void Constructor_Local_DoesNotThrow(string file)
        {      
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Atom(GetSampleFile(file)));
        }

        [Test]
        [TestCase("correct.atom", OS.GNU, "localhost", "usuario", "usuario")]
        public void Constructor_Remote_DoesNotThrow(string file, OS remoteOS, string host, string username, string password)
        {     
            //Note: the source code for local and remote mode are exactly the same, just need to test that the remote file is being downloaded from remote and parsed. 
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Atom(remoteOS, host, username, password, LocalPathToWsl(GetSampleFile(file))));
        }

        [Test]
        [TestCase("incorrect.atom")]
        public void ValidateAtomAgainstW3C_Throws_DocumentInvalidException(string file)
        {                        
            using(var conn = new AutoCheck.Core.Connectors.Atom(GetSampleFile(file)))
                Assert.Throws<DocumentInvalidException>(() => conn.ValidateAtomAgainstW3C());                                
        } 

        [Test]
        [TestCase("correct.atom")]
        public void ValidateAtomAgainstW3C_DoesNotThrow(string file)
        {                        
            using(var conn = new AutoCheck.Core.Connectors.Atom(GetSampleFile(file)))
                Assert.DoesNotThrow(() => conn.ValidateAtomAgainstW3C());
        } 

        [Test]
        [TestCase("correct.atom", "//feed", ExpectedResult=1)]
        [TestCase("correct.atom", "//feed/title", ExpectedResult=1)]
        [TestCase("correct.atom", "//feed//title", ExpectedResult=3)]
        [TestCase("incorrect.atom", "//feed", ExpectedResult=1)]
        [TestCase("incorrect.atom", "//feed/title", ExpectedResult=0)]
        [TestCase("incorrect.atom", "//feed//title", ExpectedResult=2)]
        public int CountNodes_DoesNotThrow(string file, string xpath)
        {                        
            using(var conn = new AutoCheck.Core.Connectors.Rss(GetSampleFile(file)))
                return conn.CountNodes(xpath);
        }            
    }
}