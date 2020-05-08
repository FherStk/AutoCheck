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
using System.Threading;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Download;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;

namespace AutoCheck.Connectors{    
    /// <summary>
    /// Allows in/out operations and/or data validations with a GDrive instance.
    /// </summary>
    public class GDrive: Core.Connector{          
        public DriveService Drive {get; private set;}

        public GDrive(string clientSecretJson, string userName){
            if (string.IsNullOrEmpty(clientSecretJson)) throw new ArgumentNullException("clientSecretJson");
            if (string.IsNullOrEmpty(userName)) throw new ArgumentNullException("userName");                
            if (!File.Exists(clientSecretJson)) throw new Exception("clientSecretJson file does not exist.");
            
            this.Drive = AuthenticateOauth(clientSecretJson, userName);
        }  

        /// <remarks>Credits to Linda Lawton: https://www.daimto.com/download-files-from-google-drive-with-c/</remarks>
        private void DownloadFile(Google.Apis.Drive.v3.Data.File file, string saveTo)
        {

            var request = this.Drive.Files.Get(file.Id);
            var stream = new MemoryStream();

            // Add a handler which will be notified on progress changes.
            // It will notify on each chunk download and when the
            // download is completed or failed.
            request.MediaDownloader.ProgressChanged += (IDownloadProgress progress) =>
            {
                switch (progress.Status){
                    case Google.Apis.Download.DownloadStatus.Downloading:
                        Console.WriteLine(progress.BytesDownloaded);
                        break;

                    case Google.Apis.Download.DownloadStatus.Completed:                    
                        Console.WriteLine("Download complete.");
                        SaveStream(stream, saveTo);
                        break;
                    
                    case Google.Apis.Download.DownloadStatus.Failed:
                        Console.WriteLine("Download failed.");
                        break;
                   }
            };
            
            request.Download(stream);
        }

        /// <remarks>Credits to Linda Lawton: https://www.daimto.com/download-files-from-google-drive-with-c/</remarks>
        private static void SaveStream(MemoryStream stream, string saveTo)
        {
            using (var file = new FileStream(saveTo, System.IO.FileMode.Create, System.IO.FileAccess.Write))
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
                    ApplicationName = "Drive Oauth2 Authentication Sample"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Create Oauth2 account DriveService failed" + ex.Message);
                throw new Exception("CreateServiceAccountDriveFailed", ex);
            }
        }
        
        /// <summary>
        /// Disposes the object releasing its unmanaged properties.
        /// </summary>
        public override void Dispose(){
        }         
    }
}