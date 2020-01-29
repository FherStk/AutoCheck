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
                this.WordCount = this.Content.Sum(line => line.Split(" ").Count());

                this.WordsAmount = new Dictionary<string, int>();
                foreach(string line in this.Content){
                    foreach(string word in line.Split(" ")){
                        if(!this.WordsAmount.ContainsKey(word)) this.WordsAmount.Add(word, 0);
                        this.WordsAmount[word]+=1;
                    }
                }
            }
        }
        private readonly static SqlLogValidator _instance = new SqlLogValidator();
        private List<SqlLog> SqlLogs = new List<SqlLog>();
        public string StudentFolder {get; set;}

        private SqlLogValidator(): base()
        {
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
                SqlLog log = new SqlLog(this.StudentFolder, filePath);
                Terminal.WriteResponse();

                //Comparing this file with all the others
                Terminal.WriteLine(string.Format("Comparing the file ~{0}~ with the previous ones: ", Path.GetFileName(filePath)), ConsoleColor.Yellow);     
                Terminal.Indent();                
                foreach(SqlLog prev in this.SqlLogs){
                    Terminal.WriteLine(string.Format("File ~{0}~ from the student ~{1}~: ", Path.GetFileName(prev.FilePath), prev.Student), ConsoleColor.Yellow);     
                    Terminal.Indent();   
                    //compare diferent items (word count, line count and word match) and get a % match
                    Terminal.UnIndent();   
                }

                Terminal.WriteLine("Done!");     
                Terminal.BreakLine();
                Terminal.UnIndent();                

                //Adding the new log to the collection
                this.SqlLogs.Add(log);                                            
            }
            catch(Exception ex){
                Terminal.WriteResponse(ex.Message);
                return GlobalResults;
            }            
            
            return GlobalResults;
        }           
    }
}