# Developer's guide
## How to use it as a library
### Download and compile
Follow this instructions in order to download and compile the application for the first time:

1. Create a folder where the application will be downloaded in.
2. Open a terminal and go into the newly crated folder.
3. Download the application with the following command: `git clone https://github.com/FherStk/AutoCheck.git`.
4. Go into the downloaded application folder.
5. Build the application before using it with the following command: `dotnet build`. 

### Import the libraries
Once compiled, a set of `dll` files will be available in the `\bin\Release\netcoreapp3.0` folder (recomended) or in the `\bin\Debug\netcoreapp3.0` folder (de defualt build option), import this files into your project. 

### Call the scripts
Create new instances of any needed script and invoke its `Batch` (for a set of items) or `Run` (for a single item) methods.

Simplified example:
```
var script = new DAM_M04UF1_Html5Assignment(new string[]{"--path=/home/user/folder/"});
script.Batch();
Console.WriteLine(script.Score());
```

Please, notice that all the script output will be directly send to the terminal; in future releases, it will be possible to get access the output data disabling (if needed) the terminal.

## How to extend the application

### Introduction
The main purpose of this application is to automatically check the correctness for a set of assignments delivered by a group of students, so the application must be as easy as possible to use by students (auto evaluate their own work in order to improve it and learn from the mistakes) and by teachers (create new scripts for assisting the correction process).

All the application has been designed with a main purpose in mind: reuse and extend whatever you need. Each component has its own role and responsibilities, detailed as follows:

![Schema](../images/schema.png)

### Connectors
A connector is a bridge between the application and a data source (database, file, whatever) so, a new connector can be created for communication with a new type of file or service if needed. All the connectors must inherit from `Core.Connector` class and provide access to the connected source (connection string, parsed document, etc.) and any helper or auxiliary method needed for CRUD operations. 

### Checkers
A checker is a bridge between a connector and a script, so it uses the connector in order to validate CRUD operations returning the result in a way that a script can handle (always returns a list of errors, so an empty list means no errors). All the connectors must inherit from `Core.Checker` class and provide access to its connector and any helper or auxiliary method needed for checking items or actions. 

### Scripts
A script is a set of calls to a checker (or checkers) or, to a lesser extent, to a connector (through the checker) in order to perform CRUD operations and validate its results. All the connectors must inherit from `Core.Script` (or some variants like `Core.ScriptFiles` or `Core.ScriptDB`) class and provide access to its properties. Any script must be extremely readable and as simple as possible.

Simplified example:
```
OpenQuestion("Question 1", "Index");
    Checkers.Html index = new Checkers.Html(this.Path, "index.html");
    index.Connector.ValidateHTML5AgainstW3C();

    OpenQuestion("Question 1.1", "Validating headers", 1);
        EvalQuestion(index.CheckIfNodesMatchesAmount("//h1", 1, Checkers.Html.Operator.MIN));
        EvalQuestion(index.CheckIfNodesMatchesAmount("//h2", 1, Checkers.Html.Operator.MAX));
    CloseQuestion();

    OpenQuestion("Question 1.2", "Validating images", 2);
        EvalQuestion(index.CheckIfNodesMatchesAmount("//img", 1));
    CloseQuestion();
CloseQuestion();

OpenQuestion("Question 2", "Validating text fields", 1.5f);
    EvalQuestion(index.CheckIfNodesMatchesAmount("//input[@type='text']", 2, Checkers.Html.Operator.EQUALS));
CloseQuestion();

PrintScore();
```

### Copy detectors
A copy detector is a set of methods which main goal is the check if an student's file is a copy of another one (or some). All the copy detectors must inherit from `Core.CopyDetector` class and provide an implementation for their abstract methods (it will depend on every file type or content).

### Core
The core contains a set of classes intended to be inherited (as mentioned before) but also contains the `Output` and the `Utils`. The first one is used for sending data to the output (terminal, log files, etc); the second one contains a set of useful methods and miscellanea.

