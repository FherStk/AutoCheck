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

namespace AutoCheck.Test.Connectors
{
    [Parallelizable(ParallelScope.All)]    
    public class GDrive : Test
    {
        protected const string _driveFolder = "\\AutoCheck\\test\\Connectors.GDrive";       //TODO: delete because not all tests can use it and I prefer to standarize all of them
        protected string _user = AutoCheck.Core.Utils.ConfigFile("gdrive_account.txt");     //TODO: delete because not all tests can use it and I prefer to standarize all of them
        protected string _secret = AutoCheck.Core.Utils.ConfigFile("gdrive_secret.json");   //TODO: delete because not all tests can use it and I prefer to standarize all of them

        protected AutoCheck.Core.Connectors.GDrive Conn;

        [OneTimeSetUp]        
        public override void OneTimeSetUp() 
        {            
            Conn = new AutoCheck.Core.Connectors.GDrive(_user, _secret);                        
            base.OneTimeSetUp();    //needs "Conn" in order to use it within "CleanUp"

            var path = Path.Combine(_driveFolder, "Test Folder 1", "Test Folder 1.1", "TestFolder 1.1.1");
            Conn.CreateFolder(path);

            path = Path.Combine(_driveFolder, "Test Folder 1", "Test Folder 1.1", "TestFolder 1.1.2");
            Conn.CreateFolder(path);

            path = Path.Combine(_driveFolder, "Test Folder 1", "Test Folder 1.2");
            Conn.CreateFolder(path);

            path = Path.Combine(_driveFolder, "Test Folder 2");
            Conn.CreateFolder(path);
            
            Conn.CreateFile(GetSampleFile("create.txt"), _driveFolder, "file.txt");

            path = Path.Combine(_driveFolder, "Test Folder 1");
            Conn.CreateFile(GetSampleFile("create.txt"), path, "file 1.txt");

            path = Path.Combine(_driveFolder, "Test Folder 1", "Test Folder 1.1");
            Conn.CreateFile(GetSampleFile("create.txt"), path, "file 1.1.txt");

            path = Path.Combine(_driveFolder, "Test Folder 1", "Test Folder 1.1");
            Conn.CreateFile(GetSampleFile("create.txt"), path, "file 1.2.txt");
         
        }

        [OneTimeTearDown]
        public override void OneTimeTearDown(){                
            base.OneTimeTearDown();
            Conn.Dispose();
        }

        protected override void CleanUp(){
            //TODO: remove the folders created by CreateFolder() to ensure a clean enviroment
            File.Delete(this.GetSampleFile("10mb.test"));
            Conn.DeleteFolder(_driveFolder);
            Conn.CreateFolder(_driveFolder);
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
            Assert.Throws<ArgumentNullException>(() => Conn.GetFolder(path, folder));
        }

        [Test]
        [TestCase(_FAKE, _FAKE)]
        public void GetFolder_Throws_ArgumentInvalidException(string path, string folder)
        {
            Assert.Throws<ArgumentInvalidException>(() => Conn.GetFolder(path, folder));
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
            var f = Conn.GetFolder(path, folder, recursive);
            if(f == null) return null;
            else return f.Name;
        }

        [Test]
        [TestCase(null, null)]
        [TestCase(_FAKE, null)]
        [TestCase(null, _FAKE)]
        public void GetFile_Throws_ArgumentNullException(string path, string file)
        {
            Assert.Throws<ArgumentNullException>(() => Conn.GetFile(path, file));
        }

        [Test]
        [TestCase(_FAKE, _FAKE)]
        public void GetFile_Throws_ArgumentInvalidException(string path, string file)
        {
            Assert.Throws<ArgumentInvalidException>(() => Conn.GetFile(path, file));
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
            var f = Conn.GetFile(path, file, recursive);
            if(f == null) return null;
            else return f.Name;
        }

        [Test]
        [TestCase(null)]        
        public void CountFolders_Throws_ArgumentNullException(string path)
        {
            Assert.Throws<ArgumentNullException>(() => Conn.CountFolders(path));
        }

        [Test]
        [TestCase(_driveFolder, "Test Folder 1", false, ExpectedResult = 2)]
        [TestCase(_driveFolder, "Test Folder 1", true, ExpectedResult = 4)]
        public int CountFolders_DoesNotThrows(string path, string folder, bool recursive)
        {
            return Conn.CountFolders(Path.Combine(path, folder), recursive);
        }

        [Test]
        [TestCase(null)]        
        public void CountFiles_Throws_ArgumentNullException(string path)
        {
            Assert.Throws<ArgumentNullException>(() => Conn.CountFiles(path));
        }

        [Test]
        [TestCase(_driveFolder, "Test Folder 1", false, ExpectedResult = 1)]
        [TestCase(_driveFolder, "Test Folder 1", true, ExpectedResult = 3)]
        public int CountFiles_DoesNotThrows(string path, string folder, bool recursive)
        {
            return Conn.CountFiles(Path.Combine(path, folder), recursive);
        }

