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
using System.Linq;
using NUnit.Framework;
using AutoCheck.Core.Exceptions;
using OS = AutoCheck.Core.Utils.OS;

namespace AutoCheck.Test.Connectors
{
    [Parallelizable(ParallelScope.All)]    
    public class Html : Test
    {       
        [Test]
        [TestCase("")]
        public void Constructor_Local_Throws_ArgumentNullException(string file)
        {      
             Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.Html(file));
        }

        [Test]
        [TestCase("someFile.ext")]
        public void Constructor_Local_Throws_FileNotFoundException(string file)
        {      
            Assert.Throws<FileNotFoundException>(() => new AutoCheck.Core.Connectors.Html(GetSampleFile(file)));
        }
   
        [Test]
        [TestCase("", OS.GNU, "localhost", "usuario", "usuario")]
        public void Constructor_Remote_Throws_ArgumentNullException(string file, OS remoteOS, string host, string username, string password)
        {     
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.Html(remoteOS, host, username, password, file));
        }

        [Test]
        [TestCase(_FAKE, OS.GNU, "localhost", "usuario", "usuario")]
        public void Constructor_Remote_Throws_FileNotFoundException(string file, OS remoteOS, string host, string username, string password)
        {     
            Assert.Throws<FileNotFoundException>(() => new AutoCheck.Core.Connectors.Html(remoteOS, host, username, password, file));
        }

        [Test]
        [TestCase("correct.html", OS.GNU, "localhost", "usuario", "usuario")]
        public void Constructor_DoesNotThrow(string file, OS remoteOS, string host, string username, string password)
        {           
            //Note: the source code for local and remote mode are exactly the same, just need to test that the remote file is being downloaded from remote and parsed.
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Html(remoteOS, host, username, password, LocalPathToWsl(GetSampleFile(file))));            
        }

        [Test]
        [TestCase("incorrect.html")]
        [TestCase("empty.html")]
        public void ValidateHtml5AgainstW3C_Throws_DocumentInvalidException(string file)
        {                        
            using(var conn = new AutoCheck.Core.Connectors.Html(GetSampleFile(file)))
                Assert.Throws<DocumentInvalidException>(() => conn.ValidateHtml5AgainstW3C());            
        } 

        [Test]
        [TestCase("correct.html")]
        public void ValidateHtml5AgainstW3C_DoesNotThrow(string file)
        {                        
            using(var conn = new AutoCheck.Core.Connectors.Html(GetSampleFile(file)))
                Assert.DoesNotThrow(() => conn.ValidateHtml5AgainstW3C());            
        }  

        [Test]
        [TestCase("correct.html", "//body", ExpectedResult = 1)]
        [TestCase("correct.html", "//body/p", ExpectedResult = 3)]
        [TestCase("correct.html", "//body/div/p", ExpectedResult = 3)]
        [TestCase("correct.html", "//p", ExpectedResult = 6)]
        [TestCase("correct.html", "//body//p", ExpectedResult = 6)]
        [TestCase("correct.html", "//body/.//input", ExpectedResult = 3)]
        [TestCase("correct.html", "//input[@type='text']", ExpectedResult = 2)]
        [TestCase("correct.html", "//input[@type='text'] | //input[@type='email']", ExpectedResult = 3)]
        public int CountNodes_DoesNotThrow(string file, string xpath)
        {                        
            //Internally uses SelectNodes
            using(var conn = new AutoCheck.Core.Connectors.Html(GetSampleFile(file)))
                return conn.CountNodes(xpath);
        }      

        [Test]
        [TestCase("correct.html", "//body", ExpectedResult = new int[]{1})]
        [TestCase("correct.html", "//body/p", ExpectedResult = new int[]{3})]
        [TestCase("correct.html", "//body/div/p", ExpectedResult = new int[]{3})]
        [TestCase("correct.html", "//p", ExpectedResult = new int[]{3, 3})]
        [TestCase("correct.html", "//body//p", ExpectedResult = new int[]{3, 3})]        
        public int[] CountSiblings_DoesNotThrow(string file, string xpath)
        {           
            //Internally uses GroupSiblings           
            using(var conn = new AutoCheck.Core.Connectors.Html(GetSampleFile(file)))
                return conn.CountSiblings(xpath);          
        }

        [Test]
        [TestCase("correct.html", "//body/p", ExpectedResult = 72)]
        [TestCase("correct.html", "//body/div/p", ExpectedResult = 72)]
        [TestCase("correct.html", "//p", ExpectedResult = 144)]
        [TestCase("correct.html", "//body//p", ExpectedResult = 144)]        
        public int ContentLength_DoesNotThrow(string file, string xpath)
        {                        
            using(var conn = new AutoCheck.Core.Connectors.Html(GetSampleFile(file)))
                return conn.ContentLength(xpath);        
        }

        [Test]
        [TestCase("correct.html", "//input[@type='text']", ExpectedResult = new int[]{1, 2})]
        [TestCase("correct.html", "//input", ExpectedResult = new int[]{1, 2, 0})]
        public int[] GetRelatedLabels_DoesNotThrow(string file, string xpath)
        {                        
            using(var conn = new AutoCheck.Core.Connectors.Html(GetSampleFile(file)))
                return conn.GetRelatedLabels(xpath).Select(x => x.Value.Count()).ToArray();            
        }

        [Test]
        [TestCase("correct.html", "//table[@id='colspanKo']")]
        public void CheckTableConsistence_Throws_TableInconsistencyException(string file, string xpath)
        {                        
            //TODO: check for rowspan consistency
            using(var conn = new AutoCheck.Core.Connectors.Html(GetSampleFile(file)))
                Assert.Throws<TableInconsistencyException>(() => conn.ValidateTable(xpath));                            
        }

        [Test]
        [TestCase("correct.html", "//table[@id='simpleOk']")]
        [TestCase("correct.html", "//table[@id='colspanOk']")]
        public void CheckTableConsistence_DoesNotThrow(string file, string xpath)
        {                        
            using(var conn = new AutoCheck.Core.Connectors.Html(GetSampleFile(file)))
                Assert.DoesNotThrow(() => conn.ValidateTable(xpath));                
        }
    }
}