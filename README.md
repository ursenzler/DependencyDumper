DependencyDumper
================

Dumps dependencies of classes to a .tgf file and shows them in yEd.

We use this tool to dump the dependencies that are (very likely) be built by our dependency injection container.
The analysis is static, therefore the DependencyDumper dumps potential dependencies, not dependencies that are built during runtime.

A class has the following dependencies:
- all types of its constructor (only classes with a single constructor are processed)
- for interfaces: all types implementing this interface
- for interfaces/classes with a name ending as "Factory": all types returned by its methods
- for generic types: all its generic type paramters (e.g. Foo<Bar> -> Bar)


Usage:
DependencyDumper assemblyFolder assemblyName typePattern minDepth roots absoluteOutputPath

assemblyFolder = absolute path to the folder containing the assemblies to inspect (including all referenced assemblies)
assemblyName = the name of the assembly to start inspection (e.g. MyProduct.exe or MyProduct.Core.dll)
typePattern = only types matching this regular expression are dumped (use .* for all types)
minDepth = only tpyes in a dependency chain that is at least minDepth long are dumped (use 0 to dump all types)
roots = types that are treated as root types regardless whether there are other types having them as dependencies. This allows you to break a dependency chain for better analysis of your code
absoluteOutputPath = path to the output path (should be a .tgf)

This is a raw prototype with a lot of assumptions!

Assumptions (in addition to those stated above):
- yEd is installed under C:\Program Files (x86)\yWorks\yEd\yEd.exe
- you call DependencyDumper correctly (no error handling)

Example
=======

This code (in assembly Example)

```csharp
namespace Example
{
    using System.Collections.Generic;

    public class Foo
    {
        public Foo(Bar bar, IZar zar, IEnumerable<Emu> emus, Generic<TypeParam> generic)
        {
        }
    }

    public class Bar
    {
        public Bar(IMyFactory factory)
        {
        }
    }

    public interface IZar {}

    public class Zar1 : IZar {}

    public class Zar2 : IZar {}

    public interface IMyFactory
    {
        Blah CreateBlah();
        Bloh CreateBloh();
    }

    public class Emu {}

    public class Blah {}

    public class Bloh {}

    public class Generic<T> {}

    public class TypeParam {}
}
```

is dumped as

![alt text](https://raw.github.com/ursenzler/DependencyDumper/master/source/Example/example.png "example")

the numbers after the type name represents the depth of the dependency chain of this type.

Happy dependency dumping!
