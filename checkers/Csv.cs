
/*
    Copyright © 2020 Fernando Porrino Serrano
    Third party software licenses can be found at /docs/credits/thirdparties.md

    This file is part of AutoCheck.

    AutoCheck is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    AutoCheck is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with AutoCheck.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
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
                        string[] value = (registry[k].Contains('@') ? registry[k].Trim().Split('@') : registry[k].Trim().Split(' '));
                        string exp = Core.Utils.RemoveDiacritics(expected[k].ToString().ToLower());

                        foreach(string v in value){
                            string curr = Core.Utils.RemoveDiacritics(v.ToLower());
                            if(exp.Contains(curr) || curr.Contains(exp)) count++;
                        }

                        //Match % needed depends on original length
                        float min = 0;
                        switch(value.Length){
                            case 1:
                                min = 1;
                                break;
                            
                            case 2:
                                min = 0.5f;
                                break;
                            
                            case 3:
                                min = 2f/3f;
                                break;

                            default:
                                min = 0.75f;
                                break;
                        }

                        match = ((float)count / (float)value.Length >= min);
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