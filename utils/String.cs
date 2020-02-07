
using System.Text;
using System.Globalization;

namespace AutomatedAssignmentValidator.Utils{    
    public partial class String{  
        public static string RemoveDiacritics(string text) 
        {
            //Manual replacement step (due wrong format from source)
            text = text.Replace("Ã©", "é");

            //Source: https://stackoverflow.com/a/249126
            string norm = text.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            foreach (char c in norm)
            {
                UnicodeCategory cat = CharUnicodeInfo.GetUnicodeCategory(c);
                if (cat != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }  
    }
}