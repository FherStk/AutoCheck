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

using System.IO;
using System.Collections.Generic;
using AutoCheck.Exceptions;
using NUnit.Framework;

namespace AutoCheck.Test.Core
{    
    [Parallelizable(ParallelScope.All)]    
    public class ScriptV2 : Test
    {
        private class TestScript : AutoCheck.Core.ScriptV2{
            public string Argument1 {get; private set;}
            public string Argument2 {get; private set;}
            public string Argument3 {get; private set;}
            
            public TestScript(string path): base(path){                        
            } 
        }
       
        [OneTimeSetUp]
        public void OneTimeSetUp() 
        {
            base.Setup("script");
            AutoCheck.Core.Output.Instance.Disable();
            
            //Fresh start needed!
            CleanUp();            
        }

        [OneTimeTearDown]
        public void OneTimeTearDown(){     
            //Clean before exit :)
            CleanUp();                    

            //Restore output
            AutoCheck.Core.Output.Instance.Enable();            
        }   

        private void CleanUp(){
            //Clean temp files
            var dir = Path.Combine(GetSamplePath("script"), "temp");
            if(Directory.Exists(dir)) Directory.Delete(dir, true);            

            //Clean databases            
            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok1", "postgres", "postgres"))
                if(psql.ExistsDataBase()) psql.DropDataBase();

            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok2", "postgres", "postgres"))
                if(psql.ExistsDataBase()) psql.DropDataBase();

            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok3", "postgres", "postgres"))
                if(psql.ExistsDataBase()) psql.DropDataBase();

            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "restoredb_ok5-dump1_sql", "postgres", "postgres"))
                if(psql.ExistsDataBase()) psql.DropDataBase();

            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "restoredb_ok5-dump2_sql", "postgres", "postgres"))
                if(psql.ExistsDataBase()) psql.DropDataBase(); 

            //Clean GDrive
            using(var gdrive = new AutoCheck.Connectors.GDrive(Path.Combine(AutoCheck.Core.Utils.ConfigFolder(), "gdrive_secret.json"), "porrino.fernando@elpuig.xeill.net")){                
                gdrive.DeleteFolder("\\AutoCheck\\test", "uploadgdrive_ok1");
                gdrive.DeleteFolder("\\AutoCheck\\test", "uploadgdrive_ok2");
                gdrive.DeleteFolder("\\AutoCheck\\test", "uploadgdrive_ok3");
            }
        }

        [Test]
        public void ParseVars_OK()
        {  
            var s = new TestScript(GetSampleFile("vars_ok1.yaml"));
            
            //Custom vars
            Assert.AreEqual("Fer", s.Vars["student_name"].ToString());
            Assert.AreEqual("Fer", s.Vars["student_var"].ToString());
            Assert.AreEqual("This is a test with value: Fer_Fer!", s.Vars["student_replace"].ToString());
            Assert.AreEqual("TEST_FOLDER", s.Vars["test_folder"].ToString());            
            Assert.AreEqual("FOLDER", s.Vars["folder_regex"].ToString());
            Assert.AreEqual("Fer FOLDER FOLDER", s.Vars["current_regex"].ToString());
            

            //Predefined vars
            Assert.AreEqual("vars_ok1", s.Vars["script_name"].ToString());            
            Assert.AreEqual(Path.GetDirectoryName(this.GetType().Assembly.Location) + "\\", s.Vars["current_folder"].ToString());            
            Assert.NotNull(s.Vars["current_date"].ToString());
        }

        [Test]
        public void ParseVars_KO()
        {  
            Assert.Throws<DocumentInvalidException>(() => new TestScript(GetSampleFile("vars_ko1.yaml")));
            Assert.Throws<VariableInvalidException>(() => new TestScript(GetSampleFile("vars_ko2.yaml")));
            Assert.Throws<RegexInvalidException>(() => new TestScript(GetSampleFile("vars_ko3.yaml")));
            Assert.Throws<VariableInvalidException>(() => new TestScript(GetSampleFile("vars_ko4.yaml")));
            Assert.Throws<VariableInvalidException>(() => new TestScript(GetSampleFile("vars_ko5.yaml")));
        }

        [Test]
        public void Extract_OK()
        { 
            //TEST 1: *.zip + no remove + no recursive 
            var dest = Path.Combine(GetSamplePath("script"), "temp", "extract");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);

            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest, "nopass.zip"));                        
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile(dest, "nopass.txt"))); 

            var s = new TestScript(GetSampleFile("extract_ok1.yaml"));
                        
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.txt")));
            File.Delete(GetSampleFile(dest, "nopass.txt"));

            //TEST 2: non-existing file + no remove + no recursive 
            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest, "nofound.zip"));                        
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nofound.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile(dest, "nopass.txt"))); 

            s = new TestScript(GetSampleFile("extract_ok2.yaml"));

            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nofound.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile(dest, "nopass.txt"))); 
            File.Delete(GetSampleFile(dest, "nofound.zip"));

            //TEST 3: [nopass.zip + no remove + no recursive ], [recursive/nopass.zip + remove + no recursive ]
            var rec = Path.Combine(dest, "recursive");
            if(!Directory.Exists(rec)) Directory.CreateDirectory(rec);

            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(rec, "nopass.zip"));     
            Assert.IsTrue(File.Exists(GetSampleFile(rec, "nopass.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile(rec, "nopass.txt")));
            
            s = new TestScript(GetSampleFile("extract_ok3.yaml"));

            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.txt"))); 
            Assert.IsFalse(File.Exists(GetSampleFile(rec, "nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile(rec, "nopass.txt")));
            File.Delete(GetSampleFile(rec, "nopass.txt"));
            File.Delete(GetSampleFile(dest, "nopass.txt"));

            //TEST 4: *.zip + remove + recursive 
            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(rec, "nopass.zip"));     
            s = new TestScript(GetSampleFile("extract_ok4.yaml"));

            Assert.IsFalse(File.Exists(GetSampleFile(dest, "nopass.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile(rec, "nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.txt")));
            Assert.IsTrue(File.Exists(GetSampleFile(rec, "nopass.txt")));
            File.Delete(GetSampleFile(dest, "nopass.txt"));
            File.Delete(GetSampleFile(rec, "nopass.txt"));

            //Clean
            Directory.Delete(dest, true);
        }

        //TODO: Extract_KO() testing something different to ZIP (RAR, TAR, GZ...)

        [Test]
        public void RestoreDB_OK()
        {  
            //TEST 1: *.sql + no remove + no override + no recursive
            var dest = Path.Combine(GetSamplePath("script"), "temp", "restore");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);

            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "dump.sql"));          
            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok1", "postgres", "postgres")){
                Assert.IsFalse(psql.ExistsDataBase());                
                var s = new TestScript(GetSampleFile("restoredb_ok1.yaml"));   
                
                Assert.IsTrue(psql.ExistsDataBase());
                Assert.IsTrue(File.Exists(GetSampleFile(dest, "dump.sql"))); 
                psql.DropDataBase();
            }  

            //TEST 2: *.sql + remove + no override  + no recursive
            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok2", "postgres", "postgres")){
                Assert.IsFalse(psql.ExistsDataBase());                
                var s = new TestScript(GetSampleFile("restoredb_ok2.yaml"));   
                
                Assert.IsTrue(psql.ExistsDataBase());
                Assert.IsFalse(File.Exists(GetSampleFile(dest, "dump.sql"))); 
            } 

            //TEST 3: 2 separated files + remove + override suceeded + no recursive
            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "override.sql"));
            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "nooverride.sql"));
            
            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok2", "postgres", "postgres")){
                Assert.IsTrue(psql.ExistsDataBase());
                Assert.AreEqual(10, psql.CountRegisters("test.work_history"));
                psql.Insert<short>("test.work_history", "id_employee", new Dictionary<string, object>(){{"id_employee", (short)999}, {"id_work", "MK_REP"}, {"id_department", (short)20}});
                Assert.AreEqual(11, psql.CountRegisters("test.work_history"));
            } 

            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok3", "postgres", "postgres")){
                Assert.IsFalse(psql.ExistsDataBase());                                
                var s = new TestScript(GetSampleFile("restoredb_ok3.yaml"));   
                
                Assert.IsTrue(psql.ExistsDataBase());
                Assert.IsTrue(File.Exists(GetSampleFile(dest, "nooverride.sql"))); 
                Assert.IsFalse(File.Exists(GetSampleFile(dest, "override.sql"))); 
            }

            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok2", "postgres", "postgres")){
                Assert.IsTrue(psql.ExistsDataBase());                                
                Assert.AreEqual(10, psql.CountRegisters("test.work_history"));
                psql.DropDataBase();                
            }   

            //TEST 4: nooverride.sql + remove + no override allowed + no recursive
            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok3", "postgres", "postgres")){
                Assert.True(psql.ExistsDataBase());                                
                
                Assert.AreEqual(10, psql.CountRegisters("test.work_history"));
                psql.Insert<short>("test.work_history", "id_employee", new Dictionary<string, object>(){{"id_employee", (short)999}, {"id_work", "MK_REP"}, {"id_department", (short)20}});
                Assert.AreEqual(11, psql.CountRegisters("test.work_history"));

                var s = new TestScript(GetSampleFile("restoredb_ok4.yaml"));   
                
                Assert.IsTrue(psql.ExistsDataBase());
                Assert.IsFalse(File.Exists(GetSampleFile(dest, "nooverride.sql"))); 
                Assert.AreEqual(11, psql.CountRegisters("test.work_history"));
                psql.DropDataBase();      
            }

            //TEST 5: *.sql + remove + no override allowed + recursive
            var rec = Path.Combine(dest, "recursive");
            if(!Directory.Exists(rec)) Directory.CreateDirectory(rec);

            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "dump1.sql"));
            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(rec, "dump2.sql"));
            
            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "restoredb_ok5-dump1_sql", "postgres", "postgres")){
                var s = new TestScript(GetSampleFile("restoredb_ok5.yaml"));                   

                Assert.IsTrue(psql.ExistsDataBase());
                Assert.IsFalse(File.Exists(GetSampleFile(dest, "dump1.sql"))); 
                psql.DropDataBase();
            }

            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "restoredb_ok5-dump2_sql", "postgres", "postgres")){
                Assert.IsTrue(psql.ExistsDataBase());
                Assert.IsFalse(File.Exists(GetSampleFile(rec, "dump2.sql"))); 
                psql.DropDataBase();
            }
        }

        //TODO: RestoreDB_KO() testing something different to PSQL (SQL Server, MySQL/MariaDB, Oracle...)
        
        [Test]
        public void UploadGDrive_OK()
        {  
            //TEST 1: * + no remove + upload + no link + no recursive
            var dest = Path.Combine(GetSamplePath("script"), "temp", "upload");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "uploaded.sql"));          

            var rec = Path.Combine(dest, "recursive");
            if(!Directory.Exists(rec)) Directory.CreateDirectory(rec);
            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(rec, "uploaded.zip"));          
            
            var remotePath = "\\AutoCheck\\test\\uploadgdrive_ok1";
            var remoteFile = "uploaded.sql";
            using(var gdrive = new AutoCheck.Connectors.GDrive(Path.Combine(AutoCheck.Core.Utils.ConfigFolder(), "gdrive_secret.json"), "porrino.fernando@elpuig.xeill.net")){                
                Assert.IsFalse(gdrive.ExistsFolder(remotePath));                
                var s = new TestScript(GetSampleFile("uploadgdrive_ok1.yaml"));   
                
                Assert.IsTrue(File.Exists(GetSampleFile(dest, remoteFile))); 
                Assert.IsTrue(gdrive.ExistsFile(remotePath, remoteFile));
            } 

            //TEST 2: * + remove + upload + no link + recursive
            remotePath = "\\AutoCheck\\test\\uploadgdrive_ok2";
            var remotePath2 = Path.Combine(remotePath, "recursive");
            var remoteFile2 = "uploaded.zip";
            using(var gdrive = new AutoCheck.Connectors.GDrive(Path.Combine(AutoCheck.Core.Utils.ConfigFolder(), "gdrive_secret.json"), "porrino.fernando@elpuig.xeill.net")){                
                Assert.IsFalse(gdrive.ExistsFolder(remotePath));

                var s = new TestScript(GetSampleFile("uploadgdrive_ok2.yaml"));   
                
                Assert.IsFalse(File.Exists(GetSampleFile(dest, remoteFile))); 
                Assert.IsFalse(Directory.Exists(rec));

                Assert.IsTrue(gdrive.ExistsFile(remotePath, remoteFile));
                Assert.IsTrue(gdrive.ExistsFolder(remotePath, "recursive"));
                Assert.IsTrue(gdrive.ExistsFile(remotePath2, remoteFile2));
            } 

            //TEST 3: *.txt + no remove + copy + link + no recursive
            remotePath = "\\AutoCheck\\test\\uploadgdrive_ok3";
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);    //removed by Test 2
            
            File.Copy(GetSampleFile("gdrive", "download.txt"), GetSampleFile(dest, "download.txt"));
            using(var gdrive = new AutoCheck.Connectors.GDrive(Path.Combine(AutoCheck.Core.Utils.ConfigFolder(), "gdrive_secret.json"), "porrino.fernando@elpuig.xeill.net")){                
                Assert.IsFalse(gdrive.ExistsFolder(remotePath));

                var s = new TestScript(GetSampleFile("uploadgdrive_ok3.yaml"));                               

                Assert.IsTrue(gdrive.ExistsFile(remotePath, "1MB.zip"));
                Assert.IsTrue(gdrive.ExistsFile(remotePath, "10MB.test"));
            } 
        }
    }
}
