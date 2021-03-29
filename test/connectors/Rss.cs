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
    public class Rss : Test
    {       
        [Test]
        public void Constructor()
        {            
            //Local
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.Rss(""));
            Assert.Throws<FileNotFoundException>(() => new AutoCheck.Core.Connectors.Rss(GetSampleFile("someFile.ext")));            

            //Remote
            const OS remoteOS = OS.GNU;
            const string host = "localhost";
            const string username = "usuario";
            const string password = "usuario";

            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.Rss(remoteOS, host, username, password, string.Empty));
            Assert.Throws<FileNotFoundException>(() => new AutoCheck.Core.Connectors.Rss(remoteOS, host, username, password, _FAKE));

            //Note: the source code for local and remote mode are exactly the same, just need to test that the remote file is being downloaded from remote and parsed.
            var file = LocalPathToWsl(GetSampleFile("correct.rss"));
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Rss(OS.GNU, host, username, password, file));
        }

        [Test]
        public void ValidateRssAgainstW3C()
        {                        
            using(var conn = new AutoCheck.Core.Connectors.Rss(GetSampleFile("correct.rss")))
                Assert.DoesNotThrow(() => conn.ValidateRssAgainstW3C());

            using(var conn = new AutoCheck.Core.Connectors.Rss(GetSampleFile("incorrect.rss")))
                Assert.Throws<DocumentInvalidException>(() => conn.ValidateRssAgainstW3C());                                
        } 

        [Test]
        public void CountNodes()
        {                        
            using(var conn = new AutoCheck.Core.Connectors.Rss(GetSampleFile("correct.rss"))){
                Assert.AreEqual(1, conn.CountNodes("//rss"));
                Assert.AreEqual(1, conn.CountNodes("//rss/channel/title"));
                Assert.AreEqual(2, conn.CountNodes("//rss//title"));
            }

            using(var conn = new AutoCheck.Core.Connectors.Rss(GetSampleFile("incorrect.rss"))){
                Assert.AreEqual(1, conn.CountNodes("//rss"));
                Assert.AreEqual(0, conn.CountNodes("//rss/channel/title"));
                Assert.AreEqual(1, conn.CountNodes("//rss//title"));
            }
        }      
    }
}