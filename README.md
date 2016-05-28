# Hark Library

## Plan

- [Introduction](#introduction)
- [Project structure](#project-structure)
- [Compilation](#compilation)
- [Usages](#usages)
  - [Delimiter Regular Expression (drex)](#usages-drex)
    - [Parsing](#usages-drex-parsing)
    - [Types](#usages-drex-types)
    - [Results](#usages-drex-results)
- [Tasks](#tasks)
- [License](#license)

## <a name="introduction"></a>Introduction

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
| HarkLib.Security.dll | [AES](https://github.com/OpenHark/HarkLib/wiki/) |
|  | [SecureConsole](https://github.com/OpenHark/HarkLib/wiki/) |
|  | [SecurePassword](https://github.com/OpenHark/HarkLib/wiki/) |
| HarkLib.Core.dll | [FileCache](https://github.com/OpenHark/HarkLib/wiki/) |
|  | [FileSettings](https://github.com/OpenHark/HarkLib/wiki/) |
|  | [UIDManager](https://github.com/OpenHark/HarkLib/wiki/) |
|  | [Exceptions](https://github.com/OpenHark/HarkLib/wiki/) |
|  | [Extensions](https://github.com/OpenHark/HarkLib/wiki/) |
| HarkLib.Net.dll | [XML](https://github.com/OpenHark/HarkLib/wiki/) |
|  | [Resolver](https://github.com/OpenHark/HarkLib/wiki/) |
|  | [GhostMail](https://github.com/OpenHark/HarkLib/wiki/) |
|  | [WebResource](https://github.com/OpenHark/HarkLib/wiki/) |

## <a name="project-structure"></a>Project structure

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

## <a name="compilation"></a>Compilation

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

## <a name="usages"></a>Usages

### <a name="usages-drex"></a>Delimiter Regular Expression (drex)

#### <a name="usages-drex-parsing"></a>Parsing

```csharp
using HarkLib.Parsers.Generic;
/* [...] */

ParserResult root = ByteSequencer.Parse(
    "[version: ][i/code: ][message:\r\n][<headers:\r\n\r\n>][name::][|value|:\r\n|$][</>][$s/body$]",
    "HTTP/1.1 404 Not found\r\nHeader1: data1\r\nHeader2: data2\r\n\r\nHello! This is the body!"
).Close();
```

```csharp
using HarkLib.Parsers.Generic;
/* [...] */

ParserResult root = new ByteSequencer("HTTP/1.1 404 Not found\r\nHeader1: data1\r\nErrorHeader\r\nSet-Cookie: a=1\r\nSet-Cookie: b=2\r\nHeader2: data2\r\nErrorHeaderFinal\r\n\r\nHello! This is the body!")
    .Until("version", " ")
    .Until("code", " ", converter : s => int.Parse(s))
    .Until("message", "\r\n")
    .RepeatUntil("headers", "\r\n\r\n", b => b
        .Or(bb => bb
            .Until("name", ":", validator : s => !s.Contains("\r\n"))
            .Until("value", "\r\n", converter : s => s.Trim()),
            bb => bb
            .Until("error", "\r\n"),
            bb => bb
            .ToEnd("final", converter : bs => bs.GetString())))
    .ToEnd("body", converter : bs => bs.GetString())
    .Close();
```

| Operation | Description |
| --- | --- |
| `[version: ]` | Everything until ` ` : called `version`. |
| `[i/code: ]` | Everything until ` ` casted into int : called `code`. |
| `[message:\r\n]` | Everything until `\r\n` : called `message`. |
| `[<headers:\r\n\r\n>]...[</>]` | Repeat `...` until `\r\n\r\n` : called headers. |
| `[name::]` | Everything until `:` : called `name`. |
| `[|value|:\r\n|$]` | Everything until `\r\n` or until the end of the sequence, then trimmed : called `value`. |
| `[$s/body$]` | Everything until the end of the sequence, casted into string : called `body`. |
| `[$|body|$]` | Everything until the end of the sequence, casted into string and trimmed : called `body`. |
| `{[version: ]||[message:\r\n]}` | `version` matching or message matching. |
| `{[version: ]||[subversion:--]||[message:\r\n]}` | `version` matching or subversion matching or message matching. |
| `[HeaderName:x!\r\n]` | HeaderName ends when a `x` is found and must not contain `\r\n`. |
| `[Name]` | Add an empty entry called `Name`. |
| `[ba/Name]` | Add an empty byte array entry called `Name`. |
| `[Name=Content here]` | Add an entry called `Name` with a value equals to `Content here`. |
| `[<Name>]` | Add an empty list entry called `Name`. |


#### <a name="usages-drex-types"></a>Types

| Input | Operation | Result |
| --- | --- | --- |
| `404` | `[code: ]` | "404" |
| `404` | `[s/code: ]` | "404" |
| `404` | `[bi/code: ]` | new BigInteger(404) |
| `404` | `[xi/code: ]` | 1028 (4 * 16 * 16 + 0 * 16 + 4) |
| `404` | `[xbi/code: ]` | BigInteger.Parse("1028") |
| `404` | `[i/code: ]` | 404 |
| `This is a text` | `[$s/body$]` | "This is a text" |
| `  text  ` | `[$|body|$]` | "text" |
| `  404   ` | `[$i/|body|$]` | 404 |

| Symbol | Name | Type | Operation |
| --- | --- | --- | --- |
| `i/` | Integer | int | Parse |
| `b/` | Byte | byte | Parse |
| `x/` | Hexadecimal | int | Parse |
| `s/` | String | string | Encode |
| `ba/` | Byte array | byte[] | Encode or nothing |
| `bi/` | BigInteger | BigInteger | Parse or initialize |
| `sm/` | Stream | MemoryStream | Wrap or encode + wrap |
| `xbi/` | Hexadecimal BigInteger | BigInteger | Parse |

#### <a name="usages-drex-results"></a>Results

```csharp
string version = (string)root["version"];
string version = root.GetString("version");
string version = root.Get<string>("version");

int code = (int)root["code"];
int code = root.Get<int>("code");

string value = root.Get<string>("headers.<0>.value");
string value = root.Get<string>("headers.<name=Header1>.value");
string value = root.Get<string>("headers.<|name=header1|>.value");

List<string> values = root.GetAll<string>("headers.<$name=Set-Cookie$>.value");
List<string> values = root.GetAll<string>("headers.<|$name=set-cookie$|>.value");

List<string> values2 = root.GetAll<string>("headers.<name=Header1>.value");
List<string> values2 = root.GetAll<string>("headers.<$name=Header1$>.value");
List<string> values2 = root.GetAll<string>("headers.<|name=Header1|>.value");
List<string> values2 = root.GetAll<string>("headers.<|$name=Header1$|>.value");

List<string> allValues = root.GetAll<string>("headers.<*>.value");
```

## <a name="tasks"></a>Tasks

- Parsers
  - [X] Add XML permissive parsing
- Parsers.Generic
  - [X] ByteSequencer : Create a better result parsing for `or` (`{...|...}`)
  - [X] ByteSequencer : Add exclude pattern
  - [ ] Clean the files
  - [ ] Test in real situations
- Security
  - [ ] Add RSA
  - [ ] Add anti-forcing system (slow generation - fast use)
- Net
  - [ ] Add web client
  - [ ] Add web server
  - [ ] Add webdav server
  - [X] Add ghost mail system
  - [ ] Add web directory browser
  - [ ] Add light web server for software interface
  - [ ] Add light webdav server for software interface

## <a name="license"></a>License

![GNU AGPLv3](https://www.gnu.org/graphics/agplv3-155x51.png)

[[Can-Cannot-Must description]](https://www.tldrlegal.com/l/agpl3)
[[Learn more]](http://www.gnu.org/licenses/agpl-3.0.html)
