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


