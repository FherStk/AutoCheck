namespace AutomatedAssignmentValidator.CopyDetectors{
    public class SqlLog: PlainText{                
             
        public SqlLog(): base()
        {
            this.Extension = "log";
            this.WordsAmountWeight = 0.85f;
            this.WordCountWeight = 0.1f;
            this.LineCountWeight = 0.05f; 
        }                                       
    }
}