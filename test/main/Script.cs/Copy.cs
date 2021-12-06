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

using System.IO;
using NUnit.Framework;

namespace AutoCheck.Test
{    
    [Parallelizable(ParallelScope.All)]    
    public class Copy : Test
    { 
        //WARNING:  Parametrization not allowed, because the temp folder would be shared. 
        // 
        
        public Copy(): base("script"){
        }      

        [Test, Category("Copy"), Category("Local")]
        public void Script_COPY_PLAINTEXT_PATH_ISCOPY() 
        {   
            var dest =  Path.Combine(Path.GetDirectoryName(TempScriptFolder), "test1"); //the script will use this folder, so no random path can be used
            var dest1 = Path.Combine(dest, "folder1");
            var dest2 = Path.Combine(dest, "folder2");

            if(!Directory.Exists(dest1)) Directory.CreateDirectory(dest1);
            if(!Directory.Exists(dest2)) Directory.CreateDirectory(dest2);                                 

            dest1 = GetSampleFile(dest1, "sample1.txt");
            dest2 = GetSampleFile(dest2, "sample2.txt");
            File.Copy(GetSampleFile("plaintext", "lorem1.txt"), dest1);
            File.Copy(GetSampleFile("plaintext", "lorem1.txt"), dest2);
 
            Assert.IsTrue(File.Exists(dest1));
            Assert.IsTrue(File.Exists(dest2));

            //Getting folder and file for the current OS type in order to check the output
            dest1 = Path.Combine(Path.GetFileName(Path.GetDirectoryName(dest1)), Path.GetFileName(dest1));
            dest2 = Path.Combine(Path.GetFileName(Path.GetDirectoryName(dest2)), Path.GetFileName(dest2));

            var s = new AutoCheck.Core.Script(GetSampleFile("copy_plaintext_ok1.yaml")); 
            Assert.AreEqual($"Running script copy_plaintext_ok1 (v1.0.0.0):\r\n   Starting the copy detector for PLAINTEXT:\r\n      Looking for potential copies within folder1... OK\r\n      Looking for potential copies within folder2... OK\r\n\r\nRunning on batch mode:\r\n   Potential copy detected for {dest1}:\r\n      Match score with {dest2}... 100,00 % \r\n\r\n   Script execution aborted due potential copy detection.\r\n\r\nRunning script copy_plaintext_ok1 (v1.0.0.0):\r\n   Starting the copy detector for PLAINTEXT:\r\n      Looking for potential copies within folder1... OK\r\n      Looking for potential copies within folder2... OK\r\n\r\nRunning on batch mode:\r\n   Potential copy detected for {dest2}:\r\n      Match score with {dest1}... 100,00 % \r\n\r\n   Script execution aborted due potential copy detection.", s.Output.ToString());
            Directory.Delete(dest, true);
        }

        [Test, Category("Copy"), Category("Local")]
        public void Script_COPY_PLAINTEXT_FOLDERS_NOTCOPY()
        {               
            var dest =  Path.Combine(Path.GetDirectoryName(TempScriptFolder), "test2"); //the script will use this folder, so no random path can be used
            var dest1 = Path.Combine(dest, "folder1");
            var dest2 = Path.Combine(dest, "folder2");

            if(!Directory.Exists(dest1)) Directory.CreateDirectory(dest1);
            if(!Directory.Exists(dest2)) Directory.CreateDirectory(dest2);    

            dest1 = GetSampleFile(dest1, "sample1.txt");
            dest2 = GetSampleFile(dest2, "sample2.txt");
            File.Copy(GetSampleFile("plaintext", "lorem1.txt"), dest1);
            File.Copy(GetSampleFile("plaintext", "lorem2.txt"), dest2);                             
           
            Assert.IsTrue(File.Exists(dest1));
            Assert.IsTrue(File.Exists(dest2));

            //Getting folder and file for the current OS type in order to check the output
            dest1 = Path.Combine(Path.GetFileName(Path.GetDirectoryName(dest1)), Path.GetFileName(dest1));
            dest2 = Path.Combine(Path.GetFileName(Path.GetDirectoryName(dest2)), Path.GetFileName(dest2));

            var s = new AutoCheck.Core.Script(GetSampleFile("copy_plaintext_ok2.yaml")); 
            Assert.AreEqual($"Running script copy_plaintext_ok2 (v1.0.0.0):\r\n   Starting the copy detector for PLAINTEXT:\r\n      Looking for potential copies within folder1... OK\r\n      Looking for potential copies within folder2... OK\r\n\r\nRunning on batch mode for folder1:\r\n   No potential copy detected for {dest1}:\r\n      Match score with {dest2}... 58,99 % \r\n\r\n   No potential copy has been detected.\r\n\r\nRunning script copy_plaintext_ok2 (v1.0.0.0):\r\n   Starting the copy detector for PLAINTEXT:\r\n      Looking for potential copies within folder1... OK\r\n      Looking for potential copies within folder2... OK\r\n\r\nRunning on batch mode for folder2:\r\n   No potential copy detected for {dest2}:\r\n      Match score with {dest1}... 58,99 % \r\n\r\n   No potential copy has been detected.", s.Output.ToString());            
            Directory.Delete(dest, true);
        }
    }
}