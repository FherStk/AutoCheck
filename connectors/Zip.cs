using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;

namespace AutoCheck.Connectors{    
    /// <summary>
    /// Allows in/out operations and/or data validations with ZIP files.
    /// </summary>
    public class Zip: Core.Connector{  
        /// <summary>
        /// Extracts a zip file into the current folder, so the extracted files will be in the same folder as the zip one.
        /// </summary>
        /// <param name="zipPath">ZIP file's path.</param>
        /// <param name="password">ZIP file's password.</param>
        public static void ExtractFile(string zipPath, string password = null){
            ExtractFile(zipPath, Path.GetDirectoryName(zipPath), null);
        } 
        /// <summary>
        /// Extracts a zip file into the given folder.
        /// </summary>
        /// <param name="zipPath">ZIP file's path.</param>
        /// <param name="outFolder">Destination folder for the extracted files.</param>
        /// <param name="password">ZIP file's password.</param>
        public static void ExtractFile(string zipPath, string outFolder, string password = null) {
            //source:https://github.com/icsharpcode/SharpZipLib/wiki/Unpack-a-Zip-with-full-control-over-the-operation
            using(Stream fsInput = File.OpenRead(zipPath)){ 
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

                        string entryFileName = zipEntry.Name;
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
    }
}