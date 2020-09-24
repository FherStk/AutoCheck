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
using AutoCheck.Exceptions;
using NUnit.Framework;

namespace AutoCheck.Test.Core
{    
    [Parallelizable(ParallelScope.All)]    
    public class Script : Test
    {
        private string _user = AutoCheck.Core.Utils.ConfigFile("gdrive_account.txt");
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
            using(var gdrive = new AutoCheck.Connectors.GDrive(_user, _secret)){
                gdrive.DeleteFolder("\\AutoCheck\\test", "uploadgdrive_ok1");
                gdrive.DeleteFolder("\\AutoCheck\\test", "uploadgdrive_ok2");
                gdrive.DeleteFolder("\\AutoCheck\\test", "uploadgdrive_ok3");
            }
        }

#region Vars
        [Test]
        public void ParseVars_DEFAULT_VARS()
        {  
           Assert.DoesNotThrow(() => new AutoCheck.Core.Script(GetSampleFile("vars\\vars_ok1.yaml")));                 
        }
        
        [Test]
        public void ParseVars_COMPUTED_REGEX()
        {  
            Assert.DoesNotThrow(() => new AutoCheck.Core.Script(GetSampleFile("vars\\vars_ok2.yaml")));                                           
        }
 
        [Test]
        public void ParseVars_TYPED_SIMPLE()
        {                         
            Assert.DoesNotThrow(() => new AutoCheck.Core.Script(GetSampleFile("vars\\vars_ok3.yaml")));              
        }

        [Test]
        public void ParseVars_SCOPE_LEVEL1()
        {                         
            Assert.DoesNotThrow(() => new AutoCheck.Core.Script(GetSampleFile("vars\\vars_ok4.yaml")));              
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
            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.Script(GetSampleFile("vars\\vars_ko1.yaml")));            
        }

        [Test]
        public void ParseVars_NOTEXISTS_SIMPLE()
        {  
            Assert.Throws<VariableNotFoundException>(() => new AutoCheck.Core.Script(GetSampleFile("vars\\vars_ko2.yaml")));           
        }
 
        [Test]
        public void ParseVars_REGEX_NOTAPPLIED()
        {  
            Assert.Throws<RegexInvalidException>(() => new AutoCheck.Core.Script(GetSampleFile("vars\\vars_ko3.yaml")));
        }

        [Test]
        public void ParseVars_REGEX_NOVARNAME()
        {  
            Assert.Throws<VariableNotFoundException>(() => new AutoCheck.Core.Script(GetSampleFile("vars\\vars_ko4.yaml")));
        }

        [Test]
        public void ParseVars_REGEX_NOTEXISTS()
        {  
            Assert.Throws<VariableNotFoundException>(() => new AutoCheck.Core.Script(GetSampleFile("vars\\vars_ko5.yaml")));
        }

        [Test]
        public void ParseVars_SCOPE_NOTEXISTS()
        {  
            Assert.Throws<VariableNotFoundException>(() => new AutoCheck.Core.Script(GetSampleFile("vars\\vars_ko6.yaml")));
        }
