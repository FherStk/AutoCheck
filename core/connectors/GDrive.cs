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
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Download;
using Google.Apis.Util.Store;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.StaticFiles;
using AutoCheck.Core.Exceptions;

namespace AutoCheck.Core.Connectors{    
    /// <summary>
    /// Allows in/out operations and/or data validations with a GDrive instance.
    /// </summary>
    public class GDrive: Base{      
#region Properties
        public DriveService Drive {get; private set;}
#endregion
#region Constructor / Destructor
        public GDrive(string accountFilePath, string secretFilePath){  
            accountFilePath = Utils.PathToCurrentOS(accountFilePath);
            secretFilePath = Utils.PathToCurrentOS(secretFilePath);
                     
            if (string.IsNullOrEmpty(accountFilePath)) throw new ArgumentNullException("accountFilePath");
            if (!File.Exists(accountFilePath)) throw new FileNotFoundException($"The given '{accountFilePath}' file does not exist.");

            if (string.IsNullOrEmpty(secretFilePath)) throw new ArgumentNullException("secretFilePath");                                        
            if (!File.Exists(secretFilePath)) throw new FileNotFoundException($"The given '{secretFilePath}' file does not exist.");
            

            this.Drive = AuthenticateOauth(accountFilePath, secretFilePath);
        } 

        /// <summary>
        /// Disposes the object releasing its unmanaged properties.
        /// </summary>
        public override void Dispose(){
            this.Drive.Dispose();
        }   
#endregion
#region Folders
        /// <summary>
        /// Creates the specified folder
        /// </summary>
        /// <param name="folder">The folder to create including its path (all needed subfolders will be created also).</param>
        public Google.Apis.Drive.v3.Data.File CreateFolder(string folder){
            folder = folder.Trim('\\');            
            return CreateFolder(Path.GetDirectoryName(folder), Path.GetFileName(folder));
        }

