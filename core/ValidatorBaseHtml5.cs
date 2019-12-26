
using System;
using ExCSS;
using HtmlAgilityPack;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator{
    public abstract class ValidatorBaseHtml5 : ValidatorBase{
        public string StudentFolder {get; private set;}
        public HtmlDocument HtmlDoc {get; private set;}
        public Stylesheet CssDoc {get; private set;}
        protected ValidatorBaseHtml5(string studentFolder): base(){
            this.StudentFolder = studentFolder;
        } 

        protected bool LoadHtml5Document(string fileName){
            OpenTest(string.Format("   Validating the file ~{0}:", fileName), ConsoleColor.DarkBlue);
            CloseTest(null, 0);

            OpenTest("      Loading the file...");            
            HtmlDocument htmlDoc = Utils.LoadHtmlDocument(this.StudentFolder, fileName);        
            if(htmlDoc == null) CloseTest(new List<string>(){"Unable to read the HTML file."}, 0);
            else{
                CloseTest(null, 0);

                OpenTest("      Validating against the W3C official validation tool... ");
                if(Utils.W3CSchemaValidationForHtml5(htmlDoc)) CloseTest(null);
                else CloseTest(new List<string>(){"Unable to validate."}, 0);
            }

            this.HtmlDoc = htmlDoc;
            return htmlDoc != null;
        }

        protected bool LoadCss3Document(string fileName){
            OpenTest(string.Format("   Validating the file ~{0}:", fileName), ConsoleColor.DarkBlue);
            CloseTest(null, 0);
            Stylesheet stylesheet = null;

            OpenTest("      Loading the file...");                        
            string cssDoc = Utils.LoadCssDocument(this.StudentFolder, fileName);          
            if(!string.IsNullOrEmpty(cssDoc)) CloseTest(null, 0);
            else{                
                Utils.Write("      Loading another file: ");
                cssDoc = Utils.LoadCssDocument(this.StudentFolder, "*.css");
                if(string.IsNullOrEmpty(cssDoc)) CloseTest(new List<string>(){"Unable to read the CSS file."}, 0);
                else {
                    CloseTest(null, 0);
                    
                    OpenTest("      Validating against the W3C official validation tool... ");
                    if(!Utils.W3CSchemaValidationForCss3(cssDoc)) CloseTest(new List<string>(){"Unable to validate."}, 0);
                    else{
                        OpenTest("      Parsing the CSS file... ");
                        StylesheetParser parser = new StylesheetParser();       
                        stylesheet = parser.Parse(cssDoc);

                        if(stylesheet == null) CloseTest(new List<string>(){"Unable to parse the CSS file."}, 0);
                        else CloseTest(null);
                    }
                }
            }
                       
            this.CssDoc = stylesheet;
            return stylesheet != null;
        }
    }
}