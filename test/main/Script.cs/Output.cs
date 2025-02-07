/*
    Copyright © 2023 Fernando Porrino Serrano
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
using System.Linq;
using NUnit.Framework;

namespace AutoCheck.Test
{    
    [Parallelizable(ParallelScope.All)]    
    public class Output : Test
    {
        public Output(): base("script"){
        }   

        [Test, Category("Output"), Category("Local"), Category("Text")]
        public void Output_SendToTerminal_NotNull()
        {             
            var logFilePath = Path.Combine(LogRootFolder, "script", "output", "output_single_1_yaml", "OUTPUT SINGLE 1_Student Name 1.log");
            if(File.Exists(logFilePath)) File.Delete(logFilePath);
            
            var s = new AutoCheck.Core.Script(GetSampleFile("output_single_1.yaml"));
            
            //Disabling terminal output
            System.Console.SetOut(new StringWriter());
            s.Output.SendToTerminal(s.Output.GetLog().FirstOrDefault());   

            //Enabling terminal output
            var standardOutput = new StreamWriter(System.Console.OpenStandardOutput());
            standardOutput.AutoFlush = true;
            System.Console.SetOut(standardOutput);         
        } 

        [Test, Category("Output"), Category("Local"), Category("Text")]
        public void Output_SendToTerminal_Null()
        {             
            var logFilePath = Path.Combine(LogRootFolder, "script", "output", "output_single_1_yaml", "OUTPUT SINGLE 1_Student Name 1.log");
            if(File.Exists(logFilePath)) File.Delete(logFilePath);
            
            var s = new AutoCheck.Core.Script(GetSampleFile("output_single_1.yaml"));
            s.Output.SendToTerminal(null);            
        }     
       
        [Test, Category("Output"), Category("Local"), Category("Text")]
        public void Output_SINGLE_FILE_1()
        {             
            var logFilePath = Path.Combine(LogRootFolder, "script", "output", "output_single_1_yaml", "OUTPUT SINGLE 1_Student Name 1.log");
            if(File.Exists(logFilePath)) File.Delete(logFilePath);
            
            var s = new AutoCheck.Core.Script(GetSampleFile("output_single_1.yaml"));
            Assert.AreEqual(logFilePath, Core.Utils.PathToCurrentOS(s.LogFiles.FirstOrDefault()));
            Assert.IsTrue(File.Exists(logFilePath));        
            Assert.IsTrue(File.ReadAllText(logFilePath).Equals(s.Output.GetLog().LastOrDefault().ToText()));
        } 

        [Test, Category("Output"), Category("Local"), Category("Json")]
        public void Output_SINGLE_FILE_2()
        {
            //Fails, loop when serializing json             
            var logFilePath = Path.Combine(LogRootFolder, "script", "output", "output_single_2_yaml", "OUTPUT SINGLE 2_Student Name 1.log");
            if(File.Exists(logFilePath)) File.Delete(logFilePath);
            
            var s = new AutoCheck.Core.Script(GetSampleFile("output_single_2.yaml"));
            Assert.AreEqual(logFilePath, Core.Utils.PathToCurrentOS(s.LogFiles.FirstOrDefault()));
            Assert.IsTrue(File.Exists(logFilePath));        
            Assert.IsTrue(File.ReadAllText(logFilePath).Equals(s.Output.GetLog().LastOrDefault().ToJson()));                
        }

        [Test, Category("Output"), Category("Local"), Category("Text")]
        public void Output_BATCH_FILE_1()
        {         
            var path = Path.Combine(LogRootFolder, "script", "output", "output_batch_1_yaml");  
            if(Directory.Exists(path)) Directory.Delete(path, true);
            
            var logs = new string[]{
                Path.Combine(path, "OUTPUT BATCH 1_Student Name 1.log"),
                Path.Combine(path, "OUTPUT BATCH 1_Student Name 2.log"),
                Path.Combine(path, "OUTPUT BATCH 1_Student Name 3.log")
            };

            if(Directory.Exists(path)){     
                foreach(var l in logs)
                    Assert.IsFalse(File.Exists(l));
            }
            
            var i = 0;
            var s = new AutoCheck.Core.Script(GetSampleFile("output_batch_1.yaml"));
            
            Assert.AreEqual(3, Directory.GetFiles(path).Length);

            foreach(var l in logs){
                var logFile = Core.Utils.PathToCurrentOS(l);

                Assert.IsTrue(File.Exists(logFile));
                Assert.IsTrue(File.ReadAllText(logFile).Equals(s.Output.GetLog()[i++].ToText()));
            }
        } 
        

        [Test, Category("Output"), Category("Local"), Category("Text")]
        public void Output_BATCH_FILE_2()
        {   
            var path = Path.Combine(LogRootFolder, "script", "output", "output_batch_2_yaml");  
            if(Directory.Exists(path)) Directory.Delete(path, true);                                     

            var i = 0;
            var s = new AutoCheck.Core.Script(GetSampleFile("output_batch_2.yaml"));

            Assert.AreEqual(3, Directory.GetFiles(path).Length);

            foreach(var l in s.LogFiles){
                var logFile = Core.Utils.PathToCurrentOS(l);
                
                Assert.IsTrue(File.Exists(logFile));
                Assert.IsTrue(File.ReadAllText(logFile).Equals(s.Output.GetLog()[i++].ToText()));
            }
        } 

        [Test, Category("Output"), Category("Local"), Category("Json")]
        public void Output_BATCH_FILE_3()
        {         
            var path = Path.Combine(LogRootFolder, "script", "output", "output_batch_3_yaml");  
            if(Directory.Exists(path)) Directory.Delete(path, true);
            
            var logs = new string[]{
                Path.Combine(path, "OUTPUT BATCH 3_Student Name 1.log"),
                Path.Combine(path, "OUTPUT BATCH 3_Student Name 2.log"),
                Path.Combine(path, "OUTPUT BATCH 3_Student Name 3.log")
            };

            if(Directory.Exists(path)){     
                foreach(var l in logs)
                    Assert.IsFalse(File.Exists(Core.Utils.PathToCurrentOS(l)));
            }
            
            var i = 0;
            var s = new AutoCheck.Core.Script(GetSampleFile("output_batch_3.yaml"));
            
            Assert.AreEqual(3, Directory.GetFiles(path).Length);

            foreach(var l in logs){
                Assert.IsTrue(File.Exists(l));        
                Assert.IsTrue(File.ReadAllText(l).Equals(s.Output.GetLog()[i++].ToJson()));
            }
        } 

        [Test, Category("Output"), Category("Local"), Category("Text")]
        public void Output_BATCH_FILE_4()
        {         
            var path = Path.Combine(LogRootFolder, "script", "output", "output_batch_4_yaml");  
            if(Directory.Exists(path)) Directory.Delete(path, true);
            
            var logs = new string[]{
                Path.Combine(path, "OUTPUT BATCH 3_Student Name 1.log"),
                Path.Combine(path, "OUTPUT BATCH 3_Student Name 2.log"),
                Path.Combine(path, "OUTPUT BATCH 3_Student Name 3.log")
            };

            if(Directory.Exists(path)){     
                foreach(var l in logs)
                    Assert.IsFalse(File.Exists(Core.Utils.PathToCurrentOS(l)));
            }
            
            var i = 0;
            var s = new AutoCheck.Core.Script(GetSampleFile("output_batch_4.yaml"));
            
            Assert.AreEqual(3, Directory.GetFiles(path).Length);

            foreach(var l in logs){
                Assert.IsTrue(File.Exists(l));        
                Assert.IsTrue(File.ReadAllText(l).Equals(s.Output.GetLog()[i++].ToText()));
            }
        } 
    }
}