## How to create a new script
The following guide decribes how to create new scripts using checkers and the scoring mechanism, please follow this instructions:
1. Choose an existing script (the more similar to your needs, the better) and copy it inside the **scripts folder**. 
2. Set the script name as the main file name and also within the file (as class name and constructor name, as the other scripts does):
```
public class My_New_Script: Core.Script<CopyDetectors.None>{                       
    public My_New_Script(string[] args): base(args){        
        ...
    }
}
```
3. Choose the base script you want to use (**Script** for generic ones, **ScriptDB** for databse oriented script, **ScriptFiles** for file oriented scripts) and set it on the class declaration:
```
public class My_New_Script: Core.Script<CopyDetectors.None>{                       
    public My_New_Script(string[] args): base(args){        
        ...
    }
}
```

4. Choose the copy detector you want to use from the **copy folder**, and set it next to the class declaration:
```
public class My_New_Script: Core.Script<CopyDetectors.None>{                       
    public My_New_Script(string[] args): base(args){        
        ...
    }    
}
```

5. Create a new checker instance (choose the one which best fits with your needs from the **checkers folder**) in order to use it along the script: 
```
public class My_New_Script: Core.Script<CopyDetectors.None>{                       
    public My_New_Script(string[] args): base(args){        
        Checkers.Html index = new Checkers.Html(this.Path, "index.html");
        ...
    }
}
```

6. Open the question you want to evaluate, setting up a caption, a description, and the score to compute: 
```
public class My_New_Script: Core.Script<CopyDetectors.None>{                       
    public My_New_Script(string[] args): base(args){        
        Checkers.Html index = new Checkers.Html(this.Path, "index.html");
        OpenQuestion("Question 1.1", "Validating headers", 1);
        ...
    }
}
```

6. Use the **EvalQuestion** method in order to compute a check result within the currently opened question, any kind of opperation can be performed when the question is opened but only the ones using**EvalQuestion** will score. Please, note than all checker's methods will return a set of values compatibles with **EvalQuestion**, and all the calls must be error free in order to compute the current question score. :
```
public class My_New_Script: Core.Script<CopyDetectors.None>{                       
    public My_New_Script(string[] args): base(args){        
        Checkers.Html index = new Checkers.Html(this.Path, "index.html");
        OpenQuestion("Question 1.1", "Validating headers", 1);
        EvalQuestion(index.CheckIfNodesMatchesAmount("//h1", 1, Checkers.Html.Operator.MIN));
        EvalQuestion(index.CheckIfNodesMatchesAmount("//h2", 1, Checkers.Html.Operator.MAX));
        ...
    }
}
```

7. Once all the validations have been perfomed, the question must be closed for computing the score:
```
public class My_New_Script: Core.Script<CopyDetectors.None>{                       
    public My_New_Script(string[] args): base(args){        
        Checkers.Html index = new Checkers.Html(this.Path, "index.html");
        OpenQuestion("Question 1.1", "Validating headers", 1);
        EvalQuestion(index.CheckIfNodesMatchesAmount("//h1", 1, Checkers.Html.Operator.MIN));
        EvalQuestion(index.CheckIfNodesMatchesAmount("//h2", 1, Checkers.Html.Operator.MAX));
        CloseQuestion();
        ...
    }
}
```

8. With the current example, the two validations performed (for h1 and h2 nodes) must be error free in order to compute the one point score, otherwise no score will be added to the final result. **PrintScore** can be used to display the final score:
```
public class My_New_Script: Core.Script<CopyDetectors.None>{                       
    public My_New_Script(string[] args): base(args){        
        Checkers.Html index = new Checkers.Html(this.Path, "index.html");
        OpenQuestion("Question 1.1", "Validating headers", 1);
        EvalQuestion(index.CheckIfNodesMatchesAmount("//h1", 1, Checkers.Html.Operator.MIN));
        EvalQuestion(index.CheckIfNodesMatchesAmount("//h2", 1, Checkers.Html.Operator.MAX));
        CloseQuestion();
        PrintScore();
    }
}
```

Please, note than multiple checkers and/or connectors can be used, so different guides can be mixed as needed.

