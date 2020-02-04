using System.Collections.Generic;

namespace AutomatedAssignmentValidator.CopyDetectors{
    public class Dummy: Core.CopyDetectorBase{     
        public override int Count {
            get {
                return 0;
            }
        }    
        public override void LoadFile(string path){
        }
        public override void Compare(){
        }
        public override bool CopyDetected(string path, float threshold){                             
            return false;
        }
        public override List<(string file, float match)> GetDetails(string path){
            return null;
        }
    }
}