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
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.Concurrent;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using AutoCheck.Core;
using AutoCheck.Exceptions;
using Source = AutoCheck.Connectors.Postgres.Source;
using Destination = AutoCheck.Connectors.Postgres.Destination;
using Filter = AutoCheck.Connectors.Postgres.Filter;

namespace AutoCheck.Test.Connectors
{    
    [Parallelizable(ParallelScope.All)]   
    public class Postgres : Core.Test
    {   
        /// <summary>
        /// The connector instance is created here because a new one-time use BBDD will be created on every startup, and dropped when done.
        /// </summary>
        private ConcurrentDictionary<string, AutoCheck.Connectors.Postgres> Pool = new ConcurrentDictionary<string, AutoCheck.Connectors.Postgres>();
        
        private readonly Source[] _emptySources = new Source[]{
            //Should throw null argument exception when using on a method
            null,
            new Source(string.Empty, string.Empty),
            new Source(_FAKE, string.Empty),
            new Source(string.Empty, _FAKE),
            new Source(null, null),
            new Source(_FAKE, null),
            new Source(null, _FAKE)
        };

        private readonly Destination[] _emptyDestinations = new Destination[]{
            //Should throw null argument exception when using on a method
            null,
            new Destination(string.Empty, string.Empty),
            new Destination(_FAKE, string.Empty),
            new Destination(string.Empty, _FAKE),
            new Destination(null, null),
            new Destination(_FAKE, null),
            new Destination(null, _FAKE)
        };

        private readonly Filter[] _emptyFilters = new Filter[]{
            //Should throw null argument exception when using on a method
            null,
            new Filter(null, Operator.EQUALS,null),
            new Filter(_FAKE, Operator.EQUALS, null),
            new Filter(null, Operator.EQUALS, _FAKE),
            new Filter(string.Empty, Operator.EQUALS, string.Empty),
            new Filter(_FAKE, Operator.EQUALS, string.Empty),
            new Filter(string.Empty, Operator.EQUALS, _FAKE)
        };

        private readonly Dictionary<string, object>[] _emptyFields = new Dictionary<string, object>[]{
            //Should throw null argument exception when using on a method
            new Dictionary<string, object>(){}
        };

        private readonly Source[] _wrongSources = new Source[]{
            //Should throw invalid query exception when using on a method
            new Source(_FAKE, _FAKE),
            new Source(_SCHEMA, _FAKE)            
        };

        private readonly Destination[] _wrongDestinations = new Destination[]{
            //Should throw invalid query exception when using on a method
            new Destination(_FAKE, _FAKE),
            new Destination(_SCHEMA, _FAKE)
        };

        private readonly Filter[] _wrongFilters = new Filter[]{
            //Should throw null argument exception when using on a method            
            new Filter(_FAKE, Operator.EQUALS, _FAKE),
            new Filter(_FAKE, Operator.GREATER, 0),
            new Filter(_FAKE, Operator.GREATEREQUALS, 5),
            new Filter(_FAKE, Operator.LOWER, 1.5f),
            new Filter(_FAKE, Operator.LOWEREQUALS, -1.5f),
            new Filter(_FAKE, Operator.LIKE, 'a'),
            new Filter(_FAKE, Operator.NOTEQUALS, true)
        };

        private readonly Dictionary<string, object>[] _wrongFields = new Dictionary<string, object>[]{
            //Should throw null argument exception when using on a method
            new Dictionary<string, object>(){{_FAKE, null}},
            new Dictionary<string, object>(){{_FAKE, string.Empty}},            
            new Dictionary<string, object>(){{_FAKE, _FAKE}},            
            new Dictionary<string, object>(){{_FAKE, true}},
            new Dictionary<string, object>(){{_FAKE, 1}},
            new Dictionary<string, object>(){{_FAKE, -1}},
            new Dictionary<string, object>(){{_FAKE, 15.27f}},
            new Dictionary<string, object>(){{_FAKE, 'a'}}
        };

        private const string _HOST = "localhost";    
        
        private const string _ADMIN = "postgres";
        
        private const string _SCHEMA = "test";
               

