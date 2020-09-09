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
using NUnit.Framework;
using AutoCheck.Core;

namespace AutoCheck.Test.Checkers
{
    //TODO: Remove this file when migration from C# to YAML has been completed (this includes checker & connector mergind and also connector simplification).

    // [Parallelizable(ParallelScope.All)]    
    // public class LocalShell : Core.Test
    // {
    //     //TODO: Check the exact errors messages, otherwise cannot be assured its amount and content (do not check only amount, the exact message output is needed for debug) 
        
    //     [Test]
    //     public void Constructor()
    //     {            
    //         Assert.DoesNotThrow(() => new AutoCheck.Checkers.LocalShell());
    //     }   

    //     [Test]
    //     public void CheckIfFolderExists()
    //     {    
    //         using(var conn = new AutoCheck.Checkers.LocalShell())
    //         {                               
    //             Assert.AreNotEqual(new List<string>(),conn.CheckIfFolderExists(this.SamplesScriptFolder, "testSubFolder11", false));
    //             Assert.AreEqual(new List<string>(), conn.CheckIfFolderExists(this.SamplesScriptFolder, "testSubFolder11", true));
    //             Assert.AreEqual(new List<string>(), conn.CheckIfFolderExists(this.SamplesScriptFolder, "testFolder1", false));                
    //             Assert.AreEqual(new List<string>(), conn.CheckIfFolderExists(this.SamplesScriptFolder, "testFolder1", true));
    //         }
    //     } 

    //     [Test]
    //     public void CheckIfFileExists()
    //     {    
    //         using(var conn = new AutoCheck.Checkers.LocalShell())
    //         {                               
    //             Assert.AreNotEqual(new List<string>(), conn.CheckIfFileExists(this.SamplesScriptFolder, "testFile11.txt", false));
    //             Assert.AreEqual(new List<string>(), conn.CheckIfFileExists(this.SamplesScriptFolder, "testFile11.txt", true));
    //             Assert.AreNotEqual(new List<string>(), conn.CheckIfFileExists(this.SamplesScriptFolder, "testFile11.txt", false));                
    //             Assert.AreEqual(new List<string>(), conn.CheckIfFileExists(this.SamplesScriptFolder, "testFile11.txt", true));
    //         }
    //     }

    //     [Test]
    //     public void CheckIfFoldersMatchesAmount()
    //     {    
    //         using(var conn = new AutoCheck.Checkers.LocalShell())
    //         {                               
    //             Assert.AreEqual(new List<string>(), conn.CheckIfFoldersMatchesAmount(this.SamplesScriptFolder, 2, false));
    //             Assert.AreNotEqual(new List<string>(), conn.CheckIfFoldersMatchesAmount(this.SamplesScriptFolder, 2, false, Operator.GREATER));
    //             Assert.AreEqual(new List<string>(), conn.CheckIfFoldersMatchesAmount(this.SamplesScriptFolder, 2, false, Operator.GREATEREQUALS));
    //             Assert.AreNotEqual(new List<string>(), conn.CheckIfFoldersMatchesAmount(this.SamplesScriptFolder, 2, false, Operator.LOWER));
    //             Assert.AreEqual(new List<string>(), conn.CheckIfFoldersMatchesAmount(this.SamplesScriptFolder, 2, false, Operator.LOWEREQUALS));
                
    //             Assert.AreEqual(new List<string>(), conn.CheckIfFoldersMatchesAmount(this.SamplesScriptFolder, 6, true));
    //             Assert.AreNotEqual(new List<string>(), conn.CheckIfFoldersMatchesAmount(this.SamplesScriptFolder, 6, true, Operator.GREATER));
    //             Assert.AreEqual(new List<string>(), conn.CheckIfFoldersMatchesAmount(this.SamplesScriptFolder, 6, true, Operator.GREATEREQUALS));
    //             Assert.AreNotEqual(new List<string>(), conn.CheckIfFoldersMatchesAmount(this.SamplesScriptFolder, 6, true, Operator.LOWER));
    //             Assert.AreEqual(new List<string>(), conn.CheckIfFoldersMatchesAmount(this.SamplesScriptFolder, 6, true, Operator.LOWEREQUALS));

    //         }
    //     } 

    //     [Test]
    //     public void CheckIfFilesMatchesAmount()
    //     {    
    //         using(var conn = new AutoCheck.Checkers.LocalShell())
    //         {                               
    //             Assert.AreEqual(new List<string>(), conn.CheckIfFilesMatchesAmount(this.SamplesScriptFolder, 0, false));
    //             Assert.AreNotEqual(new List<string>(), conn.CheckIfFilesMatchesAmount(this.SamplesScriptFolder, 0, false, Operator.GREATER));
    //             Assert.AreEqual(new List<string>(), conn.CheckIfFilesMatchesAmount(this.SamplesScriptFolder, 0, false, Operator.GREATEREQUALS));
    //             Assert.AreNotEqual(new List<string>(), conn.CheckIfFilesMatchesAmount(this.SamplesScriptFolder, 0, false, Operator.LOWER));
    //             Assert.AreEqual(new List<string>(), conn.CheckIfFilesMatchesAmount(this.SamplesScriptFolder, 0, false, Operator.LOWEREQUALS));
                
    //             Assert.AreEqual(new List<string>(), conn.CheckIfFilesMatchesAmount(this.SamplesScriptFolder, 2, true));
    //             Assert.AreNotEqual(new List<string>(), conn.CheckIfFilesMatchesAmount(this.SamplesScriptFolder, 2, true, Operator.GREATER));
    //             Assert.AreEqual(new List<string>(), conn.CheckIfFilesMatchesAmount(this.SamplesScriptFolder, 2, true, Operator.GREATEREQUALS));
    //             Assert.AreNotEqual(new List<string>(), conn.CheckIfFilesMatchesAmount(this.SamplesScriptFolder, 2, true, Operator.LOWER));
    //             Assert.AreEqual(new List<string>(), conn.CheckIfFilesMatchesAmount(this.SamplesScriptFolder, 2, true, Operator.LOWEREQUALS));

    //         }
    //     } 

    //     [Test]
    //     public void CheckIfCommandMatchesResult()
    //     {    
    //         using(var conn = new AutoCheck.Checkers.LocalShell())
    //         {         
    //             string command = string.Format("ls '{0}'", this.SamplesScriptFolder);
    //             if(conn.Connector.CurrentOS == OS.WIN) 
    //                 command = string.Format("wsl {0}", command.Replace("c:", "\\mnt\\c", true, null).Replace("\\", "/"));
                
    //             //TODO: on windows, test if the  wsl is installed because wsl -e will be used to test linux commands and windows ones in one step if don't, throw an exception                      
    //             Assert.AreEqual(new List<string>(), conn.CheckIfCommandMatchesResult(command, "testFolder1\ntestFolder2\n\r\n"));
    //         }
    //     }                                    
    // }
}