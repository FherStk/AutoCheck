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
using AutoCheck.Exceptions;

namespace AutoCheck.Test.Connectors
{
    [Parallelizable(ParallelScope.All)]    
    public class Csv : Core.Test
    {
        [SetUp]
        public void Setup() 
        {
            base.Setup("csv");
        }

        [Test]
        public void Constructor()
        {            
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Connectors.Csv("", "someFile.ext"));
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Connectors.Csv("somePath", ""));
            Assert.Throws<DirectoryNotFoundException>(() => new AutoCheck.Connectors.Csv("somePath", "someFile.ext"));
            Assert.Throws<FileNotFoundException>(() => new AutoCheck.Connectors.Csv(this.SamplesPath, "someFile.ext"));
            Assert.DoesNotThrow(() => new AutoCheck.Connectors.Csv(this.SamplesPath, "empty.csv"));
            Assert.DoesNotThrow(() => new AutoCheck.Connectors.Csv(this.SamplesPath, "correct1.csv", ';', '\''));
            Assert.DoesNotThrow(() => new AutoCheck.Connectors.Csv(this.SamplesPath, "correct2.csv", ',', '"'));
            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Connectors.Csv(this.SamplesPath, "incorrect.csv"));
        }

        [Test]
        public void GetLine()
        {                        
            using(var conn = new AutoCheck.Connectors.Csv(this.SamplesPath, "correct2.csv")){    
                //First            
                CollectionAssert.AreEqual(
                    new string[]{"119736", "FL", "CLAY COUNTY", "498960", "498960" ,"498960" ,"498960" ,"498960" ,"792148.9" ,"0" ,"9979.2" ,"0" ,"0" ,"30.102261" ,"-81.711777" ,"Residential" ,"Masonry" , "1"}, 
                    conn.CsvDoc.GetLine(1).Values.ToArray()
                );

                //Middle
                CollectionAssert.AreEqual(                    
                    new string[]{"223488", "FL", "CLAY COUNTY", "328500", "328500", "328500", "328500", "328500", "348374.25", "0", "16425", "0", "0", "30.102217", "-81.707146", "Residential", "Wood", "1"}, 
                    conn.CsvDoc.GetLine(8).Values.ToArray()
                );
                
                //Last
                CollectionAssert.AreEqual(                    
                    new string[]{"398149", "FL", "PINELLAS COUNTY", "373488.3", "373488.3", "0", "0", "373488.3", "596003.67", "0", "0", "0", "0", "28.06444", "-82.77459", "Residential", "Masonry", "1"}, 
                    conn.CsvDoc.GetLine(36634).Values.ToArray()
                );

                //Out of bounds (pre and post)
                Assert.Throws<IndexOutOfRangeException>(() => conn.CsvDoc.GetLine(-1));
                Assert.Throws<IndexOutOfRangeException>(() => conn.CsvDoc.GetLine(36636));                
            }                               
        }   
    }
}