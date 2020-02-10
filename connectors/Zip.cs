using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;

namespace AutomatedAssignmentValidator.Connectors{    
    public partial class Zip: Core.Connector{  
        public static void ExtractFile(string zipPath, string password = null){
            ExtractFile(zipPath, Path.GetDirectoryName(zipPath), null);
        } 
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