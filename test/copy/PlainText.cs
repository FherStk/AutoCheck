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
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;
using AutoCheck.Core.Exceptions;

namespace AutoCheck.Test.Checkers
{
    [Parallelizable(ParallelScope.All)]    
    public class PlainText : Test
    {
        protected override void CleanUp(){
            //Clean temp files
            var dir = Path.Combine(SamplesScriptFolder, "temp");
            if(Directory.Exists(dir)) Directory.Delete(dir, true);            
        }

        [Test]
        public void Constructor()
        {             
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.CopyDetectors.PlainText(0,string.Empty));
            Assert.Throws<ArgumentOutOfRangeException>(() => new AutoCheck.Core.CopyDetectors.PlainText(1.01f, "*.txt"));
            Assert.Throws<ArgumentOutOfRangeException>(() => new AutoCheck.Core.CopyDetectors.PlainText(-0.01f, "*.txt"));
            Assert.DoesNotThrow(() => new AutoCheck.Core.CopyDetectors.PlainText(0, "*.txt"));
            Assert.DoesNotThrow(() => new AutoCheck.Core.CopyDetectors.PlainText(1, "*"));            
        }   

        [Test]
        public void Load_KO()
        {     
            var dest =  Path.Combine(SamplesScriptFolder, "temp", "test1"); 
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);
            
            var file1 = GetSampleFile(dest, "sample1.txt");
            var file2 = GetSampleFile(dest, "sample2.txt");
            File.Copy(GetSampleFile("lorem1.txt"), file1);
            File.Copy(GetSampleFile("lorem1.txt"), file2);
            
            using(var cd = new AutoCheck.Core.CopyDetectors.PlainText(0, "*.fake"))
            {                         
                Assert.Throws<ArgumentNullException>(() => cd.Load(string.Empty));
                Assert.Throws<DirectoryNotFoundException>(() => cd.Load(_FAKE));
                Assert.Throws<ArgumentInvalidException>(() => cd.Load(dest));
            }

            using(var cd = new AutoCheck.Core.CopyDetectors.PlainText(0, "*.txt"))
            {                         
                Assert.DoesNotThrow(() => cd.Load(file1));
                Assert.Throws<ArgumentInvalidException>(() => cd.Load(file2));
            }
        } 

         [Test]
        public void Load_OK()
        {                
            var dest1 =  Path.Combine(SamplesScriptFolder, "temp", "test2", "folder1"); 
            if(!Directory.Exists(dest1)) Directory.CreateDirectory(dest1);

            var dest2 =  Path.Combine(SamplesScriptFolder, "temp", "test2", "folder2"); 
            if(!Directory.Exists(dest2)) Directory.CreateDirectory(dest2);
            
            var file1 = GetSampleFile(dest1, "sample1.txt");
            var file2 = GetSampleFile(dest2, "sample2.txt");
            File.Copy(GetSampleFile("lorem1.txt"), file1);
            File.Copy(GetSampleFile("lorem1.txt"), file2);            

            using(var cd = new AutoCheck.Core.CopyDetectors.PlainText(0, "*.txt"))
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
            
            var file1 = GetSampleFile(dest1, "sample1.txt");
            var file2 = GetSampleFile(dest2, "sample2.txt");
            File.Copy(GetSampleFile("lorem1.txt"), file1);
            File.Copy(GetSampleFile("lorem1.txt"), file2);            

            using(var cd = new AutoCheck.Core.CopyDetectors.PlainText(0, "*.txt"))
            {                         
                Assert.DoesNotThrow(() => cd.Compare());

                Assert.DoesNotThrow(() => cd.Load(file1));
                Assert.DoesNotThrow(() => cd.Compare());

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
            
            var file1 = GetSampleFile(dest1, "sample1.txt");
            var file2 = GetSampleFile(dest2, "sample2.txt");
            File.Copy(GetSampleFile("lorem1.txt"), file1);
            File.Copy(GetSampleFile("lorem1.txt"), file2);            

            using(var cd = new AutoCheck.Core.CopyDetectors.PlainText(1, "*.txt"))
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
            
            var file1 = GetSampleFile(dest1, "sample1.txt");
            var file2 = GetSampleFile(dest2, "sample2.txt");
            File.Copy(GetSampleFile("lorem1.txt"), file1);
            File.Copy(GetSampleFile("lorem2.txt"), file2);            

            using(var cd = new AutoCheck.Core.CopyDetectors.PlainText(0.6f, "*.txt"))
            {                                                       
                Assert.DoesNotThrow(() => cd.Load(file1));                
                Assert.DoesNotThrow(() => cd.Load(file2));  

                Assert.DoesNotThrow(() => cd.Compare());

                Assert.IsFalse(cd.CopyDetected(dest1));
                Assert.IsFalse(cd.CopyDetected(dest2));                
            }

            using(var cd = new AutoCheck.Core.CopyDetectors.PlainText(0.5f, "*.txt"))
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
            
            var file1 = GetSampleFile(dest1, "sample1.txt");
            var file2 = GetSampleFile(dest2, "sample2.txt");
            File.Copy(GetSampleFile("lorem1.txt"), file1);
            File.Copy(GetSampleFile("lorem2.txt"), file2);            

            using(var cd = new AutoCheck.Core.CopyDetectors.PlainText(0.6f, "*.txt"))
            {                                                       
                Assert.DoesNotThrow(() => cd.Load(file1));                
                Assert.DoesNotThrow(() => cd.Load(file2));  

                Assert.DoesNotThrow(() => cd.Compare());

                var res = cd.GetDetails(dest1); 
                Assert.AreEqual(dest1, res.folder);               
                Assert.AreEqual(dest2, res.matches[0].folder);

                Assert.AreEqual(Path.GetFileName(file1), res.file);               
                Assert.AreEqual(Path.GetFileName(file2), res.matches[0].file);
                
                Assert.AreEqual(0.589857578f, res.matches[0].match);
            }
        }                    
    }
}