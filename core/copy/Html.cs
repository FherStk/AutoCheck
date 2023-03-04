/*
    Copyright © 2023 Fernando Porrino Serrano
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
    /// Copy detector for HTML files.
    /// </summary>
    public class Html: PlainText{                
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="threshold">Matches above this value will be computed as potential copies.</param>
        /// <param name="sensibility">The copy detection sensibility, lower values increases the probability of false positives.</param>
        /// <param name="mode">The comparisson mode.</param>
        /// <param name="filePattern">Only the files mathing this pattern will be compared.</param>
        public Html(float threshold, int sensibility, DetectionMode mode, string filePattern = "*.html"): base(threshold, sensibility, mode, filePattern)
        {
            this.SentenceMatchWeight = 0.5f;
            this.WordCountWeight = 0.3f;
            this.LineCountWeight = 0.2f;            
        }     

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="threshold">Matches above this value will be computed as potential copies.</param>
        /// <param name="sensibility">The copy detection sensibility, lower values increases the probability of false positives.</param>
        /// <param name="filePattern">Only the files mathing this pattern will be compared.</param>
        /// <returns></returns>
        public Html(float threshold, int sensibility, string filePattern = "*.html"): this(threshold, sensibility, DetectionMode.DEFAULT, filePattern){           
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="threshold">Matches above this value will be computed as potential copies.</param>
        /// <param name="mode">The comparisson mode.</param>
        /// <param name="filePattern">Only the files mathing this pattern will be compared.</param>
        /// <returns></returns>
        public Html(float threshold, DetectionMode mode, string filePattern = "*.html"): this(threshold, -1, mode, filePattern){           
        }

        /// <summary>
        /// Creates a new instance, setting up its properties in order to allow copy detection with the lowest possible false-positive probability.
        /// </summary>
        /// <param name="threshold">Matches above this value will be computed as potential copies.</param>
        /// <param name="filePattern">Only the files mathing this pattern will be compared.</param>
        /// <returns></returns>
        public Html(float threshold, string filePattern = "*.html"): this(threshold, -1, DetectionMode.DEFAULT, filePattern){           
        }                                 
    }
}