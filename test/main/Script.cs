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
using AutoCheck.Core.Exceptions;
using NUnit.Framework;

namespace AutoCheck.Test
{    
    [Parallelizable(ParallelScope.All)]    
    public class Script : Test
    {
        private string _user = AutoCheck.Core.Utils.ConfigFile("gdrive_account.txt");
        private string _secret = AutoCheck.Core.Utils.ConfigFile("gdrive_secret.json");    
              
        protected override void CleanUp(){
            //TODO: Abort execution and display an error message if there's no PSQL, remote host, etc...

            //Clean temp files
            var dir = Path.Combine(GetSamplePath("script"), "temp");
            if(Directory.Exists(dir)) Directory.Delete(dir, true);       

            //Clean logs
            var logs = Path.Combine(AutoCheck.Core.Utils.AppFolder, "logs");
            if(Directory.Exists(logs)) Directory.Delete(logs, true);     

            //Clean databases   
            if(false) 
            {
                //WARNING: Set condition to false in order to avoid BBDD testing on missconfigured hosts
                using(var psql = new AutoCheck.Core.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok1", "postgres", "postgres"))
                    if(psql.ExistsDataBase()) psql.DropDataBase();

                using(var psql = new AutoCheck.Core.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok2", "postgres", "postgres"))
                    if(psql.ExistsDataBase()) psql.DropDataBase();

                using(var psql = new AutoCheck.Core.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok31", "postgres", "postgres"))
                    if(psql.ExistsDataBase()) psql.DropDataBase();

                using(var psql = new AutoCheck.Core.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok32", "postgres", "postgres"))
                    if(psql.ExistsDataBase()) psql.DropDataBase();

                using(var psql = new AutoCheck.Core.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok4", "postgres", "postgres"))
                    if(psql.ExistsDataBase()) psql.DropDataBase();

                using(var psql = new AutoCheck.Core.Connectors.Postgres("localhost", "restoredb_ok5-dump1_sql", "postgres", "postgres"))
                    if(psql.ExistsDataBase()) psql.DropDataBase();

                using(var psql = new AutoCheck.Core.Connectors.Postgres("localhost", "restoredb_ok5-dump2_sql", "postgres", "postgres"))
                    if(psql.ExistsDataBase()) psql.DropDataBase(); 
            }

            //Clean GDrive
            if(false) 
            {
                //WARNING: Set condition to false in order to avoid GDrive testing on missconfigured hosts                
                using(var gdrive = new AutoCheck.Core.Connectors.GDrive(_user, _secret)){
                    gdrive.DeleteFolder("\\AutoCheck\\test", "uploadgdrive_ok1");
                    gdrive.DeleteFolder("\\AutoCheck\\test", "uploadgdrive_ok2");
                    gdrive.DeleteFolder("\\AutoCheck\\test", "uploadgdrive_ok3");
                }  
            }          
        }

#region Vars
        [Test, Category("Vars")]
        public void ParseVars_DEFAULT_VARS()
        {  
           Assert.DoesNotThrow(() => new AutoCheck.Core.Script(GetSampleFile("vars\\vars_ok1.yaml")));                 
        }

        [Test, Category("Vars")]
        public void ParseVars_COMPUTED_OPPERATION()
        {  
            //NOTE: needs a local GNU users to work (usuario@usuario)
            var s = new AutoCheck.Core.Script(GetSampleFile("vars\\vars_ok5.yaml"));             
            Assert.AreEqual("Executing script vars_ok5 (v1.0.0.0):\r\n   Running opperation 1+2+3: OK", s.Output.ToString());                          
        }
        
        [Test, Category("Vars")]
        public void ParseVars_COMPUTED_REGEX()
        {  
            Assert.DoesNotThrow(() => new AutoCheck.Core.Script(GetSampleFile("vars\\vars_ok2.yaml")));                                           
        }
 
        [Test, Category("Vars")]
        public void ParseVars_TYPED_SIMPLE()
        {                         
            Assert.DoesNotThrow(() => new AutoCheck.Core.Script(GetSampleFile("vars\\vars_ok3.yaml")));              
        }

        [Test, Category("Vars")]
        public void ParseVars_SCOPE_LEVEL1()
        {                         
            Assert.DoesNotThrow(() => new AutoCheck.Core.Script(GetSampleFile("vars\\vars_ok4.yaml")));              
        }
               
        [Test, Category("Vars")]
        public void ParseVars_INVALID_DUPLICATED()
        {  
            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.Script(GetSampleFile("vars\\vars_ko1.yaml")));            
        }

        [Test, Category("Vars")]
        public void ParseVars_NOTEXISTS_SIMPLE()
        {  
            Assert.Throws<VariableNotFoundException>(() => new AutoCheck.Core.Script(GetSampleFile("vars\\vars_ko2.yaml")));           
        }
 
        [Test, Category("Vars")]
        public void ParseVars_REGEX_NOTAPPLIED()
        {  
            Assert.Throws<RegexInvalidException>(() => new AutoCheck.Core.Script(GetSampleFile("vars\\vars_ko3.yaml")));
        }

        [Test, Category("Vars")]
        public void ParseVars_REGEX_NOVARNAME()
        {  
            Assert.Throws<VariableNotFoundException>(() => new AutoCheck.Core.Script(GetSampleFile("vars\\vars_ko4.yaml")));
        }

        [Test, Category("Vars")]
        public void ParseVars_REGEX_NOTEXISTS()
        {  
            Assert.Throws<VariableNotFoundException>(() => new AutoCheck.Core.Script(GetSampleFile("vars\\vars_ko5.yaml")));
        }

        [Test, Category("Vars")]
        public void ParseVars_SCOPE_NOTEXISTS()
        {  
            Assert.Throws<VariableNotFoundException>(() => new AutoCheck.Core.Script(GetSampleFile("vars\\vars_ko6.yaml")));
        }
#endregion
#region Pre
        [Test, Category("Pre"), Category("Zip")]
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

        [Test, Category("Pre"), Category("Zip")]
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

        [Test, Category("Pre"), Category("Zip")]
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

        [Test, Category("Pre"), Category("Zip")]
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

        [Test, Category("Pre"), Category("SQL")] 
        public void RestoreDB_SQL_NOREMOVE_NOOVERRIDE_NORECURSIVE()
        {              
            var dest = Path.Combine(GetSamplePath("script"), "temp", "restore", "test1");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);

            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "dump.sql"));          
            using(var psql = new AutoCheck.Core.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok1", "postgres", "postgres")){
                Assert.IsFalse(psql.ExistsDataBase());                
                var s = new AutoCheck.Core.Script(GetSampleFile("pre\\restore_db\\restoredb_ok1.yaml"));   
                
                Assert.IsTrue(psql.ExistsDataBase());
                Assert.IsTrue(File.Exists(GetSampleFile(dest, "dump.sql"))); 
                psql.DropDataBase();
            }
            
            Directory.Delete(dest, true);
        }

        [Test, Category("Pre"), Category("SQL")] 
        public void RestoreDB_SQL_REMOVE_NOOVERRIDE_NORECURSIVE()
        {              
            var dest = Path.Combine(GetSamplePath("script"), "temp", "restore", "test2");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);

            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "dump.sql"));                     
            using(var psql = new AutoCheck.Core.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok2", "postgres", "postgres")){
                Assert.IsFalse(psql.ExistsDataBase());                
                var s = new AutoCheck.Core.Script(GetSampleFile("pre\\restore_db\\restoredb_ok2.yaml"));   
                
                Assert.IsTrue(psql.ExistsDataBase());
                Assert.IsFalse(File.Exists(GetSampleFile(dest, "dump.sql"))); 
                psql.DropDataBase();
            } 

            Directory.Delete(dest, true);
        }

        [Test, Category("Pre"), Category("SQL")] 
        public void RestoreDB_SPECIFIC_BATCH()
        {              
            var dest = Path.Combine(GetSamplePath("script"), "temp", "restore", "test3");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
                        
            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "dump.sql"));                         
            using(var psql = new AutoCheck.Core.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok31", "postgres", "postgres")){
                Assert.IsFalse(psql.ExistsDataBase()); 
                var s = new AutoCheck.Core.Script(GetSampleFile("pre\\restore_db\\restoredb_ok3.1.yaml"));   //TODO: Should use a own file (not resue another test one...)   
                
                Assert.IsTrue(psql.ExistsDataBase());
                Assert.AreEqual(10, psql.ExecuteScalar<long>("SELECT COUNT (*) FROM test.work_history;"));
                psql.ExecuteNonQuery($"INSERT INTO test.work_history (id_employee, id_work, id_department) VALUES (999, 'MK_REP', 20);");
                Assert.AreEqual(11, psql.ExecuteScalar<long>("SELECT COUNT (*) FROM test.work_history;"));
            } 

            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "override.sql"));
            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "nooverride.sql"));
            using(var psql = new AutoCheck.Core.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok32", "postgres", "postgres")){
                Assert.IsFalse(psql.ExistsDataBase());                                
                var s = new AutoCheck.Core.Script(GetSampleFile("pre\\restore_db\\restoredb_ok3.2.yaml"));   
                
                Assert.IsTrue(psql.ExistsDataBase());
                Assert.IsTrue(File.Exists(GetSampleFile(dest, "nooverride.sql"))); 
                Assert.IsFalse(File.Exists(GetSampleFile(dest, "override.sql"))); 
                psql.DropDataBase();  
            }
            File.Delete(GetSampleFile(dest, "nooverride.sql"));

            using(var psql = new AutoCheck.Core.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok31", "postgres", "postgres")){
                Assert.IsTrue(psql.ExistsDataBase());                                
                Assert.AreEqual(10, psql.ExecuteScalar<long>("SELECT COUNT (*) FROM test.work_history;"));
                psql.DropDataBase();                
            } 

            Directory.Delete(dest, true);             
        }

        [Test, Category("Pre"), Category("SQL")] 
        public void RestoreDB_SPECIFIC_REMOVE_NOOVERRIDE_NORECURSIVE()
        {              
            var dest = Path.Combine(GetSamplePath("script"), "temp", "restore", "test4");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);

            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "nooverride.sql"));
            using(var psql = new AutoCheck.Core.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok4", "postgres", "postgres")){
                Assert.IsFalse(psql.ExistsDataBase()); 
                var s = new AutoCheck.Core.Script(GetSampleFile("pre\\restore_db\\restoredb_ok4.yaml"));
                Assert.IsTrue(psql.ExistsDataBase());                                
                 
                Assert.AreEqual(10, psql.ExecuteScalar<long>("SELECT COUNT (*) FROM test.work_history;"));
                psql.ExecuteNonQuery($"INSERT INTO test.work_history (id_employee, id_work, id_department) VALUES (999, 'MK_REP', 20);");
                Assert.AreEqual(11, psql.ExecuteScalar<long>("SELECT COUNT (*) FROM test.work_history;"));

                s = new AutoCheck.Core.Script(GetSampleFile("pre\\restore_db\\restoredb_ok4.yaml"));   
                
                Assert.IsTrue(psql.ExistsDataBase());
                Assert.IsFalse(File.Exists(GetSampleFile(dest, "nooverride.sql"))); 
                Assert.AreEqual(11, psql.ExecuteScalar<long>("SELECT COUNT (*) FROM test.work_history;"));
                psql.DropDataBase();      
            } 

            Directory.Delete(dest, true);          
        }

        [Test, Category("Pre"), Category("SQL")] 
        public void RestoreDB_SQL_REMOVE_NOOVERRIDE_RECURSIVE()
        {              
            var dest = Path.Combine(GetSamplePath("script"), "temp", "restore", "test5");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
           
            var rec = Path.Combine(dest, "recursive");
            if(!Directory.Exists(rec)) Directory.CreateDirectory(rec);

            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "dump1.sql"));
            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(rec, "dump2.sql"));
            
            using(var psql = new AutoCheck.Core.Connectors.Postgres("localhost", "restoredb_ok5-dump1_sql", "postgres", "postgres")){
                Assert.IsFalse(psql.ExistsDataBase());
                var s = new AutoCheck.Core.Script(GetSampleFile("pre\\restore_db\\restoredb_ok5.yaml"));                   

                Assert.IsTrue(psql.ExistsDataBase());
                Assert.IsFalse(File.Exists(GetSampleFile(dest, "dump1.sql"))); 
                psql.DropDataBase();
            }

            using(var psql = new AutoCheck.Core.Connectors.Postgres("localhost", "restoredb_ok5-dump2_sql", "postgres", "postgres")){
                Assert.IsTrue(psql.ExistsDataBase());
                Assert.IsFalse(File.Exists(GetSampleFile(rec, "dump2.sql"))); 
                psql.DropDataBase();
            }

            Directory.Delete(dest, true);
        }

        //TODO: RestoreDB_KO() testing something different to PSQL (SQL Server, MySQL/MariaDB, Oracle...)
        
        [Test, Category("Pre"), Category("GDrive")]
        public void UploadGDrive_NOREMOVE_UPLOAD_NOLINK_NORECURSIVE()
        {  
            var dest = Path.Combine(GetSamplePath("script"), "temp", "upload", "test1");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "uploaded.sql"));                    
            
            var remotePath = "\\AutoCheck\\test\\uploadgdrive_ok1";
            var remoteFile = "uploaded.sql";
            using(var gdrive = new AutoCheck.Core.Connectors.GDrive(_user, _secret)){
                Assert.IsFalse(gdrive.ExistsFolder(remotePath));                
                
                var s = new AutoCheck.Core.Script(GetSampleFile("pre\\upload_gdrive\\uploadgdrive_ok1.yaml"));                   
                System.Threading.Thread.Sleep(5000);

                Assert.IsTrue(File.Exists(GetSampleFile(dest, remoteFile))); 
                Assert.IsTrue(gdrive.ExistsFile(remotePath, remoteFile));
            } 
            
            Directory.Delete(dest, true);
        }

        [Test, Category("Pre"), Category("GDrive")]
        public void UploadGDrive_REMOVE_UPLOAD_NOLINK_RECURSIVE()
        {  
            //TODO: fails sometimes... need some waiting time to see the changes?
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
            using(var gdrive = new AutoCheck.Core.Connectors.GDrive(_user, _secret)){
                Assert.IsFalse(gdrive.ExistsFolder(remotePath));

                var s = new AutoCheck.Core.Script(GetSampleFile("pre\\upload_gdrive\\uploadgdrive_ok2.yaml"));   
                System.Threading.Thread.Sleep(5000);

                Assert.IsFalse(File.Exists(GetSampleFile(dest, remoteFile))); 
                Assert.IsFalse(Directory.Exists(rec));

                Assert.IsTrue(gdrive.ExistsFile(remotePath, remoteFile));
                Assert.IsTrue(gdrive.ExistsFolder(remotePath, "recursive"));
                Assert.IsTrue(gdrive.ExistsFile(remotePath2, remoteFile2));
            }            
        }

        [Test, Category("Pre"), Category("GDrive")]
        public void UploadGDrive_NOREMOVE_COPY_LINK_NORECURSIVE()
        {  
            var dest = Path.Combine(GetSamplePath("script"), "temp", "upload", "test3");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "uploaded.sql"));          
            
            var remotePath = "\\AutoCheck\\test\\uploadgdrive_ok3";
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);  
            
            File.Copy(GetSampleFile("gdrive", "download.txt"), GetSampleFile(dest, "download.txt"));
            using(var gdrive = new AutoCheck.Core.Connectors.GDrive(_user, _secret)){
                Assert.IsFalse(gdrive.ExistsFolder(remotePath));

                var s = new AutoCheck.Core.Script(GetSampleFile("pre\\upload_gdrive\\uploadgdrive_ok3.yaml"));                               
                System.Threading.Thread.Sleep(5000);

                Assert.IsTrue(gdrive.ExistsFile(remotePath, "1mb-test_zip.zip"));
                Assert.IsTrue(gdrive.ExistsFile(remotePath, "10mb.test"));
            }

            Directory.Delete(dest, true);
        }

        //TODO: UploadGDrive_KO() testing something unable to parse (read the PDF content for example, it will be supported in a near future, but not right now) or upload
