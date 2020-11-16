# Student's guide
## How to use the application
### Pre-requisites
This application has been developed over the **.NET 5.0**, so its runtime enviroment must be installed in order to run the app. Also, the application is hosted on **GitHub** repository, so **Git** must be installed in order to download and update the app:
* [.NET 5.0 SDK x64](https://dotnet.microsoft.com/download)
* [Git](https://git-scm.com/downloads)

### Installation
Follow this instructions in order to install the application for the first time:
1. Create a folder where the application will be downloaded in.
2. Open a terminal and go into the newly crated folder.
3. Download the application with the following command: `git clone https://github.com/FherStk/AutoCheck.git`.

### Run (with update)
Follow this instructions in order to run the application:
1. Open a terminal and go into the application's root folder: `cd AutoCheck`.
2. Go into the application's terminal folder: `cd terminal`.
3. Run the application using the `script` argument to choose which yaml file should be used: `dotnet run "path_to_file.yaml"` 

### Examples
Please, notice that **AutoCheck will revert all changes when updates** which includes the removal of new files, so it's important to **copy the scripts** you want to modify into the `AutoCheck\scripts\custom` folder:
1. Copy the script you want to use, for example `AutoCheck\scripts\targets\xml_validation_single.yaml"`.
2. Paste the script into the `AutoCheck\scripts\custom`
3. Edit your file and change whatever you need, for example the `folder` attribute.

#### DAM M04UF1: XML Validation assignment (Namespaces + DTD + XSD)
* For testing a single assignment: `dotnet run ..\scripts\custom\xml_validation_single.yaml`
    * **folder**: Path to the folder containing the assignment's XML files.<br><br>

* For testing a group of assignments: `dotnet run ..\scripts\custom\xml_validation_batch.yaml`
    * **path**: Path to the folder containing a set of assignments, where each assignment will be within a folder.

#### DAM M04UF1: HTML5 assignment
* For testing a single assignment: `dotnet run ..\scripts\custom\html5_single.yaml`
    * **folder**: Path to the folder containing the assignment's HTML5 files.<br><br>

* For testing a group of assignments: `dotnet run ..\scripts\custom\html5_batch.yaml`
    * **path**: Path to the folder containing a set of assignments, where each assignment will be within a folder.

#### DAM M04UF1: CSS3 assignment
* For testing a single assignment: `dotnet run ..\scripts\custom\css3_single.yaml`
    * **folder**: Path to the folder containing the assignment's CSS3 files.<br><br>

* For testing a group of assignments: `dotnet run ..\scripts\custom\css3_batch.yaml`
    * **path**: Path to the folder containing a set of assignments, where each assignment will be within a folder.

### Run (with no update)
AutoCheck will automatically check for updates on startup, but update avoidance can be requested:
1. Open a terminal and go into the application's root folder: `cd AutoCheck`.
2. Go into the application's terminal folder: `cd terminal`.
3. Update the application with the following command: `dotnet run --no-update "path_to_file.yaml"`. 

### Manual update
AutoCheck will automatically check for updates on startup, but a manual update can be requested:
1. Open a terminal and go into the application's root folder: `cd AutoCheck`.
2. Go into the application's terminal folder: `cd terminal`.
3. Update the application with the following command: `dotnet run --update`. 