# DEPRECATED!

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