        [Test]
        [TestCase("", "")]
        [TestCase(_FAKE, "")]
        [TestCase("", _FAKE)]
        public void CreateFile_Throws_ArgumentNullException(string local, string remote)
        {
            Assert.Throws<ArgumentNullException>(() => Conn.CreateFile(local, remote));
        }

        [Test]
        [TestCase(_FAKE, _FAKE)]
        public void CreateFile_Throws_FileNotFoundException(string local, string remote)
        {
            Assert.Throws<FileNotFoundException>(() => Conn.CreateFile(local, remote));
        }

        [Test]
        [TestCase("create.txt", _driveFolder, null, "CreateFile_File1.txt", "CreateFile_File1.txt", ExpectedResult = "CreateFile_File1.txt")]
        [TestCase("create.txt", _driveFolder, null,  "CreateFile_File2", "CreateFile_File2.txt", ExpectedResult = "CreateFile_File2.txt")]
        [TestCase("create.txt", _driveFolder, "CreateFile_Folder1\\CreateFile_Folder1.1", "CreateFile_File3.txt", "CreateFile_File3.txt", ExpectedResult = "CreateFile_File3.txt")]
        public string CreateFile_DoesNotThrows(string sample, string remotePath, string remoteFolder, string remoteFileCreate, string remoteFileFind)
        {
            remotePath = (string.IsNullOrEmpty(remoteFolder) ? remotePath : Path.Combine(remotePath, remoteFolder));

            var local = this.GetSampleFile(sample);
            Conn.CreateFile(local, remotePath, remoteFileCreate);
            System.Threading.Thread.Sleep(5000);

            var f = Conn.GetFile(remotePath, remoteFileFind);
            if(f == null) return null;
            else return f.Name;
        }

        [Test]
        [TestCase("", "")]
        [TestCase(_FAKE, "")]
        [TestCase("", _FAKE)]
        public void DeleteFile_Throws_ArgumentNullException(string local, string remote)
        {
            Assert.Throws<ArgumentNullException>(() => Conn.DeleteFile(local, remote));           
        }

        [Test]
        [TestCase("delete.txt", _driveFolder, "DeleteFile_File1.txt")]
        public void DeleteFile_DoesNotThrow(string localFile, string remotePath, string remoteFile)
        {
            //Does not exist
            Assert.IsNull(Conn.GetFile(remotePath, remoteFile));
            System.Threading.Thread.Sleep(5000);
            Assert.DoesNotThrow(() => Conn.DeleteFile(remotePath, remoteFile));

            //Creating
            Assert.DoesNotThrow(() => Conn.CreateFile(this.GetSampleFile(localFile), remotePath, remoteFile));
            System.Threading.Thread.Sleep(5000);
            Assert.IsNotNull(Conn.GetFile(remotePath, remoteFile));

            //Destroying
            Assert.DoesNotThrow(() => Conn.DeleteFile(remotePath, remoteFile));
            System.Threading.Thread.Sleep(5000);
            Assert.IsNull(Conn.GetFile(remotePath, remoteFile));      
        }

        [Test]
        [TestCase("https://drive.google.com/file/d/0B1MVW1mFO2zmWjJMR2xSYUUwdG8/edit", "")]
        public void CopyFile_Throws_ArgumentNullException(string uri, string remote)
        {
            Assert.Throws<ArgumentNullException>(() => Conn.CopyFile(new Uri(uri), remote));
        }

        [Test]
        [TestCase("http://www.google.com", "")]
        [TestCase("http://www.google.com", _FAKE)]
        [TestCase("https://drive.google.com/file/d/", _FAKE)]
        public void CopyFile_Throws_ArgumentInvalidException(string uri, string remote)
        {
            Assert.Throws<ArgumentInvalidException>(() => Conn.CopyFile(new Uri(uri), remote));
        }

        [Test]
        [TestCase("https://drive.google.com/file/d/0B1MVW1mFO2zmWjJMR2xSYUUwdG8/edit", _driveFolder, "CopyFile_File1.txt", "CopyFile_File1.txt")]
        [TestCase("https://drive.google.com/file/d/0B1MVW1mFO2zmWjJMR2xSYUUwdG8/edit", _driveFolder, "CopyFile_File2", "CopyFile_File2.test")]
        [TestCase("https://drive.google.com/file/d/0B1MVW1mFO2zmWjJMR2xSYUUwdG8/edit", _driveFolder, "", "10mb.test")]
        public void CopyFile_DoesNotThrow(string uri, string remotePath, string remoteFileName, string remoteAssignedName)
        {               
            Assert.DoesNotThrow(() => Conn.CopyFile(new Uri("https://drive.google.com/file/d/0B1MVW1mFO2zmWjJMR2xSYUUwdG8/edit"), remotePath, remoteFileName));
            System.Threading.Thread.Sleep(5000);
            Assert.IsNotNull(Conn.GetFile(remotePath, remoteAssignedName, false));
        }

        [Test]
        [TestCase("", "")]
        [TestCase(_FAKE, "")]
        [TestCase("", _FAKE)]
        public void CreateFolder_Throws_ArgumentNullException(string path, string folder)
        {
            Assert.Throws<ArgumentNullException>(() => Conn.CreateFolder(path, folder));
        }

