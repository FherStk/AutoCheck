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
using System.Collections.Generic;
using NUnit.Framework;
using AutoCheck.Core;

namespace AutoCheck.Test.Checkers
{
    [Parallelizable(ParallelScope.All)]    
    public class GDrive : Core.Test
    {        
        protected const string _driveFolder = "\\AutoCheck\\test\\Connectors.GDrive";
        protected string _user = AutoCheck.Core.Utils.ConfigFile("gdrive_account.txt");
        protected string _secret = AutoCheck.Core.Utils.ConfigFile("gdrive_secret.json");

        private AutoCheck.Checkers.GDrive Chk;

        [OneTimeSetUp]
        public new void OneTimeSetUp() 
        {      
            //TODO: Test if both (this and base) executes in the correct order.   
            Chk = new AutoCheck.Checkers.GDrive(_user, _secret);            
        }

        [OneTimeTearDown]
        public new void OneTimeTearDown(){   
            //TODO: Test if both (this and base) executes in the correct order.    
            Chk.Dispose();
        }

        [Test]
        public void Constructor()
        {
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Checkers.GDrive(null,string.Empty));
            Assert.Throws<FileNotFoundException>(() => new AutoCheck.Checkers.GDrive(this.GetSampleFile(_FAKE),string.Empty));
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Checkers.GDrive(AutoCheck.Core.Utils.ConfigFile("gdrive_secret.json"), ""));
            Assert.DoesNotThrow(() => new AutoCheck.Checkers.GDrive(_user, _secret));
        }   

        [Test]
        public void CheckIfFolderExists()
        {    
            var path = Path.GetDirectoryName(_driveFolder);
            var folder = Path.GetFileName(_driveFolder);

            Assert.AreEqual(new List<string>(), this.Chk.CheckIfFolderExists(path, folder, false));
            Assert.AreNotEqual(new List<string>(), this.Chk.CheckIfFolderExists(path, _FAKE, false));

            Assert.AreEqual(new List<string>(), this.Chk.CheckIfFolderExists(path, folder, true));
            Assert.AreNotEqual(new List<string>(), this.Chk.CheckIfFolderExists(path, _FAKE, true));

            Assert.AreEqual(new List<string>(), this.Chk.CheckIfFolderExists("\\", folder, true));
            Assert.AreNotEqual(new List<string>(), this.Chk.CheckIfFolderExists("\\", _FAKE, true));

            path = Path.GetDirectoryName(path);
            Assert.AreEqual(new List<string>(), this.Chk.CheckIfFolderExists(path, folder, true));
            Assert.AreNotEqual(new List<string>(), this.Chk.CheckIfFolderExists(path, _FAKE, true));
        } 

        [Test]
        public void CheckIfFileExists()
        {    
            var path = _driveFolder;
            var file = "file.txt";

            Assert.AreEqual(new List<string>(), this.Chk.CheckIfFileExists(path, file, false));
            Assert.AreNotEqual(new List<string>(), this.Chk.CheckIfFileExists(path, _FAKE, false));

            Assert.AreEqual(new List<string>(), this.Chk.CheckIfFileExists(path, file, true));
            Assert.AreNotEqual(new List<string>(), this.Chk.CheckIfFileExists(path, _FAKE, true));

            Assert.AreEqual(new List<string>(), this.Chk.CheckIfFileExists("\\", file, true));
            Assert.AreNotEqual(new List<string>(), this.Chk.CheckIfFileExists("\\", _FAKE, true));

            path = Path.GetDirectoryName(path);
            Assert.AreEqual(new List<string>(), this.Chk.CheckIfFileExists(path, file, true));
            Assert.AreNotEqual(new List<string>(), this.Chk.CheckIfFileExists(path, _FAKE, true));      
        }

        [Test]
        public void CheckIfFoldersMatchesAmount()
        {    
            var path = Path.Combine(_driveFolder, "Test Folder 1");
            Assert.AreEqual(new List<string>(), this.Chk.CheckIfFoldersMatchesAmount(path, 2, false));
            Assert.AreNotEqual(new List<string>(), this.Chk.CheckIfFoldersMatchesAmount(path, 2, false, Operator.GREATER));
            Assert.AreEqual(new List<string>(), this.Chk.CheckIfFoldersMatchesAmount(path, 2, false, Operator.GREATEREQUALS));
            Assert.AreNotEqual(new List<string>(), this.Chk.CheckIfFoldersMatchesAmount(path, 2, false, Operator.LOWER));
            Assert.AreEqual(new List<string>(), this.Chk.CheckIfFoldersMatchesAmount(path, 2, false, Operator.LOWEREQUALS));
            
            Assert.AreEqual(new List<string>(), this.Chk.CheckIfFoldersMatchesAmount(path, 4, true));
            Assert.AreNotEqual(new List<string>(), this.Chk.CheckIfFoldersMatchesAmount(path, 4, true, Operator.GREATER));
            Assert.AreEqual(new List<string>(), this.Chk.CheckIfFoldersMatchesAmount(path, 4, true, Operator.GREATEREQUALS));
            Assert.AreNotEqual(new List<string>(), this.Chk.CheckIfFoldersMatchesAmount(path, 4, true, Operator.LOWER));
            Assert.AreEqual(new List<string>(), this.Chk.CheckIfFoldersMatchesAmount(path, 4, true, Operator.LOWEREQUALS));
        } 

        [Test]
        public void CheckIfFilesMatchesAmount()
        {    
            var path = Path.Combine(_driveFolder, "Test Folder 1");
            Assert.AreEqual(new List<string>(), this.Chk.CheckIfFilesMatchesAmount(path, 1, false));
            Assert.AreNotEqual(new List<string>(), this.Chk.CheckIfFilesMatchesAmount(path, 1, false, Operator.GREATER));
            Assert.AreEqual(new List<string>(), this.Chk.CheckIfFilesMatchesAmount(path, 1, false, Operator.GREATEREQUALS));
            Assert.AreNotEqual(new List<string>(), this.Chk.CheckIfFilesMatchesAmount(path, 1, false, Operator.LOWER));
            Assert.AreEqual(new List<string>(), this.Chk.CheckIfFilesMatchesAmount(path, 1, false, Operator.LOWEREQUALS));
            
            Assert.AreEqual(new List<string>(), this.Chk.CheckIfFilesMatchesAmount(path, 3, true));
            Assert.AreNotEqual(new List<string>(), this.Chk.CheckIfFilesMatchesAmount(path, 3, true, Operator.GREATER));
            Assert.AreEqual(new List<string>(), this.Chk.CheckIfFilesMatchesAmount(path, 3, true, Operator.GREATEREQUALS));
            Assert.AreNotEqual(new List<string>(), this.Chk.CheckIfFilesMatchesAmount(path, 3, true, Operator.LOWER));
            Assert.AreEqual(new List<string>(), this.Chk.CheckIfFilesMatchesAmount(path, 3, true, Operator.LOWEREQUALS));
        }                                          
    }
}