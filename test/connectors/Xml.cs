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
    public class Xml : Test
    {      
        private const OS _remoteOS = OS.GNU;
        private const string _host = "localhost";
        private const string _username = "usuario";
        private const string _password = "usuario";
        
        [Test]
        public void Constructor()
        {  
            //Local          
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.Xml(""));
            Assert.Throws<FileNotFoundException>(() => new AutoCheck.Core.Connectors.Xml(Path.Combine(this.SamplesScriptFolder, "someFile.ext")));            

            //Remote
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.Xml(_remoteOS, _host, _username, _password, string.Empty));
            Assert.Throws<FileNotFoundException>(() => new AutoCheck.Core.Connectors.Xml(_remoteOS, _host, _username, _password, _FAKE));

            //Note: the source code for local and remote mode are exactly the same, just need to test that the remote file is being downloaded from remote and parsed.
            var file = LocalPathToWsl(Path.Combine(this.SamplesScriptFolder, "sample1_simple_ok.xml"));
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Xml(_remoteOS, _host, _username, _password, file));
        }

        [Test]
        public void Validation_XML()
        {   
            //Trying to validate against unnexisting DTD/XSD
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Xml(Path.Combine(this.SamplesScriptFolder, "sample1_simple_ok.xml")));
            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.Connectors.Xml(Path.Combine(this.SamplesScriptFolder, "sample1_simple_ok.xml"), System.Xml.ValidationType.DTD));
            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.Connectors.Xml(Path.Combine(this.SamplesScriptFolder, "sample1_simple_ok.xml"), System.Xml.ValidationType.Schema));
            
            //Testing XML syntax
            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.Connectors.Xml(Path.Combine(this.SamplesScriptFolder, "sample1_simple_ko.xml")));
            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.Connectors.Xml(Path.Combine(this.SamplesScriptFolder, "sample1_simple_ko.xml"), System.Xml.ValidationType.DTD));
            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.Connectors.Xml(Path.Combine(this.SamplesScriptFolder, "sample1_simple_ko.xml"), System.Xml.ValidationType.Schema));
        }

        [Test]
        public void Validation_DTD()
        {   
            //Trying to validate against DTD/XSD
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Xml(Path.Combine(this.SamplesScriptFolder, "sample2_dtd_ok.xml")));
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Xml(Path.Combine(this.SamplesScriptFolder, "sample2_dtd_ok.xml"), System.Xml.ValidationType.DTD));
            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.Connectors.Xml(Path.Combine(this.SamplesScriptFolder, "sample2_dtd_ok.xml"), System.Xml.ValidationType.Schema));

            //Trying to validate against DTD
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Xml(Path.Combine(this.SamplesScriptFolder, "sample2_dtd_ko1.xml")));
            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.Connectors.Xml(Path.Combine(this.SamplesScriptFolder, "sample2_dtd_ko1.xml"), System.Xml.ValidationType.DTD));

            //Testing DTD syntax
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Xml(Path.Combine(this.SamplesScriptFolder, "sample2_dtd_ko2.xml")));
            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.Connectors.Xml(Path.Combine(this.SamplesScriptFolder, "sample2_dtd_ko2.xml"), System.Xml.ValidationType.DTD));
        } 

        [Test]
        public void Validation_XSD()
        {   
            //Trying to validate against DTD/XSD
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Xml(Path.Combine(this.SamplesScriptFolder, "sample3_xsd_ok.xml")));            
            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.Connectors.Xml(Path.Combine(this.SamplesScriptFolder, "sample3_xsd_ok.xml"), System.Xml.ValidationType.DTD));
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Xml(Path.Combine(this.SamplesScriptFolder, "sample3_xsd_ok.xml"), System.Xml.ValidationType.Schema));

            //Trying to validate against XSD
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Xml(Path.Combine(this.SamplesScriptFolder, "sample3_xsd_ko1.xml")));
            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.Connectors.Xml(Path.Combine(this.SamplesScriptFolder, "sample3_xsd_ko1.xml"), System.Xml.ValidationType.Schema));

            //Testing XSD syntax
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Xml(Path.Combine(this.SamplesScriptFolder, "sample3_xsd_ko2.xml")));
            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.Connectors.Xml(Path.Combine(this.SamplesScriptFolder, "sample3_xsd_ko2.xml"), System.Xml.ValidationType.Schema));
        }  

        [Test]
        public void Comments()
        {   
            var xml = new AutoCheck.Core.Connectors.Xml(Path.Combine(this.SamplesScriptFolder, "sample4_comments.xml"));
            Assert.AreEqual(3, xml.Comments.Length);        
        }       

        [Test]
        public void CountNodes()
        {   
            //Note: Uses SelectNodes internally
            var xml = new AutoCheck.Core.Connectors.Xml(Path.Combine(this.SamplesScriptFolder, "sample4_comments.xml"));            
            Assert.AreEqual(26, xml.CountNodes("//*")); 
            Assert.AreEqual(2, xml.CountNodes("//become")); 
            Assert.AreEqual(2, xml.CountNodes("/root/underline/harder/become")); 
            Assert.AreEqual(6, xml.CountNodes("//*/@*")); 
            Assert.AreEqual(1, xml.CountNodes("//*/@units")); 

            //Node types
            Assert.AreEqual(10, xml.CountNodes("//*", Core.Connectors.Xml.XmlNodeType.NUMERIC)); 
            Assert.AreEqual(2, xml.CountNodes("//*", Core.Connectors.Xml.XmlNodeType.BOOLEAN)); 
            Assert.AreEqual(10, xml.CountNodes("//*", Core.Connectors.Xml.XmlNodeType.STRING)); 

            //Attribute types
            Assert.AreEqual(2, xml.CountNodes("//*/@*", Core.Connectors.Xml.XmlNodeType.NUMERIC)); 
            Assert.AreEqual(3, xml.CountNodes("//*/@*", Core.Connectors.Xml.XmlNodeType.BOOLEAN)); 
            Assert.AreEqual(1, xml.CountNodes("//*/@*", Core.Connectors.Xml.XmlNodeType.STRING));     
        }    

        [Test]
        public void XPath2()
        {   
            //Note: Uses XPath2 external library because .NET one is not compatible with XPath 2.0
            var xml = new AutoCheck.Core.Connectors.Xml(Path.Combine(this.SamplesScriptFolder, "sample4_comments.xml"));                       
            Assert.AreEqual(1, xml.CountNodes("//*[name() = following-sibling::*/name()]"));

            //Namespaces lookup
            xml = new AutoCheck.Core.Connectors.Xml(Path.Combine(this.SamplesScriptFolder, "sample6.xml"));
            Assert.AreEqual(1, xml.CountNodes("./*/namespace::*[name()='']"));
            Assert.AreEqual(3, xml.CountNodes("/*/namespace::*[name()!='']"));            
            Assert.AreEqual(1, xml.CountNodes("//*[namespace-uri()=//*/namespace::*[name()='']]"));
            Assert.AreEqual(26, xml.CountNodes("//*[namespace-uri()=//*/namespace::*[name()!=''][1]]"));
            Assert.AreEqual(26, xml.CountNodes("//*[namespace-uri()=//*/namespace::*[name()!=''][2]]"));
        }  

        [Test]
        public void Equals()
        {               
            var dtd = new AutoCheck.Core.Connectors.Xml(Path.Combine(this.SamplesScriptFolder, "sample5_dtd.xml"));
            var xsd = new AutoCheck.Core.Connectors.Xml(Path.Combine(this.SamplesScriptFolder, "sample5_xsd.xml"));
            Assert.IsTrue(dtd.Equals(dtd));
            Assert.IsTrue(xsd.Equals(xsd));
            Assert.IsTrue(dtd.Equals(xsd));
            Assert.IsTrue(xsd.Equals(dtd));

            var none = new AutoCheck.Core.Connectors.Xml(Path.Combine(this.SamplesScriptFolder, "sample5_none.xml"));
            Assert.IsFalse(dtd.Equals(none));
            Assert.IsFalse(xsd.Equals(none));
        }
    }
}