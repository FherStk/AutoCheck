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

namespace AutoCheck.Test.Checkers
{    
    [Parallelizable(ParallelScope.All)]  
    public class Postgres : Core.Test
    {   
        /// <summary>
        /// The connector instance is created here because a new one-time use BBDD will be created on every startup, and dropped when done.
        /// </summary>
        private ConcurrentDictionary<string, AutoCheck.Checkers.Postgres> Pool = new ConcurrentDictionary<string, AutoCheck.Checkers.Postgres>();

        private AutoCheck.Checkers.Postgres Chk = null;
        
        private readonly Source[] _emptySources = new Source[]{
            //Should throw null argument exception when using on a method
            null,
            new Source(string.Empty, string.Empty),
            new Source(_fake, string.Empty),
            new Source(string.Empty, _fake),
            new Source(null, null),
            new Source(_fake, null),
            new Source(null, _fake)
        };

        private readonly Destination[] _emptyDestinations = new Destination[]{
            //Should throw null argument exception when using on a method
            null,
            new Destination(string.Empty, string.Empty),
            new Destination(_fake, string.Empty),
            new Destination(string.Empty, _fake),
            new Destination(null, null),
            new Destination(_fake, null),
            new Destination(null, _fake)
        };

        private readonly Filter[] _emptyFilters = new Filter[]{
            //Should throw null argument exception when using on a method
            null,
            new Filter(null, Operator.EQUALS,null),
            new Filter(_fake, Operator.EQUALS, null),
            new Filter(null, Operator.EQUALS, _fake),
            new Filter(string.Empty, Operator.EQUALS, string.Empty),
            new Filter(_fake, Operator.EQUALS, string.Empty),
            new Filter(string.Empty, Operator.EQUALS, _fake)
        };

        private readonly Dictionary<string, object>[] _emptyFields = new Dictionary<string, object>[]{
            //Should throw null argument exception when using on a method
            new Dictionary<string, object>(){}
        };

        private readonly Source[] _wrongSources = new Source[]{
            //Should throw invalid query exception when using on a method
            new Source(_fake, _fake),
            new Source(_schema, _fake)            
        };

        private readonly Destination[] _wrongDestinations = new Destination[]{
            //Should throw invalid query exception when using on a method
            new Destination(_fake, _fake),
            new Destination(_schema, _fake)
        };

        private readonly Filter[] _wrongFilters = new Filter[]{
            //Should throw null argument exception when using on a method            
            new Filter(_fake, Operator.EQUALS, _fake),
            new Filter(_fake, Operator.GREATER, 0),
            new Filter(_fake, Operator.GREATEREQUALS, 5),
            new Filter(_fake, Operator.LOWER, 1.5f),
            new Filter(_fake, Operator.LOWEREQUALS, -1.5f),
            new Filter(_fake, Operator.LIKE, 'a'),
            new Filter(_fake, Operator.NOTEQUALS, true)
        };

        private readonly Dictionary<string, object>[] _wrongFields = new Dictionary<string, object>[]{
            //Should throw null argument exception when using on a method
            new Dictionary<string, object>(){{_fake, null}},
            new Dictionary<string, object>(){{_fake, string.Empty}},            
            new Dictionary<string, object>(){{_fake, _fake}},            
            new Dictionary<string, object>(){{_fake, true}},
            new Dictionary<string, object>(){{_fake, 1}},
            new Dictionary<string, object>(){{_fake, -1}},
            new Dictionary<string, object>(){{_fake, 15.27f}},
            new Dictionary<string, object>(){{_fake, 'a'}}
        };

        private const string _host = "localhost";
        
        private const string _fake = "FAKE";
        
        private const string _admin = "postgres";
        
        private const string _schema = "test";

        private const string _role = "chk_pg_role";
        
        private const string _group = "chk_pg_group";

        [OneTimeSetUp]
        public void Init() 
        {
            base.Setup("postgres");
            AutoCheck.Core.Output.Instance.Disable();

            //The same database (but different checker instance, to allow parallel queries) will be shared along the different tests, because all the opperations 
            //are read-only; this will boost the test performance because loading the Odoo database is a long time opperation.
            this.Chk =new AutoCheck.Checkers.Postgres(_host, string.Format("autocheck_{0}", TestContext.CurrentContext.Test.ID), _admin, _admin); 
            
            if(this.Chk.Connector.ExistsDataBase()) this.Chk.Connector.DropDataBase();
            this.Chk.Connector.CreateDataBase(base.GetSampleFile("dump.sql"));

            if(!this.Chk.Connector.ExistsRole(_role)) this.Chk.Connector.CreateRole(_role);
            if(!this.Chk.Connector.ExistsRole(_group)) this.Chk.Connector.CreateRole(_group);            
            this.Chk.Connector.Grant("SELECT, INSERT, UPDATE", "test.work", _role);   
            this.Chk.Connector.Grant("USAGE", "test", _role);  
            this.Chk.Connector.Grant(_group, _role);
        }