#endregion
#region Connector
        [Test, Category("Connector")]
        public void ParseBody_CONNECTOR_EMPTY()
        {  
            Assert.DoesNotThrow(() => new AutoCheck.Core.Script(GetSampleFile("body\\connector\\connector_ok1.yaml")));                                         
        }

        [Test, Category("Connector")]
        public void ParseBody_CONNECTOR_INLINE_ARGS()
        {              
            Assert.DoesNotThrow(() => new AutoCheck.Core.Script(GetSampleFile("body\\connector\\connector_ok2.yaml")));            
        }

        [Test, Category("Connector")]
        public void ParseBody_CONNECTOR_TYPED_ARGS()
        {                          
            Assert.DoesNotThrow(() => new AutoCheck.Core.Script(GetSampleFile("body\\connector\\connector_ok3.yaml")));                                    
        }

        [Test, Category("Connector")]
        public void ParseBody_CONNECTOR_MULTI_LOAD()
        {                          
            Assert.DoesNotThrow(() => new AutoCheck.Core.Script(GetSampleFile("body\\connector\\connector_ok4.yaml")));                          
        }

        [Test, Category("Connector")]
        public void ParseBody_CONNECTOR_REMOTE_IP()
        {                          
            //Needs a localhost GNU user called usuario@usuario
            Assert.DoesNotThrow(() => new AutoCheck.Core.Script(GetSampleFile("body\\connector\\connector_ok5.yaml")));                          
        }

        [Test, Category("Connector")]
        public void ParseBody_CONNECTOR_IMPLICIT_INVALID_INLINE_ARGS()
        {  
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\connector\\connector_ko1.yaml"));
            Assert.AreEqual("Executing script connector_ko1 (v1.0.0.0):\r\n   Testing connector... ERROR:\n      -Unable to find any constructor for the Connector 'LocalShell' that matches with the given set of arguments.", s.Output.ToString());        
        }

        [Test, Category("Connector")]
        public void ParseBody_CONNECTOR_EXPLICIT_INVALID_INLINE_ARGS()
        {   
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\connector\\connector_ko2.yaml"));
            Assert.AreEqual("Executing script connector_ko2 (v1.0.0.0):\r\n   Testing connector... ERROR:\n      -Unable to find any constructor for the Connector 'Css' that matches with the given set of arguments.\r\n\r\n   Aborting execution!", s.Output.ToString());
        }

        [Test, Category("Connector")]
        public void ParseBody_CONNECTOR_EXPLICIT_INVALID_TYPED_ARGS()
        {  
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\connector\\connector_ko3.yaml"));
            Assert.AreEqual("Executing script connector_ko3 (v1.0.0.0):\r\n   Testing connector... ERROR:\n      -Unable to find any constructor for the Connector 'Odoo' that matches with the given set of arguments.\r\n\r\n   Aborting execution!", s.Output.ToString());                           
        }

        [Test, Category("Connector")]
        public void ParseBody_CONNECTOR_EXPLICIT_INVALID_ONEXCEPTION()
        {  
            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.Script(GetSampleFile("body\\connector\\connector_ko4.yaml")));
        }

        [Test, Category("Connector")]
        public void ParseBody_CONNECTOR_EXPLICIT_INVALID_SILENT()
        {  
            Assert.DoesNotThrow(() => new AutoCheck.Core.Script(GetSampleFile("body\\connector\\connector_ko5.yaml")));
        }
#endregion
#region Run       
        [Test, Category("Run")]
        public void ParseBody_RUN_ECHO()
        {  
            Assert.DoesNotThrow(() => new AutoCheck.Core.Script(GetSampleFile("body\\run\\run_ok1.yaml")));
        }
 
        [Test, Category("Run")]
        public void ParseBody_RUN_FIND()
        {          
            Assert.DoesNotThrow(() => new AutoCheck.Core.Script(GetSampleFile("body\\run\\run_ok2.yaml")));
        }

        [Test, Category("Run")]
        public void ParseBody_RUN_CAPTION_OK()
        {          
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\run\\run_ok3.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Executing script run_ok3 (v1.0.0.1):\r\n   Checking if file exists... OK", log);
        }

        [Test, Category("Run")]
        public void ParseBody_RUN_CAPTION_ERROR()
        {          
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\run\\run_ok4.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Executing script run_ok4 (v1.0.0.1):\r\n   Checking if file exists... OK\r\n   Counting folders... ERROR:\n      -Expected -> Wanted ERROR!; Found -> 0", log);
        }

        [Test, Category("Run")]
        public void ParseBody_RUN_CAPTION_EXCEPTION()
        {   
            Assert.Throws<ArgumentInvalidException>(() => new AutoCheck.Core.Script(GetSampleFile("body\\run\\run_ko3.yaml")));                
        }

        [Test, Category("Run")]
        public void ParseBody_RUN_NOCAPTION_EXCEPTION()
        {   
            Assert.Throws<ResultMismatchException>(() => new AutoCheck.Core.Script(GetSampleFile("body\\run\\run_ko4.yaml")));                
        } 

        [Test, Category("Run")]
        public void ParseBody_RUN_EMPTY()
        {  
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\run\\run_ok6.yaml"));
            Assert.AreEqual("Executing script run_ok6 (v1.0.0.0):", s.Output.ToString());
        }

        [Test, Category("Run")]
        public void ParseBody_RUN_INVALID_TYPED_ARGS()
        {  
            Assert.Throws<ArgumentInvalidException>(() => new AutoCheck.Core.Script(GetSampleFile("body\\run\\run_ko2.yaml")));            
        }

        [Test, Category("Run")]
        public void ParseBody_RUN_CONFLICTIVE_ARGS()
        {  
            Assert.DoesNotThrow(() => new AutoCheck.Core.Script(GetSampleFile("body\\run\\run_ok5.yaml")));            
        }

#endregion
#region Echo       
        [Test, Category("Echo")]
        public void ParseBody_ECHO_RUN()
        {  
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\echo\\echo_ok1.yaml"));
            Assert.AreEqual("Executing script echo_ok1 (v1.0.0.0):\r\n   ECHO", s.Output.ToString());
        }

        [Test, Category("Echo")]
        public void ParseBody_ECHO_CONTENT()
        {  
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\echo\\echo_ok2.yaml"));
            Assert.AreEqual("Executing script echo_ok2 (v1.0.0.0):\r\n   ECHO 1\r\n   Question 1 [1 point]:\r\n      ECHO 2\r\n\r\n   TOTAL SCORE: 10 / 10", s.Output.ToString());
        }
#endregion
#region Question
        [Test, Category("Question")]
        public void ParseBody_QUESTION_DEFAULT_SINGLE_ECHO()
        {                                      
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\question\\question_ok1.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Executing script question_ok1 (v1.0.0.0):\r\n   Question 1 [1 point]:\r\n      Running echo... OK\r\n\r\n   TOTAL SCORE: 10 / 10", log);
        }

        [Test, Category("Question")]
        public void ParseBody_QUESTION_DEFAULT_MULTI_ECHO()
        {              
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\question\\question_ok2.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Executing script question_ok2 (v1.0.0.0):\r\n   Question 1 [1 point]:\r\n      Running echo (1/2)... OK\r\n      Running echo (2/2)... OK\r\n\r\n   TOTAL SCORE: 10 / 10", log);
        }

        [Test, Category("Question")]
        public void ParseBody_QUESTION_DEFAULT_MULTI_METHODS()
        {              
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\question\\question_ok7.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Executing script question_ok7 (v1.0.0.1):\r\n   Question 1 [1 point]:\r\n      Checking files... OK\r\n      Getting files... OK\r\n\r\n   TOTAL SCORE: 10 / 10", log);
        }

        [Test, Category("Question")]
        public void ParseBody_QUESTION_BATCH_MULTI_ECHO()
        {              
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\question\\question_ok3.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Executing script question_ok3 (v1.0.0.0):\r\n   Question 1 [1 point]:\r\n      Running echo (1/2)... OK\r\n      Running echo (2/2)... OK\r\n\r\n   Question 2 [1 point]:\r\n      Running echo (1/2)... OK\r\n      Running echo (2/2)... ERROR:\n         -Expected -> Wanted fail!; Found -> This is NOT OK\r\n\r\n   TOTAL SCORE: 5 / 10", log);
        } 

        [Test, Category("Question")]
        public void ParseBody_QUESTION_BATCH_MULTI_MESSAGES()
        {              
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\question\\question_ok4.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Executing script question_ok4 (v1.0.0.0):\r\n   Question 1 [1 point]:\r\n      Running echo (1/2)... OK\r\n      Running echo (2/2)... ERROR:\n         -Expected -> Wanted fail!; Found -> Bye!\r\n\r\n   Question 2 [1 point]:\r\n      Running echo (1/2)... GREAT!\r\n      Running echo (2/2)... SO BAD!:\n         -Expected -> Wanted fail!; Found -> This is NOT OK\r\n\r\n   TOTAL SCORE: 0 / 10", log);
        }

        [Test, Category("Question")] 
        public void ParseBody_QUESTION_BATCH_MULTI_SCORE()
        {              
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\question\\question_ok5.yaml")); 
            var log = s.Output.ToString();
            Assert.AreEqual("Executing script question_ok5 (v1.0.0.0):\r\n   Question 1 [2 points]:\r\n      Running echo (1/2)... OK\r\n      Running echo (2/2)... OK\r\n\r\n   Question 2 [1 point]:\r\n      Running echo (1/2)... OK\r\n      Running echo (2/2)... ERROR:\n         -Expected -> Wanted fail!; Found -> This is NOT OK\r\n\r\n   TOTAL SCORE: 6.67 / 10", log);
        }

        [Test, Category("Question")]
        public void ParseBody_QUESTION_BATCH_MULTI_DESCRIPTION()
        {                                      
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\question\\question_ok6.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Executing script question_ok6 (v1.0.0.0):\r\n   My custom caption for the question 1 - My custom description with score 3/10 (TOTAL: 0):\r\n      Running echo (1/2)... OK\r\n      Running echo (2/2)... ERROR:\n         -Expected -> Error wanted!; Found -> Hello\r\n\r\n   My custom caption for the question 2 - My custom description with score 2/10 (TOTAL: 0):\r\n      Running echo... OK\r\n\r\n   My custom caption for the question 3 - My custom description with score 5/10 (TOTAL: 4):\r\n      Running echo (1/3)... OK\r\n      Running echo (2/3)... OK\r\n      Running echo (3/3)... OK\r\n\r\n   TOTAL SCORE: 7 / 10", log);
        }

        [Test, Category("Question")]
        public void ParseBody_QUESTION_BATCH_MULTI_METHODS()
        {              
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\question\\question_ok8.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Executing script question_ok8 (v1.0.0.1):\r\n   Question 1 [1 point]:\r\n      Checking files... OK\r\n      Getting files... OK\r\n\r\n   Question 2 [1 point]:\r\n      Counting folders... ERROR:\n         -Expected -> -1; Found -> 0\r\n\r\n   TOTAL SCORE: 5 / 10", log);
        } 

        [Test, Category("Question")]
        public void ParseBody_QUESTION_BATCH_SUBQUESTION_SINGLE()
        {                                      
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\question\\question_ok9.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Executing script question_ok9 (v1.0.0.0):\r\n   Question 1 [10 points]:\r\n\r\n      Question 1.1 [2 points]:\r\n         Running echo... OK\r\n\r\n      Question 1.2 [5 points]:\r\n         Running echo (1/3)... OK\r\n         Running echo (2/3)... OK\r\n         Running echo (3/3)... OK\r\n\r\n      Question 1.3 [3 points]:\r\n         Running echo... ERROR:\n            -Expected -> Wanted Error!; Found -> Hello\r\n\r\n   TOTAL SCORE: 7 / 10", log);
        }

        [Test, Category("Question")]
        public void ParseBody_QUESTION_BATCH_SUBQUESTION_MULTI()
        {                                      
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\question\\question_ok10.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Executing script question_ok10 (v1.0.0.0):\r\n   Question 1 [4 points]:\r\n\r\n      Question 1.1 [1 point]:\r\n         Running echo... OK\r\n\r\n      Question 1.2 [2 points]:\r\n\r\n         Question 1.2.1 [1 point]:\r\n            Running echo... OK\r\n\r\n         Question 1.2.2 [1 point]:\r\n            Running echo... OK\r\n\r\n      Question 1.3 [1 point]:\r\n         Running echo... ERROR:\n            -Expected -> Wanted Error!; Found -> Hello\r\n\r\n   Question 2 [3 points]:\r\n\r\n      Question 2.1 [1 point]:\r\n         Running echo... OK\r\n\r\n      Question 2.2 [1 point]:\r\n         Running echo (1/3)... OK\r\n         Running echo (2/3)... OK\r\n         Running echo (3/3)... OK\r\n\r\n      Question 2.3 [1 point]:\r\n         Running echo... ERROR:\n            -Expected -> Wanted Error!; Found -> Hello\r\n\r\n   TOTAL SCORE: 7.14 / 10", log);
        }
        
        [Test, Category("Question")]
        public void ParseBody_QUESTION_BATCH_SUBQUESTION_RUN()
        {                                      
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\question\\question_ok11.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Executing script question_ok11 (v1.0.0.0):\r\n   Question 1 [2 points]:\r\n\r\n      Question 1.1 [1 point]:\r\n         Running echo... OK\r\n\r\n      Question 1.2 [1 point]:\r\n         Running echo... ERROR:\n            -Expected -> Wanted Error!; Found -> Hello\r\n\r\n   TOTAL SCORE: 5 / 10", log);
        }   

        [Test, Category("Question")]
        public void ParseBody_QUESTION_BATCH_ONERROR_SKIP()
        {                                      
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\question\\question_ok12.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Executing script question_ok12 (v1.0.0.0):\r\n   Question 1 [2 points]:\r\n      Running echo One... OK\r\n\r\n      Question 1.1 [1 point]:\r\n         Running echo Two... ERROR:\n            -Expected -> WANTEDERROR; Found -> Two\r\n\r\n      Question 1.2 [1 point]:\r\n         Running echo Four... OK\r\n\r\n   TOTAL SCORE: 5 / 10", log);
        } 

        [Test, Category("Question")]
        public void ParseBody_QUESTION_BATCH_ONERROR_ABORT()
        {                                      
            var s = new AutoCheck.Core.Script(GetSampleFile("body\\question\\question_ok13.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Executing script question_ok13 (v1.0.0.0):\r\n   Question 1 [2 points]:\r\n      Running echo One... OK\r\n\r\n      Question 1.1 [1 point]:\r\n         Running echo Two... ERROR:\n            -Expected -> WANTEDERROR; Found -> Two\r\n\r\n\r\n   Aborting execution!\r\n\r\n   TOTAL SCORE: 0 / 10", log);
        }                      
#endregion
#region Inherits
        [Test, Category("Inherits")]
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

        [Test, Category("Inherits")]
        public void ParseBody_INHERITS_RUN_FOLDER()
        {       
            var dest = Path.Combine(GetSamplePath("script"), "temp", "inherits", "test2");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);                                 

            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest, "nopass.zip"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.zip")));
            var s = new AutoCheck.Core.Script(GetSampleFile("inherits\\inherits_run_ok1.yaml"));            
            
            Assert.AreEqual("Executing script inherits_run_ok1 (v1.0.0.1):", s.Output.ToString());
            Directory.Delete(dest, true);
        }
