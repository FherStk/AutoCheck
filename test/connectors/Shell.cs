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
using System.Collections.Concurrent;
using NUnit.Framework;
using AutoCheck.Core.Exceptions;
using OS = AutoCheck.Core.Utils.OS;

namespace AutoCheck.Test.Connectors
{
    [Parallelizable(ParallelScope.All)]    
    public class Shell : Test
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
        private ConcurrentDictionary<string, AutoCheck.Core.Connectors.Shell> Conn = new ConcurrentDictionary<string, AutoCheck.Core.Connectors.Shell>();

        [SetUp]
        public void Setup() 
        {
            //Create a new and unique host connection for the current context (same host for all tests)
            var conn = new AutoCheck.Core.Connectors.Shell(OS.GNU, "localhost", "usuario", "usuario"); 
            
            //Storing the connector instance for the current context
            var added = false;
            do added = this.Conn.TryAdd(TestContext.CurrentContext.Test.ID, conn);             
            while(!added);  
        }

        protected override void CleanUp(){
            var path = Path.Combine("temp", "shell");
            if(Directory.Exists(path)) Directory.Delete(path, true);
        }

        [TearDown]
        public void TearDown(){
            var conn = this.Conn[TestContext.CurrentContext.Test.ID];
            conn.Dispose();
        }

        [Test]
        public void Constructor()
        {     
            //Local                 
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Shell());

