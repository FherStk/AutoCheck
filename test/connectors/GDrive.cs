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
        protected const string _driveFolder = "\\AutoCheck\\test\\Connectors.GDrive";       //TODO: delete because not all tests can use it and I prefer to standarize all of them
        protected string _user = AutoCheck.Core.Utils.ConfigFile("gdrive_account.txt");     //TODO: delete because not all tests can use it and I prefer to standarize all of them
        protected string _secret = AutoCheck.Core.Utils.ConfigFile("gdrive_secret.json");   //TODO: delete because not all tests can use it and I prefer to standarize all of them

        protected AutoCheck.Core.Connectors.GDrive LocalConn;
        private ConcurrentDictionary<string, AutoCheck.Core.Connectors.GDrive> RemotePool = new ConcurrentDictionary<string, AutoCheck.Core.Connectors.GDrive>();

        [OneTimeSetUp]        
        public override void OneTimeSetUp() 
        {                        
            LocalConn = new AutoCheck.Core.Connectors.GDrive(_user, _secret);                 
                               
            base.OneTimeSetUp();    //needs "Conn" in order to use it within "CleanUp"
            Thread.Sleep(5000);

            var path = Path.Combine(_driveFolder, "Test Folder 1", "Test Folder 1.1", "TestFolder 1.1.1");
            LocalConn.CreateFolder(path);

            path = Path.Combine(_driveFolder, "Test Folder 1", "Test Folder 1.1", "TestFolder 1.1.2");
            LocalConn.CreateFolder(path);

            path = Path.Combine(_driveFolder, "Test Folder 1", "Test Folder 1.2");
            LocalConn.CreateFolder(path);

            path = Path.Combine(_driveFolder, "Test Folder 2");
            LocalConn.CreateFolder(path);
            
            LocalConn.CreateFile(GetSampleFile("create.txt"), _driveFolder, "file.txt");

            path = Path.Combine(_driveFolder, "Test Folder 1");
            LocalConn.CreateFile(GetSampleFile("create.txt"), path, "file 1.txt");

            path = Path.Combine(_driveFolder, "Test Folder 1", "Test Folder 1.1");
            LocalConn.CreateFile(GetSampleFile("create.txt"), path, "file 1.1.txt");

            path = Path.Combine(_driveFolder, "Test Folder 1", "Test Folder 1.1");
            LocalConn.CreateFile(GetSampleFile("create.txt"), path, "file 1.2.txt");      
        }

        [SetUp]
        public override void SetUp() 
        {
            //Create a new and unique remote connector for the current context, local connectors can be shared but not the remote ones because 
            //remote connectors cannot share its internal ssh connection or it can be closed by one when another is using it.        
            var added = false;
            do added = RemotePool.TryAdd(TestContext.CurrentContext.Test.ID, new AutoCheck.Core.Connectors.GDrive(OS.GNU, "localhost", "usuario", "usuario", _user, _secret));             
            while(!added);      

            base.SetUp();          
        }

        [OneTimeTearDown]
        public override void OneTimeTearDown(){                
            base.OneTimeTearDown();
            LocalConn.Dispose();
        }

        [TearDown]
        public void TearDown(){
            var remoteConn = RemotePool[TestContext.CurrentContext.Test.ID];
            remoteConn.Dispose();
        }

        protected override void CleanUp(){
            //TODO: remove the folders created by CreateFolder() to ensure a clean enviroment
            File.Delete(GetSampleFile("10mb.test"));
            LocalConn.DeleteFolder(_driveFolder);            
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
            Assert.Throws<ArgumentNullException>(() => LocalConn.GetFolder(path, folder));
        }

        [Test]
        [TestCase(_FAKE, _FAKE)]
        public void GetFolder_Throws_ArgumentInvalidException(string path, string folder)
        {
            Assert.Throws<ArgumentInvalidException>(() => LocalConn.GetFolder(path, folder));
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
            var f = LocalConn.GetFolder(path, folder, recursive);
            if(f == null) return null;
            else return f.Name;
        }

        [Test]
        [TestCase(null, null)]
        [TestCase(_FAKE, null)]
        [TestCase(null, _FAKE)]
        public void GetFile_Throws_ArgumentNullException(string path, string file)
        {
            Assert.Throws<ArgumentNullException>(() => LocalConn.GetFile(path, file));
        }

        [Test]
        [TestCase(_FAKE, _FAKE)]
        public void GetFile_Throws_ArgumentInvalidException(string path, string file)
        {
            Assert.Throws<ArgumentInvalidException>(() => LocalConn.GetFile(path, file));
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
            var f = LocalConn.GetFile(path, file, recursive);
            if(f == null) return null;
            else return f.Name;
        }

        [Test]
        [TestCase(null)]        
        public void CountFolders_Throws_ArgumentNullException(string path)
        {
            Assert.Throws<ArgumentNullException>(() => LocalConn.CountFolders(path));
        }

        [Test]
        [TestCase(_driveFolder, "Test Folder 1", false, ExpectedResult = 2)]
        [TestCase(_driveFolder, "Test Folder 1", true, ExpectedResult = 4)]
        public int CountFolders_DoesNotThrows(string path, string folder, bool recursive)
        {
            return LocalConn.CountFolders(Path.Combine(path, folder), recursive);
        }

        [Test]
        [TestCase(null)]        
        public void CountFiles_Throws_ArgumentNullException(string path)
        {
            Assert.Throws<ArgumentNullException>(() => LocalConn.CountFiles(path));
        }

        [Test]
        [TestCase(_driveFolder, "Test Folder 1", false, ExpectedResult = 1)]
        [TestCase(_driveFolder, "Test Folder 1", true, ExpectedResult = 3)]
        public int CountFiles_DoesNotThrows(string path, string folder, bool recursive)
        {
            return LocalConn.CountFiles(Path.Combine(path, folder), recursive);
        }

        [Test]
        [TestCase("", "")]
        [TestCase(_FAKE, "")]
        [TestCase("", _FAKE)]
        public void CreateFile_Throws_ArgumentNullException(string local, string remote)
        {
            Assert.Throws<ArgumentNullException>(() => LocalConn.CreateFile(local, remote));
        }

        [Test]
        [TestCase(_FAKE, _FAKE)]
        public void CreateFile_Throws_FileNotFoundException(string local, string remote)
        {
            Assert.Throws<FileNotFoundException>(() => LocalConn.CreateFile(local, remote));
        }

        [Test]
        [TestCase("create.txt", _driveFolder, null, "CreateFile_File1.txt", "CreateFile_File1.txt", ExpectedResult = "CreateFile_File1.txt")]
        [TestCase("create.txt", _driveFolder, null,  "CreateFile_File2", "CreateFile_File2.txt", ExpectedResult = "CreateFile_File2.txt")]
        [TestCase("create.txt", _driveFolder, "CreateFile_Folder1\\CreateFile_Folder1.1", "CreateFile_File3.txt", "CreateFile_File3.txt", ExpectedResult = "CreateFile_File3.txt")]
        public string CreateFile_DoesNotThrows(string sample, string remotePath, string remoteFolder, string remoteFileCreate, string remoteFileFind)
        {
            remotePath = (string.IsNullOrEmpty(remoteFolder) ? remotePath : Path.Combine(remotePath, remoteFolder));

            var local = GetSampleFile(sample);
            LocalConn.CreateFile(local, remotePath, remoteFileCreate);
            Thread.Sleep(5000);

            var f = LocalConn.GetFile(remotePath, remoteFileFind);
            if(f == null) return null;
            else return f.Name;
        }

        [Test]
        [TestCase("", "")]
        [TestCase(_FAKE, "")]
        [TestCase("", _FAKE)]
        public void DeleteFile_Throws_ArgumentNullException(string local, string remote)
        {
            Assert.Throws<ArgumentNullException>(() => LocalConn.DeleteFile(local, remote));           
        }

        [Test]
        [TestCase("delete.txt", _driveFolder, "DeleteFile_File1.txt")]
        public void DeleteFile_DoesNotThrow(string localFile, string remotePath, string remoteFile)
        {
            //Does not exist
            Assert.IsNull(LocalConn.GetFile(remotePath, remoteFile));
            Thread.Sleep(5000);
            Assert.DoesNotThrow(() => LocalConn.DeleteFile(remotePath, remoteFile));

            //Creating
            Assert.DoesNotThrow(() => LocalConn.CreateFile(GetSampleFile(localFile), remotePath, remoteFile));
            Thread.Sleep(5000);
            Assert.IsNotNull(LocalConn.GetFile(remotePath, remoteFile));

            //Destroying
            Assert.DoesNotThrow(() => LocalConn.DeleteFile(remotePath, remoteFile));
            Thread.Sleep(5000);
            Assert.IsNull(LocalConn.GetFile(remotePath, remoteFile));      
        }

        [Test]
        [TestCase("https://drive.google.com/file/d/0B1MVW1mFO2zmWjJMR2xSYUUwdG8/edit", "")]
        public void CopyFile_Throws_ArgumentNullException(string uri, string remote)
        {
            Assert.Throws<ArgumentNullException>(() => LocalConn.CopyFile(new Uri(uri), remote));
        }

        [Test]
        [TestCase("http://www.google.com", "")]
        [TestCase("http://www.google.com", _FAKE)]
        [TestCase("https://drive.google.com/file/d/", _FAKE)]
        public void CopyFile_Throws_ArgumentInvalidException(string uri, string remote)
        {
            Assert.Throws<ArgumentInvalidException>(() => LocalConn.CopyFile(new Uri(uri), remote));
        }

        [Test]
        [TestCase("https://drive.google.com/file/d/0B1MVW1mFO2zmWjJMR2xSYUUwdG8/edit", _driveFolder, "CopyFile_File1.txt", "CopyFile_File1.txt")]
        [TestCase("https://drive.google.com/file/d/0B1MVW1mFO2zmWjJMR2xSYUUwdG8/edit", _driveFolder, "CopyFile_File2", "CopyFile_File2.test")]
        [TestCase("https://drive.google.com/file/d/0B1MVW1mFO2zmWjJMR2xSYUUwdG8/edit", _driveFolder, "", "10mb.test")]
        public void CopyFile_DoesNotThrow(string uri, string remotePath, string remoteFileName, string remoteAssignedName)
        {               
            Assert.DoesNotThrow(() => LocalConn.CopyFile(new Uri("https://drive.google.com/file/d/0B1MVW1mFO2zmWjJMR2xSYUUwdG8/edit"), remotePath, remoteFileName));
            Thread.Sleep(5000);
            Assert.IsNotNull(LocalConn.GetFile(remotePath, remoteAssignedName, false));
        }

        [Test]
        [TestCase("", "")]
        [TestCase(_FAKE, "")]
        [TestCase("", _FAKE)]
        public void CreateFolder_Throws_ArgumentNullException(string path, string folder)
        {
            Assert.Throws<ArgumentNullException>(() => LocalConn.CreateFolder(path, folder));
        }

        [Test]
        [TestCase(_driveFolder, null, "CreateFolder_Folder1")]
        [TestCase(_driveFolder, "CreateFolder_Folder2/CreateFolder_Folder2.1", "CreateFolder_Folder2.1.1")]
        [TestCase(_driveFolder, null, "CreateFolder_Folder2/CreateFolder_Folder2.1/CreateFolder_Folder2.1.1")]
        public void CreateFolder_DoesNotThrow(string @base, string path, string folder)
        {      
            @base = (string.IsNullOrEmpty(path) ? @base : Path.Combine(@base, path));    

            Assert.DoesNotThrow(() => LocalConn.CreateFolder(@base, folder));
            Thread.Sleep(5000);
            Assert.IsNotNull(LocalConn.GetFolder(@base, Path.GetFileName(folder)));
        }

        [Test]
        [TestCase("", "")]
        [TestCase(_FAKE, "")]
        [TestCase("", _FAKE)]
        public void DeleteFolder_Throws_ArgumentNullException(string path, string folder)
        {
            Assert.Throws<ArgumentNullException>(() => LocalConn.DeleteFolder(path, folder));
        }

        [Test]
        [TestCase(_driveFolder, "DeleteFolder_Folder1")]
        public void DeleteFolder_DoesNotThrow(string path, string folder)
        {            
            //Does not exist
            Assert.IsNull(LocalConn.GetFolder(_driveFolder, folder));
            Thread.Sleep(5000);
            Assert.DoesNotThrow(() => LocalConn.DeleteFolder(_driveFolder, folder));

            //Creating
            Assert.DoesNotThrow(() => LocalConn.CreateFolder(_driveFolder, folder));
            Thread.Sleep(5000);
            Assert.IsNotNull(LocalConn.GetFolder(_driveFolder, folder));

            //Destroying
            Assert.DoesNotThrow(() => LocalConn.DeleteFolder(_driveFolder, folder));
            Thread.Sleep(5000);
            Assert.IsNull(LocalConn.GetFolder(_driveFolder, folder));
        }

        [Test]
        [TestCase("http://www.google.com", "")]
        [TestCase("http://www.google.com", _FAKE)]
        [TestCase("https://drive.google.com/file/d/", _FAKE)]
        public void Download_Throws_ArgumentInvalidException(string uri, string savePath)
        {
            Assert.Throws<ArgumentInvalidException>(() => LocalConn.Download(new Uri(uri), savePath));
        }

        [Test]
        [TestCase("https://drive.google.com/file/d/0B1MVW1mFO2zmWjJMR2xSYUUwdG8/edit", "")]
        public void Download_Throws_ArgumentNullException(string uri, string savePath)
        {
            Assert.Throws<ArgumentNullException>(() => LocalConn.Download(new Uri(uri), savePath));
        }

        [Test]
        [TestCase("https://drive.google.com/file/d/0B1MVW1mFO2zmWjJMR2xSYUUwdG8/edit", "10mb.test")]
        public void Download_DoesNotThrow(string uri, string file)
        {            
            var filePath = GetSampleFile(file);            
            Assert.AreEqual(filePath, LocalConn.Download(new Uri(uri), SamplesScriptFolder));
            Assert.IsTrue(File.Exists(filePath));
        }

        [Test]
        [TestCase(null, null, null)]
        [TestCase(_FAKE, null, null)]        
        public void UploadFile_Throws_ArgumentNullException(string localFilePath, string remoteFilePath, string remoteFileName)
        {            
            Assert.Throws<ArgumentNullException>(() => LocalConn.UploadFile(localFilePath, remoteFilePath, remoteFileName));            
        }

        [Test]
        [TestCase(_FAKE, _FAKE, null)]
        [TestCase(_FAKE, _FAKE, _FAKE)]
        public void UploadFile_Throws_FileNotFoundException(string localFilePath, string remoteFilePath, string remoteFileName)
        {            
            Assert.Throws<FileNotFoundException>(() => LocalConn.UploadFile(localFilePath, remoteFilePath, remoteFileName));            
        }

        [Test]
        [TestCase("create.txt", _driveFolder, "UploadFile 1", "uploadedFile.txt", "uploadedFile.txt")]
        [TestCase("create.txt", _driveFolder, "UploadFile 2", null, "create.txt")]
        public void UploadFile_DoesNotThrow(string localFilePath, string remoteBasePath, string remoteFilePath, string remoteFileName, string expectedFileName)
        {               
            //Note: the source code for local and remote mode are exactly the same, just need to test that the remote file is being downloaded from remote and parsed.
            remoteFilePath = (string.IsNullOrEmpty(remoteFilePath) ? remoteBasePath : Path.Combine(remoteBasePath, remoteFilePath));

            var remoteConn = RemotePool[TestContext.CurrentContext.Test.ID];
            Assert.IsFalse(remoteConn.ExistsFolder(remoteFilePath));
            Assert.DoesNotThrow(() => remoteConn.UploadFile(LocalPathToWsl(GetSampleFile(localFilePath)), remoteFilePath, remoteFileName));
            Thread.Sleep(5000);
            Assert.IsTrue(remoteConn.ExistsFile(remoteFilePath, expectedFileName));            
        }

        [Test]
        [TestCase(null, null)]
        [TestCase(_FAKE, null)]        
        public void UploadFolder_Throws_ArgumentNullException(string localFolderPath, string remoteFolderPath)
        {            
            Assert.Throws<ArgumentNullException>(() => LocalConn.UploadFolder(localFolderPath, remoteFolderPath));            
        }

        [Test]
        [TestCase(_FAKE, _FAKE)]        
        public void UploadFolder_Throws_DirectoryNotFoundException(string localFolderPath, string remoteFolderPath)
        {            
            Assert.Throws<DirectoryNotFoundException>(() => LocalConn.UploadFolder(localFolderPath, remoteFolderPath));            
        }

        [Test]
        [TestCase("Test Folder 0", _driveFolder, "UploadFolder 1", "My Uploaded Folder", false, "My Uploaded Folder", 2, 0)]
        [TestCase("Test Folder 0", _driveFolder, "UploadFolder 2", null, false, "Test Folder 0", 2, 0)]
        [TestCase("Test Folder 0", _driveFolder, "UploadFolder 3", null, true, "Test Folder 0", 4, 4)]
        public void UploadFolder_DoesNotThrow(string localFolderPath, string remoteBasePath, string remoteFolderPath, string remoteFolderName, bool recursive, string expectedFolderName, int expectedFolderCount, int expectedFileCount)
        {   
            //Note: the source code for local and remote mode are exactly the same, just need to test that the remote file is being downloaded from remote and parsed.
            remoteFolderPath = (string.IsNullOrEmpty(remoteFolderPath) ? remoteBasePath : Path.Combine(remoteBasePath, remoteFolderPath));

            var remoteConn = RemotePool[TestContext.CurrentContext.Test.ID];
            Assert.IsFalse(remoteConn.ExistsFolder(remoteFolderPath));
            Assert.DoesNotThrow(() => remoteConn.UploadFolder(LocalPathToWsl(Path.Combine(SamplesScriptFolder, localFolderPath)), remoteFolderPath, remoteFolderName, recursive));
            
            Thread.Sleep(5000);            
            Assert.IsTrue(remoteConn.ExistsFolder(remoteFolderPath, expectedFolderName));

            remoteFolderPath = Path.Combine(remoteFolderPath, expectedFolderName);            
            Assert.AreEqual(expectedFolderCount, remoteConn.CountFolders(remoteFolderPath, recursive));
            Assert.AreEqual(expectedFileCount, remoteConn.CountFiles(remoteFolderPath, recursive));
        }
    }
}