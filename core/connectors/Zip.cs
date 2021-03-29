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
using System.Globalization;
using AutoCheck.Core.Exceptions;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;

namespace AutoCheck.Core.Connectors{    
    public class Zip: Base{     
        /// <summary>
        /// Creates a new connector instance.
        /// </summary>
        public Zip(){            
        }

        /// <summary>
        /// Disposes the object releasing its unmanaged properties.
        /// </summary>
        public override void Dispose(){
        } 
        
        /// <summary>
        /// Extracts a ZIP file.
        /// </summary>
        /// <param name="filePath">ZIP file's path.</param>
        /// <param name="recursive">ZIP files within the extracted one, will be also extracted.</param>
        /// <param name="output">Destination folder for the extracted files.</param>
        /// <param name="password">ZIP file's password.</param>
        public void Extract(string filePath, bool recursive=false, string output = null, string password = null) {
            if(string.IsNullOrEmpty(filePath)) throw new ArgumentNullException("path");            
            if(!File.Exists(filePath)) throw new FileNotFoundException();
            if(!Path.GetExtension(filePath).Equals("zip", StringComparison.InvariantCultureIgnoreCase)) throw new ArgumentInvalidException("Only ZIP files are allowed.");

            output ??= Path.GetDirectoryName(filePath);
            if(!Directory.Exists(output)) throw new DirectoryNotFoundException();
            
            //Encoding must be manually setup in order to avoid errors during decompression
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            ZipStrings.CodePage = System.Globalization.CultureInfo.CurrentCulture.TextInfo.OEMCodePage;

            //source:https://github.com/icsharpcode/SharpZipLib/wiki/Unpack-a-Zip-with-full-control-over-the-operation
            using(Stream fsInput = File.OpenRead(filePath)){ 
                using(ZipFile zf = new ZipFile(fsInput)){
                    
                    if (!string.IsNullOrEmpty(password)) {
                        // AES encrypted entries are handled automatically
                        zf.Password = password;
                    }

                    foreach (ZipEntry zipEntry in zf) {
                        if (!zipEntry.IsFile) {
                            // Ignore directories
                            continue;
                        }
                        
                        //zipEntry.IsUnicodeText <- is false with error

                        string entryFileName = zipEntry.Name;
                        // to remove the folder from the entry:
                        //entryFileName = Path.GetFileName(entryFileName);
                        // Optionally match entrynames against a selection list here
                        // to skip as desired.
                        // The unpacked length is available in the zipEntry.Size property.

                        // Manipulate the output filename here as desired.
                        var fullZipToPath = Path.Combine(output, entryFileName);
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

            if(recursive){
                var files = Directory.GetFiles(Path.GetFullPath(filePath), "*.zip", SearchOption.AllDirectories); 
                foreach(string zip in files){       
                    Extract(zip, true);
                }
            }
        }
    }
}