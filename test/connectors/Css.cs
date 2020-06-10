/*
    Copyright © 2020 Fernando Porrino Serrano
    Third party software licenses can be found at /docs/credits/thirdparties.md

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
using AutoCheck.Exceptions;

namespace AutoCheck.Test.Connectors
{
    [Parallelizable(ParallelScope.All)]    
    public class Css : Core.Test
    {
        private const string _fake = "fake";

        [SetUp]
        public void Setup() 
        {
            base.Setup("css");
        }

        [Test]
        public void Constructor()
        {            
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Connectors.Css(string.Empty,string.Empty));
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Connectors.Css(_fake,string.Empty));
            Assert.Throws<DirectoryNotFoundException>(() => new AutoCheck.Connectors.Css(_fake, _fake));
            Assert.Throws<FileNotFoundException>(() => new AutoCheck.Connectors.Css(this.SamplesPath, _fake));
            Assert.DoesNotThrow(() => new AutoCheck.Connectors.Css(this.SamplesPath, "empty.css"));
            Assert.DoesNotThrow(() => new AutoCheck.Connectors.Css(this.SamplesPath, "correct.css"));
            Assert.DoesNotThrow(() => new AutoCheck.Connectors.Css(this.SamplesPath, "incorrect.css"));
        }

        [Test]
        public void ValidateCSS3AgainstW3C()
        {            
            using(var conn = new AutoCheck.Connectors.Css(this.SamplesPath, "empty.css"))
                Assert.DoesNotThrow(() => conn.ValidateCSS3AgainstW3C());

            using(var conn = new AutoCheck.Connectors.Css(this.SamplesPath, "correct.css"))
                Assert.DoesNotThrow(() => conn.ValidateCSS3AgainstW3C());

            using(var conn = new AutoCheck.Connectors.Css(this.SamplesPath, "incorrect.css"))
                Assert.Throws<DocumentInvalidException>(() => conn.ValidateCSS3AgainstW3C());            
        }

        [Test]
        public void PropertyExists()
        {            
            using(var css = new AutoCheck.Connectors.Css(this.SamplesPath, "correct.css"))
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
            using(var html = new AutoCheck.Connectors.Html(this.GetSamplePath("html"), "correct.html"))
            using(var css = new AutoCheck.Connectors.Css(this.SamplesPath, "correct.css"))
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