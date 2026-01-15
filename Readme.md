# LOCAT 
### (Liam's Obsessive Code Analysis Tool) 
Essentially just an OCD analyzer to catch things I do, that I find annoying or want to ensure I avoid doing. 

Don't expect too many useful analyzers.

<p align="center">
<img src="/icon1024.png" alt="Simple Icons" width=256>
</p>

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

Based on suggestions from [Microsoft](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/using-standard-exception-types)

# How To?
### How to debug?
- Use the [launchSettings.json](Properties/launchSettings.json) profile.
- Debug tests.

### Learn more about wiring analyzers
The complete set of information is available at [roslyn github repo wiki](https://github.com/dotnet/roslyn/blob/main/docs/wiki/README.md).
