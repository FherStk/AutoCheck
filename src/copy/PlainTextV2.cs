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
using System.Collections.Generic;
using AutoCheck.Exceptions;

namespace AutoCheck.CopyDetectors{
    /// <summary>
    /// Copy detector for plain text files.
    /// </summary>
    public class PlainTextV2: Core.CopyDetectorV2{
        private class File{
            public string Folder {get; set;}
            public string Path {get; set;}
            public int WordCount {get; set;}
            public int LineCount {get; set;}
            public Dictionary<string, int> WordsAmount {get; set;}
            public List<string> Content {get; set;}

            public File(string folder, string file){                         
                Content = System.IO.File.ReadAllLines(System.IO.Path.Combine(folder, file)).ToList();
                WordsAmount = new Dictionary<string, int>();

                foreach(string line in Content){
                    foreach(string word in line.Split(" ")){
                        if(!WordsAmount.ContainsKey(word)) WordsAmount.Add(word, 0);
                        WordsAmount[word]+=1;
                    }
                }
                
                WordCount = WordsAmount.Sum(x => x.Value);
                LineCount = Content.Count();                
                Folder = folder;                  
                Path = file;
            }
        }        
        
        private Dictionary<string, int> Index {get; set;}        
        private List<File> Files {get; set;}
        private float[,] Matches {get; set;}                
        
        /// <summary>
        /// The weight that different words counting will have when computing the global matching percentage.
        /// </summary>
        /// <value></value>
        protected float WordsAmountWeight {get; set;}
        
        /// <summary>
        /// The weight that word counting will have when computing the global matching percentage.
        /// </summary>
        /// <value></value>
        protected float WordCountWeight {get; set;}
        
        /// <summary>
        /// The weight that line counting will have when computing the global matching percentage.
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
        /// Creates a new instance, setting up its properties in order to allow copy detection with the lowest possible false-positive probability.
        /// </summary>     
        public PlainTextV2(float threshold, string filePattern = "*.txt"): base(threshold, filePattern)
        {                 
            WordsAmountWeight = 0.7f;
            WordCountWeight = 0.2f;
            LineCountWeight = 0.1f;            

            Files = new List<File>();
            Index = new Dictionary<string, int>();
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
        /// /// <param name="file">File that will be loaded into the copy detector.</param>
        public override void Load(string folder, string file){   
            if(string.IsNullOrEmpty(folder)) throw new ArgumentNullException("path");
            if(string.IsNullOrEmpty(file)) throw new ArgumentNullException("file");            
            if(Index.ContainsKey(folder)) throw new ArgumentInvalidException("Only one file per folder is allowed.");   //Because files from different folders (students) are compared, and the folder will be de unique key to distinguish between sources.

            Index.Add(folder, Files.Count);
            Files.Add(new File(folder, file));                        
        }
        
        /// <summary>
        /// Compares all the previously loaded files, between each other.
        /// </summary>
        public override void Compare(){  
            if(WordCountWeight + LineCountWeight + WordsAmountWeight != 1f)
                throw new Exception("The summary of all the weights must be 100%, set the correct values and try again.");

            //Compute the changes and store the result in a matrix
            Matches = new float[Files.Count(), Files.Count()];                
            for(int i=0; i < Files.Count(); i++){
                File left = Files[i];

                for(int j=i+1; j < Files.Count(); j++){                                                                                            
                    File right = Files[j];

                    float diffWordCount = (left.WordCount <= right.WordCount ? ((float)left.WordCount / right.WordCount) : ((float)right.WordCount / left.WordCount));                    
                    float diffLineCount = (left.LineCount <= right.LineCount ? ((float)left.LineCount / right.LineCount) : ((float)right.LineCount / left.LineCount));
                    float diffAmount = (float)(CompareWordsAmount(left, right) + CompareWordsAmount(right, left))/2;
                    
                    Matches[i,j] = (float)(diffWordCount * WordCountWeight) + (diffLineCount * LineCountWeight) + (diffAmount * WordsAmountWeight);                        
                } 
            }

            //Copy the results that has been already computed
            for(int i=0; i < Files.Count(); i++){
                for(int j=i+1; j < Files.Count(); j++){
                    Matches[j,i] = Matches[i,j];
                }
            }
        }  
        
        /// <summary>
        /// Checks if a potential copy has been detected.
        /// The Compare() method should be called firts.
        /// </summary>
        /// <param name="source">The source item asked for.</param>
        /// <param name="threshold">The threshold value, a higher one will be considered as copy.</param>
        /// <returns>True of copy has been detected.</returns>
        public override bool CopyDetected(string path){
            if(string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if(!Index.ContainsKey(path)) throw new ArgumentInvalidException("The given 'path' has not been used within the current copy detector instance.");

            int i = Index[path];   
            for(int j=0; j < Files.Count(); j++){
                if(i != j){
                    if(Matches[i,j] >= Threshold) return true;     
                }                        
            }            
           
            return false;
        }
        
        /// <summary>
        /// Returns a printable details list, containing information about the comparissons (student, source and % of match).
        /// </summary>
        /// <param name="path">Path where the files has been loaded.</param>
        /// <returns>Left file followed by all the right files compared with its matching score.</returns>
        public override (string folder, string file, (string folder, string file, float match)[] matches) GetDetails(string path){
            int i = Index[path];   
            var matches = new List<(string, string, float)>();            
            for(int j=0; j < Files.Count(); j++){                
                if(i != j) matches.Add((Files[j].Folder, Files[j].Path, Matches[i,j]));                     
            }            
           
            return (Files[i].Folder, Files[i].Path, matches.ToArray());
        }
        
        private float CompareWordsAmount(File left, File right){            
            int count = 0;            
            float diff = 0;                        
            
            foreach(string l in left.WordsAmount.Keys){                
                int wordsAmountRight = (right.WordsAmount.ContainsKey(l) ? right.WordsAmount[l] : 0);

                try{                    
                    diff += (left.WordsAmount[l] <= wordsAmountRight ? ((float)left.WordsAmount[l] / wordsAmountRight) : ((float)wordsAmountRight / left.WordsAmount[l]));
                }
                catch(DivideByZeroException){
                    diff += 0;
                }
                finally{
                    count ++;
                }
            }

            return diff / count;
        }          
    }
}