#endregion
#region Pre
        [Test]
        public void Extract_ZIP_NOREMOVE_NORECURSIVE()
        { 
            var dest = Path.Combine(GetSamplePath("script"), "temp", "extract", "test1");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);

            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest, "nopass.zip"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile(dest, "nopass.txt"))); 

            var s = new AutoCheck.Core.Script(GetSampleFile("pre\\extract\\extract_ok1.yaml"));
                        
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.txt")));
            
            Directory.Delete(dest, true);
        }

        [Test]
        public void Extract_NONEXISTING_NOREMOVE_NORECURSIVE()
        { 
            var dest = Path.Combine(GetSamplePath("script"), "temp", "extract", "test2");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);           

            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest, "nofound.zip"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nofound.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile(dest, "nopass.txt"))); 

            var s = new AutoCheck.Core.Script(GetSampleFile("pre\\extract\\extract_ok2.yaml"));

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

            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest, "nopass.zip"));
            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(rec, "nopass.zip"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile(dest, "nopass.txt")));
            Assert.IsTrue(File.Exists(GetSampleFile(rec, "nopass.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile(rec, "nopass.txt")));
            
            var s = new AutoCheck.Core.Script(GetSampleFile("pre\\extract\\extract_ok3.yaml"));

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

            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest, "nopass.zip"));
            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(rec, "nopass.zip"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile(dest, "nopass.txt")));
            Assert.IsTrue(File.Exists(GetSampleFile(rec, "nopass.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile(rec, "nopass.txt")));
            
            var s = new AutoCheck.Core.Script(GetSampleFile("pre\\extract\\extract_ok4.yaml"));

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

            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "dump.sql"));          
            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok1", "postgres", "postgres")){
                Assert.IsFalse(psql.ExistsDataBase());                
                var s = new AutoCheck.Core.Script(GetSampleFile("pre\\restore_db\\restoredb_ok1.yaml"));   
                
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

            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "dump.sql"));                     
            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok2", "postgres", "postgres")){
                Assert.IsFalse(psql.ExistsDataBase());                
                var s = new AutoCheck.Core.Script(GetSampleFile("pre\\restore_db\\restoredb_ok2.yaml"));   
                
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
                        
            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "dump.sql"));                         
            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok31", "postgres", "postgres")){
                Assert.IsFalse(psql.ExistsDataBase()); 
                var s = new AutoCheck.Core.Script(GetSampleFile("pre\\restore_db\\restoredb_ok3.1.yaml"));   //TODO: Should use a own file (not resue another test one...)   
                
                Assert.IsTrue(psql.ExistsDataBase());
                Assert.AreEqual(10, psql.ExecuteScalar<int>("SELECT COUNT (*) FROM test.work_history;"));
                psql.ExecuteNonQuery($"INSERT INTO test.work_history (id_employee, id_work, id_department) VALUES (999, 'MK_REP', 20);");
                Assert.AreEqual(11, psql.ExecuteScalar<int>("SELECT COUNT (*) FROM test.work_history;"));
            } 

            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "override.sql"));
            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "nooverride.sql"));
            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok32", "postgres", "postgres")){
                Assert.IsFalse(psql.ExistsDataBase());                                
                var s = new AutoCheck.Core.Script(GetSampleFile("pre\\restore_db\\restoredb_ok3.2.yaml"));   
                
                Assert.IsTrue(psql.ExistsDataBase());
                Assert.IsTrue(File.Exists(GetSampleFile(dest, "nooverride.sql"))); 
                Assert.IsFalse(File.Exists(GetSampleFile(dest, "override.sql"))); 
                psql.DropDataBase();  
            }
            File.Delete(GetSampleFile(dest, "nooverride.sql"));

            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok31", "postgres", "postgres")){
                Assert.IsTrue(psql.ExistsDataBase());                                
                Assert.AreEqual(10, psql.ExecuteScalar<int>("SELECT COUNT (*) FROM test.work_history;"));
                psql.DropDataBase();                
            } 

            Directory.Delete(dest, true);             
        }

        [Test] 
        public void RestoreDB_SPECIFIC_REMOVE_NOOVERRIDE_NORECURSIVE()
        {              
            var dest = Path.Combine(GetSamplePath("script"), "temp", "restore", "test4");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);

            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "nooverride.sql"));
            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok4", "postgres", "postgres")){
                Assert.IsFalse(psql.ExistsDataBase()); 
                var s = new AutoCheck.Core.Script(GetSampleFile("pre\\restore_db\\restoredb_ok4.yaml"));
                Assert.IsTrue(psql.ExistsDataBase());                                
                 
                Assert.AreEqual(10, psql.ExecuteScalar<int>("SELECT COUNT (*) FROM test.work_history;"));
                psql.ExecuteNonQuery($"INSERT INTO test.work_history (id_employee, id_work, id_department) VALUES (999, 'MK_REP', 20);");
                Assert.AreEqual(11, psql.ExecuteScalar<int>("SELECT COUNT (*) FROM test.work_history;"));

                s = new AutoCheck.Core.Script(GetSampleFile("pre\\restore_db\\restoredb_ok4.yaml"));   
                
                Assert.IsTrue(psql.ExistsDataBase());
                Assert.IsFalse(File.Exists(GetSampleFile(dest, "nooverride.sql"))); 
                Assert.AreEqual(10, psql.ExecuteScalar<int>("SELECT COUNT (*) FROM test.work_history;"));
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

            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "dump1.sql"));
            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(rec, "dump2.sql"));
            
            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "restoredb_ok5-dump1_sql", "postgres", "postgres")){
                Assert.IsFalse(psql.ExistsDataBase());
                var s = new AutoCheck.Core.Script(GetSampleFile("pre\\restore_db\\restoredb_ok5.yaml"));                   

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
            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "uploaded.sql"));                    
            
            var remotePath = "\\AutoCheck\\test\\uploadgdrive_ok1";
            var remoteFile = "uploaded.sql";
            using(var gdrive = new AutoCheck.Connectors.GDrive(_user, _secret)){
                Assert.IsFalse(gdrive.ExistsFolder(remotePath));                
                var s = new AutoCheck.Core.Script(GetSampleFile("pre\\upload_gdrive\\uploadgdrive_ok1.yaml"));   
                
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
            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "uploaded.sql"));          

            var rec = Path.Combine(dest, "recursive");
            if(!Directory.Exists(rec)) Directory.CreateDirectory(rec);
            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(rec, "uploaded.zip"));                                 
            
            var remotePath = "\\AutoCheck\\test\\uploadgdrive_ok2";
            var remotePath2 = Path.Combine(remotePath, "recursive");
            var remoteFile = "uploaded.sql";
            var remoteFile2 = "uploaded.zip";
            using(var gdrive = new AutoCheck.Connectors.GDrive(_user, _secret)){
                Assert.IsFalse(gdrive.ExistsFolder(remotePath));

                var s = new AutoCheck.Core.Script(GetSampleFile("pre\\upload_gdrive\\uploadgdrive_ok2.yaml"));   
                
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
            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "uploaded.sql"));          
            
            var remotePath = "\\AutoCheck\\test\\uploadgdrive_ok3";
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);  
            
            File.Copy(GetSampleFile("gdrive", "download.txt"), GetSampleFile(dest, "download.txt"));
            using(var gdrive = new AutoCheck.Connectors.GDrive(_user, _secret)){
                Assert.IsFalse(gdrive.ExistsFolder(remotePath));

                var s = new AutoCheck.Core.Script(GetSampleFile("pre\\upload_gdrive\\uploadgdrive_ok3.yaml"));                               

                Assert.IsTrue(gdrive.ExistsFile(remotePath, "1MB.zip"));
                Assert.IsTrue(gdrive.ExistsFile(remotePath, "10MB.test"));
            }

            Directory.Delete(dest, true);
        }

        //TODO: UploadGDrive_KO() testing something unable to parse (read the PDF content for example, it will be supported in a near future, but not right now) or upload
