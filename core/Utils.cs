using System;
using Npgsql;
using System.IO;
using System.Net;
using System.Xml;
using ToolBox.Bridge;
using ToolBox.Platform;
using ToolBox.Notification;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace AutomatedAssignmentValidator{
    class Utils{
        #region private
            private static INotificationSystem _notificationSystem { get; set; }
            private static IBridgeSystem _bridgeSystem { get; set; }
            private static ShellConfigurator _shell { get; set; }                                  
            private static void ExtractZipFile(string zipPath, string outFolder, string password = null) {
                //source:https://github.com/icsharpcode/SharpZipLib/wiki/Unpack-a-Zip-with-full-control-over-the-operation
                using(Stream fsInput = File.OpenRead(zipPath)){ 
                    using(ZipFile zf = new ZipFile(fsInput)){
                        
                        if (!String.IsNullOrEmpty(password)) {
                            // AES encrypted entries are handled automatically
                            zf.Password = password;
                        }

                        foreach (ZipEntry zipEntry in zf) {
                            if (!zipEntry.IsFile) {
                                // Ignore directories
                                continue;
                            }

                            String entryFileName = zipEntry.Name;
                            // to remove the folder from the entry:
                            //entryFileName = Path.GetFileName(entryFileName);
                            // Optionally match entrynames against a selection list here
                            // to skip as desired.
                            // The unpacked length is available in the zipEntry.Size property.

                            // Manipulate the output filename here as desired.
                            var fullZipToPath = Path.Combine(outFolder, entryFileName);
                            var directoryName = Path.GetDirectoryName(fullZipToPath);
                            if (directoryName.Length > 0) {
                                Directory.CreateDirectory(directoryName);
                            }

                            // 4K is optimum
                            var buffer = new byte[4096];

                            // Unzip file in buffered chunks. This is just as fast as unpacking
                            // to a buffer the full size of the file, but does not waste memory.
                            // The "using" will close the stream even if an exception occurs.
                            using(var zipStream = zf.GetInputStream(zipEntry))
                            using (Stream fsOutput = File.Create(fullZipToPath)) {
                                StreamUtils.Copy(zipStream, fsOutput , buffer);
                            }
                        }
                    }
                }
            }
        #endregion
        #region public
            public static ShellConfigurator Shell { 
                get{ 
                    if(_shell == null){
                        //https://github.com/deinsoftware/toolbox#system
                        //This is used in order to launch terminal commands on diferent OS systems (Windows + Linux + Mac)
                        _notificationSystem = NotificationSystem.Default;
                        switch (OS.GetCurrent())
                        {
                            case "win":
                                _bridgeSystem = BridgeSystem.Bat;
                                break;
                            case "mac":
                            case "gnu":
                                _bridgeSystem = BridgeSystem.Bash;
                                break;
                        }
                        _shell = new ShellConfigurator(_bridgeSystem, _notificationSystem);                    
                    }
                    
                    return _shell;
                }
            }        
            public static void Write(string text, ConsoleColor color = ConsoleColor.Gray){
                WriteColor(false, text, color);
            }  
            public static void WriteLine(string text, ConsoleColor color = ConsoleColor.Gray){
                WriteColor(true, text, color);
            }
            public static void WriteColor(bool newLine, string text, ConsoleColor color){
                Console.ResetColor();   
                Console.ForegroundColor = color;     
                if(newLine) Console.WriteLine(text);
                else Console.Write(text);
                Console.ResetColor();   
            }  
            public static void BreakLine(int lines = 1){
                for(int i=0; i < lines; i++)
                    WriteLine("");
            }               
            public static string MoodleFolderToStudentName(string folder){            
                string studentFolder = Path.GetFileName(folder);
                
                //Moodle assignments download uses "_" in order to separate the student name from the assignment ID
                if(!studentFolder.Contains(" ")) return null;
                else return studentFolder.Substring(0, studentFolder.IndexOf("_"));            
            }  
            public static string FolderNameToDataBase(string folder, string prefix = null){
                string[] temp = Path.GetFileNameWithoutExtension(folder).Split("_"); 
                if(temp.Length < 3) throw new Exception("The given folder does not follow the needed naming convention.");
                else return string.Format("{0}_{1}_{2}", (prefix == null ? temp[0] : prefix), temp[1], temp[2]); 
            }    
            public static bool CreateDataBase(string server, string database, string sqlDump)
            {
                Write("Creating database for the student ");
                Write(database.Substring(database.IndexOf("_")+1).Replace("_", " "), ConsoleColor.DarkYellow);
                Write(": ");

                string defaultWinPath = "C:\\Program Files\\PostgreSQL\\10\\bin";   
                string cmdPassword = "PGPASSWORD=postgres";
                string cmdCreate = string.Format("createdb -h {0} -U postgres -T template0 {1}", server, database);
                string cmdRestore = string.Format("psql -h {0} -U postgres {1} < {2}", server, database, sqlDump);            
                Response resp = null;
                List<string> errors = new List<string>();

                switch (OS.GetCurrent())
                {
                    //TODO: this must be correctly configured as a path wehn a terminal session begins
                    //Once path is ok on windows and unix the almost same code will be used.
                    case "win":                  
                        resp = Shell.Term(string.Format("SET \"{0}\" && {1}", cmdPassword, cmdCreate), Output.Hidden, defaultWinPath);
                        if(resp.code > 0) errors.Add(resp.stderr.Replace("\n", ""));

                        resp = Shell.Term(string.Format("SET \"{0}\" && {1}", cmdPassword, cmdRestore), Output.Hidden, defaultWinPath);
                        if(resp.code > 0) errors.Add(resp.stderr.Replace("\n", ""));
                        
                        break;

                    case "mac":                
                    case "gnu":
                        resp = Shell.Term(string.Format("{0} {1}", cmdPassword, cmdCreate));
                        if(resp.code > 0) errors.Add(resp.stderr.Replace("\n", ""));

                        resp = Shell.Term(string.Format("{0} {1}", cmdPassword, cmdRestore));
                        if(resp.code > 0) errors.Add(resp.stderr.Replace("\n", ""));
                        break;
                }   

                PrintTestResults(errors);
                return (errors.Count == 0);
            }
            public static bool DataBaseExists(string server, string database)
            {
                Write("Checking if a database exists for the student ");
                Write(database.Substring(database.IndexOf("_")+1).Replace("_", " "), ConsoleColor.DarkYellow);
                Write(": ");
                
                bool exist = true;
                List<string> errors = new List<string>();            
                using (NpgsqlConnection conn = new NpgsqlConnection(string.Format("Server={0};User Id={1};Password={2};Database={3};", server, "postgres", "postgres", database))){
                    try{
                        conn.Open();                    
                    }               
                    catch(Exception e){                    
                        if(e.Message.Contains(string.Format("database \"{0}\" does not exist", database))) exist = false;                       
                        else throw e;
                    } 
                }

                PrintTestResults(errors);
                return (exist);
            }                      
            public static void ExtractZipFile(string zipPath, string password = null){
                ExtractZipFile(zipPath, Path.GetDirectoryName(zipPath), null);
            }
        #endregion        
    }
}