        [OneTimeTearDown]
        public void Cleanup(){     
            this.Pool.Clear(); 
            this.Chk.Connector.DropDataBase();
        }

        [SetUp]
        public void Setup() 
        {
            //Create a new and unique database connection for the current context (same DB for all tests)
            var chk = new AutoCheck.Checkers.Postgres(this.Chk.Host, this.Chk.Database, this.Chk.User, this.Chk.User);
                       
            //Storing the chkector instance for the current context
            var added = false;
            do added = this.Pool.TryAdd(TestContext.CurrentContext.Test.ID, chk);             
            while(!added);                          
        }

        [TearDown]
        public void TearDown(){
            var chk = this.Pool[TestContext.CurrentContext.Test.ID];
            chk.Dispose();
        }

        [Test]
        public void Constructor()
        {                                            
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Checkers.Postgres("", "", "", ""));
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Checkers.Postgres(_fake, "", "", ""));
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Checkers.Postgres(_fake, _fake, "", ""));  
            Assert.DoesNotThrow(() => new AutoCheck.Checkers.Postgres(_host, _fake, _admin, _admin));         
        }                    

        [Test]
        public void CheckIfTableMatchesPrivileges(){
            var chk = this.Pool[TestContext.CurrentContext.Test.ID];

            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableMatchesPrivileges("", "", "", ""));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableMatchesPrivileges(_fake, "", "", ""));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableMatchesPrivileges(_fake, _fake, "", ""));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableMatchesPrivileges(_fake, _fake, _fake, ""));

            Assert.AreEqual(new List<string>(), chk.CheckIfTableMatchesPrivileges(_role, _schema, "work", "arw"));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableMatchesPrivileges(_role, _schema, "work", "d"));            
        }

        [Test]
        public void CheckIfTableContainsPrivileges(){
            var chk = this.Pool[TestContext.CurrentContext.Test.ID];

            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableContainsPrivileges("", "", "", ' '));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableContainsPrivileges(_fake, "", "", ' '));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableContainsPrivileges(_fake, _fake, "", ' '));

            Assert.AreEqual(new List<string>(), chk.CheckIfTableContainsPrivileges(_role, _schema, "work", 'r'));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableContainsPrivileges(_role, _schema, "work", 'd'));            
        }

        [Test]
        public void CheckIfSchemaMatchesPrivileges(){
            var chk = this.Pool[TestContext.CurrentContext.Test.ID];

            Assert.AreNotEqual(new List<string>(), chk.CheckIfSchemaMatchesPrivileges("", "", ""));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfSchemaMatchesPrivileges(_fake, "", ""));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfSchemaMatchesPrivileges(_fake, _fake, ""));

            Assert.AreEqual(new List<string>(), chk.CheckIfSchemaMatchesPrivileges(_role, _schema, "U"));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfSchemaMatchesPrivileges(_role, _schema, "C"));            
        }

        [Test]
        public void CheckIfSchemaContainsPrivilege(){
            var chk = this.Pool[TestContext.CurrentContext.Test.ID];

            Assert.AreNotEqual(new List<string>(), chk.CheckIfSchemaContainsPrivilege("", "", ' '));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfSchemaContainsPrivilege(_fake, "", ' '));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfSchemaContainsPrivilege(_fake, _fake, ' '));

            Assert.AreEqual(new List<string>(), chk.CheckIfSchemaContainsPrivilege(_role, _schema, 'U'));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfSchemaContainsPrivilege(_role, _schema, 'C'));            
        }

        [Test]
        public void CheckRoleMembership(){
            var chk = this.Pool[TestContext.CurrentContext.Test.ID];

            Assert.AreNotEqual(new List<string>(), chk.CheckRoleMembership("", null));
            Assert.AreNotEqual(new List<string>(), chk.CheckRoleMembership(_fake, null));            

            Assert.AreEqual(new List<string>(), chk.CheckRoleMembership(_role, new string[]{_group}));
            Assert.AreNotEqual(new List<string>(), chk.CheckRoleMembership(_admin, new string[]{_group}));            
        }

        [Test]
        public void CheckForeignKey(){
            var chk = this.Pool[TestContext.CurrentContext.Test.ID];

            Assert.AreNotEqual(new List<string>(), chk.CheckForeignKey("", "", "", "", "", ""));
            Assert.AreNotEqual(new List<string>(), chk.CheckForeignKey(_fake, "", "", "", "", ""));
            Assert.AreNotEqual(new List<string>(), chk.CheckForeignKey(_fake, _fake, "", "", "", ""));
            Assert.AreNotEqual(new List<string>(), chk.CheckForeignKey(_fake, _fake, _fake, "", "", ""));
            Assert.AreNotEqual(new List<string>(), chk.CheckForeignKey(_fake, _fake, _fake, _fake, "", ""));
            Assert.AreNotEqual(new List<string>(), chk.CheckForeignKey(_fake, _fake, _fake, _fake, _fake, ""));
            Assert.AreNotEqual(new List<string>(), chk.CheckForeignKey(_fake, _fake, _fake, _fake, _fake, _fake));

            Assert.AreEqual(new List<string>(), chk.CheckForeignKey(_schema, "work_history", "id_work", _schema, "work", "id_work"));
            Assert.AreNotEqual(new List<string>(), chk.CheckForeignKey(_schema, "work_history", "id_work", _schema, "work", _fake));
        }

        [Test]
        public void CheckIfEntryAdded(){
            var chk = this.Pool[TestContext.CurrentContext.Test.ID];

            Assert.AreNotEqual(new List<string>(), chk.CheckIfEntryAdded("", "", "", 0));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfEntryAdded(_fake, "", "", 0));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfEntryAdded(_fake, _fake, "", 0));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfEntryAdded(_fake, _fake, _fake, 0));
            
            Assert.AreEqual(new List<string>(), chk.CheckIfEntryAdded(_schema, "employees", "id_employee", 0));
            Assert.AreEqual(new List<string>(), chk.CheckIfEntryAdded(_schema, "employees", "id_employee", 205));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfEntryAdded(_schema, "employees", "id_employee", 206));
        }

        [Test]
        public void CheckIfEntryRemoved(){
            var chk = this.Pool[TestContext.CurrentContext.Test.ID];

            Assert.AreNotEqual(new List<string>(), chk.CheckIfEntryRemoved("", "", "", 0));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfEntryRemoved(_fake, "", "", 0));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfEntryRemoved(_fake, _fake, "", 0));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfEntryRemoved(_fake, _fake, _fake, 0));
            
            Assert.AreNotEqual(new List<string>(), chk.CheckIfEntryRemoved(_schema, "employees", "id_employee", 206));
            Assert.AreEqual(new List<string>(), chk.CheckIfEntryRemoved(_schema, "employees", "id_employee", 207));
        }

        [Test]
        public void CheckIfTableContainsData(){
            var chk = this.Pool[TestContext.CurrentContext.Test.ID];

            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableContainsData("", "", null));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableContainsData(_fake, "", null));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableContainsData(_fake, _fake, null));
           
            
            Assert.AreEqual(new List<string>(), chk.CheckIfTableContainsData(_schema, "countries", new Dictionary<string, object>(){{"id_contry", "CA"}, {"name_country", "Canada"}, {"id_region", (short)2}}));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableContainsData(_schema, "countries", new Dictionary<string, object>(){{"id_contry", _fake}, {"name_country", "Canada"}, {"id_region", (short)2}}));
        }

        [Test]
        public void CheckIfSelectContainsData(){
            var chk = this.Pool[TestContext.CurrentContext.Test.ID];

            Assert.AreNotEqual(new List<string>(), chk.CheckIfSelectContainsData("", null));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfSelectContainsData(_fake, null));           
            
            Assert.AreEqual(new List<string>(), chk.CheckIfSelectContainsData(string.Format("SELECT * FROM {0}.{1}", _schema, "countries"), new Dictionary<string, object>(){{"id_contry", "CA"}, {"name_country", "Canada"}, {"id_region", (short)2}}));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfSelectContainsData(string.Format("SELECT * FROM {0}.{1}", _schema, "countries"), new Dictionary<string, object>(){{"id_contry", _fake}, {"name_country", "Canada"}, {"id_region", (short)2}}));
        }

        [Test]
        public void CheckIfTableExists(){
            var chk = this.Pool[TestContext.CurrentContext.Test.ID];

            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableExists(null, null));            
            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableExists(_fake, null));           
            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableExists(_fake, _fake));
            
            Assert.AreEqual(new List<string>(), chk.CheckIfTableExists(_schema, "work"));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableExists(_fake, "work"));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableExists(_schema, _fake));
        }

        [Test]
        public void CheckIfViewMatchesDefinition(){
            var chk = this.Pool[TestContext.CurrentContext.Test.ID];

            Assert.AreNotEqual(new List<string>(), chk.CheckIfViewMatchesDefinition(null, null, null));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfViewMatchesDefinition(_fake, null, null));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfViewMatchesDefinition(_fake, _fake, null));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfViewMatchesDefinition(_fake, _fake, _fake));
            
            Assert.AreEqual(new List<string>(), chk.CheckIfViewMatchesDefinition(_schema, "programmers", @"
                SELECT employees.id_employee AS id, employees.id_boss, employees.name, employees.surnames, employees.email, employees.phone
                FROM test.employees
                WHERE employees.id_work = 'IT_PROG';"
            ));

            //TODO: column names should be equals... not enough time to code it :(
            Assert.AreEqual(new List<string>(), chk.CheckIfViewMatchesDefinition(_schema, "programmers", @"
                SELECT employees.id_employee AS fake, employees.id_boss, employees.name, employees.surnames, employees.email, employees.phone
                FROM test.employees
                WHERE employees.id_work = 'IT_PROG';"
            ));

            Assert.AreNotEqual(new List<string>(), chk.CheckIfViewMatchesDefinition(_schema, "programmers", @"
                SELECT employees.id_employee AS id, employees.id_boss, employees.name, employees.surnames, employees.email, employees.phone
                FROM test.employees
                WHERE employees.id_work = 'FAKE';"
            ));
        
            Assert.AreNotEqual(new List<string>(), chk.CheckIfViewMatchesDefinition(_schema, "programmers", @"
                SELECT employees.id_boss, employees.name, employees.surnames, employees.email, employees.phone
                FROM test.employees
                WHERE employees.id_work = 'IT_PROG';"
            ));

            Assert.AreNotEqual(new List<string>(), chk.CheckIfViewMatchesDefinition(_schema, "programmers", @"
                SELECT *
                FROM work;"
            ));
        }

        [Test]
        public void CheckIfTableInsertsData(){
            var chk = this.Pool[TestContext.CurrentContext.Test.ID];
            
            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableInsertsData(null, null, null));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableInsertsData(_fake, null, null));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableInsertsData(_fake, _fake, null));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableInsertsData(_fake, _fake, new Dictionary<string, object>(){{_fake, null}}));

            var table = "regions";
            var path = string.Format("{0}.{1}", _schema, table);
            var subQuery = string.Format("@(SELECT MAX(id_region)+1 FROM {0})", path);
            Assert.AreEqual(new List<string>(), chk.CheckIfTableInsertsData(_schema, table, new Dictionary<string, object>(){{"id_region", subQuery}, {"name_region", _fake}}));          
        }

        [Test]
        public void CheckIfTableUpdatesData(){
            var chk = this.Pool[TestContext.CurrentContext.Test.ID];
            
            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableUpdatesData(null, null, null));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableUpdatesData(_fake, null, null));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableUpdatesData(_fake, _fake, null));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableUpdatesData(_fake, _fake, new Dictionary<string, object>(){{_fake, null}}));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableUpdatesData(_fake, _fake, _fake, _fake, Operator.EQUALS, new Dictionary<string, object>(){{_fake, null}}));

            var table = "regions";
            var path = string.Format("{0}.{1}", _schema, table);
            var subQuery = string.Format("@(SELECT MAX(id_region)+1 FROM {0})", path);
            Assert.AreEqual(new List<string>(), chk.CheckIfTableUpdatesData(_schema, table, "id_region", (short)1, new Dictionary<string, object>(){{"name_region", _fake}}));          
        }

        [Test]
        public void CheckIfTableDeletesData(){
            var chk = this.Pool[TestContext.CurrentContext.Test.ID];
            
            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableDeletesData(null, null));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableDeletesData(_fake, null));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableDeletesData(_fake, _fake));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableDeletesData(_fake, _fake, _fake, Operator.EQUALS));
            
            Assert.AreEqual(new List<string>(), chk.CheckIfTableDeletesData(_schema, "work_history", "id_employee", (short)101));          
        }

        [Test]
        public void CheckIfTableMatchesAmountOfRegisters(){
            var chk = this.Pool[TestContext.CurrentContext.Test.ID];
            
            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableMatchesAmountOfRegisters(null, null, 0));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableMatchesAmountOfRegisters(_fake, null, 0));
            Assert.AreNotEqual(new List<string>(), chk.CheckIfTableMatchesAmountOfRegisters(_fake, _fake, 0));            
            
            Assert.AreEqual(new List<string>(), chk.CheckIfTableMatchesAmountOfRegisters(_schema, "countries", "id_region", (short)1, 2));          
            Assert.AreEqual(new List<string>(), chk.CheckIfTableMatchesAmountOfRegisters(_schema, "countries", "id_region", (short)1, Operator.GREATEREQUALS, 4));          
            Assert.AreEqual(new List<string>(), chk.CheckIfTableMatchesAmountOfRegisters(_schema, "countries", "id_region", (short)1, Operator.GREATER, 2));          
            Assert.AreEqual(new List<string>(), chk.CheckIfTableMatchesAmountOfRegisters(_schema, "countries", "id_region", (short)1, Operator.LOWER, 0));          
            Assert.AreEqual(new List<string>(), chk.CheckIfTableMatchesAmountOfRegisters(_schema, "countries", "id_region", (short)1, Operator.LOWEREQUALS, 2));          
        }      
    }
}