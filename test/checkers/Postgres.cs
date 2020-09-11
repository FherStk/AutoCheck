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
using System.Collections.Generic;
using System.Collections.Concurrent;
using NUnit.Framework;
using AutoCheck.Core;
// using Source = AutoCheck.Connectors.Postgres.Source;
// using Destination = AutoCheck.Connectors.Postgres.Destination;
// using Filter = AutoCheck.Connectors.Postgres.Filter;

namespace AutoCheck.Test.Checkers
{    
    //TODO: Remove this file when migration from C# to YAML has been completed (this includes checker & connector mergind and also connector simplification).

    // [Parallelizable(ParallelScope.All)]  
    // public class Postgres : Core.Test
    // {   
    //     /// <summary>
    //     /// The connector instance is created here because a new one-time use BBDD will be created on every startup, and dropped when done.
    //     /// </summary>
    //     private ConcurrentDictionary<string, AutoCheck.Checkers.Postgres> Pool = new ConcurrentDictionary<string, AutoCheck.Checkers.Postgres>();

    //     private AutoCheck.Checkers.Postgres Chk = null;
        
    //     private readonly Source[] _emptySources = new Source[]{
    //         //Should throw null argument exception when using on a method
    //         null,
    //         new Source(string.Empty, string.Empty),
    //         new Source(_FAKE, string.Empty),
    //         new Source(string.Empty, _FAKE),
    //         new Source(null, null),
    //         new Source(_FAKE, null),
    //         new Source(null, _FAKE)
    //     };

    //     private readonly Destination[] _emptyDestinations = new Destination[]{
    //         //Should throw null argument exception when using on a method
    //         null,
    //         new Destination(string.Empty, string.Empty),
    //         new Destination(_FAKE, string.Empty),
    //         new Destination(string.Empty, _FAKE),
    //         new Destination(null, null),
    //         new Destination(_FAKE, null),
    //         new Destination(null, _FAKE)
    //     };

    //     private readonly Filter[] _emptyFilters = new Filter[]{
    //         //Should throw null argument exception when using on a method
    //         null,
    //         new Filter(null, Operator.EQUALS,null),
    //         new Filter(_FAKE, Operator.EQUALS, null),
    //         new Filter(null, Operator.EQUALS, _FAKE),
    //         new Filter(string.Empty, Operator.EQUALS, string.Empty),
    //         new Filter(_FAKE, Operator.EQUALS, string.Empty),
    //         new Filter(string.Empty, Operator.EQUALS, _FAKE)
    //     };

    //     private readonly Dictionary<string, object>[] _emptyFields = new Dictionary<string, object>[]{
    //         //Should throw null argument exception when using on a method
    //         new Dictionary<string, object>(){}
    //     };

    //     private readonly Source[] _wrongSources = new Source[]{
    //         //Should throw invalid query exception when using on a method
    //         new Source(_FAKE, _FAKE),
    //         new Source(_SCHEMA, _FAKE)            
    //     };

    //     private readonly Destination[] _wrongDestinations = new Destination[]{
    //         //Should throw invalid query exception when using on a method
    //         new Destination(_FAKE, _FAKE),
    //         new Destination(_SCHEMA, _FAKE)
    //     };

    //     private readonly Filter[] _wrongFilters = new Filter[]{
    //         //Should throw null argument exception when using on a method            
    //         new Filter(_FAKE, Operator.EQUALS, _FAKE),
    //         new Filter(_FAKE, Operator.GREATER, 0),
    //         new Filter(_FAKE, Operator.GREATEREQUALS, 5),
    //         new Filter(_FAKE, Operator.LOWER, 1.5f),
    //         new Filter(_FAKE, Operator.LOWEREQUALS, -1.5f),
    //         new Filter(_FAKE, Operator.LIKE, 'a'),
    //         new Filter(_FAKE, Operator.NOTEQUALS, true)
    //     };

    //     private readonly Dictionary<string, object>[] _wrongFields = new Dictionary<string, object>[]{
    //         //Should throw null argument exception when using on a method
    //         new Dictionary<string, object>(){{_FAKE, null}},
    //         new Dictionary<string, object>(){{_FAKE, string.Empty}},            
    //         new Dictionary<string, object>(){{_FAKE, _FAKE}},            
    //         new Dictionary<string, object>(){{_FAKE, true}},
    //         new Dictionary<string, object>(){{_FAKE, 1}},
    //         new Dictionary<string, object>(){{_FAKE, -1}},
    //         new Dictionary<string, object>(){{_FAKE, 15.27f}},
    //         new Dictionary<string, object>(){{_FAKE, 'a'}}
    //     };

