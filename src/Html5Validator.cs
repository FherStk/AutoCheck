using System;
using System.Linq;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace AutomatedAssignmentValidator{
    class Html5Validator{
        public static void ValidateIndex(string studentFolder)
        {
            string fileName = "index.html";
            Utils.Write("   Validating the file: ");
            Utils.WriteLine(fileName, ConsoleColor.DarkBlue);

            HtmlDocument htmlDoc = Utils.LoadHtmlDocument(studentFolder, fileName);
            if(htmlDoc == null){
                Utils.WriteLine(string.Format("ERROR! {0}", "Unable to read the HTML file."), ConsoleColor.Red);
                return;
            }
        
            Utils.Write("      Validating the headers... ");
            Utils.PrintResults(CheckHeaders(htmlDoc));
            
            Utils.Write("      Validating the paragraphs... ");
            Utils.PrintResults(CheckParagraph(htmlDoc));              
            
            Utils.Write("      Validating the break-lines... ");
            Utils.PrintResults(CheckBreakLines(htmlDoc));

            Utils.Write("      Validating the images... ");
            Utils.PrintResults(CheckImages(htmlDoc));      

            Utils.Write("      Validating the unordered list... ");
            Utils.PrintResults(CheckList(htmlDoc));  

            Utils.Write("      Validating the links... ");
            Utils.PrintResults(CheckLinks(htmlDoc));                                     
        }  

        public static void ValidateContacte(string studentFolder)
        {
            string fileName = "contacte.html";
            Utils.Write("   Validating the file: ");
            Utils.WriteLine(fileName, ConsoleColor.DarkBlue);

            HtmlDocument htmlDoc = Utils.LoadHtmlDocument(studentFolder, fileName);
            if(htmlDoc != null){
                Utils.Write("      Validating the text fields... ");
                Utils.PrintResults(CheckInputFields(htmlDoc, "text", 2));

                Utils.Write("      Validating the number fields... ");
                Utils.PrintResults(CheckInputFields(htmlDoc, "number", 1));

                Utils.Write("      Validating the email fields... ");
                Utils.PrintResults(CheckInputFields(htmlDoc, "email", 1));

                Utils.Write("      Validating the radio fields... ");
                Utils.PrintResults(CheckInputFields(htmlDoc, "radio", 3));

                Utils.Write("      Validating the select fields... ");
                Utils.PrintResults(CheckSelectFields(htmlDoc));

                Utils.Write("      Validating the checkbox fields... ");
                Utils.PrintResults(CheckInputFields(htmlDoc, "checkbox", 3));

                Utils.Write("      Validating the textarea fields... ");
                Utils.PrintResults(CheckTextareaFields(htmlDoc));

                Utils.Write("      Validating the placeholders... ");
                Utils.PrintResults(CheckPlaceholders(htmlDoc));

                Utils.Write("      Validating the tables... ");
                Utils.PrintResults(CheckTables(htmlDoc));

                Utils.Write("      Validating the reset button... ");
                Utils.PrintResults(CheckReset(htmlDoc));

                Utils.Write("      Validating the submit button... ");
                Utils.PrintResults(CheckSubmit(htmlDoc));
            }                      
        }                    

        private static List<string> CheckHeaders(HtmlDocument htmlDoc){
            List<string> errors = new List<string>();

            try{
                HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes("//h1");
                if(nodes == null || nodes.Count < 1) errors.Add("Does not conatins any level-1 header.");
                
                nodes = htmlDoc.DocumentNode.SelectNodes("//h2");            
                if(nodes == null || nodes.Count < 1) errors.Add("Does not conatins any level-2 header.");
            }
            catch(Exception e){
                errors.Add(string.Format("EXCEPTION: {0}", e.Message));
            }
        
            return errors;
        }

        private static List<string> CheckParagraph(HtmlDocument htmlDoc)
        {
            int count = 0;
            int length = 0;
            List<string> errors = new List<string>();

            try{
                foreach (HtmlNode p in htmlDoc.DocumentNode.SelectNodes("//p"))
                {
                    count++;
                    length += p.InnerText.Length;
                }
                
                if (count < 2) errors.Add("does not contains enough paragraphs.");
                if (length < 1500) errors.Add("does not contains enought text inside the paragraphs.");
            }
            catch(Exception e){
               errors.Add(string.Format("EXCEPTION: {0}", e.Message));
            }
        
            return errors;
        }

        private static List<string> CheckBreakLines(HtmlDocument htmlDoc)
        {
            int count = 0;
            int length = 0;
            List<string> errors = new List<string>();

            try{                
                foreach (HtmlNode p in htmlDoc.DocumentNode.SelectNodes("//p"))
                {
                    count++;
                    length += p.InnerText.Length;
                }
                    
                HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes("//p/br");
                if (nodes == null || nodes.Count < 1) errors.Add("does not contains a break-line inside a paragraph.");

            }
            catch(Exception e){
               errors.Add(string.Format("EXCEPTION: {0}", e.Message));
            }                        

            return errors;
        }

        private static List<string> CheckImages(HtmlDocument htmlDoc){
            List<string> errors = new List<string>();

            try{
                HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes("//img");            
                if(nodes == null || nodes.Count < 1) errors.Add("Does not contains any image.");
            }
            catch(Exception e){
               errors.Add(string.Format("EXCEPTION: {0}", e.Message));
            }

            return errors;
        }  

        private static List<string> CheckList(HtmlDocument htmlDoc){
            List<string> errors = new List<string>();
            try{
                HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes("//ul");            
                if(nodes == null || nodes.Count < 1) errors.Add("Does not contains any unordered list.");
                else{
                    foreach(HtmlNode ul in nodes){
                        HtmlNodeCollection n = ul.SelectNodes("li"); 
                        if(n != null && n.Count >= 2) return errors;                        
                    }
                }  
            }
            catch(Exception e){
               errors.Add(string.Format("EXCEPTION: {0}", e.Message));
            }                      

            errors.Add("Unable to find any unordered list with at least two items inside.");
            return errors;
        }  

        private static List<string> CheckLinks(HtmlDocument htmlDoc){
            List<string> errors = new List<string>();

            try{
                 HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes("//ul/li/a");            
                if(nodes == null || nodes.Count < 1) errors.Add("Does not contains any link inside an unordered list.");
                else{
                    bool index = false;
                    bool contacte = false;
                    foreach(HtmlNode a in nodes){
                        if(a.GetAttributeValue("href", "").ToLower() == "index.html") index = true;
                        if(a.GetAttributeValue("href", "").ToLower() == "contacte.html") contacte = true;

                        if(index && contacte) break;
                    }

                    if(!index) errors.Add("Unable to find any link inside an unordered list pointing to index.html.");
                    if(!contacte) errors.Add("Unable to find any link inside an unordered list pointing to contacte.html.");
                }
            }
            catch(Exception e){
               errors.Add(string.Format("EXCEPTION: {0}", e.Message));
            }           
           
            return errors;
        } 

        private static List<string> CheckInputFields(HtmlDocument htmlDoc, string type, int min){
            List<string> errors = new List<string>();

            try{
                HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes("//input");
                if(nodes == null) errors.Add(string.Format("Does not contains any {0} fields.", type));
                else{
                    //TODO: get the nodes using XPath... I can't get the correct one, maybe a bug? //input[@type='text']
                    //TODO: solved in Css3Validator (method CheckCssProperty)
                    List<HtmlNode> filtered = nodes.Where(x => x.GetAttributeValue("type", "").Equals(type)).ToList();
                    if(filtered.Count() < min) errors.Add(string.Format("Does not contains enough {0} fields.", type));
                    else if(type == "radio" || type == "checkbox"){
                        if(filtered.GroupBy(x => x.GetAttributeValue("name", "")).Count() > 1) errors.Add(string.Format("The {0} fields does not share the same name.", type));
                        if(filtered.Where(x => x.Attributes.Where(y => y.Name == "checked").Count() > 0).Count() != 1) errors.Add(string.Format("The {0} fields does not have a single default value.", type));
                    }

                    errors.AddRange(CheckLabels(htmlDoc, filtered, type));
                }   
            }
            catch(Exception e){
               errors.Add(string.Format("EXCEPTION: {0}", e.Message));
            }                       

            return errors;
        }

        private static  List<string> CheckLabels(HtmlDocument htmlDoc, List<HtmlNode> fields, string type){
            List<string> errors = new List<string>();
        
            try{
                foreach(HtmlNode node in fields){
                    string id = node.GetAttributeValue("id", "");
                    if(string.IsNullOrEmpty(id)) errors.Add(string.Format("The current {0} field does not have an ID so no label can be related to it.", type));
                    else{
                        HtmlNodeCollection labels = htmlDoc.DocumentNode.SelectNodes("//label");
                        if(labels == null) errors.Add(string.Format("There are no labels in the document for the current {0} field.", type));
                        else{
                            List<HtmlNode> related = labels.Where(x => x.GetAttributeValue("for", "").Equals(id)).ToList();
                            if(related.Count != 1) errors.Add(string.Format("No label (or more than one) has been found for the current {0} field.", type));
                        }
                    }
                }                
            }
            catch(Exception e){
               errors.Add(string.Format("EXCEPTION: {0}", e.Message));
            }

            return errors;
        }   

        private static List<string> CheckSelectFields(HtmlDocument htmlDoc){
            List<string> errors = new List<string>();

            try{
                HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes("//select");                        
                if(nodes == null || nodes.Count < 1) errors.Add("Does not contains enough select fields.");
                else errors.AddRange(CheckLabels(htmlDoc, nodes.ToList(), "select"));

                nodes = htmlDoc.DocumentNode.SelectNodes("//select/option"); 
                if(nodes == null || nodes.Count < 3) errors.Add("The select field does not contains enough options.");
                else{
                    if(nodes.Where(x => x.Attributes.Where(y => y.Name == "selected").Count() > 0).Count() != 1) errors.Add("The select field does not have a single default option.");                
                }  
                
            }
            catch(Exception e){
               errors.Add(string.Format("EXCEPTION: {0}", e.Message));
            }         

            return errors;
        }   

        private static List<string> CheckTextareaFields(HtmlDocument htmlDoc){
            List<string> errors = new List<string>();

            try{
                HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes("//textarea");                        
                if(nodes == null || nodes.Count < 1) errors.Add("Does not contains enough textarea fields.");            
                else errors.AddRange(CheckLabels(htmlDoc, nodes.ToList(), "textarea"));
            }
            catch(Exception e){
               errors.Add(string.Format("EXCEPTION: {0}", e.Message));
            }

            return errors;
        }  

        private static List<string> CheckPlaceholders(HtmlDocument htmlDoc){
            List<string> errors = new List<string>();

            try{
                HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes("//input");
                if(nodes == null) errors.Add("Unable to find any placeholder.");          
                else{
                    List<HtmlNode> inputs = nodes.Where(x => !(new[]{"radio", "checkbox", "reset", "submit"}).Contains(x.GetAttributeValue("type", ""))).ToList();

                    nodes = htmlDoc.DocumentNode.SelectNodes("//textarea");
                    if(nodes != null) inputs.AddRange(nodes.ToList());
                    
                    if(inputs.Where(x => x.Attributes.Where(y => y.Name == "placeholder").Count() < 1).Count() > 0) 
                        errors.Add("Some fields does not have any defined placeholder.");                
                }                   
            }
            catch(Exception e){
               errors.Add(string.Format("EXCEPTION: {0}", e.Message));
            }                     

            return errors;
        }   

        private static List<string> CheckTables(HtmlDocument htmlDoc){
            List<string> errors = new List<string>();

            try{
                HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes("//table");                        
                if(nodes == null || nodes.Count < 1) errors.Add("Does not contains enough tables.");
                else
                {                
                    int rowIdx = 1;
                    HtmlNodeCollection rows = htmlDoc.DocumentNode.SelectNodes("//table/tr");
                    if(rows == null)  errors.Add("The table does not contains any rows.");   
                    else{
                        foreach(HtmlNode row in rows){
                            int count = 0;
                            int labelIdx = 2;
                            
                            HtmlNodeCollection cells = row.SelectNodes("td");
                            if(cells != null){
                                foreach(HtmlNode cell in cells){
                                    int colspan = int.Parse(cell.GetAttributeValue("colspan", "1"));
                                    count += colspan;
                                    labelIdx -= (colspan-1);
                                }

                                if(rowIdx != 3 && rowIdx < rows.Count){
                                    if(row.SelectNodes("td").FirstOrDefault().SelectNodes(".//label") == null) errors.Add(string.Format("The first column at row {0} does not contains a label.", rowIdx));
                                    if(row.SelectNodes("td").Skip(labelIdx).Take(1).FirstOrDefault().SelectNodes(".//label") == null) errors.Add(string.Format("The third column at row {0} does not contains a label.", rowIdx));
                                }
                            }                       

                            if(count != 4) errors.Add(string.Format("Does not contains enough columns at row {0}.", rowIdx));
                            rowIdx++;
                        }
                    }
                }                
            }
            catch(Exception e){
               errors.Add(string.Format("EXCEPTION: {0}", e.Message));
            }
            
            return errors;
        }  

        private static List<string> CheckReset(HtmlDocument htmlDoc){
            List<string> errors = new List<string>();

            try{
                HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes("//input"); //TODO: also button is alowed
                if(nodes == null) 
                    errors.Add("Does not contains any reset button.");        
                else if(nodes.Where(x => x.GetAttributeValue("type", "").Equals("reset")).Count() < 1)
                    errors.Add("Does not contains any reset button.");                   
            }
            catch(Exception e){
               errors.Add(string.Format("EXCEPTION: {0}", e.Message));
            }

            return errors;
        }   

        private static List<string> CheckSubmit(HtmlDocument htmlDoc){
            List<string> errors = new List<string>();

            try{
                //Checking if all the form items has setted up the "name" attribute and also for the submit input/button
                HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes("//input");
                if(nodes != null){
                    //Looking for the input names                
                    foreach(HtmlNode input in nodes.Where(x => !(new[]{"reset", "submit"}).Contains(x.GetAttributeValue("type", "")))){
                        if(string.IsNullOrEmpty(input.GetAttributeValue("name", "")))
                            errors.Add("An input form field has been found with no name.");
                    }  

                    //Looking for the submit input or button
                    if((nodes.Where(x => x.GetAttributeValue("type", "").Equals("submit")).Count() < 1)){
                        nodes = htmlDoc.DocumentNode.SelectNodes("//button");
                        if(nodes != null && nodes.Where(x => x.GetAttributeValue("type", "").Equals("submit")).Count() < 1)
                            errors.Add("Does not contains any submit button.");                                              
                    }                 
                } 
                
                //Checking for the select name
                nodes = htmlDoc.DocumentNode.SelectNodes("//select");
                if(nodes != null){
                    foreach(HtmlNode select in nodes){
                        if(string.IsNullOrEmpty(select.GetAttributeValue("name", "")))
                            errors.Add("A select form field has been found with no name.");
                    }  
                }

                //Checking for the textarea name
                nodes = htmlDoc.DocumentNode.SelectNodes("//textarea");
                if(nodes != null){
                    foreach(HtmlNode textarea in nodes){
                        if(string.IsNullOrEmpty(textarea.GetAttributeValue("id", "")) || string.IsNullOrEmpty(textarea.GetAttributeValue("name", "")))
                            errors.Add("A textarea form field has been found with no name and/or id.");
                    }  
                }                          

                //Looking for the form "action"
                nodes = htmlDoc.DocumentNode.SelectNodes("//form");
                if(nodes == null)  errors.Add("Does not contains any form.");        
                else{
                    if(nodes.Where(x => x.GetAttributeValue("action", "").Equals("formResult.html")).Count() < 1){
                        nodes = htmlDoc.DocumentNode.SelectNodes("//button");
                        if(nodes != null && nodes.Where(x => x.GetAttributeValue("formaction", "").Equals("formResult.html")).Count() < 1)
                            errors.Add("The form does not submit the data to the correct destination.");                                              
                    }
                }
                
            }
            catch(Exception e){
               errors.Add(string.Format("EXCEPTION: {0}", e.Message));
            }             

            return errors;
        } 
    }
}