### Local command validation example
```
var local = new Checkers.LocalShell();
OpenQuestion("Question 1", "File creation");
    OpenQuestion("Question 1.1", 1);
        EvalQuestion(db.CheckIfFolderExists("/home/user/", "myFile.txt"));
    CloseQuestion();   

    OpenQuestion("Question 1.2", 1);
        EvalQuestion(db.CheckIfCommandMatchesResult("cat /home/user/myFile.txt", "This is the content of the file.");             
    CloseQuestion();   
CloseQuestion();   
```

### Remote command validation example
```
var remote = new Checkers.RemoteShell("192.168.0.127", "myUser", "myPassword");
OpenQuestion("Question 1", "File creation");
    OpenQuestion("Question 1.1", 1);
        EvalQuestion(db.CheckIfFolderExists("/home/user/", "myFile.txt"));
    CloseQuestion();   

    OpenQuestion("Question 1.2", 1);
        EvalQuestion(db.CheckIfCommandMatchesResult("cat /home/user/myFile.txt", "This is the content of the file.");             
    CloseQuestion();   
CloseQuestion();   
```

### Database validation example
```
var db = new Checkers.Postgres(this.Host, this.DataBase, this.Username, this.Password);
OpenQuestion("Question 1", "View creation");
    OpenQuestion("Question 1.1", 1);
        EvalQuestion(db.CheckIfTableExists("gerencia", "responsables"));
    CloseQuestion();   

    OpenQuestion("Question 1.2", 1);
        EvalQuestion(db.CheckIfViewMatchesDefinition("gerencia", "responsables", @"
            SELECT  e.id AS id_responsable,
                    e.nom AS nom_responsable,
                    e.cognoms AS cognoms_responsable,
                    f.id AS id_fabrica,
                    f.nom AS nom_fabrica
            FROM rrhh.empleats e
            LEFT JOIN produccio.fabriques f ON e.id = f.id_responsable;"
        ));             
    CloseQuestion();   
CloseQuestion();   

OpenQuestion("Question 2", "Insert rule");
    EvalQuestion(db.CheckIfTableInsertsData("gerencia", "responsables", new Dictionary<string, object>(){
        {"nom_fabrica", "NEW FACTORY NAME 1"}, 
        {"nom_responsable", "NEW EMPLOYEE NAME 1"},
        {"cognoms_responsable","NEW EMPLOYEE SURNAME 1"}
    }));
CloseQuestion();   
```

### HTML validation example
```
var index = new Checkers.Html(this.Path, "index.html");
OpenQuestion("Question 1", "Index");
    index.Connector.ValidateHTML5AgainstW3C();

    OpenQuestion("Question 1.1", "Validating headers", 1);
        EvalQuestion(index.CheckIfNodesMatchesAmount("//h1", 1, Checkers.Html.Operator.MIN));
        EvalQuestion(index.CheckIfNodesMatchesAmount("//h2", 1, Checkers.Html.Operator.MAX));
    CloseQuestion();

    OpenQuestion("Question 1.2", "Validating images", 2);
        EvalQuestion(index.CheckIfNodesMatchesAmount("//img", 1));
    CloseQuestion();
CloseQuestion();

OpenQuestion("Question 2", "Validating text fields", 1.5f);
    EvalQuestion(index.CheckIfNodesMatchesAmount("//input[@type='text']", 2, Checkers.Html.Operator.EQUALS));
CloseQuestion();

PrintScore();
```

## CSS validation example
```
var css = new Checkers.Css(this.Path, "index.css");
OpenQuestion("Question 2", "CSS");    
    css.Connector.ValidateCSS3AgainstW3C();    //exception if fails, so no score will be computed

    OpenQuestion("Question 2.1", 1);
        EvalQuestion(css.CheckIfPropertyHasBeenApplied(html.Connector.HtmlDoc, "font"));
    CloseQuestion();

    OpenQuestion("Question 2.3", 1);
        EvalQuestion(css.CheckIfPropertyHasBeenApplied(html.Connector.HtmlDoc, "position", "relative"));
    CloseQuestion();

    OpenQuestion("Question 2.3", 1);
        EvalQuestion(css.CheckIfPropertiesAppliedMatchesAmount(html.Connector.HtmlDoc, new string[]{
            "top",
            "right",
            "bottom",
            "left"
        }, 1, Connector.Operator.MIN));
    CloseQuestion();
CloseQuestion();

PrintScore();
```