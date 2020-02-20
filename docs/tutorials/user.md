# User's guide: how to use the application
## Pre-requisites
This application has been developed over the **.NET Core Framework**, so its runtime enviroment must be installed in order to run the app. Also, the application is hosted on **GitHub** repository, so **Git** must be installed in order to download and update the app:
* [.NET Core 3.1 Runtime](https://dotnet.microsoft.com/download/dotnet-core/current/runtime)
* [Git](https://git-scm.com/downloads)

## Installation
Follow this instructions in order to install the application for the first time:

1. Create a folder where the application will be downloaded in.
2. Open a terminal and go into the newly crated folder.
3. Download the application with the following command: `git clone https://github.com/FherStk/AutoCheck.git`.
4. Go into the downloaded application folder.
5. Build the application before using it with the following command: `dotnet build`.

## Update
Follow this instructions in order to update the application, it's recomended to update before using it:
1. Open a terminal and go into the application folder.
2. Update the application with the following command: `git pull`. 
3. Build the application before using it with the following command: `dotnet build`.

## Run
Follow this instructions in order to update the application, it's recomended to update before using it:
1. Open a terminal and go into the application folder.
2. Run the application with the following commands, depending on the script you want to run.

### DAM M10UF1: Odoo usage assignment
* For testing a single assignment: `dotnet run --script=DAM_M10UF1_OdooUsageAssignment --target=single --host=HOST --database=DBNAME`
    * **host**: Odoo server host name or IP address (no port needed).
    * **database**: Odoo database name.<br><br>

* For testing a group of assignments: `dotnet run --script=DAM_M10UF1_OdooUsageAssignment --target=batch --host=HOST --path=FOLDER`
    * **host**: Odoo server host name or IP address (no port needed).
    * **path**: Path to the folder containing a Moodle's unziped batch download (a set of folders following the Moodle's naming convention: 
    `STUDENTNAME_ID_assignsubmission_file_`). Each folder must contain an Odoo backup file.

### DAM M10UF1: Odoo CSV assignment
* For testing a single assignment: `dotnet run --script=DAM_M10UF2_OdooCsvAssignment --target=single --host=HOST --database=DBNAME --path=FOLDER`
    * **host**: Odoo server host name or IP address (no port needed).
    * **database**: Odoo database name.
    * **path**: Path to the student's folder. which must contain an Odoo backup file and also a CSV file.<br><br>

* For testing a group of assignments: `dotnet run --script=DAM_M10UF1_OdooUsageAssignment --target=batch --host=HOST --path=FOLDER`
    * **host**: Odoo server host name or IP address (no port needed).
    * **path**: Path to the folder containing a Moodle's unziped batch download (a set of folders following the Moodle's naming convention: 
    `STUDENTNAME_ID_assignsubmission_file_`). Each folder must contain an Odoo backup file and also a CSV file.

### ASIX M02UF3: Permissions assignment
* For testing a single assignment: `dotnet run --script=ASIX_M02UF3_PermissionsAssignment --target=single --host=HOST --database=DBNAME`
    * **host**: PostgreSQL server host name or IP address (no port needed).
    * **database**: PostgreSQL database name.<br><br>

* For testing a group of assignments: `dotnet run --script=ASIX_M02UF3_PermissionsAssignment --target=batch --host=HOST --path=FOLDER`
    * **host**: PostgreSQL server host name or IP address (no port needed).
    * **path**: Path to the folder containing a Moodle's unziped batch download (a set of folders following the Moodle's naming convention: 
    `STUDENTNAME_ID_assignsubmission_file_`). Each folder must contain a PostgreSQL backup file and also a PostgreSQL log file.

### ASIX M02UF3: Views assignment
* For testing a single assignment: `dotnet run --script=ASIX_M02UF3_ViewsAssignment --target=single --host=HOST --database=DBNAME`
    * **host**: PostgreSQL server host name or IP address (no port needed).
    * **database**: PostgreSQL database name.<br><br>

* For testing a group of assignments: `dotnet run --script=ASIX_M02UF3_ViewsAssignment --target=batch --host=HOST --path=FOLDER`
    * **host**: PostgreSQL server host name or IP address (no port needed).
    * **path**: Path to the folder containing a Moodle's unziped batch download (a set of folders following the Moodle's naming convention: 
    `STUDENTNAME_ID_assignsubmission_file_`). Each folder must contain a PostgreSQL backup file and also a PostgreSQL log file.

### ASIX M02UF3: Views extended assignment
* For testing a single assignment: `dotnet run --script=ASIX_M02UF3_ViewsExtendedAssignment --target=single --host=HOST --database=DBNAME`
    * **host**: PostgreSQL server host name or IP address (no port needed).
    * **database**: PostgreSQL database name.<br><br>

* For testing a group of assignments: `dotnet run --script=ASIX_M02UF3_ViewsExtendedAssignment --target=batch --host=HOST --path=FOLDER`
    * **host**: PostgreSQL server host name or IP address (no port needed).
    * **path**: Path to the folder containing a Moodle's unziped batch download (a set of folders following the Moodle's naming convention: 
    `STUDENTNAME_ID_assignsubmission_file_`). Each folder must contain a PostgreSQL backup file and also a PostgreSQL log file.

### DAM M04UF1: HTML5 assignment
* For testing a single assignment: `dotnet run --script=DAM_M04UF1_Html5Assignment --target=single --path=FOLDER`
    * **path**: Path to the folder containing the assignement's HTML5 files. <br><br>

* For testing a group of assignments: `dotnet run --script=DAM_M04UF1_Html5Assignment --target=batch --path=FOLDER`
    * **path**: Path to the folder containing a Moodle's unziped batch download (a set of folders following the Moodle's naming convention: 
    `STUDENTNAME_ID_assignsubmission_file_`). Each folder must contain a single student's assignement HTML5 files.

### DAM M04UF1: CSS3 assignment
* For testing a single assignment: `dotnet run --script=DAM_M04UF1_Css3Assignment --target=single --path=FOLDER`
    * **path**: Path to the folder containing the assignement's CSS3 files. <br><br>

* For testing a group of assignments: `dotnet run --script=DAM_M04UF1_Css3Assignment --target=batch --path=FOLDER`
    * **path**: Path to the folder containing a Moodle's unziped batch download (a set of folders following the Moodle's naming convention: 
    `STUDENTNAME_ID_assignsubmission_file_`). Each folder must contain a single student's assignement CSS3 files.