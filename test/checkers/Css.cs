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
using System.Collections.Generic;
using NUnit.Framework;

namespace AutoCheck.Test.Checkers
{
    [Parallelizable(ParallelScope.All)]    
    public class Css : Core.Test
    {
        //TODO: Check the exact errors messages, otherwise cannot be assured its amount and content (do not check only amount, the exact message output is needed for debug) 

        private const string _fake = "fake";

        [SetUp]
        public void Setup() 
        {
            base.Setup("css");
            AutoCheck.Core.Output.Instance.Disable();
        }

        [Test]
        public void Constructor()
        {            
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Checkers.Css(string.Empty,string.Empty));
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Checkers.Css(_fake,string.Empty));
            Assert.Throws<DirectoryNotFoundException>(() => new AutoCheck.Checkers.Css(_fake, _fake));
            Assert.Throws<FileNotFoundException>(() => new AutoCheck.Checkers.Css(this.SamplesPath, _fake));
            Assert.DoesNotThrow(() => new AutoCheck.Checkers.Css(this.SamplesPath, "empty.css"));
            Assert.DoesNotThrow(() => new AutoCheck.Checkers.Css(this.SamplesPath, "correct.css"));
            Assert.DoesNotThrow(() => new AutoCheck.Checkers.Css(this.SamplesPath, "incorrect.css"));
        }   

        [Test]
        public void CheckIfPropertyApplied()
        {    
            using(var html = new AutoCheck.Connectors.Html(this.GetSamplePath("html"), "correct.html"))        
            using(var css = new AutoCheck.Checkers.Css(this.SamplesPath, "correct.css"))
            {                               
                Assert.AreNotEqual(new List<string>(), css.CheckIfPropertyApplied(html.HtmlDoc, "float"));        
                Assert.AreNotEqual(new List<string>(), css.CheckIfPropertyApplied(html.HtmlDoc, "text-shadow", "none"));
                Assert.AreEqual(new List<string>(), css.CheckIfPropertyApplied(html.HtmlDoc, "color"));                
            }
        } 

        [Test]
        public void CheckIfPropertiesAppliedMatchesAmount()
        {    
            using(var html = new AutoCheck.Connectors.Html(this.GetSamplePath("html"), "correct.html"))        
            using(var css = new AutoCheck.Checkers.Css(this.SamplesPath, "correct.css"))
            {                               
                Assert.AreNotEqual(new List<string>(), css.CheckIfPropertyApplied(html.HtmlDoc, _fake));
                Assert.AreEqual(new List<string>(), css.CheckIfPropertyApplied(html.HtmlDoc, "line"));
                Assert.AreEqual(new List<string>(), css.CheckIfPropertyApplied(html.HtmlDoc, "line-height"));
                Assert.AreEqual(new List<string>(), css.CheckIfPropertyApplied(html.HtmlDoc, "line-height", "1"));
                Assert.AreNotEqual(new List<string>(), css.CheckIfPropertyApplied(html.HtmlDoc, "line-height", _fake));
                
                Assert.AreEqual(new List<string>(), css.CheckIfPropertyApplied(html.HtmlDoc, new string[]{"line", "color"}));
                Assert.AreNotEqual(new List<string>(), css.CheckIfPropertyApplied(html.HtmlDoc, new string[]{"line", _fake}));
                Assert.AreEqual(new List<string>(), css.CheckIfPropertyApplied(html.HtmlDoc, new string[]{"line", _fake}, 1));

                Assert.AreEqual(new List<string>(), css.CheckIfPropertyApplied(html.HtmlDoc, new Dictionary<string, string>(){{"line-height", "1"}, {"color", null}}));
                Assert.AreNotEqual(new List<string>(), css.CheckIfPropertyApplied(html.HtmlDoc, new Dictionary<string, string>(){{"line-height", "1"}, {"color", _fake}}));
                Assert.AreEqual(new List<string>(), css.CheckIfPropertyApplied(html.HtmlDoc, new Dictionary<string, string>(){{"line-height", "1"}, {"color", _fake}}, 1));
            }
        }                
    }
}