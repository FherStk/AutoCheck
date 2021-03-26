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
                foreach(string user in new string[]{
                        "createuser_user1", 
                        "createuser_user2", 
                        "createuser_user3", 
                        "createuser_user4", 
                        "existuser_user1", 
                        "existuser_user2",
                        "dropuser_user1",
                        "dropuser_user2",
                        "existrole_role1",
                        "existrole_role2",
                        "createrole_role1",
                        "createrole_role2",
                        "droprole_role1",
                        "droprole_role2"
                    }){
                    try{ conn.ExecuteNonQuery(string.Format("DROP USER {0}", user)); } catch{}
                }

                foreach(string user in new string[]{"permissionmanagement_role1", "permissionmanagement_user1"}){
                    try{ conn.ExecuteNonQuery(string.Format("DROP OWNED BY {0}", user)); } catch{}
                    try{ conn.ExecuteNonQuery(string.Format("DROP USER {0}", user)); } catch{}
                }
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
        [TestCase("SELECT * FROM test.departments", 9, 4, _SCHEMA, "departments", "name_department", "Administration")]
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
        public void ExecuteQuery_DoesNotThrow_UPDATE()
        {
            //Should be executed in order
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];                        
            Assert.DoesNotThrow(() => conn.ExecuteQuery("INSERT INTO test.regions (id_region, name_region) VALUES ((SELECT MAX(id_region)+1 FROM test.regions), 'TEST')"));
            Assert.DoesNotThrow(() => conn.ExecuteQuery("UPDATE test.regions SET name_region='TESTv2' WHERE id_region = (SELECT MAX(id_region) FROM test.regions)"));
            Assert.DoesNotThrow(() => conn.ExecuteQuery("DELETE FROM test.regions WHERE id_region = (SELECT MAX(id_region) FROM test.regions)"));
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
        public void ExecuteNonQuery_READ_DoesNotThrow(string query)
        {   
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.DoesNotThrow(() => conn.ExecuteNonQuery(query));
        }

        [Test]        
        public void ExecuteNonQuery_WRITE_DoesNotThrow()
        {   
            //Should be executed in order
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.DoesNotThrow(() => conn.ExecuteScalar<object>("INSERT INTO test.regions (id_region, name_region) VALUES ((SELECT MAX(id_region)+1 FROM test.regions), 'TEST')"));
            Assert.DoesNotThrow(() => conn.ExecuteScalar<object>("UPDATE test.regions SET name_region='TESTv2' WHERE id_region = (SELECT MAX(id_region) FROM test.regions)"));
            Assert.DoesNotThrow(() => conn.ExecuteScalar<object>("DELETE FROM test.regions WHERE id_region = (SELECT MAX(id_region) FROM test.regions)"));
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
        public object ExecuteScalar_READ_DoesNotThrow(string query)
        {   
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            return conn.ExecuteScalar<object>(query);
        }

        [Test]        
        public void ExecuteScalar_WRITE_DoesNotThrow()
        {   
            //Should be executed in order
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.DoesNotThrow(() => conn.ExecuteScalar<object>("INSERT INTO test.regions (id_region, name_region) VALUES ((SELECT MAX(id_region)+1 FROM test.regions), 'TEST')"));
            Assert.DoesNotThrow(() => conn.ExecuteScalar<object>("UPDATE test.regions SET name_region='TESTv2' WHERE id_region = (SELECT MAX(id_region) FROM test.regions)"));
            Assert.DoesNotThrow(() => conn.ExecuteScalar<object>("DELETE FROM test.regions WHERE id_region = (SELECT MAX(id_region) FROM test.regions)"));
        }   

        [Test]
        public void CreateUser_Throws_ArgumentNullException(){  
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<ArgumentNullException>(() => conn.CreateUser(string.Empty));
        }

        [Test]
        public void DropUser_Throws_ArgumentNullException(){  
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<ArgumentNullException>(() => conn.DropUser(string.Empty));
        }  

        [Test]
        [TestCase("createuser_user1")]
        [TestCase("createuser_user2")]
        public void CreateUser_Throws_QueryInvalidException(string user){  
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];

            //Create
            Assert.IsFalse(conn.ExistsUser(user)); 
            Assert.DoesNotThrow(() =>conn.CreateUser(user));

            //Create an existing one
            Assert.Throws<QueryInvalidException>(() =>conn.CreateUser(user)); 
        }

        [Test]
        [TestCase("createuser_user3", null)]
        [TestCase("createuser_user4", "PASS")]
        public void CreateUser_DoesNotThrow(string user, string password){
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];

            Assert.IsFalse(conn.ExistsUser(user)); 
            Assert.DoesNotThrow(() =>conn.CreateUser(user, password));            
            Assert.IsTrue(conn.ExistsUser(user)); 
        }

        [Test]
        [TestCase("existuser_user1", "existuser_user1", ExpectedResult=true)]
        [TestCase("existuser_user2", "existuser_user0", ExpectedResult=false)]
        public bool ExistsUser_DoesNotThrow(string user, string find){
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            
            Assert.IsFalse(conn.ExistsUser(user));
            Assert.DoesNotThrow(() =>conn.CreateUser(user));            
            return conn.ExistsUser(find);
        }

        [Test]
        [TestCase("dropuser_user1")]
        public void DropUser_DoesNotThrow(string user){
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            
            Assert.IsFalse(conn.ExistsUser(user));
            Assert.DoesNotThrow(() =>conn.CreateUser(user));   
            Assert.IsTrue(conn.ExistsUser(user));
            Assert.DoesNotThrow(() => conn.DropUser(user));                   
            Assert.IsFalse(conn.ExistsUser(user));
        }

        [Test]        
        [TestCase("dropuser_user2")]
        public void DropUser_Throws_QueryInvalidException(string user){
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            
            Assert.IsFalse(conn.ExistsUser(user));
            Assert.Throws<QueryInvalidException>(() =>conn.DropUser(user)); 
        }
        
        [Test]
        [TestCase("existrole_role1", "existrole_role1", ExpectedResult=true)]
        [TestCase("existrole_role2", "existrole_role0", ExpectedResult=false)]
        public bool ExistsRole_DoesNotThrow(string role, string find){
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            
            Assert.IsFalse(conn.ExistsRole(role));
            Assert.DoesNotThrow(() =>conn.CreateRole(role));            
            return conn.ExistsRole(find);
        }

        [Test]
        public void CreateRole_Throws_ArgumentNullException(){    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<ArgumentNullException>(() => conn.CreateRole(string.Empty));
        }

        [Test] 
        public void DropRole_Throws_ArgumentNullException(){    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];         
            Assert.Throws<ArgumentNullException>(() => conn.DropRole(string.Empty));
        }

        [Test]
        [TestCase("createrole_role1")]
        public void CreateNonExistingRole_DoesNotThrow(string role){    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];

            Assert.IsFalse(conn.ExistsRole(role));
            Assert.DoesNotThrow(() =>conn.CreateRole(role));
            Assert.IsTrue(conn.ExistsRole(role));
        }

        [Test]
        [TestCase("createrole_role2")]
        public void CreateExistingRole_DoesNotThrow(string role){    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            
            Assert.IsFalse(conn.ExistsRole(role));
            Assert.DoesNotThrow(() =>conn.CreateRole(role));
            Assert.Throws<QueryInvalidException>(() =>conn.CreateRole(role));    
        }

        [Test]
        [TestCase("droprole_role1")]
        public void DropExistingRole_DoesNotThrow(string role){    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];

            Assert.IsFalse(conn.ExistsRole(role));
            Assert.DoesNotThrow(() =>conn.CreateRole(role));
            Assert.IsTrue(conn.ExistsRole(role));
            Assert.DoesNotThrow(() => conn.DropRole(role));
            Assert.IsFalse(conn.ExistsRole(role));
        }

        [Test]
        [TestCase("droprole_role2")]
        public void DropNonExistingRole_DoesNotThrow(string role){    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];

            Assert.IsFalse(conn.ExistsRole(role));
            Assert.Throws<QueryInvalidException>(() => conn.DropRole(role));
        }

        [Test]
        [TestCase("", "")]
        [TestCase(_FAKE, "")]
        [TestCase("", _FAKE)]
        public void CompareSelects_Throws_ArgumentNullException(string expected, string compared){  
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<ArgumentNullException>(() => conn.CompareSelects(expected, compared));
        }

        [Test]        
        [TestCase(_FAKE, _FAKE)]
        public void CompareSelects_Throws_QueryInvalidException(string expected, string compared){  
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<QueryInvalidException>(() => conn.CompareSelects(expected, compared));
        }

        [Test]        
        [TestCase("SELECT * FROM test.employees", "SELECT * FROM test.employees", ExpectedResult = true)]
        [TestCase("SELECT * FROM test.employees WHERE 1=1", "SELECT * FROM test.employees WHERE id_employee > 99", ExpectedResult = true)]
        [TestCase("SELECT * FROM test.employees WHERE id_employee=103", "SELECT * FROM test.employees WHERE email='AHUNOLD'", ExpectedResult = true)]
        [TestCase("SELECT * FROM test.employees WHERE id_employee=103", "SELECT * FROM test.employees WHERE email='NKOCHHAR'", ExpectedResult = false)]
        [TestCase("SELECT * FROM test.employees WHERE 1=1", "SELECT * FROM test.employees WHERE id_employee < 99", ExpectedResult = false)]
        public bool CompareSelects_DowsNotThrow(string expected, string compared){  
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            return conn.CompareSelects(expected, compared);
        }

        [Test]
        [TestCase(null, null)]
        [TestCase("", "")]
        [TestCase(_FAKE, "")]
        [TestCase("", _FAKE)]
        [TestCase(_FAKE, null)]
        [TestCase(null, _FAKE)]        
        public void GetViewDefinition_Throws_ArgumentNullException(string schema, string table){  
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<ArgumentNullException>(() => conn.GetViewDefinition(schema, table));                             
        }

        [Test]
        [TestCase(_FAKE, _FAKE)]
        [TestCase(_SCHEMA, _FAKE)]
        public void GetViewDefinition_DowsNotThrow_ArgumentValidation(string schema, string table){  
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.DoesNotThrow(() => conn.GetViewDefinition(schema, table));         
        }

        [Test]
        [TestCase(_SCHEMA, "programmers", " SELECT employees.id_employee AS id,\n    employees.id_boss,\n    employees.name,\n    employees.surnames,\n    employees.email,\n    employees.phone\n   FROM test.employees\n  WHERE ((employees.id_work)::text = 'IT_PROG'::text);")]
        public void GetViewDefinition_DowsNotThrow_Overloads(string schema, string table, string definition){  
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.AreEqual(definition, conn.GetViewDefinition(schema, table));     
        }

        [Test]
        [TestCase(null, null)]
        [TestCase("", "")]
        [TestCase(_FAKE, "")]
        [TestCase("", _FAKE)]
        [TestCase(_FAKE, null)]
        [TestCase(null, _FAKE)]        
        public void GetForeignKeys_Throws_ArgumentNullException(string schema, string table){  
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.Throws<ArgumentNullException>(() => conn.GetForeignKeys(schema, table));
        }

        [Test]
        [TestCase(_FAKE, _FAKE)]
        [TestCase(_SCHEMA, _FAKE)]
        public void GetForeignKeys_DowsNotThrow_ArgumentValidation(string schema, string table){  
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            Assert.DoesNotThrow(() => conn.GetViewDefinition(schema, table));         
        }

        [Test]
        [TestCase(_SCHEMA, "countries", "name", "countries_regions_fk")]
        [TestCase(_SCHEMA, "countries", "schemaFrom", _SCHEMA)]
        [TestCase(_SCHEMA, "countries", "tableFrom", "countries")]
        [TestCase(_SCHEMA, "countries", "columnFrom", "id_region")]
        [TestCase(_SCHEMA, "countries", "schemaTo", _SCHEMA)]
        [TestCase(_SCHEMA, "countries", "tableTo", "regions")]
        [TestCase(_SCHEMA, "countries", "columnTo", "id_region")]
        public void  GetForeignKeys_DowsNotThrow_Overloads(string schema, string table, string field, string value){  
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            var foreign = conn.GetForeignKeys(schema, table);

            Assert.AreEqual(1, foreign.Tables[0].Rows.Count);
            Assert.AreEqual(value, foreign.Tables[0].Rows[0][field]);
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
    }
}