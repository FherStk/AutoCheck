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

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using NUnit.Framework;
using AutoCheck.Core.Exceptions;

namespace AutoCheck.Test.Connectors
{    
    [Parallelizable(ParallelScope.All)]   
    public class Postgres : Test
    {   
        /// <summary>
        /// The connector instance is created here because a new one-time use BBDD will be created on every startup, and dropped when done.
        /// </summary>
        private ConcurrentDictionary<string, AutoCheck.Core.Connectors.Postgres> Pool = new ConcurrentDictionary<string, AutoCheck.Core.Connectors.Postgres>();
        
        private readonly List<(string schema, string table)> _emptySources = new List<(string, string)>{
            (null, null),
            (string.Empty, string.Empty),
            (_FAKE, string.Empty),
            (string.Empty, _FAKE),
            (_FAKE, null),
            (null, _FAKE),
        };

        private readonly List<(string schema, string table)> _emptyDestinations = new List<(string, string)>{
            (null, null),
            (string.Empty, string.Empty),
            (_FAKE, string.Empty),
            (string.Empty, _FAKE),
            (_FAKE, null),
            (null, _FAKE),
        };   

        private readonly List<(string schema, string table)> _wrongSources = new List<(string, string)>{
            (_FAKE, _FAKE),            
            (_SCHEMA, _FAKE),
        };  

        private readonly List<(string schema, string table)> _wrongDestinations = new List<(string, string)>{
            (_FAKE, _FAKE),            
            (_SCHEMA, _FAKE),
        };                

        private const string _HOST = "localhost";    
        
        private const string _ADMIN = "postgres";
        
        private const string _SCHEMA = "test";
               

        [SetUp]
        public void Setup() 
        {
            //Create a new and unique database for the current context (each test has its own database)
            var conn = new AutoCheck.Core.Connectors.Postgres(_HOST, string.Format("autocheck_{0}", TestContext.CurrentContext.Test.ID), _ADMIN, _ADMIN);
            if(conn.ExistsDataBase()) conn.DropDataBase();
            conn.CreateDataBase(base.GetSampleFile("dump.sql"));                  

            //Storing the connector instance for the current context
            var added = false;
            do added = this.Pool.TryAdd(TestContext.CurrentContext.Test.ID, conn);             
            while(!added);                
        }

        [OneTimeTearDown]
        public new void OneTimeTearDown(){     
            this.Pool.Clear();                        
        } 

        [TearDown]
        public void TearDown(){
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            conn.DropDataBase();
            conn.Dispose();
        }

        protected override void CleanUp(){
            using(var conn = new AutoCheck.Core.Connectors.Postgres(_HOST, _ADMIN, _ADMIN, _ADMIN)){ //default BBDD postgres can be used to user management
                try{ conn.ExecuteNonQuery(string.Format("DROP USER {0}", "usermanagement_user1")); } catch{}
                try{ conn.ExecuteNonQuery(string.Format("DROP USER {0}", "usermanagement_user2")); } catch{}                       
                try{ conn.ExecuteNonQuery(string.Format("DROP USER {0}", "rolemanagement_role1")); } catch{}                   
                
                try{ conn.ExecuteNonQuery(string.Format("DROP OWNED BY {0}", "permissionmanagement_role1")); } catch{}
                try{ conn.ExecuteNonQuery(string.Format("DROP USER {0}", "permissionmanagement_role1")); } catch{}

                try{ conn.ExecuteNonQuery(string.Format("DROP OWNED BY {0}", "permissionmanagement_user1")); } catch{}
                try{ conn.ExecuteNonQuery(string.Format("DROP USER {0}", "permissionmanagement_user1")); } catch{}
            }
        }

        [Test]
        [TestCase("", "", "")]
        [TestCase(_FAKE, "", "")]
        [TestCase(_FAKE, _FAKE, "")]
        public void Constructor_Throws_ArgumentNullException(string host, string database, string username)
        {      
             Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.Connectors.Postgres(host, database, username));
        }

        [Test]
        [TestCase(_HOST, _FAKE, _ADMIN, _ADMIN)]
        public void Constructor_DoesNotThrow(string host, string database, string username, string password)
        {                                                        
            Assert.DoesNotThrow(() => new AutoCheck.Core.Connectors.Postgres(host, database, username, password));         
        }

        [Test]
        [TestCase(_FAKE, _FAKE, _FAKE)]
        [TestCase(_HOST, _FAKE, _FAKE)]
        [TestCase(_HOST, _FAKE, _ADMIN)]
        [TestCase(_HOST, "autocheck", _FAKE)]
        public void TestConnection_Throws_ConnectionInvalidException(string host, string database, string username)
        {                    
            Assert.Throws<ConnectionInvalidException>(() => new AutoCheck.Core.Connectors.Postgres(host,database, username).TestConnection());            
        }

