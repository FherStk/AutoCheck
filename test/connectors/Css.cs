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
        public void Constructor()
        {           
            //Local  
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.Css(string.Empty));
            Assert.Throws<FileNotFoundException>(() => new AutoCheck.Core.Connectors.Css(Path.Combine(this.SamplesScriptFolder, _FAKE)));
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Css(Path.Combine(this.SamplesScriptFolder, "empty.css")));
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Css(Path.Combine(this.SamplesScriptFolder, "correct.css")));
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Css(Path.Combine(this.SamplesScriptFolder, "incorrect.css")));

            //Remote
            const OS remoteOS = OS.GNU;
            const string host = "localhost";
            const string username = "usuario";
            const string password = "usuario";

            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.Css(remoteOS, host, username, password, string.Empty));
            Assert.Throws<FileNotFoundException>(() => new AutoCheck.Core.Connectors.Css(remoteOS, host, username, password, _FAKE));

            //Note: the source code for local and remote mode are exactly the same, just need to test that the remote file is being downloaded from remote and parsed.
            var file = LocalPathToWsl(Path.Combine(this.SamplesScriptFolder, "correct.css"));
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Css(OS.GNU, host, username, password, file));  
        }

        [Test]
        public void ValidateCss3AgainstW3C()
        {            
            using(var conn = new AutoCheck.Core.Connectors.Css(Path.Combine(this.SamplesScriptFolder, "empty.css")))
                Assert.DoesNotThrow(() => conn.ValidateCss3AgainstW3C());

            using(var conn = new AutoCheck.Core.Connectors.Css(Path.Combine(this.SamplesScriptFolder, "correct.css")))
                Assert.DoesNotThrow(() => conn.ValidateCss3AgainstW3C());

            using(var conn = new AutoCheck.Core.Connectors.Css(Path.Combine(this.SamplesScriptFolder, "incorrect.css")))
                Assert.Throws<DocumentInvalidException>(() => conn.ValidateCss3AgainstW3C());            
        }

        [Test]
        public void PropertyExists()
        {            
            using(var css = new AutoCheck.Core.Connectors.Css(Path.Combine(this.SamplesScriptFolder, "correct.css")))
            {
                Assert.IsTrue(css.PropertyExists("color"));                  
                Assert.IsTrue(css.PropertyExists("font"));
                Assert.IsTrue(css.PropertyExists("font-size"));
                Assert.IsTrue(css.PropertyExists("line"));
                Assert.IsTrue(css.PropertyExists("line-height"));
                Assert.IsTrue(css.PropertyExists("line-height", "1"));
                Assert.IsFalse(css.PropertyExists("float"));                      
            }
        }

        [Test]
        public void PropertyApplied()
        {            
            using(var html = new AutoCheck.Core.Connectors.Html(Path.Combine(this.GetSamplePath("html"), "correct.html")))
            using(var css = new AutoCheck.Core.Connectors.Css(Path.Combine(this.SamplesScriptFolder, "correct.css")))
            {
                Assert.IsTrue(css.PropertyApplied(html.HtmlDoc, "color"));                  
                Assert.IsTrue(css.PropertyApplied(html.HtmlDoc, "font"));
                Assert.IsTrue(css.PropertyApplied(html.HtmlDoc, "font-size"));
                Assert.IsTrue(css.PropertyApplied(html.HtmlDoc, "line"));
                Assert.IsTrue(css.PropertyApplied(html.HtmlDoc, "line-height"));
                Assert.IsTrue(css.PropertyApplied(html.HtmlDoc, "line-height", "1"));
                Assert.IsFalse(css.PropertyApplied(html.HtmlDoc, "float"));      
                Assert.IsFalse(css.PropertyApplied(html.HtmlDoc, "text-shadow", "none"));                      
            }
        }
    }
}