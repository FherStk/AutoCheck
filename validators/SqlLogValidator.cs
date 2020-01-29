using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace AutomatedAssignmentValidator{
    //NOTE: This is not a regular validator for any assignment, this will not promt any scores for any exercise. 
    //      This validator will load an SQL log file on each "Validate" call and, once called its "Compare" method, it will proceed to compare the logs
    //      between each other in order to get a match % to avoid copies between the students.
    //      The logic has been "tweaked" in order to profit the regular validators system.
    
    class SqlLogValidator: ValidatorBase{
        private class SqlLog{
            public string Student {get; set;}
            public string FilePath {get; set;}
            public int WordCount {get; set;}
            public int LineCount {get; set;}
            public Dictionary<string, int> WordsAmount {get; set;}
            public List<string> Content {get; set;}

            public SqlLog(string studentPath, string filePath){           
                                
                this.Content = File.ReadAllLines(filePath).ToList();
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
                this.Student = Utils.MoodleFolderToStudentName(studentPath);
            }
        }
        private const float _MATCH_THRESHOLD = 0.7f;
        private const float _WORDCOUNT_WEIGHT = 0.15f;
        private const float _LINECOUNT_WEIGHT = 0.05f;
        private const float _WORDSAMOUNT_WEIGHT = 0.8f;
        private readonly static SqlLogValidator _instance = new SqlLogValidator();
        private List<SqlLog> _LOGS = new List<SqlLog>();
        private float[,] _SCORES;
        public string StudentFolder {get; set;}        

        private SqlLogValidator(): base()
        {
            if(_WORDCOUNT_WEIGHT + _LINECOUNT_WEIGHT + _WORDSAMOUNT_WEIGHT != 1f)
                throw new Exception("Please, the summary of all the weights must be 100%");
        }
 
        public static SqlLogValidator Instance
        {
            get
            {
                return _instance;
            }
        }        
        
        public new void ClearResults(){
            _SCORES = null;
            _LOGS.Clear();

            base.ClearResults();
        }
        public override List<TestResult> Validate(){                                                        
            try{
                Terminal.Write("Loading the log file... ");     
                string filePath = Directory.GetFiles(StudentFolder, "*.log", SearchOption.AllDirectories).FirstOrDefault();
                SqlLog left = new SqlLog(this.StudentFolder, filePath);
                Terminal.WriteResponse();
                
                //Adding the new log to the collection
                this._LOGS.Add(left);                                            
            }
            catch(Exception ex){
                Terminal.WriteResponse(ex.Message);
                return GlobalResults;
            }            
            
            return GlobalResults;
        } 
        public void Compare(){                                                                   
            //Compute the changes and store the result in a matrix
            _SCORES = new float[this._LOGS.Count(), this._LOGS.Count()];                
            for(int i=0; i < this._LOGS.Count(); i++){
                SqlLog left = this._LOGS[i];

                for(int j=i+1; j < this._LOGS.Count(); j++){                                                                                            
                    SqlLog right = this._LOGS[j];

                    float diffWordCount = (left.WordCount <= right.WordCount ? ((float)left.WordCount / right.WordCount) : ((float)right.WordCount / left.WordCount));                    
                    float diffLineCount = (left.LineCount <= right.LineCount ? ((float)left.LineCount / right.LineCount) : ((float)right.LineCount / left.LineCount));
                    float diffAmount = (float)(CompareWordsAmount(left, right) + CompareWordsAmount(right, left))/2;
                    
                    _SCORES[i,j] = (float)(diffWordCount * _WORDCOUNT_WEIGHT) + (diffLineCount * _LINECOUNT_WEIGHT) + (diffAmount * _WORDSAMOUNT_WEIGHT);                        
                } 
            }

            //Copy the results that has been already computed
            for(int i=0; i < this._LOGS.Count(); i++){
                for(int j=i+1; j < this._LOGS.Count(); j++){
                    _SCORES[j,i] = _SCORES[i,j];
                }
            }
        } 
        public void Print(){                                                        
            try{       

                List<string> copies = new List<string>();

                for(int i=0; i < this._LOGS.Count(); i++){
                    SqlLog left = this._LOGS[i];
                    Terminal.WriteLine(string.Format("Results for ~{0}~ from the student ~{1}~: ", Path.GetFileName(left.FilePath), left.Student), ConsoleColor.Yellow);  
                    Terminal.Indent();   

                    for(int j=0; j < this._LOGS.Count(); j++){
                        if(i != j){
                            SqlLog right = this._LOGS[j];
                            Terminal.Write(string.Format("Matching with ~{0}~ from the student ~{1}~: ", Path.GetFileName(right.FilePath), right.Student), ConsoleColor.Yellow);     
                            Terminal.WriteLine(string.Format("~{0:P2} ", _SCORES[i,j]), (_SCORES[i,j] < _MATCH_THRESHOLD ? ConsoleColor.Green : ConsoleColor.Red));

                            if(_SCORES[i,j] >= _MATCH_THRESHOLD) copies.Add(string.Format("{0} -> {1}: ~{2:P2}", left.Student, right.Student, _SCORES[i,j]));     
                        }                        
                    }
                    
                    Terminal.BreakLine();
                    Terminal.UnIndent();  
                }

                if(copies.Count == 0) Terminal.WriteLine("No copies has been detected.", ConsoleColor.Green);
                else{
                    Terminal.WriteLine("The following copies has been detected:", ConsoleColor.Red);
                    Terminal.Indent();  
                    
                    foreach(string c in copies)
                        Terminal.WriteLine(c, ConsoleColor.Red);

                    Terminal.UnIndent();  
                }
            }
            catch(Exception ex){
                Terminal.WriteResponse(ex.Message);
            }                        
        } 

        private float CompareWordsAmount(SqlLog left, SqlLog right){            
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