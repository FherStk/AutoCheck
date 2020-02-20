
using System;
using System.Linq;
using HtmlAgilityPack;
using System.Collections.Generic;
using AutoCheck.Core;

namespace AutoCheck.Checkers{     
    /// <summary>
    /// Allows data validations over a WEB set of files.
    /// </summary>  
    public class Csv : Checker{  
        /// <summary>
        /// The main connector, can be used to perform direct operations over the data source.
        /// </summary>
        /// <value></value>    
        public Connectors.Csv Connector {get; private set;}
        /// <summary>
        /// Comparator operator
        /// </summary>
        public enum Operator{
            //TODO: must be reusable by other checkers
            MIN,
            MAX,
            EQUALS
        }  
        /// <summary>
        /// Creates a new checker instance.
        /// </summary>
        /// <param name="studentFolder">The folder containing the web files.</param>
        /// <param name="file">CSV file name.</param>     
        public Csv(string studentFolder, string file){
            this.Connector = new Connectors.Csv(studentFolder, file);            
        }                 
        /// <summary>
        /// Checks if the total rows amount is lower, higher or equals than the expected.
        /// </summary>
        /// <param name="expected">Expected applied amount.</param>
        /// <param name="op">Comparison operator to be used.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfRegistriesMatchesAmount(int expected, Operator op = Operator.EQUALS){  
            List<string> errors = new List<string>();

            try{
                if(!Output.Instance.Disabled) Output.Instance.Write("Checking the amount of registries... ");
                errors.AddRange(CompareItems("Amount of registers missmatch:", expected, this.Connector.CsvDoc.Count, op));
            }
            catch(Exception e){
                errors.Add(e.Message);
            }            

            return errors;
        } 
        /// <summary>
        /// Compares if the given company data matches with the current one stored in the database.
        /// </summary>
        /// <param name="line">The registry line to check (from 1 to N).</param>
        /// <param name="expected">The expected data to match.</param>
        /// <param name="strict">The expected data must match exactly (otherwise approximations are allowed, usefull for checking students names and emails).</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfRegistriesMatchesData(int line, Dictionary<string, object> expected, bool strict = false){    
            List<string> errors = new List<string>();                        

            if(!Output.Instance.Disabled)  Output.Instance.Write(string.Format("Getting the registry data for ~line={0}... ", line), ConsoleColor.Yellow);  
            if(line > this.Connector.CsvDoc.Count) errors.Add(string.Format("Unable to find the ~line={0}... ", line)); 
            else{
                Dictionary<string, string> registry = this.Connector.CsvDoc.GetLine(line);
                foreach(string k in expected.Keys){
                    bool match = true;
                    if(strict && !registry[k].Equals(expected[k]))  match = false;
                    else if(!strict){
                        int count = 0;
                        string[] value = (registry[k].Contains('@') ? registry[k].Split('@') : registry[k].Split(' '));
                        string exp = Core.Utils.RemoveDiacritics(expected[k].ToString().ToLower());

                        foreach(string v in value){
                            string curr = Core.Utils.RemoveDiacritics(v.ToLower());
                            if(exp.Contains(curr) || curr.Contains(exp)) count++;
                        }

                        //50% match for email (just the domain); 75% for the other text fields.
                        match = (float)count / (float)value.Length >= (registry[k].Contains('@') ? 0.5f : 0.75f);
                    }

                    if(!match) errors.Add(string.Format("Incorrect data found for {0}: expected->'{1}' found->'{2}'.", k, expected[k], registry[k]));
                }
            }

            return errors;
        }          
        private List<string> CompareItems(string caption, int expected, int current, Operator op){
            //TODO: must be reusable by other checkers
            List<string> errors = new List<string>();     
            string info = string.Format("expected->'{0}' found->'{1}'.", expected, current);

            switch(op){
                case Operator.EQUALS:
                    if(current != expected) errors.Add(string.Format("{0} {1}.", caption, info));
                    break;

                case Operator.MAX:
                    if(current > expected) errors.Add(string.Format("{0} maximum {1}.", caption, info));
                    break;

                case Operator.MIN:
                    if(current < expected) errors.Add(string.Format("{0} minimum {1}.", caption, info));
                    break;
            }

            return errors;
        }
    }    
}