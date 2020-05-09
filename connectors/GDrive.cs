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
using AutoCheck.Exceptions;

namespace AutoCheck.Connectors{    
    /// <summary>
    /// Allows in/out operations and/or data validations with a GDrive instance.
    /// </summary>
    public class GDrive: Core.Connector{          
        public DriveService Drive {get; private set;}

        public GDrive(string clientSecretJson, string userName){
            if (string.IsNullOrEmpty(clientSecretJson)) throw new ArgumentNullException("clientSecretJson");                            
            if (!File.Exists(clientSecretJson)) throw new FileNotFoundException("clientSecretJson file does not exist.");
            if (string.IsNullOrEmpty(userName)) throw new ArgumentNullException("userName");

            this.Drive = AuthenticateOauth(clientSecretJson, userName);
        } 

        /// <summary>
        /// Disposes the object releasing its unmanaged properties.
        /// </summary>
        public override void Dispose(){
            this.Drive.Dispose();
        }   

        /// <summary>
        /// Determines if a folder exists.
        /// </summary>
        /// <param name="path">The folder path to find</param>
        /// <returns>True if found.</returns>
        public bool ExistsFolder(string path){         
            var folder = GetFolder(path);
            return folder != null;
        }

        /// <summary>
        /// Determines if a file exists.
        /// </summary>
        /// <param name="path">The file path to find</param>
        /// <returns>True if found.</returns>
        public bool ExistsFile(string path){         
            var file = GetFile(path);
            return file != null;
        }