        [Test]
        [TestCase(_driveFolder, null, "CreateFolder_Folder1")]
        [TestCase(_driveFolder, "CreateFolder_Folder2/CreateFolder_Folder2.1", "CreateFolder_Folder2.1.1")]
        [TestCase(_driveFolder, null, "CreateFolder_Folder2/CreateFolder_Folder2.1/CreateFolder_Folder2.1.1")]
        public void CreateFolder_DoesNotThrow(string @base, string path, string folder)
        {      
            @base = (string.IsNullOrEmpty(path) ? @base : Path.Combine(@base, path));    

            Assert.DoesNotThrow(() => Conn.CreateFolder(@base, folder));
            System.Threading.Thread.Sleep(5000);
            Assert.IsNotNull(Conn.GetFolder(@base, Path.GetFileName(folder)));
        }

        [Test]
        [TestCase("", "")]
        [TestCase(_FAKE, "")]
        [TestCase("", _FAKE)]
        public void DeleteFolder_Throws_ArgumentNullException(string path, string folder)
        {
            Assert.Throws<ArgumentNullException>(() => Conn.DeleteFolder(path, folder));
        }

        [Test]
        [TestCase(_driveFolder, "DeleteFolder_Folder1")]
        public void DeleteFolder_DoesNotThrow(string path, string folder)
        {            
            //Does not exist
            Assert.IsNull(Conn.GetFolder(_driveFolder, folder));
            System.Threading.Thread.Sleep(5000);
            Assert.DoesNotThrow(() => Conn.DeleteFolder(_driveFolder, folder));

            //Creating
            Assert.DoesNotThrow(() => Conn.CreateFolder(_driveFolder, folder));
            System.Threading.Thread.Sleep(5000);
            Assert.IsNotNull(Conn.GetFolder(_driveFolder, folder));

            //Destroying
            Assert.DoesNotThrow(() => Conn.DeleteFolder(_driveFolder, folder));
            System.Threading.Thread.Sleep(5000);
            Assert.IsNull(Conn.GetFolder(_driveFolder, folder));
        }

        [Test]
        [TestCase("http://www.google.com", "")]
        [TestCase("http://www.google.com", _FAKE)]
        [TestCase("https://drive.google.com/file/d/", _FAKE)]
        public void Download_Throws_ArgumentInvalidException(string uri, string savePath)
        {
            Assert.Throws<ArgumentInvalidException>(() => Conn.Download(new Uri(uri), savePath));
        }

        [Test]
        [TestCase("https://drive.google.com/file/d/0B1MVW1mFO2zmWjJMR2xSYUUwdG8/edit", "")]
        public void Download_Throws_ArgumentNullException(string uri, string savePath)
        {
            Assert.Throws<ArgumentNullException>(() => Conn.Download(new Uri(uri), savePath));
        }

        [Test]
        [TestCase("https://drive.google.com/file/d/0B1MVW1mFO2zmWjJMR2xSYUUwdG8/edit", "10mb.test")]
        public void Download_DoesNotThrow(string uri, string file)
        {            
            var filePath = this.GetSampleFile(file);            
            Assert.AreEqual(filePath, Conn.Download(new Uri(uri), this.SamplesScriptFolder));
            Assert.IsTrue(File.Exists(filePath));
        }

        [Test]
        [TestCase(null, null, null)]
        [TestCase(_FAKE, null, null)]        
        public void UploadFile_Throws_ArgumentNullException(string localFilePath, string remoteFilePath, string remoteFileName)
        {            
            Assert.Throws<ArgumentNullException>(() => Conn.UploadFile(localFilePath, remoteFilePath, remoteFileName));            
        }

        [Test]
        [TestCase(_FAKE, _FAKE, null)]
        [TestCase(_FAKE, _FAKE, _FAKE)]
        public void UploadFile_Throws_FileNotFoundException(string localFilePath, string remoteFilePath, string remoteFileName)
        {            
            Assert.Throws<FileNotFoundException>(() => Conn.UploadFile(localFilePath, remoteFilePath, remoteFileName));            
        }

        [Test]
        [TestCase("create.txt", _driveFolder, "UploadFile", "uploadedFile.txt", "uploadedFile.txt")]
        [TestCase("create.txt", _driveFolder, "UploadFile", null, "create.txt")]
        public void UploadFile_DoesNotThrow(string localFilePath, string remoteBasePath, string remoteFilePath, string remoteFileName, string expectedFileName)
        {   
            remoteFilePath = (string.IsNullOrEmpty(remoteFilePath) ? remoteBasePath : Path.Combine(remoteBasePath, remoteFilePath));

            Assert.IsFalse(Conn.ExistsFolder(remoteFilePath));
            Assert.DoesNotThrow(() => Conn.UploadFile(GetSampleFile(localFilePath), remoteFilePath, remoteFileName));
            System.Threading.Thread.Sleep(5000);
            Assert.IsTrue(Conn.ExistsFile(remoteFilePath, expectedFileName));            
        }
    }
}