#endregion
#region Connector
        [Test]
        public void ParseBody_CONNECTOR_EMPTY()
        {  
            Assert.DoesNotThrow(() => new AutoCheck.Core.Script(GetSampleFile("body\\connector\\connector_ok1.yaml")));                                         
        }

        [Test]
        public void ParseBody_CONNECTOR_INLINE_ARGS()
        {              
            Assert.DoesNotThrow(() => new AutoCheck.Core.Script(GetSampleFile("body\\connector\\connector_ok2.yaml")));            
        }

        [Test]
        public void ParseBody_CONNECTOR_TYPED_ARGS()
        {                          
            Assert.DoesNotThrow(() => new AutoCheck.Core.Script(GetSampleFile("body\\connector\\connector_ok3.yaml")));                                    
        }

        [Test]
        public void ParseBody_CONNECTOR_MULTI_LOAD()
        {                          
            Assert.DoesNotThrow(() => new AutoCheck.Core.Script(GetSampleFile("body\\connector\\connector_ok4.yaml")));                          
        }

        [Test]
        public void ParseBody_CONNECTOR_REMOTE_IP()
        {                          
            Assert.DoesNotThrow(() => new AutoCheck.Core.Script(GetSampleFile("body\\connector\\connector_ok5.yaml")));                          
        }

        [Test]
        public void ParseBody_CONNECTOR_IMPLICIT_INVALID_INLINE_ARGS()
        {  
            Assert.Throws<ArgumentInvalidException>(() => new AutoCheck.Core.Script(GetSampleFile("body\\connector\\connector_ko1.yaml")));           
        }

        [Test]
        public void ParseBody_CONNECTOR_EXPLICIT_INVALID_INLINE_ARGS()
        {  
            Assert.Throws<ArgumentInvalidException>(() => new AutoCheck.Core.Script(GetSampleFile("body\\connector\\connector_ko2.yaml")));
        }

        [Test]
        public void ParseBody_CONNECTOR_EXPLICIT_INVALID_TYPED_ARGS()
        {  
            Assert.Throws<ArgumentInvalidException>(() => new AutoCheck.Core.Script(GetSampleFile("body\\connector\\connector_ko3.yaml")));                           
        }
#endregion
#region Run       
        [Test]
        public void ParseBody_RUN_ECHO()
        {  
            Assert.DoesNotThrow(() => new AutoCheck.Core.Script(GetSampleFile("body\\run\\run_ok1.yaml")));
        }
 
        [Test]
        public void ParseBody_RUN_FIND()
        {          
            Assert.DoesNotThrow(() => new AutoCheck.Core.Script(GetSampleFile("body\\run\\run_ok2.yaml")));
        }

        [Test]
        public void ParseBody_RUN_CAPTION_OK()
        {          
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\run\\run_ok3.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Checking if file exists...  OK", log);
        }

        [Test]
        public void ParseBody_RUN_CAPTION_ERROR()
        {          
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\run\\run_ok4.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Checking if file exists...  OK\r\nCounting folders...  ERROR:\n   -Expected -> Wanted ERROR!; Found -> 0", log);
        }

        [Test]
        public void ParseBody_RUN_CAPTION_EXCEPTION()
        {   
            Assert.Throws<ArgumentInvalidException>(() => new AutoCheck.Core.Script(GetSampleFile("body\\run\\run_ko3.yaml")));                
        }

        [Test]
        public void ParseBody_RUN_NOCAPTION_EXCEPTION()
        {   
            Assert.Throws<ResultMismatchException>(() => new AutoCheck.Core.Script(GetSampleFile("body\\run\\run_ko4.yaml")));                
        } 

        [Test]
        public void ParseBody_RUN_EMPTY()
        {  
            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.Script(GetSampleFile("body\\run\\run_ko1.yaml")));
        }

        [Test]
        public void ParseBody_RUN_INVALID_TYPED_ARGS()
        {  
            Assert.Throws<ArgumentInvalidException>(() => new AutoCheck.Core.Script(GetSampleFile("body\\run\\run_ko2.yaml")));            
        }
