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
using System.Collections.Concurrent;
using NUnit.Framework;
using AutoCheck.Core;
using AutoCheck.Exceptions;

namespace AutoCheck.Test.Connectors
{
    [Parallelizable(ParallelScope.All)]    
    public class RemoteShell : Core.Test
    {
        //NOTE: For Windows host running WSL -> https://stackoverflow.com/a/60198221
        //          sudo nano /etc/ssh/sshd_config
        //          PasswordAuthentication yes
        //          sudo service ssh start

        private ConcurrentDictionary<string, AutoCheck.Connectors.RemoteShell> Conn = new ConcurrentDictionary<string, AutoCheck.Connectors.RemoteShell>();

        private const string _fake = "fake";

        [SetUp]
        public void Setup() 
        {
            base.Setup("remoteShell");

            //Create a new and unique host connection for the current context (same host for all tests)
            var conn = new AutoCheck.Connectors.RemoteShell(OS.GNU, "localhost", "usuario", "usuario");
            
            //Storing the connector instance for the current context
            var added = false;
            do added = this.Conn.TryAdd(TestContext.CurrentContext.Test.ID, conn);             
            while(!added);  
        }

        [TearDown]
        public void TearDown(){
            var conn = this.Conn[TestContext.CurrentContext.Test.ID];
            conn.Dispose();
        }

        [Test]
        public void Constructor()
        {                      
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Connectors.RemoteShell(OS.WIN, string.Empty, string.Empty, string.Empty));
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Connectors.RemoteShell(OS.WIN, _fake, string.Empty, string.Empty));
            Assert.Throws<ArgumentNullException>(() => new AutoCheck.Connectors.RemoteShell(OS.WIN, _fake, _fake, string.Empty));
            Assert.DoesNotThrow(() => new AutoCheck.Connectors.RemoteShell(OS.WIN, _fake, _fake, _fake));            
        }

        [Test]
        public void TestConnection()
        {                                  
            using(var conn = new AutoCheck.Connectors.RemoteShell(OS.WIN, _fake, _fake, _fake))
                Assert.Throws<ConnectionInvalidException>(() => conn.TestConnection());

            Assert.DoesNotThrow(() => this.Conn[TestContext.CurrentContext.Test.ID].TestConnection());
        }

        [Test]
        public void GetFolder()
        {     
            var conn = this.Conn[TestContext.CurrentContext.Test.ID];
            Assert.IsNotNull(conn.GetFolder("/var/lib", "sudo", false));
            Assert.IsNotNull(conn.GetFolder("/var/lib", "sudo", true));

            Assert.IsNull(conn.GetFolder("/var", "sudo", false));
            Assert.IsNotNull(conn.GetFolder("/var", "sudo", true));
        }

        [Test]
        public void GetFile()
        {            
            var conn = this.Conn[TestContext.CurrentContext.Test.ID];
            Assert.IsNotNull(conn.GetFile("/var/lib/dpkg", "status", false));
            Assert.IsNotNull(conn.GetFile("/var/lib/dpkg", "status", true));

            Assert.IsNull(conn.GetFile("/var/lib", "status", false));
            Assert.IsNotNull(conn.GetFile("/var/lib", "status", true));
        }
        
        [Test]
        public void CountFolders()
        {            
            var conn = this.Conn[TestContext.CurrentContext.Test.ID];
            Assert.AreEqual(8, conn.CountFolders("/var/lib/snapd", false));
            Assert.AreEqual(14, conn.CountFolders("/var/lib/snapd", true));
        }

        [Test]
        public void CountFiles()
        {            
            var conn = this.Conn[TestContext.CurrentContext.Test.ID];
            Assert.AreEqual(9, conn.CountFiles("/var/lib/dpkg", false));
            Assert.AreEqual(2578, conn.CountFiles("/var/lib/dpkg", true));
        }

        [Test]
        public void RunCommand()
        {                        
            var conn = this.Conn[TestContext.CurrentContext.Test.ID];            
            var result = conn.RunCommand("ls");
            Assert.AreEqual(0, result.code);
            Assert.IsNotNull(result.response);

            result = conn.RunCommand("fake");
            Assert.AreNotEqual(0, result.code);
            Assert.IsNotNull(result.response);              
        }
    }
}