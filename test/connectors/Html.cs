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
    public class Html : Test
    {       
        [Test]
        public void Constructor()
        {            
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.Html("", "someFile.ext"));
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.Html("somePath", ""));
            Assert.Throws<DirectoryNotFoundException>(() => new AutoCheck.Core.Connectors.Html("somePath", "someFile.ext"));
            Assert.Throws<FileNotFoundException>(() => new AutoCheck.Core.Connectors.Html(this.SamplesScriptFolder, "someFile.ext"));
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Html(this.SamplesScriptFolder, "empty.html"));
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Html(this.SamplesScriptFolder, "correct.html"));
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Html(this.SamplesScriptFolder, "incorrect.html"));
        }

        [Test]
        public void ValidateCSS3AgainstW3C()
        {                        
            using(var conn = new AutoCheck.Core.Connectors.Html(this.SamplesScriptFolder, "correct.html"))
                Assert.DoesNotThrow(() => conn.ValidateHTML5AgainstW3C());

            using(var conn = new AutoCheck.Core.Connectors.Html(this.SamplesScriptFolder, "incorrect.html"))
                Assert.Throws<DocumentInvalidException>(() => conn.ValidateHTML5AgainstW3C());            

            using(var conn = new AutoCheck.Core.Connectors.Html(this.SamplesScriptFolder, "empty.html"))
                Assert.Throws<DocumentInvalidException>(() => conn.ValidateHTML5AgainstW3C());            
        }      

        [Test]
        public void CountNodes()
        {                        
            //Internally uses SelectNodes
            using(var conn = new AutoCheck.Core.Connectors.Html(this.SamplesScriptFolder, "correct.html")){
                Assert.AreNotEqual(0, conn.CountNodes("//body"));
                Assert.AreEqual(3, conn.CountNodes("//body/p"));
                Assert.AreEqual(3, conn.CountNodes("//body/div/p"));
                Assert.AreEqual(6, conn.CountNodes("//p"));
                Assert.AreEqual(6, conn.CountNodes("//body//p"));
                Assert.AreEqual(3, conn.CountNodes("//body/.//input"));
                Assert.AreEqual(2, conn.CountNodes("//input[@type='text']"));
                Assert.AreEqual(3, conn.CountNodes("//input[@type='text'] | //input[@type='email']"));
            }
        }      

        [Test]
        public void CountSiblings()
        {           
            //Internally uses GroupSiblings           
            using(var conn = new AutoCheck.Core.Connectors.Html(this.SamplesScriptFolder, "correct.html")){
                Assert.AreEqual(new int[]{1}, conn.CountSiblings("//body"));
                Assert.AreEqual(new int[]{3}, conn.CountSiblings("//body/p"));
                Assert.AreEqual(new int[]{3}, conn.CountSiblings("//body/div/p"));                
                CollectionAssert.AreEqual(new int[]{3, 3}, conn.CountSiblings("//p"));
                CollectionAssert.AreEqual(new int[]{3, 3}, conn.CountSiblings("//body//p"));                
            }
        }

        [Test]
        public void ContentLength()
        {                        
            using(var conn = new AutoCheck.Core.Connectors.Html(this.SamplesScriptFolder, "correct.html")){             
                Assert.AreEqual(72, conn.ContentLength("//body/p"));
                Assert.AreEqual(72, conn.ContentLength("//body/div/p"));                
                Assert.AreEqual(144, conn.ContentLength("//p"));
                Assert.AreEqual(144, conn.ContentLength("//body//p"));                
            }
        }

        [Test]
        public void GetRelatedLabels()
        {                        
            using(var conn = new AutoCheck.Core.Connectors.Html(this.SamplesScriptFolder, "correct.html")){             
                var inputs = conn.GetRelatedLabels("//input[@type='text']");
                Assert.AreEqual(new int[]{1, 2}, inputs.Select(x => x.Value.Count()).ToArray());

                inputs = conn.GetRelatedLabels("//input");
                Assert.AreEqual(new int[]{1, 2, 0}, inputs.Select(x => x.Value.Count()).ToArray());
            }
        }

        [Test]
        public void CheckTableConsistence()
        {                        
            using(var conn = new AutoCheck.Core.Connectors.Html(this.SamplesScriptFolder, "correct.html")){
                Assert.DoesNotThrow(() => conn.ValidateTable("//table[@id='simpleOk']"));
                Assert.DoesNotThrow(() => conn.ValidateTable("//table[@id='colspanOk']"));
                Assert.Throws<TableInconsistencyException>(() => conn.ValidateTable("//table[@id='colspanKo']"));
                //TODO: check for rowspan consistency
            }
        }
    }
}