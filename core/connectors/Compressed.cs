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
using System.Text;
using System.Collections.Generic;
using SharpCompress.Readers;
using SharpCompress.Common;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.Zip;

namespace AutoCheck.Core.Connectors{    
    public class Compressed: Base{     
        public MemoryStream FileContent {get; private set;}   
        public ArchiveType FileType {get; private set;} 
        private string FilePath {get; set;}   

        /// <summary>
        /// Creates a new connector instance.
        /// </summary>
        /// <param name="filePath">A compressed file path.</param>
        public Compressed(string filePath){            
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
        public Compressed(Utils.OS remoteOS, string host, string username, string password, string filePath): this(remoteOS, host, username, password, 22, filePath){              
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
        public Compressed(Utils.OS remoteOS, string host, string username, string password, int port, string filePath){  
            ProcessRemoteFile(remoteOS, host, username, password, port, filePath, new Action<string>((filePath) => {
                Parse(filePath); 
            }));            
        }

        private void Parse(string filePath){       
            if(string.IsNullOrEmpty(filePath)) throw new ArgumentNullException("filePath");
            if(!File.Exists(filePath)) throw new FileNotFoundException();

            FilePath = filePath;
            FileContent = new MemoryStream();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            
            using (var stream = File.OpenRead(filePath)){
                using (var reader = ReaderFactory.Open(stream))
                {     
                    FileType = reader.ArchiveType;                               
                    stream.Seek(0, SeekOrigin.Begin);   
                    stream.CopyTo(FileContent);      
                }      
            }                     
        }

        /// <summary>
        /// Disposes the object releasing its unmanaged properties.
        /// </summary>
        public override void Dispose(){
            FileContent.Dispose();
        } 

        /// <summary>
        /// Extracts the compressed file.
        /// </summary>
        /// <param name="output">Destination folder for the extracted files.</param>
        /// <param name="password">Compressed file's password.</param>
        public void Extract(string output, string password = null) {
            Extract(false, output, password);
        }
        
        /// <summary>
        /// Extracts the compressed file.
        /// </summary>
        /// <param name="recursive">Compressed files within the extracted one, will be also extracted.</param>
        /// <param name="output">Destination folder for the extracted files.</param>
        /// <param name="password">Compressed file's password.</param>
        public void Extract(bool recursive=false, string output = null, string password = null) {           
            output ??= Path.GetDirectoryName(FilePath);
            if(!Directory.Exists(output)) throw new DirectoryNotFoundException();
            
            var extract = new Action<IReader>((reader) => {
                reader.WriteAllToDirectory(output, new ExtractionOptions(){
                    ExtractFullPath = true,
                    Overwrite = true
                });
            });

            switch(FileType){
                case ArchiveType.Zip:
                    var zip = ZipArchive.Open(FileContent);
                    using (var reader = zip.ExtractAllEntries())
                    {                
                       extract.Invoke(reader);
                    }
                    break;
                
                case ArchiveType.Rar:
                    var rar = RarArchive.Open(FileContent);
                    using (var reader = rar.ExtractAllEntries())
                    {                
                        extract.Invoke(reader);
                    }
                    break;                
            }

            if(recursive){                
                //Cannot call recursivelly with the recursive flag in order to avoid infinite loops.
                var done = false;
                var extracted = new HashSet<string>();                

                do{        
                    done = true;

                    var files = new List<string>();
                    files.AddRange(Directory.GetFiles(output, "*.zip", SearchOption.AllDirectories));
                    files.AddRange(Directory.GetFiles(output, "*.rar", SearchOption.AllDirectories));

                    foreach(string file in files){  
                        if(!extracted.Contains(file)){
                            extracted.Add(file);
                            
                            var connector = new Compressed(file);
                            connector.Extract(false);

                            done = false;
                        }
                    }
                } while(!done);
            }
        }
    }
}