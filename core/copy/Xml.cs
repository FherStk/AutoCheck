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

namespace AutoCheck.Core.CopyDetectors{
    /// <summary>
    /// Copy detector for XML files.
    /// </summary>
    public class Xml: PlainText{                
        /// <summary>
        /// Creates a new instance, setting up its properties in order to allow copy detection with the lowest possible false-positive probability.
        /// </summary>     
        public Xml(float threshold, string filePattern = "*.xml"): base(threshold, filePattern)
        {    
            //TODO: more items are needed:
            //  Amount of nodes
            //  Amount of attributes (per node)
            //  Amount of children (hierarchy per node)            
            //  Node or attribute naming are not so important
            
            this.WordsAmountWeight = 0.3f;
            this.WordCountWeight = 0.5f;
            this.LineCountWeight = 0.2f;    
        }                                                
    }
}