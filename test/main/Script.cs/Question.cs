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
    public class Question : Test
    { 
        public Question(): base("script"){
        }         

        [Test, Category("Question"), Category("Local")]
        public void Script_QUESTION_DEFAULT_SINGLE_ECHO()
        {                                      
            var s = new AutoCheck.Core.Script(GetSampleFile("question_ok1.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Running script question_ok1 (v1.0.0.0):\r\n   Question 1 [1 point]:\r\n      Running echo... OK\r\n\r\n   TOTAL SCORE: 10 / 10", log);
        }

        [Test, Category("Question"), Category("Local")]
        public void Script_QUESTION_DEFAULT_MULTI_ECHO()
        {              
            var s = new AutoCheck.Core.Script(GetSampleFile("question_ok2.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Running script question_ok2 (v1.0.0.0):\r\n   Question 1 [1 point]:\r\n      Running echo (1/2)... OK\r\n      Running echo (2/2)... OK\r\n\r\n   TOTAL SCORE: 10 / 10", log);
        }

        [Test, Category("Question"), Category("Local")]
        public void Script_QUESTION_DEFAULT_MULTI_METHODS()
        {              
            var s = new AutoCheck.Core.Script(GetSampleFile("question_ok7.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Running script question_ok7 (v1.0.0.1):\r\n   Question 1 [1 point]:\r\n      Checking files... OK\r\n      Getting files... OK\r\n\r\n   TOTAL SCORE: 10 / 10", log);
        }

        [Test, Category("Question"), Category("Local")]
        public void Script_QUESTION_BATCH_MULTI_ECHO()
        {              
            var s = new AutoCheck.Core.Script(GetSampleFile("question_ok3.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Running script question_ok3 (v1.0.0.0):\r\n   Question 1 [1 point]:\r\n      Running echo (1/2)... OK\r\n      Running echo (2/2)... OK\r\n\r\n   Question 2 [1 point]:\r\n      Running echo (1/2)... OK\r\n      Running echo (2/2)... ERROR:\n         -Expected -> Wanted fail!; Found -> This is NOT OK\r\n\r\n   TOTAL SCORE: 5 / 10", log);
        } 

        [Test, Category("Question"), Category("Local")]
        public void Script_QUESTION_BATCH_MULTI_MESSAGES()
        {              
            var s = new AutoCheck.Core.Script(GetSampleFile("question_ok4.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Running script question_ok4 (v1.0.0.0):\r\n   Question 1 [1 point]:\r\n      Running echo (1/2)... OK\r\n      Running echo (2/2)... ERROR:\n         -Expected -> Wanted fail!; Found -> Bye!\r\n\r\n   Question 2 [1 point]:\r\n      Running echo (1/2)... GREAT!\r\n      Running echo (2/2)... SO BAD!:\n         -Expected -> Wanted fail!; Found -> This is NOT OK\r\n\r\n   TOTAL SCORE: 0 / 10", log);
        }

        [Test, Category("Question"), Category("Local")] 
        public void Script_QUESTION_BATCH_MULTI_SCORE()
        {              
            var s = new AutoCheck.Core.Script(GetSampleFile("question_ok5.yaml")); 
            var log = s.Output.ToString();
            Assert.AreEqual("Running script question_ok5 (v1.0.0.0):\r\n   Question 1 [2 points]:\r\n      Running echo (1/2)... OK\r\n      Running echo (2/2)... OK\r\n\r\n   Question 2 [1 point]:\r\n      Running echo (1/2)... OK\r\n      Running echo (2/2)... ERROR:\n         -Expected -> Wanted fail!; Found -> This is NOT OK\r\n\r\n   TOTAL SCORE: 6.67 / 10", log);
        }

        [Test, Category("Question"), Category("Local")]
        public void Script_QUESTION_BATCH_MULTI_DESCRIPTION()
        {                                      
            var s = new AutoCheck.Core.Script(GetSampleFile("question_ok6.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Running script question_ok6 (v1.0.0.0):\r\n   My custom caption for the question 1 - My custom description with score 3/10 (TOTAL: 0):\r\n      Running echo (1/2)... OK\r\n      Running echo (2/2)... ERROR:\n         -Expected -> Error wanted!; Found -> Hello\r\n\r\n   My custom caption for the question 2 - My custom description with score 2/10 (TOTAL: 0):\r\n      Running echo... OK\r\n\r\n   My custom caption for the question 3 - My custom description with score 5/10 (TOTAL: 4):\r\n      Running echo (1/3)... OK\r\n      Running echo (2/3)... OK\r\n      Running echo (3/3)... OK\r\n\r\n   TOTAL SCORE: 7 / 10", log);
        }

        [Test, Category("Question"), Category("Local")]
        public void Script_QUESTION_BATCH_MULTI_METHODS()
        {              
            var s = new AutoCheck.Core.Script(GetSampleFile("question_ok8.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Running script question_ok8 (v1.0.0.1):\r\n   Question 1 [1 point]:\r\n      Checking files... OK\r\n      Getting files... OK\r\n\r\n   Question 2 [1 point]:\r\n      Counting folders... ERROR:\n         -Expected -> -1; Found -> 0\r\n\r\n   TOTAL SCORE: 5 / 10", log);
        } 

        [Test, Category("Question"), Category("Local")]
        public void Script_QUESTION_BATCH_SUBQUESTION_SINGLE()
        {                                      
            var s = new AutoCheck.Core.Script(GetSampleFile("question_ok9.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Running script question_ok9 (v1.0.0.0):\r\n   Question 1 [10 points]:\r\n\r\n      Question 1.1 [2 points]:\r\n         Running echo... OK\r\n\r\n      Question 1.2 [5 points]:\r\n         Running echo (1/3)... OK\r\n         Running echo (2/3)... OK\r\n         Running echo (3/3)... OK\r\n\r\n      Question 1.3 [3 points]:\r\n         Running echo... ERROR:\n            -Expected -> Wanted Error!; Found -> Hello\r\n\r\n   TOTAL SCORE: 7 / 10", log);
        }

        [Test, Category("Question"), Category("Local")]
        public void Script_QUESTION_BATCH_SUBQUESTION_MULTI()
        {                                      
            var s = new AutoCheck.Core.Script(GetSampleFile("question_ok10.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Running script question_ok10 (v1.0.0.0):\r\n   Question 1 [4 points]:\r\n\r\n      Question 1.1 [1 point]:\r\n         Running echo... OK\r\n\r\n      Question 1.2 [2 points]:\r\n\r\n         Question 1.2.1 [1 point]:\r\n            Running echo... OK\r\n\r\n         Question 1.2.2 [1 point]:\r\n            Running echo... OK\r\n\r\n      Question 1.3 [1 point]:\r\n         Running echo... ERROR:\n            -Expected -> Wanted Error!; Found -> Hello\r\n\r\n   Question 2 [3 points]:\r\n\r\n      Question 2.1 [1 point]:\r\n         Running echo... OK\r\n\r\n      Question 2.2 [1 point]:\r\n         Running echo (1/3)... OK\r\n         Running echo (2/3)... OK\r\n         Running echo (3/3)... OK\r\n\r\n      Question 2.3 [1 point]:\r\n         Running echo... ERROR:\n            -Expected -> Wanted Error!; Found -> Hello\r\n\r\n   TOTAL SCORE: 7.14 / 10", log);
        }
        
        [Test, Category("Question"), Category("Local")]
        public void Script_QUESTION_BATCH_SUBQUESTION_RUN()
        {                                      
            var s = new AutoCheck.Core.Script(GetSampleFile("question_ok11.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Running script question_ok11 (v1.0.0.0):\r\n   Question 1 [2 points]:\r\n\r\n      Question 1.1 [1 point]:\r\n         Running echo... OK\r\n\r\n      Question 1.2 [1 point]:\r\n         Running echo... ERROR:\n            -Expected -> Wanted Error!; Found -> Hello\r\n\r\n   TOTAL SCORE: 5 / 10", log);
        }   

        [Test, Category("Question"), Category("Local")]
        public void Script_QUESTION_BATCH_ONERROR_SKIP()
        {                                      
            var s = new AutoCheck.Core.Script(GetSampleFile("question_ok12.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Running script question_ok12 (v1.0.0.0):\r\n   Question 1 [2 points]:\r\n      Running echo One... OK\r\n\r\n      Question 1.1 [1 point]:\r\n         Running echo Two... ERROR:\n            -Expected -> WANTEDERROR; Found -> Two\r\n\r\n      Question 1.2 [1 point]:\r\n         Running echo Four... OK\r\n\r\n   TOTAL SCORE: 5 / 10", log);
        } 

        [Test, Category("Question"), Category("Local")]
        public void Script_QUESTION_BATCH_ONERROR_ABORT()
        {                                      
            var s = new AutoCheck.Core.Script(GetSampleFile("question_ok13.yaml"));
            var log = s.Output.ToString();
            Assert.AreEqual("Running script question_ok13 (v1.0.0.0):\r\n   Question 1 [2 points]:\r\n      Running echo One... OK\r\n\r\n      Question 1.1 [1 point]:\r\n         Running echo Two... ERROR:\n            -Expected -> WANTEDERROR; Found -> Two\r\n\r\n\r\n   Aborting execution!\r\n\r\n   TOTAL SCORE: 0 / 10", log);
        } 
    }
}