        /// <summary>
        /// Creates the specified folder
        /// </summary>
        /// <param name="path">Path where the folder will be created into.</param>
        /// <param name="folder">The folder to create (all needed subfolders will be created also).</param>
        public Google.Apis.Drive.v3.Data.File CreateFolder(string path, string folder){
            if(string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");            
            if(string.IsNullOrEmpty(folder)) throw new ArgumentNullException("folder");
            if(!path.StartsWith("\\")) throw new ArgumentInvalidException("The path argument must be absolute (starting with '\\')");
                        
            Google.Apis.Drive.v3.Data.File root = null;
            var exists = Path.Combine(path, folder);
            var create = "";

            //Looking for which part exists and wich one must be created
            do{                
                root = GetFolder(Path.GetDirectoryName(exists), Path.GetFileName(exists), false);
                if(root == null){
                    create = Path.Combine(create, Path.GetFileName(exists)); //reversed on purpose
                    exists = Path.GetDirectoryName(exists);
                }
            }
            while(root == null && exists.Length > 0);

            //create must be created within root 
            while(create.Length > 0){
                var name = Path.GetFileName(create);
                create = Path.GetDirectoryName(create);

                var file = new Google.Apis.Drive.v3.Data.File(){
                    Name = name,
                    MimeType = "application/vnd.google-apps.folder"
                };  

                if(root != null) file.Parents = new string[]{root.Id};                                                             
                root = Utils.RunWithRetry<Google.Apis.Drive.v3.Data.File, Google.GoogleApiException>(() => {
                    return this.Drive.Files.Create(file).Execute();
                });
            }

            return root;   
        }
                
        /// <summary>
        /// Returns the selected folder
        /// </summary>
        /// <param name="folder">The folder to get including its path.</param>
        public Google.Apis.Drive.v3.Data.File GetFolder(string folder){
            folder = folder.Trim('\\');            
            return GetFolder(Path.GetDirectoryName(folder), Path.GetFileName(folder), false);
        }
        
        /// <summary>
        /// Returns the selected folder
        /// </summary>
        /// <param name="path">Path where the folder will be searched into.</param>
        /// <param name="folder">The folder to search.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>The selected folder.</returns>
        public Google.Apis.Drive.v3.Data.File GetFolder(string path, string folder, bool recursive = true){   
            if(string.IsNullOrEmpty(folder)) throw new ArgumentNullException("folder");      
            return GetFileOrFolder(path, folder, true, recursive);
        }

        /// <summary>
        /// Returns how many folders has been found within the given path.
        /// </summary>
        /// <param name="path">Path where the folders will be searched into.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>The amount of folders.</returns>
        public int CountFolders(string path, bool recursive = true){
            var folder = GetFolder(Path.GetDirectoryName(path), Path.GetFileName(path), false);
            if(folder == null) return 0;
            else{                
                var list = GetList(string.Format("'{0}' in parents", folder.Id), true);
                var count = list.Count;
                
                if(recursive){
                    foreach(var f in list)
                        count += CountFolders(Path.Join(path, f.Name), recursive);
                }            

                
                return count;
            }            
        }

        /// <summary>
        /// Deletes the selected folder.
        /// </summary>
        /// <param name="folder">The folder to get including its path.</param>
        public void DeleteFolder(string folder){
            folder = folder.TrimEnd('\\');
            DeleteFolder(Path.GetDirectoryName(folder), Path.GetFileName(folder));
        }

        /// <summary>
        /// Removes a remote folder
        /// </summary>
        /// <param name="path">Path where the folder will be searched into.</param>
        /// <param name="folder">The folder to search.</param>
        public void DeleteFolder(string path, string folder){
            var f = GetFolder(path, folder, false);
            if(f != null){
                Utils.RunWithRetry<Google.GoogleApiException>(() => {
                    this.Drive.Files.Delete(f.Id).Execute();
                });
            }
        }

        /// <summary>
        /// Determines if a folder exists.
        /// </summary>
        /// <param name="folder">The folder to get including its path.</param>
        public bool ExistsFolder(string folder){
            folder = folder.TrimEnd('\\');
            return ExistsFolder(Path.GetDirectoryName(folder), Path.GetFileName(folder));
        }

        /// <summary>
        /// Determines if a folder exists.
        /// </summary>
        /// <param name="path">Path where the folder will be searched into.</param>
        /// <param name="folder">The folder to search.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>If the folder exists or not.</returns>
        public bool ExistsFolder(string path, string folder, bool recursive = false){
            return GetFolder(path, folder, recursive) != null;
        }
#endregion
#region Files          
        /// <summary>
        /// Returns the selected file.
        /// </summary>
        /// <param name="file">The file to get including its path.</param>
        public Google.Apis.Drive.v3.Data.File GetFile(string file){         
            return GetFile(Path.GetDirectoryName(file), Path.GetFileName(file), false);
        }

        /// <summary>
        /// Returns the selected file.
        /// </summary>
        /// <param name="path">Path where the file will be searched into.</param>
        /// <param name="file">The file to search.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>The selected file.</returns>
        public Google.Apis.Drive.v3.Data.File GetFile(string path, string file, bool recursive = true){   
            if(string.IsNullOrEmpty(file)) throw new ArgumentNullException("file");      
            return GetFileOrFolder(path, file, false, recursive);
        }
        
        /// <summary>
        /// Returns how many folders has been found within the given path.
        /// </summary>
        /// <param name="path">Path where the folders will be searched into.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>The amount of folders.</returns>
        public int CountFiles(string path, bool recursive = true){
            var folder = GetFolder(Path.GetDirectoryName(path), Path.GetFileName(path), false);
            if(folder == null) return 0;
            else{                
                var list = GetList(string.Format("'{0}' in parents", folder.Id), false);
                var count = list.Count;
                
                if(recursive){
                    list = GetList(string.Format("'{0}' in parents", folder.Id), true);                    
                    foreach(var f in list)
                        count += CountFiles(Path.Join(path, f.Name), recursive);
                }            

                
                return count;
            }            
        }

        /// <summary>
        /// Uploads a local file to a remote Google Drive folder.
        /// </summary>
        /// <param name="localFilePath">Local file path</param>
        /// <param name="remoteFilePath">Remote file path (will be created if not exists).</param>
        /// <param name="remoteFileName">Remote file name (extenssion and/or name will be infered from source if not provided).</param>
        public void CreateFile(string localFilePath, string remoteFilePath, string remoteFileName = null){
            if(string.IsNullOrEmpty(localFilePath)) throw new ArgumentNullException("localFilePath");    
            if(string.IsNullOrEmpty(remoteFilePath)) throw new ArgumentNullException("remoteFilePath");    
            if(!File.Exists(localFilePath)) throw new FileNotFoundException();   
                        
            string mime = string.Empty;                        
            if(!new FileExtensionContentTypeProvider().TryGetContentType(localFilePath, out mime)){                                
                //Unsupported are manually added
                mime = Path.GetExtension(localFilePath) switch
                {
                    ".mkv" => "video/x-matroska",     
                    ".sql" => "application/sql",                  
                    _     => throw new InvalidCastException(string.Format("Unable to determine the MIME Type for the file '{0}'", Path.GetFileName(localFilePath)))
                };
            }  

            if(string.IsNullOrEmpty(Path.GetFileName(remoteFileName))) remoteFileName += Path.GetFileName(localFilePath);
            if(string.IsNullOrEmpty(Path.GetExtension(remoteFileName))) remoteFileName += Path.GetExtension(localFilePath);
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = Path.GetFileName(remoteFileName),
                MimeType = mime
            };  

            if(remoteFilePath != "\\"){
                var parent = GetFolder(Path.GetDirectoryName(remoteFilePath), Path.GetFileName(remoteFilePath), false); 
                if(parent == null) parent = CreateFolder(Path.GetDirectoryName(remoteFilePath), Path.GetFileName(remoteFilePath));
                fileMetadata.Parents = new string[]{parent.Id};                    
            }   

            using (var stream = new System.IO.FileStream(localFilePath, System.IO.FileMode.Open))
            {                
                var existing = GetFile(remoteFilePath, fileMetadata.Name, false);
                if(existing == null){
                    Utils.RunWithRetry<Google.GoogleApiException>(() => {
                        //Create a new file
                        this.Drive.Files.Create(fileMetadata, stream, mime).Upload();
                    });
                }                 
                else{ 
                    Utils.RunWithRetry<Google.GoogleApiException>(() => {
                        //Update an existing one
                        this.Drive.Files.Update(fileMetadata, existing.Id).Execute();
                    });  
                }              
            }
        }