#endregion
#region Batch
        [Test, Category("Batch")]
        public void ParseBody_BATCH_RUN_FOLDER_SINGLE()
        {               
            var dest = Path.Combine(GetSamplePath("script"), "temp", "batch", "test1");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);                                 

            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest, "nopass.zip"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.zip")));
            
            var s = new AutoCheck.Core.Script(GetSampleFile("batch\\batch_run_ok1.yaml"));                        
            Assert.AreEqual($"Executing script batch_run_ok1 (v1.0.0.1):\r\n   Running on batch mode for {Path.GetFileName(dest)}:", s.Output.ToString());
            
            Directory.Delete(dest, true);
        }

        [Test, Category("Batch")]
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
            Assert.AreEqual($"Executing script batch_run_ok2 (v1.0.0.1):\r\n   Running on batch mode for {Path.GetFileName(dest1)}:\r\n   Running on batch mode for {Path.GetFileName(dest2)}:", s.Output.ToString());

            Directory.Delete(dest, true);
        }

        [Test, Category("Batch")]
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
            Assert.AreEqual($"Executing script batch_run_ok3 (v1.0.0.1):\r\n   Running on batch mode for {Path.GetFileName(dest1)}:\r\n   Running on batch mode for {Path.GetFileName(dest2)}:", s.Output.ToString());

            Directory.Delete(dest, true);
        }

        [Test, Category("Batch")]
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
            Assert.AreEqual($"Running script batch_run_ok4 for batch (v1.0.0.1):\r\n   Running on batch mode for {Path.GetFileName(dest1)}:\r\n   Running on batch mode for {Path.GetFileName(dest2)}:\r\n   Running on batch mode for {Path.GetFileName(dest1)}:\r\n   Running on batch mode for {Path.GetFileName(dest2)}:", s.Output.ToString());
            
            Directory.Delete(dest, true); 
        }

        [Test, Category("Batch")]
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
            Assert.AreEqual($"Executing script batch_run_ok5 (v1.0.0.1):\r\n   Running on batch mode for {Path.GetFileName(dest1)}:\r\n   Running on batch mode for {Path.GetFileName(dest2)}:\r\n   Running on batch mode for {Path.GetFileName(dest1)}:\r\n   Running on batch mode for {Path.GetFileName(dest2)}:", s.Output.ToString());
            
            Directory.Delete(dest, true);
        }

        [Test, Category("Batch")]
        public void ParseBody_BATCH_PRE_UNZIP()
        {               
            var dest =  Path.Combine(GetSamplePath("script"), "temp", "batch", "test6");         
            var dest1 = Path.Combine(dest, "folder1");
            var dest2 = Path.Combine(dest, "folder2");

            if(!Directory.Exists(dest1)) Directory.CreateDirectory(dest1);
            if(!Directory.Exists(dest2)) Directory.CreateDirectory(dest2);                                 

            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest1, "nopass.zip"));
            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest2, "nopass.zip"));

            Assert.IsTrue(File.Exists(GetSampleFile(dest1, "nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest2, "nopass.zip")));

            var s = new AutoCheck.Core.Script(GetSampleFile("batch\\batch_run_ok6.yaml"));            
            Assert.AreEqual($"Executing script batch_run_ok6 (v1.0.0.1):\r\n   Extracting files at: {Path.GetFileName(dest1)}\r\n      Extracting the file nopass.zip... OK\r\n\r\n   Extracting files at: {Path.GetFileName(dest2)}\r\n      Extracting the file nopass.zip... OK\r\n\r\n   Starting the copy detector for PlainText:\r\n      Looking for potential copies within {Path.GetFileName(dest1)}... OK\r\n      Looking for potential copies within {Path.GetFileName(dest2)}... OK\r\n\r\n   Running on batch mode for {Path.GetFileName(dest1)}:\r\n   Running on batch mode for {Path.GetFileName(dest2)}:", s.Output.ToString());
            
            Directory.Delete(dest, true);
        }
#endregion
#region Copy detector
        [Test, Category("Copy")]
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
            Assert.AreEqual($"Running script copy_plaintext_ok1 for copy:\r\n   Starting the copy detector for PLAINTEXT:\r\n      Looking for potential copies within folder1... OK\r\n      Looking for potential copies within folder2... OK\r\n\r\n   Running on batch mode for {Path.GetFileName(dest1)}:\r\n      Potential copy detected for folder1\\sample1.txt:\r\n         Match score with folder2\\sample2.txt... 100,00 % \r\n   Running on batch mode for {Path.GetFileName(dest2)}:\r\n      Potential copy detected for folder2\\sample2.txt:\r\n         Match score with folder1\\sample1.txt... 100,00 %", s.Output.ToString());            
            Directory.Delete(dest, true);
        }

        [Test, Category("Copy")]
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
            Assert.AreEqual($"Running script copy_plaintext_ok2 for copy:\r\n   Starting the copy detector for PLAINTEXT:\r\n      Looking for potential copies within folder1... OK\r\n      Looking for potential copies within folder2... OK\r\n\r\n   Running on batch mode for {Path.GetFileName(dest1)}:\r\n   Running on batch mode for {Path.GetFileName(dest2)}:", s.Output.ToString());            
            Directory.Delete(dest, true);
        }
#endregion
#region Dummy script testing
        [Test, Category("Dummy")]
        public void ParseBody_SCRIPT_SINGLE_OK1()
        {    
            var dest =  Path.Combine(GetSamplePath("script"), "temp", "script", "test1");                        
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
            
            File.Copy(GetSampleFile("html", "correct.html"), GetSampleFile(dest, "index.html"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "index.html")));

            var s = new AutoCheck.Core.Script(GetSampleFile("base\\script_single_1.yaml"));             
            Assert.AreEqual("Executing script Test Script #1 (v1.0.0.1):\r\n   Question 1 [2 points] - Checking Index.html:\r\n      Validating document against the W3C validation service... OK\r\n\r\n      Question 1.1 [1 point] - Validating headers:\r\n         Checking amount of level-1 headers... OK\r\n         Checking amount of level-2 headers... OK\r\n\r\n      Question 1.2 [1 point] - Validating paragraphs:\r\n         Checking amount of paragraphs... OK\r\n         Checking content legth within paragraphs... ERROR:\n            -Expected -> >=1500; Found -> 144\r\n\r\n   TOTAL SCORE: 5 / 10", s.Output.ToString());            
        }

        [Test, Category("Dummy")]
        public void ParseBody_SCRIPT_SINGLE_ONEXCEPTION_ABORT()
        {    
            var dest =  Path.Combine(GetSamplePath("script"), "temp", "script", "test2");                        
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
            
            File.Copy(GetSampleFile("html", "incorrect.html"), GetSampleFile(dest, "index.html"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "index.html")));

            var s = new AutoCheck.Core.Script(GetSampleFile("base\\script_single_2.yaml"));             
            Assert.AreEqual("Executing script Test Script #1 (v1.0.0.1):\r\n   Question 1 [2 points] - Checking Index.html:\r\n      Validating document against the W3C validation service... ERROR:\n         -No p element in scope but a p end tag seen.</h1>\n             </p>\n         </bod\r\n\r\n   Aborting execution!\r\n\r\n   TOTAL SCORE: 0 / 10", s.Output.ToString());            
        }

        [Test, Category("Dummy")]
        public void ParseBody_SCRIPT_SINGLE_ONEXCEPTION_ERROR()
        {    
            var dest =  Path.Combine(GetSamplePath("script"), "temp", "script", "test3");                        
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
            
            File.Copy(GetSampleFile("html", "incorrect.html"), GetSampleFile(dest, "index.html"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "index.html")));

            var s = new AutoCheck.Core.Script(GetSampleFile("base\\script_single_3.yaml"));             
            Assert.AreEqual("Executing script Test Script #2 (v1.0.0.1):\r\n   Question 1 [2 points] - Checking Index.html:\r\n      Validating document against the W3C validation service... ERROR:\n         -No p element in scope but a p end tag seen.</h1>\n             </p>\n         </bod\r\n\r\n      Question 1.1 [1 point] - Validating headers:\r\n         Checking amount of level-1 headers... OK\r\n         Checking amount of level-2 headers... ERROR:\n            -Expected -> >=1; Found -> 0\r\n\r\n      Question 1.2 [1 point] - Validating paragraphs:\r\n         Checking amount of paragraphs... OK\r\n         Checking content legth within paragraphs... ERROR:\n            -Expected -> >=1500; Found -> 10\r\n\r\n   TOTAL SCORE: 0 / 10", s.Output.ToString());            
        }

        [Test, Category("Dummy")]
        public void ParseBody_SCRIPT_SINGLE_ONEXCEPTION_SUCCESS()
        {    
            var dest =  Path.Combine(GetSamplePath("script"), "temp", "script", "test4");                        
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
            
            File.Copy(GetSampleFile("html", "incorrect.html"), GetSampleFile(dest, "index.html"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "index.html")));

            var s = new AutoCheck.Core.Script(GetSampleFile("base\\script_single_4.yaml"));             
            Assert.AreEqual("Executing script Test Script #3 (v1.0.0.1):\r\n   Question 1 [2 points] - Checking Index.html:\r\n      Validating document against the W3C validation service... OK\r\n\r\n      Question 1.1 [1 point] - Validating headers:\r\n         Checking amount of level-1 headers... OK\r\n         Checking amount of level-2 headers... ERROR:\n            -Expected -> >=1; Found -> 0\r\n\r\n      Question 1.2 [1 point] - Validating paragraphs:\r\n         Checking amount of paragraphs... OK\r\n         Checking content legth within paragraphs... ERROR:\n            -Expected -> >=1500; Found -> 10\r\n\r\n   TOTAL SCORE: 0 / 10", s.Output.ToString());            
        }

        [Test, Category("Dummy")]
        public void ParseBody_SCRIPT_SINGLE_ONEXCEPTION_SKIP()
        {    
            var dest =  Path.Combine(GetSamplePath("script"), "temp", "script", "test5");                        
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
            
            File.Copy(GetSampleFile("html", "incorrect.html"), GetSampleFile(dest, "index.html"));
            File.Copy(GetSampleFile("html", "correct.html"), GetSampleFile(dest, "contact.html"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "index.html")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "contact.html")));

            var s = new AutoCheck.Core.Script(GetSampleFile("base\\script_single_5.yaml"));             
            Assert.AreEqual("Executing script Test Script #4 (v1.0.0.1):\r\n   Question 1 [2 points] - Checking Index.html:\r\n      Validating document against the W3C validation service... ERROR:\n         -No p element in scope but a p end tag seen.</h1>\n             </p>\n         </bod\r\n\r\n   Question 2 [2 points] - Checking Contact.html:\r\n      Validating document against the W3C validation service... OK\r\n\r\n      Question 2.1 [1 point] - Validating headers:\r\n         Checking amount of level-1 headers... OK\r\n         Checking amount of level-2 headers... OK\r\n\r\n      Question 2.2 [1 point] - Validating paragraphs:\r\n         Checking amount of paragraphs... OK\r\n         Checking content legth within paragraphs... ERROR:\n            -Expected -> >=1500; Found -> 144\r\n\r\n   TOTAL SCORE: 2.5 / 10", s.Output.ToString());            
        }  

        [Test, Category("Dummy")]
        public void ParseBody_SCRIPT_SINGLE_ONEXCEPTION_NOCAPTION()
        {    
            var dest =  Path.Combine(GetSamplePath("script"), "temp", "script", "test6");                        
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
            
            File.Copy(GetSampleFile("html", "incorrect.html"), GetSampleFile(dest, "index.html"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "index.html")));

            Assert.Throws<DocumentInvalidException>(() => new AutoCheck.Core.Script(GetSampleFile("base\\script_single_6.yaml")));            
        }

        [Test, Category("Dummy")]
        public void ParseBody_SCRIPT_ARGUMENT_TYPE_CONNECTOR_OK()
        {    
            var dest =  Path.Combine(GetSamplePath("script"), "temp", "script", "test7");                        
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
            
            File.Copy(GetSampleFile("html", "correct.html"), GetSampleFile(dest, "index.html"));
            File.Copy(GetSampleFile("css", "correct.css"), GetSampleFile(dest, "index.css"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "index.html")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "index.css")));

            var s = new AutoCheck.Core.Script(GetSampleFile("base\\script_single_7.yaml"));             
            Assert.AreEqual("Executing script Test Script #6 (v1.0.0.1):\r\n   Question 1 [1 point] - Checking index.css:\r\n      Validating document against the W3C validation service... OK\r\n\r\n      Question 1.1 [1 point] - Validating font property:\r\n         Checking if the font property has been created... OK\r\n         Checking if the font property has NOT been applied... OK\r\n\r\n   TOTAL SCORE: 10 / 10", s.Output.ToString());            
        }

        [Test, Category("Dummy")]
        public void ParseBody_SCRIPT_ARGUMENT_TYPE_CONNECTOR_KO()
        {    
            var dest =  Path.Combine(GetSamplePath("script"), "temp", "script", "test8");                        
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
            
            File.Copy(GetSampleFile("html", "correct.html"), GetSampleFile(dest, "index.html"));
            File.Copy(GetSampleFile("css", "correct.css"), GetSampleFile(dest, "index.css"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "index.html")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "index.css")));

            var s = new AutoCheck.Core.Script(GetSampleFile("base\\script_single_8.yaml"));
            Assert.AreEqual("Executing script Test Script #7 (v1.0.0.1):\r\n   Question 1 [1 point] - Checking index.css:\r\n      Validating document against the W3C validation service... OK\r\n\r\n      Question 1.1 [1 point] - Validating font property:\r\n         Checking if the font property has been created... OK\r\n         Checking if the font property has NOT been applied... ERROR:\n            -Unable to find any connector named 'Html'.\r\n\r\n   TOTAL SCORE: 0 / 10", s.Output.ToString());
        }
       
        [Test, Category("Dummy")]
        public void ParseBody_SCRIPT_ARGUMENT_TYPE_CONNECTOR_TUPLE()
        {    
            var dest =  Path.Combine(GetSamplePath("script"), "temp", "script", "test9");                        
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
            
            File.Copy(GetSampleFile("html", "correct.html"), GetSampleFile(dest, "index.html"));
            File.Copy(GetSampleFile("css", "correct.css"), GetSampleFile(dest, "index.css"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "index.html")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "index.css")));

            var s = new AutoCheck.Core.Script(GetSampleFile("base\\script_single_9.yaml"));             
            Assert.AreEqual("Executing script Test Script #8 (v1.0.0.1):\r\n   Question 1 [1 point] - Checking index.css:\r\n      Validating document against the W3C validation service... OK\r\n\r\n      Question 1.1 [1 point] - Validating set of properties:\r\n         Checking if the (top | right | bottom | left) property has been created... OK\r\n\r\n   TOTAL SCORE: 10 / 10", s.Output.ToString());            
        } 
