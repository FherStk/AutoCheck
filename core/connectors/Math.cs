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

using NCalc2;

namespace AutoCheck.Core.Connectors{       
    /// <summary>
    /// Allows in/out operations and/or data validations with XML files.
    /// </summary>
    public class Math: Base{     
        /// <summary>
        /// Disposes the object releasing its unmanaged properties.
        /// </summary>
        public override void Dispose(){
        }   

        /// <summary>
        /// Evaluates a mathematical expression.
        /// </summary>
        /// <param name="expression">A mathematical expression.</param>
        /// <example>Round(Pow(2, 8) + Sqrt(2) * 27.4, 2)</example>
        /// <remarks>Uses NCalc2 internally (https://github.com/sklose/NCalc2)</remarks>
        /// <returns></returns>
        public object Evaluate(string expression){
            var e = new Expression(expression);
            return e.Evaluate();            
        }
    }
}