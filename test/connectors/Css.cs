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
using NUnit.Framework;
using AutoCheck.Core.Exceptions;
using OS = AutoCheck.Core.Utils.OS;

namespace AutoCheck.Test.Connectors
{
    [Parallelizable(ParallelScope.All)]    
    public class Css : Test
    {    
        [Test]
        [TestCase("empty.css")]
        [TestCase("correct.css")]
        [TestCase("incorrect.css")]
        public void Constructor_Local_DoesNotThrow(string file)
        {      
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Css(GetSampleFile(file)));
        }

        [Test]
        [TestCase("correct.css", OS.GNU, "localhost", "autocheck", "autocheck")]
        public void Constructor_Remote_DoesNotThrow(string file, OS remoteOS, string host, string username, string password)
        {     
            //Note: the source code for local and remote mode are exactly the same, just need to test that the remote file is being downloaded from remote and parsed. 
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Css(remoteOS, host, username, password, LocalPathToRemote(GetSampleFile(file), username)));
        }

        [Test]
        [TestCase("")]        
        public void Constructor_Local_Throws_ArgumentNullException(string file)
        {      
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.Css(file));
        }

        [Test]
        [TestCase(_FAKE)]        
        public void Constructor_Local_Throws_FileNotFoundException(string file)
        {      
            Assert.Throws<FileNotFoundException>(() => new AutoCheck.Core.Connectors.Css(GetSampleFile(file)));
        }

        [Test]
        [TestCase("", OS.GNU, "localhost", "autocheck", "autocheck")]        
        public void Constructor_Remote_Throws_ArgumentNullException(string file, OS remoteOS, string host, string username, string password)
        {      
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.Css(remoteOS, host, username, password, file));
        }

        [Test]
        [TestCase(_FAKE, OS.GNU, "localhost", "autocheck", "autocheck")]        
        public void Constructor_Remote_Throws_FileNotFoundException(string file, OS remoteOS, string host, string username, string password)
        {      
            Assert.Throws<FileNotFoundException>(() => new AutoCheck.Core.Connectors.Css(remoteOS, host, username, password, file));
        }
       
        [Test]
        [TestCase("empty.css")]
        [TestCase("correct.css")]
        public void ValidateCss3AgainstW3C_DoesNotThrow(string file)
        {            
            using(var conn = new AutoCheck.Core.Connectors.Css(GetSampleFile(file)))
                Assert.DoesNotThrow(() => conn.ValidateCss3AgainstW3C());
        }

        [Test]
        [TestCase("incorrect.css")]
        public void ValidateCss3AgainstW3C_Throws_DocumentInvalidException(string file)
        {            
           using(var conn = new AutoCheck.Core.Connectors.Css(GetSampleFile(file)))
                Assert.Throws<DocumentInvalidException>(() => conn.ValidateCss3AgainstW3C());
        }

        [Test]
        [TestCase("correct.css","color", null, ExpectedResult = true)]
        [TestCase("correct.css","font", null, ExpectedResult = true)]
        [TestCase("correct.css","font-size", null, ExpectedResult = true)]
        [TestCase("correct.css","line", null, ExpectedResult = true)]
        [TestCase("correct.css","line-height", null, ExpectedResult = true)]
        [TestCase("correct.css","line-height", "1", ExpectedResult = true)]
        [TestCase("correct.css","float", null, ExpectedResult = false)]        
        public bool PropertyExists_DoesNotThrow(string file, string property, string value)
        {            
            using(var css = new AutoCheck.Core.Connectors.Css(GetSampleFile(file)))
                return css.PropertyExists(property, value);                
        }

        [Test]
        [TestCase("correct.html", "correct.css", "color", null, ExpectedResult = true)]
        [TestCase("correct.html", "correct.css", "font", null, ExpectedResult = true)]
        [TestCase("correct.html", "correct.css", "font-size", null, ExpectedResult = true)]
        [TestCase("correct.html", "correct.css", "line", null, ExpectedResult = true)]
        [TestCase("correct.html", "correct.css", "line-height", null, ExpectedResult = true)]
        [TestCase("correct.html", "correct.css", "line-height", "1", ExpectedResult = true)]
        [TestCase("correct.html", "correct.css", "float", null, ExpectedResult = false)]
        [TestCase("correct.html", "correct.css", "text-shadow", "none", ExpectedResult = false)]
        public bool PropertyApplied_DoesNotThrow(string htmlFile, string cssFile, string property, string value)
        {            
            using(var html = new AutoCheck.Core.Connectors.Html(GetSampleFile("html", htmlFile)))
            using(var css = new AutoCheck.Core.Connectors.Css(GetSampleFile(cssFile)))
            {
                return css.PropertyApplied(html.HtmlDoc, property, value);                
            }
        }
    }
}