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

        //TODO: Continue from here. 
        //  - Update the test to the parametrized version
        //  - Use the remote/local connectors, because Shell must be tested always using both versions
        //  - Check the correct creation of the temp files and folders (and its removal)
        private ConcurrentDictionary<string, AutoCheck.Core.Connectors.Shell> LocalConnectors = new ConcurrentDictionary<string, AutoCheck.Core.Connectors.Shell>();
        private ConcurrentDictionary<string, AutoCheck.Core.Connectors.Shell> RemoteConnectors = new ConcurrentDictionary<string, AutoCheck.Core.Connectors.Shell>();

        private AutoCheck.Core.Connectors.Shell LocalConnector {
            get{
                return LocalConnectors[TestContext.CurrentContext.Test.ID];
            }
        }

        private AutoCheck.Core.Connectors.Shell RemoteConnector {
            get{
                return RemoteConnectors[TestContext.CurrentContext.Test.ID];
            }
        }
        
        [SetUp]
        public void Setup() 
        {
            //Create a new and unique host connection for the current context (same host for all tests)            
            var added = false;
            do added = LocalConnectors.TryAdd(TestContext.CurrentContext.Test.ID, new AutoCheck.Core.Connectors.Shell());
            while(!added); 

            added = false;
            do added = RemoteConnectors.TryAdd(TestContext.CurrentContext.Test.ID, new AutoCheck.Core.Connectors.Shell(OS.GNU, "localhost", "autocheck", "autocheck"));
            while(!added);  
        }        

        [TearDown]
        public override void TearDown(){
            LocalConnector.Dispose();
            RemoteConnector.Dispose();
            base.TearDown();
        }

        protected override void CleanUp(){
            LocalConnectors.Clear();
            RemoteConnectors.Clear();
        }

        [Test]
        [TestCase(OS.WIN, "", "", "")]
        [TestCase(OS.WIN, _FAKE, "", "")]
        [TestCase(OS.WIN, _FAKE, _FAKE, "")]
        public void Constructor_Remote_Throws_ArgumentNullException(OS remoteOS, string host, string username, string password)
        {                 
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.Shell(remoteOS, host, username, password));            
        }

        [Test]
        [TestCase(OS.WIN, _FAKE, _FAKE, _FAKE)]
        public void Constructor_Remote_DoesNotThrow(OS remoteOS, string host, string username, string password)
        {                 
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Shell(remoteOS, host, username, password));  
        }

        [Test]
        public void Constructor_Local_DoesNotThrow()
        {     
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Shell());
        }

        [Test]
        [TestCase(OS.WIN, _FAKE, _FAKE, _FAKE)]
        public void TestConnection_Remote_Throws_ConnectionInvalidException(OS remoteOS, string host, string username, string password)
        {     
            Assert.Throws<ConnectionInvalidException>(() => new AutoCheck.Core.Connectors.Shell(remoteOS, host, username, password).TestConnection());
        }

        [Test]
        [TestCase(OS.GNU, "localhost", "autocheck", "autocheck")]
        public void TestConnection_Remote_DoesNotThrow(OS remoteOS, string host, string username, string password)
        {                
            Assert.DoesNotThrow(() => RemoteConnectors[TestContext.CurrentContext.Test.ID].TestConnection());
        }

        [Test]
        [TestCase(null, null)]
        [TestCase(null, _FAKE)]
        [TestCase(_FAKE, null)]        
        public void GetFolder_Local_Throws_ArgumentNullException(string path, string folder)
        {
            Assert.Throws<ArgumentNullException>(() => LocalConnector.GetFolder(path, folder));
        }

        [Test]
        [TestCase(_FAKE, false, ExpectedResult = null)]
        [TestCase(_FAKE, true, ExpectedResult = null)]
        [TestCase("testFolder1", false, ExpectedResult = "testFolder1")]
        [TestCase("testFolder2", false, ExpectedResult = "testFolder2")]
        [TestCase("testFolder1", true, ExpectedResult = "testFolder1")]
        [TestCase("testFolder2", true, ExpectedResult = "testFolder2")]
        [TestCase("testFolder21", false, ExpectedResult = null)]
        [TestCase("testFolder21", true, ExpectedResult = "testFolder21")]        
        public string GetFolder_Local_DoesNotThrow(string folder, bool recursive)
        {
            var found = LocalConnector.GetFolder(SamplesScriptFolder, folder, recursive);
            return (string.IsNullOrEmpty(found) ? found : Path.GetFileName(found));
        }

        [Test]
        [TestCase(_FAKE, false, ExpectedResult = null)]
        [TestCase(_FAKE, true, ExpectedResult = null)]
        [TestCase("testFolder1", false, ExpectedResult = "testFolder1")]
        [TestCase("testFolder2", false, ExpectedResult = "testFolder2")]
        [TestCase("testFolder1", true, ExpectedResult = "testFolder1")]
        [TestCase("testFolder2", true, ExpectedResult = "testFolder2")]
        [TestCase("testFolder21", false, ExpectedResult = null)]
        [TestCase("testFolder21", true, ExpectedResult = "testFolder21")]        
        public string GetFolder_Remote_DoesNotThrow(string folder, bool recursive)
        {
            var found = RemoteConnector.GetFolder(LocalPathToRemote(SamplesScriptFolder, "autocheck"), folder, recursive);
            return (string.IsNullOrEmpty(found) ? found : Path.GetFileName(found));
        } 

        [Test]
        [TestCase(null, null)]
        [TestCase(null, _FAKE)]
        [TestCase(_FAKE, null)]        
        public void GetFile_Local_Throws_ArgumentNullException(string path, string folder)
        {
            Assert.Throws<ArgumentNullException>(() => LocalConnector.GetFile(path, folder));
        }    

        [Test]
        [TestCase(_FAKE, false, ExpectedResult = null)]
        [TestCase(_FAKE, true, ExpectedResult = null)]
        [TestCase("testFile11.txt", false, ExpectedResult = null)]
        [TestCase("testFile11.txt", true, ExpectedResult = "testFile11.txt")]
        [TestCase("testFile211.txt", false, ExpectedResult = null)]
        [TestCase("testFile211.txt", true, ExpectedResult = "testFile211.txt")]
        public string GetFile_Local_DoesNotThrow(string folder, bool recursive)
        {
            var found = LocalConnector.GetFile(SamplesScriptFolder, folder, recursive);
            return (string.IsNullOrEmpty(found) ? found : Path.GetFileName(found));
        }

        [Test]
        [TestCase(_FAKE, false, ExpectedResult = null)]
        [TestCase(_FAKE, true, ExpectedResult = null)]
        [TestCase("testFile11.txt", false, ExpectedResult = null)]
        [TestCase("testFile11.txt", true, ExpectedResult = "testFile11.txt")]
        [TestCase("testFile211.txt", false, ExpectedResult = null)]
        [TestCase("testFile211.txt", true, ExpectedResult = "testFile211.txt")]
        public string GetFile_Remote_DoesNotThrow(string folder, bool recursive)
        {
            var found = RemoteConnector.GetFile(LocalPathToRemote(SamplesScriptFolder, "autocheck"), folder, recursive);
            return (string.IsNullOrEmpty(found) ? found : Path.GetFileName(found));
        } 

        [Test]
        [TestCase(null)]
        public void GetFolders_Local_Throws_ArgumentNullException(string path)
        {
            Assert.Throws<ArgumentNullException>(() => LocalConnector.GetFolders(path));
        }

        [Test]
        [TestCase(false, ExpectedResult = new string[]{"testFolder1", "testFolder2"})]
        [TestCase(true, ExpectedResult = new string[]{"testFolder1", "testFolder2", "testFolder21"})]
        public string[] GetFolders_Local_DoesNotThrow(bool recursive)
        {
            return LocalConnector.GetFolders(SamplesScriptFolder, recursive).ToList().Select(x => Path.GetFileName(x)).OrderBy(x => x).ToArray();
        }

        [Test]
        [TestCase(false, ExpectedResult = new string[]{"testFolder1", "testFolder2"})]
        [TestCase(true, ExpectedResult = new string[]{"testFolder1", "testFolder2", "testFolder21"})]
        public string[] GetFolders_Remote_DoesNotThrow(bool recursive)
        {
            return RemoteConnector.GetFolders(LocalPathToRemote(SamplesScriptFolder, "autocheck"), recursive).ToList().Select(x => Path.GetFileName(x)).OrderBy(x => x).ToArray();
        }

        [Test]
        [TestCase(null)]
        public void GetFiles_Local_Throws_ArgumentNullException(string path)
        {
            Assert.Throws<ArgumentNullException>(() => LocalConnector.GetFiles(path));
        }

        [Test]
        [TestCase(false, ExpectedResult = new string[]{})]
        [TestCase(true, ExpectedResult = new string[]{"testFile11.txt", "testFile21.txt", "testFile211.txt"})]
        public string[] GetFiles_Local_DoesNotThrow(bool recursive)
        {
            return LocalConnector.GetFiles(SamplesScriptFolder, recursive).ToList().Select(x => Path.GetFileName(x)).OrderBy(x => x).ToArray();
        }

        [Test]
        [TestCase(false, ExpectedResult = new string[]{})]
        [TestCase(true, ExpectedResult = new string[]{"testFile11.txt", "testFile21.txt", "testFile211.txt"})]
        public string[] GetFiles_Remote_DoesNotThrow(bool recursive)
        {
            return RemoteConnector.GetFiles(LocalPathToRemote(SamplesScriptFolder, "autocheck"), recursive).ToList().Select(x => Path.GetFileName(x)).OrderBy(x => x).ToArray();
        }

        [Test]
        [TestCase(null)]
        public void CountFolders_Local_Throws_ArgumentNullException(string path)
        {
            Assert.Throws<ArgumentNullException>(() => LocalConnector.CountFolders(path));
        }

        [Test]
        [TestCase(false, ExpectedResult = 2)]
        [TestCase(true, ExpectedResult = 3)]
        public int CountFolders_Local_DoesNotThrow(bool recursive)
        {
            return LocalConnector.CountFolders(SamplesScriptFolder, recursive);
        }

        [Test]
        [TestCase(false, ExpectedResult = 2)]
        [TestCase(true, ExpectedResult = 3)]
        public int CountFolders_Remote_DoesNotThrow(bool recursive)
        {
            return RemoteConnector.CountFolders(LocalPathToRemote(SamplesScriptFolder, "autocheck"), recursive);
        }

         [Test]
        [TestCase(null)]
        public void CountFiles_Local_Throws_ArgumentNullException(string path)
        {
            Assert.Throws<ArgumentNullException>(() => LocalConnector.CountFolders(path));
        }

        [Test]
        [TestCase(false, ExpectedResult = 0)]
        [TestCase(true, ExpectedResult = 3)]
        public int CountFiles_Local_DoesNotThrow(bool recursive)
        {
            return LocalConnector.CountFiles(SamplesScriptFolder, recursive);
        }

        [Test]
        [TestCase(false, ExpectedResult = 0)]
        [TestCase(true, ExpectedResult = 3)]
        public int CountFiles_Remote_DoesNotThrow(bool recursive)
        {
            return RemoteConnector.CountFiles(LocalPathToRemote(SamplesScriptFolder, "autocheck"), recursive);
        }

        [Test]
        [TestCase("ls", ExpectedResult = 0)]
        [TestCase("fake", ExpectedResult = 127)]
        public int RunCommand_Local_DoesNotThrow(string command)
        {
            if(Core.Utils.CurrentOS == OS.WIN) command = string.Format("wsl {0}", command);
            
            //TODO: on windows, test if the  wsl is installed because wsl -e will be used to test linux commands and windows ones in one step if don't, throw an exception

            var lres = LocalConnector.RunCommand(command);            
            Assert.IsNotNull(lres.response);

            return lres.code;
        }

        [Test]
        [TestCase("ls", ExpectedResult = 0)]
        [TestCase("fake", ExpectedResult = 127)]
        public int RunCommand_Remote_DoesNotThrow(string command)
        {            
            var lres = RemoteConnector.RunCommand(command);            
            Assert.IsNotNull(lres.response);
            
            return lres.code;
        }

        [Test]
        [TestCase("testFolder1\\testFile11.txt")]
        public void DownloadFile_Remote_DoesNotThrow(string sourceFile)
        {   
            //Checking the local sample file
            sourceFile = GetSampleFile(sourceFile);
            Assert.IsTrue(File.Exists(sourceFile));

            //Preparing remote path and local copy one
            var remoteFile = LocalPathToRemote(sourceFile, "autocheck");
            var destFile = Path.Combine(TempScriptFolder, Path.GetFileName(sourceFile));            
            Assert.IsFalse(File.Exists(destFile));

            //Download
            var downloadedFile = RemoteConnector.DownloadFile(remoteFile, Path.GetDirectoryName(destFile));
            Assert.IsTrue(File.Exists(downloadedFile));
            Assert.AreEqual(destFile, downloadedFile);
            Assert.AreEqual(File.ReadAllText(sourceFile), File.ReadAllText(downloadedFile));
        }

        [Test]
        public void DownloadFolder_Remote_DoesNotThrow()
        {   
            //Checking the local sample file
            var sourceFolder = SamplesScriptFolder;
            Assert.IsTrue(Directory.Exists(sourceFolder));

            //Preparing remote path and local copy one
            var remoteFolder = LocalPathToRemote(sourceFolder, "autocheck");
            var destFolder = Path.Combine(TempScriptFolder, Path.GetFileName(sourceFolder));            
            Assert.IsFalse(Directory.Exists(destFolder));

            //Download
            var downloadedFolder = RemoteConnector.DownloadFolder(remoteFolder, Path.GetDirectoryName(destFolder), true);
            Assert.IsTrue(Directory.Exists(downloadedFolder));
            Assert.AreEqual(destFolder, downloadedFolder);
            Assert.AreEqual(3, Directory.GetDirectories(downloadedFolder, "*", SearchOption.AllDirectories).Length);
            Assert.AreEqual(3, Directory.GetFiles(downloadedFolder, "*", SearchOption.AllDirectories).Length);
        }        
    }
}