#endregion
#region Real script testing: XML Validation    
        [Test, Category("FullScriptXml"), Category("Real")]    
        public void Full_XML_SCRIPT_SINGLE_1() 
        {             
            var s = new AutoCheck.Core.Script(Path.Combine(GetSamplePath("script"), "targets", "xml_single_1.yaml"));                         
            Assert.AreEqual("Running script 'DAM - M04 (UF1): XML Validation Assignment (Namespaces + DTD + XSD)' in single mode for 'Student Name 1' (v1.0.0.2):\r\n   Question 1 [4 points] - Starting validation over file1.xml:\r\n      Looking for file1.xml... OK\r\n      Loading file1.xml... OK\r\n      Checking amount of nodes... OK\r\n      Checking node types... OK\r\n      Checking amount of attributes... OK\r\n      Checking attribute types... OK\r\n      Checking repeated nodes... OK\r\n\r\n      Question 1.1 [4 points] - Starting validation over file1.dtd:\r\n         Looking for file1.dtd... OK\r\n         Looking for comments... OK\r\n\r\n         Question 1.1.1 [2 points] - Content validation:\r\n            Checking document's content... OK\r\n\r\n         Question 1.1.2 [2 points] - Comments validation:\r\n            Checking document's comments... OK\r\n\r\n   Question 2 [4 points] - Starting validation over file2.xml:\r\n      Looking for file1.xml... OK\r\n      Loading file1.xml... OK\r\n      Looking for file2.xml... OK\r\n      Loading file2.xml... OK\r\n      Checking that file1.xml and file2.xml are using the same hierarchy... OK\r\n\r\n      Question 2.1 [4 points] - Starting validation over file2.xsd:\r\n         Looking for file2.xsd... OK\r\n         Loading file2.xsd... OK\r\n         Looking for comments... OK\r\n\r\n         Question 2.1.1 [2 points] - Content validation:\r\n            Checking document's content... OK\r\n\r\n         Question 2.1.2 [2 points] - Comments validation:\r\n            Checking document's comments... OK\r\n\r\n   Question 3 [2 points] - Starting validation over file3.xml:\r\n      Looking for file3.xml... OK\r\n      Loading file3.xml... OK\r\n      Looking for comments... OK\r\n\r\n      Question 3.1 [1 point] - Content validation:\r\n         Checking for a default namespace... OK\r\n         Checking for a custom namespaces... OK\r\n         Checking for a root node with using the default namespace... OK\r\n         Checking for the amount of nodes using the first namespace... OK\r\n         Checking for the amount of nodes using the second namespace... OK\r\n\r\n      Question 3.2 [1 point] - Comments validation:\r\n         Checking document's comments... OK\r\n\r\n   TOTAL SCORE: 10 / 10", s.Output.ToString());
        } 

        [Test, Category("FullScriptXml"), Category("Real")]    
        public void Full_XML_SCRIPT_SINGLE_2()
        {             
            var s = new AutoCheck.Core.Script(Path.Combine(GetSamplePath("script"), "targets", "xml_single_2.yaml"));                         
            Assert.AreEqual("Running script 'DAM - M04 (UF1): XML Validation Assignment (Namespaces + DTD + XSD)' in single mode for 'Student Name 2' (v1.0.0.2):\r\n   Question 1 [4 points] - Starting validation over file1.xml:\r\n      Looking for file1.xml... OK\r\n      Loading file1.xml... OK\r\n      Checking amount of nodes... OK\r\n      Checking node types... OK\r\n      Checking amount of attributes... ERROR:\n         -Expected -> >=5; Found -> 3\r\n\r\n   Question 2 [4 points] - Starting validation over file2.xml:\r\n      Looking for file1.xml... OK\r\n      Loading file1.xml... OK\r\n      Looking for file2.xml... OK\r\n      Loading file2.xml... OK\r\n      Checking that file1.xml and file2.xml are using the same hierarchy... OK\r\n\r\n      Question 2.1 [4 points] - Starting validation over file2.xsd:\r\n         Looking for file2.xsd... OK\r\n         Loading file2.xsd... OK\r\n         Looking for comments... OK\r\n\r\n         Question 2.1.1 [2 points] - Content validation:\r\n            Checking document's content... OK\r\n\r\n         Question 2.1.2 [2 points] - Comments validation:\r\n            Checking document's comments... OK\r\n\r\n   Question 3 [2 points] - Starting validation over file3.xml:\r\n      Looking for file3.xml... OK\r\n      Loading file3.xml... OK\r\n      Looking for comments... OK\r\n\r\n      Question 3.1 [1 point] - Content validation:\r\n         Checking for a default namespace... OK\r\n         Checking for a custom namespaces... OK\r\n         Checking for a root node with using the default namespace... OK\r\n         Checking for the amount of nodes using the first namespace... ERROR:\n            -Expected -> =25; Found -> 24\r\n         Checking for the amount of nodes using the second namespace... ERROR:\n            -Expected -> =25; Found -> 24\r\n\r\n      Question 3.2 [1 point] - Comments validation:\r\n         Checking document's comments... OK\r\n\r\n   TOTAL SCORE: 5 / 10", s.Output.ToString());
        } 

        [Test, Category("FullScriptXml"), Category("Real")]     
        public void Full_XML_SCRIPT_SINGLE_3()
        {              
            var s = new AutoCheck.Core.Script(Path.Combine(GetSamplePath("script"), "targets", "xml_single_3.yaml"));                         
            Assert.AreEqual("Running script 'DAM - M04 (UF1): XML Validation Assignment (Namespaces + DTD + XSD)' in single mode for 'Student Name 9' (v1.0.0.2):\r\n   Question 1 [4 points] - Starting validation over file1.xml:\r\n      Looking for file1.xml... OK\r\n      Loading file1.xml... OK\r\n      Checking amount of nodes... ERROR:\n         -Expected -> >=15; Found -> 13\r\n\r\n   Question 2 [4 points] - Starting validation over file2.xml:\r\n      Looking for file1.xml... OK\r\n      Loading file1.xml... OK\r\n      Looking for file2.xml... ERROR:\n         -Expected -> %file2.xml; Found -> NULL\r\n\r\n   Question 3 [2 points] - Starting validation over file3.xml:\r\n      Looking for file3.xml... ERROR:\n         -Expected -> %file3.xml; Found -> NULL\r\n\r\n   TOTAL SCORE: 0 / 10", s.Output.ToString());
        }  

        [Test, Category("FullScriptXml"), Category("Real")]    
        public void Full_XML_SCRIPT_BATCH() 
        {                         
            var s = new AutoCheck.Core.Script(Path.Combine(GetSamplePath("script"), "targets", "xml_local_batch.yaml"));                         
            Assert.AreEqual("Executing script DAM - M04 (UF1): XML Validation Assignment (Namespaces + DTD + XSD) (v1.0.0.2):\r\n   Starting the copy detector for XML:\r\n      Looking for potential copies within Student Name 1... OK\r\n      Looking for potential copies within Student Name 2... OK\r\n      Looking for potential copies within Student Name 3... OK\r\n      Looking for potential copies within Student Name 4... OK\r\n      Looking for potential copies within Student Name 5... OK\r\n      Looking for potential copies within Student Name 6... OK\r\n      Looking for potential copies within Student Name 7... OK\r\n      Looking for potential copies within Student Name 8... OK\r\n      Looking for potential copies within Student Name 9... OK\r\n\r\n   Running on batch mode for Student Name 1:\r\n      Question 1 [4 points] - Starting validation over file1.xml:\r\n         Looking for file1.xml... OK\r\n         Loading file1.xml... OK\r\n         Checking amount of nodes... OK\r\n         Checking node types... OK\r\n         Checking amount of attributes... OK\r\n         Checking attribute types... OK\r\n         Checking repeated nodes... OK\r\n\r\n         Question 1.1 [4 points] - Starting validation over file1.dtd:\r\n            Looking for file1.dtd... OK\r\n            Looking for comments... OK\r\n\r\n            Question 1.1.1 [2 points] - Content validation:\r\n               Checking document's content... OK\r\n\r\n            Question 1.1.2 [2 points] - Comments validation:\r\n               Checking document's comments... OK\r\n\r\n      Question 2 [4 points] - Starting validation over file2.xml:\r\n         Looking for file1.xml... OK\r\n         Loading file1.xml... OK\r\n         Looking for file2.xml... OK\r\n         Loading file2.xml... OK\r\n         Checking that file1.xml and file2.xml are using the same hierarchy... OK\r\n\r\n         Question 2.1 [4 points] - Starting validation over file2.xsd:\r\n            Looking for file2.xsd... OK\r\n            Loading file2.xsd... OK\r\n            Looking for comments... OK\r\n\r\n            Question 2.1.1 [2 points] - Content validation:\r\n               Checking document's content... OK\r\n\r\n            Question 2.1.2 [2 points] - Comments validation:\r\n               Checking document's comments... OK\r\n\r\n      Question 3 [2 points] - Starting validation over file3.xml:\r\n         Looking for file3.xml... OK\r\n         Loading file3.xml... OK\r\n         Looking for comments... OK\r\n\r\n         Question 3.1 [1 point] - Content validation:\r\n            Checking for a default namespace... OK\r\n            Checking for a custom namespaces... OK\r\n            Checking for a root node with using the default namespace... OK\r\n            Checking for the amount of nodes using the first namespace... OK\r\n            Checking for the amount of nodes using the second namespace... OK\r\n\r\n         Question 3.2 [1 point] - Comments validation:\r\n            Checking document's comments... OK\r\n\r\n      TOTAL SCORE: 10 / 10\r\n   Running on batch mode for Student Name 2:\r\n      Question 1 [4 points] - Starting validation over file1.xml:\r\n         Looking for file1.xml... OK\r\n         Loading file1.xml... OK\r\n         Checking amount of nodes... OK\r\n         Checking node types... OK\r\n         Checking amount of attributes... ERROR:\n            -Expected -> >=5; Found -> 3\r\n\r\n      Question 2 [4 points] - Starting validation over file2.xml:\r\n         Looking for file1.xml... OK\r\n         Loading file1.xml... OK\r\n         Looking for file2.xml... OK\r\n         Loading file2.xml... OK\r\n         Checking that file1.xml and file2.xml are using the same hierarchy... OK\r\n\r\n         Question 2.1 [4 points] - Starting validation over file2.xsd:\r\n            Looking for file2.xsd... OK\r\n            Loading file2.xsd... OK\r\n            Looking for comments... OK\r\n\r\n            Question 2.1.1 [2 points] - Content validation:\r\n               Checking document's content... OK\r\n\r\n            Question 2.1.2 [2 points] - Comments validation:\r\n               Checking document's comments... OK\r\n\r\n      Question 3 [2 points] - Starting validation over file3.xml:\r\n         Looking for file3.xml... OK\r\n         Loading file3.xml... OK\r\n         Looking for comments... OK\r\n\r\n         Question 3.1 [1 point] - Content validation:\r\n            Checking for a default namespace... OK\r\n            Checking for a custom namespaces... OK\r\n            Checking for a root node with using the default namespace... OK\r\n            Checking for the amount of nodes using the first namespace... ERROR:\n               -Expected -> =25; Found -> 24\r\n            Checking for the amount of nodes using the second namespace... ERROR:\n               -Expected -> =25; Found -> 24\r\n\r\n         Question 3.2 [1 point] - Comments validation:\r\n            Checking document's comments... OK\r\n\r\n      TOTAL SCORE: 5 / 10\r\n   Running on batch mode for Student Name 3:\r\n      Potential copy detected for Student Name 3\\file1.xml:\r\n         Match score with Student Name 1\\file1.xml... 33,13 % \r\n         Match score with Student Name 2\\file1.xml... 27,89 % \r\n         Match score with Student Name 4\\file1.xml... 100,00 % \r\n         Match score with Student Name 5\\file1.xml... 78,89 % \r\n         Match score with Student Name 6\\file1.xml... 77,78 % \r\n         Match score with Student Name 7\\file1.xml... 77,78 % \r\n         Match score with Student Name 8\\file1.xml... 77,78 % \r\n         Match score with Student Name 9\\file1.xml... 74,10 % \r\n   Running on batch mode for Student Name 4:\r\n      Potential copy detected for Student Name 4\\file1.xml:\r\n         Match score with Student Name 1\\file1.xml... 33,13 % \r\n         Match score with Student Name 2\\file1.xml... 27,89 % \r\n         Match score with Student Name 3\\file1.xml... 100,00 % \r\n         Match score with Student Name 5\\file1.xml... 78,89 % \r\n         Match score with Student Name 6\\file1.xml... 77,78 % \r\n         Match score with Student Name 7\\file1.xml... 77,78 % \r\n         Match score with Student Name 8\\file1.xml... 77,78 % \r\n         Match score with Student Name 9\\file1.xml... 74,10 % \r\n   Running on batch mode for Student Name 5:\r\n      Potential copy detected for Student Name 5\\file1.xml:\r\n         Match score with Student Name 1\\file1.xml... 33,13 % \r\n         Match score with Student Name 2\\file1.xml... 27,89 % \r\n         Match score with Student Name 3\\file1.xml... 78,89 % \r\n         Match score with Student Name 4\\file1.xml... 78,89 % \r\n         Match score with Student Name 6\\file1.xml... 94,44 % \r\n         Match score with Student Name 7\\file1.xml... 91,11 % \r\n         Match score with Student Name 8\\file1.xml... 91,11 % \r\n         Match score with Student Name 9\\file1.xml... 86,57 % \r\n   Running on batch mode for Student Name 6:\r\n      Potential copy detected for Student Name 6\\file1.xml:\r\n         Match score with Student Name 1\\file1.xml... 33,13 % \r\n         Match score with Student Name 2\\file1.xml... 27,89 % \r\n         Match score with Student Name 3\\file1.xml... 77,78 % \r\n         Match score with Student Name 4\\file1.xml... 77,78 % \r\n         Match score with Student Name 5\\file1.xml... 94,44 % \r\n         Match score with Student Name 7\\file1.xml... 92,22 % \r\n         Match score with Student Name 8\\file1.xml... 91,11 % \r\n         Match score with Student Name 9\\file1.xml... 86,57 % \r\n   Running on batch mode for Student Name 7:\r\n      Potential copy detected for Student Name 7\\file1.xml:\r\n         Match score with Student Name 1\\file1.xml... 33,13 % \r\n         Match score with Student Name 2\\file1.xml... 27,89 % \r\n         Match score with Student Name 3\\file1.xml... 77,78 % \r\n         Match score with Student Name 4\\file1.xml... 77,78 % \r\n         Match score with Student Name 5\\file1.xml... 91,11 % \r\n         Match score with Student Name 6\\file1.xml... 92,22 % \r\n         Match score with Student Name 8\\file1.xml... 94,44 % \r\n         Match score with Student Name 9\\file1.xml... 89,69 % \r\n   Running on batch mode for Student Name 8:\r\n      Potential copy detected for Student Name 8\\file1.xml:\r\n         Match score with Student Name 1\\file1.xml... 33,13 % \r\n         Match score with Student Name 2\\file1.xml... 27,89 % \r\n         Match score with Student Name 3\\file1.xml... 77,78 % \r\n         Match score with Student Name 4\\file1.xml... 77,78 % \r\n         Match score with Student Name 5\\file1.xml... 91,11 % \r\n         Match score with Student Name 6\\file1.xml... 91,11 % \r\n         Match score with Student Name 7\\file1.xml... 94,44 % \r\n         Match score with Student Name 9\\file1.xml... 94,88 % \r\n   Running on batch mode for Student Name 9:\r\n      Potential copy detected for Student Name 9\\file1.xml:\r\n         Match score with Student Name 1\\file1.xml... 37,36 % \r\n         Match score with Student Name 2\\file1.xml... 31,24 % \r\n         Match score with Student Name 3\\file1.xml... 74,10 % \r\n         Match score with Student Name 4\\file1.xml... 74,10 % \r\n         Match score with Student Name 5\\file1.xml... 86,57 % \r\n         Match score with Student Name 6\\file1.xml... 86,57 % \r\n         Match score with Student Name 7\\file1.xml... 89,69 % \r\n         Match score with Student Name 8\\file1.xml... 94,88 %", s.Output.ToString());
        }  