            //Remote
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.Shell(OS.WIN, string.Empty, string.Empty, string.Empty));
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.Shell(OS.WIN, _FAKE, string.Empty, string.Empty));
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.Shell(OS.WIN, _FAKE, _FAKE, string.Empty));
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Shell(OS.WIN, _FAKE, _FAKE, _FAKE)); 
        }

        [Test]
        public void TestConnection()
        {     
            //Remote                             
            using(var remote = new AutoCheck.Core.Connectors.Shell(OS.WIN, _FAKE, _FAKE, _FAKE))
                Assert.Throws<ConnectionInvalidException>(() => remote.TestConnection());

            Assert.DoesNotThrow(() => this.Conn[TestContext.CurrentContext.Test.ID].TestConnection());
        }

        [Test]
        public void GetFolder()
        {         
            //Local   
            using(var local = new AutoCheck.Core.Connectors.Shell()){
                Assert.IsNotNull(local.GetFolder(this.SamplesScriptFolder, "testFolder1", false));
                Assert.IsNotNull(local.GetFolder(this.SamplesScriptFolder, "testFolder2", false));
                Assert.IsNotNull(local.GetFolder(this.SamplesScriptFolder, "testFolder1", true));
                Assert.IsNotNull(local.GetFolder(this.SamplesScriptFolder, "testFolder2", true));
                Assert.IsNull(local.GetFolder(this.SamplesScriptFolder, "testFolder21", false));
                Assert.IsNotNull(local.GetFolder(this.SamplesScriptFolder, "testFolder21", true));
            }      

            //Remote
            var remote = this.Conn[TestContext.CurrentContext.Test.ID];
            var path = LocalPathToWsl(this.SamplesScriptFolder);

            Assert.IsNotNull(remote.GetFolder(path, "testFolder1", false));
            Assert.IsNotNull(remote.GetFolder(path, "testFolder2", false));
            Assert.IsNotNull(remote.GetFolder(path, "testFolder1", true));
            Assert.IsNotNull(remote.GetFolder(path, "testFolder2", true));
            Assert.IsNull(remote.GetFolder(path, "testFolder21", false));
            Assert.IsNotNull(remote.GetFolder(path, "testFolder21", true));          
        }

        [Test]
        public void GetFile()
        {            
            //Local
            using(var local = new AutoCheck.Core.Connectors.Shell()){                
                Assert.IsNull(local.GetFile(this.SamplesScriptFolder, "testFile11.txt", false));
                Assert.IsNotNull(local.GetFile(this.SamplesScriptFolder, "testFile11.txt", true));
                Assert.IsNull(local.GetFile(this.SamplesScriptFolder, "testFile211.txt", false));                
                Assert.IsNotNull(local.GetFile(this.SamplesScriptFolder, "testFile211.txt", true));
            }      

            //Remote
            var remote = this.Conn[TestContext.CurrentContext.Test.ID];
            var path = LocalPathToWsl(this.SamplesScriptFolder);

            Assert.IsNull(remote.GetFile(path, "testFile11.txt", false));
            Assert.IsNotNull(remote.GetFile(path, "testFile11.txt", true));
            Assert.IsNull(remote.GetFile(path, "testFile211.txt", false));                
            Assert.IsNotNull(remote.GetFile(path, "testFile211.txt", true));

            Assert.IsNotNull(remote.GetFile(path, "*.txt", true));
            Assert.IsNull(remote.GetFile(path, "*.txt", false));
            Assert.IsNull(remote.GetFile(path, "*.xml", true));
            Assert.IsNull(remote.GetFile(path, "*.xml", false));          
        }

        [Test]
        public void GetFolders()
        {          
            //Local  
            using(var local = new AutoCheck.Core.Connectors.Shell()){
                CollectionAssert.AreEqual(new string[]{"testFolder1", "testFolder2"}, local.GetFolders(this.SamplesScriptFolder, false).ToList().Select(x => Path.GetFileName(x)).ToArray());
                CollectionAssert.AreEqual(new string[]{"testFolder1", "testFolder2", "testFolder21"}, local.GetFolders(this.SamplesScriptFolder, true).ToList().Select(x => Path.GetFileName(x)).ToArray());
            }              

            //remote
            var remote = this.Conn[TestContext.CurrentContext.Test.ID];
            var path = LocalPathToWsl(this.SamplesScriptFolder);

            CollectionAssert.AreEqual(new string[]{"testFolder1", "testFolder2"}, remote.GetFolders(path, false).ToList().Select(x => Path.GetFileName(x)).ToArray());
            CollectionAssert.AreEqual(new string[]{"testFolder1", "testFolder2", "testFolder21"}, remote.GetFolders(path, true).ToList().Select(x => Path.GetFileName(x)).ToArray());  
        }

        [Test]
        public void GetFiles()
        {     
            //Local       
            using(var local = new AutoCheck.Core.Connectors.Shell()){
                CollectionAssert.AreEqual(new string[]{}, local.GetFiles(this.SamplesScriptFolder, false).ToList().Select(x => Path.GetFileName(x)).ToArray());
                CollectionAssert.AreEqual(new string[]{"testFile11.txt", "testFile21.txt", "testFile211.txt"}, local.GetFiles(this.SamplesScriptFolder, true).ToList().Select(x => Path.GetFileName(x)).ToArray());
            }              

            //Remote
            var remote = this.Conn[TestContext.CurrentContext.Test.ID];
            var path = LocalPathToWsl(this.SamplesScriptFolder);

            CollectionAssert.AreEqual(new string[]{}, remote.GetFiles(path, false).ToList().Select(x => Path.GetFileName(x)).ToArray());
            CollectionAssert.AreEqual(new string[]{"testFile11.txt", "testFile21.txt", "testFile211.txt"}, remote.GetFiles(path, true).ToList().Select(x => Path.GetFileName(x)).ToArray());  
        }
        
        [Test]
        public void CountFolders()
        {            
            //Local
            using(var local = new AutoCheck.Core.Connectors.Shell()){
                Assert.AreEqual(2, local.CountFolders(this.SamplesScriptFolder, false));
                Assert.AreEqual(3, local.CountFolders(this.SamplesScriptFolder, true));
            }      

            //Remote
            var remote = this.Conn[TestContext.CurrentContext.Test.ID];
            var path = LocalPathToWsl(this.SamplesScriptFolder);

            Assert.AreEqual(2, remote.CountFolders(path, false));
            Assert.AreEqual(3, remote.CountFolders(path, true));          
        }

        [Test]
        public void CountFiles()
        {            
            //Local
            using(var local = new AutoCheck.Core.Connectors.Shell()){
                Assert.AreEqual(0, local.CountFiles(this.SamplesScriptFolder, false));
                Assert.AreEqual(3, local.CountFiles(this.SamplesScriptFolder, true));
            }              

            //Remote
            var remote = this.Conn[TestContext.CurrentContext.Test.ID];
            var path = LocalPathToWsl(this.SamplesScriptFolder);

            Assert.AreEqual(0, remote.CountFiles(path, false));
            Assert.AreEqual(3, remote.CountFiles(path, true));  
        }

        [Test]
        public void RunCommand()
        {           
            //Local             
            using(var local = new AutoCheck.Core.Connectors.Shell()){                
                string command = "ls";
                if(Core.Utils.CurrentOS == OS.WIN) 
                    command = string.Format("wsl {0}", command);
                
                //TODO: on windows, test if the  wsl is installed because wsl -e will be used to test linux commands and windows ones in one step if don't, throw an exception

                var lres = local.RunCommand(command);
                Assert.AreEqual(0, lres.code);
                Assert.IsNotNull(lres.response);

                lres = local.RunCommand("fake");
                Assert.AreNotEqual(0, lres.code);
                Assert.IsNotNull(lres.response);
            }  

            //Remote
            var remote = this.Conn[TestContext.CurrentContext.Test.ID];            
            var rres = remote.RunCommand("ls");
            Assert.AreEqual(0, rres.code);
            Assert.IsNotNull(rres.response);

            rres = remote.RunCommand("fake");
            Assert.AreNotEqual(0, rres.code);
            Assert.IsNotNull(rres.response);                  
        }

        [Test]
        public void DownloadFile()
        {            
            //Local
            var temp = Path.Combine("temp", "shell", "upload_file");
            using(var local = new AutoCheck.Core.Connectors.Shell()){                                                
                Assert.IsTrue(File.Exists(Path.Combine(this.SamplesScriptFolder, "testFolder1", "testFile11.txt")));
                Assert.IsFalse(File.Exists(Path.Combine(this.SamplesScriptFolder, temp, "testFile11.txt")));
            }      

            //Remote
            var remote = this.Conn[TestContext.CurrentContext.Test.ID];
            var path = LocalPathToWsl(Path.Combine(this.SamplesScriptFolder, "testFolder1"));

            var file = remote.DownloadFile(Path.Combine(path, "testFile11.txt"), Path.Combine(this.SamplesScriptFolder, temp));
            Assert.IsTrue(File.Exists(file));
            Assert.AreEqual(File.ReadAllText(Path.Combine(this.SamplesScriptFolder, "testFolder1", "testFile11.txt")), File.ReadAllText(file));
        }

        [Test]
        public void DownloadFolder()
        {            
            //Local
            var temp = Path.Combine("temp", "shell", "upload_folder");
            using(var local = new AutoCheck.Core.Connectors.Shell()){                                                
                Assert.IsTrue(Directory.Exists(this.SamplesScriptFolder));
                Assert.IsFalse(Directory.Exists(temp));
            }      

            //Remote
            var remote = this.Conn[TestContext.CurrentContext.Test.ID];
            var path = LocalPathToWsl(this.SamplesScriptFolder);

            var dest = remote.DownloadFolder(path, temp, true);
            Assert.IsTrue(Directory.Exists(dest));
            Assert.AreEqual(3, Directory.GetDirectories(dest, "*", SearchOption.AllDirectories).Length);
            Assert.AreEqual(3, Directory.GetFiles(dest, "*", SearchOption.AllDirectories).Length);
        }
    }
}