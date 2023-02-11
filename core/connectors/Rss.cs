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

using System.Xml;
using System.Net.Http;
using AutoCheck.Core.Exceptions;

namespace AutoCheck.Core.Connectors{       
    /// <summary>
    /// Allows in/out operations and/or data validations with RSS files.
    /// </summary>
    public class Rss: Xml{                        
        /// <summary>
        /// Creates a new connector instance.
        /// </summary>
        /// <param name="filePath">RSS file path.</param>
        public Rss(string filePath): base (filePath, ValidationType.None){                      
        }

        /// <summary>
        /// Creates a new connector instance.
        /// </summary>
        /// <param name="remoteOS"The remote host OS.</param>
        /// <param name="host">Host address where the command will be run.</param>
        /// <param name="username">The remote machine's username which one will be used to login.</param>
        /// <param name="password">The remote machine's password which one will be used to login.</param>
        /// <param name="port">The remote machine's port where SSH is listening to.</param>
        /// <param name="filePath">RSS file path.</param>
        public Rss(Utils.OS remoteOS, string host, string username, string password, int port, string filePath): base (remoteOS, host, username, password, port, filePath, ValidationType.None){                      
        }

        /// <summary>
        /// Creates a new connector instance.
        /// </summary>
        /// <param name="remoteOS"The remote host OS.</param>
        /// <param name="host">Host address where the command will be run.</param>
        /// <param name="username">The remote machine's username which one will be used to login.</param>
        /// <param name="password">The remote machine's password which one will be used to login.</param>
        /// <param name="filePath">RSS file path.</param>
        public Rss(Utils.OS remoteOS, string host, string username, string password, string filePath): base (remoteOS, host, username, password, 22, filePath, ValidationType.None){                      
            //This method can be avoided if the overload containing the 'port' argument moves it to the end and makes it optional, but then the signature becomes inconsistent compared with the remoteShell constructor...
        }
        
        /// <summary>
        /// Disposes the object releasing its unmanaged properties.
        /// </summary>
        public override void Dispose(){
        }

                  
        /// <summary>
        /// Validates the currently loaded RSS document against the W3C public API. 
        /// Throws an exception if the document is invalid.
        /// </summary>
        public void ValidateRssAgainstW3C(){            
            //Documentation:    https://validator.w3.org/feed/docs/soap.html
            //                  https://github.com/validator/validator/wiki/Service-%C2%BB-Input-%C2%BB-POST-body         
            var httpClient = new HttpClient();
            var rss = System.Web.HttpUtility.UrlEncode(this.Raw.Replace("\r\n", ""));    
            var asyncGet = httpClient.GetAsync($"http://validator.w3.org/feed/check.cgi?output=soap12&rawdata={rss}");
            asyncGet.Wait();

            asyncGet.Result.EnsureSuccessStatusCode();
            
            var asyncRead = asyncGet.Result.Content.ReadAsStringAsync();
            asyncRead.Wait();
            
            XmlDocument document = new XmlDocument();
            document.LoadXml(asyncRead.Result); 
                        
            int errorCount = int.Parse(document.GetElementsByTagName("m:errorcount")[0].InnerText);
            if(errorCount > 0){
                //TODO: add the error list to the description
                //Loop through all the "m:error" nodes
                //  Display: "m:line" + "m:errortype" + "m:context" + "m_message"
                foreach(XmlNode error in document.GetElementsByTagName("error")){
                    //TODO: add the error list to the description
                    //Loop through all the "m:error" nodes
                    //  Display: "m:line" + "m:errortype" + "m:context" + "m_message"                    
                    var message = $"Line {error.SelectSingleNode("line").InnerText}. {error.SelectSingleNode("text").InnerText.Trim()}";
                    throw new DocumentInvalidException(message.Replace("\\$", System.Environment.NewLine));         
                }
            }         
        }
    }
}