#endregion
#region Real script testing: HTML5
        [Test, Category("FullScriptHtml5"), Category("Real")]
        public void Full_HTML5_SCRIPT_SINGLE_1() 
        {             
            var s = new AutoCheck.Core.Script(Path.Combine(GetSamplePath("script"), "targets", "html5_single_1.yaml"));                        
            Assert.AreEqual("Running script 'DAM - M04 (UF1): HTML5 Assignment' in single mode for 'Student Name 1' (v1.0.0.2):\r\n   Question 1 [4 points] - Checking index.html:\r\n      Looking for index.html... OK\r\n      Loading index.html... OK\r\n      Validating document against the W3C validation service... OK\r\n\r\n      Question 1.1 [1 point] - Validating headers:\r\n         Checking amount of level-1 headers... OK\r\n         Checking amount of level-2 headers... OK\r\n\r\n      Question 1.2 [1 point] - Validating paragraphs:\r\n         Checking amount of paragraphs... OK\r\n         Checking content legth within paragraphs... OK\r\n\r\n      Question 1.3 [1 point] - Validating breaklines:\r\n         Checking amount of breaklines within a paragraph... OK\r\n\r\n      Question 1.4 [1 point] - Validating images:\r\n         Checking amount of images... OK\r\n\r\n   Question 2 [12 points] - Checking contacte.html:\r\n      Looking for contacte.html... OK\r\n      Loading contacte.html... OK\r\n      Validating document against the W3C validation service... OK\r\n\r\n      Question 2.1 [1 point] - Validating text fields:\r\n         Checking amount of text fields... OK\r\n\r\n      Question 2.2 [1 point] - Validating numeric fields:\r\n         Checking amount of numeric fields... OK\r\n\r\n      Question 2.3 [1 point] - Validating email fields:\r\n         Checking amount of email fields... OK\r\n\r\n      Question 2.4 [1 point] - Validating radio fields:\r\n         Checking amount of radio fields... OK\r\n         Checking group for the radio fields... OK\r\n         Checking the checked radio fields... OK\r\n\r\n      Question 2.5 [1 point] - Validating select fields:\r\n         Checking amount of select fields... OK\r\n         Checking select options... OK\r\n         Checking the selected option... OK\r\n\r\n      Question 2.6 [1 point] - Validating checkbox fields:\r\n         Checking amount of checkbox fields... OK\r\n         Checking group for the checkbox fields... OK\r\n         Checking the checked option... OK\r\n\r\n      Question 2.7 [1 point] - Validating textarea fields:\r\n         Checking amount of textarea fields... OK\r\n\r\n      Question 2.8 [1 point] - Validating placeholders:\r\n         Checking amount of placelhoders for text fields... OK\r\n         Checking amount of placelhoders for email fields... OK\r\n         Checking amount of placelhoders for numeric fields... OK\r\n         Checking amount of placelhoders for textarea fields... OK\r\n\r\n      Question 2.9 [1 point] - Validating labels:\r\n         Checking amount of labels for text fields... OK\r\n         Checking amount of labels for numeric fields... OK\r\n         Checking amount of labels for email fields... OK\r\n         Checking amount of labels for radio fields... OK\r\n         Checking amount of labels for select fields... OK\r\n         Checking amount of labels for check fields... OK\r\n         Checking amount of labels for textarea fields... OK\r\n\r\n      Question 2.10 [1 point] - Validating table:\r\n         Checking amount of columns... OK\r\n         Checking amount of merged columns... OK\r\n         Checking amount of labels within the first column... OK\r\n         Checking amount of labels within the second column... OK\r\n         Checking amount of labels within the third column... OK\r\n         Checking amount of labels within the fourth column... OK\r\n         Checking table's consistency... OK\r\n\r\n      Question 2.11 [1 point] - Validating form reset:\r\n         Checking amount of reset buttons... OK\r\n\r\n      Question 2.12 [1 point] - Validating form submit:\r\n         Checking amount of fields with no name... OK\r\n         Checking amount of submit buttons... OK\r\n         Checking form action... OK\r\n\r\n   Question 3 [2 points] - Checking menu (index.html):\r\n      Loading index.html... OK\r\n      Validating document against the W3C validation service... OK\r\n\r\n      Question 3.1 [1 point] - Validating lists:\r\n         Checking amount of lists... OK\r\n         Checking amount of list items... OK\r\n\r\n      Question 3.2 [1 point] - Validating links:\r\n         Checking amount of links... OK\r\n         Checking links destination... OK\r\n\r\n   TOTAL SCORE: 10 / 10", s.Output.ToString());            
        }  

        [Test, Category("FullScriptHtml5"), Category("Real")]
        public void Full_HTML5_SCRIPT_SINGLE_2()
        {             
            var s = new AutoCheck.Core.Script(Path.Combine(GetSamplePath("script"), "targets", "html5_single_2.yaml"));                        
            Assert.AreEqual("Running script 'DAM - M04 (UF1): HTML5 Assignment' in single mode for 'Student Name 2' (v1.0.0.2):\r\n   Question 1 [4 points] - Checking index.html:\r\n      Looking for index.html... OK\r\n      Loading index.html... OK\r\n      Validating document against the W3C validation service... ERROR:\n         -Duplicate attribute alt.ht=\"333\"  alt=\"Cavs\">\r\n\r\n   Question 2 [12 points] - Checking contacte.html:\r\n      Looking for contacte.html... OK\r\n      Loading contacte.html... OK\r\n      Validating document against the W3C validation service... ERROR:\n         -Stray end tag label. teu nom\"></label></td>\r\n\r\n   Question 3 [2 points] - Checking menu (index.html):\r\n      Loading index.html... OK\r\n      Validating document against the W3C validation service... ERROR:\n         -Duplicate attribute alt.ht=\"333\"  alt=\"Cavs\">\r\n\r\n   TOTAL SCORE: 0 / 10", s.Output.ToString());            
        }  

        [Test, Category("FullScriptHtml5"), Category("Real")]
        public void Full_HTML5_SCRIPT_SINGLE_3()
        {             
            var s = new AutoCheck.Core.Script(Path.Combine(GetSamplePath("script"), "targets", "html5_single_3.yaml"));
            Assert.AreEqual("Running script 'DAM - M04 (UF1): HTML5 Assignment' in single mode for 'Student Name 3' (v1.0.0.2):\r\n   Question 1 [4 points] - Checking index.html:\r\n      Looking for index.html... OK\r\n      Loading index.html... OK\r\n      Validating document against the W3C validation service... OK\r\n\r\n      Question 1.1 [1 point] - Validating headers:\r\n         Checking amount of level-1 headers... OK\r\n         Checking amount of level-2 headers... OK\r\n\r\n      Question 1.2 [1 point] - Validating paragraphs:\r\n         Checking amount of paragraphs... OK\r\n         Checking content legth within paragraphs... OK\r\n\r\n      Question 1.3 [1 point] - Validating breaklines:\r\n         Checking amount of breaklines within a paragraph... OK\r\n\r\n      Question 1.4 [1 point] - Validating images:\r\n         Checking amount of images... OK\r\n\r\n   Question 2 [12 points] - Checking contacte.html:\r\n      Looking for contacte.html... OK\r\n      Loading contacte.html... OK\r\n      Validating document against the W3C validation service... OK\r\n\r\n      Question 2.1 [1 point] - Validating text fields:\r\n         Checking amount of text fields... OK\r\n\r\n      Question 2.2 [1 point] - Validating numeric fields:\r\n         Checking amount of numeric fields... OK\r\n\r\n      Question 2.3 [1 point] - Validating email fields:\r\n         Checking amount of email fields... OK\r\n\r\n      Question 2.4 [1 point] - Validating radio fields:\r\n         Checking amount of radio fields... OK\r\n         Checking group for the radio fields... OK\r\n         Checking the checked radio fields... OK\r\n\r\n      Question 2.5 [1 point] - Validating select fields:\r\n         Checking amount of select fields... OK\r\n         Checking select options... OK\r\n         Checking the selected option... OK\r\n\r\n      Question 2.6 [1 point] - Validating checkbox fields:\r\n         Checking amount of checkbox fields... OK\r\n         Checking group for the checkbox fields... OK\r\n         Checking the checked option... OK\r\n\r\n      Question 2.7 [1 point] - Validating textarea fields:\r\n         Checking amount of textarea fields... OK\r\n\r\n      Question 2.8 [1 point] - Validating placeholders:\r\n         Checking amount of placelhoders for text fields... OK\r\n         Checking amount of placelhoders for email fields... OK\r\n         Checking amount of placelhoders for numeric fields... OK\r\n         Checking amount of placelhoders for textarea fields... OK\r\n\r\n      Question 2.9 [1 point] - Validating labels:\r\n         Checking amount of labels for text fields... OK\r\n         Checking amount of labels for numeric fields... OK\r\n         Checking amount of labels for email fields... OK\r\n         Checking amount of labels for radio fields... OK\r\n         Checking amount of labels for select fields... OK\r\n         Checking amount of labels for check fields... OK\r\n         Checking amount of labels for textarea fields... OK\r\n\r\n      Question 2.10 [1 point] - Validating table:\r\n         Checking amount of columns... OK\r\n         Checking amount of merged columns... OK\r\n         Checking amount of labels within the first column... OK\r\n         Checking amount of labels within the second column... OK\r\n         Checking amount of labels within the third column... OK\r\n         Checking amount of labels within the fourth column... OK\r\n         Checking table's consistency... OK\r\n\r\n      Question 2.11 [1 point] - Validating form reset:\r\n         Checking amount of reset buttons... OK\r\n\r\n      Question 2.12 [1 point] - Validating form submit:\r\n         Checking amount of fields with no name... OK\r\n         Checking amount of submit buttons... OK\r\n         Checking form action... OK\r\n\r\n   Question 3 [2 points] - Checking menu (index.html):\r\n      Loading index.html... OK\r\n      Validating document against the W3C validation service... OK\r\n\r\n      Question 3.1 [1 point] - Validating lists:\r\n         Checking amount of lists... OK\r\n         Checking amount of list items... OK\r\n\r\n      Question 3.2 [1 point] - Validating links:\r\n         Checking amount of links... OK\r\n         Checking links destination... OK\r\n\r\n   TOTAL SCORE: 10 / 10", s.Output.ToString());            
        } 

        [Test, Category("FullScriptHtml5"), Category("Real")]
        public void Full_HTML5_SCRIPT_BATCH()
        {             
            var s = new AutoCheck.Core.Script(Path.Combine(GetSamplePath("script"), "targets", "html5_local_batch.yaml"));
            Assert.AreEqual("Executing script DAM - M04 (UF1): HTML5 Assignment (v1.0.0.2):\r\n   Starting the copy detector for HTML:\r\n      Looking for potential copies within Student Name 1... OK\r\n      Looking for potential copies within Student Name 2... OK\r\n      Looking for potential copies within Student Name 3... OK\r\n\r\n   Running on batch mode for Student Name 1:\r\n      Question 1 [4 points] - Checking index.html:\r\n         Looking for index.html... OK\r\n         Loading index.html... OK\r\n         Validating document against the W3C validation service... OK\r\n\r\n         Question 1.1 [1 point] - Validating headers:\r\n            Checking amount of level-1 headers... OK\r\n            Checking amount of level-2 headers... OK\r\n\r\n         Question 1.2 [1 point] - Validating paragraphs:\r\n            Checking amount of paragraphs... OK\r\n            Checking content legth within paragraphs... OK\r\n\r\n         Question 1.3 [1 point] - Validating breaklines:\r\n            Checking amount of breaklines within a paragraph... OK\r\n\r\n         Question 1.4 [1 point] - Validating images:\r\n            Checking amount of images... OK\r\n\r\n      Question 2 [12 points] - Checking contacte.html:\r\n         Looking for contacte.html... OK\r\n         Loading contacte.html... OK\r\n         Validating document against the W3C validation service... OK\r\n\r\n         Question 2.1 [1 point] - Validating text fields:\r\n            Checking amount of text fields... OK\r\n\r\n         Question 2.2 [1 point] - Validating numeric fields:\r\n            Checking amount of numeric fields... OK\r\n\r\n         Question 2.3 [1 point] - Validating email fields:\r\n            Checking amount of email fields... OK\r\n\r\n         Question 2.4 [1 point] - Validating radio fields:\r\n            Checking amount of radio fields... OK\r\n            Checking group for the radio fields... OK\r\n            Checking the checked radio fields... OK\r\n\r\n         Question 2.5 [1 point] - Validating select fields:\r\n            Checking amount of select fields... OK\r\n            Checking select options... OK\r\n            Checking the selected option... OK\r\n\r\n         Question 2.6 [1 point] - Validating checkbox fields:\r\n            Checking amount of checkbox fields... OK\r\n            Checking group for the checkbox fields... OK\r\n            Checking the checked option... OK\r\n\r\n         Question 2.7 [1 point] - Validating textarea fields:\r\n            Checking amount of textarea fields... OK\r\n\r\n         Question 2.8 [1 point] - Validating placeholders:\r\n            Checking amount of placelhoders for text fields... OK\r\n            Checking amount of placelhoders for email fields... OK\r\n            Checking amount of placelhoders for numeric fields... OK\r\n            Checking amount of placelhoders for textarea fields... OK\r\n\r\n         Question 2.9 [1 point] - Validating labels:\r\n            Checking amount of labels for text fields... OK\r\n            Checking amount of labels for numeric fields... OK\r\n            Checking amount of labels for email fields... OK\r\n            Checking amount of labels for radio fields... OK\r\n            Checking amount of labels for select fields... OK\r\n            Checking amount of labels for check fields... OK\r\n            Checking amount of labels for textarea fields... OK\r\n\r\n         Question 2.10 [1 point] - Validating table:\r\n            Checking amount of columns... OK\r\n            Checking amount of merged columns... OK\r\n            Checking amount of labels within the first column... OK\r\n            Checking amount of labels within the second column... OK\r\n            Checking amount of labels within the third column... OK\r\n            Checking amount of labels within the fourth column... OK\r\n            Checking table's consistency... OK\r\n\r\n         Question 2.11 [1 point] - Validating form reset:\r\n            Checking amount of reset buttons... OK\r\n\r\n         Question 2.12 [1 point] - Validating form submit:\r\n            Checking amount of fields with no name... OK\r\n            Checking amount of submit buttons... OK\r\n            Checking form action... OK\r\n\r\n      Question 3 [2 points] - Checking menu (index.html):\r\n         Loading index.html... OK\r\n         Validating document against the W3C validation service... OK\r\n\r\n         Question 3.1 [1 point] - Validating lists:\r\n            Checking amount of lists... OK\r\n            Checking amount of list items... OK\r\n\r\n         Question 3.2 [1 point] - Validating links:\r\n            Checking amount of links... OK\r\n            Checking links destination... OK\r\n\r\n      TOTAL SCORE: 10 / 10\r\n   Running on batch mode for Student Name 2:\r\n      Question 1 [4 points] - Checking index.html:\r\n         Looking for index.html... OK\r\n         Loading index.html... OK\r\n         Validating document against the W3C validation service... ERROR:\n            -Duplicate attribute alt.ht=\"333\"  alt=\"Cavs\">\r\n\r\n      Question 2 [12 points] - Checking contacte.html:\r\n         Looking for contacte.html... OK\r\n         Loading contacte.html... OK\r\n         Validating document against the W3C validation service... ERROR:\n            -Stray end tag label. teu nom\"></label></td>\r\n\r\n      Question 3 [2 points] - Checking menu (index.html):\r\n         Loading index.html... OK\r\n         Validating document against the W3C validation service... ERROR:\n            -Duplicate attribute alt.ht=\"333\"  alt=\"Cavs\">\r\n\r\n      TOTAL SCORE: 0 / 10\r\n   Running on batch mode for Student Name 3:\r\n      Question 1 [4 points] - Checking index.html:\r\n         Looking for index.html... OK\r\n         Loading index.html... OK\r\n         Validating document against the W3C validation service... OK\r\n\r\n         Question 1.1 [1 point] - Validating headers:\r\n            Checking amount of level-1 headers... OK\r\n            Checking amount of level-2 headers... OK\r\n\r\n         Question 1.2 [1 point] - Validating paragraphs:\r\n            Checking amount of paragraphs... OK\r\n            Checking content legth within paragraphs... OK\r\n\r\n         Question 1.3 [1 point] - Validating breaklines:\r\n            Checking amount of breaklines within a paragraph... OK\r\n\r\n         Question 1.4 [1 point] - Validating images:\r\n            Checking amount of images... OK\r\n\r\n      Question 2 [12 points] - Checking contacte.html:\r\n         Looking for contacte.html... OK\r\n         Loading contacte.html... OK\r\n         Validating document against the W3C validation service... OK\r\n\r\n         Question 2.1 [1 point] - Validating text fields:\r\n            Checking amount of text fields... OK\r\n\r\n         Question 2.2 [1 point] - Validating numeric fields:\r\n            Checking amount of numeric fields... OK\r\n\r\n         Question 2.3 [1 point] - Validating email fields:\r\n            Checking amount of email fields... OK\r\n\r\n         Question 2.4 [1 point] - Validating radio fields:\r\n            Checking amount of radio fields... OK\r\n            Checking group for the radio fields... OK\r\n            Checking the checked radio fields... OK\r\n\r\n         Question 2.5 [1 point] - Validating select fields:\r\n            Checking amount of select fields... OK\r\n            Checking select options... OK\r\n            Checking the selected option... OK\r\n\r\n         Question 2.6 [1 point] - Validating checkbox fields:\r\n            Checking amount of checkbox fields... OK\r\n            Checking group for the checkbox fields... OK\r\n            Checking the checked option... OK\r\n\r\n         Question 2.7 [1 point] - Validating textarea fields:\r\n            Checking amount of textarea fields... OK\r\n\r\n         Question 2.8 [1 point] - Validating placeholders:\r\n            Checking amount of placelhoders for text fields... OK\r\n            Checking amount of placelhoders for email fields... OK\r\n            Checking amount of placelhoders for numeric fields... OK\r\n            Checking amount of placelhoders for textarea fields... OK\r\n\r\n         Question 2.9 [1 point] - Validating labels:\r\n            Checking amount of labels for text fields... OK\r\n            Checking amount of labels for numeric fields... OK\r\n            Checking amount of labels for email fields... OK\r\n            Checking amount of labels for radio fields... OK\r\n            Checking amount of labels for select fields... OK\r\n            Checking amount of labels for check fields... OK\r\n            Checking amount of labels for textarea fields... OK\r\n\r\n         Question 2.10 [1 point] - Validating table:\r\n            Checking amount of columns... OK\r\n            Checking amount of merged columns... OK\r\n            Checking amount of labels within the first column... OK\r\n            Checking amount of labels within the second column... OK\r\n            Checking amount of labels within the third column... OK\r\n            Checking amount of labels within the fourth column... OK\r\n            Checking table's consistency... OK\r\n\r\n         Question 2.11 [1 point] - Validating form reset:\r\n            Checking amount of reset buttons... OK\r\n\r\n         Question 2.12 [1 point] - Validating form submit:\r\n            Checking amount of fields with no name... OK\r\n            Checking amount of submit buttons... OK\r\n            Checking form action... OK\r\n\r\n      Question 3 [2 points] - Checking menu (index.html):\r\n         Loading index.html... OK\r\n         Validating document against the W3C validation service... OK\r\n\r\n         Question 3.1 [1 point] - Validating lists:\r\n            Checking amount of lists... OK\r\n            Checking amount of list items... OK\r\n\r\n         Question 3.2 [1 point] - Validating links:\r\n            Checking amount of links... OK\r\n            Checking links destination... OK\r\n\r\n      TOTAL SCORE: 10 / 10", s.Output.ToString());            
        } 
