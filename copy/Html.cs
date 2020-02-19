namespace AutoCheck.CopyDetectors{
    /// <summary>
    /// Copy detector for HTML files.
    /// </summary>
    public class Html: PlainText{                
        /// <summary>
        /// Creates a new instance, setting up its properties in order to allow copy detection with the lowest possible false-positive probability.
        /// </summary>     
        public Html(): base()
        {
            this.Extension = "html";
            this.WordsAmountWeight = 0.5f;
            this.WordCountWeight = 0.3f;
            this.LineCountWeight = 0.2f;            
        }                                       
    }
}