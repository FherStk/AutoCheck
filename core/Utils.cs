namespace AutomatedAssignmentValidator{
    class Utils{    
        public static string DataBaseToStudentName(string database){
            return database.Substring(database.IndexOf("_")+1).Replace("_", " ");
        }   
    }
}