# Student's guide
## Video tutorial
Despite that you'll find a complete tutorial below this lines, a video tutorial about how to install and setup AutoCheck in order to run its **command line interface** and also its **web application interface** : [https://youtu.be/1ObOx6oYZ3w](https://youtu.be/1ObOx6oYZ3w)

## How to use the application
### Pre-requisites
This application has been developed over the **.NET 6.0**, so its runtime enviroment must be installed in order to run the app; also, the application is hosted on **GitHub** repository, so **Git** must be installed in order to download and update the app. Notice than Java's JDK is also required because some components could relay on its use, like the `SourceCode` Copy Detector or some Java oriented scripts:
* [Git](https://git-scm.com/downloads)
* [.NET 6.0 SDK x64](https://dotnet.microsoft.com/download)
* [Java Developement Kit (JDK)](http://jdk.java.net/17/)

### Installation
Follow this instructions in order to install the application for the first time:
1. Create a folder where the application will be downloaded in.
2. Open a terminal and go into the newly crated folder.
3. Download the application with the following command: `git clone https://github.com/FherStk/AutoCheck.git`.

### Permissions
**For GNU/Linux only.** Running a .NET application with `dotnet run` wont work properly with .NET6 under Ubuntu 20.04 if it has been installed as a snap package, so **shelscript launchers** has been distributed in order to use AutoCheck under any kind of OS and installation type, but some execution permissions are needed:
1. Go to the web app folder with the following command: `cd AutoCheck/web`.
2. Give permissions to the startup file with: `chmod +x run.sh`.
3. Go to the cli app folder with the following command: `cd AutoCheck/cli`.
4. Give permissions to the startup file with: `chmod +x run.sh`.

### Web Application Interface
#### Preparation
Please, notice that **AutoCheck will revert all changes when updates** which includes the removal of new files, so it's important to **store your own custom scripts** into the `AutoCheck/scripts/custom` folder.

#### Run
Follow this instructions in order to run the application:
1. Open a terminal and go into the application's web folder: `cd AutoCheck/web`.
2. For GNU/Linux: run the application with `./run.sh`
3. For Windows: run the application with `run`
4. A message like `Now listening on: http://localhost:5000` will be prompted, `Ctrl + Click` on the displayed URL will be open the app in a web browser.
5. Once the web broswer opens, the remaining instructions will be displayed within the home page. 

### Command Line Interface
#### Preparation
Please, notice that **AutoCheck will revert all changes when updates** which includes the removal of new files, so it's important to **copy the scripts** you want to modify into the `AutoCheck/scripts/custom` folder:
1. Copy the script you want to use, for example `AutoCheck/scripts/targets/single/xml_validation.yaml`.
2. Paste the script into the `AutoCheck/scripts/custom`
3. Edit the new file and change the *folder* attribute so it points to the path containing your assignemnt, like `C:\\users\myuser\mystudies\myassignment` for Windows or `/home/myuser/mystudies/mysassignment` for GNU/Linux. Some scripts could use the *host* attribute to point to a remote database or a remote file system. 

#### Run (with update)
Follow this instructions in order to run the application:
1. Open a terminal and go into the application's cli folder: `cd AutoCheck/cli`.
2. For GNU/Linux: run the application with `./run.sh "path_to_file.yaml"`, for example: `./run.sh ../scripts/custom/xml_validation_single.yaml`  
3. For Windows: run the application with `run "path_to_file.yaml"`, for example: `run ../scripts/custom/xml_validation_single.yaml`  

#### Run (with no update)
AutoCheck will automatically check for updates on startup, but update avoidance can be requested:
1. Open a terminal and go into the application's cli folder: `cd AutoCheck/cli`.
2. For GNU/Linux: run the application avoiding updates with the following command: `./run.sh --no-update "path_to_file.yaml"`. 
3. For Windows: run the application avoiding updates with the following command: `run --no-update "path_to_file.yaml"`. 

#### Manual update
AutoCheck will automatically check for updates on startup, but a manual update can be requested:
1. Open a terminal and go into the application's cli folder: `cd AutoCheck/cli`.
2. For GNU/Linux: update the application with the following command: `./run.sh --update`.
3. For Windows: update the application with the following command: `run --update`.