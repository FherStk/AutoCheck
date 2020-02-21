/*
    Copyright © 2020 Fernando Porrino Serrano
    Third party software licenses can be found at /docs/credits/thirdparties.md

    This file is part of AutoCheck.

    AutoCheck is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    AutoCheck is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with AutoCheck.  If not, see <https://www.gnu.org/licenses/>.
*/

using System.Collections.Generic;

namespace AutoCheck.Core{
    /// <summary>
    /// This class must be inherited in order to develop a custom copy detectors.
    /// This class is in charge of performing the copy detection along the student's files, code the abstract methods and provide all the necessary extra code needed.
    /// </summary>
    public abstract class CopyDetector{
        /// <summary>
        /// The amount of items loaded into the copy detector.
        /// </summary>
        /// <value></value>
        public abstract int Count {get;}
        /// <summary>
        /// Loads the source data into the copy detector.
        /// </summary>
        /// <param name="source">File content, file path, whatever the copy detector needs.</param>
        public abstract void Load(string source);
        /// <summary>
        /// Performs the item comparison between each other.
        /// </summary>
        public abstract void Compare();
        /// <summary>
        /// Checks if a potential copy has been detected.
        /// The Compare() method should be called firts.
        /// </summary>
        /// <param name="source">The source item asked for.</param>
        /// <param name="threshold">The threshold value, a higher one will be considered as copy.</param>
        /// <returns>True of copy has been detected.</returns>
        public abstract bool CopyDetected(string source, float threshold);
        /// <summary>
        /// Returns a printable details list, containing information about the comparissons (student, source and % of match).
        /// </summary>
        /// <param name="path">Student name</param>
        /// <returns>A list of tuples, on each one will contain information about the current student, the source compared with and the % of match. </returns>
        public abstract List<(string student, string source, float match)> GetDetails(string path);
    }
}