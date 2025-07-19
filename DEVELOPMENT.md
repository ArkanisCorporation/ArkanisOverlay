# Development

## Project Setup

- [Build a Windows Presentation Foundation (WPF) Blazor app](https://learn.microsoft.com/en-us/aspnet/core/blazor/hybrid/tutorials/wpf?view=aspnetcore-9.0)
- [Setup Routing](https://learn.microsoft.com/en-us/aspnet/core/blazor/hybrid/routing?view=aspnetcore-9.0&pivots=wpf#get-or-set-a-path-for-initial-navigation)
- [Add MudBlazor](https://mudblazor.com/getting-started/installation#online-playground)
- [Add Microsoft Win32 API C# Source Generator](https://github.com/microsoft/cswin32)
    - [Parent Project: win32metadata](https://github.com/microsoft/win32metadata)
    - [Getting Started](https://microsoft.github.io/CsWin32/docs/getting-started.html)
        - [Workaround for WPF PackageReference and source generators bug](https://github.com/microsoft/CsWin32/issues/7)
    - List desired Native Methods in `NativeMethods.txt`
        - **Exported method name** (e.g. CreateFile). This may include the A or W suffix, where applicable. This may be
          qualified with a namespace but is only recommended in cases of ambiguity, which CsWin32 will prompt where
          appropriate.
        - **A macro name** (e.g. HRESULT_FROM_WIN32). These are generated into the same class with extern methods.
          Macros must be hand-authored into CsWin32, so let us know if you want to see a macro added.
        - **A namespace** to generate all APIs from (e.g. Windows.Win32.Storage.FileSystem would search the metadata for
          all APIs within that namespace and generate them).
        - **Module name followed by `.*`** to generate all methods exported from that module (e.g. `Kernel32.*`).
        - **The name of a struct, enum, constant or interface** to generate. This may be qualified with a namespace but
          is only recommended in cases of ambiguity, which CsWin32 will prompt where appropriate.
        - **A prefix shared by many constants**, followed by `*`, to generate all constants that share that prefix (e.g.
          `ALG_SID_MD*`).
        - **A comment** (i.e. any line starting with `//`) or white space line, which will be ignored.
    - Debug:
        - Check generated files at:
            - `<Project>`
                - `Dependencies`
                    - `.NET <X.Y>`
                        - `Source Generators`
                            - `Microsoft.Windows.CsWin32.SourceGenerator.CsWin32SourceGenerator`
- [Programming reference for the Win32 API](https://learn.microsoft.com/en-us/windows/win32/api/)
- [Setup Dependency Injection](https://medium.com/@shalahuddinshanto/dependency-injection-in-wpf-a-complete-implementation-guide-468abcf95337)
- [Setup Window Blur](https://github.com/riverar/sample-win32-acrylicblur)

## Tools

- [seq](https://datalust.co/docs/getting-started)
- [seqcli](https://datalust.co/docs/command-line-client)
  - Install: `dotnet tool install -g seqcli`
  - Import log file: `seqcli ingest -i <path/to/log.log> --json` (not working for me right now, weird errors)
