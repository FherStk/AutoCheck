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

            File.Delete(GetSampleFile("nopass.txt"));
            File.Delete(GetSampleFile("nopass.zip"));
            File.Copy(GetSampleFile("zip", "nopass.zip"), GetSampleFile("nopass.zip"));            
        }

        [OneTimeTearDown]
        public void Cleanup(){     
            AutoCheck.Core.Output.Instance.Enable();
        }

        [Test]
        public void ParseVars_OK()
        {  
            var s = new TestScript(GetSampleFile("vars_ok.yaml"));
            
            //Custom vars
            Assert.AreEqual("Fer", s.Vars["student_name"].ToString());
            Assert.AreEqual("Fer", s.Vars["student_var"].ToString());
            Assert.AreEqual("This is a test with value: Fer_Fer!", s.Vars["student_replace"].ToString());
            Assert.AreEqual("TEST_FOLDER", s.Vars["test_folder"].ToString());            
            Assert.AreEqual("FOLDER", s.Vars["folder_regex"].ToString());
            Assert.AreEqual("Fer FOLDER FOLDER", s.Vars["current_regex"].ToString());
            

            //Predefined vars
            Assert.AreEqual("vars ok", s.Vars["script_name"].ToString());            
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
        public void Constructor()
        {   
            Assert.IsTrue(File.Exists(GetSampleFile("nopass.zip")));
            Assert.IsFalse(File.Exists(GetSampleFile("nopass.txt")));                                                                                       

            var s = new TestScript(GetSampleFile("single.yaml"));
            
            //Custom vars
            Assert.AreEqual("Fer", s.Vars["student_name"].ToString());
            Assert.AreEqual("Fer", s.Vars["student_var"].ToString());
            Assert.AreEqual("This is a test with value: Fer_Fer!", s.Vars["student_replace"].ToString());
            Assert.AreEqual("TEST_FOLDER", s.Vars["test_folder"].ToString());            
            Assert.AreEqual("FOLDER", s.Vars["folder_regex"].ToString());
            Assert.AreEqual("Fer FOLDER FOLDER", s.Vars["current_regex"].ToString());

            //Predefined vars
            Assert.AreEqual("USER FRIENDLY NAME", s.Vars["script_name"].ToString());            
            Assert.AreEqual("C:\\Users\\fher\\source\\repos\\AutoCheck.Test\\samples\\script", s.Vars["current_folder"].ToString());            
            Assert.NotNull(s.Vars["current_date"].ToString());

            //Extract
            Assert.IsFalse(File.Exists(GetSampleFile("nopass.zip")));
            Assert.IsTrue(File.Exists(GetSampleFile("nopass.txt")));
            File.Delete(GetSampleFile("nopass.txt"));

            //TODO: test db resotre
        }

        //TODO: Constructor with errors
    }
}
