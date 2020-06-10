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
using System.Collections.Generic;
using System.Collections.Concurrent;
using NUnit.Framework;
using AutoCheck.Core;

namespace AutoCheck.Test.Checkers
{
    [Parallelizable(ParallelScope.All)]    
    public class RemoteShell : Core.Test
    {
        //TODO: Check the exact errors messages, otherwise cannot be assured its amount and content (do not check only amount, the exact message output is needed for debug) 
        
        private ConcurrentDictionary<string, AutoCheck.Checkers.RemoteShell> Pool = new ConcurrentDictionary<string, AutoCheck.Checkers.RemoteShell>();

        private const string _fake = "fake";       

        [SetUp]
        public void Setup() 
        {
            base.Setup("remoteShell");
            AutoCheck.Core.Output.Instance.Disable();

            //Create a new and unique host connection for the current context (same host for all tests)
            var conn = new AutoCheck.Checkers.RemoteShell(OS.GNU, "localhost", "usuario", "usuario");            
            
            //Storing the connector instance for the current context
            var added = false;
            do added = this.Pool.TryAdd(TestContext.CurrentContext.Test.ID, conn);             
            while(!added);
        }

        [TearDown]
        public void TearDown(){
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            conn.Dispose();
        }

        [Test]
        public void Constructor()
        {            
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Checkers.RemoteShell(OS.WIN, string.Empty, string.Empty, string.Empty));
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Checkers.RemoteShell(OS.WIN, _fake, string.Empty, string.Empty));
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Checkers.RemoteShell(OS.WIN, _fake, _fake, string.Empty));
            Assert.DoesNotThrow(() => new AutoCheck.Checkers.RemoteShell(OS.WIN, _fake, _fake, _fake));  
        }   

        [Test]
        public void CheckIfFolderExists()
        {                                
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];                                      
            var path = GetSamplesPath(conn);

            Assert.AreNotEqual(new List<string>(), conn.CheckIfFolderExists(path, "testSubFolder11", false));
            Assert.AreEqual(new List<string>(), conn.CheckIfFolderExists(path, "testSubFolder11", true));
            Assert.AreEqual(new List<string>(), conn.CheckIfFolderExists(path, "testFolder1", false));                
            Assert.AreEqual(new List<string>(), conn.CheckIfFolderExists(path, "testFolder1", true));
        } 

        [Test]
        public void CheckIfFileExists()
        {    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];                                      
            var path = GetSamplesPath(conn);                          

            Assert.AreNotEqual(new List<string>(), conn.CheckIfFileExists(path, "testFile11.txt", false));
            Assert.AreEqual(new List<string>(), conn.CheckIfFileExists(path, "testFile11.txt", true));
            Assert.AreNotEqual(new List<string>(), conn.CheckIfFileExists(path, "testFile11.txt", false));                
            Assert.AreEqual(new List<string>(), conn.CheckIfFileExists(path, "testFile11.txt", true));
        }

        [Test]
        public void CheckIfFoldersMatchesAmount()
        {    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];                                      
            var path = GetSamplesPath(conn);                                
            
            Assert.AreEqual(new List<string>(), conn.CheckIfFoldersMatchesAmount(path, 2, false));
            Assert.AreNotEqual(new List<string>(), conn.CheckIfFoldersMatchesAmount(path, 2, false, Operator.GREATER));
            Assert.AreEqual(new List<string>(), conn.CheckIfFoldersMatchesAmount(path, 2, false, Operator.GREATEREQUALS));
            Assert.AreNotEqual(new List<string>(), conn.CheckIfFoldersMatchesAmount(path, 2, false, Operator.LOWER));
            Assert.AreEqual(new List<string>(), conn.CheckIfFoldersMatchesAmount(path, 2, false, Operator.LOWEREQUALS));
            
            Assert.AreEqual(new List<string>(), conn.CheckIfFoldersMatchesAmount(path, 6, true));
            Assert.AreNotEqual(new List<string>(), conn.CheckIfFoldersMatchesAmount(path, 6, true, Operator.GREATER));
            Assert.AreEqual(new List<string>(), conn.CheckIfFoldersMatchesAmount(path, 6, true, Operator.GREATEREQUALS));
            Assert.AreNotEqual(new List<string>(), conn.CheckIfFoldersMatchesAmount(path, 6, true, Operator.LOWER));
            Assert.AreEqual(new List<string>(), conn.CheckIfFoldersMatchesAmount(path, 6, true, Operator.LOWEREQUALS));
        } 

        [Test]
        public void CheckIfFilesMatchesAmount()
        {    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];                                      
            var path = GetSamplesPath(conn);                                 
               
            Assert.AreEqual(new List<string>(), conn.CheckIfFilesMatchesAmount(path, 0, false));
            Assert.AreNotEqual(new List<string>(), conn.CheckIfFilesMatchesAmount(path, 0, false, Operator.GREATER));
            Assert.AreEqual(new List<string>(), conn.CheckIfFilesMatchesAmount(path, 0, false, Operator.GREATEREQUALS));
            Assert.AreNotEqual(new List<string>(), conn.CheckIfFilesMatchesAmount(path, 0, false, Operator.LOWER));
            Assert.AreEqual(new List<string>(), conn.CheckIfFilesMatchesAmount(path, 0, false, Operator.LOWEREQUALS));
            
            Assert.AreEqual(new List<string>(), conn.CheckIfFilesMatchesAmount(path, 2, true));
            Assert.AreNotEqual(new List<string>(), conn.CheckIfFilesMatchesAmount(path, 2, true, Operator.GREATER));
            Assert.AreEqual(new List<string>(), conn.CheckIfFilesMatchesAmount(path, 2, true, Operator.GREATEREQUALS));
            Assert.AreNotEqual(new List<string>(), conn.CheckIfFilesMatchesAmount(path, 2, true, Operator.LOWER));
            Assert.AreEqual(new List<string>(), conn.CheckIfFilesMatchesAmount(path, 2, true, Operator.LOWEREQUALS));
        } 

        [Test]
        public void CheckIfCommandMatchesResult()
        {    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];                                      
            var path = GetSamplesPath(conn);  
            
            //TODO: on windows, test if the  wsl is installed because wsl -e will be used to test linux commands and windows ones in one step if don't, throw an exception                 
            Assert.AreEqual(new List<string>(), conn.CheckIfCommandMatchesResult(string.Format("ls '{0}'", path), "testFolder1\ntestFolder2\n"));
        }

        private string GetSamplesPath(AutoCheck.Checkers.RemoteShell conn){
            var path = base.SamplesPath;
            if(conn.Connector.CurrentOS == OS.WIN && ((AutoCheck.Connectors.RemoteShell)conn.Connector).RemoteOS != OS.WIN) 
                path = path.Replace("c:", "\\mnt\\c", true, null).Replace("\\", "/"); 

            return path;
        }
    }
}