        /// <summary>
        /// Deletes the selected file.
        /// </summary>
        /// <param name="file">The folder to get including its path.</param>
        public void DeleteFile(string file){
            DeleteFile(Path.GetDirectoryName(file), Path.GetFileName(file));
        }

        /// <summary>
        /// Removes a remote file
        /// </summary>
        /// <param name="remoteFilePath">Remote file path .</param>
        /// <param name="remoteFileName">Remote file name (extenssion and/or name will be infered from source if not provided).</param>
        public void DeleteFile(string remoteFilePath, string remoteFileName){
            var file = GetFile(remoteFilePath, remoteFileName);
            if(file != null){
                Utils.RunWithRetry<Google.GoogleApiException>(() => {
                    this.Drive.Files.Delete(file.Id).Execute();
                });
            } 
        }

        /// <summary>
        /// Copy an external Google Drive file into the main account.
        /// </summary>
        /// <param name="uri">The Google Drive API file URI to copy.</param>
        /// <param name="remoteFilePath">Remote file path</param>
        /// <param name="remoteFileName">Remote file name (extenssion and/or name will be infered from source if not provided).</param>
        public void CopyFile(Uri uri, string remoteFilePath, string remoteFileName = null){
            var id = GetFileIdFromUri(uri);
            CopyFile(id, remoteFilePath, remoteFileName);    
        }

        /// <summary>
        /// Copy an external Google Drive file into the main account.
        /// </summary>
        /// <param name="file">The Google Drive API file to copy.</param>
        /// <param name="remoteFilePath">Remote file path</param>
        /// <param name="remoteFileName">Remote file name (extenssion and/or name will be infered from source if not provided).</param>
        public void CopyFile(Google.Apis.Drive.v3.Data.File file, string remoteFilePath, string remoteFileName = null){
            CopyFile(file.Id, remoteFilePath, remoteFileName);
        }

        /// <summary>
        /// Copy an external Google Drive file into the main account.
        /// </summary>
        /// <param name="fileID">The Google Drive API file's ID to copy.</param>
        /// <param name="remoteFilePath">Remote file path</param>
        /// <param name="remoteFileName">Remote file name (extenssion and/or name will be infered from source if not provided).</param>
        public void CopyFile(string fileID, string remoteFilePath, string remoteFileName = null){
            if(string.IsNullOrEmpty(fileID)) throw new ArgumentNullException("fileID");   
            if(string.IsNullOrEmpty(remoteFilePath)) throw new ArgumentNullException("remoteFilePath");

            
            if(string.IsNullOrEmpty(remoteFileName) || string.IsNullOrEmpty(Path.GetExtension(remoteFileName))){
                var original = Utils.RunWithRetry<Google.Apis.Drive.v3.Data.File, Google.GoogleApiException>(() => {
                    return this.Drive.Files.Get(fileID).Execute(); 
                });

                if(string.IsNullOrEmpty(remoteFileName)) remoteFileName = original.Name;
                else if(string.IsNullOrEmpty(Path.GetExtension(remoteFileName))) remoteFileName += Path.GetExtension(original.Name);
            }

            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = remoteFileName
            };
            