    //     private const string _HOST = "localhost";
               
    //     private const string _ADMIN = "postgres";
        
    //     private const string _SCHEMA = "test";

    //     private const string _ROLE = "chk_pg_role";
        
    //     private const string _GROUP = "chk_pg_group";

    //     [OneTimeSetUp]
    //     public override void OneTimeSetUp() 
    //     {            
    //         //The same database (but different checker instance, to allow parallel queries) will be shared along the different tests, because all the opperations 
    //         //are read-only; this will boost the test performance because loading the Odoo database is a long time opperation.
    //         this.Chk =new AutoCheck.Checkers.Postgres(_HOST, string.Format("autocheck_{0}", TestContext.CurrentContext.Test.ID), _ADMIN, _ADMIN); 
    //         base.OneTimeSetUp(); //Because "Chk" is needed within "CleanUp"
            
    //         //Setup database and privileges
    //         this.Chk.Connector.CreateDataBase(base.GetSampleFile("dump.sql"));
    //         if(!this.Chk.Connector.ExistsRole(_ROLE)) this.Chk.Connector.CreateRole(_ROLE);
    //         if(!this.Chk.Connector.ExistsRole(_GROUP)) this.Chk.Connector.CreateRole(_GROUP);            
    //         this.Chk.Connector.Grant("SELECT, INSERT, UPDATE", "test.work", _ROLE);   
    //         this.Chk.Connector.Grant("USAGE", "test", _ROLE);  
    //         this.Chk.Connector.Grant(_GROUP, _ROLE);            
    //     }

    //     [OneTimeTearDown]
    //     public new void OneTimeTearDown(){     
    //         this.Pool.Clear(); 
    //     }

    //     protected override void CleanUp(){   
    //         if(this.Chk.Connector.ExistsDataBase())
    //             this.Chk.Connector.DropDataBase();            
    //     }

    //     [SetUp]
    //     public void Setup() 
    //     {            
    //         //Create a new and unique database connection for the current context (same DB for all tests)
    //         var chk = new AutoCheck.Checkers.Postgres(this.Chk.Host, this.Chk.Database, this.Chk.User, this.Chk.User);
                       
    //         //Storing the chkector instance for the current context
    //         var added = false;
    //         do added = this.Pool.TryAdd(TestContext.CurrentContext.Test.ID, chk);             
    //         while(!added);                          
    //     }

    //     [TearDown]
    //     public void TearDown(){
    //         var chk = this.Pool[TestContext.CurrentContext.Test.ID];
    //         chk.Dispose();
    //     }

    //     [Test]
    //     public void Constructor()
    //     {                                            
    //         Assert.Throws<ArgumentNullException>(() => new AutoCheck.Checkers.Postgres("", "", "", ""));
    //         Assert.Throws<ArgumentNullException>(() => new AutoCheck.Checkers.Postgres(_FAKE, "", "", ""));
    //         Assert.Throws<ArgumentNullException>(() => new AutoCheck.Checkers.Postgres(_FAKE, _FAKE, "", ""));  
    //         Assert.DoesNotThrow(() => new AutoCheck.Checkers.Postgres(_HOST, _FAKE, _ADMIN, _ADMIN));         
    //     }                    

