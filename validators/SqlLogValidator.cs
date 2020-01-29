using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace AutomatedAssignmentValidator{
    class SqlLogValidator: ValidatorBase{
        private class SqlLog{
            public string Student {get; set;}
            public string FilePath {get; set;}
            public int WordCount {get; set;}
            public int LineCount {get; set;}
            public Dictionary<string, int> WordsAmount {get; set;}
            public List<string> Content {get; set;}

            public SqlLog(string studentPath, string filePath){           
                this.Student = Utils.MoodleFolderToStudentName(studentPath);
                this.FilePath = filePath;
                this.Content = File.ReadAllLines(filePath).ToList();
                this.LineCount = this.Content.Count();                

                this.WordsAmount = new Dictionary<string, int>();
                foreach(string line in this.Content){
                    foreach(string word in line.Split(" ")){
                        if(!this.WordsAmount.ContainsKey(word)) this.WordsAmount.Add(word, 0);
                        this.WordsAmount[word]+=1;
                    }
                }

                this.WordCount = this.WordsAmount.Count();
            }
        }
        private const float _WORDCOUNT_THRESHOLD = 0.7f;
        private const float _LINECOUNT_THRESHOLD = 0.6f;
        private const float _WORDSAMOUNT_THRESHOLD = 0.25f;
        private const float _TOTAL_THRESHOLD = 0.5f;
        private const float _WORDCOUNT_WEIGHT = 0.3f;
        private const float _LINECOUNT_WEIGHT = 0.1f;
        private const float _WORDSAMOUNT_WEIGHT = 0.6f;
        private readonly static SqlLogValidator _instance = new SqlLogValidator();
        private List<SqlLog> SqlLogs = new List<SqlLog>();
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
            //If needed, for clearing the singleton accumulated data
            base.ClearResults();
        }
        public override List<TestResult> Validate(){                                                        
            try{
                Terminal.Write("Loading the log file... ");     
                string filePath = Directory.GetFiles(StudentFolder, "*.log", SearchOption.AllDirectories).FirstOrDefault();
                SqlLog left = new SqlLog(this.StudentFolder, filePath);
                Terminal.WriteResponse();

                //Comparing this file with all the others
                Terminal.WriteLine(string.Format("Comparing the file ~{0}~ with the previous ones: ", Path.GetFileName(filePath)), ConsoleColor.Yellow);     
                Terminal.Indent();                
                foreach(SqlLog right in this.SqlLogs){
                    Terminal.WriteLine(string.Format("File ~{0}~ from the student ~{1}~: ", Path.GetFileName(right.FilePath), right.Student), ConsoleColor.Yellow);     
                    Terminal.Indent();   
                    
                    //compare diferent items (word count, line count and word match) and get a % match
                    float diffWordCount = (left.WordCount <= right.WordCount ? ((float)left.WordCount / right.WordCount) : ((float)right.WordCount / left.WordCount));
                    Terminal.WriteLine(string.Format("Word count match: ~{0:P2} ", diffWordCount), (diffWordCount < _WORDCOUNT_THRESHOLD ? ConsoleColor.Green : ConsoleColor.Red));     
                    
                    float diffLineCount = (left.LineCount <= right.LineCount ? ((float)left.LineCount / right.LineCount) : ((float)right.LineCount / left.LineCount));
                    Terminal.WriteLine(string.Format("Line count match: ~{0:P2} ", diffLineCount), (diffLineCount < _LINECOUNT_THRESHOLD ? ConsoleColor.Green : ConsoleColor.Red));     

                    //TODO: test this ones
                    float diffAmount = (float)(CompareWordsAmount(left, right) + CompareWordsAmount(right, left))/2;
                    Terminal.WriteLine(string.Format("Words amount match: ~{0:P2} ", diffAmount), (diffAmount < _WORDSAMOUNT_THRESHOLD ? ConsoleColor.Green : ConsoleColor.Red));     

                    float total = (float)(diffWordCount * _WORDCOUNT_WEIGHT) + (diffLineCount * _LINECOUNT_WEIGHT) + (diffAmount * _WORDSAMOUNT_WEIGHT);
                    Terminal.WriteLine(string.Format("TOTAL match: ~{0:P2} ", total), (total < _WORDSAMOUNT_THRESHOLD ? ConsoleColor.Green : ConsoleColor.Red));     

                    Terminal.UnIndent();   
                }

                Terminal.WriteLine("Done!");     
                Terminal.BreakLine();
                Terminal.UnIndent();                

                //Adding the new log to the collection
                this.SqlLogs.Add(left);                                            
            }
            catch(Exception ex){
                Terminal.WriteResponse(ex.Message);
                return GlobalResults;
            }            
            
            return GlobalResults;
        } 

        private float CompareWordsAmount(SqlLog left, SqlLog right){
            int count = 0;
            float diff = 0;            
            
            foreach(string l in left.WordsAmount.Keys){
                count ++;
                diff += Math.Abs(left.WordsAmount[l] - (right.WordsAmount.ContainsKey(l) ? right.WordsAmount[l] : 0)) / left.WordsAmount[l];
            }

            return diff / count;
        }          
    }
}