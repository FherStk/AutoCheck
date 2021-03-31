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
using System.Threading;
using System.Collections.Concurrent;
using NUnit.Framework;
using AutoCheck.Core.Exceptions;
using OS = AutoCheck.Core.Utils.OS;

namespace AutoCheck.Test.Connectors
{
    [Parallelizable(ParallelScope.All)]    
    public class GDrive : Test
    {
        private const string _driveFolder = "\\AutoCheck\\test\\Connectors.GDrive";       //TODO: delete because not all tests can use it and I prefer to standarize all of them
        private string _user = AutoCheck.Core.Utils.ConfigFile("gdrive_account.txt");     //TODO: delete because not all tests can use it and I prefer to standarize all of them
        private string _secret = AutoCheck.Core.Utils.ConfigFile("gdrive_secret.json");   //TODO: delete because not all tests can use it and I prefer to standarize all of them
        private ConcurrentDictionary<string, AutoCheck.Core.Connectors.GDrive> LocalConnectors = new ConcurrentDictionary<string, AutoCheck.Core.Connectors.GDrive>();
        private ConcurrentDictionary<string, AutoCheck.Core.Connectors.GDrive> RemoteConnectors = new ConcurrentDictionary<string, AutoCheck.Core.Connectors.GDrive>();
        private AutoCheck.Core.Connectors.GDrive LocalConnector {
            get{
                return LocalConnectors[TestContext.CurrentContext.Test.ID];
            }
        }
        private AutoCheck.Core.Connectors.GDrive RemoteConnector {
            get{
                return RemoteConnectors[TestContext.CurrentContext.Test.ID];
            }
        }

        [OneTimeSetUp]        
        public override void OneTimeSetUp() 
        {                                                                                  
            base.OneTimeSetUp();    //needs "Conn" in order to use it within "CleanUp"
            Thread.Sleep(5000);

            var localConn = new AutoCheck.Core.Connectors.GDrive(_user, _secret);  
            var path = Path.Combine(_driveFolder, "Test Folder 1", "Test Folder 1.1", "TestFolder 1.1.1");
            localConn.CreateFolder(path);

            path = Path.Combine(_driveFolder, "Test Folder 1", "Test Folder 1.1", "TestFolder 1.1.2");
            localConn.CreateFolder(path);

            path = Path.Combine(_driveFolder, "Test Folder 1", "Test Folder 1.2");
            localConn.CreateFolder(path);

            path = Path.Combine(_driveFolder, "Test Folder 2");
            localConn.CreateFolder(path);
            
            localConn.CreateFile(GetSampleFile("create.txt"), _driveFolder, "file.txt");

            path = Path.Combine(_driveFolder, "Test Folder 1");
            localConn.CreateFile(GetSampleFile("create.txt"), path, "file 1.txt");

            path = Path.Combine(_driveFolder, "Test Folder 1", "Test Folder 1.1");
            localConn.CreateFile(GetSampleFile("create.txt"), path, "file 1.1.txt");

            path = Path.Combine(_driveFolder, "Test Folder 1", "Test Folder 1.1");
            localConn.CreateFile(GetSampleFile("create.txt"), path, "file 1.2.txt");      
        }

        [SetUp]
        public void SetUp() 
        {
            //Create a new and unique remote connector for the current context, local connectors can be shared but not the remote ones because 
            //remote connectors cannot share its internal ssh connection or it can be closed by one when another is using it.        
            var added = false;
            do added = RemoteConnectors.TryAdd(TestContext.CurrentContext.Test.ID, new AutoCheck.Core.Connectors.GDrive(OS.GNU, "localhost", "usuario", "usuario", _user, _secret));             
            while(!added);      

            added = false;
            do added = LocalConnectors.TryAdd(TestContext.CurrentContext.Test.ID, new AutoCheck.Core.Connectors.GDrive(_user, _secret));             
            while(!added);       
        }       

        [TearDown]
        public override void TearDown(){            
            RemoteConnector.Dispose();
            LocalConnector.Dispose();
            base.TearDown();
        }

        protected override void CleanUp(){                        
            var localConn = new AutoCheck.Core.Connectors.GDrive(_user, _secret);  
            localConn.DeleteFolder(_driveFolder);            

            LocalConnectors.Clear();
            RemoteConnectors.Clear();
        }

        [Test]    
        [TestCase("gdrive_account.txt", "gdrive_secret.json")]    
        public void Constructor_DoesNotThrow(string accountFile, string secretFile)
        {      
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.GDrive(AutoCheck.Core.Utils.ConfigFile(accountFile), AutoCheck.Core.Utils.ConfigFile(secretFile)));
        }    

