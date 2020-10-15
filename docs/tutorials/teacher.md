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

### Script definition


### Single mode execution

### Batch mode execution


# ----------- OLD -----------------
The following guide decribes how to create new scripts using checkers and the scoring mechanism, please follow this instructions:
1. Choose an existing script (the more similar to your needs, the better) and copy it inside the **scripts folder**. 
2. Set the script name as the main file name and also within the file (as class name and constructor name, as the other scripts does):
```
public class My_New_Script: Core.Script<CopyDetectors.None>{                       
    public My_New_Script(Dictionary<string, string> args): base(args){        
        ...
    }
}
```
3. Choose the base script you want to use (**Script** for generic ones, **ScriptDB** for databse oriented script, **ScriptFiles** for file oriented scripts, **ScriptGDrive** for copying Google Drive files from one account to another) and set it on the class declaration:
```
public class My_New_Script: Core.Script<CopyDetectors.None>{                       
    public My_New_Script(Dictionary<string, string> args): base(args){        
        ...
    }
}
```

4. Choose the copy detector you want to use from the **copy folder**, and set it next to the class declaration:
```
public class My_New_Script: Core.Script<CopyDetectors.PlainText>{                       
    public My_New_Script(Dictionary<string, string> args): base(args){        
        ...
    }    
}
```

5. Create a new checker instance (choose the one which best fits with your needs from the **checkers folder**) in order to use it along the script: 
```
public class My_New_Script: Core.Script<CopyDetectors.PlainText>{                       
    public My_New_Script(Dictionary<string, string> args): base(args){        
        var index = new Checkers.Html(this.Path, "index.html");
        ...
    }
}
```

6. Open the question you want to evaluate, setting up a caption, a description, and the score to compute: 
```
public class My_New_Script: Core.Script<CopyDetectors.PlainText>{                       
    public My_New_Script(Dictionary<string, string> args): base(args){        
        var index = new Checkers.Html(this.Path, "index.html");
        OpenQuestion("Question 1.1", "Validating headers", 1);
        ...
    }
}
```

6. Use the **EvalQuestion** method in order to compute a check result within the currently opened question, any kind of opperation can be performed when the question is opened but only the ones using **EvalQuestion** will score. Please, note than all checker's methods will return a set of values compatibles with **EvalQuestion**, and all the calls must be error free in order to compute the current question score:
```
public class My_New_Script: Core.Script<CopyDetectors.PlainText>{                       
    public My_New_Script(Dictionary<string, string> args): base(args){        
        var index = new Checkers.Html(this.Path, "index.html");
        OpenQuestion("Question 1.1", "Validating headers", 1);
        EvalQuestion(index.CheckIfNodesMatchesAmount("//h1", 1, Operator.LOWER));
        EvalQuestion(index.CheckIfNodesMatchesAmount("//h2", 1, Operator.GREATER));
        ...
    }
}
```

7. Once all the validations have been perfomed, the question must be closed for computing the score. Remember that the score has been setup when opening the question, so any error found until closing it will not compute any socre (partial questions can be used for compute partial scores, opening and closing subquestions):
```
public class My_New_Script: Core.Script<CopyDetectors.PlainText>{                       
    public My_New_Script(Dictionary<string, string> args): base(args){        
        var index = new Checkers.Html(this.Path, "index.html");
        OpenQuestion("Question 1.1", "Validating headers", 1);
        EvalQuestion(index.CheckIfNodesMatchesAmount("//h1", 1, Operator.LOWER));
        EvalQuestion(index.CheckIfNodesMatchesAmount("//h2", 1, Operator.GREATER));
        CloseQuestion();
        ...
    }
}
```

8. With the current example, the two validations performed (for h1 and h2 nodes) must be error free in order to compute the one point score, otherwise no score will be added to the final result. **PrintScore** can be used to display the final score:
```
public class My_New_Script: Core.Script<CopyDetectors.PlainText>{                       
    public My_New_Script(Dictionary<string, string> args): base(args){        
        var index = new Checkers.Html(this.Path, "index.html");
        OpenQuestion("Question 1.1", "Validating headers", 1);
        EvalQuestion(index.CheckIfNodesMatchesAmount("//h1", 1, Operator.LOWER));
        EvalQuestion(index.CheckIfNodesMatchesAmount("//h2", 1, Operator.GREATER));
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
        EvalQuestion(index.CheckIfNodesMatchesAmount("//h1", 1, Operator.LOWER));
        EvalQuestion(index.CheckIfNodesMatchesAmount("//h2", 1, Operator.GREATER));
    CloseQuestion();

    OpenQuestion("Question 1.2", "Validating images", 2);
        EvalQuestion(index.CheckIfNodesMatchesAmount("//img", 1));
    CloseQuestion();
CloseQuestion();

OpenQuestion("Question 2", "Validating text fields", 1.5f);
    EvalQuestion(index.CheckIfNodesMatchesAmount("//input[@type='text']", 2, Operator.EQUALS));
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