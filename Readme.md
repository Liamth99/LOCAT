# LOCAT 
### (Liam's Obsessive Code Analysis Tool) 
Essentially just an OCD analyzer to catch things I do, that I find annoying or want to ensure I avoid doing. 

Don't expect too many useful analyzers.

## Rules

### V1.0.0 release
| Rule ID  | Category | Severity | Has Fix | Notes                                                                     |
|----------|----------|----------|:-------:|---------------------------------------------------------------------------|
| LOCAT001 | Design   | Warning  |    ✓    | Classes should have a Debugger display if it is within a model namespace. |
| LOCAT002 | Design   | Error    |         | Debugger display should not be empty.                                     |
| LOCAT003 | Design   | Warning  |         | Debugger display should not be constant.                                  |
| LOCAT004 | Style    | Info     |    ✓    | Use 'is' pattern for constant checks.                                     |
| LOCAT005 | Naming   | Warning  |    ✓    | Async Method names should end with Async.                                 |

### V1.1.0 release
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

Note: added code "fix" to supress the warning in v1.1.1

## How To?
### How to debug?
- Use the [launchSettings.json](Properties/launchSettings.json) profile.
- Debug tests.

### Learn more about wiring analyzers
The complete set of information is available at [roslyn github repo wiki](https://github.com/dotnet/roslyn/blob/main/docs/wiki/README.md).