        [Test]
        [TestCase(null, null)]                             
        public void Constructor_Throws_ArgumentNullException(string accountFile, string secretFile)
        {      
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.GDrive(AutoCheck.Core.Utils.ConfigFile(accountFile), AutoCheck.Core.Utils.ConfigFile(secretFile)));
        } 

        [Test]        
        [TestCase(_FAKE, "")]
        [TestCase("", _FAKE)]   
        [TestCase("gdrive_account.txt", "")]   
        [TestCase("", "gdrive_secret.json")]     
        public void Constructor_Throws_FileNotFoundException(string accountFile, string secretFile)
        {      
            Assert.Throws<FileNotFoundException>(() => new AutoCheck.Core.Connectors.GDrive(AutoCheck.Core.Utils.ConfigFile(accountFile), AutoCheck.Core.Utils.ConfigFile(secretFile)));
        }   

        [Test]
        [TestCase(null, null)]
        [TestCase(_FAKE, null)]
        [TestCase(null, _FAKE)]
        public void GetFolder_Throws_ArgumentNullException(string path, string folder)
        {
            var localConn = new AutoCheck.Core.Connectors.GDrive(_user, _secret);  
            Assert.Throws<ArgumentNullException>(() => localConn.GetFolder(path, folder));
        }

        [Test]
        [TestCase(_FAKE, _FAKE)]
        public void GetFolder_Throws_ArgumentInvalidException(string path, string folder)
        {
            var localConn = new AutoCheck.Core.Connectors.GDrive(_user, _secret);  
            Assert.Throws<ArgumentInvalidException>(() => localConn.GetFolder(path, folder));
        }
      
        [Test]
        [TestCase("\\AutoCheck\\test", "Connectors.GDrive", false, ExpectedResult = "Connectors.GDrive")]
        [TestCase("\\AutoCheck\\test", _FAKE, false, ExpectedResult = null)]
        [TestCase("\\", "Connectors.GDrive", true, ExpectedResult = "Connectors.GDrive")]
        [TestCase("\\", _FAKE, true, ExpectedResult = null)]
        [TestCase("\\AutoCheck", "Connectors.GDrive", true, ExpectedResult = "Connectors.GDrive")]
        [TestCase("\\AutoCheck", _FAKE, true, ExpectedResult = null)]
        public string GetFolder_DoesNotThrow(string path, string folder, bool recursive)
        {                    
            var f = LocalConnector.GetFolder(path, folder, recursive);
            if(f == null) return null;
            else return f.Name;
        }

        [Test]
        [TestCase(null, null)]
        [TestCase(_FAKE, null)]
        [TestCase(null, _FAKE)]
        public void GetFile_Throws_ArgumentNullException(string path, string file)
        {
            Assert.Throws<ArgumentNullException>(() => LocalConnector.GetFile(path, file));
        }

        [Test]
        [TestCase(_FAKE, _FAKE)]
        public void GetFile_Throws_ArgumentInvalidException(string path, string file)
        {
            Assert.Throws<ArgumentInvalidException>(() => LocalConnector.GetFile(path, file));
        }

        [Test]
        [TestCase(_driveFolder, "file.txt", false, ExpectedResult = "file.txt")]
        [TestCase(_driveFolder, _FAKE, false, ExpectedResult = null)]
        [TestCase("\\", "file.txt", true, ExpectedResult = "file.txt")]
        [TestCase("\\", _FAKE, true, ExpectedResult = null)]
        [TestCase("\\AutoCheck", "file.txt", true, ExpectedResult = "file.txt")]
        [TestCase("\\AutoCheck", _FAKE, true, ExpectedResult = null)]
        public string GetFile_DoesNotThrow(string path, string file, bool recursive)
        {                             
            var f = LocalConnector.GetFile(path, file, recursive);
            if(f == null) return null;
            else return f.Name;
        }

        [Test]
        [TestCase(null)]        
        public void CountFolders_Throws_ArgumentNullException(string path)
        {
            Assert.Throws<ArgumentNullException>(() => LocalConnector.CountFolders(path));
        }

        [Test]
        [TestCase(_driveFolder, "Test Folder 1", false, ExpectedResult = 2)]
        [TestCase(_driveFolder, "Test Folder 1", true, ExpectedResult = 4)]
        public int CountFolders_DoesNotThrows(string path, string folder, bool recursive)
        {
            return LocalConnector.CountFolders(Path.Combine(path, folder), recursive);
        }

        [Test]
        [TestCase(null)]        
        public void CountFiles_Throws_ArgumentNullException(string path)
        {
            Assert.Throws<ArgumentNullException>(() => LocalConnector.CountFiles(path));
        }

        [Test]
        [TestCase(_driveFolder, "Test Folder 1", false, ExpectedResult = 1)]
        [TestCase(_driveFolder, "Test Folder 1", true, ExpectedResult = 3)]
        public int CountFiles_DoesNotThrows(string path, string folder, bool recursive)
        {
            return LocalConnector.CountFiles(Path.Combine(path, folder), recursive);
        }

        [Test]
        [TestCase("", "")]        
        [TestCase("", _FAKE)]
        public void CreateFile_Throws_ArgumentNullException(string local, string remote)
        {
            Assert.Throws<ArgumentNullException>(() => LocalConnector.CreateFile(local, remote));
        }

        [Test]
        [TestCase(_FAKE, "")]
        [TestCase(_FAKE, _FAKE)]
        public void CreateFile_Throws_FileNotFoundException(string local, string remote)
        {
            Assert.Throws<FileNotFoundException>(() => LocalConnector.CreateFile(local, remote));
        }

        [Test]
        [TestCase("create.txt", _driveFolder, null, "CreateFile_File1.txt", "CreateFile_File1.txt", ExpectedResult = "CreateFile_File1.txt")]
        [TestCase("create.txt", _driveFolder, null,  "CreateFile_File2", "CreateFile_File2.txt", ExpectedResult = "CreateFile_File2.txt")]
        [TestCase("create.txt", _driveFolder, "CreateFile_Folder1\\CreateFile_Folder1.1", "CreateFile_File3.txt", "CreateFile_File3.txt", ExpectedResult = "CreateFile_File3.txt")]
        public string CreateFile_DoesNotThrows(string sample, string remotePath, string remoteFolder, string remoteFileCreate, string remoteFileFind)
        {
            var conn = LocalConnector;
            remotePath = (string.IsNullOrEmpty(remoteFolder) ? remotePath : Path.Combine(remotePath, remoteFolder));

            conn.CreateFile(GetSampleFile(sample), remotePath, remoteFileCreate);
            Thread.Sleep(5000);

            var f = conn.GetFile(remotePath, remoteFileFind);
            if(f == null) return null;
            else return f.Name;
        }

        [Test]
        [TestCase("", "")]
        [TestCase(_FAKE, "")]
        [TestCase("", _FAKE)]
        public void DeleteFile_Throws_ArgumentNullException(string local, string remote)
        {
            Assert.Throws<ArgumentNullException>(() => LocalConnector.DeleteFile(local, remote));           
        }

        [Test]
        [TestCase("delete.txt", _driveFolder, "DeleteFile_File1.txt")]
        public void DeleteFile_DoesNotThrow(string localFile, string remotePath, string remoteFile)
        {
            var conn = LocalConnector;

            //Does not exist
            Assert.IsNull(conn.GetFile(remotePath, remoteFile));
            Thread.Sleep(5000);
            Assert.DoesNotThrow(() => conn.DeleteFile(remotePath, remoteFile));

            //Creating
            Assert.DoesNotThrow(() => conn.CreateFile(GetSampleFile(localFile), remotePath, remoteFile));
            Thread.Sleep(5000);
            Assert.IsNotNull(conn.GetFile(remotePath, remoteFile));

            //Destroying
            Assert.DoesNotThrow(() =>conn.DeleteFile(remotePath, remoteFile));
            Thread.Sleep(5000);
            Assert.IsNull(conn.GetFile(remotePath, remoteFile));      
        }

        [Test]
        [TestCase("https://drive.google.com/file/d/0B1MVW1mFO2zmWjJMR2xSYUUwdG8/edit", "")]
        public void CopyFile_Throws_ArgumentNullException(string uri, string remote)
        {
            Assert.Throws<ArgumentNullException>(() => LocalConnector.CopyFile(new Uri(uri), remote));
        }

        [Test]
        [TestCase("http://www.google.com", "")]
        [TestCase("http://www.google.com", _FAKE)]
        [TestCase("https://drive.google.com/file/d/", _FAKE)]
        public void CopyFile_Throws_ArgumentInvalidException(string uri, string remote)
        {
            Assert.Throws<ArgumentInvalidException>(() => LocalConnector.CopyFile(new Uri(uri), remote));
        }

        [Test]
        [TestCase("https://drive.google.com/file/d/0B1MVW1mFO2zmWjJMR2xSYUUwdG8/edit", _driveFolder, "CopyFile_File1.txt", "CopyFile_File1.txt")]
        [TestCase("https://drive.google.com/file/d/0B1MVW1mFO2zmWjJMR2xSYUUwdG8/edit", _driveFolder, "CopyFile_File2", "CopyFile_File2.test")]
        [TestCase("https://drive.google.com/file/d/0B1MVW1mFO2zmWjJMR2xSYUUwdG8/edit", _driveFolder, "", "10mb.test")]
        public void CopyFile_DoesNotThrow(string uri, string remotePath, string remoteFileName, string remoteAssignedName)
        {       
            var conn = LocalConnector; 

            Assert.DoesNotThrow(() => conn.CopyFile(new Uri("https://drive.google.com/file/d/0B1MVW1mFO2zmWjJMR2xSYUUwdG8/edit"), remotePath, remoteFileName));
            Thread.Sleep(5000);
            Assert.IsNotNull(conn.GetFile(remotePath, remoteAssignedName, false));
        }

        [Test]
        [TestCase("", "")]
        [TestCase(_FAKE, "")]
        [TestCase("", _FAKE)]
        public void CreateFolder_Throws_ArgumentNullException(string path, string folder)
        {
            Assert.Throws<ArgumentNullException>(() => LocalConnector.CreateFolder(path, folder));
        }

        [Test]
        [TestCase(_driveFolder, null, "CreateFolder_Folder1")]
        [TestCase(_driveFolder, "CreateFolder_Folder2/CreateFolder_Folder2.1", "CreateFolder_Folder2.1.1")]
        [TestCase(_driveFolder, null, "CreateFolder_Folder2/CreateFolder_Folder2.1/CreateFolder_Folder2.1.1")]
        public void CreateFolder_DoesNotThrow(string @base, string path, string folder)
        {      
            var conn = LocalConnector; 
            @base = (string.IsNullOrEmpty(path) ? @base : Path.Combine(@base, path));    

            Assert.DoesNotThrow(() => conn.CreateFolder(@base, folder));
            Thread.Sleep(5000);
            Assert.IsNotNull(conn.GetFolder(@base, Path.GetFileName(folder)));
        }

        [Test]
        [TestCase("", "")]
        [TestCase(_FAKE, "")]
        [TestCase("", _FAKE)]
        public void DeleteFolder_Throws_ArgumentNullException(string path, string folder)
        {
            Assert.Throws<ArgumentNullException>(() => LocalConnector.DeleteFolder(path, folder));
        }

        [Test]
        [TestCase(_driveFolder, "DeleteFolder_Folder1")]
        public void DeleteFolder_DoesNotThrow(string path, string folder)
        {        
            var conn = LocalConnector; 

            //Does not exist
            Assert.IsNull(conn.GetFolder(_driveFolder, folder));
            Thread.Sleep(5000);
            Assert.DoesNotThrow(() => conn.DeleteFolder(_driveFolder, folder));

            //Creating
            Assert.DoesNotThrow(() => conn.CreateFolder(_driveFolder, folder));
            Thread.Sleep(5000);
            Assert.IsNotNull(conn.GetFolder(_driveFolder, folder));

            //Destroying
            Assert.DoesNotThrow(() => conn.DeleteFolder(_driveFolder, folder));
            Thread.Sleep(5000);
            Assert.IsNull(conn.GetFolder(_driveFolder, folder));
        }

        [Test]
        [TestCase("http://www.google.com", "")]
        [TestCase("http://www.google.com", _FAKE)]
        [TestCase("https://drive.google.com/file/d/", _FAKE)]
        public void Download_Throws_ArgumentInvalidException(string uri, string savePath)
        {
            Assert.Throws<ArgumentInvalidException>(() => LocalConnector.Download(new Uri(uri), savePath));
        }

        [Test]
        [TestCase("https://drive.google.com/file/d/0B1MVW1mFO2zmWjJMR2xSYUUwdG8/edit", "")]
        public void Download_Throws_ArgumentNullException(string uri, string savePath)
        {
            Assert.Throws<ArgumentNullException>(() => LocalConnector.Download(new Uri(uri), savePath));
        }

        [Test]
        [TestCase("https://drive.google.com/file/d/0B1MVW1mFO2zmWjJMR2xSYUUwdG8/edit", "10mb.test")]
        public void Download_DoesNotThrow(string uri, string file)
        {            
            var filePath = Path.Combine(TempScriptFolder, file)            ;
            Assert.AreEqual(filePath, LocalConnector.Download(new Uri(uri), TempScriptFolder));
            Assert.IsTrue(File.Exists(filePath));
        }

        [Test]
        [TestCase(null, null, null)]            
        public void UploadFile_Throws_ArgumentNullException(string localFilePath, string remoteFilePath, string remoteFileName)
        {            
            Assert.Throws<ArgumentNullException>(() => LocalConnector.UploadFile(localFilePath, remoteFilePath, remoteFileName));            
        }

        [Test]
        [TestCase(_FAKE, null, null)]    
        [TestCase(_FAKE, _FAKE, null)]
        [TestCase(_FAKE, _FAKE, _FAKE)]
        public void UploadFile_Throws_FileNotFoundException(string localFilePath, string remoteFilePath, string remoteFileName)
        {            
            Assert.Throws<FileNotFoundException>(() => LocalConnector.UploadFile(localFilePath, remoteFilePath, remoteFileName));            
        }

        [Test]
        [TestCase("create.txt", _driveFolder, "UploadFile 1", "uploadedFile.txt", "uploadedFile.txt")]
        [TestCase("create.txt", _driveFolder, "UploadFile 2", null, "create.txt")]
        public void UploadFile_DoesNotThrow(string localFilePath, string remoteBasePath, string remoteFilePath, string remoteFileName, string expectedFileName)
        {               
            var conn = RemoteConnector;

            //Note: the source code for local and remote mode are exactly the same, just need to test that the remote file is being downloaded from remote and parsed.
            remoteFilePath = (string.IsNullOrEmpty(remoteFilePath) ? remoteBasePath : Path.Combine(remoteBasePath, remoteFilePath));
           
            Assert.IsFalse(conn.ExistsFolder(remoteFilePath));
            Assert.DoesNotThrow(() => conn.UploadFile(LocalPathToWsl(GetSampleFile(localFilePath)), remoteFilePath, remoteFileName));
            Thread.Sleep(5000);
            Assert.IsTrue(conn.ExistsFile(remoteFilePath, expectedFileName));            
        }

        [Test]
        [TestCase(null, null)]
        [TestCase(_FAKE, null)]        
        public void UploadFolder_Throws_ArgumentNullException(string localFolderPath, string remoteFolderPath)
        {            
            Assert.Throws<ArgumentNullException>(() => LocalConnector.UploadFolder(localFolderPath, remoteFolderPath));            
        }

        [Test]
        [TestCase(_FAKE, _FAKE)]        
        public void UploadFolder_Throws_DirectoryNotFoundException(string localFolderPath, string remoteFolderPath)
        {            
            Assert.Throws<DirectoryNotFoundException>(() => LocalConnector.UploadFolder(localFolderPath, remoteFolderPath));            
        }

        [Test]
        [TestCase("Test Folder 0", _driveFolder, "UploadFolder 1", "My Uploaded Folder", false, "My Uploaded Folder", 2, 0)]
        [TestCase("Test Folder 0", _driveFolder, "UploadFolder 2", null, false, "Test Folder 0", 2, 0)]
        [TestCase("Test Folder 0", _driveFolder, "UploadFolder 3", null, true, "Test Folder 0", 4, 4)]
        public void UploadFolder_DoesNotThrow(string localFolderPath, string remoteBasePath, string remoteFolderPath, string remoteFolderName, bool recursive, string expectedFolderName, int expectedFolderCount, int expectedFileCount)
        {   
            var conn = RemoteConnector;

            //Note: the source code for local and remote mode are exactly the same, just need to test that the remote file is being downloaded from remote and parsed.
            remoteFolderPath = (string.IsNullOrEmpty(remoteFolderPath) ? remoteBasePath : Path.Combine(remoteBasePath, remoteFolderPath));

            Assert.IsFalse(conn.ExistsFolder(remoteFolderPath));
            Assert.DoesNotThrow(() => conn.UploadFolder(LocalPathToWsl(Path.Combine(SamplesScriptFolder, localFolderPath)), remoteFolderPath, remoteFolderName, recursive));
            
            Thread.Sleep(5000);            
            Assert.IsTrue(conn.ExistsFolder(remoteFolderPath, expectedFolderName));

            remoteFolderPath = Path.Combine(remoteFolderPath, expectedFolderName);            
            Assert.AreEqual(expectedFolderCount, conn.CountFolders(remoteFolderPath, recursive));
            Assert.AreEqual(expectedFileCount, conn.CountFiles(remoteFolderPath, recursive));
        }
    }
}