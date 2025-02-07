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
using System.Linq;
using System.Collections.Generic;
using AutoCheck.Core.Exceptions;
using Google.DiffMatchPatch;

namespace AutoCheck.Core.CopyDetectors{
    /// <summary>
    /// Copy detector for plain text files.
    /// </summary>
    public class PlainText: Base{
        protected class File{
            public string FolderPath {get; set;}
            public string FolderName {get; set;}
            public string FilePath {get; set;}
            public string FileName {get; set;}
            public int WordCount {get; set;}
            public int LineCount {get; set;}
            public List<string> Content {get; set;}

            public File(string folder, string file){                         
                Content = System.IO.File.ReadAllLines(System.IO.Path.Combine(folder, file)).ToList();
                                
                WordCount = Content.SelectMany(x => x.Split(" ")).Count();
                LineCount = Content.Count();
                FolderPath = folder;                                  
                FolderName = System.IO.Path.GetFileName(folder);
                FilePath = file;
                FileName = System.IO.Path.GetFileName(file);
            }

            public override string ToString(){
                return string.Join(System.Environment.NewLine, Content);
            } 
        }        
        
        protected Dictionary<string, int> Index {get; set;}        
        protected List<File> Files {get; set;}
        protected float[,] Matches {get; set;}                
        protected List<Diff>[,] Diffs {get; set;}
        
        /// <summary>
        /// The weight that sentence matching (different documents with the same sentences within) will have when computing the global matching percentage.
        /// </summary>
        /// <value></value>
        protected float SentenceMatchWeight {get; set;}
        
        /// <summary>
        /// The weight that word counting (different documents with the same amount of words) will have when computing the global matching percentage.
        /// </summary>
        /// <value></value>
        protected float WordCountWeight {get; set;}
        
        /// <summary>
        /// The weight that line counting (different documents with the same amount of lines) will have when computing the global matching percentage.
        /// </summary>
        /// <value></value>
        protected float LineCountWeight {get; set;}   
        
