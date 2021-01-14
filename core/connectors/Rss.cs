/*
    Copyright © 2020 Fernando Porrino Serrano
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
using System.Xml;
using System.Net;
using System.Xml.XPath;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using AutoCheck.Core.Exceptions;
using Wmhelp.XPath2;

namespace AutoCheck.Core.Connectors{       
    /// <summary>
    /// Allows in/out operations and/or data validations with RSS files.
    /// </summary>
    public class Rss: Xml{                        
        /// <summary>
        /// Creates a new connector instance.
        /// </summary>
        /// <param name="folder">The folder containing the files.</param>
        /// <param name="file">RSS file name.</param>
        public Rss(string folder, string file): base (folder, file, ValidationType.None){                      
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
            string html = string.Empty;
            string url = "http://validator.w3.org/feed/check.cgi?output=soap12";
            byte[] dataBytes = Encoding.UTF8.GetBytes(this.Raw);

            //Documentation:    https://validator.w3.org/feed/docs/soap.html
            //                  https://github.com/validator/validator/wiki/Service-%C2%BB-Input-%C2%BB-POST-body            
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.ContentLength = dataBytes.Length;
            request.Method = "POST";
            request.ContentType = "text/html; charset=utf-8";
            request.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.101 Safari/537.36";

            using(Stream requestBody = request.GetRequestStream())
                requestBody.Write(dataBytes, 0, dataBytes.Length);
        
            XmlDocument document = new XmlDocument();
            using(HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using(Stream stream = response.GetResponseStream())             
            using(StreamReader reader = new StreamReader(stream)){
                string output = reader.ReadToEnd();                                
                document.LoadXml(output); 
            }
                        
            foreach(XmlNode msg in document.GetElementsByTagName("info")){               
                XmlAttribute type = msg.Attributes["type"];
                if(type != null && type.InnerText.Equals("error"))
                    throw new DocumentInvalidException();  //TODO: add the error list to the description
            }
            
            foreach(XmlNode msg in document.GetElementsByTagName("error")){
                //Workaround: works on manual validation but fails on API cal...
                if(msg.InnerText.StartsWith("Attribute allow not allowed on element iframe at this point.")) continue;
                
                //TODO: add the error list to the description                
                string node = "<ul>";                
                throw new DocumentInvalidException(msg.InnerText.Contains(node) ? msg.InnerText.Substring(0, msg.InnerText.LastIndexOf(node)) : msg.InnerText); //TODO: add the error list to the description
            }

        }
    }
}