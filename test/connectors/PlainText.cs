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
using OS = AutoCheck.Core.Utils.OS;

namespace AutoCheck.Test.Connectors
{
    [Parallelizable(ParallelScope.All)]    
    public class PlainText : Test
    { 
        string commentsRegex = "<!--[\\s\\S\n]*?-->";

        [Test]
        public void Constructor()
        {            
            //Local
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.PlainText(""));
            Assert.Throws<FileNotFoundException>(() => new AutoCheck.Core.Connectors.PlainText(Path.Combine(this.SamplesScriptFolder, "someFile.ext")));            

            //Remote
            const OS remoteOS = OS.GNU;
            const string host = "localhost";
            const string username = "usuario";
            const string password = "usuario";

            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.PlainText(remoteOS, host, username, password, string.Empty));
            Assert.Throws<FileNotFoundException>(() => new AutoCheck.Core.Connectors.PlainText(remoteOS, host, username, password, _FAKE));

            //Note: the source code for local and remote mode are exactly the same, just need to test that the remote file is being downloaded from remote and parsed.
            var file = LocalPathToWsl(Path.Combine(this.SamplesScriptFolder, "dtd_no_comments.dtd"));
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.PlainText(OS.GNU, host, username, password, file));
        }

        [Test]
        public void Count()
        {   
            //Uses Find() internally
            Assert.AreEqual(0, new AutoCheck.Core.Connectors.PlainText(Path.Combine(this.SamplesScriptFolder, "dtd_no_comments.dtd")).Count(commentsRegex)); 
            Assert.AreEqual(2, new AutoCheck.Core.Connectors.PlainText(Path.Combine(this.SamplesScriptFolder, "dtd_few_comments.dtd")).Count(commentsRegex)); 

            var pt = new AutoCheck.Core.Connectors.PlainText(Path.Combine(this.SamplesScriptFolder, "dtd_all_comments.dtd"));
            Assert.AreEqual(pt.plainTextDoc.Lines, pt.Count(commentsRegex)); 
        }           
    }
}