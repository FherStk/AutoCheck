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
        /// <param name="filePath">RSS file path.</param>
        public Rss(string filePath): base (filePath, ValidationType.None){                      
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
            string url = "http://validator.w3.org/feed/check.cgi";
            string rss = System.Web.HttpUtility.UrlEncode(this.Raw.Replace("\r\n", ""));
            string parameters = string.Format("output=soap12&rawdata={0}", rss);            
            byte[] dataBytes = System.Web.HttpUtility.UrlEncodeToBytes(parameters);

            //Documentation:    https://validator.w3.org/feed/docs/soap.html
            //                  https://github.com/validator/validator/wiki/Service-%C2%BB-Input-%C2%BB-POST-body           
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format("{0}?{1}", url, parameters));
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;            
    
            XmlDocument document = new XmlDocument();
            using(HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using(Stream stream = response.GetResponseStream())            
            using(StreamReader reader = new StreamReader(stream))
            {
                string output = reader.ReadToEnd();                             
                document.LoadXml(output); 
            }
                        
            int errorCount = int.Parse(document.GetElementsByTagName("m:errorcount")[0].InnerText);
            if(errorCount > 0){
                //TODO: add the error list to the description
                //Loop through all the "m:error" nodes
                //  Display: "m:line" + "m:errortype" + "m:context" + "m_message"
                throw new DocumentInvalidException(); 
            }          
        }
    }
}