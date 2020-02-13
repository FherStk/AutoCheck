namespace AutomatedAssignmentValidator.CopyDetectors{
    public class Html: PlainText{                
             
        public Html(): base()
        {
            this.Extension = "html";
            this.WordsAmountWeight = 0.5f;
            this.WordCountWeight = 0.3f;
            this.LineCountWeight = 0.2f;            
        }                                       
    }
}