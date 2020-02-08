using System.Collections.Generic;

namespace AutomatedAssignmentValidator.Core{
    public abstract class CopyDetector{
        public abstract int Count { get;}
        public abstract void LoadFile(string path);
        public abstract void Compare();
        public abstract bool CopyDetected(string path, float threshold);
        public abstract List<(string file, float match)> GetDetails(string path);
    }
}