
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

namespace AutoCheck.Core{
    //TODO: Remove this file when migration from C# to YAML has been completed (this includes checker & connector mergind and also connector simplification).

    // /// <summary>
    // /// This class must be inherited in order to develop a custom checker.
    // /// The checker is in charge of testing items using a connector, and the result will be always a list of errors. 
    // /// </summary>       
    // public abstract class Checker<T>: IDisposable where T: Core.Connector {      
    //     /// <summary>
    //     /// Disposes the object releasing its unmanaged properties.
    //     /// </summary>
    //     public abstract void Dispose();

    //     public abstract T Connector {get; protected set;}        

    //     protected List<string> CompareItems(string caption, int current, Operator op, int expected){
    //         //TODO: must be reusable by other checkers
    //         var errors = new List<string>();
    //         string info = string.Format("expected->'{0}' found->'{1}'.", expected, current);

    //         switch(op){
    //             case AutoCheck.Core.Operator.EQUALS:
    //                 if(current != expected) errors.Add(string.Format("{0} {1}.", caption, info));
    //                 break;

    //             case AutoCheck.Core.Operator.GREATER:
    //                 if(current <= expected) errors.Add(string.Format("{0} maximum {1}.", caption, info));
    //                 break;

    //             case AutoCheck.Core.Operator.GREATEREQUALS:
    //                 if(current < expected) errors.Add(string.Format("{0} maximum or equals {1}.", caption, info));
    //                 break;

    //             case AutoCheck.Core.Operator.LOWER:
    //                 if(current >= expected) errors.Add(string.Format("{0} minimum {1}.", caption, info));
    //                 break;

    //             case AutoCheck.Core.Operator.LOWEREQUALS:
    //                 if(current > expected) errors.Add(string.Format("{0} minimum or equals {1}.", caption, info));
    //                 break;
                
    //             default:
    //                 throw new NotImplementedException();
    //         }

    //         return errors;
    //     }

    //     protected List<string> CompareItems(string caption, int[] current, Operator op, int[] expected){                        
    //         var errors = new List<string>();            
    //         if(expected.Length != current.Length) errors.Add(string.Format("Unable to compare the given items because the array length mismatches: expected->'{0}' current->'{1}'", expected.Length, current.Length));
    //         else
    //             for(int i = 0; i < expected.Length; i++)
    //                 errors.AddRange(CompareItems(caption, current[i], op, expected[i]));                    
            
    //         return errors;
    //     }
    // }  
}
