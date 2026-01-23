# LOCAT 
![NuGet Version](https://img.shields.io/nuget/v/LOCAT.svg)
![NuGet Downloads](https://img.shields.io/nuget/dt/LOCAT.svg)
![GitHub Release](https://img.shields.io/github/v/release/Liamth99/LOCAT)
![License](https://img.shields.io/github/license/Liamth99/LOCAT)

### (Liam's Obsessive Code Analysis Tool) 
Essentially just an OCD analyzer to catch things I do, that I find annoying or want to ensure I avoid doing. 

<p align="center">
<img src="/icon1024.png" alt="Simple Icons" width=256>
</p>

If a rule annoys you, disable it by adding the following to your `.editorconfig`

``` ini
[*.cs]
dotnet_diagnostic.<rule-ID>.severity = none
```

## Other C# Analyzer Projects Worth Considering
LOCAT is opinionated and incomplete. If you’re looking for broad analyzers, these projects are far better options:

- [Meziantou.Analyzer](https://github.com/meziantou/Meziantou.Analyzer)
- [StyleCop.Analyzers](https://github.com/DotNetAnalyzers/StyleCopAnalyzers)
- [Roslynator](https://github.com/dotnet/roslynator)
- [Sonar Analyzer for C# (SonarAnalyzer.CSharp)](https://github.com/SonarSource/sonar-dotnet)


# Installation

LOCAT is distributed as a [NuGet package](https://www.nuget.org/packages/LOCAT/).

## Via .NET CLI
```bash
dotnet add package LOCAT
```

## Via Visual Studio

1. Right-click your project → Manage NuGet Packages

2. Search for LOCAT

3. Install the latest version

Once installed, the analyzers will run automatically during builds and in the editor.
No additional setup is required unless a rule supports configuration via `.editorconfig`.

# Rules

## V1.0.0 release
| Rule ID  | Category | Severity | Has Fix | Notes                                                                     |
|----------|----------|----------|:-------:|---------------------------------------------------------------------------|
| LOCAT001 | Design   | Warning  |    ✓    | Classes should have a Debugger display if it is within a model namespace. |
| LOCAT002 | Design   | Error    |         | Debugger display should not be empty.                                     |
| LOCAT003 | Design   | Warning  |         | Debugger display should not be constant.                                  |
| LOCAT004 | Style    | Info     |    ✓    | Use 'is' pattern for constant checks.                                     |
| LOCAT005 | Naming   | Warning  |    ✓    | Async Method names should end with Async.                                 |

## V1.1.0 release
| Rule ID  | Category | Severity | Has Fix | Notes                                                                      |
|----------|----------|----------|:-------:|----------------------------------------------------------------------------|
| LOCAT006 | Design   | Warning  |    ✓    | TODO Comments should be fixed, or be added as issues on the projects repo. |

Will also trigger on comments with fixme, bug or temp. Comments can be ignored by marking them with a tilde:
```csharp
    /* TODO: fix (caught by the analyzer) */
    // TODO: fix (caught by the analyzer)
    
    /*~ TODO: fix (ignored) */
    //~ TODO: fix (ignored)
```

Note: added code "fix" to suppress the warning in v1.1.1

Note: added the ability to configure the regex used to capture comments in v1.2.0 below is the default

``` ini
[*.cs]
dotnet_diagnostic.LOCAT006.todo_regex = \b(todo|fixme|bug|temp)\b
```

## V1.2.0 release
| Rule ID  | Category | Severity | Has Fix | Notes                                                                |
|----------|----------|----------|:-------:|----------------------------------------------------------------------|
| LOCAT007 | Usage    | Warning  |         | Possible incorrect use of null-conditional operator.                 |
| LOCAT008 | Naming   | Warning  |         | Variable name is not allowed according to the class’s naming policy. |

LOCAT008 Uses the .editorconfig to set up class name restrictions. Leaving the class name as default will apply the 
restriction to all classes. The regex is case-sensitive by default, and unfortunately, the class name is insensitive. Below are some (not very useful) examples.

``` ini
[*.cs]
dotnet_diagnostic.LOCAT008.<ClassName> = <regex pattern>

# Only i, j and k for int ¯\(ツ)/¯
dotnet_diagnostic.LOCAT008.Int32 = [ijk]$

# Only lowercase for everything
dotnet_diagnostic.LOCAT008.default = ^[a-z]+$ 

# Restrict temporary variable names (case-insesitivie)
dotnet_diagnostic.LOCAT008.default = (?i)^(temp|tmp)$
```

## V1.3.0 release
| Rule ID  | Category | Severity | Has Fix | Notes                                                       |
|----------|----------|----------|:-------:|-------------------------------------------------------------|
| LOCAT009 | Usage    | Info     |    ✓    | Optional parameters should be passed using named arguments. |

Note: added attribute argument support in v1.3.2

## V1.4.0 release
| Rule ID  | Category | Severity | Has Fix | Notes                                |
|----------|----------|----------|:-------:|--------------------------------------|
| LOCAT010 | Design   | Warning  |         | Do not use reserved exception types. |

Based on suggestions from [Microsoft](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/using-standard-exception-types).

## V1.5.0 release
| Rule ID  | Category    | Severity | Has Fix | Notes                        |
|----------|-------------|----------|:-------:|------------------------------|
| LOCAT011 | Performance | Warning  |         | Regex missing match timeout. |

Works on both Regex objects and attributes, warning is suppressed when using the NonBacktracking option flag.

# Development & Debugging
### How to debug?
- Use the [launchSettings.json](Properties/launchSettings.json) profile.
- The sample project can be used to quickly test rules.
- Debug tests.

### Learning more about analyzer development
The complete set of information is available at [roslyn github repo wiki](https://github.com/dotnet/roslyn/blob/main/docs/wiki/README.md).

# Contributing

Contributions are welcome but not expected.

If you have:
- A suggestion for a new rule
- An improvement to an existing analyzer
- A small, focused PR

feel free to open an issue or pull request.

That said, LOCAT is primarily a personal project, and changes may be accepted, modified, or ignored
depending on whether they align with the project’s goals. All rules must have test cases as well.
