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
        private ConcurrentDictionary<string, AutoCheck.Core.Connectors.Postgres> Connectors = new ConcurrentDictionary<string, AutoCheck.Core.Connectors.Postgres>();                            

        private const string _HOST = "localhost";    
        
        private const string _ADMIN = "postgres";
        
        private const string _SCHEMA = "test";
        
        private AutoCheck.Core.Connectors.Postgres Connector {
            get{
                return Connectors[TestContext.CurrentContext.Test.ID];
            }
        }

        [SetUp]
        public void Setup() 
        {
            //Create a new and unique database for the current context (each test has its own database)
            var conn = new AutoCheck.Core.Connectors.Postgres(_HOST, string.Format("autocheck_{0}", TestContext.CurrentContext.Test.ID), _ADMIN, _ADMIN);
            conn.CreateDataBase(base.GetSampleFile("dump.sql"), true);

            //Storing the connector instance for the current context
            var added = false;
            do added = Connectors.TryAdd(TestContext.CurrentContext.Test.ID, conn);             
            while(!added);                
        }
       
        [TearDown]
        public override void TearDown(){
            Connector.DropDataBase();
            Connector.Dispose();
            base.TearDown();
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

            Connectors.Clear();
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
            Assert.DoesNotThrow(() => Connector.TestConnection());
        }

        [Test]
        public void ExistsDataBase_DoesNotThrow_True() 
        {            
            Assert.IsTrue(Connector.ExistsDataBase());                        
        }  

        [Test]
        [TestCase(_HOST, _FAKE, _ADMIN, _ADMIN)]
        public void ExistsDataBase_DoesNotThrow_False(string host, string database, string username, string password) 
        {            
            using(var conn = new AutoCheck.Core.Connectors.Postgres(host, database, username, password))
                Assert.IsFalse(conn.ExistsDataBase());
        }     

        [Test]
        [TestCase(null)]
        public void ExecuteQuery_Throws_ArgumentNullException(string query)
        {
            Assert.Throws<ArgumentNullException>(() => Connector.ExecuteQuery(query));
        } 

        [Test]
        [TestCase(_FAKE)]
        [TestCase("SELECT * FROM fake")]
        public void ExecuteQuery_Throws_QueryInvalidException(string query)
        {
            Assert.Throws<QueryInvalidException>(() => Connector.ExecuteQuery(query));
        } 

        [Test]
        [TestCase("SELECT * FROM test.departments", 9, 4, _SCHEMA, "departments", "name_department", "Administration")]
        [TestCase("SELECT name_department FROM test.departments WHERE id_department=60", 1, 1, _SCHEMA, "departments", "name_department", "IT")]
        public void ExecuteQuery_DoesNotThrow_READ(string query, int rowCount, int columnCount, string schema, string table, string scalarField, string scalarValue)
        {
            var ds = Connector.ExecuteQuery(query);
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
            Assert.DoesNotThrow(() => Connector.ExecuteQuery("INSERT INTO test.regions (id_region, name_region) VALUES ((SELECT MAX(id_region)+1 FROM test.regions), 'TEST')"));
            Assert.DoesNotThrow(() => Connector.ExecuteQuery("UPDATE test.regions SET name_region='TESTv2' WHERE id_region = (SELECT MAX(id_region) FROM test.regions)"));
            Assert.DoesNotThrow(() => Connector.ExecuteQuery("DELETE FROM test.regions WHERE id_region = (SELECT MAX(id_region) FROM test.regions)"));
        }

        [Test]
        [TestCase(null)]
        public void ExecuteNonQuery_Throws_ArgumentNullException(string query)
        {   
            Assert.Throws<ArgumentNullException>(() => Connector.ExecuteNonQuery(query));
        }

        [Test]
        [TestCase(_FAKE)]
        [TestCase("SELECT * FROM fake")]
        public void ExecuteNonQuery_Throws_QueryInvalidException(string query)
        {   
            Assert.Throws<QueryInvalidException>(() => Connector.ExecuteNonQuery(query));
        }

        [Test]
        [TestCase("SELECT * FROM test.departments")]
        [TestCase("SELECT name_department FROM test.departments WHERE id_department=60")]        
        public void ExecuteNonQuery_READ_DoesNotThrow(string query)
        {   
            Assert.DoesNotThrow(() => Connector.ExecuteNonQuery(query));
        }

        [Test]        
        public void ExecuteNonQuery_WRITE_DoesNotThrow()
        {   
            //Should be executed in order
            Assert.DoesNotThrow(() => Connector.ExecuteScalar<object>("INSERT INTO test.regions (id_region, name_region) VALUES ((SELECT MAX(id_region)+1 FROM test.regions), 'TEST')"));
            Assert.DoesNotThrow(() => Connector.ExecuteScalar<object>("UPDATE test.regions SET name_region='TESTv2' WHERE id_region = (SELECT MAX(id_region) FROM test.regions)"));
            Assert.DoesNotThrow(() => Connector.ExecuteScalar<object>("DELETE FROM test.regions WHERE id_region = (SELECT MAX(id_region) FROM test.regions)"));
        }

        [Test]
        [TestCase(null)]
        public void ExecuteScalar_Throws_ArgumentNullException(string query)
        {   
            Assert.Throws<ArgumentNullException>(() => Connector.ExecuteScalar<object>(query));
        }

        [Test]
        [TestCase(_FAKE)]
        [TestCase("SELECT * FROM fake")]
        public void ExecuteScalar_Throws_QueryInvalidException(string query)
        {   
            Assert.Throws<QueryInvalidException>(() => Connector.ExecuteScalar<object>(query));
        }

        [Test]
        [TestCase("SELECT * FROM test.departments", ExpectedResult=10)]
        [TestCase("SELECT name_department FROM test.departments WHERE id_department=60", ExpectedResult="IT")]
        [TestCase("INSERT INTO test.regions (id_region, name_region) VALUES ((SELECT MAX(id_region)+1 FROM test.regions), 'TEST') RETURNING id_region;", ExpectedResult=3)]                
        public object ExecuteScalar_READ_DoesNotThrow(string query)
        {   
            return Connector.ExecuteScalar<object>(query);
        }

        [Test]        
        public void ExecuteScalar_WRITE_DoesNotThrow()
        {   
            //Should be executed in order
            Assert.DoesNotThrow(() => Connector.ExecuteScalar<object>("INSERT INTO test.regions (id_region, name_region) VALUES ((SELECT MAX(id_region)+1 FROM test.regions), 'TEST')"));
            Assert.DoesNotThrow(() => Connector.ExecuteScalar<object>("UPDATE test.regions SET name_region='TESTv2' WHERE id_region = (SELECT MAX(id_region) FROM test.regions)"));
            Assert.DoesNotThrow(() => Connector.ExecuteScalar<object>("DELETE FROM test.regions WHERE id_region = (SELECT MAX(id_region) FROM test.regions)"));
        }   

        [Test]
        public void CreateUser_Throws_ArgumentNullException(){  
            Assert.Throws<ArgumentNullException>(() => Connector.CreateUser(string.Empty));
        }

        [Test]
        public void DropUser_Throws_ArgumentNullException(){  
            Assert.Throws<ArgumentNullException>(() => Connector.DropUser(string.Empty));
        }  

        [Test]
        [TestCase("createuser_user1")]
        [TestCase("createuser_user2")]
        public void CreateUser_Throws_QueryInvalidException(string user){  
            //Create
            Assert.IsFalse(Connector.ExistsUser(user)); 
            Assert.DoesNotThrow(() =>Connector.CreateUser(user));

            //Create an existing one
            Assert.Throws<QueryInvalidException>(() =>Connector.CreateUser(user)); 
        }

        [Test]
        [TestCase("createuser_user3", null)]
        [TestCase("createuser_user4", "PASS")]
        public void CreateUser_DoesNotThrow(string user, string password){
            Assert.IsFalse(Connector.ExistsUser(user)); 
            Assert.DoesNotThrow(() =>Connector.CreateUser(user, password));            
            Assert.IsTrue(Connector.ExistsUser(user)); 
        }

        [Test]
        [TestCase("existuser_user1", "existuser_user1", ExpectedResult=true)]
        [TestCase("existuser_user2", "existuser_user0", ExpectedResult=false)]
        public bool ExistsUser_DoesNotThrow(string user, string find){
            Assert.IsFalse(Connector.ExistsUser(user));
            Assert.DoesNotThrow(() =>Connector.CreateUser(user));            
            return Connector.ExistsUser(find);
        }

        [Test]
        [TestCase("dropuser_user1")]
        public void DropUser_DoesNotThrow(string user){
            Assert.IsFalse(Connector.ExistsUser(user));
            Assert.DoesNotThrow(() =>Connector.CreateUser(user));   
            Assert.IsTrue(Connector.ExistsUser(user));
            Assert.DoesNotThrow(() => Connector.DropUser(user));                   
            Assert.IsFalse(Connector.ExistsUser(user));
        }

        [Test]        
        [TestCase("dropuser_user2")]
        public void DropUser_Throws_QueryInvalidException(string user){
            Assert.IsFalse(Connector.ExistsUser(user));
            Assert.Throws<QueryInvalidException>(() =>Connector.DropUser(user)); 
        }
        
        [Test]
        [TestCase("existrole_role1", "existrole_role1", ExpectedResult=true)]
        [TestCase("existrole_role2", "existrole_role0", ExpectedResult=false)]
        public bool ExistsRole_DoesNotThrow(string role, string find){
            Assert.IsFalse(Connector.ExistsRole(role));
            Assert.DoesNotThrow(() =>Connector.CreateRole(role));            
            return Connector.ExistsRole(find);
        }

        [Test]
        public void CreateRole_Throws_ArgumentNullException(){    
            Assert.Throws<ArgumentNullException>(() => Connector.CreateRole(string.Empty));
        }

        [Test] 
        public void DropRole_Throws_ArgumentNullException(){    
            Assert.Throws<ArgumentNullException>(() => Connector.DropRole(string.Empty));
        }

        [Test]
        [TestCase("createrole_role1")]
        public void CreateNonExistingRole_DoesNotThrow(string role){    
            Assert.IsFalse(Connector.ExistsRole(role));
            Assert.DoesNotThrow(() =>Connector.CreateRole(role));
            Assert.IsTrue(Connector.ExistsRole(role));
        }

        [Test]
        [TestCase("createrole_role2")]
        public void CreateExistingRole_DoesNotThrow(string role){    
            Assert.IsFalse(Connector.ExistsRole(role));
            Assert.DoesNotThrow(() =>Connector.CreateRole(role));
            Assert.Throws<QueryInvalidException>(() =>Connector.CreateRole(role));    
        }

        [Test]
        [TestCase("droprole_role1")]
        public void DropExistingRole_DoesNotThrow(string role){    
            Assert.IsFalse(Connector.ExistsRole(role));
            Assert.DoesNotThrow(() =>Connector.CreateRole(role));
            Assert.IsTrue(Connector.ExistsRole(role));
            Assert.DoesNotThrow(() => Connector.DropRole(role));
            Assert.IsFalse(Connector.ExistsRole(role));
        }

        [Test]
        [TestCase("droprole_role2")]
        public void DropNonExistingRole_DoesNotThrow(string role){    
            Assert.IsFalse(Connector.ExistsRole(role));
            Assert.Throws<QueryInvalidException>(() => Connector.DropRole(role));
        }

        [Test]
        [TestCase("", "")]
        [TestCase(_FAKE, "")]
        [TestCase("", _FAKE)]
        public void CompareSelects_Throws_ArgumentNullException(string expected, string compared){  
            Assert.Throws<ArgumentNullException>(() => Connector.CompareSelects(expected, compared));
        }

        [Test]        
        [TestCase(_FAKE, _FAKE)]
        public void CompareSelects_Throws_QueryInvalidException(string expected, string compared){  
            Assert.Throws<QueryInvalidException>(() => Connector.CompareSelects(expected, compared));
        }

        [Test]        
        [TestCase("SELECT * FROM test.employees", "SELECT * FROM test.employees", ExpectedResult = true)]
        [TestCase("SELECT * FROM test.employees WHERE 1=1", "SELECT * FROM test.employees WHERE id_employee > 99", ExpectedResult = true)]
        [TestCase("SELECT * FROM test.employees WHERE id_employee=103", "SELECT * FROM test.employees WHERE email='AHUNOLD'", ExpectedResult = true)]
        [TestCase("SELECT * FROM test.employees WHERE id_employee=103", "SELECT * FROM test.employees WHERE email='NKOCHHAR'", ExpectedResult = false)]
        [TestCase("SELECT * FROM test.employees WHERE 1=1", "SELECT * FROM test.employees WHERE id_employee < 99", ExpectedResult = false)]
        public bool CompareSelects_DowsNotThrow(string expected, string compared){  
            return Connector.CompareSelects(expected, compared);
        }

        [Test]
        [TestCase(null, null)]
        [TestCase("", "")]
        [TestCase(_FAKE, "")]
        [TestCase("", _FAKE)]
        [TestCase(_FAKE, null)]
        [TestCase(null, _FAKE)]        
        public void GetViewDefinition_Throws_ArgumentNullException(string schema, string table){  
            Assert.Throws<ArgumentNullException>(() => Connector.GetViewDefinition(schema, table));                             
        }

        [Test]
        [TestCase(_FAKE, _FAKE)]
        [TestCase(_SCHEMA, _FAKE)]
        public void GetViewDefinition_DowsNotThrow_ArgumentValidation(string schema, string table){  
            Assert.DoesNotThrow(() => Connector.GetViewDefinition(schema, table));         
        }

        [Test]
        [TestCase(_SCHEMA, "programmers", " SELECT employees.id_employee AS id,\n    employees.id_boss,\n    employees.name,\n    employees.surnames,\n    employees.email,\n    employees.phone\n   FROM test.employees\n  WHERE ((employees.id_work)::text = 'IT_PROG'::text);")]
        public void GetViewDefinition_DowsNotThrow_Overloads(string schema, string table, string definition){  
            Assert.AreEqual(definition, Connector.GetViewDefinition(schema, table));     
        }

        [Test]
        [TestCase(null, null)]
        [TestCase("", "")]
        [TestCase(_FAKE, "")]
        [TestCase("", _FAKE)]
        [TestCase(_FAKE, null)]
        [TestCase(null, _FAKE)]        
        public void GetForeignKeys_Throws_ArgumentNullException(string schema, string table){  
            Assert.Throws<ArgumentNullException>(() => Connector.GetForeignKeys(schema, table));
        }

        [Test]
        [TestCase(_FAKE, _FAKE)]
        [TestCase(_SCHEMA, _FAKE)]
        public void GetForeignKeys_DowsNotThrow_ArgumentValidation(string schema, string table){  
            Assert.DoesNotThrow(() => Connector.GetViewDefinition(schema, table));         
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
            var foreign = Connector.GetForeignKeys(schema, table);

            Assert.AreEqual(1, foreign.Tables[0].Rows.Count);
            Assert.AreEqual(value, foreign.Tables[0].Rows[0][field]);
        }
         
        [Test]
        [NonParallelizable()]
        public void CountRolesAndUsers(){  
            //Note: this test cannot run in parallel, because users and roles are shared along all the postgres instance.

            //Vars for testing            
            var user = "permissionmanagement_user1";
            var role = "permissionmanagement_role1";

            var roles = Connector.CountRoles();
            var users = Connector.CountUsers();

            //Adding data
            Assert.DoesNotThrow(() => Connector.CreateUser(user));
            Assert.DoesNotThrow(() => Connector.CreateRole(role));
            Assert.AreEqual(roles+1, Connector.CountRoles());
            Assert.AreEqual(users+1, Connector.CountUsers());

            //Removing data
            Assert.DoesNotThrow(() => Connector.DropUser(user));
            Assert.DoesNotThrow(() => Connector.DropRole(role));
            Assert.AreEqual(roles, Connector.CountRoles());
            Assert.AreEqual(users, Connector.CountUsers());
        } 
    }
}