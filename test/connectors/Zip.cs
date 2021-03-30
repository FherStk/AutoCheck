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
    public class Zip : Test
    {    
        [Test]
        [TestCase("")]
        public void Constructor_Local_Throws_ArgumentNullException(string file)
        {      
             Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.Zip(file));
        }

        [Test]
        [TestCase("someFile.ext")]
        public void Constructor_Local_Throws_FileNotFoundException(string file)
        {      
            Assert.Throws<FileNotFoundException>(() => new AutoCheck.Core.Connectors.Zip(GetSampleFile(file)));
        }       
   
        [Test]
        [TestCase("", OS.GNU, "localhost", "usuario", "usuario")]
        public void Constructor_Remote_Throws_ArgumentNullException(string file, OS remoteOS, string host, string username, string password)
        {     
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.Zip(remoteOS, host, username, password, file));
        }

        [Test]
        [TestCase(_FAKE, OS.GNU, "localhost", "usuario", "usuario")]
        public void Constructor_Remote_Throws_FileNotFoundException(string file, OS remoteOS, string host, string username, string password)
        {     
            Assert.Throws<FileNotFoundException>(() => new AutoCheck.Core.Connectors.Zip(remoteOS, host, username, password, file));
        }

        [Test]
        [TestCase("nopass.zip", OS.GNU, "localhost", "usuario", "usuario")]
        public void Constructor_DoesNotThrow(string file, OS remoteOS, string host, string username, string password)
        {           
            //Note: the source code for local and remote mode are exactly the same, just need to test that the remote file is being downloaded from remote and parsed.
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Zip(remoteOS, host, username, password, LocalPathToWsl(GetSampleFile(file))));            
        }

        [Test]
        [TestCase("nopass.zip", null, "nopass.txt", "nopass")]
        [TestCase("withpass.zip", "1234", "withpass.txt", "withpass")]
        public void Extract_Local_NoRecursive_DoesNotThrow(string file, string password, string expectedFile, string expectedContent)
        {                        
            var local = new AutoCheck.Core.Connectors.Zip(GetSampleFile(file));
            Assert.IsFalse(Directory.Exists(TempScriptFolder));                     
            
            Directory.CreateDirectory(TempScriptFolder);            
            local.Extract(TempScriptFolder, password);

            Assert.IsTrue(File.Exists(Path.Combine(TempScriptFolder, expectedFile)));
            Assert.AreEqual(expectedContent, File.ReadAllText(Path.Combine(TempScriptFolder, expectedFile)));
        } 

        [Test]
        [TestCase("recursive.zip", 1, 4)]        
        public void Extract_Local_Recursive_DoesNotThrow(string file, int expectedFolders, int expectedFiles)
        {                        
            var local = new AutoCheck.Core.Connectors.Zip(GetSampleFile(file));         
            Assert.IsFalse(Directory.Exists(TempScriptFolder));      

            Directory.CreateDirectory(TempScriptFolder);     
            local.Extract(true, TempScriptFolder);
            
            Assert.AreEqual(expectedFolders, Directory.GetDirectories(TempScriptFolder, "*", SearchOption.AllDirectories).Length);
            Assert.AreEqual(expectedFiles, Directory.GetFiles(TempScriptFolder, "*", SearchOption.AllDirectories).Length);
        } 
    }
}