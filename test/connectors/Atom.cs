/*
    Copyright © 2020 Fernando Porrino Serrano
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

namespace AutoCheck.Test.Connectors
{
    [Parallelizable(ParallelScope.All)]    
    public class Atom : Test
    {       
        [Test]
        public void Constructor()
        {            
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.Atom(""));
            Assert.Throws<FileNotFoundException>(() => new AutoCheck.Core.Connectors.Atom(Path.Combine(this.SamplesScriptFolder, "someFile.ext")));
        }

        [Test]
        public void ValidateAtomAgainstW3C()
        {                        
            using(var conn = new AutoCheck.Core.Connectors.Atom(Path.Combine(this.SamplesScriptFolder, "correct.atom")))
                Assert.DoesNotThrow(() => conn.ValidateAtomAgainstW3C());

            using(var conn = new AutoCheck.Core.Connectors.Atom(Path.Combine(this.SamplesScriptFolder, "incorrect.atom")))
                Assert.Throws<DocumentInvalidException>(() => conn.ValidateAtomAgainstW3C());                                
        } 

        [Test]
        public void CountNodes()
        {                        
            using(var conn = new AutoCheck.Core.Connectors.Rss(Path.Combine(this.SamplesScriptFolder, "correct.atom"))){
                Assert.AreEqual(1, conn.CountNodes("//feed"));
                Assert.AreEqual(1, conn.CountNodes("//feed/title"));
                Assert.AreEqual(3, conn.CountNodes("//feed//title"));
            }

            using(var conn = new AutoCheck.Core.Connectors.Rss(Path.Combine(this.SamplesScriptFolder, "incorrect.atom"))){
                Assert.AreEqual(1, conn.CountNodes("//feed"));
                Assert.AreEqual(0, conn.CountNodes("//feed/title"));
                Assert.AreEqual(2, conn.CountNodes("//feed//title"));
            }
        }       
    }
}