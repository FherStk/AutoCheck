# AutomatedAssignmentValidator
Multi purpose assignment validation for academic purposes only.
Is has been created in order to automatically check the correctness of a set of students assignments, but manual supervision is still needed.

Feel free to use, copy, fork or modify this project; but please refer a mention to this project and its author respecting also the licenses of the included third party software.

### Third party software and licenses:
Please notice than this project could not be possible without the help of:
* The [HtmlAgilityPack](https://html-agility-pack.net/) library property of [zzzprojects](https://github.com/zzzprojects/html-agility-pack): under the MIT license, (further details about the license can be found at [https://github.com/khalidabuhakmeh/ConsoleTables/blob/master/LICENSE](https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE)).    
* The [ExCSS](https://github.com/TylerBrinks/ExCSS) library property of [Tyler Brinks](https://github.com/TylerBrinks): under the MIT license, (further details about the license can be found at [https://github.com/TylerBrinks/ExCSS/blob/master/license.txt](https://github.com/TylerBrinks/ExCSS/blob/master/license.txt)).
* The [Npgsql](https://www.npgsql.org/) library property of [The Npgsql Development Team](https://www.npgsql.org/index.html): under the PostgreSQL License, (further details about the license can be found at [https://github.com/npgsql/npgsql/blob/master/LICENSE.txt](https://github.com/npgsql/npgsql/blob/master/LICENSE.txt)).
* The [ToolBox](https://github.com/deinsoftware/toolbox) library property of [Camilo Martinez](https://dev.to/equiman): under the MIT License, (further details about the license can be found at [https://github.com/deinsoftware/toolbox/blob/master/LICENSE](https://github.com/deinsoftware/toolbox/blob/master/LICENSE)).

## WARNING: still in an early development stage.
### How to use it:
#### As an stand-alone console app:
Clone the repository to your local working directory, restore the dependencies with `dotnet restore`, build it with `dotnet build` and, finally, run the project with `dotnet run`. The following parameters will be needed:
* `path`: a path to the folder containing all the student's assignements, inside it must be a batch of folders following the Moodle's batch download naming convention like `NAME SURNAMES_ID_assignsubmission_file_`.
* `folder`: if only one assignment would be checked, use this parameter instead `path` with the folder's path. 
* `server`: only for those assignments that must be checked over a server instead of a folder. 
* `assig`: the assignment to check, where can be:
    * `html5`: checks the "HTML5" practical assignment for ASIX-DAM M04.
    * `css3`: checks the "CSS3" practical assignment for ASIX-DAM M04.
    * `odoo`: checks the "Odoo Backoffice Management" practical assignment for DAM M10.
    * `permissions`: checks the "PostgreSQL Permissions Management" practical assignment for ASIX M10.

#### assig=html5
Examples with the allowed parameter combinations:
* For testing a single assignment: `dotnet run --assig=html5 --folder='/home/user/assignment'`
* For testing a group of assignments: `dotnet run --assig=html5 --path='/home/user/assignment'` (nothe that path must contain a set of folders following the Moodle's naming convention: `STUDENTNAME_ID_assignsubmission_file_`).

#### assig=css3
Examples with the allowed parameter combinations:
* For testing a single assignment: `dotnet run --assig=css3 --folder='/home/user/assignment'`
* For testing a group of assignments: `dotnet run --assig=css3 --path='/home/user/assignment'` (nothe that path must contain a set of folders following the Moodle's naming convention: `STUDENTNAME_ID_assignsubmission_file_`).

#### assig=odoo
Examples with the allowed parameter combinations:
* For testing a single assignment: `dotnet run --assig=odoo --folder='/home/user/assignment'`
* For testing a group of assignments: `dotnet run --assig=css3 --path='/home/user/assignment'` (nothe that path must contain a set of folders following the Moodle's naming convention: `STUDENTNAME_ID_assignsubmission_file_`).