using System.Collections.Generic;

namespace AutoCheck.CopyDetectors{
    /// <summary>
    /// Empty copy detector, use it in order to avoid copy detection.
    /// </summary>
    public class None: Core.CopyDetector{     
        public override int Count {
            get {
                return 0;
            }
        }    
        public override void Load(string path){
        }
        public override void Compare(){
        }
        public override bool CopyDetected(string path, float threshold){                             
            return false;
        }
        public override List<(string student, string source, float match)> GetDetails(string path){
            return null;
        }
    }
}