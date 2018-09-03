namespace BlackFox.VsWhere

open System

type VsSetupPackage =
  { Id: string
    Version: string
    Chip: string
    Language: string
    Branch: string
    Type: string
    UniqueId: string
    IsExtension: bool }

type VsSetupErrorInfo =
  { HResult: int
    ErrorClassName: string
    ErrorMessage: string }

type VsSetupErrorState =
  { FailedPackages: VsSetupPackage []
    SkippedPackages: VsSetupPackage []
    ErrorLogFilePath: string option
    LogFilePath: string option
    RuntimeError: VsSetupErrorInfo option }

type VsSetupInstance =
    {
        InstanceId: string
        InstallDate: DateTimeOffset
        InstallationName: string
        InstallationPath: string
        InstallationVersion: string
        DisplayName: string
        Description: string
        State: InstanceState option
        Packages: VsSetupPackage []
        Product: VsSetupPackage option
        ProductPath: string option
        Errors: VsSetupErrorState option
        IsLaunchable: bool option
        IsComplete: bool option
        Properties: Map<string, obj>
        EnginePath: string option
        IsPrerelease: bool option
        CatalogInfo: Map<string, obj>
    }
