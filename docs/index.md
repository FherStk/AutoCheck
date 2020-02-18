[//]: # (WARNING: DO NOT EDIT README.md file because it's a copy of docs/index.md auto-generated during on build.)
# AutomatedAssignmentValidator
Multi purpose assignment validation for academic purposes only.
Is has been created in order to automatically check the correctness of a set of students assignments, but manual supervision is still needed.

Feel free to use, copy, fork or modify this project; but please refer a mention to this project and its author respecting also the licenses of the included third party software.

### Third party software and licenses:
Please notice than this project could not be possible without the help of:
* The [HtmlAgilityPack](https://html-agility-pack.net/) library by [zzzprojects](https://github.com/zzzprojects/html-agility-pack): under the MIT license, (further details about the license can be found at [https://github.com/khalidabuhakmeh/ConsoleTables/blob/master/LICENSE](https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE)).    
* The [ExCSS](https://github.com/TylerBrinks/ExCSS) library by [Tyler Brinks](https://github.com/TylerBrinks): under the MIT license, (further details about the license can be found at [https://github.com/TylerBrinks/ExCSS/blob/master/license.txt](https://github.com/TylerBrinks/ExCSS/blob/master/license.txt)).
* The [Npgsql](https://www.npgsql.org/) library by [The Npgsql Development Team](https://www.npgsql.org/index.html): under the PostgreSQL License, (further details about the license can be found at [https://github.com/npgsql/npgsql/blob/master/LICENSE.txt](https://github.com/npgsql/npgsql/blob/master/LICENSE.txt)).
* The [ToolBox](https://github.com/deinsoftware/toolbox) library by [Camilo Martinez](https://dev.to/equiman): under the MIT License, (further details about the license can be found at [https://github.com/deinsoftware/toolbox/blob/master/LICENSE](https://github.com/deinsoftware/toolbox/blob/master/LICENSE)).
* The [SharpZipLib](https://github.com/icsharpcode/SharpZipLib) library by [Mike Krüger](https://github.com/icsharpcode): under the MIT License, (further details about the license can be found at [https://github.com/icsharpcode/SharpZipLib/blob/master/LICENSE.txt](https://github.com/icsharpcode/SharpZipLib/blob/master/LICENSE.txt)).

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
    * `permissions`: checks the "PostgreSQL Permissions Management" practical assignment for ASIX M02UF3.
    * `views`: checks the "PostgreSQL Updatable Views" practical assignment for ASIX M02UF3.
    * `viewsExtended`: checks the "PostgreSQL Updatable Views (extended version)" practical assignment for ASIX M02UF3.
    * `sqlLog`: checks a collection of PostgreSQL log files (like the ones attached to the "PostgreSQL Updatable Views" practical assignment) in order to detect copies between students. Please, note this is not a regular assignment so no score will be output, just the % of matching between files.

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
* For testing a single assignment using a folder: `dotnet run --assig=odoo --server=POSTGRESQL_SERVER_ADDRESS --folder='/home/user/assignment'`
* For testing a single assignment using an existing database: `dotnet run --assig=odoo --server=POSTGRESQL_SERVER_ADDRESS --database='odoo_NAME_SURNAME'`
* For testing a group of assignments: `dotnet run --assig=odoo --server=POSTGRESQL_SERVER_ADDRESS --path='/home/user/assignment'` (nothe that path must contain a set of folders following the assignment naming convention: `x_NAME_SURNAME` where `x` can be whatever).

#### assig=permissions
Examples with the allowed parameter combinations:
* For testing a single assignment using a folder: `dotnet run --assig=permissions --server=POSTGRESQL_SERVER_ADDRESS --folder='/home/user/assignment'`
* For testing a single assignment using an existing database: `dotnet run --assig=permissions --server=POSTGRESQL_SERVER_ADDRESS --database='empresa_NAME_SURNAME'`
* For testing a group of assignments: `dotnet run --assig=permissions --server=POSTGRESQL_SERVER_ADDRESS --path='/home/user/assignment'` (nothe that path must contain a set of folders following the assignment naming convention: `x_NAME_SURNAME` where `x` can be whatever).

#### assig=views
Examples with the allowed parameter combinations:
* For testing a single assignment using a folder: `dotnet run --assig=views --server=POSTGRESQL_SERVER_ADDRESS --folder='/home/user/assignment'`
* For testing a single assignment using an existing database: `dotnet run --assig=views --server=POSTGRESQL_SERVER_ADDRESS --database='empresa_NAME_SURNAME'`
* For testing a group of assignments: `dotnet run --assig=views --server=POSTGRESQL_SERVER_ADDRESS --path='/home/user/assignment'` (nothe that path must contain a set of folders following the assignment naming convention: `x_NAME_SURNAME` where `x` can be whatever).

#### assig=viewsExtended
Examples with the allowed parameter combinations:
* For testing a single assignment using a folder: `dotnet run --assig=viewsExtended --server=POSTGRESQL_SERVER_ADDRESS --folder='/home/user/assignment'`
* For testing a single assignment using an existing database: `dotnet run --assig=viewsExtended --server=POSTGRESQL_SERVER_ADDRESS --database='empresa_NAME_SURNAME'`
* For testing a group of assignments: `dotnet run --assig=viewsExtended --server=POSTGRESQL_SERVER_ADDRESS --path='/home/user/assignment'` (nothe that path must contain a set of folders following the assignment naming convention: `x_NAME_SURNAME` where `x` can be whatever).

#### assig=sqlLog
Examples with the allowed parameter combinations:
* For testing a group of assignments: `dotnet run --assig=sqlLog --path='/home/user/assignment'` (nothe that path must contain a set of folders following the assignment naming convention: `x_NAME_SURNAME` where `x` can be whatever).