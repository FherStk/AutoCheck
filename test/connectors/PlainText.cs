/*
    Copyright © 2022 Fernando Porrino Serrano
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
using OS = AutoCheck.Core.Utils.OS;

namespace AutoCheck.Test.Connectors
{
    [Parallelizable(ParallelScope.All)]    
    public class PlainText : Test
    { 
        [Test]
        [TestCase("")]
        public void Constructor_Local_Throws_ArgumentNullException(string file)
        {      
             Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.PlainText(file));
        }

        [Test]
        [TestCase("someFile.ext")]
        public void Constructor_Local_Throws_FileNotFoundException(string file)
        {      
            Assert.Throws<FileNotFoundException>(() => new AutoCheck.Core.Connectors.PlainText(GetSampleFile(file)));
        }

        [Test]
        [TestCase("", OS.GNU, "localhost", "autocheck", "autocheck")]
        public void Constructor_Remote_Throws_ArgumentNullException(string file, OS remoteOS, string host, string username, string password)
        {     
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.PlainText(remoteOS, host, username, password, file));
        }

        [Test]
        [TestCase(_FAKE, OS.GNU, "localhost", "autocheck", "autocheck")]
        public void Constructor_Remote_Throws_FileNotFoundException(string file, OS remoteOS, string host, string username, string password)
        {     
            Assert.Throws<FileNotFoundException>(() => new AutoCheck.Core.Connectors.PlainText(remoteOS, host, username, password, file));
        }

        [Test]
        [TestCase("dtd_no_comments.dtd", OS.GNU, "localhost", "autocheck", "autocheck")]
        public void Constructor_DoesNotThrow(string file, OS remoteOS, string host, string username, string password)
        {           
            //Note: the source code for local and remote mode are exactly the same, just need to test that the remote file is being downloaded from remote and parsed.
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.PlainText(remoteOS, host, username, password, LocalPathToRemote(GetSampleFile(file), username)));            
        }



        [Test]
        [TestCase("dtd_no_comments.dtd", "<!--[\\s\\S\n]*?-->", ExpectedResult=0)]
        [TestCase("dtd_few_comments.dtd", "<!--[\\s\\S\n]*?-->", ExpectedResult=2)]
        [TestCase("dtd_all_comments.dtd", "<!--[\\s\\S\n]*?-->", ExpectedResult=21)]
        public int Count_DoesNotThrow(string file, string regex)
        {   
            //Uses Find() internally
            return new AutoCheck.Core.Connectors.PlainText(GetSampleFile(file)).Count(regex);             
        }           
    }
}