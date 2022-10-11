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
using AutoCheck.Core.Exceptions;
using OS = AutoCheck.Core.Utils.OS;

namespace AutoCheck.Test.Connectors
{    
    [Parallelizable(ParallelScope.All)]    
    public class Xml : Test
    {   
        [Test]
        [TestCase("")]
        public void Constructor_Local_Throws_ArgumentNullException(string file)
        {      
             Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.Xml(file));
        }

        [Test]
        [TestCase("someFile.ext")]
        public void Constructor_Local_Throws_FileNotFoundException(string file)
        {      
            Assert.Throws<FileNotFoundException>(() => new AutoCheck.Core.Connectors.Xml(GetSampleFile(file)));
        }

        [Test]
        [TestCase("", OS.GNU, "localhost", "autocheck", "autocheck")]
        public void Constructor_Remote_Throws_ArgumentNullException(string file, OS remoteOS, string host, string username, string password)
        {     
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.Xml(remoteOS, host, username, password, file));
        }

        [Test]
        [TestCase(_FAKE, OS.GNU, "localhost", "autocheck", "autocheck")]
        public void Constructor_Remote_Throws_FileNotFoundException(string file, OS remoteOS, string host, string username, string password)
        {     
            Assert.Throws<FileNotFoundException>(() => new AutoCheck.Core.Connectors.Xml(remoteOS, host, username, password, file));
        }

        [Test]
        [TestCase("sample1_simple_ok.xml", OS.GNU, "localhost", "autocheck", "autocheck")]
        public void Constructor_DoesNotThrow(string file, OS remoteOS, string host, string username, string password)
        {           
            //Note: the source code for local and remote mode are exactly the same, just need to test that the remote file is being downloaded from remote and parsed.
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Rss(remoteOS, host, username, password, LocalPathToRemote(GetSampleFile(file), username)));            
        }

        [Test]
        public void Validation_XML_DoesNotThrow()
        {               
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Xml(GetSampleFile("sample1_simple_ok.xml")));            
        }

        [Test]
        [TestCase("sample1_simple_ok.xml", System.Xml.ValidationType.DTD)]
        [TestCase("sample1_simple_ok.xml", System.Xml.ValidationType.Schema)]
        [TestCase("sample1_simple_ko.xml", null)]
        [TestCase("sample1_simple_ko.xml", System.Xml.ValidationType.DTD)]
        [TestCase("sample1_simple_ko.xml", System.Xml.ValidationType.Schema)]
        public void Validation_XML_Throws(string file, System.Xml.ValidationType validation)
        {               
            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.Connectors.Xml(GetSampleFile(file), validation));            
        }

        [Test]
        [TestCase("sample2_dtd_ok.xml", null)]
        [TestCase("sample2_dtd_ok.xml", System.Xml.ValidationType.DTD)]
        [TestCase("sample2_dtd_ko1.xml", null)]
        [TestCase("sample2_dtd_ko2.xml", null)]
        public void Validation_DTD_DoesNotThrow(string file, System.Xml.ValidationType validation)
        {   
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Xml(GetSampleFile(file), validation));            
        } 


        [Test]
        [TestCase("sample2_dtd_ok.xml", System.Xml.ValidationType.Schema)]
        [TestCase("sample2_dtd_ko1.xml", System.Xml.ValidationType.DTD)]
        [TestCase("sample2_dtd_ko2.xml", System.Xml.ValidationType.DTD)]
        [TestCase("sample3_xsd_ok.xml", System.Xml.ValidationType.DTD)]
        public void Validation_DTD_Throws(string file, System.Xml.ValidationType validation)
        {              
            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.Connectors.Xml(GetSampleFile(file), validation));
        } 


        [Test]        
        [TestCase("sample3_xsd_ok.xml", System.Xml.ValidationType.Schema)]
        [TestCase("sample3_xsd_ko1.xml", null)]
        [TestCase("sample3_xsd_ko2.xml", null)]
        public void Validation_XSD_DoesNotThrow(string file, System.Xml.ValidationType validation)
        {   
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Xml(GetSampleFile(file), validation));
        }

        [Test]
        [TestCase("sample3_xsd_ok.xml", System.Xml.ValidationType.DTD)]
        [TestCase("sample3_xsd_ko1.xml", System.Xml.ValidationType.Schema)]
        [TestCase("sample3_xsd_ko2.xml", System.Xml.ValidationType.Schema)]
        public void Validation_XSD_Throws(string file, System.Xml.ValidationType validation)
        {   
            //Trying to validate against DTD/XSD
            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.Connectors.Xml(GetSampleFile(file), validation));
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Xml(GetSampleFile("sample3_xsd_ok.xml"), System.Xml.ValidationType.Schema));

            //Trying to validate against XSD
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Xml(GetSampleFile("sample3_xsd_ko1.xml")));
            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.Connectors.Xml(GetSampleFile("sample3_xsd_ko1.xml"), System.Xml.ValidationType.Schema));

            //Testing XSD syntax
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Xml(GetSampleFile("sample3_xsd_ko2.xml")));
            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.Connectors.Xml(GetSampleFile("sample3_xsd_ko2.xml"), System.Xml.ValidationType.Schema));
        }  

        [Test]
        public void Comments()
        {   
            var xml = new AutoCheck.Core.Connectors.Xml(GetSampleFile("sample4_comments.xml"));
            Assert.AreEqual(3, xml.Comments.Length);        
        }       

        [Test]
        [TestCase("sample4_comments.xml", "//*", Core.Connectors.Xml.XmlNodeType.ALL, ExpectedResult=26)]
        [TestCase("sample4_comments.xml", "//become", Core.Connectors.Xml.XmlNodeType.ALL, ExpectedResult=2)]
        [TestCase("sample4_comments.xml", "/root/underline/harder/become", Core.Connectors.Xml.XmlNodeType.ALL, ExpectedResult=2)]
        [TestCase("sample4_comments.xml", "//*/@*", Core.Connectors.Xml.XmlNodeType.ALL, ExpectedResult=6)]
        [TestCase("sample4_comments.xml", "//*/@units", Core.Connectors.Xml.XmlNodeType.ALL, ExpectedResult=1)]
        [TestCase("sample4_comments.xml", "//*", Core.Connectors.Xml.XmlNodeType.NUMERIC, ExpectedResult=10)]
        [TestCase("sample4_comments.xml", "//*", Core.Connectors.Xml.XmlNodeType.BOOLEAN, ExpectedResult=2)]
        [TestCase("sample4_comments.xml", "//*", Core.Connectors.Xml.XmlNodeType.STRING, ExpectedResult=10)]
        [TestCase("sample4_comments.xml", "//*/@*", Core.Connectors.Xml.XmlNodeType.NUMERIC, ExpectedResult=2)]
        [TestCase("sample4_comments.xml", "//*/@*", Core.Connectors.Xml.XmlNodeType.BOOLEAN, ExpectedResult=3)]
        [TestCase("sample4_comments.xml", "//*/@*", Core.Connectors.Xml.XmlNodeType.STRING, ExpectedResult=1)]
        public int CountNodes(string file, string query, Core.Connectors.Xml.XmlNodeType type)
        {   
            //Note: Uses SelectNodes internally
            var xml = new AutoCheck.Core.Connectors.Xml(GetSampleFile(file));
            return xml.CountNodes(query, type);             
        }    

        [Test]
        [TestCase("sample4_comments.xml", "//*[name() = following-sibling::*/name()]", ExpectedResult=1)]
        [TestCase("sample6.xml", "./*/namespace::*[name()='']", ExpectedResult=1)]
        [TestCase("sample6.xml", "./*/namespace::*[name()!='']", ExpectedResult=3)]
        [TestCase("sample6.xml", ".//*[namespace-uri()=//*/namespace::*[name()='']]", ExpectedResult=1)]
        [TestCase("sample6.xml", "//*[namespace-uri()=//*/namespace::*[name()!=''][1]]", ExpectedResult=26)]
        [TestCase("sample6.xml", "//*[namespace-uri()=//*/namespace::*[name()!=''][2]]", ExpectedResult=26)]
        public int XPath2(string file, string query)
        {   
            //Note: Uses XPath2 external library because .NET one is not compatible with XPath 2.0
            var xml = new AutoCheck.Core.Connectors.Xml(GetSampleFile(file));
            return xml.CountNodes(query);
        }  

        [Test]
        [TestCase("sample5_dtd.xml", "sample5_dtd.xml", ExpectedResult=true)]
        [TestCase("sample5_xsd.xml", "sample5_xsd.xml", ExpectedResult=true)]
        [TestCase("sample5_dtd.xml", "sample5_xsd.xml", ExpectedResult=true)]
        [TestCase("sample5_xsd.xml", "sample5_dtd.xml", ExpectedResult=true)]
        [TestCase("sample5_dtd.xml", "sample5_none.xml", ExpectedResult=false)]
        [TestCase("sample5_xsd.xml", "sample5_none.xml", ExpectedResult=false)]
        public bool Equals(string leftPath, string rightPath)
        {               
            var left = new AutoCheck.Core.Connectors.Xml(GetSampleFile(leftPath));
            var right = new AutoCheck.Core.Connectors.Xml(GetSampleFile(rightPath));
            return left.Equals(right);
        }
    }
}