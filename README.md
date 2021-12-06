# Interactive Small Basic (ISB)

Interactive Small Basic (ISB) is a simple scripting language derived from
[Microsoft Small Basic](https://github.com/sb).

## Background

I implemented ISB to support the following scenarios:

* As an in-game scripting language, to be embedded in Unity games.
* As a shell scripting language, to provide a command-line interface where
  simple code can be executed to control the host system.

ISB is implemented in C# (as the original Microsoft Small Basic does) since C#
is Unity's default scripting language.

## ISB Programming Language

Microsoft maintains a [comprehensive introduction of Small
Basic](https://smallbasic-publicwebsite.azurewebsites.net/tutorials/chapter1).
It's recommended to read it first if you are not familiar with at least one
BASIC dialects.

Interactive Small Basic (ISB) has a couple of language and API differences
compared with Microsoft Small Basic (MSB). I highlighted and described those
features and APIs in the [Quick Intro: ISB Programming
Language](./isb_quick_intro.md) doc.

## Build and Test

Build and test from source code:

```shell
cd csharp

dotnet build

dotnet test
```

## ISB Interactive Shell

Run the interactive shell of ISB:

```shell
$ dotnet run -p ISB.Shell
ISB.Shell ...
Copyright (C) ...

Type "quit" to exit, "list" to show the code, "clear" to clear the code, "help" to list available libraries.
] print("Hello, World!")
Hello, World!
]
] For i = 1 To 10
>   Print(Math.Log10(i))
> EndFor
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

The shell can also be run as a command-line compiler and executor of ISB:

```shell
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

For example, print the fibonacci sequence with the sample code:

```shell
dotnet run -p ISB.Shell -- -i ../examples/fibonacci.bas
```

## Embed ISB in .Net Projects

To reference to the ISB DLL module from your .Net project, it's recommended to
import ISB via NuGet (Of course, you can also copy the source code or the
DLL of ISB to your project manually).

ISB is released as a NuGet package at
[https://www.nuget.org/packages/isb](https://www.nuget.org/packages/isb).

In your .Net project, add the NuGet package of ISB:

```shell
dotnet add package ISB
```

Now you are ready to create an instance of the ISB engine to compile and run
BASIC code. For example, here is a C# program that runs ISB:

```CSharp
using System.Collections.Generic;
using ISB.Runtime;
using ISB.Utilities;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            var engine = new Engine("test");
            string code = "print(\"Hello, World!\")";
            engine.Compile(code, true);
            if (engine.HasError)
            {
                ReportErrors(engine.ErrorInfo.Contents);
                return;
            }
            engine.Run(true);
            if (engine.HasError)
            {
                ReportErrors(engine.ErrorInfo.Contents);
                return;
            }
        }

        private static void ReportErrors(IReadOnlyList<Diagnostic> diagnostics)
        {
            foreach (var diagnostic in diagnostics)
            {
                System.Console.WriteLine(diagnostic.ToDisplayString());
            }
        }
    }
}
```

## Unity In-Game Scripting

A main use scenario of ISB is Unity in-game scripting.

The [Unity integration demos](./unity_integration_demos) show how ISB can be
embedded in Unity projects to enable users to control game object via BASIC
code.

Below are some quick descriptions of the
[AddGameObject](./unity_integration_demos/AddGameObjects/) demo.

### Prepare the Unity project

Initiate the scene and the game objects in Unity. Typically, we need a
multi-line input field to type BASIC code in, and a button to trigger the
execution.

![Unity Demo 1](screenshots/01.png)

### Import the ISB assembly

Option 1: import ISB via NuGet

[NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) is a unity package
that helps manage NuGet dependencies. You can install
[NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) first then use it
to install the ISB NuGet package. Check its documentations for more details.

Option 2: import ISB Dll directly

Build the ISB dll from source code and copy the ISB assembly
`ISB/bin/Debug/netstandard2.0/ISB.dll` to your Unity project's `Assets/Plugins`
or `Assets/Script` dir.

### Define a BASIC library to control game objects

Create `Game.cs` under `Asserts/Scripts`. The class defined in `Game.cs` will be
registered as an ISB external library.

```CSharp
using ISB.Runtime;
using ISB.Utilities;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class Game
{
    [Doc("Example lib function to access Unity objects.")]
    [Preserve]
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

The lib function `AddBall` simply loads the sphere prefab and instantiates a
clone object then places it at the location specified by the function's
arguments. In-game BASIC code can then invoke `AddBall(x, y, z)` to put balls
onto the scene.

Note that the attribute `[Preserve]` is used to preventing Unity from stripping
the library code. See Unity's [Managed code
stripping](https://docs.unity3d.com/Manual/ManagedCodeStripping.html) for more
details.

### Run the ISB engine in Game

Now in the button handler code, an ISB engine can be set up to compile and run
BASIC code.

```CSharp
public class GameManager : MonoBehaviour
{
    public Button RunButton;
    public InputField CodeInput;

    void Start()
    {
        RunButton.onClick.AddListener(OnRun);
    }

    public void OnRun()
    {
        string code = CodeInput.text;
        Engine engine = new Engine("UnityDemo", new Type[] { typeof(Game) });
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

The code `Engine engine = new Engine("Unity", new Type[] { typeof(Game) });`
registers the class `Game` into the ISB engine.

The button click event handler reads BASIC code from the input field then
compiles and runs it with the ISB engine. Error messages got from the ISB engine
will be reported to Unity's `Debug.Log`.

### Run the Unity project

Start the Unity game. Type the following BASIC code in the input field:

```BASIC
For x = -3 To 3
  For z = -3 To 3
     Game.AddBall(x, 5, z)
  EndFor
EndFor
```

Click the "Run" button.

49 bouncing balls will be put onto the main scene. Enjoy them!

![Unity Demo 2](screenshots/02.png)

![Unity Demo 3](screenshots/03.gif)

### Run BASIC code as Unity Coroutine

To prevent an execution of BASIC code from blocking Unity's animation loop, the
ISB engine also provides an interface to run BASIC code as a [Unity
coroutine](https://docs.unity3d.com/Manual/Coroutines.html).

Here is the coroutine version of the button's click handler:

```CSharp
    public void OnRun()
    {
        string code = Code.text;
        DebugInfo.text = "";
        Engine engine = new Engine("UnityDemo", new Type[] { typeof(Game) });
        if (!engine.Compile(code, true))
        {
            ReportErrors(engine);
            return;
        }
        // Runs the program in a Unity coroutine.
        Action<bool> doneCallback = (isSuccess) =>
        {
            if (!isSuccess)
            {
                ReportErrors(engine);
            }
            else if (engine.StackCount > 0)
            {
                string ret = engine.StackTop.ToDisplayString();
                PrintDebugInfo(ret);
            }
        };
        // Prevents the scripting engine from being stuck in an infinite loop.
        int maxInstructionsToExecute = 1000000;
        Func<int, bool> stepCallback =
            (counter) => counter >= maxInstructionsToExecute ? false : true;
        StartCoroutine(engine.RunAsCoroutine(doneCallback, stepCallback));
    }
```

A `doneCallback` can be passed in to receive the final execution state.

The code also uses a `stepCallback` to check if the BASIC code has
time-consuming logic such as infinite loops. The execution will be canceled if
the it exceeds a large number of IR instructions.
