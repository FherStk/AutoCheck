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
using AutoCheck.Core;

namespace AutoCheck.Test.Checkers
{
    [Parallelizable(ParallelScope.All)]    
    public class Html : Core.Test
    {
        //TODO: Check the exact errors messages, otherwise cannot be assured its amount and content (do not check only amount, the exact message output is needed for debug) 
        
        private const string _fake = "fake";

        [SetUp]
        public void Setup() 
        {
            base.Setup("html");
            AutoCheck.Core.Output.Instance.Disable();
        }

        [Test]
        public void Constructor()
        {            
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Checkers.Html("", "someFile.ext"));
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Checkers.Html("somePath", ""));
            Assert.Throws<DirectoryNotFoundException>(() => new AutoCheck.Checkers.Html("somePath", "someFile.ext"));
            Assert.Throws<FileNotFoundException>(() => new AutoCheck.Checkers.Html(this.SamplesPath, "someFile.ext"));
            Assert.DoesNotThrow(() => new AutoCheck.Checkers.Html(this.SamplesPath, "empty.html"));
            Assert.DoesNotThrow(() => new AutoCheck.Checkers.Html(this.SamplesPath, "correct.html"));
            Assert.DoesNotThrow(() => new AutoCheck.Checkers.Html(this.SamplesPath, "incorrect.html"));
        }   

        [Test]
        public void CheckIfNodesMatchesAmount()
        {    
            using(var html = new AutoCheck.Checkers.Html(this.SamplesPath, "correct.html"))
            {                               
                Assert.AreEqual(new List<string>(), html.CheckIfNodesMatchesAmount("//body", 1));
                Assert.AreNotEqual(new List<string>(), html.CheckIfNodesMatchesAmount("//body", 1, Operator.GREATER));
                Assert.AreEqual(new List<string>(), html.CheckIfNodesMatchesAmount("//body", 1, Operator.GREATEREQUALS));
                Assert.AreNotEqual(new List<string>(), html.CheckIfNodesMatchesAmount("//body", 1, Operator.LOWER));
                Assert.AreEqual(new List<string>(), html.CheckIfNodesMatchesAmount("//body", 1, Operator.LOWEREQUALS));

                Assert.AreEqual(new List<string>(), html.CheckIfNodesMatchesAmount("//body/p", 3));
                Assert.AreEqual(new List<string>(), html.CheckIfNodesMatchesAmount("//body/div/p", 3));
                Assert.AreEqual(new List<string>(), html.CheckIfNodesMatchesAmount("//p", 6));
                Assert.AreEqual(new List<string>(), html.CheckIfNodesMatchesAmount("//body//p", 6));
                Assert.AreEqual(new List<string>(), html.CheckIfNodesMatchesAmount("//body/.//input", 3));
                Assert.AreEqual(new List<string>(), html.CheckIfNodesMatchesAmount("//input[@type='text']", 2));
                Assert.AreEqual(new List<string>(), html.CheckIfNodesMatchesAmount("//input[@type='text'] | //input[@type='email']", 3));
            }
        }  

        [Test]
        public void CheckIfSiblingsMatchesAmount()
        {    
            using(var html = new AutoCheck.Checkers.Html(this.SamplesPath, "correct.html"))
            {                               
                Assert.AreEqual(new List<string>(), html.CheckIfSiblingsMatchesAmount("//body", 1));
                Assert.AreNotEqual(new List<string>(), html.CheckIfSiblingsMatchesAmount("//body", 1, Operator.GREATER));
                Assert.AreEqual(new List<string>(), html.CheckIfSiblingsMatchesAmount("//body", 1, Operator.GREATEREQUALS));
                Assert.AreNotEqual(new List<string>(), html.CheckIfSiblingsMatchesAmount("//body", 1, Operator.LOWER));
                Assert.AreEqual(new List<string>(), html.CheckIfSiblingsMatchesAmount("//body", 1, Operator.LOWEREQUALS));

                Assert.AreEqual(new List<string>(), html.CheckIfSiblingsMatchesAmount("//body/p", 3));
                Assert.AreEqual(new List<string>(), html.CheckIfSiblingsMatchesAmount("//body/div/p", 3));

                Assert.AreEqual(new List<string>(), html.CheckIfSiblingsMatchesAmount("//p", new int[]{3, 3}));
                Assert.AreEqual(new List<string>(), html.CheckIfSiblingsMatchesAmount("//body//p", new int[]{3, 3}));
                Assert.AreNotEqual(new List<string>(), html.CheckIfSiblingsMatchesAmount("//body//p", new int[]{3, 2}));
                Assert.AreNotEqual(new List<string>(), html.CheckIfSiblingsMatchesAmount("//body//p", new int[]{2, 3}));
                Assert.AreNotEqual(new List<string>(), html.CheckIfSiblingsMatchesAmount("//body//p", new int[]{2, 2}));

                Assert.AreNotEqual(new List<string>(), html.CheckIfSiblingsMatchesAmount("//body//p", new int[]{3, 3}, Operator.GREATER));
                Assert.AreEqual(new List<string>(), html.CheckIfSiblingsMatchesAmount("//body//p", new int[]{3, 3}, Operator.GREATEREQUALS));
                Assert.AreNotEqual(new List<string>(), html.CheckIfSiblingsMatchesAmount("//body//p", new int[]{3, 3}, Operator.LOWER));
                Assert.AreEqual(new List<string>(), html.CheckIfSiblingsMatchesAmount("//body//p", new int[]{3, 3}, Operator.LOWEREQUALS));
            }
        } 

        [Test]
        public void CheckIfNodesContentMatchesAmount()
        {    
            using(var html = new AutoCheck.Checkers.Html(this.SamplesPath, "correct.html"))
            {                               
                Assert.AreEqual(new List<string>(), html.CheckIfNodesContentMatchesAmount("//body/p", 72));
                Assert.AreEqual(new List<string>(), html.CheckIfNodesContentMatchesAmount("//body/div/p", 72));
                Assert.AreEqual(new List<string>(), html.CheckIfNodesContentMatchesAmount("//p", 144));
                Assert.AreEqual(new List<string>(), html.CheckIfNodesContentMatchesAmount("//body//p", 144));

                Assert.AreNotEqual(new List<string>(), html.CheckIfNodesContentMatchesAmount("//body//p", 144, Operator.GREATER));
                Assert.AreEqual(new List<string>(), html.CheckIfNodesContentMatchesAmount("//body//p", 144, Operator.GREATEREQUALS));
                Assert.AreNotEqual(new List<string>(), html.CheckIfNodesContentMatchesAmount("//body//p", 144, Operator.LOWER));
                Assert.AreEqual(new List<string>(), html.CheckIfNodesContentMatchesAmount("//body//p", 144, Operator.LOWEREQUALS));
            }
        }

        [Test]
        public void CheckIfNodesRelatedLabelsMatchesAmount()
        {    
            using(var html = new AutoCheck.Checkers.Html(this.SamplesPath, "correct.html"))
            {                               
                Assert.AreEqual(new List<string>(), html.CheckIfNodesRelatedLabelsMatchesAmount("//input[@type='text']", new int[]{1, 2}));
                Assert.AreEqual(new List<string>(), html.CheckIfNodesRelatedLabelsMatchesAmount("//input", new int[]{1, 2, 0}));
                Assert.AreEqual(new List<string>(), html.CheckIfNodesRelatedLabelsMatchesAmount("//input[@id='name']", new int[]{1}));
                Assert.AreEqual(new List<string>(), html.CheckIfNodesRelatedLabelsMatchesAmount("//input[@id='name']", 1));

                Assert.AreNotEqual(new List<string>(), html.CheckIfNodesRelatedLabelsMatchesAmount("//input[@type='text']", new int[]{1, 2}, Operator.GREATER));
                Assert.AreNotEqual(new List<string>(), html.CheckIfNodesRelatedLabelsMatchesAmount("//input[@type='text']", new int[]{1, 1}, Operator.GREATER));
                Assert.AreEqual(new List<string>(), html.CheckIfNodesRelatedLabelsMatchesAmount("//input[@type='text']", new int[]{0, 1}, Operator.GREATER));

                Assert.AreNotEqual(new List<string>(), html.CheckIfNodesRelatedLabelsMatchesAmount("//input[@type='text']", new int[]{2, 3}, Operator.GREATEREQUALS));
                Assert.AreNotEqual(new List<string>(), html.CheckIfNodesRelatedLabelsMatchesAmount("//input[@type='text']", new int[]{2, 2}, Operator.GREATEREQUALS));
                Assert.AreEqual(new List<string>(), html.CheckIfNodesRelatedLabelsMatchesAmount("//input[@type='text']", new int[]{1, 1}, Operator.GREATEREQUALS));                                

                Assert.AreNotEqual(new List<string>(), html.CheckIfNodesRelatedLabelsMatchesAmount("//input[@type='text']", new int[]{1, 1}, Operator.LOWER));
                Assert.AreNotEqual(new List<string>(), html.CheckIfNodesRelatedLabelsMatchesAmount("//input[@type='text']", new int[]{2, 1}, Operator.LOWER));
                Assert.AreEqual(new List<string>(), html.CheckIfNodesRelatedLabelsMatchesAmount("//input[@type='text']", new int[]{3, 3}, Operator.LOWER));

                Assert.AreNotEqual(new List<string>(), html.CheckIfNodesRelatedLabelsMatchesAmount("//input[@type='text']", new int[]{0, 0}, Operator.LOWEREQUALS));
                Assert.AreNotEqual(new List<string>(), html.CheckIfNodesRelatedLabelsMatchesAmount("//input[@type='text']", new int[]{1, 1}, Operator.LOWEREQUALS));
                Assert.AreEqual(new List<string>(), html.CheckIfNodesRelatedLabelsMatchesAmount("//input[@type='text']", new int[]{2, 2}, Operator.LOWEREQUALS));
            }
        } 

        [Test]
        public void CheckIfTablesIsConsistent()
        {    
            using(var html = new AutoCheck.Checkers.Html(this.SamplesPath, "correct.html"))
            {                               
                Assert.AreEqual(new List<string>(), html.CheckIfTablesIsConsistent("//table[@id='simpleOk']"));
                Assert.AreEqual(new List<string>(), html.CheckIfTablesIsConsistent("//table[@id='colspanOk']"));
                Assert.AreNotEqual(new List<string>(), html.CheckIfTablesIsConsistent("//table[@id='colspanKo']"));
                // //TODO: check for rowspan consistency                
            }
        }                             
    }
}