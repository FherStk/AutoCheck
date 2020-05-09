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

namespace AutoCheck.Scripts{
    public class DAM_M04UF3_ErpPresentationAssignment: Core.ScriptGDrive<CopyDetectors.None>{                       
        public DAM_M04UF3_ErpPresentationAssignment(string[] args): base(args){        
        }                

        public override void Run(){
            base.Run();            
            
            //Just upload the student's videos to the teacher's Google Drive account to keep them safe.
        }
    }
}