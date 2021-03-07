# The ISB standard libraries.

A standard library is either a built-in library or a named library:

 * Built-in library: the library defined by `BuiltIn.cs`. The functions
   of the built-in library are accessed without lib name by the BASIC code.
   E.g.: `Print(x+y)`, `Quit()`, etc. There is no properties defined in the
   built-in library (to avoid ambiguity between variable names and property
   names).
 * Named library: each named library is defined by a separate C# class. The
  functions and the properties of a named libraries are accessed via
  `LibName.EntryName`. E.g.: `Math.Sin(x)`, `Math.Pi`, etc.

The names of libraries, functions and propertis are case insensitive in BASIC
code. E.g., `Math.Pi` is the same thing as `math.pi`, `Math.PI` or `MATH.PI`.

## Note

All the c# classes in the `ISB.Lib` namespace (under this dir) are loaded as
ISB standard libraries automatically. Do NOT put non-lib classes here.

## Functions

For a library definition class, all public methods that fulfill the following
criteria are loaded as the functions of that library automatically:

 * The method must not be an inherited method, i.e., it must not have the
   `override` modifier.
 * The method must have zero or a definite number of arguments. The `params`
   keyword is not allowed.
 * The `out` parameter modifier is not allowed.
 * Optional parameters are not allowed.
 * The type of all the arguments must be either `ISB.Runtime.BaseValue` or a
   derived class of `ISB.Runtime.BaseValue`, such as `ISB.Runtime.NumberValue`,
   `ISB.Runtime.StringValue` or `ISB.Runtime.ArrayValue`.
 * If the return type is not void, it must be either `ISB.Runtime.BaseValue` or
   a derived class of `ISB.Runtime.BaseValue`.

For example:

```
namespace ISB.Lib
{
    public class Math
    {
        public NumberValue Sin(NumberValue x) {
            double value = System.Math.Sin(Convert.ToDouble(x.ToNumber()));
            return new NumberValue(Convert.ToDecimal(value));
        }
    }
}
```

## Property

For a library definition class, all public properties that fulfill the
following criteria are loaded as the properties of that library automatically:

 * The type of the property must be either `ISB.Runtime.BaseValue` or a
   derived class of `ISB.Runtime.BaseValue`.

As for the accessibility of the exported properties:

 * If the property has both the public getter and the public setter, the
   property is writable from the BASIC code.
 * If the property has only the public getter, the property is readonly from
   the BASIC code.

For example, the following code defines a readonly property. Its value is
initialized by the class constructor:

```
namespace ISB.Lib
{
    public class Math
    {
        public NumberValue Pi { get; private init; }

        public Math()
        {
            this.Pi = new NumberValue(3.1415926);
        }
    }
}
```