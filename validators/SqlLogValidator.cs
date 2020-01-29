using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace AutomatedAssignmentValidator{
    class SqlLogValidator: ValidatorBase{
        private class SqlLog{
            int WordCount {get; set;}
            int LineCount {get; set;}
            Dictionary<string, int> WordsAmount {get; set;}
            List<string> Content {get; set;}

            public SqlLog(string filePath){                
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
            Terminal.Write("Loading the log file... ");     
            try{
                string filePath = Directory.GetFiles(StudentFolder, "*.log", SearchOption.AllDirectories).FirstOrDefault();
                SqlLog log = new SqlLog(filePath);

                
                Terminal.WriteResponse();
                Terminal.BreakLine();

                /*TODO: compare each file with its predecessors:
                    word count
                    line count
                    word match (amount of equal words contained on each file)

                    Each result will be stored in order to avoid recounting.
                */  

            }
            catch(Exception ex){
                Terminal.WriteResponse(ex.Message);
                return GlobalResults;
            }            
            
            return GlobalResults;
        }           
    }
}