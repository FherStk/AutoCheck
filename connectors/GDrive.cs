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
        /// Uploads a local file to a remote Google Drive folder.
        /// </summary>
        /// <param name="localFilePath">Local file path</param>
        /// <param name="remoteFilePath">Remote file path</param>
        public void Upload(string localFilePath, string remoteFilePath){
            if(string.IsNullOrEmpty(localFilePath)) throw new ArgumentNullException("localFilePath");    
            if(string.IsNullOrEmpty(remoteFilePath)) throw new ArgumentNullException("remoteFilePath");    
            if(!File.Exists(localFilePath)) throw new FileNotFoundException();   

            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = Path.GetFileName(remoteFilePath)
            };

            string mime = string.Empty;            
            if(!new FileExtensionContentTypeProvider().TryGetContentType(localFilePath, out mime))
                throw new InvalidCastException(string.Format("Unable to determine the MIME Type for the file '{0}'", Path.GetFileName(localFilePath)));
            
            FilesResource.CreateMediaUpload request;
            using (var stream = new System.IO.FileStream(localFilePath, System.IO.FileMode.Open))
            {
                request = this.Drive.Files.Create(fileMetadata, stream, mime);
                request.Fields = "id";
                request.Upload();
            }

            var file = request.ResponseBody;
            //Console.WriteLine("File ID: " + file.Id);

            //TODO: create and/or get the folder and move the file within
            //https://developers.google.com/drive/api/v3/folder
        }

        /// <summary>
        /// Downloads a file from an external Google Drive account.
        /// </summary>
        /// <param name="uri">The uri to the file.</param>
        /// <param name="savePath">Local path where store the file</param>
        /// <remarks>The file must be shared with the downloader's account.</remarks>
        public void Download(Uri uri, string savePath){
            //Documentation: https://developers.google.com/drive/api/v3/search-files
            //               https://developers.google.com/drive/api/v3/reference/files
            if(uri == null) throw new ArgumentNullException("uri");
            if(string.IsNullOrEmpty(savePath)) throw new ArgumentNullException("savePath");            
            
            if(!uri.Authority.Contains("drive.google.com")) throw new ArgumentInvalidException("The provided uri must point to drive.google.com");
            if(uri.AbsoluteUri.Substring(uri.AbsoluteUri.IndexOf("//")+2).ToCharArray().Where(x => x.Equals('/')).ToArray().Length < 4) throw new ArgumentInvalidException("The provided uri must point to a shared file in drive.google.com");            

            var id = uri.AbsoluteUri.Substring(0, uri.AbsoluteUri.LastIndexOf("/"));
            id = id.Substring(id.LastIndexOf("/")+1);            

            if(!Directory.Exists(savePath)) throw new DirectoryNotFoundException();            
            Download(id, savePath);            
        }

        /// <summary>
        /// Downloads a file from an external Google Drive account.
        /// </summary>
        /// <param name="file">The Google Drive API file to download.</param>
        /// <param name="savePath">Local path where store the file</param>
        /// <remarks>The file must be shared with the downloader's account.</remarks>
        public void Download(Google.Apis.Drive.v3.Data.File file, string savePath)
        { 
            Download(file.Id, savePath);
        }

        /// <summary>
        /// Downloads a file from an external Google Drive account.
        /// </summary>
        /// <param name="fileID">The Google Drive API file's ID to download.</param>
        /// <param name="savePath">Local path where store the file</param>
        /// <remarks>The file must be shared with the downloader's account.</remarks>
        public void Download(string fileID, string savePath)
        {            
            var request = this.Drive.Files.Get(fileID);
            var stream = new MemoryStream();

            // Add a handler which will be notified on progress changes.
            // It will notify on each chunk download and when the
            // download is completed or failed.
            request.MediaDownloader.ProgressChanged += (IDownloadProgress progress) =>
            {
                switch (progress.Status){
                   case Google.Apis.Download.DownloadStatus.Completed:                    
                        SaveStream(stream, Path.Combine(savePath, request.Execute().Name));
                        break;
                    
                    case Google.Apis.Download.DownloadStatus.Failed:
                        throw new DownloadFailedException();
                   }
            };
            
            request.Download(stream);
        }

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
                // These are the scopes of permissions you need. It is best to request only what you need and not all of them
                string[] scopes = new string[] { DriveService.Scope.DriveReadonly};         	//View the files in your Google Drive                                                 
                UserCredential credential;
                using (var stream = new FileStream(clientSecretJson, FileMode.Open, FileAccess.Read))
                {
                    string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                    credPath = Path.Combine(credPath, ".credentials/", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);

                    // Requesting Authentication or loading previously stored authentication for userName
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        scopes,
                        userName,
                        CancellationToken.None,
                        new FileDataStore(credPath, true)).Result;
                }

                // Create Drive API service.
                return new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Autocheck GDrive Connector"
                });
            }
            catch (Exception ex)
            {
                throw new ConnectionInvalidException("Unable to stablish a connection to Google Drive's API using OAuth 2", ex);
            }
        }       
    }
}