#endregion
#region Real script testing: CSS3
        [Test, Category("FullScriptCss3"), Category("Real")]
        public void Full_CSS3_SCRIPT_SINGLE_1()
        {             
            var s = new AutoCheck.Core.Script(Path.Combine(GetSamplePath("script"), "targets", "css3_single_1.yaml"));                        
            Assert.AreEqual("Running script 'DAM - M04 (UF1): CSS3 Assignment' in single mode for 'Student Name 1' (v1.0.0.2):\r\n   Question 1 [3 points] - Checking index.html:\r\n      Looking for index.html... OK\r\n      Loading index.html... OK\r\n      Validating document against the W3C validation service... OK\r\n\r\n      Question 1.1 [1 point] - Validating inline CSS:\r\n         Checking for inline CSS entries... OK\r\n\r\n      Question 1.2 [1 point] - Validating DIVs:\r\n         Checking the amount of divs... OK\r\n\r\n      Question 1.3 [1 point] - Validating video:\r\n         Checking amount of video entries... ERROR:\n            -Expected -> >=1; Found -> 0\r\n\r\n   Question 2 [14 points] - Checking index.css:\r\n      Looking for index.css... OK\r\n      Loading index.css... OK\r\n      Loading index.html... OK\r\n      Validating document against the W3C validation service... OK\r\n\r\n      Question 2.1 [1 point] - Validating font property:\r\n         Checking if the font property has been created... OK\r\n         Checking if the font property has been applied... OK\r\n\r\n      Question 2.2 [1 point] - Validating border property:\r\n         Checking if the border property has been created... OK\r\n         Checking if the border property has been applied... OK\r\n\r\n      Question 2.3 [1 point] - Validating text property:\r\n         Checking if the text property has been created... OK\r\n         Checking if the text property has been applied... OK\r\n\r\n      Question 2.4 [1 point] - Validating color property:\r\n         Checking if the color property has been created... OK\r\n         Checking if the color property has been applied... OK\r\n\r\n      Question 2.5 [1 point] - Validating background property:\r\n         Checking if the background property has been created... OK\r\n         Checking if the background property has been applied... OK\r\n\r\n      Question 2.6 [1 point] - Validating position:absolute property:\r\n         Checking if the position:absolute property has been created... OK\r\n         Checking if the position:absolute property has been applied... OK\r\n\r\n      Question 2.7 [1 point] - Validating position:relative property:\r\n         Checking if the position:relative property has been created... OK\r\n         Checking if the position:relative property has been applied... OK\r\n\r\n      Question 2.8 [1 point] - Validating clear property:\r\n         Checking if the clear property has been created... OK\r\n         Checking if the clear property has been applied... OK\r\n\r\n      Question 2.9 [1 point] - Validating clear property:\r\n         Checking if the width property has been created... OK\r\n         Checking if the width property has been applied... OK\r\n\r\n      Question 2.10 [1 point] - Validating height property:\r\n         Checking if the height property has been created... OK\r\n         Checking if the height property has been applied... OK\r\n\r\n      Question 2.11 [1 point] - Validating margin property:\r\n         Checking if the margin property has been created... OK\r\n         Checking if the margin property has been applied... OK\r\n\r\n      Question 2.12 [1 point] - Validating padding property:\r\n         Checking if the padding property has been created... OK\r\n         Checking if the padding property has been applied... OK\r\n\r\n      Question 2.13 [1 point] - Validating list property:\r\n         Checking if the list property has been created... OK\r\n         Checking if the list property has been applied... OK\r\n\r\n      Question 2.14 [1 point] - Validating (top | right | bottom | left) property:\r\n         Checking if the (top | right | bottom | left) property has been created... OK\r\n         Checking if the (top | right | bottom | left) property has been applied... OK\r\n\r\n   TOTAL SCORE: 9.41 / 10", s.Output.ToString());
        }

        [Test, Category("FullScriptCss3"), Category("Real")]
        public void Full_CSS3_SCRIPT_SINGLE_2()
        {             
            var s = new AutoCheck.Core.Script(Path.Combine(GetSamplePath("script"), "targets", "css3_single_2.yaml"));
            Assert.AreEqual("Running script 'DAM - M04 (UF1): CSS3 Assignment' in single mode for 'Student Name 2' (v1.0.0.2):\r\n   Question 1 [3 points] - Checking index.html:\r\n      Looking for index.html... OK\r\n      Loading index.html... OK\r\n      Validating document against the W3C validation service... OK\r\n\r\n      Question 1.1 [1 point] - Validating inline CSS:\r\n         Checking for inline CSS entries... OK\r\n\r\n      Question 1.2 [1 point] - Validating DIVs:\r\n         Checking the amount of divs... OK\r\n\r\n      Question 1.3 [1 point] - Validating video:\r\n         Checking amount of video entries... OK\r\n\r\n   Question 2 [14 points] - Checking index.css:\r\n      Looking for index.css... OK\r\n      Loading index.css... OK\r\n      Loading index.html... OK\r\n      Validating document against the W3C validation service... OK\r\n\r\n      Question 2.1 [1 point] - Validating font property:\r\n         Checking if the font property has been created... OK\r\n         Checking if the font property has been applied... OK\r\n\r\n      Question 2.2 [1 point] - Validating border property:\r\n         Checking if the border property has been created... OK\r\n         Checking if the border property has been applied... OK\r\n\r\n      Question 2.3 [1 point] - Validating text property:\r\n         Checking if the text property has been created... OK\r\n         Checking if the text property has been applied... OK\r\n\r\n      Question 2.4 [1 point] - Validating color property:\r\n         Checking if the color property has been created... OK\r\n         Checking if the color property has been applied... OK\r\n\r\n      Question 2.5 [1 point] - Validating background property:\r\n         Checking if the background property has been created... OK\r\n         Checking if the background property has been applied... OK\r\n\r\n      Question 2.6 [1 point] - Validating position:absolute property:\r\n         Checking if the position:absolute property has been created... OK\r\n         Checking if the position:absolute property has been applied... OK\r\n\r\n      Question 2.7 [1 point] - Validating position:relative property:\r\n         Checking if the position:relative property has been created... OK\r\n         Checking if the position:relative property has been applied... OK\r\n\r\n      Question 2.8 [1 point] - Validating clear property:\r\n         Checking if the clear property has been created... OK\r\n         Checking if the clear property has been applied... OK\r\n\r\n      Question 2.9 [1 point] - Validating clear property:\r\n         Checking if the width property has been created... OK\r\n         Checking if the width property has been applied... OK\r\n\r\n      Question 2.10 [1 point] - Validating height property:\r\n         Checking if the height property has been created... OK\r\n         Checking if the height property has been applied... OK\r\n\r\n      Question 2.11 [1 point] - Validating margin property:\r\n         Checking if the margin property has been created... OK\r\n         Checking if the margin property has been applied... OK\r\n\r\n      Question 2.12 [1 point] - Validating padding property:\r\n         Checking if the padding property has been created... OK\r\n         Checking if the padding property has been applied... OK\r\n\r\n      Question 2.13 [1 point] - Validating list property:\r\n         Checking if the list property has been created... OK\r\n         Checking if the list property has been applied... OK\r\n\r\n      Question 2.14 [1 point] - Validating (top | right | bottom | left) property:\r\n         Checking if the (top | right | bottom | left) property has been created... OK\r\n         Checking if the (top | right | bottom | left) property has been applied... OK\r\n\r\n   TOTAL SCORE: 10 / 10", s.Output.ToString());
        }

        [Test, Category("FullScriptCss3"), Category("Real")]
        public void Full_CSS3_SCRIPT_SINGLE_3()
        {             
            var s = new AutoCheck.Core.Script(Path.Combine(GetSamplePath("script"), "targets", "css3_single_3.yaml"));
            Assert.AreEqual("Running script 'DAM - M04 (UF1): CSS3 Assignment' in single mode for 'Student Name 3' (v1.0.0.2):\r\n   Question 1 [3 points] - Checking index.html:\r\n      Looking for index.html... OK\r\n      Loading index.html... OK\r\n      Validating document against the W3C validation service... OK\r\n\r\n      Question 1.1 [1 point] - Validating inline CSS:\r\n         Checking for inline CSS entries... OK\r\n\r\n      Question 1.2 [1 point] - Validating DIVs:\r\n         Checking the amount of divs... OK\r\n\r\n      Question 1.3 [1 point] - Validating video:\r\n         Checking amount of video entries... OK\r\n\r\n   Question 2 [14 points] - Checking index.css:\r\n      Looking for index.css... OK\r\n      Loading index.css... OK\r\n      Loading index.html... OK\r\n      Validating document against the W3C validation service... OK\r\n\r\n      Question 2.1 [1 point] - Validating font property:\r\n         Checking if the font property has been created... OK\r\n         Checking if the font property has been applied... OK\r\n\r\n      Question 2.2 [1 point] - Validating border property:\r\n         Checking if the border property has been created... OK\r\n         Checking if the border property has been applied... OK\r\n\r\n      Question 2.3 [1 point] - Validating text property:\r\n         Checking if the text property has been created... OK\r\n         Checking if the text property has been applied... OK\r\n\r\n      Question 2.4 [1 point] - Validating color property:\r\n         Checking if the color property has been created... OK\r\n         Checking if the color property has been applied... OK\r\n\r\n      Question 2.5 [1 point] - Validating background property:\r\n         Checking if the background property has been created... OK\r\n         Checking if the background property has been applied... OK\r\n\r\n      Question 2.6 [1 point] - Validating position:absolute property:\r\n         Checking if the position:absolute property has been created... OK\r\n         Checking if the position:absolute property has been applied... OK\r\n\r\n      Question 2.7 [1 point] - Validating position:relative property:\r\n         Checking if the position:relative property has been created... OK\r\n         Checking if the position:relative property has been applied... OK\r\n\r\n      Question 2.8 [1 point] - Validating clear property:\r\n         Checking if the clear property has been created... OK\r\n         Checking if the clear property has been applied... OK\r\n\r\n      Question 2.9 [1 point] - Validating clear property:\r\n         Checking if the width property has been created... OK\r\n         Checking if the width property has been applied... OK\r\n\r\n      Question 2.10 [1 point] - Validating height property:\r\n         Checking if the height property has been created... OK\r\n         Checking if the height property has been applied... OK\r\n\r\n      Question 2.11 [1 point] - Validating margin property:\r\n         Checking if the margin property has been created... OK\r\n         Checking if the margin property has been applied... OK\r\n\r\n      Question 2.12 [1 point] - Validating padding property:\r\n         Checking if the padding property has been created... OK\r\n         Checking if the padding property has been applied... OK\r\n\r\n      Question 2.13 [1 point] - Validating list property:\r\n         Checking if the list property has been created... OK\r\n         Checking if the list property has been applied... OK\r\n\r\n      Question 2.14 [1 point] - Validating (top | right | bottom | left) property:\r\n         Checking if the (top | right | bottom | left) property has been created... OK\r\n         Checking if the (top | right | bottom | left) property has been applied... OK\r\n\r\n   TOTAL SCORE: 10 / 10", s.Output.ToString());
        }

        [Test, Category("FullScriptCss3"), Category("Real")]
        public void Full_CSS3_SCRIPT_BATCH()
        {             
            var s = new AutoCheck.Core.Script(Path.Combine(GetSamplePath("script"), "targets", "css3_local_batch.yaml"));
            var expected = "Executing script DAM - M04 (UF1): CSS3 Assignment (v1.0.0.2):\r\n   Running on batch mode for Student Name 1:\r\n      Question 1 [3 points] - Checking index.html:\r\n         Looking for index.html... OK\r\n         Loading index.html... OK\r\n         Validating document against the W3C validation service... OK\r\n\r\n         Question 1.1 [1 point] - Validating inline CSS:\r\n            Checking for inline CSS entries... OK\r\n\r\n         Question 1.2 [1 point] - Validating DIVs:\r\n            Checking the amount of divs... OK\r\n\r\n         Question 1.3 [1 point] - Validating video:\r\n            Checking amount of video entries... ERROR:\n               -Expected -> >=1; Found -> 0\r\n\r\n      Question 2 [14 points] - Checking index.css:\r\n         Looking for index.css... OK\r\n         Loading index.css... OK\r\n         Loading index.html... OK\r\n         Validating document against the W3C validation service... OK\r\n\r\n         Question 2.1 [1 point] - Validating font property:\r\n            Checking if the font property has been created... OK\r\n            Checking if the font property has been applied... OK\r\n\r\n         Question 2.2 [1 point] - Validating border property:\r\n            Checking if the border property has been created... OK\r\n            Checking if the border property has been applied... OK\r\n\r\n         Question 2.3 [1 point] - Validating text property:\r\n            Checking if the text property has been created... OK\r\n            Checking if the text property has been applied... OK\r\n\r\n         Question 2.4 [1 point] - Validating color property:\r\n            Checking if the color property has been created... OK\r\n            Checking if the color property has been applied... OK\r\n\r\n         Question 2.5 [1 point] - Validating background property:\r\n            Checking if the background property has been created... OK\r\n            Checking if the background property has been applied... OK\r\n\r\n         Question 2.6 [1 point] - Validating position:absolute property:\r\n            Checking if the position:absolute property has been created... OK\r\n            Checking if the position:absolute property has been applied... OK\r\n\r\n         Question 2.7 [1 point] - Validating position:relative property:\r\n            Checking if the position:relative property has been created... OK\r\n            Checking if the position:relative property has been applied... OK\r\n\r\n         Question 2.8 [1 point] - Validating clear property:\r\n            Checking if the clear property has been created... OK\r\n            Checking if the clear property has been applied... OK\r\n\r\n         Question 2.9 [1 point] - Validating clear property:\r\n            Checking if the width property has been created... OK\r\n            Checking if the width property has been applied... OK\r\n\r\n         Question 2.10 [1 point] - Validating height property:\r\n            Checking if the height property has been created... OK\r\n            Checking if the height property has been applied... OK\r\n\r\n         Question 2.11 [1 point] - Validating margin property:\r\n            Checking if the margin property has been created... OK\r\n            Checking if the margin property has been applied... OK\r\n\r\n         Question 2.12 [1 point] - Validating padding property:\r\n            Checking if the padding property has been created... OK\r\n            Checking if the padding property has been applied... OK\r\n\r\n         Question 2.13 [1 point] - Validating list property:\r\n            Checking if the list property has been created... OK\r\n            Checking if the list property has been applied... OK\r\n\r\n         Question 2.14 [1 point] - Validating (top | right | bottom | left) property:\r\n            Checking if the (top | right | bottom | left) property has been created... OK\r\n            Checking if the (top | right | bottom | left) property has been applied... OK\r\n\r\n      TOTAL SCORE: 9.41 / 10\r\n   Running on batch mode for Student Name 2:\r\n      Question 1 [3 points] - Checking index.html:\r\n         Looking for index.html... OK\r\n         Loading index.html... OK\r\n         Validating document against the W3C validation service... OK\r\n\r\n         Question 1.1 [1 point] - Validating inline CSS:\r\n            Checking for inline CSS entries... OK\r\n\r\n         Question 1.2 [1 point] - Validating DIVs:\r\n            Checking the amount of divs... OK\r\n\r\n         Question 1.3 [1 point] - Validating video:\r\n            Checking amount of video entries... OK\r\n\r\n      Question 2 [14 points] - Checking index.css:\r\n         Looking for index.css... OK\r\n         Loading index.css... OK\r\n         Loading index.html... OK\r\n         Validating document against the W3C validation service... OK\r\n\r\n         Question 2.1 [1 point] - Validating font property:\r\n            Checking if the font property has been created... OK\r\n            Checking if the font property has been applied... OK\r\n\r\n         Question 2.2 [1 point] - Validating border property:\r\n            Checking if the border property has been created... OK\r\n            Checking if the border property has been applied... OK\r\n\r\n         Question 2.3 [1 point] - Validating text property:\r\n            Checking if the text property has been created... OK\r\n            Checking if the text property has been applied... OK\r\n\r\n         Question 2.4 [1 point] - Validating color property:\r\n            Checking if the color property has been created... OK\r\n            Checking if the color property has been applied... OK\r\n\r\n         Question 2.5 [1 point] - Validating background property:\r\n            Checking if the background property has been created... OK\r\n            Checking if the background property has been applied... OK\r\n\r\n         Question 2.6 [1 point] - Validating position:absolute property:\r\n            Checking if the position:absolute property has been created... OK\r\n            Checking if the position:absolute property has been applied... OK\r\n\r\n         Question 2.7 [1 point] - Validating position:relative property:\r\n            Checking if the position:relative property has been created... OK\r\n            Checking if the position:relative property has been applied... OK\r\n\r\n         Question 2.8 [1 point] - Validating clear property:\r\n            Checking if the clear property has been created... OK\r\n            Checking if the clear property has been applied... OK\r\n\r\n         Question 2.9 [1 point] - Validating clear property:\r\n            Checking if the width property has been created... OK\r\n            Checking if the width property has been applied... OK\r\n\r\n         Question 2.10 [1 point] - Validating height property:\r\n            Checking if the height property has been created... OK\r\n            Checking if the height property has been applied... OK\r\n\r\n         Question 2.11 [1 point] - Validating margin property:\r\n            Checking if the margin property has been created... OK\r\n            Checking if the margin property has been applied... OK\r\n\r\n         Question 2.12 [1 point] - Validating padding property:\r\n            Checking if the padding property has been created... OK\r\n            Checking if the padding property has been applied... OK\r\n\r\n         Question 2.13 [1 point] - Validating list property:\r\n            Checking if the list property has been created... OK\r\n            Checking if the list property has been applied... OK\r\n\r\n         Question 2.14 [1 point] - Validating (top | right | bottom | left) property:\r\n            Checking if the (top | right | bottom | left) property has been created... OK\r\n            Checking if the (top | right | bottom | left) property has been applied... OK\r\n\r\n      TOTAL SCORE: 10 / 10\r\n   Running on batch mode for Student Name 3:\r\n      Question 1 [3 points] - Checking index.html:\r\n         Looking for index.html... OK\r\n         Loading index.html... OK\r\n         Validating document against the W3C validation service... OK\r\n\r\n         Question 1.1 [1 point] - Validating inline CSS:\r\n            Checking for inline CSS entries... OK\r\n\r\n         Question 1.2 [1 point] - Validating DIVs:\r\n            Checking the amount of divs... OK\r\n\r\n         Question 1.3 [1 point] - Validating video:\r\n            Checking amount of video entries... OK\r\n\r\n      Question 2 [14 points] - Checking index.css:\r\n         Looking for index.css... OK\r\n         Loading index.css... OK\r\n         Loading index.html... OK\r\n         Validating document against the W3C validation service... OK\r\n\r\n         Question 2.1 [1 point] - Validating font property:\r\n            Checking if the font property has been created... OK\r\n            Checking if the font property has been applied... OK\r\n\r\n         Question 2.2 [1 point] - Validating border property:\r\n            Checking if the border property has been created... OK\r\n            Checking if the border property has been applied... OK\r\n\r\n         Question 2.3 [1 point] - Validating text property:\r\n            Checking if the text property has been created... OK\r\n            Checking if the text property has been applied... OK\r\n\r\n         Question 2.4 [1 point] - Validating color property:\r\n            Checking if the color property has been created... OK\r\n            Checking if the color property has been applied... OK\r\n\r\n         Question 2.5 [1 point] - Validating background property:\r\n            Checking if the background property has been created... OK\r\n            Checking if the background property has been applied... OK\r\n\r\n         Question 2.6 [1 point] - Validating position:absolute property:\r\n            Checking if the position:absolute property has been created... OK\r\n            Checking if the position:absolute property has been applied... OK\r\n\r\n         Question 2.7 [1 point] - Validating position:relative property:\r\n            Checking if the position:relative property has been created... OK\r\n            Checking if the position:relative property has been applied... OK\r\n\r\n         Question 2.8 [1 point] - Validating clear property:\r\n            Checking if the clear property has been created... OK\r\n            Checking if the clear property has been applied... OK\r\n\r\n         Question 2.9 [1 point] - Validating clear property:\r\n            Checking if the width property has been created... OK\r\n            Checking if the width property has been applied... OK\r\n\r\n         Question 2.10 [1 point] - Validating height property:\r\n            Checking if the height property has been created... OK\r\n            Checking if the height property has been applied... OK\r\n\r\n         Question 2.11 [1 point] - Validating margin property:\r\n            Checking if the margin property has been created... OK\r\n            Checking if the margin property has been applied... OK\r\n\r\n         Question 2.12 [1 point] - Validating padding property:\r\n            Checking if the padding property has been created... OK\r\n            Checking if the padding property has been applied... OK\r\n\r\n         Question 2.13 [1 point] - Validating list property:\r\n            Checking if the list property has been created... OK\r\n            Checking if the list property has been applied... OK\r\n\r\n         Question 2.14 [1 point] - Validating (top | right | bottom | left) property:\r\n            Checking if the (top | right | bottom | left) property has been created... OK\r\n            Checking if the (top | right | bottom | left) property has been applied... OK\r\n\r\n      TOTAL SCORE: 10 / 10";
            Assert.AreEqual(expected, s.Output.ToString());
        }
