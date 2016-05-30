# Hark Library

## Plan

- [Introduction](#introduction)
- [Project structure](#project-structure)
- [Compilation](#compilation)
- [Usages](#usages)
- [Tasks](#tasks)
- [License](#license)

## Introduction

This is a set of .NET libraries for advanced use.

### Disclaimer

No contributor is responsible or liable for illegal use of
this library.

### Documentation

| Library | Documentation |
| --- | --- |
| HarkLib.Parsers.Generic.dll | [Delimiter Regular Expression (drex)](https://github.com/OpenHark/HarkLib/wiki/Delimiter-Regular-Expression-(drex)) |
| HarkLib.Parsers.dll | [Document](https://github.com/OpenHark/HarkLib/wiki/) |
|  | [HTML](https://github.com/OpenHark/HarkLib/wiki/) |
|  | [XML](https://github.com/OpenHark/HarkLib/wiki/) |
| HarkLib.Security.dll | [AES](https://github.com/OpenHark/HarkLib/wiki/) |
|  | [RSA](https://github.com/OpenHark/HarkLib/wiki/) |
|  | [SecureConsole](https://github.com/OpenHark/HarkLib/wiki/) |
|  | [SecurePassword](https://github.com/OpenHark/HarkLib/wiki/) |
| HarkLib.Core.dll | [FileCache](https://github.com/OpenHark/HarkLib/wiki/) |
|  | [FileSettings](https://github.com/OpenHark/HarkLib/wiki/) |
|  | [UIDManager](https://github.com/OpenHark/HarkLib/wiki/) |
|  | [Exceptions](https://github.com/OpenHark/HarkLib/wiki/) |
|  | [Extensions](https://github.com/OpenHark/HarkLib/wiki/) |
| HarkLib.Net.dll | [Resolver](https://github.com/OpenHark/HarkLib/wiki/) |
|  | [GhostMail](https://github.com/OpenHark/HarkLib/wiki/) |
|  | [WebResource](https://github.com/OpenHark/HarkLib/wiki/Web-Resource) |

## Project structure

```
HarkLib
|:: .make               - Solution builder folder
    |:: config.ini      - Configuration file for compilation
|:: .vscode             - Visual Studio Code folder (optional)
|:: out                 - Output folder
|:: src                 - Source code
    |:: Core            - C#
        |:: Exceptions
        |:: Extensions
    |:: Net             - C#
        |:: Exceptions
        |:: WebResource
    |:: Parsers         - F#
    |:: Parsers.Generic - C#
        |:: Exceptions
    |:: Security        - C#
        |:: Extensions
    |:: UnitTesting     - C# for some unit tests
        |:: Net
        |:: Parsers
            |:: Sequencers
        |:: Security
```

> Because the C# language and the F# have their own strengths
and weaknesses, I made the choice to combine both where I
feel their are the better.

## Compilation

You need to have installed csc.exe (C# compiler) and fsc.exe
(F# compiler). You need to have both compilers accessible in
you PATH environment variable.

If you are using Visual Studio Code, you just have to press
Ctrl+Shift+B. It will run the command 'make'.

If you are not using Visual Studio Code, open a terminal on
the root folder (where there is the file **makefile**) and run
the following command : ` make ` or with full specification :
` make build `.

If you want to clean the project, you can run ` make clean `.

If you want to clean the project and compile just after, please
run ` make clean build `.

The **.make** folder contains the source code (C#) of the solution
compiler. If needed, you can recompile it.

The compilation uses cache system which allows faster compilations.
Indeed, if you edit a project, it will recompile only this one.
This may result in some errors if you recompile a library while
removing a method/class. The other programs will try to find this
class or this method at JIT compilation time, producing a runtime
error. To solve this kind of problem, just clean and rebuild the whole
project.

In the file **.make/config.ini**, you can find the configuration of
the compilation. This way, no need to recompile the compiler to
change the references added in each sub project, the output file
names, etc...

## Usages

If you are using a compiler, you can load one or many of the libraries
provided by this project by adding a reference while compiling.

If you use the command line, you can compile a C# program with :

```sh
csc ... /reference:HarkLib.Core.dll /reference:HarkLib.Net ...
```

For a F# program :

```sh
fsc ... --reference:HarkLib.Core.dll --reference:HarkLib.Net ...
```

For a C++/CLI program :

```sh
link ... HarkLib.Code.dll ...
```

If you are using Visual Studio, you can add it as a reference in
you project. If you don't know how, you can follow this "[tutorial](https://msdn.microsoft.com/en-us/library/7314433t(v=vs.90).aspx)".

If you want to use a library without referencing it, you can use
the following way (here in C#) :

```csharp
Assembly assembly = Assembly.Load(AssemblyName.GetAssemblyName("HarkLib.Security.dll"));

foreach(Type t in assembly.GetTypes())
    Console.WriteLine(t.Name);
```

## Tasks

- Parsers
  - [X] Add XML permissive parsing
- Parsers.Generic
  - [X] ByteSequencer : Create a better result parsing for `or` (`{...|...}`)
  - [X] ByteSequencer : Add exclude pattern
  - [ ] Clean the files
  - [ ] Test in real situations
- Security
  - [X] Add RSA
  - [X] Add "anti-forcing system" (slow generation)
- Net
  - [ ] Add web client
  - [ ] Add web server
  - [ ] Add webdav server
  - [X] Add ghost mail system
  - [ ] Add web directory browser
  - [ ] Add light web server for software interface
  - [ ] Add light webdav server for software interface

## License

![GNU AGPLv3](https://www.gnu.org/graphics/agplv3-155x51.png)

[[Can-Cannot-Must description]](https://www.tldrlegal.com/l/agpl3)
[[Learn more]](http://www.gnu.org/licenses/agpl-3.0.html)
