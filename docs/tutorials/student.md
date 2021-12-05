# Student's guide
## Video tutorial
Despite that you'll find a complete tutorial below this lines, a video tutorial about how to install and setup AutoCheck in order to run it is also available : [https://youtu.be/PHIZlj9Ly3Q](https://youtu.be/PHIZlj9Ly3Q)

**WARNING**: Running the application as seen in the video wont work with .NET6 under Ubuntu 20.04 if it has been installed as a snap package, the following tutorial describes a method that works for any kind of OS and installation type.

## How to use the application
### Pre-requisites
This application has been developed over the **.NET 6.0**, so its runtime enviroment must be installed in order to run the app. Also, the application is hosted on **GitHub** repository, so **Git** must be installed in order to download and update the app:
* [.NET 6.0 SDK x64](https://dotnet.microsoft.com/download)
* [Git](https://git-scm.com/downloads)

### Installation
Follow this instructions in order to install the application for the first time:
1. Create a folder where the application will be downloaded in.
2. Open a terminal and go into the newly crated folder.
3. Download the application with the following command: `git clone https://github.com/FherStk/AutoCheck.git`.
4. Go to the client app folder with the following command: `cd AutoCheck/terminal`.
5. GNU/Linux only: give permissions to the startup file with: `chmod +x run.sh`.

### Preparation
Please, notice that **AutoCheck will revert all changes when updates** which includes the removal of new files, so it's important to **copy the scripts** you want to modify into the `AutoCheck/scripts/custom` folder:
1. Copy the script you want to use, for example `AutoCheck/scripts/targets/xml_validation_single.yaml`.
2. Paste the script into the `AutoCheck/scripts/custom`
3. Edit the new file and change the *folder* attribute so it points to the path containing your assignemnt, like `C:\\users\myuser\mystudies\myassignment` for Windows or `/home/myuser/mystudies/mysassignment` for GNU/Linux.

### Run (with update)
Follow this instructions in order to run the application:
1. Open a terminal and go into the application's terminal folder: `cd AutoCheck/terminal`.
2. Go into the application's binary (executable) folder: `cd bin/Release/net6.0`.
3. For GNU/Linux: run the application with `./run.sh "path_to_file.yaml"`, for example: `./run.sh ../scripts/custom/xml_validation_single.yaml`  
4. For Windows: run the application with `run "path_to_file.yaml"`, for example: `run ../scripts/custom/xml_validation_single.yaml`  

### Run (with no update)
AutoCheck will automatically check for updates on startup, but update avoidance can be requested:
1. Open a terminal and go into the application's terminal folder: `cd AutoCheck/terminal`.
2. Go into the application's binary (executable) folder: `cd bin/Release/net6.0`.
3. For GNU/Linux: run the application avoiding updates with the following command: `./run.sh --no-update "path_to_file.yaml"`. 
4. For Windows: run the application avoiding updates with the following command: `run --no-update "path_to_file.yaml"`. 

### Manual update
AutoCheck will automatically check for updates on startup, but a manual update can be requested:
1. Open a terminal and go into the application's terminal folder: `cd AutoCheck/terminal`.
2. Go into the application's binary (executable) folder: `cd bin/Release/net6.0`.
3. For GNU/Linux: update the application with the following command: `./run.sh --update`.
4. For Windows: update the application with the following command: `run --update`.

### Avaliable scripts
Here you will find different excamples about how to invoke existing scripts, but remember: you need to setup the target first, just need to copy the original one to the `scripts/custom` folder and change the target parameter (`host`, `path` or `folder`).

#### DAM - M04 UF1: XML Validation assignment (Namespaces + DTD + XSD)
In order to check this assignment:
1. Copy the `AutoCheck/scripts/targets/xml_validation_single.yaml` script into the `scripts/custom` folder.
2. Edit the `AutoCheck/scripts/custom/xml_validation_single.yaml` script and change the `folder` parameter to point the folder where your assignment is. 
3. Open a terminal and go to the `terminal` binary (executable) folder with `cd AutoCheck/terminal/bin/Release/net6.0`.
4. GNU/Linux: run the script with `./run.sh ../scripts/custom/xml_validation_single.yaml`.
5. Windows: run the script with `run ../scripts/custom/xml_validation_single.yaml`.

#### DAM - M04 UF1: HTML5 assignment
In order to check this assignment:
1. Copy the `AutoCheck/scripts/targets/html5_single.yaml` script into the `scripts/custom` folder.
2. Edit the `AutoCheck/scripts/custom/html5_single.yaml` script and change the `folder` parameter to point the folder where your assignment is. 
3. Open a terminal and go to the `terminal` binary (executable) folder with `cd AutoCheck/terminal/bin/Release/net6.0`.
4. GNU/Linux: run the script with `./run.sh ../scripts/custom/html5_single.yaml`.
5. Windows: run the script with `run ../scripts/custom/html5_single.yaml`.

#### DAM - M04 UF1: CSS3 assignment
In order to check this assignment:
1. Copy the `AutoCheck/scripts/targets/css3_single.yaml` script into the `scripts/custom` folder.
2. Edit the `AutoCheck/scripts/custom/css3_single.yaml` script and change the `folder` parameter to point the folder where your assignment is. 
3. Open a terminal and go to the `terminal` binary (executable) folder with `cd AutoCheck/terminal/bin/Release/net6.0`.
4. GNU/Linux: run the script with `./run.sh ../scripts/custom/css3_single.yaml`.
5. Windows: run the script with `run ../scripts/custom/css3_single.yaml`.

#### DAM - M04 UF2: Web Syndication (RSS + ATOM)
In order to check this assignment:
1. Copy the `AutoCheck/scripts/targets/web_syndication_single.yaml` script into the `scripts/custom` folder.
2. Edit the `AutoCheck/scripts/custom/web_syndication_single.yaml` script and change the `folder` parameter to point the folder where your assignment is. 
3. Open a terminal and go to the `terminal` binary (executable) folder with `cd AutoCheck/terminal/bin/Release/net6.0`.
4. GNU/Linux: run the script with `./run.sh ../scripts/custom/web_syndication_single.yaml`.
5. Windows: run the script with `run ../scripts/custom/web_syndication_single.yaml`.

#### ASIX - M02 UF3: SQL DataBase Permissions
In order to check this assignment:
1. Copy the `AutoCheck/scripts/targets/permissions_single.yaml` script into the `scripts/custom` folder.
2. Edit the `AutoCheck/scripts/custom/permissions_single.yaml` script and change the `host` parameter to point the host name or IP address where your Postgres server is listening to. 
3. Open a terminal and go to the `terminal` binary (executable) folder with `cd AutoCheck/terminal/bin/Release/net6.0`.
4. GNU/Linux: run the script with `./run.sh ../scripts/custom/permissions_single.yaml`.
5. Windows: run the script with `run ../scripts/custom/permissions_single.yaml`.

#### ASIX - M02 UF3: SQL DataBase Updatable Views
In order to check this assignment:
1. Copy the `AutoCheck/scripts/targets/views_single.yaml` script into the `scripts/custom` folder.
2. Edit the `AutoCheck/scripts/custom/views_single.yaml` script and change the `host` parameter to point the host name or IP address where your Postgres server is listening to. 
3. Open a terminal and go to the `terminal` binary (executable) folder with `cd AutoCheck/terminal/bin/Release/net6.0`.
4. GNU/Linux: run the script with `./run.sh ../scripts/custom/views_single.yaml`.
5. Windows: run the script with `run ../scripts/custom/views_single.yaml`.