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
        public void ParseVars_DEFAULT_VARS()
        {  
           Assert.DoesNotThrow(() => new AutoCheck.Core.ScriptV2(GetSampleFile("vars\\vars_ok1.yaml")));                 
        }
        
        [Test]
        public void ParseVars_COMPUTED_REGEX()
        {  
            Assert.DoesNotThrow(() => new AutoCheck.Core.ScriptV2(GetSampleFile("vars\\vars_ok2.yaml")));                                           
        }
 
        [Test]
        public void ParseVars_TYPED_SIMPLE()
        {                         
            Assert.DoesNotThrow(() => new AutoCheck.Core.ScriptV2(GetSampleFile("vars\\vars_ok3.yaml")));              
        }

        [Test]
        public void ParseVars_SCOPE_LEVEL1()
        {                         
            Assert.DoesNotThrow(() => new AutoCheck.Core.ScriptV2(GetSampleFile("vars\\vars_ok4.yaml")));              
        }
        
        //  [Test]
        // public void ParseVars_SCOPE_LEVEL2()
        // {                         
                //TODO: check vars scope including questions, so "undone" changes can be check
        //     Assert.DoesNotThrow(() => new AutoCheck.Core.ScriptV2(GetSampleFile("vars\\vars_ok5.yaml")));              
        // }

        [Test]
        public void ParseVars_INVALID_DUPLICATED()
        {  
            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.ScriptV2(GetSampleFile("vars\\vars_ko1.yaml")));            
        }

        [Test]
        public void ParseVars_NOTEXISTS_SIMPLE()
        {  
            Assert.Throws<VariableNotFoundException>(() => new AutoCheck.Core.ScriptV2(GetSampleFile("vars\\vars_ko2.yaml")));           
        }
 
        [Test]
        public void ParseVars_REGEX_NOTAPPLIED()
        {  
            Assert.Throws<RegexInvalidException>(() => new AutoCheck.Core.ScriptV2(GetSampleFile("vars\\vars_ko3.yaml")));
        }

        [Test]
        public void ParseVars_REGEX_NOVARNAME()
        {  
            Assert.Throws<VariableNotFoundException>(() => new AutoCheck.Core.ScriptV2(GetSampleFile("vars\\vars_ko4.yaml")));
        }

        [Test]
        public void ParseVars_REGEX_NOTEXISTS()
        {  
            Assert.Throws<VariableNotFoundException>(() => new AutoCheck.Core.ScriptV2(GetSampleFile("vars\\vars_ko5.yaml")));
        }

        [Test]
        public void ParseVars_SCOPE_NOTEXISTS()
        {  
            Assert.Throws<VariableNotFoundException>(() => new AutoCheck.Core.ScriptV2(GetSampleFile("vars\\vars_ko6.yaml")));
        }

        [Test]
        public void Extract_ZIP_NOREMOVE_NORECURSIVE()
        { 
            var dest = Path.Combine(GetSamplePath("script"), "temp", "extract", "test1");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);

            File.Copy(GetSampleFile("resources\\nopass.zip"), GetSampleFile(dest, "nopass.zip"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile(dest, "nopass.txt"))); 

            var s = new AutoCheck.Core.ScriptV2(GetSampleFile("pre\\extract\\extract_ok1.yaml"));
                        
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.txt")));
            
            Directory.Delete(dest, true);
        }

        [Test]
        public void Extract_NONEXISTING_NOREMOVE_NORECURSIVE()
        { 
            var dest = Path.Combine(GetSamplePath("script"), "temp", "extract", "test2");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);           

            File.Copy(GetSampleFile("resources\\nopass.zip"), GetSampleFile(dest, "nofound.zip"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nofound.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile(dest, "nopass.txt"))); 

            var s = new AutoCheck.Core.ScriptV2(GetSampleFile("pre\\extract\\extract_ok2.yaml"));

            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nofound.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile(dest, "nopass.txt"))); 
           
            Directory.Delete(dest, true);
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
            
            var s = new AutoCheck.Core.ScriptV2(GetSampleFile("pre\\extract\\extract_ok3.yaml"));

            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.txt"))); 
            Assert.IsFalse(File.Exists(GetSampleFile(rec, "nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile(rec, "nopass.txt")));
            
            Directory.Delete(dest, true);
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
            
            var s = new AutoCheck.Core.ScriptV2(GetSampleFile("pre\\extract\\extract_ok4.yaml"));

            Assert.IsFalse(File.Exists(GetSampleFile(dest, "nopass.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile(rec, "nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.txt")));
            Assert.IsTrue(File.Exists(GetSampleFile(rec, "nopass.txt")));
            
            Directory.Delete(dest, true);
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
                var s = new AutoCheck.Core.ScriptV2(GetSampleFile("pre\\restore_db\\restoredb_ok1.yaml"));   
                
                Assert.IsTrue(psql.ExistsDataBase());
                Assert.IsTrue(File.Exists(GetSampleFile(dest, "dump.sql"))); 
                psql.DropDataBase();
            }
            
            Directory.Delete(dest, true);
        }

        [Test] 
        public void RestoreDB_SQL_REMOVE_NOOVERRIDE_NORECURSIVE()
        {              
            var dest = Path.Combine(GetSamplePath("script"), "temp", "restore", "test2");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);

            File.Copy(GetSampleFile("resources\\dump.sql"), GetSampleFile(dest, "dump.sql"));                     
            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok2", "postgres", "postgres")){
                Assert.IsFalse(psql.ExistsDataBase());                
                var s = new AutoCheck.Core.ScriptV2(GetSampleFile("pre\\restore_db\\restoredb_ok2.yaml"));   
                
                Assert.IsTrue(psql.ExistsDataBase());
                Assert.IsFalse(File.Exists(GetSampleFile(dest, "dump.sql"))); 
                psql.DropDataBase();
            } 

            Directory.Delete(dest, true);
        }

        [Test] 
        public void RestoreDB_SPECIFIC_BATCH()
        {              
            var dest = Path.Combine(GetSamplePath("script"), "temp", "restore", "test3");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
                        
            File.Copy(GetSampleFile("resources\\dump.sql"), GetSampleFile(dest, "dump.sql"));                         
            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok31", "postgres", "postgres")){
                Assert.IsFalse(psql.ExistsDataBase()); 
                var s = new AutoCheck.Core.ScriptV2(GetSampleFile("pre\\restore_db\\restoredb_ok3.1.yaml"));   //TODO: Should use a own file (not resue another test one...)   
                
                Assert.IsTrue(psql.ExistsDataBase());
                Assert.AreEqual(10, psql.CountRegisters("test.work_history"));
                psql.Insert<short>("test.work_history", "id_employee", new Dictionary<string, object>(){{"id_employee", (short)999}, {"id_work", "MK_REP"}, {"id_department", (short)20}});
                Assert.AreEqual(11, psql.CountRegisters("test.work_history"));
            } 

            File.Copy(GetSampleFile("resources\\dump.sql"), GetSampleFile(dest, "override.sql"));
            File.Copy(GetSampleFile("resources\\dump.sql"), GetSampleFile(dest, "nooverride.sql"));
            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok32", "postgres", "postgres")){
                Assert.IsFalse(psql.ExistsDataBase());                                
                var s = new AutoCheck.Core.ScriptV2(GetSampleFile("pre\\restore_db\\restoredb_ok3.2.yaml"));   
                
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

            Directory.Delete(dest, true);             
        }

        [Test] 
        public void RestoreDB_SPECIFIC_REMOVE_NOOVERRIDE_NORECURSIVE()
        {              
            var dest = Path.Combine(GetSamplePath("script"), "temp", "restore", "test4");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);

            File.Copy(GetSampleFile("resources\\dump.sql"), GetSampleFile(dest, "nooverride.sql"));
            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok4", "postgres", "postgres")){
                Assert.IsFalse(psql.ExistsDataBase()); 
                var s = new AutoCheck.Core.ScriptV2(GetSampleFile("pre\\restore_db\\restoredb_ok4.yaml"));
                Assert.IsTrue(psql.ExistsDataBase());                                
                 
                Assert.AreEqual(10, psql.CountRegisters("test.work_history"));
                psql.Insert<short>("test.work_history", "id_employee", new Dictionary<string, object>(){{"id_employee", (short)999}, {"id_work", "MK_REP"}, {"id_department", (short)20}});
                Assert.AreEqual(11, psql.CountRegisters("test.work_history"));

                s = new AutoCheck.Core.ScriptV2(GetSampleFile("pre\\restore_db\\restoredb_ok4.yaml"));   
                
                Assert.IsTrue(psql.ExistsDataBase());
                Assert.IsFalse(File.Exists(GetSampleFile(dest, "nooverride.sql"))); 
                Assert.AreEqual(11, psql.CountRegisters("test.work_history"));
                psql.DropDataBase();      
            } 

            Directory.Delete(dest, true);          
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
                var s = new AutoCheck.Core.ScriptV2(GetSampleFile("pre\\restore_db\\restoredb_ok5.yaml"));                   

                Assert.IsTrue(psql.ExistsDataBase());
                Assert.IsFalse(File.Exists(GetSampleFile(dest, "dump1.sql"))); 
                psql.DropDataBase();
            }

            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "restoredb_ok5-dump2_sql", "postgres", "postgres")){
                Assert.IsTrue(psql.ExistsDataBase());
                Assert.IsFalse(File.Exists(GetSampleFile(rec, "dump2.sql"))); 
                psql.DropDataBase();
            }

            Directory.Delete(dest, true);
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
                var s = new AutoCheck.Core.ScriptV2(GetSampleFile("pre\\upload_gdrive\\uploadgdrive_ok1.yaml"));   
                
                Assert.IsTrue(File.Exists(GetSampleFile(dest, remoteFile))); 
                Assert.IsTrue(gdrive.ExistsFile(remotePath, remoteFile));
            } 
            
            Directory.Delete(dest, true);
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

                var s = new AutoCheck.Core.ScriptV2(GetSampleFile("pre\\upload_gdrive\\uploadgdrive_ok2.yaml"));   
                
                Assert.IsFalse(File.Exists(GetSampleFile(dest, remoteFile))); 
                Assert.IsFalse(Directory.Exists(rec));

                Assert.IsTrue(gdrive.ExistsFile(remotePath, remoteFile));
                Assert.IsTrue(gdrive.ExistsFolder(remotePath, "recursive"));
                Assert.IsTrue(gdrive.ExistsFile(remotePath2, remoteFile2));
            }
            
            //No need to delete, the script does it
            //Directory.Delete(dest, true);
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

                var s = new AutoCheck.Core.ScriptV2(GetSampleFile("pre\\upload_gdrive\\uploadgdrive_ok3.yaml"));                               

                Assert.IsTrue(gdrive.ExistsFile(remotePath, "1MB.zip"));
                Assert.IsTrue(gdrive.ExistsFile(remotePath, "10MB.test"));
            }

            Directory.Delete(dest, true);
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
        public void ParseBody_CONNECTOR_REMOTE_IP()
        {                          
            Assert.DoesNotThrow(() => new AutoCheck.Core.ScriptV2(GetSampleFile("body\\connector\\connector_ok5.yaml")));                          
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
            Assert.DoesNotThrow(() => new AutoCheck.Core.ScriptV2(GetSampleFile("body\\run\\run_ok1.yaml")));
        }
 
        [Test]
        public void ParseBody_RUN_FIND()
        {          
            var s = new AutoCheck.Core.ScriptV2(GetSampleFile("body\\run\\run_ok2.yaml"));         
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

        [Test]
        public void ParseBody_QUESTION_DEFAULT_SINGLE_ECHO()
        {                                      
            var s = new AutoCheck.Core.ScriptV2(GetSampleFile("body\\question\\question_ok1.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Question 1 [1 point]:\r\n   Running echo... OK\r\n\r\nTOTAL SCORE: 10", log);
        }

        [Test]
        public void ParseBody_QUESTION_DEFAULT_MULTI_ECHO()
        {              
            var s = new AutoCheck.Core.ScriptV2(GetSampleFile("body\\question\\question_ok2.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Question 1 [1 point]:\r\n   Running echo (1/2)... OK\r\n   Running echo (2/2)... OK\r\n\r\nTOTAL SCORE: 10", log);
        }

        [Test]
        public void ParseBody_QUESTION_DEFAULT_MULTI_METHODS()
        {              
            var s = new AutoCheck.Core.ScriptV2(GetSampleFile("body\\question\\question_ok7.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Question 1 [1 point]:\r\n   Checking files... OK\r\n   Getting files... OK\r\n\r\nTOTAL SCORE: 10", log);
        }

        [Test]
        public void ParseBody_QUESTION_BATCH_MULTI_ECHO()
        {              
            var s = new AutoCheck.Core.ScriptV2(GetSampleFile("body\\question\\question_ok3.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Question 1 [1 point]:\r\n   Running echo (1/2)... OK\r\n   Running echo (2/2)... OK\r\n\r\nQuestion 2 [1 point]:\r\n   Running echo (1/2)... OK\r\n   Running echo (2/2)... ERROR:\n      -Expected -> Wanted fail!; Found -> This is NOT OK\r\n\r\nTOTAL SCORE: 5", log);
        } 

        [Test]
        public void ParseBody_QUESTION_BATCH_MULTI_MESSAGES()
        {              
            var s = new AutoCheck.Core.ScriptV2(GetSampleFile("body\\question\\question_ok4.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Question 1 [1 point]:\r\n   Running echo (1/2)... OK\r\n   Running echo (2/2)... ERROR:\n      -Expected -> Wanted fail!; Found -> Bye!\r\n\r\nQuestion 2 [1 point]:\r\n   Running echo (1/2)... GREAT!\r\n   Running echo (2/2)... SO BAD!:\n      -Expected -> Wanted fail!; Found -> This is NOT OK\r\n\r\nTOTAL SCORE: 0", log);
        }

        [Test] 
        public void ParseBody_QUESTION_BATCH_MULTI_SCORE()
        {              
            var s = new AutoCheck.Core.ScriptV2(GetSampleFile("body\\question\\question_ok5.yaml")); 
            var log = s.Output.ToString();
            Assert.AreEqual("Question 1 [2 points]:\r\n   Running echo (1/2)... OK\r\n   Running echo (2/2)... OK\r\n\r\nQuestion 2 [1 point]:\r\n   Running echo (1/2)... OK\r\n   Running echo (2/2)... ERROR:\n      -Expected -> Wanted fail!; Found -> This is NOT OK\r\n\r\nTOTAL SCORE: 6.67", log);
        }

        [Test]
        public void ParseBody_QUESTION_BATCH_MULTI_DESCRIPTION()
        {                                      
            var s = new AutoCheck.Core.ScriptV2(GetSampleFile("body\\question\\question_ok6.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("My custom caption for the question 1 - My custom description with score 3/10 (TOTAL: 0):\r\n   Running echo (1/2)... OK\r\n   Running echo (2/2)... ERROR:\n      -Expected -> Error wanted!; Found -> Hello\r\n\r\nMy custom caption for the question 2 - My custom description with score 2/10 (TOTAL: 0):\r\n   Running echo... OK\r\n\r\nMy custom caption for the question 3 - My custom description with score 5/10 (TOTAL: 4):\r\n   Running echo (1/3)... OK\r\n   Running echo (2/3)... OK\r\n   Running echo (3/3)... OK\r\n\r\nTOTAL SCORE: 7", log);
        }

        [Test]
        public void ParseBody_QUESTION_BATCH_MULTI_METHODS()
        {              
            var s = new AutoCheck.Core.ScriptV2(GetSampleFile("body\\question\\question_ok8.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Question 1 [1 point]:\r\n   Checking files... OK\r\n   Getting files... OK\r\n\r\nQuestion 2 [1 point]:\r\n   Counting folders... ERROR:\n      -Expected -> -1; Found -> 0\r\n\r\nTOTAL SCORE: 5", log);
        } 

        [Test]
        public void ParseBody_QUESTION_BATCH_SUBQUESTION_SINGLE()
        {                                      
            var s = new AutoCheck.Core.ScriptV2(GetSampleFile("body\\question\\question_ok9.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Question 1 [10 points]:\r\n\r\n   Question 1.1 [2 points]:\r\n      Running echo... OK\r\n\r\n   Question 1.2 [5 points]:\r\n      Running echo (1/3)... OK\r\n      Running echo (2/3)... OK\r\n      Running echo (3/3)... OK\r\n\r\n   Question 1.3 [3 points]:\r\n      Running echo... ERROR:\n         -Expected -> Wanted Error!; Found -> Hello\r\n\r\n\r\nTOTAL SCORE: 7", log);
        }

        [Test]
        public void ParseBody_QUESTION_BATCH_SUBQUESTION_MULTI()
        {                                      
            var s = new AutoCheck.Core.ScriptV2(GetSampleFile("body\\question\\question_ok10.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Question 1 [4 points]:\r\n\r\n   Question 1.1 [1 point]:\r\n      Running echo... OK\r\n\r\n   Question 1.2 [2 points]:\r\n\r\n      Question 1.2.1 [1 point]:\r\n         Running echo... OK\r\n\r\n      Question 1.2.2 [1 point]:\r\n         Running echo... OK\r\n\r\n\r\n   Question 1.3 [1 point]:\r\n      Running echo... ERROR:\n         -Expected -> Wanted Error!; Found -> Hello\r\n\r\n\r\nQuestion 2 [3 points]:\r\n\r\n   Question 2.1 [1 point]:\r\n      Running echo... OK\r\n\r\n   Question 2.2 [1 point]:\r\n      Running echo (1/3)... OK\r\n      Running echo (2/3)... OK\r\n      Running echo (3/3)... OK\r\n\r\n   Question 2.3 [1 point]:\r\n      Running echo... ERROR:\n         -Expected -> Wanted Error!; Found -> Hello\r\n\r\n\r\nTOTAL SCORE: 7.14", log);
        }

        
        [Test]
        public void ParseBody_QUESTION_BATCH_SUBQUESTION_RUN()
        {                                      
            var s = new AutoCheck.Core.ScriptV2(GetSampleFile("body\\question\\question_ok11.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Question 1 [2 points]:\r\n\r\n   Question 1.1 [1 point]:\r\n      Running echo... OK\r\n\r\n   Question 1.2 [1 point]:\r\n      Running echo... ERROR:\n         -Expected -> Wanted Error!; Found -> Hello\r\n\r\n\r\nTOTAL SCORE: 5", log);
        } 
                
        [Test]
        public void ParseBody_QUESTION_NO_CAPTION()
        {              
             Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.ScriptV2(GetSampleFile("body\\question\\question_ko1.yaml")));                        
        }

        [Test]
        public void ParseBody_INHERITS_VARS_REPLACE()
        {        
            try{ 
                var s = new AutoCheck.Core.ScriptV2(GetSampleFile("inherits\\inherits_vars_ok1.yaml"));
            }   
            catch(ResultMismatchException ex){
                Assert.AreEqual("Expected -> Fer; Found -> New Fer", ex.Message);
            }                                       
        }

        //TODO: test to override other level-1 nodes (only 'vars' has been tested)

        [Test]
        public void ParseBody_INHERITS_SINGLE_RUN_FOLDER()
        {       
            var dest = Path.Combine(GetSamplePath("script"), "temp", "inherits", "test1");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);                                 

            File.Copy(GetSampleFile("resources\\nopass.zip"), GetSampleFile(dest, "nopass.zip"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.zip")));
            var s = new AutoCheck.Core.ScriptV2(GetSampleFile("inherits\\inherits_run_single_ok1.yaml"));            
            
            Assert.AreEqual("TOTAL SCORE: 0", s.Output.ToString());
            Directory.Delete(dest, true);
        }

        [Test]
        public void ParseBody_INHERITS_BATCH_RUN_FOLDER_SINGLE()
        {               
            var dest = Path.Combine(GetSamplePath("script"), "temp", "inherits", "test2");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);                                 

            File.Copy(GetSampleFile("resources\\nopass.zip"), GetSampleFile(dest, "nopass.zip"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.zip")));
            var s = new AutoCheck.Core.ScriptV2(GetSampleFile("inherits\\inherits_run_batch_ok1.yaml"));            
            
            Assert.AreEqual("Running script inherits_run_batch_ok1:\r\n   TOTAL SCORE: 0", s.Output.ToString());
            Directory.Delete(dest, true);
        }

        [Test]
        public void ParseBody_INHERITS_BATCH_RUN_FOLDER_MULTI()
        {     
            var dest =  Path.Combine(GetSamplePath("script"), "temp", "inherits", "test3");         
            var dest1 = Path.Combine(dest, "folder1");
            var dest2 = Path.Combine(dest, "folder2");

            if(!Directory.Exists(dest1)) Directory.CreateDirectory(dest1);
            if(!Directory.Exists(dest2)) Directory.CreateDirectory(dest2);                                 

            File.Copy(GetSampleFile("resources\\nopass.zip"), GetSampleFile(dest1, "nopass.zip"));
            File.Copy(GetSampleFile("resources\\nopass.zip"), GetSampleFile(dest2, "nopass.zip"));

            Assert.IsTrue(File.Exists(GetSampleFile(dest1, "nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest2, "nopass.zip")));

            var s = new AutoCheck.Core.ScriptV2(GetSampleFile("inherits\\inherits_run_batch_ok2.yaml"));   
            Assert.AreEqual("Running script inherits_run_batch_ok2:\r\n   TOTAL SCORE: 0\r\n\r\nRunning script inherits_run_batch_ok2:\r\n   TOTAL SCORE: 0", s.Output.ToString());

            Directory.Delete(dest, true);
        }

        [Test]
        public void ParseBody_INHERITS_BATCH_RUN_PATH()
        {               
            var dest =  Path.Combine(GetSamplePath("script"), "temp", "inherits", "test4");         
            var dest1 = Path.Combine(dest, "folder1");
            var dest2 = Path.Combine(dest, "folder2");

            if(!Directory.Exists(dest1)) Directory.CreateDirectory(dest1);
            if(!Directory.Exists(dest2)) Directory.CreateDirectory(dest2);                                 

            File.Copy(GetSampleFile("resources\\nopass.zip"), GetSampleFile(dest1, "nopass.zip"));
            File.Copy(GetSampleFile("resources\\nopass.zip"), GetSampleFile(dest2, "nopass.zip"));

            Assert.IsTrue(File.Exists(GetSampleFile(dest1, "nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest2, "nopass.zip")));

            var s = new AutoCheck.Core.ScriptV2(GetSampleFile("inherits\\inherits_run_batch_ok3.yaml"));            
            Assert.AreEqual("Running script inherits_run_batch_ok3:\r\n   TOTAL SCORE: 0\r\n\r\nRunning script inherits_run_batch_ok3:\r\n   TOTAL SCORE: 0", s.Output.ToString());

            Directory.Delete(dest, true);
        }

        [Test]
        public void ParseBody_INHERITS_BATCH_RUN_COMBO()
        {               
            var dest =  Path.Combine(GetSamplePath("script"), "temp", "inherits", "test5");         
            var dest1 = Path.Combine(dest, "folder1");
            var dest2 = Path.Combine(dest, "folder2");

            if(!Directory.Exists(dest1)) Directory.CreateDirectory(dest1);
            if(!Directory.Exists(dest2)) Directory.CreateDirectory(dest2);                                 

            File.Copy(GetSampleFile("resources\\nopass.zip"), GetSampleFile(dest1, "nopass.zip"));
            File.Copy(GetSampleFile("resources\\nopass.zip"), GetSampleFile(dest2, "nopass.zip"));

            Assert.IsTrue(File.Exists(GetSampleFile(dest1, "nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest2, "nopass.zip")));

            var s = new AutoCheck.Core.ScriptV2(GetSampleFile("inherits\\inherits_run_batch_ok4.yaml"));            
            Assert.AreEqual("Running script inherits_run_batch_ok4 for folder1:\r\n   TOTAL SCORE: 0\r\n\r\nRunning script inherits_run_batch_ok4 for folder2:\r\n   TOTAL SCORE: 0\r\n\r\nRunning script inherits_run_batch_ok4 for folder1:\r\n   TOTAL SCORE: 0\r\n\r\nRunning script inherits_run_batch_ok4 for folder2:\r\n   TOTAL SCORE: 0", s.Output.ToString());
            
            Directory.Delete(dest, true);
        }
                
        //TODO: copy detector        
        //TODO: json to dictionaries for complex Checkers/Connectors
        //TODO: think about how to merge checkers and connectors, make sense? is afordable with the new YAML scripting system? It will be clearer during old C# scripts migration to YAML :)
    }
}
