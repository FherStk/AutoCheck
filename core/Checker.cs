
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
    /// <summary>
    /// This class must be inherited in order to develop a custom checker.
    /// The checker is in charge of testing items using a connector, and the result will be always a list of errors. 
    /// </summary>       
    public abstract class Checker: IDisposable {      
        /// <summary>
        /// Disposes the object releasing its unmanaged properties.
        /// </summary>
        public abstract void Dispose();

        protected List<string> CompareItems(string caption, int current, Connector.Operator op, int expected){
            //TODO: must be reusable by other checkers
            List<string> errors = new List<string>();
            string info = string.Format("expected->'{0}' found->'{1}'.", expected, current);

            switch(op){
                case AutoCheck.Core.Connector.Operator.EQUALS:
                    if(current != expected) errors.Add(string.Format("{0} {1}.", caption, info));
                    break;

                case AutoCheck.Core.Connector.Operator.GREATER:
                    if(current <= expected) errors.Add(string.Format("{0} maximum {1}.", caption, info));
                    break;

                case AutoCheck.Core.Connector.Operator.GREATEREQUALS:
                    if(current < expected) errors.Add(string.Format("{0} maximum or equals {1}.", caption, info));
                    break;

                case AutoCheck.Core.Connector.Operator.LOWER:
                    if(current >= expected) errors.Add(string.Format("{0} minimum {1}.", caption, info));
                    break;

                case AutoCheck.Core.Connector.Operator.LOWEREQUALS:
                    if(current > expected) errors.Add(string.Format("{0} minimum or equals {1}.", caption, info));
                    break;
                
                default:
                    throw new NotImplementedException();
            }

            return errors;
        }
    }  
}
