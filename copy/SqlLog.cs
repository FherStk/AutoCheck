namespace AutomatedAssignmentValidator.CopyDetectors{
    /// <summary>
    /// Copy detector for SQL log files.
    /// </summary>
    public class SqlLog: PlainText{                
             
        /// <summary>
        /// Creates a new instance, setting up its properties in order to allow copy detection with the lowest possible false-positive probability.
        /// </summary>     
        public SqlLog(): base()
        {
            this.Extension = "log";
            this.WordsAmountWeight = 0.85f;
            this.WordCountWeight = 0.1f;
            this.LineCountWeight = 0.05f; 
        }                                       
    }
}