
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
using System.Linq;
using System.Collections.Generic;
using HtmlAgilityPack;
using AutoCheck.Core;
using AutoCheck.Exceptions;
using Operator = AutoCheck.Core.Connector.Operator;

namespace AutoCheck.Checkers{     
    /// <summary>
    /// Allows data validations over a WEB set of files.
    /// </summary>  
    public class Css : Checker{  
        /// <summary>
        /// The main connector, can be used to perform direct operations over the data source.
        /// </summary>
        /// <value></value>    
        public Connectors.Css Connector {get; private set;}        
        
        /// <summary>
        /// Creates a new checker instance.
        /// </summary>
        /// <param name="studentFolder">The folder containing the web files.</param>
        /// <param name="file">CSS file name.</param>     
        public Css(string studentFolder, string file){
            this.Connector = new Connectors.Css(studentFolder, file);            
        }  
        
        /// <summary>
        /// Disposes the object releasing its unmanaged properties.
        /// </summary>
        public override void Dispose(){
            this.Connector.Dispose();
        }               
        
        /// <summary>
        /// Given a CSS property, checks if its has been applied within the HTML document.
        /// </summary>
        /// <param name="htmlDoc">The HTML document that must be using the property.</param>
        /// <param name="property">The CSS property name.</param>
        /// <param name="value">The CSS property value.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfPropertyApplied(HtmlDocument htmlDoc, string property, string value = null){  
            List<string> errors = new List<string>();

            try{
                string info = string.IsNullOrEmpty(value) ? property : string.Format("{0}={1}", property, value);
                if(!Output.Instance.Disabled){
                    if(string.IsNullOrEmpty(value)) Output.Instance.Write(string.Format("Checking the '{0}' CSS property... ", property));
                    else Output.Instance.Write(string.Format("Checking the '{0}:{1}' CSS property... ", property, value));
                }

                if(!this.Connector.PropertyExists(property, value)) throw new StyleNotFoundException(string.Format("The given CSS property '{0}' has not been found within the current CSS document.", info));                                
                if(!this.Connector.PropertyApplied(htmlDoc, property, value)) throw new StyleNotAppliedException(string.Format("The given CSS property '{0}' has been found within the current CSS document but it's not beeing applied on the given HTML document.", property)); 
            }
            catch(Exception e){
                errors.Add(e.Message);
            }            

            return errors;
        }              

        /// <summary>
        ///  Given a set of CSS properties, checks how many of them has been applied within the HTML document and if that matches with the given threshold.
        /// </summary>
        /// <param name="htmlDoc">The HTML document that must be using the properties.</param>
        /// <param name="properties">A set of CSS property names.</param>
        /// <param name="expected">Expected minimum amount of applied properties (0 for MAX).</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfPropertyApplied(HtmlDocument htmlDoc, string[] properties, int expected = 0){  
            return CheckIfPropertyApplied(htmlDoc, properties.ToDictionary(x => x, y => string.Empty), expected);
        }

        /// <summary>
        ///  Given a set of CSS properties, checks how many of them has been applied within the HTML document and if that matches with the given threshold.
        /// </summary>
        /// <param name="htmlDoc">The HTML document that must be using the properties.</param>
        /// <param name="properties">A set of CSS property names (key) and values (value).</param>
        /// <param name="expected">Expected minimum amount of applied properties (0 for MAX).</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfPropertyApplied(HtmlDocument htmlDoc, Dictionary<string, string> properties, int expected = 0){  
            List<string> errors = new List<string>();
            if(expected == 0) expected = properties.Values.Count; 

             try{
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Checking the '({0})' CSS properties... ", string.Join(" | ", properties)));
                
                Output.Instance.Disable();
                int applied = 0;
                foreach(string k in properties.Keys){
                    //this.Connector.CheckIfCssPropertyApplied can be also called, but might be better to use CheckIfCssPropertyApplied in order to unify behaviours
                    if(CheckIfPropertyApplied(htmlDoc, k, properties[k]).Count == 0) applied++;                   
                }

                Output.Instance.UndoStatus();
                errors.AddRange(CompareItems("Applied CSS properties missmatch:", applied, Operator.GREATEREQUALS, expected));                
            }
            catch(Exception e){
                errors.Add(e.Message);
            }            

            return errors;
        } 
    }    
}