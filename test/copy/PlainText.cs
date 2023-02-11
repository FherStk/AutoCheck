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

using System;
using System.IO;
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
        public void Constructor_Throws_ArgumentNullException()
        {             
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Core.CopyDetectors.PlainText(0,string.Empty));            
        }   

        [Test]
        [TestCase(1.01f, "*.txt")]
        [TestCase(-0.01f, "*.txt")]
        public void Constructor_Throws_ArgumentOutOfRangeException(float threshold, string filepattern)
        {             
            Assert.Throws<ArgumentOutOfRangeException>(() => new AutoCheck.Core.CopyDetectors.PlainText(threshold, filepattern));            
        }   

        [Test]
        [TestCase(0f, "*.txt")]
        [TestCase(1f, "*")]
        public void Constructor_DoesNotThrow(float threshold, string filepattern)
        {             
            Assert.DoesNotThrow(() => new AutoCheck.Core.CopyDetectors.PlainText(threshold, filepattern));            
        }

        [Test]
        [TestCase(0f, "*.fake", "")]
        public void Load_Throws_ArgumentNullException(float threshold, string filepattern, string filePath)
        {                     
            using(var cd = new AutoCheck.Core.CopyDetectors.PlainText(0, "*.fake"))
            {                         
                Assert.Throws<ArgumentNullException>(() => cd.Load(filePath));                
            }
        }

        [Test]
        [TestCase(0f, "*.fake", _FAKE)]
        public void Load_Throws_DirectoryNotFoundException(float threshold, string filepattern, string filePath)
        {                     
            using(var cd = new AutoCheck.Core.CopyDetectors.PlainText(0, "*.fake"))
            {                         
                Assert.Throws<DirectoryNotFoundException>(() => cd.Load(filePath));                
            }
        }

        [Test]
        [TestCase(0, "*.fake", "lorem1.txt")]
        public void Load_Throws_ArgumentInvalidException(float threshold, string filepattern, string sampleFile)
        {   
            var dest =  Path.Combine(SamplesScriptFolder, "temp", Guid.NewGuid().ToString());    
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);

            var file = GetSampleFile(dest, sampleFile);
            File.Copy(GetSampleFile(sampleFile), file);

            using(var cd = new AutoCheck.Core.CopyDetectors.PlainText(threshold, filepattern))
            {                         
                Assert.Throws<ArgumentInvalidException>(() => cd.Load(dest));
            }
        }

        [Test]
        [TestCase(0, "*.txt", "lorem1.txt")]
        [TestCase(0, "*.txt", "lorem2.txt")]
        public void Load_DoesNotThrow(float threshold, string filepattern, string sampleFile)
        {   
            var dest =  Path.Combine(SamplesScriptFolder, "temp", Guid.NewGuid().ToString());    
            if(!Directory.Exists(dest)) Directory.CreateDirectory(dest);

            var file = GetSampleFile(dest, sampleFile);
            File.Copy(GetSampleFile(sampleFile), file);

            using(var cd = new AutoCheck.Core.CopyDetectors.PlainText(threshold, filepattern))
            {                         
                Assert.DoesNotThrow(() => cd.Load(dest));
            }
        }

        [Test]
        [TestCase(0, "*.txt", "lorem1.txt", "lorem1.txt")]
        public void Compare_DoesNotThrow(float threshold, string filepattern, string sampleFileLeft, string sampleFileRight)
        {
            var folder = Guid.NewGuid().ToString();     
            var dest1 =  Path.Combine(SamplesScriptFolder, "temp", folder, "folder1"); 
            if(!Directory.Exists(dest1)) Directory.CreateDirectory(dest1);

            var dest2 =  Path.Combine(SamplesScriptFolder, "temp", folder, "folder2"); 
            if(!Directory.Exists(dest2)) Directory.CreateDirectory(dest2);
            
            var file1 = GetSampleFile(dest1, sampleFileLeft);
            var file2 = GetSampleFile(dest2, sampleFileRight);
            File.Copy(GetSampleFile(sampleFileLeft), file1);
            File.Copy(GetSampleFile(sampleFileRight), file2);            

            using(var cd = new AutoCheck.Core.CopyDetectors.PlainText(threshold, filepattern))
            {                         
                Assert.DoesNotThrow(() => cd.Compare());

                Assert.DoesNotThrow(() => cd.Load(file1));
                Assert.DoesNotThrow(() => cd.Compare());

                Assert.DoesNotThrow(() => cd.Load(file2));
                Assert.DoesNotThrow(() => cd.Compare());
            }
        } 

        [Test]
        [TestCase(0, "*.txt", "lorem1.txt", "lorem1.txt", true)]
        [TestCase(0, "*.txt", "lorem1.txt", "lorem2.txt", true)]
        [TestCase(0.5f, "*.txt", "lorem1.txt", "lorem2.txt", true)]
        [TestCase(0.6f, "*.txt", "lorem1.txt", "lorem2.txt", false)]
        public void CopyDetected_DoesNotThrow(float threshold, string filepattern, string sampleFileLeft, string sampleFileRight, bool expected)
        { 
            var folder = Guid.NewGuid().ToString();     
            var dest1 =  Path.Combine(SamplesScriptFolder, "temp", folder, "folder1"); 
            if(!Directory.Exists(dest1)) Directory.CreateDirectory(dest1);

            var dest2 =  Path.Combine(SamplesScriptFolder, "temp", folder, "folder2"); 
            if(!Directory.Exists(dest2)) Directory.CreateDirectory(dest2);
            
            var file1 = GetSampleFile(dest1, sampleFileLeft);
            var file2 = GetSampleFile(dest2, sampleFileRight);
            File.Copy(GetSampleFile(sampleFileLeft), file1);
            File.Copy(GetSampleFile(sampleFileRight), file2);            

            using(var cd = new AutoCheck.Core.CopyDetectors.PlainText(threshold, filepattern))
            {                         
                cd.Load(file1);
                cd.Load(file2);
                cd.Compare();

                //both sides must be tested
                Assert.AreEqual(expected, cd.CopyDetected(dest1));
                Assert.AreEqual(expected, cd.CopyDetected(dest2));
            }
        }   

        [Test]
        [TestCase(0.6f, "*.txt", "lorem1.txt", "lorem2.txt", ExpectedResult=0.582976282f)]
        public float GetDetails_DoesNotThrow(float threshold, string filepattern, string sampleFileLeft, string sampleFileRight)
        { 
            var folder = Guid.NewGuid().ToString();     
            var dest1 =  Path.Combine(SamplesScriptFolder, "temp", folder, "folder1"); 
            if(!Directory.Exists(dest1)) Directory.CreateDirectory(dest1);

            var dest2 =  Path.Combine(SamplesScriptFolder, "temp", folder, "folder2"); 
            if(!Directory.Exists(dest2)) Directory.CreateDirectory(dest2);
            
            var file1 = GetSampleFile(dest1, sampleFileLeft);
            var file2 = GetSampleFile(dest2, sampleFileRight);
            File.Copy(GetSampleFile(sampleFileLeft), file1);
            File.Copy(GetSampleFile(sampleFileRight), file2);            

            using(var cd = new AutoCheck.Core.CopyDetectors.PlainText(threshold, filepattern))
            {                         
                cd.Load(file1);
                cd.Load(file2);
                cd.Compare();

                var res = cd.GetDetails(dest1); 
                Assert.AreEqual(dest1, res.Folder);               
                Assert.AreEqual(dest2, res.matches[0].Folder);

                Assert.AreEqual(Path.GetFileName(file1), res.File);               
                Assert.AreEqual(Path.GetFileName(file2), res.matches[0].File);
                
                return res.matches[0].Match;
            }
        }                    
    }
}