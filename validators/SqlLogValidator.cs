using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace AutomatedAssignmentValidator{
    class SqlLogValidator: ValidatorBase{
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
                List<string> file = LoadLogFile();
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

        private List<string> LoadLogFile(){          
            string filePath = Directory.GetFiles(StudentFolder, "*.log", SearchOption.AllDirectories).FirstOrDefault();
            return File.ReadAllLines(filePath).ToList();            
        }              
    }
}