#endregion
#region Real script testing: WEB Syndication
        [Test, Category("FullScriptWebSyndication"), Category("Real")]
        public void Full_WEB_SYNDICATION_SCRIPT_SINGLE_1()
        {             
            var s = new AutoCheck.Core.Script(Path.Combine(GetSamplePath("script"), "targets", "web_syndication_single_1.yaml"));                        
            Assert.AreEqual("Running script 'DAM - M04 (UF2): Web Syndication (RSS + Atom)' in single mode for 'Student Name 1' (v1.0.0.2):\r\n   Question 1 [1.8 points] - Checking document.rss:\r\n      Looking for document.rss... OK\r\n      Loading document.rss... OK\r\n      Validating document against the W3C validation service... OK\r\n\r\n      Question 1.1 [0.15 points] - Validating rss tag:\r\n         Checking amount of rss tags... OK\r\n\r\n      Question 1.2 [0.15 points] - Validating channel tag:\r\n         Checking amount of channel tags... OK\r\n\r\n      Question 1.3 [0.15 points] - Validating item tag:\r\n         Checking amount of items... OK\r\n\r\n      Question 1.4 [0.15 points] - Validating title (within channel) tag:\r\n         Checking amount of titles... OK\r\n\r\n      Question 1.5 [0.15 points] - Validating title (within item) tag:\r\n         Checking amount of titles... OK\r\n\r\n      Question 1.6 [0.15 points] - Validating description (within channel) tag:\r\n         Checking amount of descriptions... OK\r\n\r\n      Question 1.7 [0.15 points] - Validating description (within item) tag:\r\n         Checking amount of descriptions... OK\r\n\r\n      Question 1.8 [0.15 points] - Validating link (within channel) tag:\r\n         Checking amount of links... OK\r\n\r\n      Question 1.9 [0.15 points] - Validating link (within item) tag:\r\n         Checking amount of links... OK\r\n\r\n      Question 1.10 [0.15 points] - Validating pubdate (within channel) tag:\r\n         Checking amount of pubdates... OK\r\n\r\n      Question 1.11 [0.15 points] - Validating pubdate (within item) tag:\r\n         Checking amount of pubdates... OK\r\n\r\n      Question 1.12 [0.15 points] - Validating guid (within item) tag:\r\n         Checking amount of guids... OK\r\n\r\n   Question 2 [1.65 points] - Checking document.atom:\r\n      Looking for document.atom... OK\r\n      Loading document.atom... OK\r\n      Validating document against the W3C validation service... OK\r\n\r\n      Question 2.1 [0.15 points] - Validating feed tag:\r\n         Checking amount of feed tags... OK\r\n\r\n      Question 2.2 [0.15 points] - Validating entry tag:\r\n         Checking amount of entries... OK\r\n\r\n      Question 2.3 [0.15 points] - Validating title (within feed) tag:\r\n         Checking amount of titles... OK\r\n\r\n      Question 2.4 [0.15 points] - Validating title (within entry) tag:\r\n         Checking amount of titles... OK\r\n\r\n      Question 2.5 [0.15 points] - Validating subtitle (within feed) tag:\r\n         Checking amount of subtitles... OK\r\n\r\n      Question 2.6 [0.15 points] - Validating summary (within entry) tag:\r\n         Checking amount of summaries... OK\r\n\r\n      Question 2.7 [0.15 points] - Validating link (within feed) tag:\r\n         Checking amount of links... OK\r\n\r\n      Question 2.8 [0.15 points] - Validating link (within entry) tag:\r\n         Checking amount of links... OK\r\n\r\n      Question 2.9 [0.15 points] - Validating updated (within feed) tag:\r\n         Checking amount of updateds... OK\r\n\r\n      Question 2.10 [0.15 points] - Validating updated (within entry) tag:\r\n         Checking amount of updateds... OK\r\n\r\n      Question 2.11 [0.15 points] - Validating id (within entry) tag:\r\n         Checking amount of ids... OK\r\n\r\n   TOTAL SCORE: 3.45 / 3.45", s.Output.ToString());
        }

        [Test, Category("FullScriptWebSyndication"), Category("Real")]
        public void Full_WEB_SYNDICATION_SCRIPT_SINGLE_2()
        {             
            var s = new AutoCheck.Core.Script(Path.Combine(GetSamplePath("script"), "targets", "web_syndication_single_2.yaml"));                        
            Assert.AreEqual("Running script 'DAM - M04 (UF2): Web Syndication (RSS + Atom)' in single mode for 'Student Name 2' (v1.0.0.2):\r\n   Question 1 [1.8 points] - Checking document.rss:\r\n      Looking for document.rss... OK\r\n      Loading document.rss... OK\r\n      Validating document against the W3C validation service... OK\r\n\r\n      Question 1.1 [0.15 points] - Validating rss tag:\r\n         Checking amount of rss tags... OK\r\n\r\n      Question 1.2 [0.15 points] - Validating channel tag:\r\n         Checking amount of channel tags... OK\r\n\r\n      Question 1.3 [0.15 points] - Validating item tag:\r\n         Checking amount of items... ERROR:\n            -Expected -> >=3; Found -> 2\r\n\r\n      Question 1.4 [0.15 points] - Validating title (within channel) tag:\r\n         Checking amount of titles... OK\r\n\r\n      Question 1.5 [0.15 points] - Validating title (within item) tag:\r\n         Checking amount of titles... OK\r\n\r\n      Question 1.6 [0.15 points] - Validating description (within channel) tag:\r\n         Checking amount of descriptions... OK\r\n\r\n      Question 1.7 [0.15 points] - Validating description (within item) tag:\r\n         Checking amount of descriptions... OK\r\n\r\n      Question 1.8 [0.15 points] - Validating link (within channel) tag:\r\n         Checking amount of links... OK\r\n\r\n      Question 1.9 [0.15 points] - Validating link (within item) tag:\r\n         Checking amount of links... OK\r\n\r\n      Question 1.10 [0.15 points] - Validating pubdate (within channel) tag:\r\n         Checking amount of pubdates... OK\r\n\r\n      Question 1.11 [0.15 points] - Validating pubdate (within item) tag:\r\n         Checking amount of pubdates... OK\r\n\r\n      Question 1.12 [0.15 points] - Validating guid (within item) tag:\r\n         Checking amount of guids... OK\r\n\r\n   Question 2 [1.65 points] - Checking document.atom:\r\n      Looking for document.atom... OK\r\n      Loading document.atom... OK\r\n      Validating document against the W3C validation service... OK\r\n\r\n      Question 2.1 [0.15 points] - Validating feed tag:\r\n         Checking amount of feed tags... OK\r\n\r\n      Question 2.2 [0.15 points] - Validating entry tag:\r\n         Checking amount of entries... OK\r\n\r\n      Question 2.3 [0.15 points] - Validating title (within feed) tag:\r\n         Checking amount of titles... OK\r\n\r\n      Question 2.4 [0.15 points] - Validating title (within entry) tag:\r\n         Checking amount of titles... OK\r\n\r\n      Question 2.5 [0.15 points] - Validating subtitle (within feed) tag:\r\n         Checking amount of subtitles... OK\r\n\r\n      Question 2.6 [0.15 points] - Validating summary (within entry) tag:\r\n         Checking amount of summaries... OK\r\n\r\n      Question 2.7 [0.15 points] - Validating link (within feed) tag:\r\n         Checking amount of links... OK\r\n\r\n      Question 2.8 [0.15 points] - Validating link (within entry) tag:\r\n         Checking amount of links... OK\r\n\r\n      Question 2.9 [0.15 points] - Validating updated (within feed) tag:\r\n         Checking amount of updateds... OK\r\n\r\n      Question 2.10 [0.15 points] - Validating updated (within entry) tag:\r\n         Checking amount of updateds... OK\r\n\r\n      Question 2.11 [0.15 points] - Validating id (within entry) tag:\r\n         Checking amount of ids... OK\r\n\r\n   TOTAL SCORE: 3.3 / 3.45", s.Output.ToString());
        }

        [Test, Category("FullScriptWebSyndication"), Category("Real")]
        public void Full_WEB_SYNDICATION_SCRIPT_SINGLE_3()
        {             
            var s = new AutoCheck.Core.Script(Path.Combine(GetSamplePath("script"), "targets", "web_syndication_single_3.yaml"));                        
            Assert.AreEqual("Running script 'DAM - M04 (UF2): Web Syndication (RSS + Atom)' in single mode for 'Student Name 3' (v1.0.0.2):\r\n   Question 1 [1.8 points] - Checking document.rss:\r\n      Looking for document.rss... OK\r\n      Loading document.rss... OK\r\n      Validating document against the W3C validation service... OK\r\n\r\n      Question 1.1 [0.15 points] - Validating rss tag:\r\n         Checking amount of rss tags... OK\r\n\r\n      Question 1.2 [0.15 points] - Validating channel tag:\r\n         Checking amount of channel tags... OK\r\n\r\n      Question 1.3 [0.15 points] - Validating item tag:\r\n         Checking amount of items... OK\r\n\r\n      Question 1.4 [0.15 points] - Validating title (within channel) tag:\r\n         Checking amount of titles... OK\r\n\r\n      Question 1.5 [0.15 points] - Validating title (within item) tag:\r\n         Checking amount of titles... OK\r\n\r\n      Question 1.6 [0.15 points] - Validating description (within channel) tag:\r\n         Checking amount of descriptions... OK\r\n\r\n      Question 1.7 [0.15 points] - Validating description (within item) tag:\r\n         Checking amount of descriptions... OK\r\n\r\n      Question 1.8 [0.15 points] - Validating link (within channel) tag:\r\n         Checking amount of links... OK\r\n\r\n      Question 1.9 [0.15 points] - Validating link (within item) tag:\r\n         Checking amount of links... OK\r\n\r\n      Question 1.10 [0.15 points] - Validating pubdate (within channel) tag:\r\n         Checking amount of pubdates... OK\r\n\r\n      Question 1.11 [0.15 points] - Validating pubdate (within item) tag:\r\n         Checking amount of pubdates... OK\r\n\r\n      Question 1.12 [0.15 points] - Validating guid (within item) tag:\r\n         Checking amount of guids... OK\r\n\r\n   Question 2 [1.65 points] - Checking document.atom:\r\n      Looking for document.atom... OK\r\n      Loading document.atom... OK\r\n      Validating document against the W3C validation service... OK\r\n\r\n      Question 2.1 [0.15 points] - Validating feed tag:\r\n         Checking amount of feed tags... OK\r\n\r\n      Question 2.2 [0.15 points] - Validating entry tag:\r\n         Checking amount of entries... OK\r\n\r\n      Question 2.3 [0.15 points] - Validating title (within feed) tag:\r\n         Checking amount of titles... OK\r\n\r\n      Question 2.4 [0.15 points] - Validating title (within entry) tag:\r\n         Checking amount of titles... OK\r\n\r\n      Question 2.5 [0.15 points] - Validating subtitle (within feed) tag:\r\n         Checking amount of subtitles... ERROR:\n            -Expected -> =1; Found -> 0\r\n\r\n      Question 2.6 [0.15 points] - Validating summary (within entry) tag:\r\n         Checking amount of summaries... OK\r\n\r\n      Question 2.7 [0.15 points] - Validating link (within feed) tag:\r\n         Checking amount of links... OK\r\n\r\n      Question 2.8 [0.15 points] - Validating link (within entry) tag:\r\n         Checking amount of links... OK\r\n\r\n      Question 2.9 [0.15 points] - Validating updated (within feed) tag:\r\n         Checking amount of updateds... OK\r\n\r\n      Question 2.10 [0.15 points] - Validating updated (within entry) tag:\r\n         Checking amount of updateds... OK\r\n\r\n      Question 2.11 [0.15 points] - Validating id (within entry) tag:\r\n         Checking amount of ids... OK\r\n\r\n   TOTAL SCORE: 3.3 / 3.45", s.Output.ToString());
        }

        [Test, Category("FullScriptWebSyndication"), Category("Real")]
        public void Full_WEB_SYNDICATION_SCRIPT_SINGLE_4()
        {             
            var s = new AutoCheck.Core.Script(Path.Combine(GetSamplePath("script"), "targets", "web_syndication_single_4.yaml"));                        
            Assert.AreEqual("Running script 'DAM - M04 (UF2): Web Syndication (RSS + Atom)' in single mode for 'Student Name 4' (v1.0.0.2):\r\n   Question 1 [1.8 points] - Checking document.rss:\r\n      Looking for document.rss... OK\r\n      Loading document.rss... OK\r\n      Validating document against the W3C validation service... ERROR:\n         -Exception of type 'AutoCheck.Core.Exceptions.DocumentInvalidException' was thrown.\r\n\r\n   Question 2 [1.65 points] - Checking document.atom:\r\n      Looking for document.atom... OK\r\n      Loading document.atom... OK\r\n      Validating document against the W3C validation service... OK\r\n\r\n      Question 2.1 [0.15 points] - Validating feed tag:\r\n         Checking amount of feed tags... OK\r\n\r\n      Question 2.2 [0.15 points] - Validating entry tag:\r\n         Checking amount of entries... ERROR:\n            -Expected -> >=3; Found -> 2\r\n\r\n      Question 2.3 [0.15 points] - Validating title (within feed) tag:\r\n         Checking amount of titles... OK\r\n\r\n      Question 2.4 [0.15 points] - Validating title (within entry) tag:\r\n         Checking amount of titles... OK\r\n\r\n      Question 2.5 [0.15 points] - Validating subtitle (within feed) tag:\r\n         Checking amount of subtitles... OK\r\n\r\n      Question 2.6 [0.15 points] - Validating summary (within entry) tag:\r\n         Checking amount of summaries... OK\r\n\r\n      Question 2.7 [0.15 points] - Validating link (within feed) tag:\r\n         Checking amount of links... OK\r\n\r\n      Question 2.8 [0.15 points] - Validating link (within entry) tag:\r\n         Checking amount of links... OK\r\n\r\n      Question 2.9 [0.15 points] - Validating updated (within feed) tag:\r\n         Checking amount of updateds... OK\r\n\r\n      Question 2.10 [0.15 points] - Validating updated (within entry) tag:\r\n         Checking amount of updateds... OK\r\n\r\n      Question 2.11 [0.15 points] - Validating id (within entry) tag:\r\n         Checking amount of ids... OK\r\n\r\n   TOTAL SCORE: 1.5 / 3.45", s.Output.ToString());
        }

        [Test, Category("FullScriptWebSyndication"), Category("Real")]
        public void Full_WEB_SYNDICATION_SCRIPT_BATCH()
        {             
            var s = new AutoCheck.Core.Script(Path.Combine(GetSamplePath("script"), "targets", "web_syndication_local_batch.yaml"));                        
            var expected = "Executing script DAM - M04 (UF2): Web Syndication (RSS + Atom) (v1.0.0.2):\r\n   Running on batch mode for Student Name 1:\r\n      Question 1 [1.8 points] - Checking document.rss:\r\n         Looking for document.rss... OK\r\n         Loading document.rss... OK\r\n         Validating document against the W3C validation service... OK\r\n\r\n         Question 1.1 [0.15 points] - Validating rss tag:\r\n            Checking amount of rss tags... OK\r\n\r\n         Question 1.2 [0.15 points] - Validating channel tag:\r\n            Checking amount of channel tags... OK\r\n\r\n         Question 1.3 [0.15 points] - Validating item tag:\r\n            Checking amount of items... OK\r\n\r\n         Question 1.4 [0.15 points] - Validating title (within channel) tag:\r\n            Checking amount of titles... OK\r\n\r\n         Question 1.5 [0.15 points] - Validating title (within item) tag:\r\n            Checking amount of titles... OK\r\n\r\n         Question 1.6 [0.15 points] - Validating description (within channel) tag:\r\n            Checking amount of descriptions... OK\r\n\r\n         Question 1.7 [0.15 points] - Validating description (within item) tag:\r\n            Checking amount of descriptions... OK\r\n\r\n         Question 1.8 [0.15 points] - Validating link (within channel) tag:\r\n            Checking amount of links... OK\r\n\r\n         Question 1.9 [0.15 points] - Validating link (within item) tag:\r\n            Checking amount of links... OK\r\n\r\n         Question 1.10 [0.15 points] - Validating pubdate (within channel) tag:\r\n            Checking amount of pubdates... OK\r\n\r\n         Question 1.11 [0.15 points] - Validating pubdate (within item) tag:\r\n            Checking amount of pubdates... OK\r\n\r\n         Question 1.12 [0.15 points] - Validating guid (within item) tag:\r\n            Checking amount of guids... OK\r\n\r\n      Question 2 [1.65 points] - Checking document.atom:\r\n         Looking for document.atom... OK\r\n         Loading document.atom... OK\r\n         Validating document against the W3C validation service... OK\r\n\r\n         Question 2.1 [0.15 points] - Validating feed tag:\r\n            Checking amount of feed tags... OK\r\n\r\n         Question 2.2 [0.15 points] - Validating entry tag:\r\n            Checking amount of entries... OK\r\n\r\n         Question 2.3 [0.15 points] - Validating title (within feed) tag:\r\n            Checking amount of titles... OK\r\n\r\n         Question 2.4 [0.15 points] - Validating title (within entry) tag:\r\n            Checking amount of titles... OK\r\n\r\n         Question 2.5 [0.15 points] - Validating subtitle (within feed) tag:\r\n            Checking amount of subtitles... OK\r\n\r\n         Question 2.6 [0.15 points] - Validating summary (within entry) tag:\r\n            Checking amount of summaries... OK\r\n\r\n         Question 2.7 [0.15 points] - Validating link (within feed) tag:\r\n            Checking amount of links... OK\r\n\r\n         Question 2.8 [0.15 points] - Validating link (within entry) tag:\r\n            Checking amount of links... OK\r\n\r\n         Question 2.9 [0.15 points] - Validating updated (within feed) tag:\r\n            Checking amount of updateds... OK\r\n\r\n         Question 2.10 [0.15 points] - Validating updated (within entry) tag:\r\n            Checking amount of updateds... OK\r\n\r\n         Question 2.11 [0.15 points] - Validating id (within entry) tag:\r\n            Checking amount of ids... OK\r\n\r\n      TOTAL SCORE: 3.45 / 3.45\r\n   Running on batch mode for Student Name 2:\r\n      Question 1 [1.8 points] - Checking document.rss:\r\n         Looking for document.rss... OK\r\n         Loading document.rss... OK\r\n         Validating document against the W3C validation service... OK\r\n\r\n         Question 1.1 [0.15 points] - Validating rss tag:\r\n            Checking amount of rss tags... OK\r\n\r\n         Question 1.2 [0.15 points] - Validating channel tag:\r\n            Checking amount of channel tags... OK\r\n\r\n         Question 1.3 [0.15 points] - Validating item tag:\r\n            Checking amount of items... ERROR:\n               -Expected -> >=3; Found -> 2\r\n\r\n         Question 1.4 [0.15 points] - Validating title (within channel) tag:\r\n            Checking amount of titles... OK\r\n\r\n         Question 1.5 [0.15 points] - Validating title (within item) tag:\r\n            Checking amount of titles... OK\r\n\r\n         Question 1.6 [0.15 points] - Validating description (within channel) tag:\r\n            Checking amount of descriptions... OK\r\n\r\n         Question 1.7 [0.15 points] - Validating description (within item) tag:\r\n            Checking amount of descriptions... OK\r\n\r\n         Question 1.8 [0.15 points] - Validating link (within channel) tag:\r\n            Checking amount of links... OK\r\n\r\n         Question 1.9 [0.15 points] - Validating link (within item) tag:\r\n            Checking amount of links... OK\r\n\r\n         Question 1.10 [0.15 points] - Validating pubdate (within channel) tag:\r\n            Checking amount of pubdates... OK\r\n\r\n         Question 1.11 [0.15 points] - Validating pubdate (within item) tag:\r\n            Checking amount of pubdates... OK\r\n\r\n         Question 1.12 [0.15 points] - Validating guid (within item) tag:\r\n            Checking amount of guids... OK\r\n\r\n      Question 2 [1.65 points] - Checking document.atom:\r\n         Looking for document.atom... OK\r\n         Loading document.atom... OK\r\n         Validating document against the W3C validation service... OK\r\n\r\n         Question 2.1 [0.15 points] - Validating feed tag:\r\n            Checking amount of feed tags... OK\r\n\r\n         Question 2.2 [0.15 points] - Validating entry tag:\r\n            Checking amount of entries... OK\r\n\r\n         Question 2.3 [0.15 points] - Validating title (within feed) tag:\r\n            Checking amount of titles... OK\r\n\r\n         Question 2.4 [0.15 points] - Validating title (within entry) tag:\r\n            Checking amount of titles... OK\r\n\r\n         Question 2.5 [0.15 points] - Validating subtitle (within feed) tag:\r\n            Checking amount of subtitles... OK\r\n\r\n         Question 2.6 [0.15 points] - Validating summary (within entry) tag:\r\n            Checking amount of summaries... OK\r\n\r\n         Question 2.7 [0.15 points] - Validating link (within feed) tag:\r\n            Checking amount of links... OK\r\n\r\n         Question 2.8 [0.15 points] - Validating link (within entry) tag:\r\n            Checking amount of links... OK\r\n\r\n         Question 2.9 [0.15 points] - Validating updated (within feed) tag:\r\n            Checking amount of updateds... OK\r\n\r\n         Question 2.10 [0.15 points] - Validating updated (within entry) tag:\r\n            Checking amount of updateds... OK\r\n\r\n         Question 2.11 [0.15 points] - Validating id (within entry) tag:\r\n            Checking amount of ids... OK\r\n\r\n      TOTAL SCORE: 3.3 / 3.45\r\n   Running on batch mode for Student Name 3:\r\n      Question 1 [1.8 points] - Checking document.rss:\r\n         Looking for document.rss... OK\r\n         Loading document.rss... OK\r\n         Validating document against the W3C validation service... OK\r\n\r\n         Question 1.1 [0.15 points] - Validating rss tag:\r\n            Checking amount of rss tags... OK\r\n\r\n         Question 1.2 [0.15 points] - Validating channel tag:\r\n            Checking amount of channel tags... OK\r\n\r\n         Question 1.3 [0.15 points] - Validating item tag:\r\n            Checking amount of items... OK\r\n\r\n         Question 1.4 [0.15 points] - Validating title (within channel) tag:\r\n            Checking amount of titles... OK\r\n\r\n         Question 1.5 [0.15 points] - Validating title (within item) tag:\r\n            Checking amount of titles... OK\r\n\r\n         Question 1.6 [0.15 points] - Validating description (within channel) tag:\r\n            Checking amount of descriptions... OK\r\n\r\n         Question 1.7 [0.15 points] - Validating description (within item) tag:\r\n            Checking amount of descriptions... OK\r\n\r\n         Question 1.8 [0.15 points] - Validating link (within channel) tag:\r\n            Checking amount of links... OK\r\n\r\n         Question 1.9 [0.15 points] - Validating link (within item) tag:\r\n            Checking amount of links... OK\r\n\r\n         Question 1.10 [0.15 points] - Validating pubdate (within channel) tag:\r\n            Checking amount of pubdates... OK\r\n\r\n         Question 1.11 [0.15 points] - Validating pubdate (within item) tag:\r\n            Checking amount of pubdates... OK\r\n\r\n         Question 1.12 [0.15 points] - Validating guid (within item) tag:\r\n            Checking amount of guids... OK\r\n\r\n      Question 2 [1.65 points] - Checking document.atom:\r\n         Looking for document.atom... OK\r\n         Loading document.atom... OK\r\n         Validating document against the W3C validation service... OK\r\n\r\n         Question 2.1 [0.15 points] - Validating feed tag:\r\n            Checking amount of feed tags... OK\r\n\r\n         Question 2.2 [0.15 points] - Validating entry tag:\r\n            Checking amount of entries... OK\r\n\r\n         Question 2.3 [0.15 points] - Validating title (within feed) tag:\r\n            Checking amount of titles... OK\r\n\r\n         Question 2.4 [0.15 points] - Validating title (within entry) tag:\r\n            Checking amount of titles... OK\r\n\r\n         Question 2.5 [0.15 points] - Validating subtitle (within feed) tag:\r\n            Checking amount of subtitles... ERROR:\n               -Expected -> =1; Found -> 0\r\n\r\n         Question 2.6 [0.15 points] - Validating summary (within entry) tag:\r\n            Checking amount of summaries... OK\r\n\r\n         Question 2.7 [0.15 points] - Validating link (within feed) tag:\r\n            Checking amount of links... OK\r\n\r\n         Question 2.8 [0.15 points] - Validating link (within entry) tag:\r\n            Checking amount of links... OK\r\n\r\n         Question 2.9 [0.15 points] - Validating updated (within feed) tag:\r\n            Checking amount of updateds... OK\r\n\r\n         Question 2.10 [0.15 points] - Validating updated (within entry) tag:\r\n            Checking amount of updateds... OK\r\n\r\n         Question 2.11 [0.15 points] - Validating id (within entry) tag:\r\n            Checking amount of ids... OK\r\n\r\n      TOTAL SCORE: 3.3 / 3.45\r\n   Running on batch mode for Student Name 4:\r\n      Question 1 [1.8 points] - Checking document.rss:\r\n         Looking for document.rss... OK\r\n         Loading document.rss... OK\r\n         Validating document against the W3C validation service... ERROR:\n            -Exception of type 'AutoCheck.Core.Exceptions.DocumentInvalidException' was thrown.\r\n\r\n      Question 2 [1.65 points] - Checking document.atom:\r\n         Looking for document.atom... OK\r\n         Loading document.atom... OK\r\n         Validating document against the W3C validation service... OK\r\n\r\n         Question 2.1 [0.15 points] - Validating feed tag:\r\n            Checking amount of feed tags... OK\r\n\r\n         Question 2.2 [0.15 points] - Validating entry tag:\r\n            Checking amount of entries... ERROR:\n               -Expected -> >=3; Found -> 2\r\n\r\n         Question 2.3 [0.15 points] - Validating title (within feed) tag:\r\n            Checking amount of titles... OK\r\n\r\n         Question 2.4 [0.15 points] - Validating title (within entry) tag:\r\n            Checking amount of titles... OK\r\n\r\n         Question 2.5 [0.15 points] - Validating subtitle (within feed) tag:\r\n            Checking amount of subtitles... OK\r\n\r\n         Question 2.6 [0.15 points] - Validating summary (within entry) tag:\r\n            Checking amount of summaries... OK\r\n\r\n         Question 2.7 [0.15 points] - Validating link (within feed) tag:\r\n            Checking amount of links... OK\r\n\r\n         Question 2.8 [0.15 points] - Validating link (within entry) tag:\r\n            Checking amount of links... OK\r\n\r\n         Question 2.9 [0.15 points] - Validating updated (within feed) tag:\r\n            Checking amount of updateds... OK\r\n\r\n         Question 2.10 [0.15 points] - Validating updated (within entry) tag:\r\n            Checking amount of updateds... OK\r\n\r\n         Question 2.11 [0.15 points] - Validating id (within entry) tag:\r\n            Checking amount of ids... OK\r\n\r\n      TOTAL SCORE: 1.5 / 3.45";
            Assert.AreEqual(expected, s.Output.ToString());
        }

