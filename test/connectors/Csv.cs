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
using System.Linq;
using NUnit.Framework;
using OS = AutoCheck.Core.Utils.OS;

namespace AutoCheck.Test.Connectors
{
    [Parallelizable(ParallelScope.All)]    
    public class Csv : Test
    {      
        [Test]
        [TestCase("empty.csv", null, null)]
        [TestCase("correct1.csv", ';', '\'')]
        [TestCase("correct2.csv",  ',', '"')]
        public void Constructor_Local_DoesNotThrow(string file, char fieldDelimiter, char textDelimiter)
        {      
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Csv(GetSampleFile(file), fieldDelimiter, textDelimiter));
        }

        [Test]
        [TestCase("empty.csv", OS.GNU, "localhost", "autocheck", "autocheck")]
        public void Constructor_Remote_DoesNotThrow(string file, OS remoteOS, string host, string username, string password)
        {      
            //Note: the source code for local and remote mode are exactly the same, just need to test that the remote file is being downloaded from remote and parsed.
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Csv(remoteOS, host, username, password, LocalPathToRemote(GetSampleFile(file), username)));  
        }

        [Test]
        [TestCase("")]
        public void Constructor_Local_Throws_ArgumentNullException(string file)
        {      
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.Csv(file));
        }

        [Test]
        [TestCase("", OS.GNU, "localhost", "autocheck", "autocheck")]
        public void Constructor_Remote_Throws_ArgumentNullException(string file, OS remoteOS, string host, string username, string password)
        {      
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.Csv(remoteOS, host, username, password, file));
        }

        [Test]
        [TestCase(_FAKE)]        
        public void Constructor_Local_Throws_FileNotFoundException(string file)
        {      
            Assert.Throws<FileNotFoundException>(() => new AutoCheck.Core.Connectors.Csv(GetSampleFile(file)));
        }

        [Test]
        [TestCase(_FAKE, OS.GNU, "localhost", "autocheck", "autocheck")]        
        public void Constructor_Remote_Throws_FileNotFoundException(string file, OS remoteOS, string host, string username, string password)
        {      
             Assert.Throws<FileNotFoundException>(() => new AutoCheck.Core.Connectors.Csv(remoteOS, host, username, password, file));
        }

        [Test]        
        [TestCase("correct2.csv", 1, ExpectedResult = new string[]{"119736", "FL", "CLAY COUNTY", "498960", "498960" ,"498960" ,"498960" ,"498960" ,"792148.9" ,"0" ,"9979.2" ,"0" ,"0" ,"30.102261" ,"-81.711777" ,"Residential" ,"Masonry" , "1"})]
        [TestCase("correct2.csv", 8, ExpectedResult = new string[]{"223488", "FL", "CLAY COUNTY", "328500", "328500", "328500", "328500", "328500", "348374.25", "0", "16425", "0", "0", "30.102217", "-81.707146", "Residential", "Wood", "1"})]
        [TestCase("correct2.csv", 36634, ExpectedResult = new string[]{"398149", "FL", "PINELLAS COUNTY", "373488.3", "373488.3", "0", "0", "373488.3", "596003.67", "0", "0", "0", "0", "28.06444", "-82.77459", "Residential", "Masonry", "1"})]
        public string[] GetLine_DoesNotThrow(string file, int line)
        {
            using(var conn = new AutoCheck.Core.Connectors.Csv(GetSampleFile(file)))    
                return conn.CsvDoc.GetLine(line).Values.ToArray();
        }

        [Test]
        [TestCase("correct2.csv", -1)]
        [TestCase("correct2.csv", 36636)]
        public void GetLine_Throws_IndexOutOfRangeException(string file, int line)
        {
            using(var conn = new AutoCheck.Core.Connectors.Csv(GetSampleFile(file)))    
                Assert.Throws<IndexOutOfRangeException>(() => conn.CsvDoc.GetLine(line));
        } 
    }
}