        /// <summary>
        /// The quantity of files loaded.
        /// </summary>
        /// <value></value>
        public override int Count {
            get {
                return Files.Count();
            }
        }   
        
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="threshold">Matches above this value will be computed as potential copies.</param>
        /// <param name="sensibility">The copy detection sensibility, lower values increases the probability of false positives.</param>
        /// <param name="mode">The comparisson mode.</param>
        /// <param name="filePattern">Only the files mathing this pattern will be compared.</param>
        public PlainText(float threshold, int sensibility, DetectionMode mode, string filePattern = "*.txt"): base(threshold, sensibility, mode, filePattern)
        {                 
            SentenceMatchWeight = 0.7f;
            WordCountWeight = 0.2f;
            LineCountWeight = 0.1f;            

            Files = new List<File>();
            Index = new Dictionary<string, int>();
        } 

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="threshold">Matches above this value will be computed as potential copies.</param>
        /// <param name="sensibility">The copy detection sensibility, lower values increases the probability of false positives.</param>
        /// <param name="filePattern">Only the files mathing this pattern will be compared.</param>
        /// <returns></returns>
        public PlainText(float threshold, int sensibility, string filePattern = "*.txt"): this(threshold, sensibility, DetectionMode.DEFAULT, filePattern){           
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="threshold">Matches above this value will be computed as potential copies.</param>
        /// <param name="mode">The comparisson mode.</param>
        /// <param name="filePattern">Only the files mathing this pattern will be compared.</param>
        /// <returns></returns>
        public PlainText(float threshold, DetectionMode mode, string filePattern = "*.txt"): this(threshold, -1, mode, filePattern){           
        }

        /// <summary>
        /// Creates a new instance, setting up its properties in order to allow copy detection with the lowest possible false-positive probability.
        /// </summary>
        /// <param name="threshold">Matches above this value will be computed as potential copies.</param>
        /// <param name="filePattern">Only the files mathing this pattern will be compared.</param>
        /// <returns></returns>
        public PlainText(float threshold, string filePattern = "*.txt"): this(threshold, -1, DetectionMode.DEFAULT, filePattern){           
        } 
        
        /// <summary>
        /// Disposes the current copy detector instance and releases its internal objects.
        /// </summary>
        public override void Dispose(){
            Index.Clear();
            Files.Clear();
        }     

        /// <summary>
        /// Loads the given file into the local collection, in order to compare it when Compare() is called.
        /// </summary>
        /// <param name="folder">Path where the files will be looked for.</param>                       
        /// <param name="file">File that will be loaded into the copy detector.</param>
        public override void Load(string folder, string file){   
            if(string.IsNullOrEmpty(folder)) throw new ArgumentNullException("path");
            if(string.IsNullOrEmpty(file)) throw new ArgumentNullException("file");            
            if(Index.ContainsKey(folder)) throw new ArgumentInvalidException("Two compared files cannot share the same folder because this folder must be used as an unique key.");   //Because files from different folders (students) are compared, and the folder will be de unique key to distinguish between sources.

            Index.Add(folder, Files.Count);
            Files.Add(new File(folder, file));                        
        }
        
        /// <summary>
        /// Compares all the previously loaded files, between each other.
        /// </summary>
        public override void Compare(){  
            if(WordCountWeight + LineCountWeight + SentenceMatchWeight != 1f)
                throw new Exception("The summary of all the weights must be 100%, set the correct values and try again.");
            
            //Compute the changes and store the result in a matrix
            DiffMatchPatch dmp = new DiffMatchPatch();
            dmp.DiffTimeout = 0;

            var accum = new List<double>();
            Matches = new float[Files.Count(), Files.Count()];                
            Diffs = new List<Diff>[Files.Count(), Files.Count()];
            for(int i=0; i < Files.Count(); i++){
                File left = Files[i];

                for(int j=i; j < Files.Count(); j++){                                                                                            
                    File right = Files[j];
                                        
                    List<Diff> diff = dmp.DiffMain(left.ToString(), right.ToString());
                    if(i == j) Matches[i,j] = 1;    //Optimization
                    else{
                        float diffAmount = (float)diff.Where(x => x.Operation == Operation.EQUAL).Count() / diff.Count;
                        float diffWordCount = (left.WordCount <= right.WordCount ? ((float)left.WordCount / right.WordCount) : ((float)right.WordCount / left.WordCount));                    
                        float diffLineCount = (left.LineCount <= right.LineCount ? ((float)left.LineCount / right.LineCount) : ((float)right.LineCount / left.LineCount));

                        var match = (float)(diffWordCount * WordCountWeight) + (diffLineCount * LineCountWeight) + (diffAmount * SentenceMatchWeight);                        
                        Matches[i,j] = match;
                        Matches[j, i] = match;
                        
                        //This will be used to compute the median
                        accum.Add(match);        
                    }

                    //This should be always added                                        
                    Diffs[i,j] = diff; 
                    Diffs[j,i] = Diffs[i,j];         
                } 
            }

            //Computing the median if needed
            if(Mode == DetectionMode.AUTO) ComputeAutoModeProperties(accum);
        }                 
                
        /// <summary>
        /// Checks if a potential copy has been detected.
        /// The Compare() method should be called firts.
        /// </summary>
        /// <param name="path">The path to a compared file.</param>
        /// <returns>True of copy has been detected.</returns>
        public override bool CopyDetected(string path){
            if(string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if(!Index.ContainsKey(path)) throw new ArgumentInvalidException("The given 'path' has not been used within the current copy detector instance.");
            
            int i = Index[path];                           
            for(int j=0; j < Files.Count(); j++){
                if(i != j){
                    switch(Mode){
                        case DetectionMode.DEFAULT:
                            if(Matches[i,j] >= Threshold) return true;     
                            break;
                        
                        case DetectionMode.AUTO:
                            if(Matches[i,j] >= AutomaticThreshold) return true;
                            break;
                    }                    
                }                        
            }            

            return false;
        }
        
        /// <summary>
        /// Returns a printable details list, containing information about the comparissons (student, source and % of match).
        /// </summary>
        /// <param name="path">Path where the files has been loaded.</param>
        /// <returns>Left file followed by all the right files compared with its matching score.</returns>
        public override (string Folder, string File, (string Folder, string File, float Match)[] matches, float Threshold) GetDetails(string path){
            int i = Index[path];   
            var matches = new List<(string, string, float)>();            
            for(int j=0; j < Files.Count(); j++){                
                if(i != j) matches.Add((Files[j].FolderPath, Files[j].FilePath, Matches[i,j]));                     
            }            
           
            return (Files[i].FolderPath, Files[i].FilePath, matches.ToArray(), (Mode == DetectionMode.DEFAULT ? Threshold : AutomaticThreshold));
        }       
    }
}