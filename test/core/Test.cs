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
using System.IO;

namespace AutoCheck.Test.Core
{
    public class Test
    {
        protected string SamplesFolder {get; set;}
        protected string SamplesPath {get; set;}

        protected virtual void Setup(string folder) 
        {
            this.SamplesFolder = Path.Combine(AutoCheck.Core.Utils.AppFolder(), "samples"); 
            this.SamplesPath = GetSamplePath(folder); 
        }           

        protected string GetSamplePath(string folder) 
        {
            return Path.Combine(this.SamplesFolder, folder); 
        }
        protected string GetSampleFile(string file) 
        {
            if(string.IsNullOrEmpty(this.SamplesPath)) throw new ArgumentNullException("The global samples path value is empty, use another overload or set up the SamplesPath parameter.");
            return Path.Combine(this.SamplesPath, file); 
        }
        protected string GetSampleFile(string folder, string file) 
        {
            return Path.Combine(GetSamplePath(folder), file); 
        }
    }
}
