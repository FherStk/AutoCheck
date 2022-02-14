/*
    Copyright © 2022 Fernando Porrino Serrano
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
using System.IO;
using NUnit.Framework;
using AutoCheck.Core.Exceptions;

namespace AutoCheck.Test.Checkers
{
    [Parallelizable(ParallelScope.All)]    
    public class SourceCode : Test
    {
        protected override void CleanUp(){
            //Clean temp files
            var dir = Path.Combine(SamplesScriptFolder, "temp");
            if(Directory.Exists(dir)) Directory.Delete(dir, true);            
        }

        [Test]
        public void Constructor_Throws_ArgumentNullException()
        {             
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.CopyDetectors.SourceCode(0,string.Empty));            
        }   

        [Test]
        [TestCase(1.01f, "*.java")]
        [TestCase(-0.01f, "*.java")]
        public void Constructor_Throws_ArgumentOutOfRangeException(float threshold, string filepattern)
        {             
            Assert.Throws<ArgumentOutOfRangeException>(() => new AutoCheck.Core.CopyDetectors.SourceCode(threshold, filepattern));            
        }   

        [Test]
        [TestCase(0f, "*.SourceCode")]
        [TestCase(1f, "*")]
        public void Constructor_DoesNotThrow(float threshold, string filepattern)
        {             
            Assert.DoesNotThrow(() => new AutoCheck.Core.CopyDetectors.SourceCode(threshold, filepattern));            
        }

        [Test]
        public void Load_KO()
        {     
            var dest1 =  Path.Combine(SamplesScriptFolder, "temp", "test1"); 
            var dest2 =  Path.Combine(SamplesScriptFolder, "temp", "test2"); 
            if(!Directory.Exists(dest1)) Directory.CreateDirectory(dest1);
            if(!Directory.Exists(dest2)) Directory.CreateDirectory(dest2);
            
            var file1 = GetSampleFile(dest1, "sample1.java");
            var file2 = GetSampleFile(dest2, "sample2.java");
            File.Copy(GetSampleFile("sample1.java"), file1);
            File.Copy(GetSampleFile("sample2.java"), file2);
            
            using(var cd = new AutoCheck.Core.CopyDetectors.SourceCode(0, "*.fake"))
            {                         
                Assert.Throws<ArgumentNullException>(() => cd.Load(string.Empty));
                Assert.Throws<DirectoryNotFoundException>(() => cd.Load(_FAKE));
                Assert.Throws<ArgumentInvalidException>(() => cd.Load(dest1));
            }

            using(var cd = new AutoCheck.Core.CopyDetectors.SourceCode(0, "*.java"))
            {                         
                Assert.DoesNotThrow(() => cd.Load(file1));
                Assert.DoesNotThrow(() => cd.Load(file2));
            }
        } 

        [Test]
        public void Load_OK()
        {                
            var dest1 =  Path.Combine(SamplesScriptFolder, "temp", "test2", "folder1"); 
            if(!Directory.Exists(dest1)) Directory.CreateDirectory(dest1);

            var dest2 =  Path.Combine(SamplesScriptFolder, "temp", "test2", "folder2"); 
            if(!Directory.Exists(dest2)) Directory.CreateDirectory(dest2);
            
            var file1 = GetSampleFile(dest1, "sample1.java");
            var file2 = GetSampleFile(dest2, "sample2.java");
            File.Copy(GetSampleFile("sample1.java"), file1);
            File.Copy(GetSampleFile("sample2.java"), file2);

            using(var cd = new AutoCheck.Core.CopyDetectors.SourceCode(0, "*.java"))
            {                         
                Assert.DoesNotThrow(() => cd.Load(file1));
                Assert.DoesNotThrow(() => cd.Load(file2));
            }
        }

        [Test]
        public void Compare()
        {     
            var dest1 =  Path.Combine(SamplesScriptFolder, "temp", "test3", "folder1"); 
            if(!Directory.Exists(dest1)) Directory.CreateDirectory(dest1);

            var dest2 =  Path.Combine(SamplesScriptFolder, "temp", "test3", "folder2"); 
            if(!Directory.Exists(dest2)) Directory.CreateDirectory(dest2);
            
            var file1 = GetSampleFile(dest1, "sample1.java");
            var file2 = GetSampleFile(dest2, "sample1.java");
            File.Copy(GetSampleFile("sample1.java"), file1);
            File.Copy(GetSampleFile("sample1.java"), file2);    

            using(var cd = new AutoCheck.Core.CopyDetectors.SourceCode(0, "sample1.java"))
            {                         
                Assert.DoesNotThrow(() => cd.Load(file1));
                Assert.DoesNotThrow(() => cd.Load(file2));
                Assert.DoesNotThrow(() => cd.Compare());
            }
        } 

        [Test]
        public void CopyDetected_SameFiles()
        {     
            var dest1 =  Path.Combine(SamplesScriptFolder, "temp", "test4", "folder1"); 
            if(!Directory.Exists(dest1)) Directory.CreateDirectory(dest1);

            var dest2 =  Path.Combine(SamplesScriptFolder, "temp", "test4", "folder2"); 
            if(!Directory.Exists(dest2)) Directory.CreateDirectory(dest2);
            
            var file1 = GetSampleFile(dest1, "sample1.java");
            var file2 = GetSampleFile(dest2, "sample2.java");
            File.Copy(GetSampleFile("sample1.java"), file1);
            File.Copy(GetSampleFile("sample1.java"), file2);

            using(var cd = new AutoCheck.Core.CopyDetectors.SourceCode(1, "sample1.java"))
            {                                        
                Assert.DoesNotThrow(() => cd.Load(file1));                
                Assert.DoesNotThrow(() => cd.Load(file2));                
                Assert.DoesNotThrow(() => cd.Compare());

                Assert.Throws<ArgumentNullException>(() => cd.CopyDetected(string.Empty));
                Assert.Throws<ArgumentInvalidException>(() => cd.CopyDetected(_FAKE));

                Assert.IsTrue(cd.CopyDetected(dest1));
                Assert.IsTrue(cd.CopyDetected(dest2));                
            }
        } 

        [Test]
        public void CopyDetected_DifferentFiles()
        {     
            var dest1 =  Path.Combine(SamplesScriptFolder, "temp", "test5", "folder1"); 
            if(!Directory.Exists(dest1)) Directory.CreateDirectory(dest1);

            var dest2 =  Path.Combine(SamplesScriptFolder, "temp", "test5", "folder2"); 
            if(!Directory.Exists(dest2)) Directory.CreateDirectory(dest2);
            
            var file1 = GetSampleFile(dest1, "sample1.java");
            var file2 = GetSampleFile(dest2, "sample2.java");
            File.Copy(GetSampleFile("sample1.java"), file1);
            File.Copy(GetSampleFile("sample2.java"), file2);

            using(var cd = new AutoCheck.Core.CopyDetectors.SourceCode(0.9f, "*.java"))
            {                                                       
                Assert.DoesNotThrow(() => cd.Load(file1));                
                Assert.DoesNotThrow(() => cd.Load(file2));  

                Assert.DoesNotThrow(() => cd.Compare());

                Assert.IsFalse(cd.CopyDetected(dest1));
                Assert.IsFalse(cd.CopyDetected(dest2));                
            }

            using(var cd = new AutoCheck.Core.CopyDetectors.SourceCode(0.8f, "*.java"))
            {                                                       
                Assert.DoesNotThrow(() => cd.Load(file1));                
                Assert.DoesNotThrow(() => cd.Load(file2));  

                Assert.DoesNotThrow(() => cd.Compare());

                Assert.IsTrue(cd.CopyDetected(dest1));
                Assert.IsTrue(cd.CopyDetected(dest2));                
            }
        } 

         [Test]
        public void GetDetails()
        {     
            var dest1 =  Path.Combine(SamplesScriptFolder, "temp", "test6", "folder1"); 
            if(!Directory.Exists(dest1)) Directory.CreateDirectory(dest1);

            var dest2 =  Path.Combine(SamplesScriptFolder, "temp", "test6", "folder2"); 
            if(!Directory.Exists(dest2)) Directory.CreateDirectory(dest2);
            
            var file1 = GetSampleFile(dest1, "sample1.java");
            var file2 = GetSampleFile(dest2, "sample2.java");
            File.Copy(GetSampleFile("sample1.java"), file1);
            File.Copy(GetSampleFile("sample2.java"), file2);

            using(var cd = new AutoCheck.Core.CopyDetectors.SourceCode(0.6f, "*.java"))
            {                                                       
                Assert.DoesNotThrow(() => cd.Load(file1));                
                Assert.DoesNotThrow(() => cd.Load(file2));  

                Assert.DoesNotThrow(() => cd.Compare());

                var res = cd.GetDetails(dest1); 
                Assert.AreEqual(dest1, res.Folder);               
                Assert.AreEqual(dest2, res.matches[0].Folder);

                Assert.AreEqual(Path.GetFileName(file1), res.File);               
                Assert.AreEqual(Path.GetFileName(file2), res.matches[0].File);
                
                Assert.AreEqual(0.81099999f, res.matches[0].Match);
            }
        }                    
    }
}