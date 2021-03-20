# Interactive Small Basic (ISB)

Interactive Small Basic (ISB) is a simple scripting language derived from
[Microsoft Small Basic](https://github.com/sb).

I implemented ISB to support the following scenarios mainly:

 * As an in-game scripting language, to be embedded in Unity games.
 * As a shell scripting language, to provide a command-line interface where simple code pieces can be executed to
   control the host system.

The first implementation of ISB is written in C# (as the original Microsoft Small Basic does) since C# is Unity's
recommended scripting language.

Compared to the original Microsoft Small Basic, ISB has the following updates:

 * The compiler and the runtime support incremental invocations. Code pieces can be passed into the system one by one
   without losing internal states.
 * The client program can access some internal states, such as the stack top, to provide an interactive experience.
   E.g., it's easy to build a read-evaluate-print loop on top of ISB to implement a command-line calculator.
 * I simplified most of the XML-data-driven logic. I also used the reflection feature to support a way more
   straightforward structure to add and maintain library functions and properties.
 * I implemented an IR (intermediate representation) to split the compiler into a front-end and a back-end. The IR
   is a simple assembly language, which looks a little bit similar to WebAssembly.
 * Other trivial updates such as the "mod" operator.

## Usage

Build and test:

```
$ cd csharp
$ dotnet restore
$ dotnet build
$ dotnet test
```

Start the example shell and try the language:

```
$ dotnet run -p ISB.Shell
ISB.Shell ...
Copyright (C) ...

Type "quit" to exit, "list" to show the code, "clear" to clear the code, "help" to list available libraries.
] print("Hello, World!")
Hello, World!
]
] for i = 1 to 10
>   print(math.log10(i))
> endfor
0
0.301029995663981
0.477121254719662
0.602059991327962
0.698970004336019
0.778151250383644
0.845098040014257
0.903089986991944
0.954242509439325
1
] quit
$
```

The example shell can also be used as a command-line ISB compiler:

```
$ dotnet run -p ISB.Shell -- --help
ISB.Shell ...
Copyright (C) ...

  -i, --input      BASIC file (*.bas) to run/compile, or ISB assembly file (*.asm) to run. If not set, the interactive
                   shell mode will start.

  -c, --compile    Compile BASIC code to ISB assembly, without running it.

  -o, --output     Output file path when --compile is set. If not set, the output assembly will be written to stdout.

  --help           Display this help screen.

  --version        Display version information.
```

## How to integrate ISB compiler and runtime into Unity games

### Create a Unity project

Initiate the scene and the game objects in Unity. Typically, we need a multi-line input field to input BASIC code, and
a button to trigger the code execution.

![](screenshots/01.png)

### Import the ISB assembly

Makes a `Scripts` dir under `Asserts` of the Unity project.

Copy `csharp/ISB/bin/Debug/netstandard2.0/ISB.dll` into the `Scripts` folder.

### Connect Unity and ISB with C# code

Create `Game.cs` under `Asserts/Scripts'. The class defined in `Game.cs` will be registered as an ISB external lib
by the main program.

```
// Game.cs
public class Game
{
    [Doc("Example lib function to access Unity objects.")]
    public void AddBall(NumberValue x, NumberValue y, NumberValue z)
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Sphere");
        if (prefab != null)
        {
            Object.Instantiate(prefab,
                new Vector3((float)x.ToNumber(), (float)y.ToNumber(), (float)z.ToNumber()),
                Quaternion.identity);
        }
        else
        {
            Debug.Log("Failed to load prefab.");
        }
    }
}
```

The lib funciton `AddBall` simply loads the sphere prefab and instantiates a clone object then places it at the
location specified by the function's arguments.

In-game BASIC code can invoke `AddBall(x, y, z)` to put balls onto the scene.

Then, creates an empty GameObject in Unity to host the main program `Program.cs`.

```
// Program.cs
public class Program : MonoBehaviour
{
    public Button uiButton;
    public InputField uiInput;
    public GameObject objBall;

    void Start()
    {
        uiButton.onClick.AddListener(onButtonClick);
    }

    void Update()
    {
    }

    void onButtonClick()
    {
        string code = uiInput.text;
        Debug.Log(code);

        Engine engine = new Engine("Unity", new Type[] { typeof(Game) });
        if (engine.Compile(code, true) && engine.Run(true))
        {
            if (engine.StackCount > 0)
            {
                string ret = engine.StackTop.ToDisplayString();
                Debug.Log(ret);
            }
        }
        else
        {
            foreach (var content in engine.ErrorInfo.Contents)
            {
                Debug.Log(content.ToDisplayString());
            }
        }
    }
}
```

The code line `Engine engine = new Engine("Unity", new Type[] { typeof(Game) });` registers the class `Game` into
the ISB engine.

The click event handler of the button reads the input BASIC code from the input field then compiles and runs the code
with the ISB engine. Error messages got from the ISB engine will be reported to Unity's `Debug.Log`.

### Start the game and execute BASIC code in game

Start the Unity game. Type the following example code into the input field:

```
For x = -3 To 3
  For z = -3 To 3
     Game.AddBall(x, 5, z)
  EndFor
EndFor
```

Click the "Run" button.

49 bouncing balls will be put onto the main scene. Enjoy them!

![](screenshots/02.png)

![](screenshots/03.gif)

