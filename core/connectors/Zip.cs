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

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using AutoCheck.Core.Exceptions;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;

namespace AutoCheck.Core.Connectors{    
    public class Zip: Base{     
        //public ZipFile ZipFile {get; private set;}   
        private MemoryStream FileContent {get; set;}   
        private string FilePath {get; set;}   

        /// <summary>
        /// Creates a new connector instance.
        /// </summary>
        /// <param name="filePath">ZIP file path.</param>
        public Zip(string filePath){            
            Parse(filePath);    
        }

        /// <summary>
        /// Creates a new connector instance.
        /// </summary>
        /// <param name="remoteOS"The remote host OS.</param>
        /// <param name="host">Host address where the command will be run.</param>
        /// <param name="username">The remote machine's username which one will be used to login.</param>
        /// <param name="password">The remote machine's password which one will be used to login.</param>
        /// <param name="filePath">XML file path.</param>
        /// <param name="validation">Validation type.</param>
        public Zip(Utils.OS remoteOS, string host, string username, string password, string filePath): this(remoteOS, host, username, password, 22, filePath){              
        }

        /// <summary>
        /// Creates a new connector instance.
        /// </summary>
        /// <param name="remoteOS"The remote host OS.</param>
        /// <param name="host">Host address where the command will be run.</param>
        /// <param name="username">The remote machine's username which one will be used to login.</param>
        /// <param name="password">The remote machine's password which one will be used to login.</param>
        /// <param name="port">The remote machine's port where SSH is listening to.</param>
        /// <param name="filePath">XML file path.</param>
        /// <param name="validation">Validation type.</param>
        public Zip(Utils.OS remoteOS, string host, string username, string password, int port, string filePath){  
            ProcessRemoteFile(remoteOS, host, username, password, port, filePath, new Action<string>((filePath) => {
                Parse(filePath); 
            }));            
        }

        private void Parse(string filePath){       
            if(string.IsNullOrEmpty(filePath)) throw new ArgumentNullException("filePath");
            if(!File.Exists(filePath)) throw new FileNotFoundException();

            //Encoding must be manually setup in order to avoid errors during decompression
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            ZipStrings.CodePage = System.Globalization.CultureInfo.CurrentCulture.TextInfo.OEMCodePage;

            FilePath = filePath;
            FileContent = new MemoryStream();
            using(var fs = File.OpenRead(filePath)){
                fs.CopyTo(FileContent);
            }            
        }

        /// <summary>
        /// Disposes the object releasing its unmanaged properties.
        /// </summary>
        public override void Dispose(){
            FileContent.Dispose();
        } 

        /// <summary>
        /// Extracts the ZIP file.
        /// </summary>
        /// <param name="output">Destination folder for the extracted files.</param>
        /// <param name="password">ZIP file's password.</param>
        public void Extract(string output, string password = null) {
            Extract(false, output, password);
        }
        
        /// <summary>
        /// Extracts the ZIP file.
        /// </summary>
        /// <param name="recursive">ZIP files within the extracted one, will be also extracted.</param>
        /// <param name="output">Destination folder for the extracted files.</param>
        /// <param name="password">ZIP file's password.</param>
        public void Extract(bool recursive=false, string output = null, string password = null) {           
            output ??= Path.GetDirectoryName(FilePath);
            if(!Directory.Exists(output)) throw new DirectoryNotFoundException();

            //Source: https://github.com/icsharpcode/SharpZipLib/wiki/Unpack-a-Zip-with-full-control-over-the-operation   
            using(var zf = new ZipFile(FileContent)){
                if (!String.IsNullOrEmpty(password)) zf.Password = password;

                foreach (ZipEntry zipEntry in zf) {
                    if (!zipEntry.IsFile) continue;
                    
                    var entryFileName = zipEntry.Name;
                    var fullZipToPath = Path.Combine(output, entryFileName);
                    var directoryName = Path.GetDirectoryName(fullZipToPath);

                    if (directoryName.Length > 0) Directory.CreateDirectory(directoryName);
                    
                    var buffer = new byte[4096];
                    using(var zipStream = zf.GetInputStream(zipEntry))
                    using (Stream fsOutput = File.Create(fullZipToPath)) {
                        StreamUtils.Copy(zipStream, fsOutput , buffer);
                    }
                }
            }

            if(recursive){                
                //Cannot call recursivelly with the recursive flag in order to avoid infinite loops.
                var done = false;
                var extracted = new HashSet<string>();                

                do{        
                    done = true;                                                               
                    foreach(string file in Directory.GetFiles(output, "*.zip", SearchOption.AllDirectories)){  
                        if(!extracted.Contains(file)){
                            extracted.Add(file);
                            
                            var connector = new Zip(file);
                            connector.Extract(false);

                            done = false;
                        }
                    }
                } while(!done);
            }
        }
    }
}