            if(remoteFilePath != "\\"){
                var folder = GetFolder(Path.GetDirectoryName(remoteFilePath), Path.GetFileName(remoteFilePath));
                fileMetadata.Parents = new string[]{folder.Id};
            }

            var copy = this.Drive.Files.Copy(fileMetadata, fileID);
            var file = Utils.RunWithRetry<Google.Apis.Drive.v3.Data.File, Google.GoogleApiException>(() => {
                return copy.Execute(); 
            });
        }

        //TODO: not needed right now, but could be useful -> moveFile / moveFolder / emptyTrash        
        
        /// <summary>
        /// Uploads a local file to a remote Google Drive folder.
        /// </summary>
        /// <param name="localFilePath">Local file path</param>
        /// <param name="remoteFilePath">Remote file path (will be created if not exists).</param>
        /// <param name="remoteFileName">Remote file name (extenssion and/or name will be infered from source if not provided).</param>
        /// <remarks>This method is an alias for CreateFile.</remarks>
        public void Upload(string localFilePath, string remoteFilePath, string remoteFileName = null){
            CreateFile(localFilePath, remoteFilePath, remoteFileName);
        }

        /// <summary>
        /// Downloads a file from an external Google Drive account.
        /// </summary>
        /// <param name="uri">The Google Drive API file URI to download.</param>
        /// <param name="savePath">Local path where store the file</param>
        /// <returns>The downloaded file path<returns>
        /// <remarks>The file must be shared with the downloader's account.</remarks>
        public string Download(Uri uri, string savePath){
            //Documentation: https://developers.google.com/drive/api/v3/search-files
            //               https://developers.google.com/drive/api/v3/reference/files
                                    
            var id = GetFileIdFromUri(uri);                                
            return Download(id, savePath);            
        }

        /// <summary>
        /// Downloads a file from an external Google Drive account.
        /// </summary>
        /// <param name="file">The Google Drive API file to download.</param>
        /// <param name="savePath">Local path where store the file</param>
        /// <returns>The downloaded file path<returns>
        /// <remarks>The file must be shared with the downloader's account.</remarks>
        public string Download(Google.Apis.Drive.v3.Data.File file, string savePath)
        { 
            return Download(file.Id, savePath);
        }

        /// <summary>
        /// Downloads a file from an external Google Drive account.
        /// </summary>
        /// <param name="fileID">The Google Drive API file's ID to download.</param>
        /// <param name="savePath">Local path where store the file</param>
        /// <returns>The downloaded file path<returns>
        /// <remarks>The file must be shared with the downloader's account.</remarks>
        public string Download(string fileID, string savePath)
        {    
            if(string.IsNullOrEmpty(fileID)) throw new ArgumentNullException("fileID");    
            if(string.IsNullOrEmpty(savePath)) throw new ArgumentNullException("savePath");    
            if(!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);

            var request = this.Drive.Files.Get(fileID);
            var filePath = Path.Combine(savePath, Utils.RunWithRetry<string, Google.GoogleApiException>(() => {
                return request.Execute().Name; 
            }));

            var stream = new MemoryStream();           
            request.MediaDownloader.ProgressChanged += (IDownloadProgress progress) =>
            {
                switch (progress.Status){
                   case Google.Apis.Download.DownloadStatus.Completed:                    
                        SaveStream(stream, filePath);
                        break;
                    
                    case Google.Apis.Download.DownloadStatus.Failed:
                        throw new DownloadFailedException();
                   }
            };
            
            Utils.RunWithRetry<Google.GoogleApiException>(() => {
                request.Download(stream); 
            });

            return filePath;
        }

        /// <summary>
        /// Determines if a file exists.
        /// </summary>
        /// <param name="file">The file to get including its path.</param>
        public bool ExistsFile(string file){
            return ExistsFile(Path.GetDirectoryName(file), Path.GetFileName(file));
        }

