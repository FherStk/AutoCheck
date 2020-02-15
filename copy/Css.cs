namespace AutomatedAssignmentValidator.CopyDetectors{
    public class Css: PlainText{                
             
        public Css(): base()
        {
            this.Extension = "css";
            this.WordsAmountWeight = 0.5f;
            this.WordCountWeight = 0.3f;
            this.LineCountWeight = 0.2f;            
        }                                       
    }
}