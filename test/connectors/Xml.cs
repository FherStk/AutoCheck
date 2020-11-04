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
using System.Linq;
using NUnit.Framework;
using AutoCheck.Core.Exceptions;

namespace AutoCheck.Test.Connectors
{
    [Parallelizable(ParallelScope.All)]    
    public class Xml : Test
    {      
        [Test]
        public void Constructor()
        {            
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.Xml("", "someFile.ext"));
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.Xml("somePath", ""));
            Assert.Throws<DirectoryNotFoundException>(() => new AutoCheck.Core.Connectors.Xml("somePath", "someFile.ext"));
            Assert.Throws<FileNotFoundException>(() => new AutoCheck.Core.Connectors.Xml(this.SamplesScriptFolder, "someFile.ext"));            
        }

        [Test]
        public void Validation_XML()
        {   
            //Trying to validate against unnexisting DTD/XSD
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Xml(this.SamplesScriptFolder, "sample1_simple_ok.xml"));
            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.Connectors.Xml(this.SamplesScriptFolder, "sample1_simple_ok.xml", System.Xml.ValidationType.DTD));
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Xml(this.SamplesScriptFolder, "sample1_simple_ok.xml", System.Xml.ValidationType.Schema));
            
            //Testing XML syntax
            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.Connectors.Xml(this.SamplesScriptFolder, "sample1_simple_ko.xml"));
            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.Connectors.Xml(this.SamplesScriptFolder, "sample1_simple_ko.xml", System.Xml.ValidationType.DTD));
            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.Connectors.Xml(this.SamplesScriptFolder, "sample1_simple_ko.xml", System.Xml.ValidationType.Schema));
        }

        [Test]
        public void Validation_DTD()
        {   
            //Trying to validate against DTD/XSD
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Xml(this.SamplesScriptFolder, "sample2_dtd_ok.xml"));
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Xml(this.SamplesScriptFolder, "sample2_dtd_ok.xml", System.Xml.ValidationType.DTD));
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Xml(this.SamplesScriptFolder, "sample2_dtd_ok.xml", System.Xml.ValidationType.Schema));

            //Trying to validate against DTD
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Xml(this.SamplesScriptFolder, "sample2_dtd_ko1.xml"));
            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.Connectors.Xml(this.SamplesScriptFolder, "sample2_dtd_ko1.xml", System.Xml.ValidationType.DTD));

            //Testing DTD syntax
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Xml(this.SamplesScriptFolder, "sample2_dtd_ko2.xml"));
            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.Connectors.Xml(this.SamplesScriptFolder, "sample2_dtd_ko2.xml", System.Xml.ValidationType.DTD));
        } 

        [Test]
        public void Validation_XSD()
        {   
            //Trying to validate against DTD/XSD
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Xml(this.SamplesScriptFolder, "sample3_xsd_ok.xml"));
            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.Connectors.Xml(this.SamplesScriptFolder, "sample3_xsd_ok.xml", System.Xml.ValidationType.DTD));
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Xml(this.SamplesScriptFolder, "sample3_xsd_ok.xml", System.Xml.ValidationType.Schema));

            //Trying to validate against XSD
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Xml(this.SamplesScriptFolder, "sample3_xsd_ko1.xml"));
            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.Connectors.Xml(this.SamplesScriptFolder, "sample3_xsd_ko1.xml", System.Xml.ValidationType.Schema));

            //Testing XSD syntax
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Xml(this.SamplesScriptFolder, "sample3_xsd_ko2.xml"));
            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.Connectors.Xml(this.SamplesScriptFolder, "sample3_xsd_ko2.xml", System.Xml.ValidationType.DTD));
        }        
    }
}