        [Test]
        public void TestConnection_DoesNotThrow()
        {                                
            Assert.DoesNotThrow(() => this.Pool[TestContext.CurrentContext.Test.ID].TestConnection());
        }

        [Test]
        public void ExistsDataBase_DoesNotThrow_True() 
        {            
            Assert.IsTrue(this.Pool[TestContext.CurrentContext.Test.ID].ExistsDataBase());                        
        }  

        [Test]
        [TestCase(_HOST, _FAKE, _ADMIN, _ADMIN)]
        public void ExistsDataBase_DoesNotThrow_False(string host, string database, string username, string password) 
        {            
            using(var conn =  new AutoCheck.Core.Connectors.Postgres(host, database, username, password))
                Assert.IsFalse(conn.ExistsDataBase());
        }     

        [Test]
        [TestCase(null)]
        public void ExecuteQuery_Throws_ArgumentNullException(string query)
        {
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<ArgumentNullException>(() => conn.ExecuteQuery(query));
        } 

        [Test]
        [TestCase(_FAKE)]
        [TestCase("SELECT * FROM fake")]
        public void ExecuteQuery_Throws_QueryInvalidException(string query)
        {
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<QueryInvalidException>(() => conn.ExecuteQuery(query));
        } 

        [Test]
        [TestCase("SELECT * FROM test.departments", 9, 4, _SCHEMA, "departments", "name_department", "IT")]
        [TestCase("SELECT name_department FROM test.departments WHERE id_department=60", 1, 1, _SCHEMA, "departments", "name_department", "IT")]
        public void ExecuteQuery_DoesNotThrow_READ(string query, int rowCount, int columnCount, string schema, string table, string scalarField, string scalarValue)
        {
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];            
            var ds = conn.ExecuteQuery(query);
            Assert.AreEqual(rowCount, ds.Tables[0].Rows.Count);
            Assert.AreEqual(columnCount, ds.Tables[0].Columns.Count);
            Assert.AreEqual(schema, ds.Tables[0].Namespace);
            Assert.AreEqual(table, ds.Tables[0].TableName);
            Assert.AreEqual(scalarValue, ds.Tables[0].Rows[0][scalarField]);                
        } 

        [Test]
        [TestCase("INSERT INTO test.regions (id_region, name_region) VALUES ((SELECT MAX(id_region)+1 FROM test.regions), 'TEST')")]
        [TestCase("UPDATE test.regions SET name_region='TESTv2' WHERE id_region = (SELECT MAX(id_region) FROM test.regions)")]
        [TestCase("DELETE FROM test.regions WHERE id_region = (SELECT MAX(id_region) FROM test.regions)")]
        public void ExecuteQuery_DoesNotThrow_UPDATE(string query)
        {
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];                        
            Assert.DoesNotThrow(() => conn.ExecuteQuery(query));            
        }

        [Test]
        [TestCase(null)]
        public void ExecuteNonQuery_Throws_ArgumentNullException(string query)
        {   
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<ArgumentNullException>(() => conn.ExecuteNonQuery(query));
        }

        [Test]
        [TestCase(_FAKE)]
        [TestCase("SELECT * FROM fake")]
        public void ExecuteNonQuery_Throws_QueryInvalidException(string query)
        {   
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<QueryInvalidException>(() => conn.ExecuteNonQuery(query));
        }

        [Test]
        [TestCase("SELECT * FROM test.departments")]
        [TestCase("SELECT name_department FROM test.departments WHERE id_department=60")]
        [TestCase("INSERT INTO test.regions (id_region, name_region) VALUES ((SELECT MAX(id_region)+1 FROM test.regions), 'TEST')")]
        [TestCase("UPDATE test.regions SET name_region='TESTv2' WHERE id_region = (SELECT MAX(id_region) FROM test.regions)")]
        [TestCase("DELETE FROM test.regions WHERE id_region = (SELECT MAX(id_region) FROM test.regions)")]
        public void ExecuteNonQuery_DoesNotThrow(string query)
        {   
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.DoesNotThrow(() => conn.ExecuteNonQuery(query));
        }

        [Test]
        [TestCase(null)]
        public void ExecuteScalar_Throws_ArgumentNullException(string query)
        {   
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<ArgumentNullException>(() => conn.ExecuteScalar<object>(query));
        }

        [Test]
        [TestCase(_FAKE)]
        [TestCase("SELECT * FROM fake")]
        public void ExecuteScalar_Throws_QueryInvalidException(string query)
        {   
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<QueryInvalidException>(() => conn.ExecuteScalar<object>(query));
        }

        [Test]
        [TestCase("SELECT * FROM test.departments", ExpectedResult=10)]
        [TestCase("SELECT name_department FROM test.departments WHERE id_department=60", ExpectedResult="IT")]
        [TestCase("INSERT INTO test.regions (id_region, name_region) VALUES ((SELECT MAX(id_region)+1 FROM test.regions), 'TEST') RETURNING id_region;", ExpectedResult=3)]        
        [TestCase("INSERT INTO test.regions (id_region, name_region) VALUES ((SELECT MAX(id_region)+1 FROM test.regions), 'TEST')", ExpectedResult=null)]
        [TestCase("UPDATE test.regions SET name_region='TESTv2' WHERE id_region = (SELECT MAX(id_region) FROM test.regions)", ExpectedResult=null)]
        [TestCase("DELETE FROM test.regions WHERE id_region = (SELECT MAX(id_region) FROM test.regions)", ExpectedResult=null)]
        public object ExecuteScalar_DoesNotThrow(string query)
        {   
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            return conn.ExecuteScalar<object>(query);
        }    

        [Test]
        public void UserManagement(){  
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            
            //Vars for testing
            var user1 = "usermanagement_user1";
            var user2 = "usermanagement_user2";
            var user3 = "usermanagement_user3";

            //Argument validation
            Assert.Throws<ArgumentNullException>(() => conn.CreateUser(string.Empty));
            Assert.Throws<ArgumentNullException>(() => conn.DropUser(string.Empty));
        
            //Create
            Assert.DoesNotThrow(() =>conn.CreateUser(user1));
            Assert.DoesNotThrow(() =>conn.CreateUser(user2, "PASS"));
            var users = conn.GetUsers();     
            Assert.IsTrue(users.Tables[0].Select(string.Format("username='{0}'", user1)).Length == 1);
            Assert.IsTrue(users.Tables[0].Select(string.Format("username='{0}'", user2)).Length == 1);
            
            //Create an existing one
            Assert.Throws<QueryInvalidException>(() =>conn.CreateUser(user2));   

            //Search         
            Assert.IsTrue(conn.ExistsUser(user2));            
            Assert.IsFalse(conn.ExistsUser(user3));      

            //Distroying existing users
            Assert.DoesNotThrow(() => conn.DropUser(user1));            
            Assert.DoesNotThrow(() => conn.DropUser(user2));
            users = conn.GetUsers();
            Assert.IsTrue(users.Tables[0].Select(string.Format("username='{0}'", user1)).Length == 0);
            Assert.IsTrue(users.Tables[0].Select(string.Format("username='{0}'", user2)).Length == 0);

            //Distroying non-existing users
            Assert.Throws<QueryInvalidException>(() => conn.DropUser(user3));
        }   

        [Test]
        public void RoleManagement(){    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            
            //Vars for testing
            var role1 = "rolemanagement_role1";
            var role2 = "rolemanagement_role2";
            string filter = string.Format("rolname='{0}'", role1);

            //Argument validation
            Assert.Throws<ArgumentNullException>(() => conn.CreateRole(string.Empty));
            Assert.Throws<ArgumentNullException>(() => conn.DropRole(string.Empty));

            //Create a new one
            Assert.DoesNotThrow(() =>conn.CreateRole(role1));
            var roles = conn.GetRoles();
            Assert.IsTrue(roles.Tables[0].Select(filter).Length == 1);

            //Create an existing one
            Assert.Throws<QueryInvalidException>(() =>conn.CreateRole(role1));            

            //Search         
            Assert.IsTrue(conn.ExistsRole(role1));            
            Assert.IsFalse(conn.ExistsRole(role2));   

            //Distroying existing roles
            Assert.DoesNotThrow(() => conn.DropRole(role1));
            roles = conn.GetRoles();
            Assert.IsTrue(roles.Tables[0].Select(filter).Length == 0);

            //Distroying non-existing role
            Assert.Throws<QueryInvalidException>(() => conn.DropRole("rolemanagement_role2"));                      
        }  

        [Test]
        [NonParallelizable()]
        public void CountRolesAndUsers(){  
            //Note: this test cannot run in parallel, because users and roles are shared along all the postgres instance.
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];

            //Vars for testing            
            var user = "permissionmanagement_user1";
            var role = "permissionmanagement_role1";

            var roles = conn.CountRoles();
            var users = conn.CountUsers();

            //Adding data
            Assert.DoesNotThrow(() => conn.CreateUser(user));
            Assert.DoesNotThrow(() => conn.CreateRole(role));
            Assert.AreEqual(roles+1, conn.CountRoles());
            Assert.AreEqual(users+1, conn.CountUsers());

            //Removing data
            Assert.DoesNotThrow(() => conn.DropUser(user));
            Assert.DoesNotThrow(() => conn.DropRole(role));
            Assert.AreEqual(roles, conn.CountRoles());
            Assert.AreEqual(users, conn.CountUsers());
        } 

        [Test]
        public void CompareSelects(){  
            //Note: this test cannot run in parallel, because users and roles are shared along all the postgres instance.
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];

            //Argument validation
            Assert.Throws<ArgumentNullException>(() => conn.CompareSelects(string.Empty, string.Empty));
            Assert.Throws<ArgumentNullException>(() => conn.CompareSelects(_FAKE, string.Empty));
            Assert.Throws<ArgumentNullException>(() => conn.CompareSelects(string.Empty, _FAKE));
            Assert.Throws<QueryInvalidException>(() => conn.CompareSelects(_FAKE, _FAKE));

            var query = "SELECT * FROM test.employees";
            Assert.IsTrue(conn.CompareSelects(query, query));
            Assert.IsTrue(conn.CompareSelects(string.Format("{0} WHERE 1=1", query), string.Format("{0} WHERE id_employee > 99", query)));
            Assert.IsTrue(conn.CompareSelects(string.Format("{0} WHERE id_employee=103", query), string.Format("{0} WHERE email='AHUNOLD'", query)));            
            Assert.IsFalse(conn.CompareSelects(string.Format("{0} WHERE id_employee=103", query), string.Format("{0} WHERE email='NKOCHHAR'", query)));
            Assert.IsFalse(conn.CompareSelects(string.Format("{0} WHERE 1=1", query), string.Format("{0} WHERE id_employee < 99", query)));
        }

        [Test]
        public void GetViewDefinition_ArgumentValidation(){  
            //Note: this test cannot run in parallel, because users and roles are shared along all the postgres instance.
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];

            //Argument validation
            foreach(var s in _emptySources)
                Assert.Throws<ArgumentNullException>(() => conn.GetViewDefinition(s.schema, s.table));
            

            foreach(var s in _wrongSources)
                Assert.DoesNotThrow(() => conn.GetViewDefinition(s.schema, s.table));                  
        }

        [Test]
        public void GetViewDefinition_Overloads(){  
            //Note: this test cannot run in parallel, because users and roles are shared along all the postgres instance.
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];

            //Testing
            string def = " SELECT employees.id_employee AS id,\n    employees.id_boss,\n    employees.name,\n    employees.surnames,\n    employees.email,\n    employees.phone\n   FROM test.employees\n  WHERE ((employees.id_work)::text = 'IT_PROG'::text);";
            Assert.AreEqual(def, conn.GetViewDefinition(_SCHEMA, "programmers"));
        }           

        [Test]
        public void GetForeignKeys_ArgumentValidation(){  
            //Note: this test cannot run in parallel, because users and roles are shared along all the postgres instance.
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];

            //Argument validation
            foreach(var s in _emptySources)
                Assert.Throws<ArgumentNullException>(() => conn.GetForeignKeys(s.schema, s.table));
            
            foreach(var s in _wrongSources)
                Assert.DoesNotThrow(() => conn.GetForeignKeys(s.schema, s.table));               
        }

        [Test]
        public void  GetForeignKeys_Overloads(){  
            //Note: this test cannot run in parallel, because users and roles are shared along all the postgres instance.
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];

            //Vars for testing
            var table = "countries";

            //Testing
            var foreign = conn.GetForeignKeys(_SCHEMA, table);
            Assert.AreEqual(1, foreign.Tables[0].Rows.Count);
            Assert.AreEqual("countries_regions_fk", foreign.Tables[0].Rows[0]["name"]);
            Assert.AreEqual(_SCHEMA, foreign.Tables[0].Rows[0]["schemaFrom"]);
            Assert.AreEqual(table, foreign.Tables[0].Rows[0]["tableFrom"]);
            Assert.AreEqual("id_region", foreign.Tables[0].Rows[0]["columnFrom"]);
            Assert.AreEqual(_SCHEMA, foreign.Tables[0].Rows[0]["schemaTo"]);
            Assert.AreEqual("regions", foreign.Tables[0].Rows[0]["tableTo"]);
            Assert.AreEqual("id_region", foreign.Tables[0].Rows[0]["columnTo"]);
        } 
    }
}