using System;
using System.IO;
using System.Text;
using System.Globalization;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;

namespace AutomatedAssignmentValidator.Core{
    //TODO: this methods will be spread along different Utils classes
    class Utils{    
        public static string DataBaseToStudentName(string database){
            return database.Substring(database.IndexOf("_")+1).Replace("_", " ");
        }  
        public static string MoodleFolderToStudentName(string folder){            
            string studentFolder = Path.GetFileName(folder);
                        
            try{
                //Moodle assignments download uses "_" in order to separate the student name from the assignment ID
                return studentFolder.Substring(0, studentFolder.IndexOf("_"));            
            }
            catch{
                return "UNKNOWN";
            }            
        }
        public static string FolderNameToDataBase(string folder, string prefix = "database"){
            string[] temp = Path.GetFileNameWithoutExtension(folder).Split("_"); 
            if(temp.Length < 5) throw new Exception("The given folder does not follow the needed naming convention.");
            else return RemoveDiacritics(string.Format("{0}_{1}", prefix, temp[0]).Replace(" ", "_")); 
        }         
        public static string RemoveDiacritics(string text) 
        {
            //Manual replacement step (due wrong format from source)
            text = text.Replace("Ã©", "é");

            //Source: https://stackoverflow.com/a/249126
            string norm = text.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            foreach (char c in norm)
            {
                UnicodeCategory cat = CharUnicodeInfo.GetUnicodeCategory(c);
                if (cat != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }  
        public static void ExtractZipFile(string zipPath, string password = null){
            ExtractZipFile(zipPath, Path.GetDirectoryName(zipPath), null);
        } 
        public static void ExtractZipFile(string zipPath, string outFolder, string password = null) {
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
    }
}