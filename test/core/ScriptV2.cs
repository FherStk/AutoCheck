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

        // [SetUp]
        // public void Setup() 
        // {
        //     base.Setup("script");
        //     AutoCheck.Core.Output.Instance.Disable();
        // }

        // [TearDown]
        // public void TearDown(){
        //     AutoCheck.Core.Output.Instance.Enable();
        // }  

        [OneTimeSetUp]
        public void Init() 
        {
            base.Setup("script");
            AutoCheck.Core.Output.Instance.Disable();

            //ZIP
            File.Delete(GetSampleFile("nopass.txt"));
            File.Delete(GetSampleFile("nopass.zip"));
            File.Delete(GetSampleFile("nofound.zip"));
            File.Delete(GetSampleFile("script\\recursive", "nopass.zip"));
            File.Delete(GetSampleFile("script\\recursive", "nofound.zip"));
            File.Delete(GetSampleFile("script\\recursive", "nopass.txt"));           
            
            //BBDD
            File.Delete(GetSampleFile("dump.sql"));
            File.Delete(GetSampleFile("override.sql"));
            File.Delete(GetSampleFile("nooverride.sql"));
            
            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok1", "postgres", "postgres"))
                if(psql.ExistsDataBase()) psql.DropDataBase();

            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok2", "postgres", "postgres"))
                if(psql.ExistsDataBase()) psql.DropDataBase();

            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok3", "postgres", "postgres"))
                if(psql.ExistsDataBase()) psql.DropDataBase();
        }

        [OneTimeTearDown]
        public void Cleanup(){     
            AutoCheck.Core.Output.Instance.Enable();
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
            Assert.AreEqual("vars ok1", s.Vars["script_name"].ToString());            
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
            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile("nopass.zip"));                        
            Assert.IsTrue(File.Exists(GetSampleFile("nopass.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile("nopass.txt"))); 

            var s = new TestScript(GetSampleFile("extract_ok1.yaml"));
                        
            Assert.IsTrue(File.Exists(GetSampleFile("nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile("nopass.txt")));
            File.Delete(GetSampleFile("nopass.txt"));

            //TEST 2: non-existing file + no remove + no recursive 
            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile("nofound.zip"));                        
            Assert.IsTrue(File.Exists(GetSampleFile("nofound.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile("nopass.txt"))); 

            s = new TestScript(GetSampleFile("extract_ok2.yaml"));

            Assert.IsTrue(File.Exists(GetSampleFile("nofound.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile("nopass.txt"))); 
            File.Delete(GetSampleFile("nofound.zip"));

            //TEST 3: [nopass.zip + no remove + no recursive ], [recursive/nopass.zip + remove + no recursive ]
            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile("script\\recursive", "nopass.zip"));     
            Assert.IsTrue(File.Exists(GetSampleFile("script\\recursive", "nopass.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile("script\\recursive", "nopass.txt")));
            
            s = new TestScript(GetSampleFile("extract_ok3.yaml"));

            Assert.IsTrue(File.Exists(GetSampleFile("nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile("nopass.txt"))); 
            Assert.IsFalse(File.Exists(GetSampleFile("script\\recursive", "nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile("script\\recursive", "nopass.txt")));
            File.Delete(GetSampleFile("script\\recursive", "nopass.txt"));
            File.Delete(GetSampleFile("nopass.txt"));

            //TEST 4: *.zip + remove + recursive 
            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile("script\\recursive", "nopass.zip"));     
            s = new TestScript(GetSampleFile("extract_ok4.yaml"));

            Assert.IsFalse(File.Exists(GetSampleFile("nopass.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile("script\\recursive", "nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile("nopass.txt")));
            Assert.IsTrue(File.Exists(GetSampleFile("script\\recursive", "nopass.txt")));
            File.Delete(GetSampleFile("nopass.txt"));
            File.Delete(GetSampleFile("script\\recursive", "nopass.txt"));
        }

        //TODO: Extract_KO() testing something different to ZIP (RAR, TAR, GZ...)

        [Test]
        public void RestoreDB_OK()
        {  
            //TEST 1: *.sql + no remove + no override 
            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile("dump.sql"));          
            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok1", "postgres", "postgres")){
                Assert.IsFalse(psql.ExistsDataBase());                
                var s = new TestScript(GetSampleFile("restoredb_ok1.yaml"));   
                
                Assert.IsTrue(psql.ExistsDataBase());
                Assert.IsTrue(File.Exists(GetSampleFile("dump.sql"))); 
                psql.DropDataBase();
            }  

            //TEST 2: *.sql + remove + no override 
            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok2", "postgres", "postgres")){
                Assert.IsFalse(psql.ExistsDataBase());                
                var s = new TestScript(GetSampleFile("restoredb_ok2.yaml"));   
                
                Assert.IsTrue(psql.ExistsDataBase());
                Assert.IsFalse(File.Exists(GetSampleFile("dump.sql"))); 
            } 

            //TEST 3: 2 separated files + remove + override suceeded
            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile("override.sql"));
            File.Copy(GetSampleFile("postgres", "dump.sql"), GetSampleFile("nooverride.sql"));
            
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
                Assert.IsTrue(File.Exists(GetSampleFile("nooverride.sql"))); 
                Assert.IsFalse(File.Exists(GetSampleFile("override.sql"))); 
            }

            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok2", "postgres", "postgres")){
                Assert.IsTrue(psql.ExistsDataBase());                                
                Assert.AreEqual(10, psql.CountRegisters("test.work_history"));
                psql.DropDataBase();                
            }   

            //TEST 4: nooverride.sql + remove + no override allowed 
            using(var psql = new AutoCheck.Connectors.Postgres("localhost", "AutoCheck-Test-RestoreDB-Ok3", "postgres", "postgres")){
                Assert.True(psql.ExistsDataBase());                                
                
                Assert.AreEqual(10, psql.CountRegisters("test.work_history"));
                psql.Insert<short>("test.work_history", "id_employee", new Dictionary<string, object>(){{"id_employee", (short)999}, {"id_work", "MK_REP"}, {"id_department", (short)20}});
                Assert.AreEqual(11, psql.CountRegisters("test.work_history"));

                var s = new TestScript(GetSampleFile("restoredb_ok4.yaml"));   
                
                Assert.IsTrue(psql.ExistsDataBase());
                Assert.IsFalse(File.Exists(GetSampleFile("nooverride.sql"))); 
                Assert.AreEqual(11, psql.CountRegisters("test.work_history"));
                psql.DropDataBase();      
            }
        }
    }
}
