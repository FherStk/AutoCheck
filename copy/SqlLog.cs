using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator.CopyDetectors{
    public class SqlLog: PlainText{                
             
        public SqlLog(): base()
        {
            this.Extension = "log";
        }                                       
    }
}