#endregion
#region Question
        [Test]
        public void ParseBody_QUESTION_DEFAULT_SINGLE_ECHO()
        {                                      
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\question\\question_ok1.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Question 1 [1 point]:\r\n   Running echo... OK\r\n\r\nTOTAL SCORE: 10", log);
        }

        [Test]
        public void ParseBody_QUESTION_DEFAULT_MULTI_ECHO()
        {              
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\question\\question_ok2.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Question 1 [1 point]:\r\n   Running echo (1/2)... OK\r\n   Running echo (2/2)... OK\r\n\r\nTOTAL SCORE: 10", log);
        }

        [Test]
        public void ParseBody_QUESTION_DEFAULT_MULTI_METHODS()
        {              
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\question\\question_ok7.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Question 1 [1 point]:\r\n   Checking files... OK\r\n   Getting files... OK\r\n\r\nTOTAL SCORE: 10", log);
        }

        [Test]
        public void ParseBody_QUESTION_BATCH_MULTI_ECHO()
        {              
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\question\\question_ok3.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Question 1 [1 point]:\r\n   Running echo (1/2)... OK\r\n   Running echo (2/2)... OK\r\n\r\nQuestion 2 [1 point]:\r\n   Running echo (1/2)... OK\r\n   Running echo (2/2)... ERROR:\n      -Expected -> Wanted fail!; Found -> This is NOT OK\r\n\r\nTOTAL SCORE: 5", log);
        } 

        [Test]
        public void ParseBody_QUESTION_BATCH_MULTI_MESSAGES()
        {              
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\question\\question_ok4.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Question 1 [1 point]:\r\n   Running echo (1/2)... OK\r\n   Running echo (2/2)... ERROR:\n      -Expected -> Wanted fail!; Found -> Bye!\r\n\r\nQuestion 2 [1 point]:\r\n   Running echo (1/2)... GREAT!\r\n   Running echo (2/2)... SO BAD!:\n      -Expected -> Wanted fail!; Found -> This is NOT OK\r\n\r\nTOTAL SCORE: 0", log);
        }

        [Test] 
        public void ParseBody_QUESTION_BATCH_MULTI_SCORE()
        {              
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\question\\question_ok5.yaml")); 
            var log = s.Output.ToString();
            Assert.AreEqual("Question 1 [2 points]:\r\n   Running echo (1/2)... OK\r\n   Running echo (2/2)... OK\r\n\r\nQuestion 2 [1 point]:\r\n   Running echo (1/2)... OK\r\n   Running echo (2/2)... ERROR:\n      -Expected -> Wanted fail!; Found -> This is NOT OK\r\n\r\nTOTAL SCORE: 6.67", log);
        }

        [Test]
        public void ParseBody_QUESTION_BATCH_MULTI_DESCRIPTION()
        {                                      
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\question\\question_ok6.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("My custom caption for the question 1 - My custom description with score 3/10 (TOTAL: 0):\r\n   Running echo (1/2)... OK\r\n   Running echo (2/2)... ERROR:\n      -Expected -> Error wanted!; Found -> Hello\r\n\r\nMy custom caption for the question 2 - My custom description with score 2/10 (TOTAL: 0):\r\n   Running echo... OK\r\n\r\nMy custom caption for the question 3 - My custom description with score 5/10 (TOTAL: 4):\r\n   Running echo (1/3)... OK\r\n   Running echo (2/3)... OK\r\n   Running echo (3/3)... OK\r\n\r\nTOTAL SCORE: 7", log);
        }

        [Test]
        public void ParseBody_QUESTION_BATCH_MULTI_METHODS()
        {              
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\question\\question_ok8.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Question 1 [1 point]:\r\n   Checking files... OK\r\n   Getting files... OK\r\n\r\nQuestion 2 [1 point]:\r\n   Counting folders... ERROR:\n      -Expected -> -1; Found -> 0\r\n\r\nTOTAL SCORE: 5", log);
        } 

        [Test]
        public void ParseBody_QUESTION_BATCH_SUBQUESTION_SINGLE()
        {                                      
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\question\\question_ok9.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Question 1 [10 points]:\r\n\r\n   Question 1.1 [2 points]:\r\n      Running echo... OK\r\n\r\n   Question 1.2 [5 points]:\r\n      Running echo (1/3)... OK\r\n      Running echo (2/3)... OK\r\n      Running echo (3/3)... OK\r\n\r\n   Question 1.3 [3 points]:\r\n      Running echo... ERROR:\n         -Expected -> Wanted Error!; Found -> Hello\r\n\r\n\r\nTOTAL SCORE: 7", log);
        }

        [Test]
        public void ParseBody_QUESTION_BATCH_SUBQUESTION_MULTI()
        {                                      
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\question\\question_ok10.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Question 1 [4 points]:\r\n\r\n   Question 1.1 [1 point]:\r\n      Running echo... OK\r\n\r\n   Question 1.2 [2 points]:\r\n\r\n      Question 1.2.1 [1 point]:\r\n         Running echo... OK\r\n\r\n      Question 1.2.2 [1 point]:\r\n         Running echo... OK\r\n\r\n\r\n   Question 1.3 [1 point]:\r\n      Running echo... ERROR:\n         -Expected -> Wanted Error!; Found -> Hello\r\n\r\n\r\nQuestion 2 [3 points]:\r\n\r\n   Question 2.1 [1 point]:\r\n      Running echo... OK\r\n\r\n   Question 2.2 [1 point]:\r\n      Running echo (1/3)... OK\r\n      Running echo (2/3)... OK\r\n      Running echo (3/3)... OK\r\n\r\n   Question 2.3 [1 point]:\r\n      Running echo... ERROR:\n         -Expected -> Wanted Error!; Found -> Hello\r\n\r\n\r\nTOTAL SCORE: 7.14", log);
        }
        
        [Test]
        public void ParseBody_QUESTION_BATCH_SUBQUESTION_RUN()
        {                                      
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\question\\question_ok11.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Question 1 [2 points]:\r\n\r\n   Question 1.1 [1 point]:\r\n      Running echo... OK\r\n\r\n   Question 1.2 [1 point]:\r\n      Running echo... ERROR:\n         -Expected -> Wanted Error!; Found -> Hello\r\n\r\n\r\nTOTAL SCORE: 5", log);
        }                        
#endregion
#region Inherits
        [Test]
        public void ParseBody_INHERITS_VARS_REPLACE()
        {        
            try{ 
                var s = new AutoCheck.Core.Script(GetSampleFile("inherits\\inherits_vars_ok1.yaml"));
            }   
            catch(ResultMismatchException ex){
                Assert.AreEqual("Expected -> Fer; Found -> New Fer", ex.Message);
            }                                       
        }

        //TODO: test to override other level-1 nodes (only 'vars' has been tested)

        [Test]
        public void ParseBody_INHERITS_RUN_FOLDER()
        {       
            var dest = Path.Combine(GetSamplePath("script"), "temp", "inherits", "test2");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);                                 

            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest, "nopass.zip"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.zip")));
            var s = new AutoCheck.Core.Script(GetSampleFile("inherits\\inherits_run_ok1.yaml"));            
            
            Assert.AreEqual(string.Empty, s.Output.ToString());
            Directory.Delete(dest, true);
        }
