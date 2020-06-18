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

using System.Collections.Generic;
using AutoCheck.Exceptions;
using NUnit.Framework;

namespace AutoCheck.Test.Core
{    
    [Parallelizable(ParallelScope.All)]    
    public class Script : Test
    {
        private class TestScript : AutoCheck.Core.Script<CopyDetectors.None>{
            public string Argument1 {get; private set;}
            public string Argument2 {get; private set;}
            public string Argument3 {get; private set;}
            
            public TestScript(Dictionary<string, string> args): base(args){                        
            } 

            protected override void DefaultArguments(){
                base.DefaultArguments();

                this.Argument1 = "default1";
                this.Argument2 = "default2";
                this.Argument3 = "default3";
            }  

            public new void OpenQuestion(string caption, float score=0){
                base.OpenQuestion(caption, score);
            }

            protected new void OpenQuestion(string caption, string description, float score=0){
                base.OpenQuestion(caption, description, score);
            }

            public new void CloseQuestion(string caption=null){
                base.CloseQuestion(caption);
            }

            public new void CancelQuestion(){
                base.CancelQuestion();
            }

            public new void EvalQuestion(){
                base.EvalQuestion();
            }

            public new void EvalQuestion(List<string> errors){
                base.EvalQuestion(errors);
            }
        }         

        [Test]
        public void LoadArguments()
        {                               
            Assert.DoesNotThrow(() => new TestScript(new Dictionary<string, string>{})); 
            Assert.Throws<ArgumentInvalidException>(() => new TestScript(new Dictionary<string, string>{{"fake", "fake"}})); 
            Assert.DoesNotThrow(() => new TestScript(new Dictionary<string, string>{{"argument1", "fake"}})); 
            Assert.DoesNotThrow(() => new TestScript(new Dictionary<string, string>{{"argument1", "fake1"}, {"argument2", "fake2"}, {"argument3", "fake3"}})); 
            Assert.Throws<ArgumentInvalidException>(() => new TestScript(new Dictionary<string, string>{{"argument1", "fake1"}, {"argument2", "fake2"}, {"fake", "fake"}, {"argument3", "fake3"}})); 

            var ts = new TestScript(new Dictionary<string, string>{{"argument1", "fake1"}, {"argument2", "fake2"}, {"argument3", "fake3"}});
            Assert.AreEqual("fake1", ts.Argument1);
            Assert.AreEqual("fake2", ts.Argument2);
            Assert.AreEqual("fake3", ts.Argument3);
            Assert.AreEqual(0, ts.Score);
            Assert.IsFalse(ts.IsQuestionOpen);

            ts = new TestScript(new Dictionary<string, string>{});
            Assert.AreEqual("default1", ts.Argument1);
            Assert.AreEqual("default2", ts.Argument2);
            Assert.AreEqual("default3", ts.Argument3);

            ts = new TestScript(new Dictionary<string, string>{{"argument2", "fake2"}});
            Assert.AreEqual("default1", ts.Argument1);
            Assert.AreEqual("fake2", ts.Argument2);
            Assert.AreEqual("default3", ts.Argument3);            
        }             

        [Test]
        public void CloseQuestion()
        {                                          
            var ts = new TestScript(new Dictionary<string, string>{});
            Assert.AreEqual(0, ts.Score);
                       
            ts.OpenQuestion("Caption", 2);
            Assert.IsTrue(ts.IsQuestionOpen);          
            Assert.AreEqual(0, ts.Score);
            ts.CloseQuestion();
            Assert.IsFalse(ts.IsQuestionOpen); 
            Assert.AreEqual(10f, ts.Score);

            ts.OpenQuestion("Caption", 1);
            Assert.IsTrue(ts.IsQuestionOpen);          
            Assert.AreEqual(10f, ts.Score);
            ts.CloseQuestion();
            Assert.IsFalse(ts.IsQuestionOpen); 
            Assert.AreEqual(10f, ts.Score);
        }  

        [Test]
        public void CancelQuestion()
        {                                          
            var ts = new TestScript(new Dictionary<string, string>{});
            Assert.AreEqual(0, ts.Score);
            
            ts.OpenQuestion("Caption", 1);                        
            Assert.IsTrue(ts.IsQuestionOpen);          
            Assert.AreEqual(0, ts.Score);
            ts.CancelQuestion();
            Assert.IsFalse(ts.IsQuestionOpen); 
            Assert.AreEqual(0, ts.Score);
        } 

        [Test]
        public void EvalQuestion()
        {                                          
            var ts = new TestScript(new Dictionary<string, string>{});
            Assert.AreEqual(0, ts.Score);
                       
            ts.OpenQuestion("Caption", 3);
            Assert.IsTrue(ts.IsQuestionOpen);          
            Assert.AreEqual(0, ts.Score);
            ts.EvalQuestion();
            ts.CloseQuestion();
            Assert.IsFalse(ts.IsQuestionOpen); 
            Assert.AreEqual(10f, ts.Score);

            ts.OpenQuestion("Caption", 1);
            Assert.IsTrue(ts.IsQuestionOpen);          
            Assert.AreEqual(10f, ts.Score);
            ts.EvalQuestion(new List<string>(){"ERROR"});
            ts.CloseQuestion();
            Assert.IsFalse(ts.IsQuestionOpen); 
            Assert.AreEqual(7.5f, ts.Score);
        }

        [Test]
        public void Run()
        {                                          
            var ts = new TestScript(new Dictionary<string, string>{});
            Assert.DoesNotThrow(() => ts.Run());            
        }

        
        //TODO: test Batch()        
        //          1. Recursive unzip
        //          2. Delete zipped files
        //          3. Copy detection
        //          4. Run

        //TODO: once the output has been configurable (terminal, file, etc) test the captions, PrintScore and other print stuff
    }
}