    //     [Test]
    //     public void CheckIfTableMatchesPrivileges(){
    //         var chk = this.Pool[TestContext.CurrentContext.Test.ID];

    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableMatchesPrivileges("", "", "", ""));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableMatchesPrivileges(_FAKE, "", "", ""));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableMatchesPrivileges(_FAKE, _FAKE, "", ""));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableMatchesPrivileges(_FAKE, _FAKE, _FAKE, ""));

    //         Assert.AreEqual(new List<string>(), chk.CheckIfTableMatchesPrivileges(_ROLE, _SCHEMA, "work", "arw"));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableMatchesPrivileges(_ROLE, _SCHEMA, "work", "d"));            
    //     }

    //     [Test]
    //     public void CheckIfTableContainsPrivileges(){
    //         var chk = this.Pool[TestContext.CurrentContext.Test.ID];

    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableContainsPrivileges("", "", "", ' '));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableContainsPrivileges(_FAKE, "", "", ' '));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableContainsPrivileges(_FAKE, _FAKE, "", ' '));

    //         Assert.AreEqual(new List<string>(), chk.CheckIfTableContainsPrivileges(_ROLE, _SCHEMA, "work", 'r'));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableContainsPrivileges(_ROLE, _SCHEMA, "work", 'd'));            
    //     }

    //     [Test]
    //     public void CheckIfSchemaMatchesPrivileges(){
    //         var chk = this.Pool[TestContext.CurrentContext.Test.ID];

    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfSchemaMatchesPrivileges("", "", ""));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfSchemaMatchesPrivileges(_FAKE, "", ""));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfSchemaMatchesPrivileges(_FAKE, _FAKE, ""));

    //         Assert.AreEqual(new List<string>(), chk.CheckIfSchemaMatchesPrivileges(_ROLE, _SCHEMA, "U"));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfSchemaMatchesPrivileges(_ROLE, _SCHEMA, "C"));            
    //     }

    //     [Test]
    //     public void CheckIfSchemaContainsPrivilege(){
    //         var chk = this.Pool[TestContext.CurrentContext.Test.ID];

    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfSchemaContainsPrivilege("", "", ' '));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfSchemaContainsPrivilege(_FAKE, "", ' '));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfSchemaContainsPrivilege(_FAKE, _FAKE, ' '));

    //         Assert.AreEqual(new List<string>(), chk.CheckIfSchemaContainsPrivilege(_ROLE, _SCHEMA, 'U'));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfSchemaContainsPrivilege(_ROLE, _SCHEMA, 'C'));            
    //     }

    //     [Test]
    //     public void CheckRoleMembership(){
    //         var chk = this.Pool[TestContext.CurrentContext.Test.ID];

    //         Assert.AreNotEqual(new List<string>(), chk.CheckRoleMembership("", null));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckRoleMembership(_FAKE, null));            

    //         Assert.AreEqual(new List<string>(), chk.CheckRoleMembership(_ROLE, new string[]{_GROUP}));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckRoleMembership(_ADMIN, new string[]{_GROUP}));            
    //     }

    //     [Test]
    //     public void CheckForeignKey(){
    //         var chk = this.Pool[TestContext.CurrentContext.Test.ID];

    //         Assert.AreNotEqual(new List<string>(), chk.CheckForeignKey("", "", "", "", "", ""));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckForeignKey(_FAKE, "", "", "", "", ""));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckForeignKey(_FAKE, _FAKE, "", "", "", ""));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckForeignKey(_FAKE, _FAKE, _FAKE, "", "", ""));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckForeignKey(_FAKE, _FAKE, _FAKE, _FAKE, "", ""));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckForeignKey(_FAKE, _FAKE, _FAKE, _FAKE, _FAKE, ""));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckForeignKey(_FAKE, _FAKE, _FAKE, _FAKE, _FAKE, _FAKE));

    //         Assert.AreEqual(new List<string>(), chk.CheckForeignKey(_SCHEMA, "work_history", "id_work", _SCHEMA, "work", "id_work"));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckForeignKey(_SCHEMA, "work_history", "id_work", _SCHEMA, "work", _FAKE));
    //     }

    //     [Test]
    //     public void CheckIfEntryAdded(){
    //         var chk = this.Pool[TestContext.CurrentContext.Test.ID];

    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfEntryAdded("", "", "", 0));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfEntryAdded(_FAKE, "", "", 0));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfEntryAdded(_FAKE, _FAKE, "", 0));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfEntryAdded(_FAKE, _FAKE, _FAKE, 0));
            
    //         Assert.AreEqual(new List<string>(), chk.CheckIfEntryAdded(_SCHEMA, "employees", "id_employee", 0));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfEntryAdded(_SCHEMA, "employees", "id_employee", 205));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfEntryAdded(_SCHEMA, "employees", "id_employee", 206));
    //     }

    //     [Test]
    //     public void CheckIfEntryRemoved(){
    //         var chk = this.Pool[TestContext.CurrentContext.Test.ID];

    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfEntryRemoved("", "", "", 0));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfEntryRemoved(_FAKE, "", "", 0));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfEntryRemoved(_FAKE, _FAKE, "", 0));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfEntryRemoved(_FAKE, _FAKE, _FAKE, 0));
            
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfEntryRemoved(_SCHEMA, "employees", "id_employee", 206));
    //         Assert.AreEqual(new List<string>(), chk.CheckIfEntryRemoved(_SCHEMA, "employees", "id_employee", 207));
    //     }

    //     [Test]
    //     public void CheckIfTableContainsData(){
    //         var chk = this.Pool[TestContext.CurrentContext.Test.ID];

    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableContainsData("", "", null));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableContainsData(_FAKE, "", null));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableContainsData(_FAKE, _FAKE, null));
           
            
    //         Assert.AreEqual(new List<string>(), chk.CheckIfTableContainsData(_SCHEMA, "countries", new Dictionary<string, object>(){{"id_contry", "CA"}, {"name_country", "Canada"}, {"id_region", (short)2}}));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableContainsData(_SCHEMA, "countries", new Dictionary<string, object>(){{"id_contry", _FAKE}, {"name_country", "Canada"}, {"id_region", (short)2}}));
    //     }

    //     [Test]
    //     public void CheckIfSelectContainsData(){
    //         var chk = this.Pool[TestContext.CurrentContext.Test.ID];

    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfSelectContainsData("", null));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfSelectContainsData(_FAKE, null));           
            
    //         Assert.AreEqual(new List<string>(), chk.CheckIfSelectContainsData(string.Format("SELECT * FROM {0}.{1}", _SCHEMA, "countries"), new Dictionary<string, object>(){{"id_contry", "CA"}, {"name_country", "Canada"}, {"id_region", (short)2}}));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfSelectContainsData(string.Format("SELECT * FROM {0}.{1}", _SCHEMA, "countries"), new Dictionary<string, object>(){{"id_contry", _FAKE}, {"name_country", "Canada"}, {"id_region", (short)2}}));
    //     }

    //     [Test]
    //     public void CheckIfTableExists(){
    //         var chk = this.Pool[TestContext.CurrentContext.Test.ID];

    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableExists(null, null));            
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableExists(_FAKE, null));           
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableExists(_FAKE, _FAKE));
            
    //         Assert.AreEqual(new List<string>(), chk.CheckIfTableExists(_SCHEMA, "work"));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableExists(_FAKE, "work"));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableExists(_SCHEMA, _FAKE));
    //     }

    //     [Test]
    //     public void CheckIfViewMatchesDefinition(){
    //         var chk = this.Pool[TestContext.CurrentContext.Test.ID];

    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfViewMatchesDefinition(null, null, null));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfViewMatchesDefinition(_FAKE, null, null));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfViewMatchesDefinition(_FAKE, _FAKE, null));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfViewMatchesDefinition(_FAKE, _FAKE, _FAKE));
            
    //         Assert.AreEqual(new List<string>(), chk.CheckIfViewMatchesDefinition(_SCHEMA, "programmers", @"
    //             SELECT employees.id_employee AS id, employees.id_boss, employees.name, employees.surnames, employees.email, employees.phone
    //             FROM test.employees
    //             WHERE employees.id_work = 'IT_PROG';"
    //         ));

    //         //TODO: column names should be equals... not enough time to code it :(
    //         Assert.AreEqual(new List<string>(), chk.CheckIfViewMatchesDefinition(_SCHEMA, "programmers", @"
    //             SELECT employees.id_employee AS fake, employees.id_boss, employees.name, employees.surnames, employees.email, employees.phone
    //             FROM test.employees
    //             WHERE employees.id_work = 'IT_PROG';"
    //         ));

    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfViewMatchesDefinition(_SCHEMA, "programmers", @"
    //             SELECT employees.id_employee AS id, employees.id_boss, employees.name, employees.surnames, employees.email, employees.phone
    //             FROM test.employees
    //             WHERE employees.id_work = 'FAKE';"
    //         ));
        
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfViewMatchesDefinition(_SCHEMA, "programmers", @"
    //             SELECT employees.id_boss, employees.name, employees.surnames, employees.email, employees.phone
    //             FROM test.employees
    //             WHERE employees.id_work = 'IT_PROG';"
    //         ));

    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfViewMatchesDefinition(_SCHEMA, "programmers", @"
    //             SELECT *
    //             FROM work;"
    //         ));
    //     }

    //     [Test]
    //     public void CheckIfTableInsertsData(){
    //         var chk = this.Pool[TestContext.CurrentContext.Test.ID];
            
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableInsertsData(null, null, null));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableInsertsData(_FAKE, null, null));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableInsertsData(_FAKE, _FAKE, null));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableInsertsData(_FAKE, _FAKE, new Dictionary<string, object>(){{_FAKE, null}}));

    //         var table = "regions";
    //         var path = string.Format("{0}.{1}", _SCHEMA, table);
    //         var subQuery = string.Format("@(SELECT MAX(id_region)+1 FROM {0})", path);
    //         Assert.AreEqual(new List<string>(), chk.CheckIfTableInsertsData(_SCHEMA, table, new Dictionary<string, object>(){{"id_region", subQuery}, {"name_region", _FAKE}}));          
    //     }

    //     [Test]
    //     public void CheckIfTableUpdatesData(){
    //         var chk = this.Pool[TestContext.CurrentContext.Test.ID];
            
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableUpdatesData(null, null, null));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableUpdatesData(_FAKE, null, null));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableUpdatesData(_FAKE, _FAKE, null));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableUpdatesData(_FAKE, _FAKE, new Dictionary<string, object>(){{_FAKE, null}}));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableUpdatesData(_FAKE, _FAKE, _FAKE, _FAKE, Operator.EQUALS, new Dictionary<string, object>(){{_FAKE, null}}));

    //         var table = "regions";
    //         var path = string.Format("{0}.{1}", _SCHEMA, table);
    //         var subQuery = string.Format("@(SELECT MAX(id_region)+1 FROM {0})", path);
    //         Assert.AreEqual(new List<string>(), chk.CheckIfTableUpdatesData(_SCHEMA, table, "id_region", (short)1, new Dictionary<string, object>(){{"name_region", _FAKE}}));          
    //     }

    //     [Test]
    //     public void CheckIfTableDeletesData(){
    //         var chk = this.Pool[TestContext.CurrentContext.Test.ID];
            
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableDeletesData(null, null));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableDeletesData(_FAKE, null));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableDeletesData(_FAKE, _FAKE));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableDeletesData(_FAKE, _FAKE, _FAKE, Operator.EQUALS));
            
    //         Assert.AreEqual(new List<string>(), chk.CheckIfTableDeletesData(_SCHEMA, "work_history", "id_employee", (short)101));          
    //     }

    //     [Test]
    //     public void CheckIfTableMatchesAmountOfRegisters(){
    //         var chk = this.Pool[TestContext.CurrentContext.Test.ID];
            
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableMatchesAmountOfRegisters(null, null, 0));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableMatchesAmountOfRegisters(_FAKE, null, 0));
    //         Assert.AreNotEqual(new List<string>(), chk.CheckIfTableMatchesAmountOfRegisters(_FAKE, _FAKE, 0));            
            
    //         Assert.AreEqual(new List<string>(), chk.CheckIfTableMatchesAmountOfRegisters(_SCHEMA, "countries", "id_region", (short)1, 2));          
    //         Assert.AreEqual(new List<string>(), chk.CheckIfTableMatchesAmountOfRegisters(_SCHEMA, "countries", "id_region", (short)1, Operator.GREATEREQUALS, 4));          
    //         Assert.AreEqual(new List<string>(), chk.CheckIfTableMatchesAmountOfRegisters(_SCHEMA, "countries", "id_region", (short)1, Operator.GREATER, 2));          
    //         Assert.AreEqual(new List<string>(), chk.CheckIfTableMatchesAmountOfRegisters(_SCHEMA, "countries", "id_region", (short)1, Operator.LOWER, 0));          
    //         Assert.AreEqual(new List<string>(), chk.CheckIfTableMatchesAmountOfRegisters(_SCHEMA, "countries", "id_region", (short)1, Operator.LOWEREQUALS, 2));          
    //     }      
    // }
}