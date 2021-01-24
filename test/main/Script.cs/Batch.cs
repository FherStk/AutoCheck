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
    public class Batch : Test
    {                
        [OneTimeSetUp]
        public virtual void StartUp() 
        {
            SamplesScriptFolder = GetSamplePath(Path.Combine("script", Name));            
        }
       
        [Test, Category("Batch"), Category("Local")]
        public void Script_BATCH_RUN_FOLDER_SINGLE()
        {               
            var dest = Path.Combine(TempScriptFolder, "test1");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);                                 

            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest, "nopass.zip"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.zip")));
            
            var s = new AutoCheck.Core.Script(GetSampleFile("batch_run_ok1.yaml"));                        
            Assert.AreEqual($"Running script batch_run_ok1 (v1.0.0.1):\r\nRunning on batch mode for {Path.GetFileName(dest)}:", s.Output.ToString());
            
            Directory.Delete(dest, true);
        }

        [Test, Category("Batch"), Category("Local")]
        public void Script_BATCH_RUN_FOLDER_MULTI()
        {     
            var dest =  Path.Combine(TempScriptFolder, "test2");         
            var dest1 = Path.Combine(dest, "folder1");
            var dest2 = Path.Combine(dest, "folder2");

            if(!Directory.Exists(dest1)) Directory.CreateDirectory(dest1);
            if(!Directory.Exists(dest2)) Directory.CreateDirectory(dest2);                                 

            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest1, "nopass.zip"));
            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest2, "nopass.zip"));

            Assert.IsTrue(File.Exists(GetSampleFile(dest1, "nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest2, "nopass.zip")));

            var s = new AutoCheck.Core.Script(GetSampleFile("batch_run_ok2.yaml"));  
            Assert.AreEqual($"Running script batch_run_ok2 (v1.0.0.1):\r\nRunning script batch_run_ok2 (v1.0.0.1) on batch mode for {Path.GetFileName(dest1)}:\r\nRunning script batch_run_ok2 (v1.0.0.1) on batch mode for {Path.GetFileName(dest2)}:", s.Output.ToString());

            Directory.Delete(dest, true);
        }

        [Test, Category("Batch"), Category("Local")]
        public void Script_BATCH_RUN_PATH()
        {               
            var dest =  Path.Combine(TempScriptFolder, "test3");         
            var dest1 = Path.Combine(dest, "folder1");
            var dest2 = Path.Combine(dest, "folder2");

            if(!Directory.Exists(dest1)) Directory.CreateDirectory(dest1);
            if(!Directory.Exists(dest2)) Directory.CreateDirectory(dest2);                                 

            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest1, "nopass.zip"));
            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest2, "nopass.zip"));

            Assert.IsTrue(File.Exists(GetSampleFile(dest1, "nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest2, "nopass.zip")));

            var s = new AutoCheck.Core.Script(GetSampleFile("batch_run_ok3.yaml"));    
            Assert.AreEqual($"Running script batch_run_ok3 (v1.0.0.1):\r\nRunning on batch mode:\r\nRunning on batch mode:", s.Output.ToString());

            Directory.Delete(dest, true);
        }

        [Test, Category("Batch"), Category("Local")]
        public void Script_BATCH_RUN_COMBO_INTERNAL()
        {               
            var dest =  Path.Combine(TempScriptFolder, "test4");         
            var dest1 = Path.Combine(dest, "folder1");
            var dest2 = Path.Combine(dest, "folder2");

            if(!Directory.Exists(dest1)) Directory.CreateDirectory(dest1);
            if(!Directory.Exists(dest2)) Directory.CreateDirectory(dest2);                                 

            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest1, "nopass.zip"));
            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest2, "nopass.zip"));

            Assert.IsTrue(File.Exists(GetSampleFile(dest1, "nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest2, "nopass.zip")));
        
            //NOTE: Folder order matters, folder order is as in the script, folders within a given path are collected sorted by name
            var s = new AutoCheck.Core.Script(GetSampleFile("batch_run_ok4.yaml"));            
            Assert.AreEqual($"Running script batch_run_ok4 (v1.0.0.1):\r\nRunning script batch_run_ok4 (v1.0.0.1) in batch mode for {Path.GetFileName(dest1)}:\r\nRunning script batch_run_ok4 (v1.0.0.1) in batch mode for {Path.GetFileName(dest2)}:\r\nRunning script batch_run_ok4 (v1.0.0.1) in batch mode for {Path.GetFileName(dest1)}:\r\nRunning script batch_run_ok4 (v1.0.0.1) in batch mode for {Path.GetFileName(dest2)}:", s.Output.ToString());
            
            Directory.Delete(dest, true); 
        }

        [Test, Category("Batch"), Category("Local")]
        public void Script_BATCH_RUN_COMBO_EXTERNAL()
        {               
            var dest =  Path.Combine(TempScriptFolder, "test5");         
            var dest1 = Path.Combine(dest, "folder1");
            var dest2 = Path.Combine(dest, "folder2");

            if(!Directory.Exists(dest1)) Directory.CreateDirectory(dest1);
            if(!Directory.Exists(dest2)) Directory.CreateDirectory(dest2);                                 

            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest1, "nopass.zip"));
            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest2, "nopass.zip"));

            Assert.IsTrue(File.Exists(GetSampleFile(dest1, "nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest2, "nopass.zip")));

            var s = new AutoCheck.Core.Script(GetSampleFile("batch_run_ok5.yaml"));            
            Assert.AreEqual($"Running script batch_run_ok5 (v1.0.0.1):\r\nRunning on batch mode for {Path.GetFileName(dest1)}:\r\nRunning on batch mode for {Path.GetFileName(dest2)}:\r\nRunning on batch mode for {Path.GetFileName(dest1)}:\r\nRunning on batch mode for {Path.GetFileName(dest2)}:", s.Output.ToString());
            
            Directory.Delete(dest, true);
        }

        [Test, Category("Batch"), Category("Local")]
        public void Script_BATCH_PRE_UNZIP()
        {               
            var dest =  Path.Combine(TempScriptFolder, "test6");         
            var dest1 = Path.Combine(dest, "folder1");
            var dest2 = Path.Combine(dest, "folder2");

            if(!Directory.Exists(dest1)) Directory.CreateDirectory(dest1);
            if(!Directory.Exists(dest2)) Directory.CreateDirectory(dest2);                                 

            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest1, "nopass.zip"));
            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest2, "nopass.zip"));

            Assert.IsTrue(File.Exists(GetSampleFile(dest1, "nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest2, "nopass.zip")));

            var s = new AutoCheck.Core.Script(GetSampleFile("batch_run_ok6.yaml"));            
            Assert.AreEqual($"Running script batch_run_ok6 (v1.0.0.1):\r\n   Extracting files at: {Path.GetFileName(dest1)}\r\n      Extracting the file nopass.zip... OK\r\n\r\n   Extracting files at: {Path.GetFileName(dest2)}\r\n      Extracting the file nopass.zip... OK\r\n\r\n   Starting the copy detector for PlainText:\r\n      Looking for potential copies within {Path.GetFileName(dest1)}... OK\r\n      Looking for potential copies within {Path.GetFileName(dest2)}... OK\r\n\r\nRunning on batch mode for {Path.GetFileName(dest1)}:\r\nRunning on batch mode for {Path.GetFileName(dest2)}:", s.Output.ToString());
            
            Directory.Delete(dest, true);
        }

        [Test, Category("Batch"), Category("Remote")]
        public void Script_BATCH_REMOTE_ECHO()
        {                           
            var s = new AutoCheck.Core.Script(GetSampleFile("batch_run_ok7.yaml"));            
            Assert.AreEqual("Running script batch_run_ok7 (v1.0.0.0):\r\nRunning on batch mode for localhost:\r\n   Question 1 [1 point]:\r\n      Testing echo command... OK\r\n\r\n   TOTAL SCORE: 10 / 10\r\nRunning on batch mode for 127.0.0.1:\r\n   Question 1 [1 point]:\r\n      Testing echo command... OK\r\n\r\n   TOTAL SCORE: 10 / 10", s.Output.ToString());
        }

        [Test, Category("Batch"), Category("Remote")]
        public void Script_BATCH_REMOTE_FOLDER()
        {                           
            var s = new AutoCheck.Core.Script(GetSampleFile("batch_run_ok8.yaml"));            
            Assert.AreEqual("Running script batch_run_ok8 (v1.0.0.0):\r\nRunning on batch mode for localhost:\r\n   Question 1 [1 point]:\r\n      Testing echo command... OK\r\n\r\n   TOTAL SCORE: 10 / 10\r\nRunning on batch mode for localhost:\r\n   Question 1 [1 point]:\r\n      Testing echo command... OK\r\n\r\n   TOTAL SCORE: 10 / 10", s.Output.ToString());
        }
    }
}