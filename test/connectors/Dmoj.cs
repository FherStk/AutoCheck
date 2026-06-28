/*
    Copyright © 2023 Fernando Porrino Serrano
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
using NUnit.Framework.Legacy;

namespace AutoCheck.Test.Connectors
{
    [Parallelizable(ParallelScope.All)]    
    public class Dmoj : Test
    {    
        [Test]
        [TestCase("dmoj.elpuig.xeill.net")]
        public void Constructor_DoesNotThrow(string host)
        {      
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Dmoj(host));
        }

        [Test]
        [TestCase("")]        
        public void Constructor_Throws_ArgumentNullException(string host)
        {      
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.Dmoj(host));
        }

        [Test]
        [TestCase("")]        
        public void DownloadContestSubmissions_Throws_ArgumentNullException(string contestCode)
        {      
            using(var conn = new AutoCheck.Core.Connectors.Dmoj("dmoj.elpuig.xeill.net"))
                Assert.Throws<ArgumentNullException>(() => conn.DownloadContestSubmissions(contestCode));
        }

        [Test]
        [TestCase("asix1p4curs22")]
        public void DownloadContestSubmissions_DoesNotThrow(string contestCode)
        {
            // Isolated path to avoid race condition with Real_Dmoj tests that download to the same contest folder
            var output = Path.Combine(AutoCheck.Core.Utils.TempFolder, "DMOJ", TestContext.CurrentContext.Test.ID);
            using(var conn = new AutoCheck.Core.Connectors.Dmoj("dmoj.elpuig.xeill.net")){
                Assert.DoesNotThrow(() => conn.DownloadContestSubmissions(contestCode, output));

                ClassicAssert.AreEqual(23, Directory.GetDirectories(output).Count());
                ClassicAssert.AreEqual(30, Directory.GetFiles(output, "*.java", SearchOption.AllDirectories).Count());
            }
        }
    }
}