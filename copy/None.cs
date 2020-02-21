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

using System.Collections.Generic;

namespace AutoCheck.CopyDetectors{
    /// <summary>
    /// Empty copy detector, use it in order to avoid copy detection.
    /// </summary>
    public class None: Core.CopyDetector{     
        public override int Count {
            get {
                return 0;
            }
        }    
        public override void Load(string path){
        }
        public override void Compare(){
        }
        public override bool CopyDetected(string path, float threshold){                             
            return false;
        }
        public override List<(string student, string source, float match)> GetDetails(string path){
            return null;
        }
    }
}