/*
    Copyright © 2021 Fernando Porrino Serrano
    Third party software licenses can be found at /docs/credits/credits.md

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
    /// Copy detector for SQL log files.
    /// </summary>
    public class SqlLog: PlainText{                
             
        /// <summary>
        /// Creates a new instance, setting up its properties in order to allow copy detection with the lowest possible false-positive probability.
        /// </summary>     
        public SqlLog(float threshold, string filePattern = "*.log"): base(threshold, filePattern)
        {
            this.SentenceMatchWeight = 0.85f;
            this.WordCountWeight = 0.1f;
            this.LineCountWeight = 0.05f; 
        }                                       
    }
}