#endregion
#region Batch
        [Test]
        public void ParseBody_BATCH_RUN_FOLDER_SINGLE()
        {               
            var dest = Path.Combine(GetSamplePath("script"), "temp", "batch", "test1");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);                                 

            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest, "nopass.zip"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.zip")));
            var s = new AutoCheck.Core.Script(GetSampleFile("batch\\batch_run_ok1.yaml"));            
            
            Assert.AreEqual("Running script batch_run_ok1:", s.Output.ToString());
            Directory.Delete(dest, true);
        }

        [Test]
        public void ParseBody_BATCH_RUN_FOLDER_MULTI()
        {     
            var dest =  Path.Combine(GetSamplePath("script"), "temp", "batch", "test2");         
            var dest1 = Path.Combine(dest, "folder1");
            var dest2 = Path.Combine(dest, "folder2");

            if(!Directory.Exists(dest1)) Directory.CreateDirectory(dest1);
            if(!Directory.Exists(dest2)) Directory.CreateDirectory(dest2);                                 

            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest1, "nopass.zip"));
            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest2, "nopass.zip"));

            Assert.IsTrue(File.Exists(GetSampleFile(dest1, "nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest2, "nopass.zip")));

            var s = new AutoCheck.Core.Script(GetSampleFile("batch\\batch_run_ok2.yaml"));   
            Assert.AreEqual("Running script batch_run_ok2:\r\n\r\nRunning script batch_run_ok2:", s.Output.ToString());

            Directory.Delete(dest, true);
        }

        [Test]
        public void ParseBody_BATCH_RUN_PATH()
        {               
            var dest =  Path.Combine(GetSamplePath("script"), "temp", "batch", "test3");         
            var dest1 = Path.Combine(dest, "folder1");
            var dest2 = Path.Combine(dest, "folder2");

            if(!Directory.Exists(dest1)) Directory.CreateDirectory(dest1);
            if(!Directory.Exists(dest2)) Directory.CreateDirectory(dest2);                                 

            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest1, "nopass.zip"));
            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest2, "nopass.zip"));

            Assert.IsTrue(File.Exists(GetSampleFile(dest1, "nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest2, "nopass.zip")));

            var s = new AutoCheck.Core.Script(GetSampleFile("batch\\batch_run_ok3.yaml"));            
            Assert.AreEqual("Running script batch_run_ok3:\r\n\r\nRunning script batch_run_ok3:", s.Output.ToString());

            Directory.Delete(dest, true);
        }

        [Test]
        public void ParseBody_BATCH_RUN_COMBO_INTERNAL()
        {               
            var dest =  Path.Combine(GetSamplePath("script"), "temp", "batch", "test4");         
            var dest1 = Path.Combine(dest, "folder1");
            var dest2 = Path.Combine(dest, "folder2");

            if(!Directory.Exists(dest1)) Directory.CreateDirectory(dest1);
            if(!Directory.Exists(dest2)) Directory.CreateDirectory(dest2);                                 

            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest1, "nopass.zip"));
            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest2, "nopass.zip"));

            Assert.IsTrue(File.Exists(GetSampleFile(dest1, "nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest2, "nopass.zip")));

            var s = new AutoCheck.Core.Script(GetSampleFile("batch\\batch_run_ok4.yaml"));            
            Assert.AreEqual("Running script batch_run_ok4 for folder1:\r\n\r\nRunning script batch_run_ok4 for folder2:\r\n\r\nRunning script batch_run_ok4 for folder1:\r\n\r\nRunning script batch_run_ok4 for folder2:", s.Output.ToString());
            
            Directory.Delete(dest, true);
        }

        [Test]
        public void ParseBody_BATCH_RUN_COMBO_EXTERNAL()
        {               
            var dest =  Path.Combine(GetSamplePath("script"), "temp", "batch", "test5");         
            var dest1 = Path.Combine(dest, "folder1");
            var dest2 = Path.Combine(dest, "folder2");

            if(!Directory.Exists(dest1)) Directory.CreateDirectory(dest1);
            if(!Directory.Exists(dest2)) Directory.CreateDirectory(dest2);                                 

            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest1, "nopass.zip"));
            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest2, "nopass.zip"));

            Assert.IsTrue(File.Exists(GetSampleFile(dest1, "nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest2, "nopass.zip")));

            var s = new AutoCheck.Core.Script(GetSampleFile("batch\\batch_run_ok5.yaml"));            
            Assert.AreEqual("Running script batch_run_ok5 for folder1:\r\n\r\nRunning script batch_run_ok5 for folder2:\r\n\r\nRunning script batch_run_ok5 for folder1:\r\n\r\nRunning script batch_run_ok5 for folder2:", s.Output.ToString());
            
            Directory.Delete(dest, true);
        }
#endregion
#region Copy detector
        [Test]
        public void ParseBody_COPY_PLAINTEXT_PATH_ISCOPY()
        {               
            var dest =  Path.Combine(GetSamplePath("script"), "temp", "copy", "test1");         
            var dest1 = Path.Combine(dest, "folder1");
            var dest2 = Path.Combine(dest, "folder2");

            if(!Directory.Exists(dest1)) Directory.CreateDirectory(dest1);
            if(!Directory.Exists(dest2)) Directory.CreateDirectory(dest2);                                 

            File.Copy(GetSampleFile("plaintext", "lorem1.txt"), GetSampleFile(dest1, "sample1.txt"));
            File.Copy(GetSampleFile("plaintext", "lorem1.txt"), GetSampleFile(dest2, "sample2.txt"));
 
            Assert.IsTrue(File.Exists(GetSampleFile(dest1, "sample1.txt")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest2, "sample2.txt")));

            var s = new AutoCheck.Core.Script(GetSampleFile("copy\\copy_plaintext_ok1.yaml")); 
            
            Assert.AreEqual("Looking for potential copies within folder1... OK\r\nLooking for potential copies within folder2... OK\r\nRunning script copy_plaintext_ok1 for folder1:\r\n   Potential copy detected for folder1\\sample1.txt!\r\n      Match score with folder2\\sample2.txt: 100,00 % \r\n\r\n\r\nRunning script copy_plaintext_ok1 for folder2:\r\n   Potential copy detected for folder2\\sample2.txt!\r\n      Match score with folder1\\sample1.txt: 100,00 %", s.Output.ToString());            
            Directory.Delete(dest, true);
        }

        [Test]
        public void ParseBody_COPY_PLAINTEXT_FOLDERS_NOTCOPY()
        {               
            var dest =  Path.Combine(GetSamplePath("script"), "temp", "copy", "test2");         
            var dest1 = Path.Combine(dest, "folder1");
            var dest2 = Path.Combine(dest, "folder2");

            if(!Directory.Exists(dest1)) Directory.CreateDirectory(dest1);
            if(!Directory.Exists(dest2)) Directory.CreateDirectory(dest2);                                 

            File.Copy(GetSampleFile("plaintext", "lorem1.txt"), GetSampleFile(dest1, "sample1.txt"));
            File.Copy(GetSampleFile("plaintext", "lorem2.txt"), GetSampleFile(dest2, "sample2.txt"));
 
            Assert.IsTrue(File.Exists(GetSampleFile(dest1, "sample1.txt")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest2, "sample2.txt")));

            var s = new AutoCheck.Core.Script(GetSampleFile("copy\\copy_plaintext_ok2.yaml")); 
            
            Assert.AreEqual("Looking for potential copies within folder1... OK\r\nLooking for potential copies within folder2... OK\r\nRunning script copy_plaintext_ok2 for folder1:\r\n\r\nRunning script copy_plaintext_ok2 for folder2:", s.Output.ToString());            
            Directory.Delete(dest, true);
        }