        /// <summary>
        /// Creates the specified folder
        /// </summary>
        /// <param name="path">Remote folder to create (all needed subfolders will be created also)</param>
        public void CreateFolder(string path){
            if(string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
                        
            Google.Apis.Drive.v3.Data.File root = null;
            var exists = path.Split("\\").ToList();
            var create = new List<string>();

            //Looking for which part exists and wich one must be created
            do{
                root = GetFolder(Path.Combine(exists.ToArray()));
                if(root == null){
                    create.Add(exists.TakeLast(1).SingleOrDefault());
                    exists = exists.SkipLast(1).ToList();                
                }
            }
            while(root == null && exists.Count > 0);

            create.Reverse();

            //create must be created within root 
            foreach(var name in create){
                var file = new Google.Apis.Drive.v3.Data.File(){
                    Name = name,
                    MimeType = "application/vnd.google-apps.folder"
                };  

                if(root != null) file.Parents = new string[]{root.Id};
                root = this.Drive.Files.Create(file).Execute();
            }
        }
        
        /// <summary>
        /// Returns the selected folder
        /// </summary>
        /// <param name="path">The folder path to get</param>
        /// <returns>The selected folder.</returns>
        public Google.Apis.Drive.v3.Data.File GetFolder(string path){         
            return GetFile(path, true);
        }

        /// <summary>
        /// Returns the selected file
        /// </summary>
        /// <param name="path">The file path to get</param>
        /// <returns>The selected file.</returns>
        public Google.Apis.Drive.v3.Data.File GetFile(string path){         
            return GetFile(path, false);
        }
        
        private Google.Apis.Drive.v3.Data.File GetFile(string path, bool isFolder){         
            if(string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");    

            var folders = path.Split("\\").Reverse().ToArray();
            var list = this.Drive.Files.List();

            int i = 0;
            list.Q = string.Format("trashed=false and name = '{0}'", folders[i].Trim());
            if(isFolder) list.Q += " and mimeType = 'application/vnd.google-apps.folder'";

            foreach(var folder in list.Execute().Files){                                                
                Google.Apis.Drive.v3.Data.File parent = folder;
                var get = this.Drive.Files.Get(parent.Id);
                get.Fields = "name, parents";
                parent = get.Execute();

                for(i=1; i < folders.Length; i++){
                    //note: if there's two parents with the same name, this won't work!
                    //      in this case, an extra loop over parents is needed (so GetParent should return an array)                    
                    parent = GetParent(parent, folders[i].Trim());                    
                }                

                if(parent != null) return folder;
            }

            return null;
        }
        
        private Google.Apis.Drive.v3.Data.File GetParent(Google.Apis.Drive.v3.Data.File folder, string parentName){
            foreach(var parentID in folder.Parents){
                var get = this.Drive.Files.Get(parentID);                
                get.Fields = "name, parents";

                var parent = get.Execute();
                if(parent.Name.Equals(parentName)) return parent;
            }

            return null;
        }        
        
        /// <summary>
        /// Uploads a local file to a remote Google Drive folder.
        /// </summary>
        /// <param name="localFilePath">Local file path</param>
        /// <param name="remoteFilePath">Remote file path</param>
        public void CreateFile(string localFilePath, string remoteFilePath){
            if(string.IsNullOrEmpty(localFilePath)) throw new ArgumentNullException("localFilePath");    
            if(string.IsNullOrEmpty(remoteFilePath)) throw new ArgumentNullException("remoteFilePath");    
            if(!File.Exists(localFilePath)) throw new FileNotFoundException();   

            string mime = string.Empty;            
            if(!new FileExtensionContentTypeProvider().TryGetContentType(localFilePath, out mime))
                throw new InvalidCastException(string.Format("Unable to determine the MIME Type for the file '{0}'", Path.GetFileName(localFilePath)));

            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = Path.GetFileName(remoteFilePath),
                MimeType = mime
            };  

            var path = Path.GetDirectoryName(remoteFilePath);
            if(!string.IsNullOrEmpty(path)){
                var folder = GetFolder(path);
                fileMetadata.Parents = new string[]{folder.Id};
            }
            
            FilesResource.CreateMediaUpload request;
            using (var stream = new System.IO.FileStream(localFilePath, System.IO.FileMode.Open))
            {
                request = this.Drive.Files.Create(fileMetadata, stream, mime);
                request.Upload();
            }

            //TODO: create and/or get the folder and move the file within
            //https://developers.google.com/drive/api/v3/folder
        }

        /// <summary>
        /// Removes a remote file
        /// </summary>
        /// <param name="remoteFilePath">The remote file path to remove.</param>
        public void DeleteFile(string remoteFilePath){
            var file = GetFile(remoteFilePath);
            if(file != null) this.Drive.Files.Delete(file.Id).Execute();
        }

        /// <summary>
        /// Removes a remote folder
        /// </summary>
        /// <param name="remoteFolderPath">The remote folder path to remove.</param>
        public void DeleteFolder(string remoteFolderPath){
            var folder = GetFolder(remoteFolderPath);
            if(folder != null) this.Drive.Files.Delete(folder.Id).Execute();
        }

        /// <summary>
        /// Downloads a file from an external Google Drive account.
        /// </summary>
        /// <param name="uri">The uri to the file.</param>
        /// <param name="savePath">Local path where store the file</param>
        /// <returns>The downloaded file path<returns>
        /// <remarks>The file must be shared with the downloader's account.</remarks>
        public string Download(Uri uri, string savePath){
            //Documentation: https://developers.google.com/drive/api/v3/search-files
            //               https://developers.google.com/drive/api/v3/reference/files
            if(uri == null) throw new ArgumentNullException("uri");
            if(string.IsNullOrEmpty(savePath)) throw new ArgumentNullException("savePath");            
            
            if(!uri.Authority.Contains("drive.google.com")) throw new ArgumentInvalidException("The provided uri must point to drive.google.com");
            if(uri.AbsoluteUri.Substring(uri.AbsoluteUri.IndexOf("//")+2).ToCharArray().Where(x => x.Equals('/')).ToArray().Length < 4) throw new ArgumentInvalidException("The provided uri must point to a shared file in drive.google.com");            

            var id = uri.AbsoluteUri.Substring(0, uri.AbsoluteUri.LastIndexOf("/"));
            id = id.Substring(id.LastIndexOf("/")+1);            

            if(!Directory.Exists(savePath)) throw new DirectoryNotFoundException();            
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
            var request = this.Drive.Files.Get(fileID);
            var filePath = Path.Combine(savePath, request.Execute().Name);
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
            
            request.Download(stream);
            return filePath;
        }
        
        //TODO: not needed right now, but could be useful -> moveFile / moveFolder / emptyTrash

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
        /// <param name="clientSecretJson">Path to the client secret json file from Google Developers console.</param>
        /// <param name="userName">Identifying string for the user who is being authentcated.</param>
        /// <returns>DriveService used to make requests against the Drive API</returns>
        /// <remarks>Credits to Linda Lawton: https://www.daimto.com/download-files-from-google-drive-with-c/</remarks>
        private static DriveService AuthenticateOauth(string clientSecretJson, string userName)
        {
            try
            {                        
                UserCredential credential;
                using (var stream = new FileStream(clientSecretJson, FileMode.Open, FileAccess.Read))
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
    }
}