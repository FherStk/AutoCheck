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
        /*        
            Prerequisites for Windows 10 hosts:
                - Open WSL terminal:
                    - Install openssh with "sudo apt install openssh-server":
                        - sudo nano /etc/ssh/sshd_config
                        - PasswordAuthentication yes 

                     - Allow sshd service start with no root password with:
                        - sudo visudo
                        - Add this line: YOUR_USER_HERE ALL=(ALL) NOPASSWD: /usr/sbin/service ssh start

                - For WSL v1 only:
                    - Create a .bat file within startup (C:\Users\YOUR_USER_HERE\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup):
                        - Add this line: wsl sudo service ssh start
            
                - For WSL v2 only (https://github.com/microsoft/WSL/issues/4150#issuecomment-504209723):                    
                    - Copy the files within /samples/wsl2 into a local folder (by default: "C:\WSL2 Setup"):                    
                        - Edit the wsl2_setup.bat file to set the correct files path (by default: "C:\WSL2 Setup")                        
                        
                    - Create a shortcut to the wsl2_setup.bat file within startup (C:\Users\YOUR_USER_HERE\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup):
                        - Setup run as administrator and minimized
        */
        
        //TODO: Check the exact errors messages, otherwise cannot be assured its amount and content (do not check only amount, the exact message output is needed for debug) 
        
        private ConcurrentDictionary<string, AutoCheck.Checkers.RemoteShell> Pool = new ConcurrentDictionary<string, AutoCheck.Checkers.RemoteShell>();


        [SetUp]
        public void Setup() 
        {                        
            //Create a new and unique host connection for the current context (same host for all tests)            
            var conn = new AutoCheck.Checkers.RemoteShell(OS.GNU, "127.0.0.1", "usuario", "usuario");   //"localhost" fails with WSL2, can be used with WSL1 or UNIX hosts.
            
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
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Checkers.RemoteShell(OS.WIN, _FAKE, string.Empty, string.Empty));
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Checkers.RemoteShell(OS.WIN, _FAKE, _FAKE, string.Empty));
            Assert.DoesNotThrow(() => new AutoCheck.Checkers.RemoteShell(OS.WIN, _FAKE, _FAKE, _FAKE));  
        }   

        [Test]
        public void CheckIfFolderExists()
        {                                
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];                                      
            var path = GetSamplePath(conn);

            Assert.AreNotEqual(new List<string>(), conn.CheckIfFolderExists(path, "testSubFolder11", false));
            Assert.AreEqual(new List<string>(), conn.CheckIfFolderExists(path, "testSubFolder11", true));
            Assert.AreEqual(new List<string>(), conn.CheckIfFolderExists(path, "testFolder1", false));                
            Assert.AreEqual(new List<string>(), conn.CheckIfFolderExists(path, "testFolder1", true));
        } 

        [Test]
        public void CheckIfFileExists()
        {    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];                                      
            var path = GetSamplePath(conn);                          

            Assert.AreNotEqual(new List<string>(), conn.CheckIfFileExists(path, "testFile11.txt", false));
            Assert.AreEqual(new List<string>(), conn.CheckIfFileExists(path, "testFile11.txt", true));
            Assert.AreNotEqual(new List<string>(), conn.CheckIfFileExists(path, "testFile11.txt", false));                
            Assert.AreEqual(new List<string>(), conn.CheckIfFileExists(path, "testFile11.txt", true));
        }

        [Test]
        public void CheckIfFoldersMatchesAmount()
        {    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];                                      
            var path = GetSamplePath(conn);                                
            
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
            var path = GetSamplePath(conn);                                 
               
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
            var path = GetSamplePath(conn);  
            
            //TODO: on windows, test if the  wsl is installed because wsl -e will be used to test linux commands and windows ones in one step if don't, throw an exception                 
            Assert.AreEqual(new List<string>(), conn.CheckIfCommandMatchesResult(string.Format("ls '{0}'", path), "testFolder1\ntestFolder2\n"));
        }

        private string GetSamplePath(AutoCheck.Checkers.RemoteShell conn){
            var path = this.SamplesScriptFolder;
            if(conn.Connector.CurrentOS == OS.WIN && ((AutoCheck.Connectors.RemoteShell)conn.Connector).RemoteOS != OS.WIN) 
                path = path.Replace("c:", "\\mnt\\c", true, null).Replace("\\", "/"); 

            return path;
        }
    }
}