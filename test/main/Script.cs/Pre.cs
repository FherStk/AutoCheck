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
    public class Pre : Test
    {             
        private string _user = AutoCheck.Core.Utils.ConfigFile("gdrive_account.txt");
        private string _secret = AutoCheck.Core.Utils.ConfigFile("gdrive_secret.json");    
        
        [OneTimeSetUp]
        public virtual void StartUp() 
        {
            SamplesScriptFolder = GetSamplePath(Path.Combine("script", Name));            
        }

        protected override void CleanUp(){
            base.CleanUp();

            //Clean GDrive
            if(true) 
            {
                //WARNING: Set condition to false in order to avoid GDrive testing on missconfigured hosts                
                using(var gdrive = new AutoCheck.Core.Connectors.GDrive(_user, _secret)){
                    gdrive.DeleteFolder("\\AutoCheck\\test", "uploadgdrive_ok1");
                    gdrive.DeleteFolder("\\AutoCheck\\test", "uploadgdrive_ok2");
                    gdrive.DeleteFolder("\\AutoCheck\\test", "uploadgdrive_ok3");
                }  
            } 

            //Clean databases   
            if(true) 
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

                using(var psql = new AutoCheck.Core.Connectors.Postgres("localhost", "restore_db_ok5-dump1_sql", "postgres", "postgres"))
                    if(psql.ExistsDataBase()) psql.DropDataBase();

                using(var psql = new AutoCheck.Core.Connectors.Postgres("localhost", "restore_db_ok5-dump2_sql", "postgres", "postgres"))
                    if(psql.ExistsDataBase()) psql.DropDataBase(); 
            } 
        }

        [Test, Category("Pre"), Category("Zip"), Category("Local")]
        public void Extract_ZIP_NOREMOVE_NORECURSIVE()
        { 
            var dest = Path.Combine(TempScriptFolder, "extract", "test1");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);

            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest, "nopass.zip"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile(dest, "nopass.txt"))); 

            var s = new AutoCheck.Core.Script(GetSampleFile("extract\\extract_ok1.yaml"));
                        
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.txt")));
            
            Directory.Delete(dest, true);
        }

        [Test, Category("Pre"), Category("Zip"), Category("Local")]
        public void Extract_NONEXISTING_NOREMOVE_NORECURSIVE()
        { 
            var dest = Path.Combine(TempScriptFolder, "extract", "test2");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);           

            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest, "nofound.zip"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nofound.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile(dest, "nopass.txt"))); 

            var s = new AutoCheck.Core.Script(GetSampleFile("extract\\extract_ok2.yaml"));

            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nofound.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile(dest, "nopass.txt"))); 
           
            Directory.Delete(dest, true);
        }

        [Test, Category("Pre"), Category("Zip"), Category("Local")]
        public void Extract_SPECIFIC_BATCH()
        { 
            var dest = Path.Combine(TempScriptFolder, "extract", "test3");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);                     

            var rec = Path.Combine(dest, "recursive");
            if(!Directory.Exists(rec)) Directory.CreateDirectory(rec);

            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest, "nopass.zip"));
            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(rec, "nopass.zip"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile(dest, "nopass.txt")));
            Assert.IsTrue(File.Exists(GetSampleFile(rec, "nopass.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile(rec, "nopass.txt")));
            
            var s = new AutoCheck.Core.Script(GetSampleFile("extract\\extract_ok3.yaml"));

            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.txt"))); 
            Assert.IsFalse(File.Exists(GetSampleFile(rec, "nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile(rec, "nopass.txt")));
            
            Directory.Delete(dest, true);
        }

        [Test, Category("Pre"), Category("Zip"), Category("Local")]
        public void Extract_ZIP_REMOVE_RECURSIVE()
        { 
            var dest = Path.Combine(TempScriptFolder, "extract", "test4");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);                     

            var rec = Path.Combine(dest, "recursive");
            if(!Directory.Exists(rec)) Directory.CreateDirectory(rec);

            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(dest, "nopass.zip"));
            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(rec, "nopass.zip"));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile(dest, "nopass.txt")));
            Assert.IsTrue(File.Exists(GetSampleFile(rec, "nopass.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile(rec, "nopass.txt")));
            
            var s = new AutoCheck.Core.Script(GetSampleFile("extract\\extract_ok4.yaml"));

            Assert.IsFalse(File.Exists(GetSampleFile(dest, "nopass.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile(rec, "nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile(dest, "nopass.txt")));
            Assert.IsTrue(File.Exists(GetSampleFile(rec, "nopass.txt")));
            
            Directory.Delete(dest, true);
        }

        //TODO: Extract_KO() testing something different to ZIP (RAR, TAR, GZ...)

        [Test, Category("Pre"), Category("SQL"), Category("Remote")] 
        public void RestoreDB_SQL_NOREMOVE_NOOVERRIDE_NORECURSIVE()
        {              
            var dest = Path.Combine(TempScriptFolder, "restore", "test1");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);

            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "dump.sql"));          
            using(var psql = new AutoCheck.Core.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok1", "postgres", "postgres")){
                Assert.IsFalse(psql.ExistsDataBase());                
                var s = new AutoCheck.Core.Script(GetSampleFile("restore_db\\restore_db_ok1.yaml"));   
                
                Assert.IsTrue(psql.ExistsDataBase());
                Assert.IsTrue(File.Exists(GetSampleFile(dest, "dump.sql"))); 
                psql.DropDataBase();
            }
            
            Directory.Delete(dest, true);
        }

        [Test, Category("Pre"), Category("SQL"), Category("Remote")] 
        public void RestoreDB_SQL_REMOVE_NOOVERRIDE_NORECURSIVE()
        {              
            var dest = Path.Combine(TempScriptFolder, "restore", "test2");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);

            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "dump.sql"));                     
            using(var psql = new AutoCheck.Core.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok2", "postgres", "postgres")){
                Assert.IsFalse(psql.ExistsDataBase());                
                var s = new AutoCheck.Core.Script(GetSampleFile("restore_db\\restore_db_ok2.yaml"));   
                
                Assert.IsTrue(psql.ExistsDataBase());
                Assert.IsFalse(File.Exists(GetSampleFile(dest, "dump.sql"))); 
                psql.DropDataBase();
            } 

            Directory.Delete(dest, true);
        }

        [Test, Category("Pre"), Category("SQL"), Category("Remote")] 
        public void RestoreDB_SPECIFIC_BATCH()
        {              
            var dest = Path.Combine(TempScriptFolder, "restore", "test3");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
                        
            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "dump.sql"));                         
            using(var psql = new AutoCheck.Core.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok31", "postgres", "postgres")){
                Assert.IsFalse(psql.ExistsDataBase()); 
                var s = new AutoCheck.Core.Script(GetSampleFile("restore_db\\restore_db_ok3.1.yaml"));   //TODO: Should use a own file (not resue another test one...)   
                
                Assert.IsTrue(psql.ExistsDataBase());
                Assert.AreEqual(10, psql.ExecuteScalar<long>("SELECT COUNT (*) FROM test.work_history;"));
                psql.ExecuteNonQuery($"INSERT INTO test.work_history (id_employee, id_work, id_department) VALUES (999, 'MK_REP', 20);");
                Assert.AreEqual(11, psql.ExecuteScalar<long>("SELECT COUNT (*) FROM test.work_history;"));
            } 

            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "override.sql"));
            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "nooverride.sql"));
            using(var psql = new AutoCheck.Core.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok32", "postgres", "postgres")){
                Assert.IsFalse(psql.ExistsDataBase());                                
                var s = new AutoCheck.Core.Script(GetSampleFile("restore_db\\restore_db_ok3.2.yaml"));   
                
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

        [Test, Category("Pre"), Category("SQL"), Category("Remote")] 
        public void RestoreDB_SPECIFIC_REMOVE_NOOVERRIDE_NORECURSIVE()
        {              
            var dest = Path.Combine(TempScriptFolder, "restore", "test4");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);

            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "nooverride.sql"));
            using(var psql = new AutoCheck.Core.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok4", "postgres", "postgres")){
                Assert.IsFalse(psql.ExistsDataBase()); 
                var s = new AutoCheck.Core.Script(GetSampleFile("restore_db\\restore_db_ok4.yaml"));
                Assert.IsTrue(psql.ExistsDataBase());                                
                 
                Assert.AreEqual(10, psql.ExecuteScalar<long>("SELECT COUNT (*) FROM test.work_history;"));
                psql.ExecuteNonQuery($"INSERT INTO test.work_history (id_employee, id_work, id_department) VALUES (999, 'MK_REP', 20);");
                Assert.AreEqual(11, psql.ExecuteScalar<long>("SELECT COUNT (*) FROM test.work_history;"));

                s = new AutoCheck.Core.Script(GetSampleFile("restore_db\\restore_db_ok4.yaml"));   
                
                Assert.IsTrue(psql.ExistsDataBase());
                Assert.IsFalse(File.Exists(GetSampleFile(dest, "nooverride.sql"))); 
                Assert.AreEqual(11, psql.ExecuteScalar<long>("SELECT COUNT (*) FROM test.work_history;"));
                psql.DropDataBase();      
            } 

            Directory.Delete(dest, true);          
        }

        [Test, Category("Pre"), Category("SQL"), Category("Remote")] 
        public void RestoreDB_SQL_REMOVE_NOOVERRIDE_RECURSIVE()
        {              
            var dest = Path.Combine(TempScriptFolder, "restore", "test5");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
           
            var rec = Path.Combine(dest, "recursive");
            if(!Directory.Exists(rec)) Directory.CreateDirectory(rec);

            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "dump1.sql"));
            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(rec, "dump2.sql"));
            
            using(var psql = new AutoCheck.Core.Connectors.Postgres("localhost", "restore_db_ok5-dump1_sql", "postgres", "postgres")){
                Assert.IsFalse(psql.ExistsDataBase());
                var s = new AutoCheck.Core.Script(GetSampleFile("restore_db\\restore_db_ok5.yaml"));                   

                Assert.IsTrue(psql.ExistsDataBase());
                Assert.IsFalse(File.Exists(GetSampleFile(dest, "dump1.sql"))); 
                psql.DropDataBase();
            }

            using(var psql = new AutoCheck.Core.Connectors.Postgres("localhost", "restore_db_ok5-dump2_sql", "postgres", "postgres")){
                Assert.IsTrue(psql.ExistsDataBase());
                Assert.IsFalse(File.Exists(GetSampleFile(rec, "dump2.sql"))); 
                psql.DropDataBase();
            }

            Directory.Delete(dest, true);
        }

        //TODO: RestoreDB_KO() testing something different to PSQL (SQL Server, MySQL/MariaDB, Oracle...)
        
        [Test, Category("Pre"), Category("GDrive"), Category("Remote")]
        public void UploadGDrive_NOREMOVE_UPLOAD_NOLINK_NORECURSIVE()
        {  
            var dest = Path.Combine(TempScriptFolder, "upload", "test1");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "uploaded.sql"));                    
            
            var remotePath = "\\AutoCheck\\test\\upload_gdrive_ok1";
            var remoteFile = "uploaded.sql";
            using(var gdrive = new AutoCheck.Core.Connectors.GDrive(_user, _secret)){
                Assert.IsFalse(gdrive.ExistsFolder(remotePath));                
                
                var s = new AutoCheck.Core.Script(GetSampleFile("upload_gdrive\\upload_gdrive_ok1.yaml"));                   
                System.Threading.Thread.Sleep(5000);

                Assert.IsTrue(File.Exists(GetSampleFile(dest, remoteFile))); 
                Assert.IsTrue(gdrive.ExistsFile(remotePath, remoteFile));
            } 
            
            Directory.Delete(dest, true);
        }

        [Test, Category("Pre"), Category("GDrive"), Category("Remote")]
        public void UploadGDrive_REMOVE_UPLOAD_NOLINK_RECURSIVE()
        {  
            //TODO: fails sometimes... need some waiting time to see the changes?
            var dest = Path.Combine(TempScriptFolder, "upload", "test2");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "uploaded.sql"));          

            var rec = Path.Combine(dest, "recursive");
            if(!Directory.Exists(rec)) Directory.CreateDirectory(rec);
            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile(rec, "uploaded.zip"));                                 
            
            var remotePath = "\\AutoCheck\\test\\upload_gdrive_ok2";
            var remotePath2 = Path.Combine(remotePath, "recursive");
            var remoteFile = "uploaded.sql";
            var remoteFile2 = "uploaded.zip";
            using(var gdrive = new AutoCheck.Core.Connectors.GDrive(_user, _secret)){
                Assert.IsFalse(gdrive.ExistsFolder(remotePath));

                var s = new AutoCheck.Core.Script(GetSampleFile("upload_gdrive\\upload_gdrive_ok2.yaml"));   
                System.Threading.Thread.Sleep(5000);

                Assert.IsFalse(File.Exists(GetSampleFile(dest, remoteFile))); 
                Assert.IsFalse(Directory.Exists(rec));

                Assert.IsTrue(gdrive.ExistsFile(remotePath, remoteFile));
                Assert.IsTrue(gdrive.ExistsFolder(remotePath, "recursive"));
                Assert.IsTrue(gdrive.ExistsFile(remotePath2, remoteFile2));
            }            
        }

        [Test, Category("Pre"), Category("GDrive"), Category("Remote")]
        public void UploadGDrive_NOREMOVE_COPY_LINK_NORECURSIVE()
        {  
            var dest = Path.Combine(TempScriptFolder, "upload", "test3");
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile(dest, "uploaded.sql"));          
            
            var remotePath = "\\AutoCheck\\test\\upload_gdrive_ok3";
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);  
            
            File.Copy(GetSampleFile("gdrive", "download.txt"), GetSampleFile(dest, "download.txt"));
            using(var gdrive = new AutoCheck.Core.Connectors.GDrive(_user, _secret)){
                Assert.IsFalse(gdrive.ExistsFolder(remotePath));

                var s = new AutoCheck.Core.Script(GetSampleFile("upload_gdrive\\upload_gdrive_ok3.yaml"));                               
                System.Threading.Thread.Sleep(5000);

                Assert.IsTrue(gdrive.ExistsFile(remotePath, "1mb-test_zip.zip"));
                Assert.IsTrue(gdrive.ExistsFile(remotePath, "10mb.test"));
            }

            Directory.Delete(dest, true);
        }

        //TODO: UploadGDrive_KO() testing something unable to parse (read the PDF content for example, it will be supported in a near future, but not right now) or upload
    }
}