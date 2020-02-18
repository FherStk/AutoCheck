using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator.CopyDetectors{
    /// <summary>
    /// Copy detector for plain text files.
    /// </summary>
    public class PlainText: Core.CopyDetector{
        private class File{
            public string Student {get; set;}
            public string FilePath {get; set;}
            public int WordCount {get; set;}
            public int LineCount {get; set;}
            public Dictionary<string, int> WordsAmount {get; set;}
            public List<string> Content {get; set;}

            public File(string studentPath, string filePath){           
                                
                this.Content = System.IO.File.ReadAllLines(filePath).ToList();
                this.WordsAmount = new Dictionary<string, int>();

                foreach(string line in this.Content){
                    foreach(string word in line.Split(" ")){
                        if(!this.WordsAmount.ContainsKey(word)) this.WordsAmount.Add(word, 0);
                        this.WordsAmount[word]+=1;
                    }
                }

                this.FilePath = filePath;
                this.WordCount = this.WordsAmount.Sum(x => x.Value);
                this.LineCount = this.Content.Count();                
                this.Student = Core.Utils.FolderNameToStudentName(studentPath);
            }
        }        
        private Dictionary<string, int> Index {get; set;}
        private List<File> Files {get; set;}
        private float[,] Matches {get; set;}    
        /// <summary>
        /// File extension
        /// </summary>
        /// <value></value>
        protected string Extension {get; set;}  
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
        public PlainText(): base()
        {
            //NOTE: this has been built as is because this kind of CopyDetector can also be used for other kind of plain text files...
            //      sadly, the constructor must remain with no parameters (generic usage of the CopyDetector within Script) so
            //      if needed, this class can be inherited as SqlLog is doing :)            
            this.Extension = "txt";
            this.WordsAmountWeight = 0.7f;
            this.WordCountWeight = 0.2f;
            this.LineCountWeight = 0.1f;            

            this.Files = new List<File>();
            this.Index = new Dictionary<string, int>();
        } 
        /// <summary>
        /// Loads the given file into the local collection, in order to compare it when Compare() is called.
        /// </summary>
        /// <param name="path"></param>                       
        public override void Load(string path){                                                        
            string filePath = Directory.GetFiles(path, string.Format("*.{0}", this.Extension), SearchOption.AllDirectories).FirstOrDefault();            
            if(string.IsNullOrEmpty(filePath)) throw new Exception(string.Format("Unable to find any file '*.{0}' file.", this.Extension));
            else{
                this.Index.Add(path, this.Files.Count);
                this.Files.Add(new File(path, filePath));            
            }
        } 
        /// <summary>
        /// Compares all the previously loaded files, between each other.
        /// </summary>
        public override void Compare(){  
            if(WordCountWeight + LineCountWeight + WordsAmountWeight != 1f)
                throw new Exception("The summary of all the weights must be 100%, set the correct values and try again.");

            //Compute the changes and store the result in a matrix
            Matches = new float[this.Files.Count(), this.Files.Count()];                
            for(int i=0; i < this.Files.Count(); i++){
                File left = this.Files[i];

                for(int j=i+1; j < this.Files.Count(); j++){                                                                                            
                    File right = this.Files[j];

                    float diffWordCount = (left.WordCount <= right.WordCount ? ((float)left.WordCount / right.WordCount) : ((float)right.WordCount / left.WordCount));                    
                    float diffLineCount = (left.LineCount <= right.LineCount ? ((float)left.LineCount / right.LineCount) : ((float)right.LineCount / left.LineCount));
                    float diffAmount = (float)(CompareWordsAmount(left, right) + CompareWordsAmount(right, left))/2;
                    
                    Matches[i,j] = (float)(diffWordCount * WordCountWeight) + (diffLineCount * LineCountWeight) + (diffAmount * WordsAmountWeight);                        
                } 
            }

            //Copy the results that has been already computed
            for(int i=0; i < this.Files.Count(); i++){
                for(int j=i+1; j < this.Files.Count(); j++){
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
        public override bool CopyDetected(string path, float threshold){
            int i = Index[path];   
            for(int j=0; j < Files.Count(); j++){
                if(i != j){
                    if(Matches[i,j] >= threshold) return true;     
                }                        
            }            
           
            return false;
        }
        /// <summary>
        /// Returns a printable details list, containing information about the comparissons (student, source and % of match).
        /// </summary>
        /// <param name="path">Student name</param>
        /// <returns>A list of tuples, on each one will contain information about the current student, the source compared with and the % of match. </returns>
        public override List<(string student, string source, float match)> GetDetails(string path){
            int i = Index[path];   
            var matches = new List<(string, string, float)>();            
            for(int j=0; j < Files.Count(); j++){
                if(i != j)
                    matches.Add((Files[j].Student, Files[j].FilePath, Matches[i,j]));                     
            }            
           
            return matches;
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