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
using System.IO;
using NUnit.Framework;
using AutoCheck.Exceptions;

namespace AutoCheck.Test.Connectors
{
    [Parallelizable(ParallelScope.All)]    
    public class GDrive : Core.Test
    {
        protected const string _fake = "fake";
        protected const string _driveFolder = "\\AutoCheck\\test\\Connectors.GDrive";
        protected AutoCheck.Connectors.GDrive Conn;

        [OneTimeSetUp]
        public void Init() 
        {
            base.Setup("gdrive");
            
            Conn = new AutoCheck.Connectors.GDrive(this.GetSampleFile("client_secret.json"), "porrino.fernando@elpuig.xeill.net");            
            
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
            
            CleanUp();
        }

        [OneTimeTearDown]
        public void ShutDown(){    
            CleanUp(); 
            Conn.Dispose();
        }

        private void CleanUp(){
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
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Connectors.GDrive(null,string.Empty));
            Assert.Throws<FileNotFoundException>(() => new AutoCheck.Connectors.GDrive(this.GetSampleFile(_fake),string.Empty));
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Connectors.GDrive(this.GetSampleFile("client_secret.json"), ""));
            Assert.DoesNotThrow(() => new AutoCheck.Connectors.GDrive(this.GetSampleFile("client_secret.json"), "porrino.fernando@elpuig.xeill.net"));
        }
      
        [Test]
        public void GetFolder()
        {
            Assert.Throws<ArgumentNullException>(() => Conn.GetFolder(null, null));
            Assert.Throws<ArgumentNullException>(() => Conn.GetFolder(_fake, null));
            Assert.Throws<ArgumentNullException>(() => Conn.GetFolder(null, _fake));
            Assert.Throws<ArgumentInvalidException>(() => Conn.GetFolder(_fake, _fake));            

            var path = Path.GetDirectoryName(_driveFolder);
            var folder = Path.GetFileName(_driveFolder);
            Assert.IsNotNull(Conn.GetFolder(path, folder, false));
            Assert.IsNull(Conn.GetFolder(path, _fake, false));
            
            Assert.IsNotNull(Conn.GetFolder(path, folder, true));
            Assert.IsNull(Conn.GetFolder(path, _fake, true));

            Assert.IsNotNull(Conn.GetFolder("\\", folder, true));
            Assert.IsNull(Conn.GetFolder("\\", _fake, true));
            
            path = Path.GetDirectoryName(path);
            Assert.IsNotNull(Conn.GetFolder(path, folder, true));
            Assert.IsNull(Conn.GetFolder(path, _fake, true));
            
        }

        [Test]
        public void GetFile()
        {
            Assert.Throws<ArgumentNullException>(() => Conn.GetFile(null, null));
            Assert.Throws<ArgumentNullException>(() => Conn.GetFile(_fake, null));
            Assert.Throws<ArgumentNullException>(() => Conn.GetFile(null, _fake));
            Assert.Throws<ArgumentInvalidException>(() => Conn.GetFile(_fake, _fake));            

            var path = _driveFolder;
            var file = "file.txt";
            
            Assert.IsNotNull(Conn.GetFile(path, file, false));
            Assert.IsNull(Conn.GetFile(path, _fake, false));
            
            Assert.IsNotNull(Conn.GetFile(path, file, true));
            Assert.IsNull(Conn.GetFile(path, _fake, true));

            Assert.IsNotNull(Conn.GetFile("\\", file, true));
            Assert.IsNull(Conn.GetFile("\\", _fake, true));
            
            path = Path.GetDirectoryName(path);
            Assert.IsNotNull(Conn.GetFile(path, file, true));
            Assert.IsNull(Conn.GetFile(path, _fake, true));
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
            Assert.Throws<ArgumentNullException>(() => Conn.CreateFile("", ""));
            Assert.Throws<ArgumentNullException>(() => Conn.CreateFile(_fake, ""));
            Assert.Throws<ArgumentNullException>(() => Conn.CreateFile("", _fake));
            Assert.Throws<FileNotFoundException>(() => Conn.CreateFile(_fake, _fake));

            var sample = "create.txt";
            var remote = "CreateFile_File1.txt";
            var path = _driveFolder;
            var local = this.GetSampleFile(sample);
            
            Assert.DoesNotThrow(() => Conn.CreateFile(local, path, remote));   
            Assert.IsNotNull(Conn.GetFile(path, remote));

            remote = "CreateFile_File2";
            Assert.DoesNotThrow(() => Conn.CreateFile(local, path, remote));   
            Assert.IsNotNull(Conn.GetFile(path, String.Format("{0}.txt", remote)));

            Assert.DoesNotThrow(() => Conn.CreateFile(local, path));   
            Assert.IsNotNull(Conn.GetFile(path, sample));   

            remote = "CreateFile_File3.txt";  
            path = Path.Combine(_driveFolder, "CreateFile_Folder1\\CreateFile_Folder1.1");          
            Assert.DoesNotThrow(() => Conn.CreateFile(local, path, remote));   
            Assert.IsNotNull(Conn.GetFile(path, remote));
        }

        [Test]
        public void DeleteFile()
        {
            var file ="DeleteFile_File1.txt";
            Assert.Throws<ArgumentNullException>(() => Conn.DeleteFile("", ""));
            Assert.Throws<ArgumentNullException>(() => Conn.DeleteFile(_fake, ""));
            Assert.Throws<ArgumentNullException>(() => Conn.DeleteFile("", _fake));
            
            //Does not exist
            Assert.IsNull(Conn.GetFile(_driveFolder, file));
            Assert.DoesNotThrow(() => Conn.DeleteFile(_driveFolder, file));

            //Creating
            Assert.DoesNotThrow(() => Conn.CreateFile(this.GetSampleFile("delete.txt"), _driveFolder, file));
            Assert.IsNotNull(Conn.GetFile(_driveFolder, file));

            //Destroying
            Assert.DoesNotThrow(() => Conn.DeleteFile(_driveFolder, file));
            Assert.IsNull(Conn.GetFile(_driveFolder, file));
        }

        [Test]
        public void CopyFile()
        {                       
            Assert.Throws<ArgumentInvalidException>(() => Conn.CopyFile(new Uri("http://www.google.com"), ""));
            Assert.Throws<ArgumentInvalidException>(() => Conn.CopyFile(new Uri("http://www.google.com"), _fake));                
            Assert.Throws<ArgumentInvalidException>(() => Conn.CopyFile(new Uri("https://drive.google.com/file/d/"), _fake));
            Assert.Throws<ArgumentNullException>(() => Conn.CopyFile(new Uri("https://drive.google.com/file/d/0B1MVW1mFO2zmWjJMR2xSYUUwdG8/edit"), ""));

            var file = "CopyFile_File1.txt";
            Assert.DoesNotThrow(() => Conn.CopyFile(new Uri("https://drive.google.com/file/d/0B1MVW1mFO2zmWjJMR2xSYUUwdG8/edit"), _driveFolder, file));
            Assert.IsNotNull(Conn.GetFile(_driveFolder, file, false));

            file = "CopyFile_File2";
            Assert.DoesNotThrow(() => Conn.CopyFile(new Uri("https://drive.google.com/file/d/0B1MVW1mFO2zmWjJMR2xSYUUwdG8/edit"), _driveFolder, file));
            Assert.IsNotNull(Conn.GetFile(_driveFolder, string.Format("{0}.test", file), false));

            Assert.DoesNotThrow(() => Conn.CopyFile(new Uri("https://drive.google.com/file/d/0B1MVW1mFO2zmWjJMR2xSYUUwdG8/edit"), _driveFolder));
            Assert.IsNotNull(Conn.GetFile(_driveFolder, "10mb.test", false));
        }

        [Test]
        public void CreateFolder()
        {            
            Assert.Throws<ArgumentNullException>(() => Conn.CreateFolder("", ""));
            Assert.Throws<ArgumentNullException>(() => Conn.CreateFolder(_fake, ""));
            Assert.Throws<ArgumentNullException>(() => Conn.CreateFolder("", _fake));
            
            var path = _driveFolder;
            var folder = "CreateFolder_Folder1";
            Assert.DoesNotThrow(() => Conn.CreateFolder(path, folder));
            Assert.IsNotNull(Conn.GetFolder(path, folder));
            
            path = Path.Combine(_driveFolder, "CreateFolder_Folder2", "CreateFolder_Folder2.1");
            folder = "CreateFolder_Folder2.1.1";
            Assert.DoesNotThrow(() => Conn.CreateFolder(path, folder));
            Assert.IsNotNull(Conn.GetFolder(path, folder));
        }
       
        [Test]
        public void DeleteFolder()
        {            
            Assert.Throws<ArgumentNullException>(() => Conn.DeleteFolder("", ""));
            Assert.Throws<ArgumentNullException>(() => Conn.DeleteFolder(_fake, ""));
            Assert.Throws<ArgumentNullException>(() => Conn.DeleteFolder("", _fake));

            var folder = "DeleteFolder_Folder1";

            //Does not exist
            Assert.IsNull(Conn.GetFolder(_driveFolder, folder));
            Assert.DoesNotThrow(() => Conn.DeleteFolder(_driveFolder, folder));

            //Creating
            Assert.DoesNotThrow(() => Conn.CreateFolder(_driveFolder, folder));
            Assert.IsNotNull(Conn.GetFolder(_driveFolder, folder));

            //Destroying
            Assert.DoesNotThrow(() => Conn.DeleteFolder(_driveFolder, folder));
            Assert.IsNull(Conn.GetFolder(_driveFolder, folder));
        }

       
        [Test]
        public void Download()
        {            
            var filePath = this.GetSampleFile("10mb.test");
            Assert.Throws<ArgumentInvalidException>(() => Conn.Download(new Uri("http://www.google.com"), ""));
            Assert.Throws<ArgumentInvalidException>(() => Conn.Download(new Uri("http://www.google.com"), _fake));                
            Assert.Throws<ArgumentInvalidException>(() => Conn.Download(new Uri("https://drive.google.com/file/d/"), _fake));
            Assert.Throws<ArgumentNullException>(() => Conn.Download(new Uri("https://drive.google.com/file/d/0B1MVW1mFO2zmWjJMR2xSYUUwdG8/edit"), ""));
            Assert.AreEqual(filePath, Conn.Download(new Uri("https://drive.google.com/file/d/0B1MVW1mFO2zmWjJMR2xSYUUwdG8/edit"), this.SamplesPath));
            Assert.IsTrue(File.Exists(filePath));
        }
    }
}