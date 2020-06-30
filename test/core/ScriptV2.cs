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
using AutoCheck.Exceptions;
using NUnit.Framework;

namespace AutoCheck.Test.Core
{    
    [Parallelizable(ParallelScope.All)]    
    public class ScriptV2 : Test
    {
        private const string _user = "porrino.fernando@elpuig.xeill.net";
        private string _secret = AutoCheck.Core.Utils.ConfigFile("gdrive_secret.json");               
              
        protected override void CleanUp(){
            //Clean temp files
            var dir = Path.Combine(GetSamplePath("script"), "temp");
            if(Directory.Exists(dir)) Directory.Delete(dir, true);            

            //Clean databases            
            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok1", "postgres", "postgres"))
                if(psql.ExistsDataBase()) psql.DropDataBase();

            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok2", "postgres", "postgres"))
                if(psql.ExistsDataBase()) psql.DropDataBase();

            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok31", "postgres", "postgres"))
                if(psql.ExistsDataBase()) psql.DropDataBase();

            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok32", "postgres", "postgres"))
                if(psql.ExistsDataBase()) psql.DropDataBase();

            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok4", "postgres", "postgres"))
                if(psql.ExistsDataBase()) psql.DropDataBase();

            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "restoredb_ok5-dump1_sql", "postgres", "postgres"))
                if(psql.ExistsDataBase()) psql.DropDataBase();

            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "restoredb_ok5-dump2_sql", "postgres", "postgres"))
                if(psql.ExistsDataBase()) psql.DropDataBase(); 

            //Clean GDrive
            using(var gdrive = new AutoCheck.Connectors.GDrive(AutoCheck.Core.Utils.ConfigFile("gdrive_secret.json"), "porrino.fernando@elpuig.xeill.net")){                
                gdrive.DeleteFolder("\\AutoCheck\\test", "uploadgdrive_ok1");
                gdrive.DeleteFolder("\\AutoCheck\\test", "uploadgdrive_ok2");
                gdrive.DeleteFolder("\\AutoCheck\\test", "uploadgdrive_ok3");
            }
        }

        [Test]
        public void ParseVars_REGEX()
        {  
            var s = new AutoCheck.Core.ScriptV2(GetSampleFile("vars\\vars_ok1.yaml"));
            
            //Custom vars
            Assert.AreEqual("Fer", s.GetVar("student_name").ToString());
            Assert.AreEqual("Fer", s.GetVar("student_var").ToString());
            Assert.AreEqual("This is a test with value: Fer_Fer!", s.GetVar("student_replace").ToString());
            Assert.AreEqual("TEST_FOLDER", s.GetVar("test_folder").ToString());            
            Assert.AreEqual("FOLDER", s.GetVar("folder_regex").ToString());
            Assert.AreEqual("Fer FOLDER FOLDER", s.GetVar("current_regex").ToString());                        
        }

        [Test]
        public void ParseVars_DEFAULT()
        {  
            var file = GetSampleFile("vars\\vars_ok1.yaml");
            var s = new AutoCheck.Core.ScriptV2(GetSampleFile("vars\\vars_ok1.yaml"));
            
            //Default vars
            Assert.AreEqual(Path.GetFileNameWithoutExtension(file), s.ScriptName);            
            Assert.AreEqual(Path.GetFileName(file), s.CurrentFile);
            Assert.AreEqual(Path.GetDirectoryName(file), s.CurrentFolder);
            Assert.IsNull(s.Result);
            Assert.NotNull(s.Now);
        }

        [Test]
        public void ParseVars_TYPED()
        {                         
            var s = new AutoCheck.Core.ScriptV2(GetSampleFile("vars\\vars_ok2.yaml"));
            Assert.AreEqual("STRING", s.GetVar("string"));
            Assert.AreEqual(1, s.GetVar("int"));
            Assert.AreEqual(false, s.GetVar("bool"));
            Assert.AreEqual(33.5f, s.GetVar("float"));
        }

        [Test]
        public void ParseVars_DUPLICATED()
        {  
            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.ScriptV2(GetSampleFile("vars\\vars_ko1.yaml")));            
        }

        [Test]
        public void ParseVars_NOTEXISTS_SIMPLE()
        {  
            Assert.Throws<VariableNotFoundException>(() => new AutoCheck.Core.ScriptV2(GetSampleFile("vars\\vars_ko2.yaml")));           
        }

        [Test]
        public void ParseVars_REGEX_SIMPLE()
        {  
            Assert.Throws<RegexInvalidException>(() => new AutoCheck.Core.ScriptV2(GetSampleFile("vars\\vars_ko3.yaml")));
        }

        [Test]
        public void ParseVars_REGEX_NOVAR()
        {  
            Assert.Throws<VariableNotFoundException>(() => new AutoCheck.Core.ScriptV2(GetSampleFile("vars\\vars_ko4.yaml")));
        }

        [Test]
        public void ParseVars_REGEX_NOTEXISTS()
        {  
            Assert.Throws<VariableNotFoundException>(() => new AutoCheck.Core.ScriptV2(GetSampleFile("vars\\vars_ko5.yaml")));
        }

        //TODO: updatable vars and scope validation

        [Test]
        public void Extract_ZIP_NOREMOVE_NORECURSIVE()
        { 
            var dest = Path.Combine(GetSamplePath("script"), "temp", "extract", "test1");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);

            File.Copy(GetSampleFile("resources\\nopass.zip"), GetSampleFile(dest, "nopass.zip"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile(dest, "nopass.txt"))); 

            var s = new AutoCheck.Core.ScriptV2(GetSampleFile("extract\\extract_ok1.yaml"));
                        
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.txt")));
            
            File.Delete(GetSampleFile(dest, "nopass.zip"));
            File.Delete(GetSampleFile(dest, "nopass.txt"));
        }

        [Test]
        public void Extract_NONEXISTING_NOREMOVE_NORECURSIVE()
        { 
            var dest = Path.Combine(GetSamplePath("script"), "temp", "extract", "test2");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);           

            File.Copy(GetSampleFile("resources\\nopass.zip"), GetSampleFile(dest, "nofound.zip"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nofound.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile(dest, "nopass.txt"))); 

            var s = new AutoCheck.Core.ScriptV2(GetSampleFile("extract\\extract_ok2.yaml"));

            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nofound.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile(dest, "nopass.txt"))); 
            File.Delete(GetSampleFile(dest, "nofound.zip"));
        }

        [Test]
        public void Extract_SPECIFIC_BATCH()
        { 
            var dest = Path.Combine(GetSamplePath("script"), "temp", "extract", "test3");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);                     

            var rec = Path.Combine(dest, "recursive");
            if(!Directory.Exists(rec)) Directory.CreateDirectory(rec);

            File.Copy(GetSampleFile("resources\\nopass.zip"), GetSampleFile(dest, "nopass.zip"));
            File.Copy(GetSampleFile("resources\\nopass.zip"), GetSampleFile(rec, "nopass.zip"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile(dest, "nopass.txt")));
            Assert.IsTrue(File.Exists(GetSampleFile(rec, "nopass.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile(rec, "nopass.txt")));
            
            var s = new AutoCheck.Core.ScriptV2(GetSampleFile("extract\\extract_ok3.yaml"));

            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.txt"))); 
            Assert.IsFalse(File.Exists(GetSampleFile(rec, "nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile(rec, "nopass.txt")));
            File.Delete(GetSampleFile(rec, "nopass.txt"));
            File.Delete(GetSampleFile(dest, "nopass.txt"));
            File.Delete(GetSampleFile(dest, "nopass.zip"));
        }

        [Test]
        public void Extract_ZIP_REMOVE_RECURSIVE()
        { 
            var dest = Path.Combine(GetSamplePath("script"), "temp", "extract", "test4");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);                     

            var rec = Path.Combine(dest, "recursive");
            if(!Directory.Exists(rec)) Directory.CreateDirectory(rec);

            File.Copy(GetSampleFile("resources\\nopass.zip"), GetSampleFile(dest, "nopass.zip"));
            File.Copy(GetSampleFile("resources\\nopass.zip"), GetSampleFile(rec, "nopass.zip"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile(dest, "nopass.txt")));
            Assert.IsTrue(File.Exists(GetSampleFile(rec, "nopass.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile(rec, "nopass.txt")));
            
            var s = new AutoCheck.Core.ScriptV2(GetSampleFile("extract\\extract_ok4.yaml"));

            Assert.IsFalse(File.Exists(GetSampleFile(dest, "nopass.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile(rec, "nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.txt")));
            Assert.IsTrue(File.Exists(GetSampleFile(rec, "nopass.txt")));
            File.Delete(GetSampleFile(dest, "nopass.txt"));
            File.Delete(GetSampleFile(rec, "nopass.txt"));
        }

        //TODO: Extract_KO() testing something different to ZIP (RAR, TAR, GZ...)

        [Test] 
        public void RestoreDB_SQL_NOREMOVE_NOOVERRIDE_NORECURSIVE()
        {              
            var dest = Path.Combine(GetSamplePath("script"), "temp", "restore", "test1");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);

            File.Copy(GetSampleFile("resources\\dump.sql"), GetSampleFile(dest, "dump.sql"));          
            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok1", "postgres", "postgres")){
                Assert.IsFalse(psql.ExistsDataBase());                
                var s = new AutoCheck.Core.ScriptV2(GetSampleFile("restore_db\\restoredb_ok1.yaml"));   
                
                Assert.IsTrue(psql.ExistsDataBase());
                Assert.IsTrue(File.Exists(GetSampleFile(dest, "dump.sql"))); 
                psql.DropDataBase();
            }
            File.Delete(GetSampleFile(dest, "dump.sql"));
        }

        [Test] 
        public void RestoreDB_SQL_REMOVE_NOOVERRIDE_NORECURSIVE()
        {              
            var dest = Path.Combine(GetSamplePath("script"), "temp", "restore", "test2");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);

            File.Copy(GetSampleFile("resources\\dump.sql"), GetSampleFile(dest, "dump.sql"));                     
            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok2", "postgres", "postgres")){
                Assert.IsFalse(psql.ExistsDataBase());                
                var s = new AutoCheck.Core.ScriptV2(GetSampleFile("restore_db\\restoredb_ok2.yaml"));   
                
                Assert.IsTrue(psql.ExistsDataBase());
                Assert.IsFalse(File.Exists(GetSampleFile(dest, "dump.sql"))); 
                psql.DropDataBase();
            } 
        }

        [Test] 
        public void RestoreDB_SPECIFIC_BATCH()
        {              
            var dest = Path.Combine(GetSamplePath("script"), "temp", "restore", "test3");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
                        
            File.Copy(GetSampleFile("resources\\dump.sql"), GetSampleFile(dest, "dump.sql"));                         
            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok31", "postgres", "postgres")){
                Assert.IsFalse(psql.ExistsDataBase()); 
                var s = new AutoCheck.Core.ScriptV2(GetSampleFile("restore_db\\restoredb_ok3.1.yaml"));   //TODO: Should use a own file (not resue another test one...)   
                
                Assert.IsTrue(psql.ExistsDataBase());
                Assert.AreEqual(10, psql.CountRegisters("test.work_history"));
                psql.Insert<short>("test.work_history", "id_employee", new Dictionary<string, object>(){{"id_employee", (short)999}, {"id_work", "MK_REP"}, {"id_department", (short)20}});
                Assert.AreEqual(11, psql.CountRegisters("test.work_history"));
            } 

            File.Copy(GetSampleFile("resources\\dump.sql"), GetSampleFile(dest, "override.sql"));
            File.Copy(GetSampleFile("resources\\dump.sql"), GetSampleFile(dest, "nooverride.sql"));
            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok32", "postgres", "postgres")){
                Assert.IsFalse(psql.ExistsDataBase());                                
                var s = new AutoCheck.Core.ScriptV2(GetSampleFile("restore_db\\restoredb_ok3.2.yaml"));   
                
                Assert.IsTrue(psql.ExistsDataBase());
                Assert.IsTrue(File.Exists(GetSampleFile(dest, "nooverride.sql"))); 
                Assert.IsFalse(File.Exists(GetSampleFile(dest, "override.sql"))); 
                psql.DropDataBase();  
            }
            File.Delete(GetSampleFile(dest, "nooverride.sql"));

            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok31", "postgres", "postgres")){
                Assert.IsTrue(psql.ExistsDataBase());                                
                Assert.AreEqual(10, psql.CountRegisters("test.work_history"));
                psql.DropDataBase();                
            }              
        }

        [Test] 
        public void RestoreDB_SPECIFIC_REMOVE_NOOVERRIDE_NORECURSIVE()
        {              
            var dest = Path.Combine(GetSamplePath("script"), "temp", "restore", "test4");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);

            File.Copy(GetSampleFile("resources\\dump.sql"), GetSampleFile(dest, "nooverride.sql"));
            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok4", "postgres", "postgres")){
                Assert.IsFalse(psql.ExistsDataBase()); 
                var s = new AutoCheck.Core.ScriptV2(GetSampleFile("restore_db\\restoredb_ok4.yaml"));
                Assert.IsTrue(psql.ExistsDataBase());                                
                
                Assert.AreEqual(10, psql.CountRegisters("test.work_history"));
                psql.Insert<short>("test.work_history", "id_employee", new Dictionary<string, object>(){{"id_employee", (short)999}, {"id_work", "MK_REP"}, {"id_department", (short)20}});
                Assert.AreEqual(11, psql.CountRegisters("test.work_history"));

                s = new AutoCheck.Core.ScriptV2(GetSampleFile("restore_db\\restoredb_ok4.yaml"));   
                
                Assert.IsTrue(psql.ExistsDataBase());
                Assert.IsFalse(File.Exists(GetSampleFile(dest, "nooverride.sql"))); 
                Assert.AreEqual(11, psql.CountRegisters("test.work_history"));
                psql.DropDataBase();      
            }           
        }

        [Test] 
        public void RestoreDB_SQL_REMOVE_NOOVERRIDE_RECURSIVE()
        {              
            var dest = Path.Combine(GetSamplePath("script"), "temp", "restore", "test5");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
           
            var rec = Path.Combine(dest, "recursive");
            if(!Directory.Exists(rec)) Directory.CreateDirectory(rec);

            File.Copy(GetSampleFile("resources\\dump.sql"), GetSampleFile(dest, "dump1.sql"));
            File.Copy(GetSampleFile("resources\\dump.sql"), GetSampleFile(rec, "dump2.sql"));
            
            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "restoredb_ok5-dump1_sql", "postgres", "postgres")){
                Assert.IsFalse(psql.ExistsDataBase());
                var s = new AutoCheck.Core.ScriptV2(GetSampleFile("restore_db\\restoredb_ok5.yaml"));                   

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
        public void UploadGDrive_NOREMOVE_UPLOAD_NOLINK_NORECURSIVE()
        {  
            var dest = Path.Combine(GetSamplePath("script"), "temp", "upload", "test1");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
            File.Copy(GetSampleFile("resources\\dump.sql"), GetSampleFile(dest, "uploaded.sql"));                    
            
            var remotePath = "\\AutoCheck\\test\\uploadgdrive_ok1";
            var remoteFile = "uploaded.sql";
            using(var gdrive = new AutoCheck.Connectors.GDrive(_secret, _user)){
                Assert.IsFalse(gdrive.ExistsFolder(remotePath));                
                var s = new AutoCheck.Core.ScriptV2(GetSampleFile("upload_gdrive\\uploadgdrive_ok1.yaml"));   
                
                Assert.IsTrue(File.Exists(GetSampleFile(dest, remoteFile))); 
                Assert.IsTrue(gdrive.ExistsFile(remotePath, remoteFile));
            } 
            File.Delete(GetSampleFile(dest, remoteFile));
        }

        [Test]
        public void UploadGDrive_REMOVE_UPLOAD_NOLINK_RECURSIVE()
        {  
            var dest = Path.Combine(GetSamplePath("script"), "temp", "upload", "test2");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
            File.Copy(GetSampleFile("resources\\dump.sql"), GetSampleFile(dest, "uploaded.sql"));          

            var rec = Path.Combine(dest, "recursive");
            if(!Directory.Exists(rec)) Directory.CreateDirectory(rec);
            File.Copy(GetSampleFile("resources\\nopass.zip"), GetSampleFile(rec, "uploaded.zip"));                                 
            
            var remotePath = "\\AutoCheck\\test\\uploadgdrive_ok2";
            var remotePath2 = Path.Combine(remotePath, "recursive");
            var remoteFile = "uploaded.sql";
            var remoteFile2 = "uploaded.zip";
            using(var gdrive = new AutoCheck.Connectors.GDrive(_secret, _user)){
                Assert.IsFalse(gdrive.ExistsFolder(remotePath));

                var s = new AutoCheck.Core.ScriptV2(GetSampleFile("upload_gdrive\\uploadgdrive_ok2.yaml"));   
                
                Assert.IsFalse(File.Exists(GetSampleFile(dest, remoteFile))); 
                Assert.IsFalse(Directory.Exists(rec));

                Assert.IsTrue(gdrive.ExistsFile(remotePath, remoteFile));
                Assert.IsTrue(gdrive.ExistsFolder(remotePath, "recursive"));
                Assert.IsTrue(gdrive.ExistsFile(remotePath2, remoteFile2));
            } 
        }

         [Test]
        public void UploadGDrive_NOREMOVE_COPY_LINK_NORECURSIVE()
        {  
            var dest = Path.Combine(GetSamplePath("script"), "temp", "upload", "test3");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
            File.Copy(GetSampleFile("resources\\dump.sql"), GetSampleFile(dest, "uploaded.sql"));          
            
            var remotePath = "\\AutoCheck\\test\\uploadgdrive_ok3";
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);  
            
            File.Copy(GetSampleFile("gdrive", "download.txt"), GetSampleFile(dest, "download.txt"));
            using(var gdrive = new AutoCheck.Connectors.GDrive(_secret, _user)){
                Assert.IsFalse(gdrive.ExistsFolder(remotePath));

                var s = new AutoCheck.Core.ScriptV2(GetSampleFile("upload_gdrive\\uploadgdrive_ok3.yaml"));                               

                Assert.IsTrue(gdrive.ExistsFile(remotePath, "1MB.zip"));
                Assert.IsTrue(gdrive.ExistsFile(remotePath, "10MB.test"));
            }

            File.Delete(GetSampleFile(dest, "uploaded.sql")) ;
            File.Delete(GetSampleFile(dest, "download.txt")) ;
        }

        //TODO: UploadGDrive_KO() testing something unable to parse (read the PDF content for example, it will be supported in a near future, but not right now) or upload

        [Test]
        public void ParseBody_CONNECTOR_EMPTY()
        {  
            Assert.DoesNotThrow(() => new AutoCheck.Core.ScriptV2(GetSampleFile("body\\connector\\connector_ok1.yaml")));                                         
        }

        [Test]
        public void ParseBody_CONNECTOR_INLINE_ARGS()
        {              
            Assert.DoesNotThrow(() => new AutoCheck.Core.ScriptV2(GetSampleFile("body\\connector\\connector_ok2.yaml")));            
        }

        [Test]
        public void ParseBody_CONNECTOR_TYPED_ARGS()
        {                          
            Assert.DoesNotThrow(() => new AutoCheck.Core.ScriptV2(GetSampleFile("body\\connector\\connector_ok3.yaml")));                                    
        }

        [Test]
        public void ParseBody_CONNECTOR_MULTI_LOAD()
        {                          
            Assert.DoesNotThrow(() => new AutoCheck.Core.ScriptV2(GetSampleFile("body\\connector\\connector_ok4.yaml")));                          
        }

        [Test]
        public void ParseBody_CONNECTOR_IMPLICIT_INVALID_INLINE_ARGS()
        {  
            Assert.Throws<ArgumentInvalidException>(() => new AutoCheck.Core.ScriptV2(GetSampleFile("body\\connector\\connector_ko1.yaml")));           
        }

        [Test]
        public void ParseBody_CONNECTOR_EXPLICIT_INVALID_INLINE_ARGS()
        {  
            Assert.Throws<ArgumentInvalidException>(() => new AutoCheck.Core.ScriptV2(GetSampleFile("body\\connector\\connector_ko2.yaml")));
        }

        [Test]
        public void ParseBody_CONNECTOR_EXPLICIT_INVALID_TYPED_ARGS()
        {  
            Assert.Throws<ArgumentInvalidException>(() => new AutoCheck.Core.ScriptV2(GetSampleFile("body\\connector\\connector_ko3.yaml")));                           
        }

        [Test]
        public void ParseBody_RUN_ECHO()
        {  
            var s = new AutoCheck.Core.ScriptV2(GetSampleFile("body\\run\\run_ok1.yaml"));
            Assert.AreEqual("TEST", s.Result.TrimEnd('\r', '\n')); //on Windows an end breakline is added when calling ECHO   
        }

        [Test]
        public void ParseBody_RUN_FIND()
        {              
            var s = new AutoCheck.Core.ScriptV2(GetSampleFile("body\\run\\run_ok2.yaml"));
            Assert.AreEqual(string.Empty, s.Result); //on Windows an end breakline is added when calling ECHO  
 
        }

        [Test]
        public void ParseBody_RUN_EMPTY()
        {  
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.ScriptV2(GetSampleFile("body\\run\\run_ko1.yaml")));
        }

        [Test]
        public void ParseBody_RUN_INVALID_TYPED_ARGS()
        {  
            Assert.Throws<ArgumentInvalidException>(() => new AutoCheck.Core.ScriptV2(GetSampleFile("body\\run\\run_ko2.yaml")));            
        }
    }
}
