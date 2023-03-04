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

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using MathNet.Numerics.Statistics;
using AutoCheck.Core.Exceptions;
using AutoCheck.Core.Connectors;
using OS = AutoCheck.Core.Utils.OS;

namespace AutoCheck.Core.CopyDetectors{
    /// <summary>
    /// This class must be inherited in order to develop a custom copy detectors.
    /// This class is in charge of performing the copy detection along the student's files, code the abstract methods and provide all the necessary extra code needed.
    /// </summary>
    public abstract class Base : IDisposable{
        /// <summary>
        /// DEFAULT = Values above the 'threshold' will compute as a potential copy.
        /// AUTO = A median will be automatically computed using all the matching values and the threshold' will be used as a margin error, so only the matching values over the margin error will compute as a potential copy.
        /// </summary>
        public enum DetectionMode{
            DEFAULT,
            AUTO
        }

        /// <summary>
        /// Matching values higher than this one, will be considered as a potential copy.
        /// </summary>
        /// <value></value>
        public float Threshold {get; protected set;}

        /// <summary>
        /// This value is used by the AUTO mode, so matching values higher than this one, will be considered as a potential copy.
        /// Must be computed wihtin 'Compare()' methods for performance reasons, using the ComputeAutoModeProperties() value just after computing the 'Median' value.
        /// </summary>
        /// <value></value>
        protected float AutomaticThreshold {get; set;}

        /// <summary>
        /// The comparison sensibility, a lower value increases the sensibility so produces more potential copy results.
        /// </summary>
        /// <value></value>
        protected int Sensibility {get; set;}

        /// <summary>
        /// Pattern that will be used to find and load files within the copy detector.
        /// </summary>
        /// <value></value>
        public string FilePattern {get; protected set;}    

        /// <summary>
        /// Pattern that will be used to find and load files within the copy detector.
        /// </summary>
        /// <value></value>
        public DetectionMode Mode {get; protected set;}     
        
