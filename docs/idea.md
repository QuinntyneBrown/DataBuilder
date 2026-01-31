Create a DataBuilder.Cli project
- Dotnet tool "db"
- Command per file using System.CommandLine
- leverages
    - Microsoft.Extensions.{Logging|DI|Configuration}
    - CommandLineBuilder
- Commands
    - Solution Create ("db solution-create")
        - the command generates an C# N-Tier Api backend 
            - Couchbase as database
            - SystemTextJsonSerializer as defualt Serializer
            - https://www.nuget.org/packages/QuinntyneBrown.Gateway.Core as ORM
            - supports CRUD operations of the user defined Json Object
                - GetAll, GetPage, GetById, Update, Delete, Create
        - the command generates an Angular Workspace named {SolutionName}.Ui
            - workspace has an Angular application called {solution-name}
                - application provides UI for the Api backend (supporting CRUD)
                - application adheres to C:\projects\DataBuilder\docs\ADMIN-UI-IMPLEMENTATION-GUIDE.md
        - accepts the following as command line options
            - directory: directory where the solution will be generated. defualt is System.Environment.CurrentDirectory
            - name: name of solution. default is data-builder
        - the command shall open a JSON editor for a user defined json object can be authoredto
            - the default content of the json editor shall be

            ```json
            {
                "toDo": {
                    "id": 0,
                    "name":""
                }
            }
            ```
            - based on the user json input, the command can infer the name of the entity. In the example, then ane of the entity would be ToDo. (GetAllToDos, CreateToDo..etc..)
        - the command ensure the {solution-name} application in the {SolutionName}.Ui workspaces is configured to use the https url the {SolutionName}.Api is set to run on.
        - the command ensures that Angular material icons is properly linked in the index.html of the {solution-name} application 