#endregion
#region Dummy script testing
        [Test]
        public void ParseBody_SCRIPT_SINGLE_OK1()
        {    
            var dest =  Path.Combine(GetSamplePath("script"), "temp", "script", "test1");                        
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
            
            File.Copy(GetSampleFile("html", "correct.html"), GetSampleFile(dest, "index.html"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "index.html")));

            var s = new AutoCheck.Core.Script(GetSampleFile("base\\script_single_1.yaml"));             
            Assert.AreEqual("Question 1 [2 points] - Checking Index.html:\r\n   Validating document against the W3C validation service...  OK\r\n\r\n   Question 1.1 [1 point] - Validating headers:\r\n      Checking amount of level-1 headers...  OK\r\n      Checking amount of level-2 headers...  OK\r\n\r\n   Question 1.2 [1 point] - Validating paragraphs:\r\n      Checking amount of paragraphs...  OK\r\n      Checking content legth within paragraphs...  ERROR:\n         -Expected -> >=1500; Found -> 144\r\n\r\n\r\nTOTAL SCORE: 5", s.Output.ToString());            
        }

        [Test]
        public void ParseBody_SCRIPT_SINGLE_ONEXCEPTION_ABORT()
        {    
            var dest =  Path.Combine(GetSamplePath("script"), "temp", "script", "test2");                        
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
            
            File.Copy(GetSampleFile("html", "incorrect.html"), GetSampleFile(dest, "index.html"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "index.html")));

            var s = new AutoCheck.Core.Script(GetSampleFile("base\\script_single_2.yaml"));             
            Assert.AreEqual("Question 1 [2 points] - Checking Index.html:\r\n   Validating document against the W3C validation service...  ERROR:\n      -Expected -> ; Found -> Exception has been thrown by the target of an invocation. --> Exception of type 'AutoCheck.Exceptions.DocumentInvalidException' was thrown.\r\n\r\n\r\nAborting execution!\r\n\r\nTOTAL SCORE: 0", s.Output.ToString());            
        }

        [Test]
        public void ParseBody_SCRIPT_SINGLE_ONEXCEPTION_ERROR()
        {    
            var dest =  Path.Combine(GetSamplePath("script"), "temp", "script", "test3");                        
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
            
            File.Copy(GetSampleFile("html", "incorrect.html"), GetSampleFile(dest, "index.html"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "index.html")));

            var s = new AutoCheck.Core.Script(GetSampleFile("base\\script_single_3.yaml"));             
            Assert.AreEqual("Question 1 [2 points] - Checking Index.html:\r\n   Validating document against the W3C validation service...  ERROR:\n      -Expected -> ; Found -> Exception has been thrown by the target of an invocation. --> Exception of type 'AutoCheck.Exceptions.DocumentInvalidException' was thrown.\r\n\r\n   Question 1.1 [1 point] - Validating headers:\r\n      Checking amount of level-1 headers...  OK\r\n      Checking amount of level-2 headers...  ERROR:\n         -Expected -> >=1; Found -> 0\r\n\r\n   Question 1.2 [1 point] - Validating paragraphs:\r\n      Checking amount of paragraphs...  OK\r\n      Checking content legth within paragraphs...  ERROR:\n         -Expected -> >=1500; Found -> 36\r\n\r\n\r\nTOTAL SCORE: 0", s.Output.ToString());            
        }

        [Test]
        public void ParseBody_SCRIPT_SINGLE_ONEXCEPTION_SUCCESS()
        {    
            var dest =  Path.Combine(GetSamplePath("script"), "temp", "script", "test4");                        
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
            
            File.Copy(GetSampleFile("html", "incorrect.html"), GetSampleFile(dest, "index.html"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "index.html")));

            var s = new AutoCheck.Core.Script(GetSampleFile("base\\script_single_4.yaml"));             
            Assert.AreEqual("Question 1 [2 points] - Checking Index.html:\r\n   Validating document against the W3C validation service...  OK\r\n\r\n   Question 1.1 [1 point] - Validating headers:\r\n      Checking amount of level-1 headers...  OK\r\n      Checking amount of level-2 headers...  ERROR:\n         -Expected -> >=1; Found -> 0\r\n\r\n   Question 1.2 [1 point] - Validating paragraphs:\r\n      Checking amount of paragraphs...  OK\r\n      Checking content legth within paragraphs...  ERROR:\n         -Expected -> >=1500; Found -> 36\r\n\r\n\r\nTOTAL SCORE: 0", s.Output.ToString());            
        }

        [Test]
        public void ParseBody_SCRIPT_SINGLE_ONEXCEPTION_SKIP()
        {    
            var dest =  Path.Combine(GetSamplePath("script"), "temp", "script", "test5");                        
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
            
            File.Copy(GetSampleFile("html", "incorrect.html"), GetSampleFile(dest, "index.html"));
            File.Copy(GetSampleFile("html", "correct.html"), GetSampleFile(dest, "contact.html"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "index.html")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "contact.html")));

            var s = new AutoCheck.Core.Script(GetSampleFile("base\\script_single_5.yaml"));             
            Assert.AreEqual("Question 1 [2 points] - Checking Index.html:\r\n   Validating document against the W3C validation service...  ERROR:\n      -Expected -> ; Found -> Exception has been thrown by the target of an invocation. --> Exception of type 'AutoCheck.Exceptions.DocumentInvalidException' was thrown.\r\n\r\nQuestion 2 [2 points] - Checking Contact.html:\r\n   Validating document against the W3C validation service...  OK\r\n\r\n   Question 2.1 [1 point] - Validating headers:\r\n      Checking amount of level-1 headers...  OK\r\n      Checking amount of level-2 headers...  OK\r\n\r\n   Question 2.2 [1 point] - Validating paragraphs:\r\n      Checking amount of paragraphs...  OK\r\n      Checking content legth within paragraphs...  ERROR:\n         -Expected -> >=1500; Found -> 144\r\n\r\n\r\nTOTAL SCORE: 2.5", s.Output.ToString());            
        }

        [Test]
        public void ParseBody_SCRIPT_SINGLE_ONEXCEPTION_NOCAPTION()
        {    
            var dest =  Path.Combine(GetSamplePath("script"), "temp", "script", "test6");                        
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
            
            File.Copy(GetSampleFile("html", "incorrect.html"), GetSampleFile(dest, "index.html"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "index.html")));

            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.Script(GetSampleFile("base\\script_single_6.yaml")));            
        }

        [Test]
        public void ParseBody_SCRIPT_ARGUMENT_TYPE_CONNECTOR_OK()
        {    
            var dest =  Path.Combine(GetSamplePath("script"), "temp", "script", "test7");                        
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
            
            File.Copy(GetSampleFile("html", "correct.html"), GetSampleFile(dest, "index.html"));
            File.Copy(GetSampleFile("css", "correct.css"), GetSampleFile(dest, "index.css"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "index.html")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "index.css")));

            var s = new AutoCheck.Core.Script(GetSampleFile("base\\script_single_7.yaml"));             
            Assert.AreEqual("Question 1 [1 point] - Checking index.css:\r\n   Validating document against the W3C validation service...  OK\r\n\r\n   Question 1.1 [1 point] - Validating font property:\r\n      Checking if the font property has been created... OK\r\n      Checking if the font property has NOT been applied... OK\r\n\r\n\r\nTOTAL SCORE: 10", s.Output.ToString());            
        }

        [Test]
        public void ParseBody_SCRIPT_ARGUMENT_TYPE_CONNECTOR_KO()
        {    
            var dest =  Path.Combine(GetSamplePath("script"), "temp", "script", "test8");                        
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
            
            File.Copy(GetSampleFile("html", "correct.html"), GetSampleFile(dest, "index.html"));
            File.Copy(GetSampleFile("css", "correct.css"), GetSampleFile(dest, "index.css"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "index.html")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "index.css")));

            Assert.Throws<ConnectorNotFoundException>(() => new AutoCheck.Core.Script(GetSampleFile("base\\script_single_8.yaml")));               
        }
       
        [Test]
        public void ParseBody_SCRIPT_ARGUMENT_TYPE_CONNECTOR_TUPLE()
        {    
            var dest =  Path.Combine(GetSamplePath("script"), "temp", "script", "test9");                        
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
            
            File.Copy(GetSampleFile("html", "correct.html"), GetSampleFile(dest, "index.html"));
            File.Copy(GetSampleFile("css", "correct.css"), GetSampleFile(dest, "index.css"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "index.html")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "index.css")));

            var s = new AutoCheck.Core.Script(GetSampleFile("base\\script_single_9.yaml"));             
            Assert.AreEqual("Question 1 [1 point] - Checking index.css:\r\n   Validating document against the W3C validation service...  OK\r\n\r\n   Question 1.1 [1 point] - Validating set of properties:\r\n      Checking if the (top | right | bottom | left) property has been created... OK\r\n\r\n\r\nTOTAL SCORE: 10", s.Output.ToString());            
        }