#endregion
#region Output
    [Test, Category("Output")]
    public void Output_SINGLE_FILE_1()
    {             
        var log = Path.Combine(AutoCheck.Core.Utils.AppFolder, "logs", "OUTPUT SINGLE 1_Student Name 1.log");
        Assert.IsFalse(File.Exists(log));
        
        var s = new AutoCheck.Core.Script(Path.Combine(GetSamplePath("script"), "output", "output_single_1.yaml"));
        Assert.IsTrue(File.Exists(log));        
        Assert.IsTrue(File.ReadAllText(log).Equals(s.Output.ToString()));                
    } 

    [Test, Category("Output")]
    public void Output_BATCH_FILE_1()
    {             
        var logs = new string[]{
            Path.Combine(AutoCheck.Core.Utils.AppFolder, "logs", "OUTPUT BATCH 1_Student Name 1.log"),
            Path.Combine(AutoCheck.Core.Utils.AppFolder, "logs", "OUTPUT BATCH 1_Student Name 2.log"),
            Path.Combine(AutoCheck.Core.Utils.AppFolder, "logs", "OUTPUT BATCH 1_Student Name 3.log")
        };

        foreach(var l in logs)
            Assert.IsFalse(File.Exists(l));
        
        var i = 0;
        var s = new AutoCheck.Core.Script(Path.Combine(GetSamplePath("script"), "output", "output_batch_1.yaml"));
                
        foreach(var l in logs){
            Assert.IsTrue(File.Exists(l));        
            Assert.IsTrue(File.ReadAllText(l).Equals(s.Output.ToArray()[i++]));
        }
    } 
#endregion                               
        //TODO: test remote scripts in batch mode
        //TODO: parse YAML dictionaries to C# objects (casting and testing are pending)
        //TODO: GDrive tests fails randomly, check how to fix it (a timeout hasn't worked)
        
        //NOTE: dotnet test --filter TestCategory=Output
    }
}