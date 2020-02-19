namespace AutomatedAssignmentValidator.CopyDetectors{
    /// <summary>
    /// Copy detector for CSS files.
    /// </summary>
    public class Css: PlainText{                
        /// <summary>
        /// Creates a new instance, setting up its properties in order to allow copy detection with the lowest possible false-positive probability.
        /// </summary>     
        public Css(): base()
        {
            this.Extension = "css";
            this.WordsAmountWeight = 0.5f;
            this.WordCountWeight = 0.3f;
            this.LineCountWeight = 0.2f;            
        }                                       
    }
}