#endregion
#region Real script testing        
        [Test]
        public void FULL_HTML5_SCRIPT_SINGLE()
        {             
            var source = Path.Combine(GetSamplePath("private"), "html5", "Student Name 1");
            Assert.IsTrue(Directory.Exists(source));
            
            var s = new AutoCheck.Core.Script(Path.Combine(GetSamplePath("script"), "targets", "html5_single_1.yaml"));                        
            Assert.AreEqual("Running script 'DAM - M04 (UF1): HTML5 Assignment' in single mode for 'targets':\r\n\r\nQuestion 1 [4 points] - Checking index.html:\r\n   Validating document against the W3C validation service...  OK\r\n\r\n   Question 1.1 [1 point] - Validating headers:\r\n      Checking amount of level-1 headers...  OK\r\n      Checking amount of level-2 headers...  OK\r\n\r\n   Question 1.2 [1 point] - Validating paragraphs:\r\n      Checking amount of paragraphs...  OK\r\n      Checking content legth within paragraphs...  OK\r\n\r\n   Question 1.3 [1 point] - Validating breaklines:\r\n      Checking amount of breaklines...  OK\r\n\r\n   Question 1.4 [1 point] - Validating images:\r\n      Checking amount of images...  OK\r\n\r\n\r\nQuestion 2 [12 points] - Checking contacte.html:\r\n   Validating document against the W3C validation service...  OK\r\n\r\n   Question 2.1 [1 point] - Validating text fields:\r\n      Checking amount of text fields...  OK\r\n\r\n   Question 2.2 [1 point] - Validating numeric fields:\r\n      Checking amount of numeric fields...  OK\r\n\r\n   Question 2.3 [1 point] - Validating email fields:\r\n      Checking amount of email fields...  OK\r\n\r\n   Question 2.4 [1 point] - Validating radio fields:\r\n      Checking amount of radio fields...  OK\r\n      Checking group for the radio fields...  OK\r\n      Checking the checked radio fields...  OK\r\n\r\n   Question 2.5 [1 point] - Validating select fields:\r\n      Checking amount of select fields...  OK\r\n      Checking select options...  OK\r\n      Checking the selected option...  OK\r\n\r\n   Question 2.6 [1 point] - Validating checkbox fields:\r\n      Checking amount of checkbox fields...  OK\r\n      Checking group for the checkbox fields...  OK\r\n      Checking the checked option...  OK\r\n\r\n   Question 2.7 [1 point] - Validating textarea fields:\r\n      Checking amount of textarea fields...  OK\r\n\r\n   Question 2.8 [1 point] - Validating placeholders:\r\n      Checking amount of placelhoders for text fields...  OK\r\n      Checking amount of placelhoders for email fields...  OK\r\n      Checking amount of placelhoders for numeric fields...  OK\r\n      Checking amount of placelhoders for textarea fields...  OK\r\n\r\n   Question 2.9 [1 point] - Validating labels:\r\n      Checking amount of labels for text fields...  OK\r\n      Checking amount of labels for numeric fields...  OK\r\n      Checking amount of labels for email fields...  OK\r\n      Checking amount of labels for radio fields...  OK\r\n      Checking amount of labels for select fields...  OK\r\n      Checking amount of labels for check fields...  OK\r\n      Checking amount of labels for textarea fields...  OK\r\n\r\n   Question 2.10 [1 point] - Validating table:\r\n      Checking amount of columns...  OK\r\n      Checking amount of merged columns...  OK\r\n      Checking amount of labels within the first column...  OK\r\n      Checking amount of labels within the second column...  OK\r\n      Checking amount of labels within the third column...  OK\r\n      Checking table's consistency...  OK\r\n\r\n   Question 2.11 [1 point] - Validating form reset:\r\n      Checking amount of reset buttons...  OK\r\n\r\n   Question 2.12 [1 point] - Validating form submit:\r\n      Checking amount of fields with no name...  OK\r\n      Checking amount of submit buttons...  OK\r\n      Checking form action...  OK\r\n\r\n\r\nQuestion 3 [2 points] - Checking menu (index.html):\r\n   Validating document against the W3C validation service...  OK\r\n\r\n   Question 3.1 [1 point] - Validating lists:\r\n      Checking amount of lists...  OK\r\n      Checking amount of list items...  OK\r\n\r\n   Question 3.2 [1 point] - Validating links:\r\n      Checking amount of links...  OK\r\n      Checking links destination...  OK\r\n\r\n\r\nTOTAL SCORE: 10", s.Output.ToString());            
        }  

#endregion
        //TODO: Captions on single and batch to know the folder/student overriding the "caption" var (done but not tested) 
        //TODO: parse YAML dictionaries to C# objects (casting and testing are pending)
        //TODO: individual tests for copy detectors when migration (old V1 removed and replaced by V2) completed
        //TODO: test the other copy detectors when migration (old V1 removed and replaced by V2) completed    
        //TODO: templates should be moved to the documentation, so only real scripts will remain within "scripts" folder        

        //IDEAS AND IMPROVEMENTS: 
        //  Connectors and Checkers where created in order to support scripting using C# but, with the new YAML scripts, a lot of the old stuff can be removed or simplified:
        //      All connectors must provide primitive type as return in order to compare the result through the YAML scripts, arrays and dictionaries are also allowed.

        //  Allow executing custom code within pre and post or remove pre and post and allow its behaviour within the body. Choose wisely.
        //  Same behaviour as "onexception" but with "onerror" so wrong questions can be skipped or can stop the execution (and not only under unexpected behaviours).
        //  New "echo" option within "body" to avoid empty question or runs to display messages.        
        //  Load "folder", "path", etc. from terminal when loading a script, so target execution can be done overriding the script default values.
        
    }
}