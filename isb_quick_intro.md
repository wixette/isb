# Quick Intro: ISB Programming Language

This doc highlights and describes the differences in the programming language
design and implementation between Interactive Small Basic (ISB) and [Microsoft
Small Basic (MSB)]((https://github.com/sb)).

## ISB Programming Language

Microsoft maintains a [comprehensive introduction of Small
Basic](https://smallbasic-publicwebsite.azurewebsites.net/tutorials/chapter1),
which has already covered most programming language features of ISB.

Here I only listed the ISB-specific features.

### Arithmetic operator `Mod`

```BASIC
' a will be set to 3.
a = 10 Mod 7
```

### Logical operator `Not`

```BASIC
` 3 will be printed.
a = 3
If Not (a > 5) Then
  Print(a)
EndIf
```

### Boolean literals and expressions

ISB reserves the keywords `True` and `False` as boolean literals. Boolean
literals and logical operators can be used to construct boolean expressions:

```BASIC
toCheck = True
a = 3
b = 4
c = 5
p2 = (a * a + b * b = c * c)
If toCheck and p2 Then
  print("Passed")
EndIf
```

Note that in BASIC the token `=` can be explained either as an assignment
operator or a logical equal operator, depending on the context. Please use
parentheses when necessary to avoid confusions.

### Implicit Conversions from/to boolean values

ISB supports implicit value conversions among numbers, strings and boolean
values, where the implicit conversions to and from boolean values are different
with MSB.

```BASIC
' Implicit conversion from boolean values to strings.
a = True
b = False
print(a + ", " + b)  ' True, False

' Implicit conversion from boolean values to numbers.
a = True  ' True will be converted to 1
b = False  ' False will be converted to 0
print(a - b)  ' 1

' Implicit conversion from numbers to boolean values.
a = 0
b = True
print(a and b)  ' False
a = 1
b = True
print(a and b)  ' True
a = 0
b = 1
print(a and b)  ' False
a = -1
b = True
print(a and b)  ' True
a = 98765.4321
b = True
print(a and b)  ' True

' Implicit conversion from strings to boolean values.
a = ""
b = True
print(a and b)  ' False
a = "True"
b = True
print(a and b)  ' True
a = "False"
b = True
print(a and b)  ' True
a = "Anything"
b = True
print(a and b)  ' True
```

## Standard and Extendable Libraries

As a lightweight embeddable scripting engine, ISB's standard library functions
and properties are way less than MSB's.

Please type `help` within ISB's interactive shell to get the up-to-date function
and property list of standard libraries.

### How to add more standard libraries

I used C#'s reflection feature to support a straightforward structure to add and
maintain library functions and properties.

See the [standard libraries doc](./csharp/ISB/Lib/README.md) for the steps how
to add more standard libraries.

## Engine API

See [Engine.cs](./csharp/ISB/Runtime/Engine.cs) for the interfaces of the ISB
Engine.

### Incremental Compiler and Executor

The compiler and the runtime support incremental invocations. Code pieces can be
passed into the system one by one without losing internal states.

The client program can access some internal states, such as the stack top, to
provide an interactive experience. E.g., it's easy to build an REPL
(read-evaluate-print loop) on top of ISB to implement a command-line calculator.

See the [REPL loop of the ISB interactive shell](./csharp/ISB.Shell/Program.cs)
as an example.

### Representative IR

I implemented an IR (intermediate representation) to split the compiler into a
front-end and a back-end. The IR is a simple assembly language, looking a bit
similar to WebAssembly.

See [Instruction.cs](./csharp/ISB/Runtime/Instruction.cs) for the instruction
set of the IR.
