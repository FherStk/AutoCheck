/*
    Copyright © 2020 Fernando Porrino Serrano
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
        protected const string _driveFolder = "\\AutoCheck\\test\\Connectors.GDrive";
        protected string _user = AutoCheck.Core.Utils.ConfigFile("gdrive_account.txt");
        protected string _secret = AutoCheck.Core.Utils.ConfigFile("gdrive_secret.json");

        protected AutoCheck.Core.Connectors.GDrive Conn;

        [OneTimeSetUp]
        public override void OneTimeSetUp() 
        {            
            Conn = new AutoCheck.Core.Connectors.GDrive(_user, _secret);                        
            base.OneTimeSetUp();    //needs "Conn" in order to use it within "CleanUp"

            if(Conn.GetFolder(Path.Combine(_driveFolder, "Test Folder 1", "Test Folder 1.1"), "TestFolder 1.1.1") == null)
                Conn.CreateFolder(Path.Combine(_driveFolder, "Test Folder 1", "Test Folder 1.1"), "TestFolder 1.1.1");

            if(Conn.GetFolder(Path.Combine(_driveFolder, "Test Folder 1", "Test Folder 1.1"), "TestFolder 1.1.2") == null)
                Conn.CreateFolder(Path.Combine(_driveFolder, "Test Folder 1", "Test Folder 1.1"), "TestFolder 1.1.2");

            if(Conn.GetFolder(Path.Combine(_driveFolder, "Test Folder 1"), "TestFolder 1.2") == null)
                Conn.CreateFolder(Path.Combine(_driveFolder, "Test Folder 1"), "TestFolder 1.2");

            if(Conn.GetFolder(_driveFolder, "Test Folder 2") == null)
                Conn.CreateFolder(_driveFolder, "Test Folder 2");

            if(Conn.GetFile(_driveFolder, "file.txt") == null)
                Conn.CreateFile(GetSampleFile("create.txt"), _driveFolder, "file.txt");

            if(Conn.GetFile(Path.Combine(_driveFolder, "Test Folder 1"), "file 1.txt") == null)
                Conn.CreateFile(GetSampleFile("create.txt"), Path.Combine(_driveFolder, "Test Folder 1"), "file 1.txt");

            if(Conn.GetFile(Path.Combine(_driveFolder, "Test Folder 1", "Test Folder 1.1"), "file 1.1.txt") == null)
                Conn.CreateFile(GetSampleFile("create.txt"), Path.Combine(_driveFolder, "Test Folder 1", "Test Folder 1.1"), "file 1.1.txt");

            if(Conn.GetFile(Path.Combine(_driveFolder, "Test Folder 1", "Test Folder 1.1"), "file 1.2.txt") == null)
                Conn.CreateFile(GetSampleFile("create.txt"), Path.Combine(_driveFolder, "Test Folder 1", "Test Folder 1.1"), "file 1.2.txt");            
        }

        [OneTimeTearDown]
        public override void OneTimeTearDown(){                
            base.OneTimeTearDown();
            Conn.Dispose();
        }

        protected override void CleanUp(){
            //TODO: remove the folders created by CreateFolder() to ensure a clean enviroment
            File.Delete(this.GetSampleFile("10mb.test"));
            Conn.DeleteFile(_driveFolder, "create.txt");
            Conn.DeleteFile(_driveFolder, "CreateFile_File1.txt");
            Conn.DeleteFile(_driveFolder, "CreateFile_File2.txt");
            Conn.DeleteFile(_driveFolder, "DeleteFile_File1.txt");
            Conn.DeleteFile(_driveFolder, "CopyFile_File1.txt");
            Conn.DeleteFile(_driveFolder, "CopyFile_File2.test");
            Conn.DeleteFile(_driveFolder, "10mb.test");
            Conn.DeleteFolder(_driveFolder, "DeleteFolder_Folder1");
            Conn.DeleteFolder(_driveFolder, "CreateFolder_Folder1");
            Conn.DeleteFolder(_driveFolder, "CreateFolder_Folder2");
            Conn.DeleteFolder(_driveFolder, "CreateFile_Folder1");
        }

        [Test]
        public void Constructor()
        {            
            //TODO: opens a browser to request interaction permissions... this must work on terminal...
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.GDrive(null, null));
            Assert.Throws<FileNotFoundException>(() => new AutoCheck.Core.Connectors.GDrive(this.GetSampleFile(_FAKE), string.Empty));
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.GDrive(string.Empty, this.GetSampleFile(_FAKE)));
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.GDrive(_user, ""));
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.GDrive("", _secret));
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.GDrive(_user, _secret));
        }
      
        [Test]
        public void GetFolder()
        {
            Assert.Throws<ArgumentNullException>(() => Conn.GetFolder(null, null));
            Assert.Throws<ArgumentNullException>(() => Conn.GetFolder(_FAKE, null));
            Assert.Throws<ArgumentNullException>(() => Conn.GetFolder(null, _FAKE));
            Assert.Throws<ArgumentInvalidException>(() => Conn.GetFolder(_FAKE, _FAKE));            

            var path = Path.GetDirectoryName(_driveFolder);
            var folder = Path.GetFileName(_driveFolder);
            Assert.IsNotNull(Conn.GetFolder(path, folder, false));
            Assert.IsNull(Conn.GetFolder(path, _FAKE, false));
            
            Assert.IsNotNull(Conn.GetFolder(path, folder, true));
            Assert.IsNull(Conn.GetFolder(path, _FAKE, true));

            Assert.IsNotNull(Conn.GetFolder("\\", folder, true));
            Assert.IsNull(Conn.GetFolder("\\", _FAKE, true));
            
            path = Path.GetDirectoryName(path);
            Assert.IsNotNull(Conn.GetFolder(path, folder, true));
            Assert.IsNull(Conn.GetFolder(path, _FAKE, true));
            
        }

        [Test]
        public void GetFile()
        {
            Assert.Throws<ArgumentNullException>(() => Conn.GetFile(null, null));
            Assert.Throws<ArgumentNullException>(() => Conn.GetFile(_FAKE, null));
            Assert.Throws<ArgumentNullException>(() => Conn.GetFile(null, _FAKE));
            Assert.Throws<ArgumentInvalidException>(() => Conn.GetFile(_FAKE, _FAKE));            

            var path = _driveFolder;
            var file = "file.txt";
            
            Assert.IsNotNull(Conn.GetFile(path, file, false));
            Assert.IsNull(Conn.GetFile(path, _FAKE, false));
            
            Assert.IsNotNull(Conn.GetFile(path, file, true));
            Assert.IsNull(Conn.GetFile(path, _FAKE, true));

            Assert.IsNotNull(Conn.GetFile("\\", file, true));
            Assert.IsNull(Conn.GetFile("\\", _FAKE, true));
            
            path = Path.GetDirectoryName(path);
            Assert.IsNotNull(Conn.GetFile(path, file, true));
            Assert.IsNull(Conn.GetFile(path, _FAKE, true));
        }

        [Test]
        public void CountFolders()
        {
            Assert.Throws<ArgumentNullException>(() => Conn.CountFolders(null));            

            var path = Path.Combine(_driveFolder, "Test Folder 1");
            Assert.AreEqual(2, Conn.CountFolders(path, false));
            Assert.AreEqual(4, Conn.CountFolders(path, true));
        }

        [Test]
        public void CountFiles()
        {
            Assert.Throws<ArgumentNullException>(() => Conn.CountFiles(null));            

            var path = Path.Combine(_driveFolder, "Test Folder 1");
            Assert.AreEqual(1, Conn.CountFiles(path, false));
            Assert.AreEqual(3, Conn.CountFiles(path, true));            
        }

        [Test]
        public void CreateFile()
        {       
            //NOTE: sometimes the test fails because the API has not refreshed the content

            Assert.Throws<ArgumentNullException>(() => Conn.CreateFile("", ""));
            Assert.Throws<ArgumentNullException>(() => Conn.CreateFile(_FAKE, ""));
            Assert.Throws<ArgumentNullException>(() => Conn.CreateFile("", _FAKE));
            Assert.Throws<FileNotFoundException>(() => Conn.CreateFile(_FAKE, _FAKE));

            var sample = "create.txt";
            var remote = "CreateFile_File1.txt";
            var path = _driveFolder;
            var local = this.GetSampleFile(sample);
            
            Assert.DoesNotThrow(() => Conn.CreateFile(local, path, remote));   
            System.Threading.Thread.Sleep(1000);
            Assert.IsNotNull(Conn.GetFile(path, remote));

            remote = "CreateFile_File2";
            Assert.DoesNotThrow(() => Conn.CreateFile(local, path, remote));   
            System.Threading.Thread.Sleep(1000);
            Assert.IsNotNull(Conn.GetFile(path, String.Format("{0}.txt", remote)));

            Assert.DoesNotThrow(() => Conn.CreateFile(local, path));   
            System.Threading.Thread.Sleep(1000);
            Assert.IsNotNull(Conn.GetFile(path, sample));   

            remote = "CreateFile_File3.txt";  
            path = Path.Combine(_driveFolder, "CreateFile_Folder1\\CreateFile_Folder1.1");          
            Assert.DoesNotThrow(() => Conn.CreateFile(local, path, remote));   
            System.Threading.Thread.Sleep(1000);
            Assert.IsNotNull(Conn.GetFile(path, remote));
        }

        [Test]
        public void DeleteFile()
        {
            //NOTE: sometimes the test fails because the API has not refreshed the content

            var file ="DeleteFile_File1.txt";
            Assert.Throws<ArgumentNullException>(() => Conn.DeleteFile("", ""));
            Assert.Throws<ArgumentNullException>(() => Conn.DeleteFile(_FAKE, ""));
            Assert.Throws<ArgumentNullException>(() => Conn.DeleteFile("", _FAKE));
            
            //Does not exist
            Assert.IsNull(Conn.GetFile(_driveFolder, file));
            System.Threading.Thread.Sleep(1000);
            Assert.DoesNotThrow(() => Conn.DeleteFile(_driveFolder, file));

            //Creating
            Assert.DoesNotThrow(() => Conn.CreateFile(this.GetSampleFile("delete.txt"), _driveFolder, file));
            System.Threading.Thread.Sleep(1000);
            Assert.IsNotNull(Conn.GetFile(_driveFolder, file));

            //Destroying
            Assert.DoesNotThrow(() => Conn.DeleteFile(_driveFolder, file));
            System.Threading.Thread.Sleep(1000);
            Assert.IsNull(Conn.GetFile(_driveFolder, file));
        }

        [Test]
        public void CopyFile()
        {               
            //NOTE: sometimes the test fails because the API has not refreshed the content
            Assert.Throws<ArgumentInvalidException>(() => Conn.CopyFile(new Uri("http://www.google.com"), ""));
            Assert.Throws<ArgumentInvalidException>(() => Conn.CopyFile(new Uri("http://www.google.com"), _FAKE));                
            Assert.Throws<ArgumentInvalidException>(() => Conn.CopyFile(new Uri("https://drive.google.com/file/d/"), _FAKE));
            Assert.Throws<ArgumentNullException>(() => Conn.CopyFile(new Uri("https://drive.google.com/file/d/0B1MVW1mFO2zmWjJMR2xSYUUwdG8/edit"), ""));
            
            var file = "CopyFile_File1.txt";
            Assert.DoesNotThrow(() => Conn.CopyFile(new Uri("https://drive.google.com/file/d/0B1MVW1mFO2zmWjJMR2xSYUUwdG8/edit"), _driveFolder, file));
            System.Threading.Thread.Sleep(1000);
            Assert.IsNotNull(Conn.GetFile(_driveFolder, file, false));

            file = "CopyFile_File2";
            Assert.DoesNotThrow(() => Conn.CopyFile(new Uri("https://drive.google.com/file/d/0B1MVW1mFO2zmWjJMR2xSYUUwdG8/edit"), _driveFolder, file));
            System.Threading.Thread.Sleep(1000);
            Assert.IsNotNull(Conn.GetFile(_driveFolder, string.Format("{0}.test", file), false));

            Assert.DoesNotThrow(() => Conn.CopyFile(new Uri("https://drive.google.com/file/d/0B1MVW1mFO2zmWjJMR2xSYUUwdG8/edit"), _driveFolder));
            System.Threading.Thread.Sleep(1000);
            Assert.IsNotNull(Conn.GetFile(_driveFolder, "10mb.test", false));
        }

        [Test]
        public void CreateFolder()
        {            
            //NOTE: sometimes the test fails because the API has not refreshed the content
            Assert.Throws<ArgumentNullException>(() => Conn.CreateFolder("", ""));
            Assert.Throws<ArgumentNullException>(() => Conn.CreateFolder(_FAKE, ""));
            Assert.Throws<ArgumentNullException>(() => Conn.CreateFolder("", _FAKE));
            
            var path = _driveFolder;
            var folder = "CreateFolder_Folder1";
            Assert.DoesNotThrow(() => Conn.CreateFolder(path, folder));
            System.Threading.Thread.Sleep(1000);
            Assert.IsNotNull(Conn.GetFolder(path, folder));
            
            path = Path.Combine(_driveFolder, "CreateFolder_Folder2", "CreateFolder_Folder2.1");
            folder = "CreateFolder_Folder2.1.1";
            Assert.DoesNotThrow(() => Conn.CreateFolder(path, folder));
            System.Threading.Thread.Sleep(1000);
            Assert.IsNotNull(Conn.GetFolder(path, folder));
        }
       
        [Test]
        public void DeleteFolder()
        {            
            //NOTE: sometimes the test fails because the API has not refreshed the content
            Assert.Throws<ArgumentNullException>(() => Conn.DeleteFolder("", ""));
            Assert.Throws<ArgumentNullException>(() => Conn.DeleteFolder(_FAKE, ""));
            Assert.Throws<ArgumentNullException>(() => Conn.DeleteFolder("", _FAKE));

            var folder = "DeleteFolder_Folder1";

            //Does not exist
            Assert.IsNull(Conn.GetFolder(_driveFolder, folder));
            System.Threading.Thread.Sleep(1000);
            Assert.DoesNotThrow(() => Conn.DeleteFolder(_driveFolder, folder));

            //Creating
            Assert.DoesNotThrow(() => Conn.CreateFolder(_driveFolder, folder));
            System.Threading.Thread.Sleep(1000);
            Assert.IsNotNull(Conn.GetFolder(_driveFolder, folder));

            //Destroying
            Assert.DoesNotThrow(() => Conn.DeleteFolder(_driveFolder, folder));
            System.Threading.Thread.Sleep(1000);
            Assert.IsNull(Conn.GetFolder(_driveFolder, folder));
        }

       
        [Test]
        public void Download()
        {            
            var filePath = this.GetSampleFile("10mb.test");
            Assert.Throws<ArgumentInvalidException>(() => Conn.Download(new Uri("http://www.google.com"), ""));
            Assert.Throws<ArgumentInvalidException>(() => Conn.Download(new Uri("http://www.google.com"), _FAKE));                
            Assert.Throws<ArgumentInvalidException>(() => Conn.Download(new Uri("https://drive.google.com/file/d/"), _FAKE));
            Assert.Throws<ArgumentNullException>(() => Conn.Download(new Uri("https://drive.google.com/file/d/0B1MVW1mFO2zmWjJMR2xSYUUwdG8/edit"), ""));
            Assert.AreEqual(filePath, Conn.Download(new Uri("https://drive.google.com/file/d/0B1MVW1mFO2zmWjJMR2xSYUUwdG8/edit"), this.SamplesScriptFolder));
            Assert.IsTrue(File.Exists(filePath));
        }
    }
}