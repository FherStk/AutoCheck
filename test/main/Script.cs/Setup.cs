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
    public class Setup : Test
    {
        public Setup(): base("script"){
        }

        [Test, Category("Setup"), Category("Local")]
        public void Setup_ECHO()
        { 
            var dest =  Path.Combine(Path.GetDirectoryName(TempScriptFolder), "test1"); //the script will use this folder, so no random path can be used
            var dest1 = Path.Combine(dest, "folder1");
            var dest2 = Path.Combine(dest, "folder2");

            if(!Directory.Exists(dest1)) Directory.CreateDirectory(dest1);
            if(!Directory.Exists(dest2)) Directory.CreateDirectory(dest2);         

            dest1 = Path.GetFileName(dest1);
            dest2 = Path.GetFileName(dest2);
            
            var s = new AutoCheck.Core.Script(GetSampleFile("setup_ok1.yaml"));
            Assert.AreEqual($"Running script setup_ok1 (v1.0.0.0):\r\n   Echo for setup execution over folder1\r\n\r\n   Echo for setup execution over folder2\r\n\r\nRunning on batch mode for folder1:\r\n   Echo for pre execution over folder1\r\n\r\n   Echo for body execution over folder1\r\n\r\n   Echo for post execution over folder1\r\n\r\n\r\n   Echo for teardown execution over folder1\r\n\r\n   Echo for teardown execution over folder2\r\n\r\nRunning script setup_ok1 (v1.0.0.0):\r\n   Echo for setup execution over folder1\r\n\r\n   Echo for setup execution over folder2\r\n\r\nRunning on batch mode for folder2:\r\n   Echo for pre execution over folder2\r\n\r\n   Echo for body execution over folder2\r\n\r\n   Echo for post execution over folder2\r\n\r\n\r\n   Echo for teardown execution over folder1\r\n\r\n   Echo for teardown execution over folder2", s.Output.ToString());            
        }       
    }
}