        /// <summary>
        /// Determines if a file exists.
        /// </summary>
        /// <param name="path">Path where the file will be searched into.</param>
        /// <param name="file">The file to search.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>If the file exists or not.</returns>
        public bool ExistsFile(string path, string file, bool recursive = false){
            return GetFile(path, file, recursive) != null;
        }
#endregion
#region Private
        /// <remarks>Credits to Linda Lawton: https://www.daimto.com/download-files-from-google-drive-with-c/</remarks>
        private static void SaveStream(MemoryStream stream, string filePath)
        {
            using (var file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                stream.WriteTo(file);
        }

        /// <summary>
        /// This method requests Authentcation from a user using Oauth2.  
        /// Credentials are stored in System.Environment.SpecialFolder.Personal
        /// Documentation https://developers.google.com/accounts/docs/OAuth2
        /// </summary>        
        /// <param name="accountFilePath">Path to the file containing the account name who is being authentcated.</param>
        /// <param name="secretFilePath">Path to the client secret json file from Google Developers console.</param>
        /// <returns>DriveService used to make requests against the Drive API</returns>
        /// <remarks>Credits to Linda Lawton: https://www.daimto.com/download-files-from-google-drive-with-c/</remarks>
        private static DriveService AuthenticateOauth(string accountFilePath, string secretFilePath)
        {
            try
            {                                        
                UserCredential credential;
                string userName = File.ReadAllText(accountFilePath).Trim();                

                using (var stream = new FileStream(secretFilePath, FileMode.Open, FileAccess.Read))
                {
                    string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                    credPath = Path.Combine(credPath, ".credentials/", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
                    
                    // These are the scopes of permissions you need. It is best to request only what you need and not all of them
                    string[] scopes = new string[] { 
                        DriveService.Scope.Drive
                    };
                    
                    // Requesting Authentication or loading previously stored authentication for userName
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        scopes,
                        userName,
                        CancellationToken.None,
                        new FileDataStore(credPath, true)
                    ).Result;
                }

                // Create Drive API service.
                return new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Autocheck's GDrive Connector"
                });
            }
            catch (Exception ex)
            {
                throw new ConnectionInvalidException("Unable to stablish a connection to Google Drive's API using OAuth 2", ex);
            }
        }                            

        private string GetFileIdFromUri(Uri uri){
            if(uri == null) throw new ArgumentNullException("uri");
            if(!uri.Authority.Contains("drive.google.com")) throw new ArgumentInvalidException("The provided URL must point to drive.google.com");
            
            var id = string.Empty;
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);            
            
            if(query.GetValues("id") != null) id = query.GetValues("id").FirstOrDefault();
            else{
                var parts = uri.AbsolutePath.Split("/");                
                if(parts.Length < 4 || string.IsNullOrEmpty(parts[3]))  throw new ArgumentInvalidException("The provided URL must point to a shared file in drive.google.com");            
                else id = parts[3];                
            } 

            return id;   
        }          
        
        private IList<Google.Apis.Drive.v3.Data.File> GetList(string query, bool isFolder){
            var list = this.Drive.Files.List();
            list.Q = string.Format("trashed=false and mimeType {0} 'application/vnd.google-apps.folder' and {1}", (isFolder ? "=" : "!=") ,query);
            return Utils.RunWithRetry<IList<Google.Apis.Drive.v3.Data.File>, Google.GoogleApiException>(() => {
                return list.Execute().Files; 
            });
        }

        private Google.Apis.Drive.v3.Data.File GetFileOrFolder(string path, string item, bool isFolder, bool recursive = true){         
            if(string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");    
            if(!path.StartsWith("\\")) throw new ArgumentInvalidException("The path argument must be absolute (starting with '\\')");
            
            var original = path;            
            foreach(var folder in GetList(string.Format("name='{0}'", item), isFolder)){
                Google.Apis.Drive.v3.Data.File current = folder;
                var get = this.Drive.Files.Get(current.Id);
                get.Fields = "name, parents";
                
                current = Utils.RunWithRetry<Google.Apis.Drive.v3.Data.File, Google.GoogleApiException>(() => {
                    return get.Execute();
                });
                
                path = original;
                while(path.Length > 0 && !path.Equals("\\")){
                    //note: if there's two parents with the same name, this won't work!
                    //      in this case, an extra loop over parents is needed (so GetParent should return an array)                    
                    var parent = Path.GetFileName(path);
                    path = Path.GetDirectoryName(path);

                    current = GetParent(current, parent, recursive);
                    if(current == null) break;
                }                

                if(current != null) return folder;
            }

            return null;
        }
        
        private Google.Apis.Drive.v3.Data.File GetParent(Google.Apis.Drive.v3.Data.File folder, string parentName, bool recursive){
            foreach(var parentID in folder.Parents){
                var get = this.Drive.Files.Get(parentID);                
                get.Fields = "name, parents";

                var parent = Utils.RunWithRetry<Google.Apis.Drive.v3.Data.File, Google.GoogleApiException>(() => {
                    return get.Execute();
                });
            
                if(parent.Name.Equals(parentName)) return parent;
                else if(recursive) return(GetParent(parent, parentName, recursive));
            }

            return null;
        }        
#endregion        
    }    
}