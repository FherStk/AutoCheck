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
using System.Collections.Concurrent;
using NUnit.Framework;
using AutoCheck.Core.Exceptions;
using OS = AutoCheck.Core.Utils.OS;

namespace AutoCheck.Test.Connectors
{
    [Parallelizable(ParallelScope.All)]    
    public class RemoteShell : Test
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
                    - Create a .bat file within startup (%AppData%\Microsoft\Windows\Start Menu\Programs\Startup):
                        - Add this line: wsl sudo service ssh start
            
                - For WSL v2 only (https://github.com/microsoft/WSL/issues/4150#issuecomment-504209723):                    
                    - Copy the files within /samples/wsl2 into a local folder (by default: "C:\WSL2 Setup"):                    
                        - Edit the wsl2_setup.bat file to set the correct files path (by default: "C:\WSL2 Setup")                        
                        
                    - Create a scheduled task with Win+R and "taskschd.msc"
                        - Setup it to run when the user loggins with admin privileges and hidden.
        */

        private ConcurrentDictionary<string, AutoCheck.Core.Connectors.RemoteShell> Conn = new ConcurrentDictionary<string, AutoCheck.Core.Connectors.RemoteShell>();

        [SetUp]
        public void Setup() 
        {
            //Create a new and unique host connection for the current context (same host for all tests)
            var conn = new AutoCheck.Core.Connectors.RemoteShell(OS.GNU, "localhost", "usuario", "usuario"); 
            
            //Storing the connector instance for the current context
            var added = false;
            do added = this.Conn.TryAdd(TestContext.CurrentContext.Test.ID, conn);             
            while(!added);  
        }

        [TearDown]
        public void TearDown(){
            var conn = this.Conn[TestContext.CurrentContext.Test.ID];
            conn.Dispose();
        }

        [Test]
        public void Constructor()
        {                      
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.RemoteShell(OS.WIN, string.Empty, string.Empty, string.Empty));
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.RemoteShell(OS.WIN, _FAKE, string.Empty, string.Empty));
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.RemoteShell(OS.WIN, _FAKE, _FAKE, string.Empty));
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.RemoteShell(OS.WIN, _FAKE, _FAKE, _FAKE));            
        }

        [Test]
        public void TestConnection()
        {                                  
            using(var conn = new AutoCheck.Core.Connectors.RemoteShell(OS.WIN, _FAKE, _FAKE, _FAKE))
                Assert.Throws<ConnectionInvalidException>(() => conn.TestConnection());

            Assert.DoesNotThrow(() => this.Conn[TestContext.CurrentContext.Test.ID].TestConnection());
        }

        [Test]
        public void GetFolder()
        {     
            var conn = this.Conn[TestContext.CurrentContext.Test.ID];
            var path = LocalToRemotePath(this.SamplesScriptFolder);

            Assert.IsNotNull(conn.GetFolder(path, "testFolder1", false));
            Assert.IsNotNull(conn.GetFolder(path, "testFolder2", false));
            Assert.IsNotNull(conn.GetFolder(path, "testFolder1", true));
            Assert.IsNotNull(conn.GetFolder(path, "testFolder2", true));
            Assert.IsNull(conn.GetFolder(path, "testFolder21", false));
            Assert.IsNotNull(conn.GetFolder(path, "testFolder21", true));
        }

        [Test]
        public void GetFile()
        {            
            var conn = this.Conn[TestContext.CurrentContext.Test.ID];
            var path = LocalToRemotePath(this.SamplesScriptFolder);

            Assert.IsNull(conn.GetFile(path, "testFile11.txt", false));
            Assert.IsNotNull(conn.GetFile(path, "testFile11.txt", true));
            Assert.IsNull(conn.GetFile(path, "testFile211.txt", false));                
            Assert.IsNotNull(conn.GetFile(path, "testFile211.txt", true));
        }
        
        [Test]
        public void CountFolders()
        {        
            var conn = this.Conn[TestContext.CurrentContext.Test.ID];
            var path = LocalToRemotePath(this.SamplesScriptFolder);

            Assert.AreEqual(2, conn.CountFolders(path, false));
            Assert.AreEqual(3, conn.CountFolders(path, true));
        }

        [Test]
        public void CountFiles()
        {               
            var conn = this.Conn[TestContext.CurrentContext.Test.ID];
            var path = LocalToRemotePath(this.SamplesScriptFolder);

            Assert.AreEqual(0, conn.CountFiles(path, false));
            Assert.AreEqual(3, conn.CountFiles(path, true));
        }

        [Test]
        public void RunCommand()
        {                        
            var conn = this.Conn[TestContext.CurrentContext.Test.ID];            
            var result = conn.RunCommand("ls");
            Assert.AreEqual(0, result.code);
            Assert.IsNotNull(result.response);

            result = conn.RunCommand("fake");
            Assert.AreNotEqual(0, result.code);
            Assert.IsNotNull(result.response);              
        }

        private string LocalToRemotePath(string local){            
            //TODO: change the drive letter automatically 
            if(Core.Utils.CurrentOS == OS.WIN)local = local.Replace("c:\\", "/mnt/c/", StringComparison.InvariantCultureIgnoreCase).Replace("\\", "/");    
            return local;
        }
    }
}