        [SetUp]
        public void Setup() 
        {
            //Create a new and unique database for the current context (each test has its own context)
            var conn = new AutoCheck.Connectors.Postgres(_HOST, string.Format("autocheck_{0}", TestContext.CurrentContext.Test.ID), _ADMIN, _ADMIN);
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

        private void CleanUp(){
            using(var conn = new AutoCheck.Connectors.Postgres(_HOST, string.Format("autocheck_{0}", TestContext.CurrentContext.Test.ID), _ADMIN, _ADMIN)){
                try{ conn.ExecuteNonQuery(string.Format("DROP USER {0}", "usermanagement_user1")); } catch{}
                try{ conn.ExecuteNonQuery(string.Format("DROP USER {0}", "usermanagement_user2")); } catch{}                       
                try{ conn.ExecuteNonQuery(string.Format("DROP USER {0}", "rolemanagement_role1")); } catch{}                   
                
                try{ conn.ExecuteNonQuery(string.Format("DROP OWNED BY {0}", "permissionmanagement_role1")); } catch{}
                try{ conn.ExecuteNonQuery(string.Format("DROP USER {0}", "permissionmanagement_role1")); } catch{}
            }
        }

        [Test]
        public void Constructor()
        {                                            
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Connectors.Postgres("", "", ""));
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Connectors.Postgres(_FAKE, "", ""));
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Connectors.Postgres(_FAKE, _FAKE, ""));  
            Assert.DoesNotThrow(() => new AutoCheck.Connectors.Postgres(_HOST, _FAKE, _ADMIN, _ADMIN));         
        }

        [Test]
        public void TestConnection()
        {                    
            Assert.Throws<ConnectionInvalidException>(() => new AutoCheck.Connectors.Postgres(_FAKE,_FAKE, _FAKE).TestConnection());
            Assert.Throws<ConnectionInvalidException>(() => new AutoCheck.Connectors.Postgres(_HOST, _FAKE, _FAKE).TestConnection());
            Assert.Throws<ConnectionInvalidException>(() => new AutoCheck.Connectors.Postgres(_HOST, _FAKE, _ADMIN).TestConnection());
            Assert.Throws<ConnectionInvalidException>(() => new AutoCheck.Connectors.Postgres(_HOST, "autocheck", _FAKE).TestConnection());            
            Assert.DoesNotThrow(() => this.Pool[TestContext.CurrentContext.Test.ID].TestConnection());
        }

        [Test]
        public void ExistsDataBase() 
        {            
            Assert.IsTrue(this.Pool[TestContext.CurrentContext.Test.ID].ExistsDataBase());            
            using(var conn =  new AutoCheck.Connectors.Postgres(_HOST, _FAKE, _ADMIN, _ADMIN))
                Assert.IsFalse(conn.ExistsDataBase());
        }       

        [Test]
        public void ExecuteQuery()
        {
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];

            //Argument validation       
            Assert.Throws<ArgumentNullException>(() => conn.ExecuteQuery(null));
            Assert.Throws<QueryInvalidException>(() => conn.ExecuteQuery(_FAKE));
            Assert.Throws<QueryInvalidException>(() => conn.ExecuteQuery("SELECT * FROM fake"));
            
            //SELECT with no filter
            var ds = conn.ExecuteQuery("SELECT * FROM test.departments");
            Assert.AreEqual(9, ds.Tables[0].Rows.Count);
            Assert.AreEqual(4, ds.Tables[0].Columns.Count);
            Assert.AreEqual(_SCHEMA, ds.Tables[0].Namespace);
            Assert.AreEqual("departments", ds.Tables[0].TableName);
                            
            //SELECT with filter
            ds = conn.ExecuteQuery("SELECT name_department FROM test.departments WHERE id_department=60");
            Assert.AreEqual(1, ds.Tables[0].Rows.Count);
            Assert.AreEqual(1, ds.Tables[0].Columns.Count);
            Assert.AreEqual("IT", ds.Tables[0].Rows[0]["name_department"]);            

            //INSERT + UPDATE + DELETE with filters and subqueries                   
            Assert.DoesNotThrow(() => ds = conn.ExecuteQuery("INSERT INTO test.regions (id_region, name_region) VALUES ((SELECT MAX(id_region)+1 FROM test.regions), 'TEST')"));
            Assert.DoesNotThrow(() => ds = conn.ExecuteQuery("UPDATE test.regions SET name_region='TESTv2' WHERE id_region = (SELECT MAX(id_region) FROM test.regions)"));
            Assert.DoesNotThrow(() => ds = conn.ExecuteQuery("DELETE FROM test.regions WHERE id_region = (SELECT MAX(id_region) FROM test.regions)"));                                      
        } 

        [Test]
        public void ExecuteNonQuery()
        {         
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];

            //Argument validation
            Assert.Throws<ArgumentNullException>(() => conn.ExecuteNonQuery(null));
            Assert.Throws<QueryInvalidException>(() => conn.ExecuteNonQuery(_FAKE));
            Assert.Throws<QueryInvalidException>(() => conn.ExecuteNonQuery("SELECT * FROM fake"));                

            //Queries
            Assert.DoesNotThrow(() => conn.ExecuteNonQuery("SELECT * FROM test.departments"));
            Assert.DoesNotThrow(() => conn.ExecuteNonQuery("SELECT name_department FROM test.departments WHERE id_department=60"));
            Assert.DoesNotThrow(() => conn.ExecuteNonQuery("INSERT INTO test.regions (id_region, name_region) VALUES ((SELECT MAX(id_region)+1 FROM test.regions), 'TEST')"));
            Assert.DoesNotThrow(() => conn.ExecuteNonQuery("UPDATE test.regions SET name_region='TESTv2' WHERE id_region = (SELECT MAX(id_region) FROM test.regions)"));
            Assert.DoesNotThrow(() => conn.ExecuteNonQuery("DELETE FROM test.regions WHERE id_region = (SELECT MAX(id_region) FROM test.regions)"));                
        } 

        [Test]
        public void ExecuteScalar()
        {          
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];

            //Argument validation
            Assert.Throws<ArgumentNullException>(() => conn.ExecuteScalar<object>(null));
            Assert.Throws<QueryInvalidException>(() => conn.ExecuteScalar<object>(_FAKE));
            Assert.Throws<QueryInvalidException>(() => conn.ExecuteScalar<object>("SELECT * FROM fake"));
            
            //Queries
            Assert.AreEqual(10, conn.ExecuteScalar<short>("SELECT * FROM test.departments"));
            Assert.AreEqual("IT", conn.ExecuteScalar<string>("SELECT name_department FROM test.departments WHERE id_department=60"));
            Assert.IsNull(conn.ExecuteScalar<object>("INSERT INTO test.regions (id_region, name_region) VALUES ((SELECT MAX(id_region)+1 FROM test.regions), 'TEST')"));
            Assert.IsNull(conn.ExecuteScalar<object>("UPDATE test.regions SET name_region='TESTv2' WHERE id_region = (SELECT MAX(id_region) FROM test.regions)"));
            Assert.IsNull(conn.ExecuteScalar<object>("DELETE FROM test.regions WHERE id_region = (SELECT MAX(id_region) FROM test.regions)"));
        }   

        [Test]
        public void SelectData_ArgumentValidation(){
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];

            //Argument validation      
            foreach(var s in _emptySources){
                Assert.Throws<ArgumentNullException>(() => conn.Select(s));  

                foreach(var f in _emptyFilters)
                    Assert.Throws<ArgumentNullException>(() => conn.Select(s, f));

                foreach(var f in _wrongFilters)
                    Assert.Throws<ArgumentNullException>(() => conn.Select(s, f));
            }

            foreach(var s in _wrongSources){
                Assert.Throws<QueryInvalidException>(() => conn.Select(s));  

                foreach(var f in _emptyFilters)
                    Assert.Throws<ArgumentNullException>(() => conn.Select(s, f));

                foreach(var f in _wrongFilters)
                    Assert.Throws<QueryInvalidException>(() => conn.Select(s, f));              
            } 
        }
        
        [Test]
        public void SelectData_TableNoFilters()
        {                    
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];

            //Vars for testing
            var table = "departments";
            var path = string.Format("{0}.{1}", _SCHEMA, table);
            var source = new Source(_SCHEMA, table);

            //Existing source + existing table + all fields            
            var ds = conn.Select(path, string.Empty, "*");
            Assert.AreEqual(9, ds.Tables[0].Rows.Count);
            Assert.AreEqual(4, ds.Tables[0].Columns.Count);

            ds = conn.Select(source);
            Assert.AreEqual(9, ds.Tables[0].Rows.Count);
            Assert.AreEqual(4, ds.Tables[0].Columns.Count);

            ds = conn.Select(source, "*");
            Assert.AreEqual(9, ds.Tables[0].Rows.Count);
            Assert.AreEqual(4, ds.Tables[0].Columns.Count);                

            //Existing schema + existing table + single existing field    
            ds = conn.Select(path, string.Empty, "id_department");
            Assert.AreEqual(9, ds.Tables[0].Rows.Count);
            Assert.AreEqual(1, ds.Tables[0].Columns.Count);

            ds = conn.Select(source, "id_department");
            Assert.AreEqual(9, ds.Tables[0].Rows.Count);
            Assert.AreEqual(1, ds.Tables[0].Columns.Count);                               

            //Existing schema + existing table + multiple existing fields              
            ds = conn.Select(path, string.Empty, "id_department, id_boss");
            Assert.AreEqual(9, ds.Tables[0].Rows.Count);
            Assert.AreEqual(2, ds.Tables[0].Columns.Count);

            ds = conn.Select(source, "id_department, id_boss");
            Assert.AreEqual(9, ds.Tables[0].Rows.Count);
            Assert.AreEqual(2, ds.Tables[0].Columns.Count);

            ds = conn.Select(source, new string[]{"id_department", "id_boss"});
            Assert.AreEqual(9, ds.Tables[0].Rows.Count);
            Assert.AreEqual(2, ds.Tables[0].Columns.Count);                

            //Existing schema + existing table + single non-existing field                
            Assert.Throws<QueryInvalidException>(() =>conn.Select(path, string.Empty, _FAKE));
            Assert.Throws<QueryInvalidException>(() =>conn.Select(source, _FAKE));                

            //Existing schema + existing table + multiple existing fields (with some non-existing)                
            Assert.Throws<QueryInvalidException>(() =>conn.Select(path, string.Empty, "id_department, fake"));
            Assert.Throws<QueryInvalidException>(() =>conn.Select(source, "id_department, fake"));                
            Assert.Throws<QueryInvalidException>(() =>conn.Select(source, new string[]{"id_department", _FAKE}));                                            
        }

        [Test]
        public void SelectData_TableSingleFilter()
        {           
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];

            //Vars for testing
            var table = "departments";
            var path = string.Format("{0}.{1}", _SCHEMA, table);
            var source = new Source(_SCHEMA, table);                 

            //Existing schema + existing table + multiple existing fields + filter by a single non-existing numeric field (equals)
            Assert.Throws<QueryInvalidException>(() => conn.Select(path, "fake=10", new string[]{"id_department", "id_boss"}));
            Assert.Throws<QueryInvalidException>(() => conn.Select(source, new Filter(_FAKE, Operator.EQUALS, 10), new string[]{"id_department", "id_boss"}));

            //Existing schema + existing table + multiple existing fields + filter by a single existing numeric field (equals)
            var ds = conn.Select(path, "id_department=10", new string[]{"id_department", "id_boss"});
            Assert.AreEqual(1, ds.Tables[0].Rows.Count);
            Assert.AreEqual(2, ds.Tables[0].Columns.Count);                

            ds = conn.Select(source, new Filter("id_department", Operator.EQUALS, 10), new string[]{"id_department", "id_boss"});
            Assert.AreEqual(1, ds.Tables[0].Rows.Count);
            Assert.AreEqual(2, ds.Tables[0].Columns.Count);

            ds = conn.Select(source, new Filter("id_department", Operator.EQUALS, 10), "id_department");
            Assert.AreEqual(1, ds.Tables[0].Rows.Count);
            Assert.AreEqual(1, ds.Tables[0].Columns.Count);

            //Existing schema + existing table + multiple existing fields + filter by a single existing numeric field (lower or equals)
            ds = conn.Select(path, "id_department<=10", new string[]{"id_department", "id_boss"});
            Assert.AreEqual(1, ds.Tables[0].Rows.Count);
            Assert.AreEqual(2, ds.Tables[0].Columns.Count);                

            ds = conn.Select(source, new Filter("id_department", Operator.LOWEREQUALS, 10), new string[]{"id_department", "id_boss"});
            Assert.AreEqual(1, ds.Tables[0].Rows.Count);
            Assert.AreEqual(2, ds.Tables[0].Columns.Count);

            //Existing schema + existing table + multiple existing fields + filter by a single existing numeric field (lower)
            ds = conn.Select(path, "id_department<10", new string[]{"id_department", "id_boss"});
            Assert.AreEqual(0, ds.Tables[0].Rows.Count);
            Assert.AreEqual(2, ds.Tables[0].Columns.Count);                

            ds = conn.Select(source, new Filter("id_department", Operator.LOWER, 10), new string[]{"id_department", "id_boss"});
            Assert.AreEqual(0, ds.Tables[0].Rows.Count);
            Assert.AreEqual(2, ds.Tables[0].Columns.Count);

            //Existing schema + existing table + multiple existing fields + filter by a single existing numeric field (higher or equals)
            ds = conn.Select(path, "id_department>=10", new string[]{"id_department", "id_boss"});
            Assert.AreEqual(9, ds.Tables[0].Rows.Count);
            Assert.AreEqual(2, ds.Tables[0].Columns.Count);                

            ds = conn.Select(source, new Filter("id_department", Operator.GREATEREQUALS, 10), new string[]{"id_department", "id_boss"});
            Assert.AreEqual(9, ds.Tables[0].Rows.Count);
            Assert.AreEqual(2, ds.Tables[0].Columns.Count);

            //Existing schema + existing table + multiple existing fields + filter by a single existing numeric field (higher)
            ds = conn.Select(path, "id_department>10", new string[]{"id_department", "id_boss"});
            Assert.AreEqual(8, ds.Tables[0].Rows.Count);
            Assert.AreEqual(2, ds.Tables[0].Columns.Count);                

            ds = conn.Select(source, new Filter("id_department", Operator.GREATER, 10), new string[]{"id_department", "id_boss"});
            Assert.AreEqual(8, ds.Tables[0].Rows.Count);
            Assert.AreEqual(2, ds.Tables[0].Columns.Count);

            //Existing schema + existing table + multiple existing fields + filter by a single existing text field (like)
            ds = conn.Select(path, "name_department LIKE '%ti%'", new string[]{"id_department", "id_boss"});
            Assert.AreEqual(5, ds.Tables[0].Rows.Count);
            Assert.AreEqual(2, ds.Tables[0].Columns.Count);

            ds = conn.Select(source, new Filter("name_department", Operator.LIKE, "ti"), new string[]{"id_department", "id_boss"});
            Assert.AreEqual(5, ds.Tables[0].Rows.Count);
            Assert.AreEqual(2, ds.Tables[0].Columns.Count);
        }

        [Test]
        public void SelectData_TableMultyFilters()
        {            
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];

            //Vars for testing
            var table = "departments";
            var path = string.Format("{0}.{1}", _SCHEMA, table);

            //Existing schema + existing table + multiple existing fields + filter by a wrong filter
            Assert.Throws<QueryInvalidException>(() => conn.Select(path, "fake=10 AND id_departament=10", new string[]{"id_department", "id_boss"}));

            //Existing schema + existing table + multiple existing fields + filter by a complex filter
            var ds = conn.Select(path, "id_department < 90 AND name_department LIKE '%ti%'", new string[]{"id_department", "id_boss"});
            Assert.AreEqual(2, ds.Tables[0].Rows.Count);
            Assert.AreEqual(2, ds.Tables[0].Columns.Count);

            ds = conn.Select(path, "id_department < 90 AND name_department LIKE '%ti%'", new string[]{"id_department", "id_boss"});
            Assert.AreEqual(2, ds.Tables[0].Rows.Count);
            Assert.AreEqual(2, ds.Tables[0].Columns.Count);
        }   

        [Test]
        public void GetField(){
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            
            //Vars for testing
            var table = "departments";
            var path = string.Format("{0}.{1}", _SCHEMA, table);
            var source = new Source(_SCHEMA, table);

            //Argument validation      
            foreach(var s in _emptySources){
                Assert.Throws<ArgumentNullException>(() => conn.GetField<object>(s, null));

                foreach(var f in _emptyFilters)
                    Assert.Throws<ArgumentNullException>(() => conn.GetField<object>(s, f, null));

                foreach(var f in _wrongFilters)
                    Assert.Throws<ArgumentNullException>(() => conn.GetField<object>(s, f, null));             
            }

            foreach(var s in _wrongSources){
                Assert.Throws<QueryInvalidException>(() => conn.GetField<object>(s, _FAKE));

                foreach(var f in _emptyFilters)
                    Assert.Throws<ArgumentNullException>(() => conn.GetField<object>(s, f, _FAKE));

                foreach(var f in _wrongFilters)
                    Assert.Throws<QueryInvalidException>(() => conn.GetField<object>(s, f, _FAKE));
            }                
                     
            //With filter
            Assert.AreEqual("IT", conn.GetField<string>(path, "id_department=60", "name_department"));
            Assert.AreEqual("IT", conn.GetField<string>(source, new Filter("id_department", Operator.EQUALS, 60), "name_department"));
            
            //With no filter and ordering                
            Assert.AreEqual(10, conn.GetField<short>(source, "id_department", ListSortDirection.Ascending));                
            Assert.AreEqual(190, conn.GetField<short>(source, "id_department", ListSortDirection.Descending));
        }

        [Test]
        public void Insert_ArgumentValidation(){
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];        

            //Argument validation                  
            foreach(var d in _emptyDestinations){
                foreach(var f in _emptyFields){
                    Assert.Throws<ArgumentNullException>(() => conn.Insert(d, f));   
                    Assert.Throws<ArgumentNullException>(() => conn.Insert<object>(d, string.Empty, f)); 
                }                

                foreach(var f in _wrongFields){
                    Assert.Throws<ArgumentNullException>(() => conn.Insert(d, f));   
                    Assert.Throws<ArgumentNullException>(() => conn.Insert<object>(d, string.Empty, f)); 
                }                
            }

            foreach(var d in _wrongDestinations){
                foreach(var f in _emptyFields){
                    Assert.Throws<ArgumentNullException>(() => conn.Insert(d, f));   
                    Assert.Throws<ArgumentNullException>(() => conn.Insert<object>(d, string.Empty, f)); 
                } 

                foreach(var f in _wrongFields){
                    Assert.Throws<QueryInvalidException>(() => conn.Insert(d, f));   
                    Assert.Throws<QueryInvalidException>(() => conn.Insert<object>(d, string.Empty, f)); 
                }                
            }
        }

        [Test]
        public void Insert_Overloads(){
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];        

            //Vars for testing
            var table = "regions";
            var path = string.Format("{0}.{1}", _SCHEMA, table);
            var source = new Source(_SCHEMA, table);    
            
            //Insert and compare (Insert<T>() is unsing Insert() and GetField() internally)
            var countQuery = string.Format("SELECT COUNT(*) FROM {0}", path);
            var selectQuery = "SELECT name_region FROM test.regions WHERE id_region={0}";
            var subQuery = string.Format("@(SELECT MAX(id_region)+1 FROM {0})", path);
            var total = conn.ExecuteScalar<long>(countQuery);
            total+=1;

            var id = conn.Insert<short>(path, "id_region", new Dictionary<string, object>(){{"id_region", subQuery}, {"name_region", _SCHEMA}});                            
            Assert.AreEqual(_SCHEMA, conn.ExecuteScalar<string>(string.Format(selectQuery, id)));            
            Assert.AreEqual(total, conn.ExecuteScalar<long>(countQuery));

            total+=1;
            id = conn.Insert<short>(new Destination(_SCHEMA, table), "id_region", new Dictionary<string, object>(){{"id_region", subQuery}, {"name_region", _SCHEMA}});                
            Assert.AreEqual(_SCHEMA, conn.ExecuteScalar<string>(string.Format(selectQuery, id)));
            Assert.AreEqual(total, conn.ExecuteScalar<long>(countQuery));
        }

        [Test]
        public void Update_ArgumentValidation(){
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];            

            //Argument validation      
            foreach(var d in _emptyDestinations){
                foreach(var fd in _emptyFields){
                    Assert.Throws<ArgumentNullException>(() => conn.Update(d, fd));
                    
                    foreach(var ft in _emptyFilters){
                        Assert.Throws<ArgumentNullException>(() => conn.Update(d, ft, fd));

                        foreach(var s in _emptySources)
                            Assert.Throws<ArgumentNullException>(() => conn.Update(d, s, ft, fd)); 

                        foreach(var s in _wrongSources)
                            Assert.Throws<ArgumentNullException>(() => conn.Update(d, s, ft, fd));                       
                    }

                    foreach(var ft in _wrongFilters){
                        Assert.Throws<ArgumentNullException>(() => conn.Update(d, ft, fd));

                        foreach(var s in _emptySources)
                            Assert.Throws<ArgumentNullException>(() => conn.Update(d, s, ft, fd)); 

                        foreach(var s in _wrongSources)
                            Assert.Throws<ArgumentNullException>(() => conn.Update(d, s, ft, fd));
                    }
                } 

                foreach(var fd in _wrongFields){
                    Assert.Throws<ArgumentNullException>(() => conn.Update(d, fd));

                    foreach(var ft in _emptyFilters){
                        Assert.Throws<ArgumentNullException>(() => conn.Update(d, ft, fd));

                        foreach(var s in _emptySources)
                            Assert.Throws<ArgumentNullException>(() => conn.Update(d, s, ft, fd)); 

                        foreach(var s in _wrongSources)
                            Assert.Throws<ArgumentNullException>(() => conn.Update(d, s, ft, fd));
                    }

                    foreach(var ft in _wrongFilters){
                        Assert.Throws<ArgumentNullException>(() => conn.Update(d, ft, fd));

                        foreach(var s in _emptySources)
                            Assert.Throws<ArgumentNullException>(() => conn.Update(d, s, ft, fd)); 

                        foreach(var s in _wrongSources)
                            Assert.Throws<ArgumentNullException>(() => conn.Update(d, s, ft, fd));
                    }
                }                
            }

            foreach(var d in _wrongDestinations){
                foreach(var fd in _emptyFields){
                    Assert.Throws<ArgumentNullException>(() => conn.Update(d, fd));

                    foreach(var ft in _emptyFilters){
                        Assert.Throws<ArgumentNullException>(() => conn.Update(d, ft, fd));

                        foreach(var s in _emptySources)
                            Assert.Throws<ArgumentNullException>(() => conn.Update(d, s, ft, fd)); 

                        foreach(var s in _wrongSources)
                            Assert.Throws<ArgumentNullException>(() => conn.Update(d, s, ft, fd));
                    }

                    foreach(var ft in _wrongFilters){
                        Assert.Throws<ArgumentNullException>(() => conn.Update(d, ft, fd));

                        foreach(var s in _emptySources)
                            Assert.Throws<ArgumentNullException>(() => conn.Update(d, s, ft, fd)); 

                        foreach(var s in _wrongSources)
                            Assert.Throws<ArgumentNullException>(() => conn.Update(d, s, ft, fd));
                    }
                }  

                foreach(var fd in _wrongFields){
                    Assert.Throws<QueryInvalidException>(() => conn.Update(d, fd));

                    foreach(var ft in _emptyFilters){
                        Assert.Throws<ArgumentNullException>(() => conn.Update(d, ft, fd));

                        foreach(var s in _emptySources)
                            Assert.Throws<ArgumentNullException>(() => conn.Update(d, s, ft, fd)); 

                        foreach(var s in _wrongSources)
                            Assert.Throws<ArgumentNullException>(() => conn.Update(d, s, ft, fd));
                    }

                    foreach(var ft in _wrongFilters){
                        Assert.Throws<QueryInvalidException>(() => conn.Update(d, ft, fd));

                        foreach(var s in _emptySources)
                            Assert.Throws<ArgumentNullException>(() => conn.Update(d, s, ft, fd)); 

                        foreach(var s in _wrongSources)
                            Assert.Throws<QueryInvalidException>(() => conn.Update(d, s, ft, fd));
                    }
                }                
            }                           
        }

        [Test]
        public void Update_Overloads(){
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];                                           
            
            //Update with no filter
            string query = "SELECT COUNT(*) FROM test.regions";
            var total = conn.ExecuteScalar<long>(query);
            Assert.DoesNotThrow(() => conn.Update(new Destination(_SCHEMA, "regions"), new Dictionary<string, object>(){{"name_region", "TESTv1"}}));
            
            query += " WHERE name_region='{0}'";
            Assert.AreEqual(total, conn.ExecuteScalar<long>(string.Format(query, "TESTv1")));

            //Update using filter
            Assert.DoesNotThrow(() => conn.Update(new Destination(_SCHEMA, "regions"), new Filter("id_region", Operator.EQUALS, 1), new Dictionary<string, object>(){{"name_region", "TESTv2"}}));
            Assert.AreEqual(1, conn.ExecuteScalar<long>(string.Format(query, "TESTv1")));

            query = "SELECT COUNT(*) FROM test.employees WHERE name='{0}'";
            Assert.AreEqual(0, conn.ExecuteScalar<long>(string.Format(query, "TESTv1")));
            Assert.DoesNotThrow(() => conn.Update(new Destination(_SCHEMA, "employees e"), new Source(_SCHEMA, "departments d"), new Filter("d.id_boss", Operator.EQUALS, "@e.id_employee"), new Dictionary<string, object>(){{"name", "TESTv1"}}));
            Assert.AreEqual(7, conn.ExecuteScalar<long>(string.Format(query, "TESTv1")));

            //Update using source
            Assert.AreEqual(0, conn.ExecuteScalar<long>(string.Format(query, "TESTv2")));
            Assert.DoesNotThrow(() => conn.Update("test.employees e", "test.departments d", "d.id_boss=e.id_employee AND d.id_boss=103",  new Dictionary<string, object>(){{"name", "TESTv2"}}));
            Assert.AreEqual(1, conn.ExecuteScalar<long>(string.Format(query, "TESTv2")));            
        } 

        [Test]
        public void Delete_ArgumentValidation(){
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];            

            //Argument validation      
            foreach(var d in _emptyDestinations){
                Assert.Throws<ArgumentNullException>(() => conn.Delete(d));

                foreach(var ft in _emptyFilters){
                    Assert.Throws<ArgumentNullException>(() => conn.Delete(d, ft));

                    foreach(var s in _emptySources)
                        Assert.Throws<ArgumentNullException>(() => conn.Delete(d, s, ft)); 

                    foreach(var s in _wrongSources)
                        Assert.Throws<ArgumentNullException>(() => conn.Delete(d, s, ft));                       
                }

                foreach(var ft in _wrongFilters){
                    Assert.Throws<ArgumentNullException>(() => conn.Delete(d, ft));

                    foreach(var s in _emptySources)
                        Assert.Throws<ArgumentNullException>(() => conn.Delete(d, s, ft)); 

                    foreach(var s in _wrongSources)
                        Assert.Throws<ArgumentNullException>(() => conn.Delete(d, s, ft));
                }               
            }

            foreach(var d in _wrongDestinations){               
                Assert.Throws<QueryInvalidException>(() => conn.Delete(d));

                foreach(var ft in _emptyFilters){
                    Assert.Throws<ArgumentNullException>(() => conn.Delete(d, ft));

                    foreach(var s in _emptySources)
                        Assert.Throws<ArgumentNullException>(() => conn.Delete(d, s, ft)); 

                    foreach(var s in _wrongSources)
                        Assert.Throws<ArgumentNullException>(() => conn.Delete(d, s, ft));
                }

                foreach(var ft in _wrongFilters){
                    Assert.Throws<QueryInvalidException>(() => conn.Delete(d, ft));

                    foreach(var s in _emptySources)
                        Assert.Throws<ArgumentNullException>(() => conn.Delete(d, s, ft)); 

                    foreach(var s in _wrongSources)
                        Assert.Throws<QueryInvalidException>(() => conn.Delete(d, s, ft));
                }                                 
            }                           
        }

        [Test]
        public void Delete_Overloads(){
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
                 
            var query = "SELECT COUNT(*) FROM test.work_history";
            var total = conn.ExecuteScalar<long>(query);

            Assert.DoesNotThrow(() => conn.Delete(new Destination(_SCHEMA, "work_history"), new Filter("id_department", Operator.EQUALS, 110)));            
            Assert.AreEqual(total-2, conn.ExecuteScalar<long>(query));

            Assert.DoesNotThrow(() => conn.Delete("test.work_history wh", "test.work w", string.Format("wh.id_work=w.id_work AND wh.id_department={0}", 60)));
            Assert.AreEqual(total-3, conn.ExecuteScalar<long>(query));

            Assert.DoesNotThrow(() => conn.Delete(new Destination(_SCHEMA, "work_history wh"), new Source(_SCHEMA, "work w"), new Filter("wh.id_work", Operator.EQUALS, "@w.id_work")));
            Assert.AreEqual(0, conn.ExecuteScalar<long>(query));

            Assert.DoesNotThrow(() => conn.Delete(new Destination(_SCHEMA, "categories")));
            Assert.AreEqual(0, conn.ExecuteScalar<long>(query));      
        } 

        [Test]
        public void CountRegisters_ArgumentValidation(){
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];                  
            
            //Argument validation      
            foreach(var s in _emptySources){
                Assert.Throws<ArgumentNullException>(() => conn.CountRegisters(s));

                foreach(var f in _emptyFilters)
                    Assert.Throws<ArgumentNullException>(() => conn.CountRegisters(s, f));

                foreach(var f in _wrongFilters)
                   Assert.Throws<ArgumentNullException>(() => conn.CountRegisters(s, f));
            }

            foreach(var s in _wrongSources){
                Assert.Throws<QueryInvalidException>(() => conn.CountRegisters(s));

                foreach(var f in _emptyFilters)
                    Assert.Throws<ArgumentNullException>(() => conn.CountRegisters(s, f));

                foreach(var f in _wrongFilters)
                   Assert.Throws<QueryInvalidException>(() => conn.CountRegisters(s, f));
            }                       
        }

        [Test]
        public void CountRegisters_Overloads(){
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
            
            //Vars for testing            
            var table = "work_history";
            var path = string.Format("{0}.{1}", _SCHEMA, table);                    
            
            //With no filter
            Assert.AreEqual(10, conn.CountRegisters(path));
            Assert.AreEqual(10, conn.CountRegisters(new Destination(_SCHEMA, table)));
            
            //With filter
            Assert.AreEqual(2, conn.CountRegisters(path, "id_employee=101"));
            Assert.AreEqual(1, conn.CountRegisters(string.Format("{0} wh LEFT JOIN test.work w ON w.id_work=wh.id_work", path), "wh.id_work='AD_ASST'"));
            Assert.AreEqual(0, conn.CountRegisters(string.Format("{0} wh LEFT JOIN test.work w ON w.id_work=wh.id_work", path), "wh.id_work='AD_PRES'"));
            Assert.AreEqual(2, conn.CountRegisters(new Destination(_SCHEMA, table), new Filter("id_employee", Operator.EQUALS, 101)));                        
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
        public void PermissionManagement_ArgumentValidation(){  
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];
          
            //Argument validation
            //Argument validation      
            foreach(var s in _emptyDestinations){
                Assert.Throws<ArgumentNullException>(() => conn.Grant(null, s, null));
                Assert.Throws<ArgumentNullException>(() => conn.Grant(string.Empty, s, null));
                Assert.Throws<ArgumentNullException>(() => conn.Grant(null, s, string.Empty));
                Assert.Throws<ArgumentNullException>(() => conn.Grant(string.Empty, s, string.Empty));

                Assert.Throws<ArgumentNullException>(() => conn.Revoke(null, s, null));      
                Assert.Throws<ArgumentNullException>(() => conn.Revoke(string.Empty, s, null));
                Assert.Throws<ArgumentNullException>(() => conn.Revoke(null, s, string.Empty));
                Assert.Throws<ArgumentNullException>(() => conn.Revoke(string.Empty, s, string.Empty));  

                Assert.Throws<ArgumentNullException>(() => conn.GetTablePrivileges(s));

            }

            foreach(var s in _wrongDestinations){
                Assert.Throws<QueryInvalidException>(() => conn.Grant(_FAKE, s, _FAKE));
                Assert.Throws<QueryInvalidException>(() => conn.Revoke(_FAKE, s, _FAKE));
                
                Assert.DoesNotThrow(() => conn.GetTablePrivileges(s));
                Assert.DoesNotThrow(() => conn.GetTablePrivileges(s, _FAKE));
            }

            Assert.Throws<ArgumentNullException>(() => conn.GetMembership(null));
            Assert.Throws<ArgumentNullException>(() => conn.GetMembership(string.Empty));
            
            Assert.Throws<ArgumentNullException>(() => conn.GetSchemaPrivileges(string.Empty));                  
            Assert.Throws<ArgumentNullException>(() => conn.GetSchemaPrivileges(string.Empty, string.Empty));                       
            Assert.Throws<ArgumentNullException>(() => conn.GetSchemaPrivileges(string.Empty, _FAKE));                                      
            
            Assert.DoesNotThrow(() => conn.GetSchemaPrivileges(_FAKE));
            Assert.DoesNotThrow(() => conn.GetSchemaPrivileges(_FAKE, string.Empty));             
        }

        [Test]
        public void PermissionManagement_Overloads(){  
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];

            //Vars for testing
            var table = "categories";
            var destination = new Destination(_SCHEMA, table);
			var empty = new Destination(string.Empty, string.Empty);
            var role = "permissionmanagement_role1";                        

            //Grant an existing one
            var info = conn.GetMembership(_ADMIN);            
            Assert.AreEqual(0, info.Tables[0].Rows.Count);
            Assert.DoesNotThrow(() =>conn.CreateRole(role));
            Assert.DoesNotThrow(() => conn.Grant(role, _ADMIN));

            info = conn.GetMembership(_ADMIN);
            Assert.AreEqual(1, info.Tables[0].Rows.Count); 
            Assert.AreEqual(_ADMIN, info.Tables[0].Rows[0]["rolname"]); 
            Assert.AreEqual(role, info.Tables[0].Rows[0]["memberOf"]);

            //Grant a non-existing one (and already granted)            
            Assert.DoesNotThrow(() => conn.GetMembership(_FAKE));
            Assert.DoesNotThrow(() => conn.Grant(role, _ADMIN));
            Assert.Throws<QueryInvalidException>(() => conn.Grant(_FAKE, _ADMIN));
            Assert.Throws<QueryInvalidException>(() => conn.Grant(role, _FAKE));             

            //Revoke an existing one
            Assert.DoesNotThrow(() => conn.Revoke(role, _ADMIN));
            info = conn.GetMembership(_ADMIN);
            Assert.AreEqual(0, info.Tables[0].Rows.Count); 

            //Revoke a non-existing one (and already revoked)
            Assert.Throws<QueryInvalidException>(() => conn.Revoke(_FAKE, _ADMIN));
            Assert.Throws<QueryInvalidException>(() => conn.Revoke(role, _FAKE));
            Assert.DoesNotThrow(() => conn.Revoke(role, _ADMIN));            

            //Table permissions
            string permission = "INSERT";
            string filter = string.Format("privilege='{0}' AND grantee='{1}'", permission, role);            

            Assert.DoesNotThrow(() => conn.Grant(permission, destination, role));
            var privs = conn.GetTablePrivileges(destination, role);            
            Assert.AreEqual(1, privs.Tables[0].Select(filter).Length);

            Assert.DoesNotThrow(() => conn.Revoke(permission, destination, role));
            privs = conn.GetTablePrivileges(destination, role);
            Assert.AreEqual(0, privs.Tables[0].Select(filter).Length);

            //Schema permissions
            permission = "USAGE";
            filter = string.Format("{0}=true AND grantee='{1}'", permission, role);

            Assert.DoesNotThrow(() => conn.Grant(permission, _SCHEMA, role));
            privs = conn.GetSchemaPrivileges(_SCHEMA, role);
            Assert.AreEqual(1, privs.Tables[0].Select(filter).Length);

            Assert.DoesNotThrow(() => conn.Revoke(permission, _SCHEMA, role));
            privs = conn.GetSchemaPrivileges(_SCHEMA, role);
            Assert.AreEqual(0, privs.Tables[0].Select(filter).Length);

            //Distroying existing roles
            Assert.DoesNotThrow(() => conn.DropRole(role));

            //Distroying non-existing role
            Assert.Throws<QueryInvalidException>(() => conn.DropRole("permissionmanagement_role2"));                      
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
                Assert.Throws<ArgumentNullException>(() => conn.GetViewDefinition(s));
            

            foreach(var s in _wrongSources)
                Assert.DoesNotThrow(() => conn.GetViewDefinition(s));                  
        }

        [Test]
        public void GetViewDefinition_Overloads(){  
            //Note: this test cannot run in parallel, because users and roles are shared along all the postgres instance.
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];

            //Vars for testing
            var view = "programmers";
            var path = string.Format("{0}.{1}", _SCHEMA, view);
            var source = new Source(_SCHEMA, view);

            //Testing
            string def = " SELECT employees.id_employee AS id,\n    employees.id_boss,\n    employees.name,\n    employees.surnames,\n    employees.email,\n    employees.phone\n   FROM test.employees\n  WHERE ((employees.id_work)::text = 'IT_PROG'::text);";
            Assert.AreEqual(def, conn.GetViewDefinition(source));
            Assert.AreEqual(def, conn.GetViewDefinition(path));
        }           

        [Test]
        public void GetForeignKeys_ArgumentValidation(){  
            //Note: this test cannot run in parallel, because users and roles are shared along all the postgres instance.
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];

            //Argument validation
            foreach(var s in _emptySources)
                Assert.Throws<ArgumentNullException>(() => conn.GetForeignKeys(s));
            

            foreach(var s in _wrongSources)
                Assert.DoesNotThrow(() => conn.GetForeignKeys(s));               
        }

        [Test]
        public void  GetForeignKeys_Overloads(){  
            //Note: this test cannot run in parallel, because users and roles are shared along all the postgres instance.
            var conn = this.Pool[TestContext.CurrentContext.Test.ID];

            //Vars for testing
            var table = "countries";
            var path = string.Format("{0}.{1}", _SCHEMA, table);
            var source = new Source(_SCHEMA, table);

            //Testing
            var foreign = conn.GetForeignKeys(source);
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