        /// <summary>
        /// The amount of items loaded into the copy detector.
        /// </summary>
        /// <value></value>
        public abstract int Count {get;}       
        
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="threshold">Matches above this value will be computed as potential copies.</param>
        /// <param name="sensibility">The copy detection sensibility, lower values increases the probability of false positives.</param>
        /// <param name="mode">The comparisson mode.</param>
        /// <param name="filePattern">Only the files mathing this pattern will be compared.</param>
        public Base(float threshold, int sensibility, DetectionMode mode, string filePattern = "*")
        {
            if(string.IsNullOrEmpty(filePattern)) throw new ArgumentNullException("filePattern");
            FilePattern = filePattern;

            if(threshold < 0 || threshold > 1) throw new ArgumentOutOfRangeException("threshold");
            Threshold = threshold;   

            Sensibility = sensibility;                
            Mode = mode;
        }        
        
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="threshold">Matches above this value will be computed as potential copies.</param>
        /// <param name="sensibility">The copy detection sensibility, lower values increases the probability of false positives.</param>
        /// <param name="filePattern">Only the files mathing this pattern will be compared.</param>
        /// <returns></returns>
        public Base(float threshold, int sensibility, string filePattern = "*"): this(threshold, sensibility, DetectionMode.DEFAULT, filePattern){           
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="threshold">Matches above this value will be computed as potential copies.</param>
        /// <param name="mode">The comparisson mode.</param>
        /// <param name="filePattern">Only the files mathing this pattern will be compared.</param>
        /// <returns></returns>
        public Base(float threshold, DetectionMode mode, string filePattern = "*"): this(threshold, -1, mode, filePattern){           
        }

        /// <summary>
        /// Creates a new instance, setting up its properties in order to allow copy detection with the lowest possible false-positive probability.
        /// </summary>
        /// <param name="threshold">Matches above this value will be computed as potential copies.</param>
        /// <param name="filePattern">Only the files mathing this pattern will be compared.</param>
        /// <returns></returns>
        public Base(float threshold, string filePattern = "*"): this(threshold, -1, DetectionMode.DEFAULT, filePattern){           
        } 

        /// <summary>
        /// Disposes the current copy detector instance and releases its internal objects.
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Loads a local file into the local collection in order to compare it when Compare() is called.
        /// </summary>
        /// <param name="path">Path to a file or folder; if the path points to a folder, the first file found using the FilePattern property will be loaded.</param>                       
        public virtual void Load(string path){   
            if(string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");            

            if(!string.IsNullOrEmpty(Path.GetExtension(path))) Load(Path.GetDirectoryName(path), Path.GetFileName(path));
            else{            
                string file = Directory.GetFiles(path, FilePattern, SearchOption.AllDirectories).FirstOrDefault();            
                if(string.IsNullOrEmpty(file)) throw new ArgumentInvalidException($"Unable to find any file using the search pattern '{FilePattern}'.");            
                Load(path, file);
            }            
        }

        /// <summary>
        /// Loads a remote file into the local collection in order to compare it when Compare() is called.
        /// </summary>
        /// <param name="host">Remote OS family.</param>    
        /// <param name="host">Remote host name.</param>                       
        /// <param name="username">The username wich will be used to connect with the remote host.</param>                       
        /// <param name="password">The password wich will be used to connect with the remote host.</param>                       
        /// <param name="password">The SSH port wich will be used to connect with the remote host.</param>                       
        public virtual void Load(OS os, string host, string username, string password, string path){ 
            //Just to keep the signature consistent along connectors and other stuff
            Load(os, host, username, password, 22, path);
        }

        /// <summary>
        /// Loads a remote file into the local collection in order to compare it when Compare() is called.
        /// </summary>
        /// <param name="host">Remote OS family.</param>    
        /// <param name="host">Remote host name.</param>                       
        /// <param name="username">The username wich will be used to connect with the remote host.</param>                       
        /// <param name="password">The password wich will be used to connect with the remote host.</param>                       
        /// <param name="password">The SSH port wich will be used to connect with the remote host.</param>                       
        /// <param name="path">Path to a file or folder; if the path points to a folder, the first file found using the FilePattern property will be loaded.</param>                       
        public virtual void Load(OS os, string host, string username, string password, int port, string path){   
            if(string.IsNullOrEmpty(host)) throw new ArgumentNullException("host");
            if(string.IsNullOrEmpty(username)) throw new ArgumentNullException("username");
            if(string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");

            var remote = new Shell(os, host, username, password, port);
            if(!string.IsNullOrEmpty(Path.GetExtension(path))){                                
                if(!remote.ExistsFile(path)) throw new FileNotFoundException("path");
                        
                path = remote.DownloadFile(path);
                Load(Path.GetDirectoryName(path), Path.GetFileName(path));
                File.Delete(path);    
            } 
            else{            
                string file = remote.GetFile(path, FilePattern, true);                
                if(string.IsNullOrEmpty(file)) throw new ArgumentInvalidException($"Unable to find any file using the search pattern '{FilePattern}'."); 

                path = remote.DownloadFile(path);           
                Load(path);
            }            
        } 

        /// <summary>
        /// Loads the given file into the local collection, in order to compare it when Compare() is called.
        /// </summary>
        /// <param name="folder">Path where the files will be looked for.</param>                       
        /// /// <param name="file">File that will be loaded into the copy detector.</param>
        public abstract void Load(string folder, string file);
        
        /// <summary>
        /// Performs the item comparison between each other.
        /// </summary>
        public abstract void Compare();
        
        /// <summary>
        /// Checks if a potential copy has been detected.
        /// The Compare() method should be called firts.
        /// </summary>
        /// <param name="path">The path to a compared file.</param>
        /// <returns>True of copy has been detected.</returns>
        public abstract bool CopyDetected(string path);
        
        /// <summary>
        /// Returns a printable details list, containing information about the comparissons (student, source and % of match).
        /// </summary>
        /// <param name="path">Student name</param>
        /// <returns>Left file followed by all the right files compared with its matching score.</returns>
        public abstract (string Folder, string File, (string Folder, string File, float Match)[] matches, float Threshold) GetDetails(string path);

        protected void ComputeAutoModeProperties(List<double> matches){
            var median = matches.Median();
            AutomaticThreshold = (float)(median + ((1 - median) * Threshold));
        }
    }
}