# Teacher's guide
## How to create a new script
A script is a set of calls to connectors in order to perform CRUD operations and validate its results. All the scripts are defined as YAML files and should be as simplier as possible in order to improve readability.

Simplified example:
```
name: "DAM - M04 (UF1): HTML5 Assignment"
body:
  - connector:            
      type: "Html"        
      arguments: "--folder {$CURRENT_FOLDER} --file index.html"       
        
  - question: 
      description: "Checking index.html"                          
      content:                            

        - run:
            caption:  "Validating document against the W3C validation service... "
            connector: "Html"
            command:  "ValidateHTML5AgainstW3C"            
            onexception: "SKIP"

        - question:                     
            description: "Validating headers"
            content:                            
              - run:
                  caption:  "Checking amount of level-1 headers... "
                  connector: "Html"
                  command:  "CountNodes"
                  arguments: "--xpath //h1"                  
                  expected: ">=1"

              - run:
                  caption:  "Checking amount of level-2 headers... "
                  connector: "Html"
                  command:  "CountNodes"
                  arguments: "--xpath //h2"                  
                  expected: ">=1"
```

## YAML node types
There are different nodes types than can be used within a YAML file:

### Scalar
A scalar node means a primitive value, so no children allowed.

In the following example, `type` and `arguments` are scalar nodes:
```
type: "Html"
arguments: "--folder {$CURRENT_FOLDER} --file index.html"
```

### Collection
A collection node means a collection of other nodes, so children are allowed but those cannot be repeated.

In the following example, `vars` is a collection:
```
vars:     
  student_name: "Fer"
  student_var: "{$STUDENT_NAME}"
  student_replace: "This is a test with value: {$STUDENT_NAME}_{$STUDENT_VAR}!" 
```

### Sequence
A sequence node means a collection of other nodes, so children are allowed and also those can be repeated.

In the following example, `body` is a sequence because its children uses `-` before the node name, but each `run` node is a collection:
``` 
body:     
  - run:
      command: "echo {$STUDENT_NAME}"
      expected: "demo"
  
  - run:
      command: "echo {$STUDENT_REPLACE}"
      expected: "This is a test with value: Demo!"

  - run:
      command: "echo {$TEST_FOLDER}"
      expected: "TEST_FOLDER"

  - run:
      command: "echo {$FOLDER_REGEX}"
      expected: "FOLDER"
```

## Script nodes definition
The following guide describes the allowed YAML tags within each level:

#### root
Root-level tags has no parent.

Name | Type | Mandatory | Description | Default
------------ | -------------
name | text | no | The script name will be displayed at the output. | Current file's name
caption | text | no | Message to display before every execution (batch or single). | Running script {$SCRIPT_NAME}:
vars | collection | no | Custom global vars can be defined here and refered later as $VARNAME, allowing regex and string formatters. | 
pre | sequence | no | Defined blocks will be executed (in order) before the body. |
post | sequence | no | Defined blocks will be executed (in order) before the body. |
body | sequence | no | Script body. |

### vars
Custom global vars can be defined wihtin `vars` node and refered later as `$VARNAME`, allowing regex and string formatters.

#### Predefined vars:

Name | Type | Description
------------ | -------------| -------------
SCRIPT_NAME | text | The current script name. 
SCRIPT_CAPTION | text | The current script caption. 
BATCH_CAPTION | text | The current script batch caption (only on batch mode). 
APP_FOLDER | text | The root app execution folder. 
EXECUTION_FOLDER | text | The current script execution folder. 
CURRENT_FOLDER | text | The current script folder for single-typed scripts (the same as "folder"); can change during the execution for batch-typed scripts with the folder used to extract, restore a database, etc.
CURRENT_FILE | text | The current script file for single-typed scripts; can change during the execution for batch-typed scripts with the file used to extract, restore a database, etc.
CURRENT_QUESTION | decimal | The current question (and subquestion) number (1, 2, 2.1, etc.)
CURRENT_SCORE | decimal | The current question (and subquestion) score.
CURRENT_HOST | text | The IP value for the current execution (the same as "ip"); can change during the execution for batch-typed scripts.
CURRENT_TARGET | text | The host or folder where the script is running on batch mode ($CURRENT_HOST or $CURRENT_FOLDER)
MAX_SCORE | decimal | The maximum score available.
TOTAL_SCORE | decimal | The computed total score, it will be updated for each question close.
RESULT | text | Last run command result.
NOW | datetime | The current datetime.  

#### Custom example vars:
Vars can be combined within each other by injection, just call the var with `$` and surround it between brackets `{}` when definint a text var like in the following examples:
```
vars:
    var1: "VALUE1"
    var2: "This is a sample text including {$VAR1} and also {$SCRIPT_NAME}"    
    var3: !!bool False
    var4: !!int 1986    
```

#### Regular expressions over vars example:
Regular expressions (regex) can be used to extract text from an original var, just add the regex expression before calling the var surrounding it between a hashtag `#` and a dollar `$` like in the following examples:
```
vars:    
    var1: "VALUE1"
    var2: "PRE_POST"
    var3: "This is the result of applying a regular expression to var1: {#regex$VAR1}"
    var4: "This will display the last word after an underscore: {#(?<=_)(.*)$VAR2}"
    var5: "This will display the last folder for a given path: {#[^\\\\]+$$CURRENT_FOLDER}"        
```

#### Scopes:
Because `vars` can be used at different script levels, those vars will be created within its own scope being accessible from lower scopes but being destroyed once the scope is left:
```
vars:
    var1:  "Value1.1" #Declares a var1 within the current scope or updates the current value.
    $var1: "Value1.2" #Does not declares but updates var1 value on the first predecesor scope where the var has been found.
```

When requesting a var value (for example: {$var1}) the scope-nearest one will be retreived, so shadowed vars will be not accessible; it has been designed as is to reduce the scope logic complexity, simple workarounds can be used as store the value locally before shadowing or just use another var name to avoid shadowing.

### pre
Defined blocks will be executed (in order) before the script's body does.

#### extract

#### restore_db

#### upload_gdrive

### post
Defined blocks will be executed (in order) after the script's body does; same nodes as `pre` are allowed.

### body

#### vars

#### connector

#### run

#### question

## Single mode execution

## Batch mode execution