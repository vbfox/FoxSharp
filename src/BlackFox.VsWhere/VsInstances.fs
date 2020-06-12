module BlackFox.VsWhere.VsInstances

open System
open System.Diagnostics
open System.Runtime.InteropServices
open Microsoft.Win32

let inline private emptySeqIfNull (s: _ seq) =
    if isNull s then
        Seq.empty
    else
        s

let private setupConfiguration = lazy(
    let configType = System.Type.GetTypeFromCLSID (System.Guid "177F0C4A-1CD3-4DE7-A32C-71DBBB9FA36D")
    Activator.CreateInstance configType :?> ISetupConfiguration
)

let private enumAllInstances () =
    let instancesEnumerator =
        let v1 = setupConfiguration.Value
        match v1 with
        | :? ISetupConfiguration2 as v2 -> v2.EnumAllInstances()
        | _ -> v1.EnumInstances()
    let instances = Array.zeroCreate<ISetupInstance> 1
    let fetched = ref 1
    seq {
        while !fetched = 1 do
            instancesEnumerator.Next(1, instances, fetched)
            if !fetched = 1 then
                yield instances.[0]
    }

let private parseErrorInfo (error: ISetupErrorInfo) =
    if isNull error then
        None
    else
        {
            HResult = error.GetErrorHResult()
            ErrorClassName = error.GetErrorClassName()
            ErrorMessage = error.GetErrorMessage()
        }
        |> Some

let private parsePackageReference (instance: ISetupPackageReference) =
    {
        Id = instance.GetId()
        Version = instance.GetVersion()
        Chip = instance.GetChip()
        Language = instance.GetLanguage()
        Branch = instance.GetBranch()
        Type = instance.GetType()
        UniqueId = instance.GetUniqueId()
        IsExtension = instance.GetIsExtension()
    }

let private parseErrorState (state: ISetupErrorState) =
    let result =
        {
            FailedPackages = state.GetFailedPackages() |> emptySeqIfNull |> Seq.map parsePackageReference |> List.ofSeq
            SkippedPackages = state.GetSkippedPackages() |> emptySeqIfNull |> Seq.map parsePackageReference |> List.ofSeq
            ErrorLogFilePath = None
            LogFilePath = None
            RuntimeError = None
        }

    match state with
    | :? ISetupErrorState2 as state2 ->
        let result2 =
            { result with
                ErrorLogFilePath = state2.GetErrorLogFilePath() |> Option.ofObj
                LogFilePath = state2.GetLogFilePath() |> Option.ofObj
            }
        match state2 with
        | :? ISetupErrorState3 as state3 ->
            { result2 with
                RuntimeError = state3.GetRuntimeError() |> parseErrorInfo
            }
        | _-> result2
    | _ -> result

let private parseDate (date: System.Runtime.InteropServices.ComTypes.FILETIME) =
    let high = uint64 (uint32 date.dwHighDateTime)
    let low = uint64 (uint32 date.dwLowDateTime)
    let composed = (high <<< 32) ||| low
    DateTimeOffset.FromFileTime(int64 composed)

let private parseProperties (store: ISetupPropertyStore) =
    if isNull store then
        Map.empty
    else
        store.GetNames()
        |> emptySeqIfNull
        |> Seq.map(fun name ->
            let value = store.GetValue(name)
            name, value)
        |> Map.ofSeq

let private parseInstance (instance: ISetupInstance) =
    let mutable result =
      { InstanceId = instance.GetInstanceId()
        InstallDate = parseDate (instance.GetInstallDate())
        InstallationName = instance.GetInstallationName()
        InstallationPath = instance.GetInstallationPath()
        InstallationVersion = instance.GetInstallationVersion()
        DisplayName = instance.GetDisplayName(0)
        Description = instance.GetDescription(0)
        State = None
        Packages = List.empty
        Product = None
        ProductPath = None
        Errors = None
        IsLaunchable = None
        IsComplete = None
        Properties = Map.empty
        EnginePath = None
        IsPrerelease = None
        CatalogInfo = Map.empty }

    match instance with
    | :? ISetupInstanceCatalog as catalog ->
        result <- { result with
                      IsPrerelease = catalog.IsPrerelease() |> Some
                      CatalogInfo = catalog.GetCatalogInfo() |> parseProperties }
    | _ -> ()

    match instance with
    | :? ISetupInstance2 as v2 ->
        { result with
            State = v2.GetState() |> Some
            Packages = v2.GetPackages() |> emptySeqIfNull |> Seq.map parsePackageReference |> List.ofSeq
            Product = parsePackageReference (v2.GetProduct()) |> Some
            ProductPath = v2.GetProductPath() |> Some
            Errors = v2.GetErrors() |> Option.ofObj |> Option.map parseErrorState
            IsLaunchable = v2.IsLaunchable() |> Some
            IsComplete = v2.IsComplete() |> Some
            Properties = parseProperties (v2.GetProperties())
            EnginePath = v2.GetEnginePath() |> Some }
    | _ -> result

let private parseInstanceOrNone (instance: ISetupInstance) =
    try
        parseInstance instance
        |> Some
    with
    | exn ->
        Trace.TraceError("Failed to parse Visual Studio ISetupInstance: {0}", exn)
        None

module private Legacy =
    open System.IO

    let private legacyVsNames = Map.ofArray [|
        ("7.0", "Visual Studio .NET 2002")
        ("7.1", "Visual Studio .NET 2003")
        ("8.0", "Visual Studio 2005")
        ("9.0", "Visual Studio 2008")
        ("10.0", "Visual Studio 2010")
        ("11.0", "Visual Studio 2012")
        ("12.0", "Visual Studio 2013")
        ("14.0", "Visual Studio 2015")
    |]

    let private legacyProductPath = "Common7\\IDE\\devenv.exe"

    let getAll() =
        if Environment.OSVersion.Platform <> PlatformID.Win32NT then
            List.empty
        else
            try
                use hklm32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
                use vs7Root = hklm32.OpenSubKey("SOFTWARE\\Microsoft\\VisualStudio\\SxS\\VS7")
                let detectedInstance =
                    match vs7Root with
                    | null ->
                        Seq.empty
                    | _ ->
                        vs7Root.GetValueNames()
                        |> Seq.where (fun valueName -> Map.containsKey valueName legacyVsNames)
                detectedInstance
                |> Seq.choose (fun (version) ->
                    try
                        let installationPath = vs7Root.GetValue(version) |> string
                        let productPathFull = Path.Combine(installationPath, legacyProductPath)
                        let installDate =
                            try
                                DateTimeOffset(Directory.GetCreationTimeUtc(installationPath))
                            with
                            _ -> DateTimeOffset.MinValue
                        let productPathExists =
                            try
                                File.Exists productPathFull
                            with
                            _ -> false
                        let fullVersion =
                            if productPathExists then
                                try
                                    defaultArg
                                        (FileVersionInfo.GetVersionInfo(productPathFull).ProductVersion |> Option.ofObj)
                                        version
                                with
                                _ -> version
                            else
                                version

                        {
                            InstanceId = "VisualStudio." + version
                            InstallDate = installDate
                            InstallationName = "VisualStudio/" + fullVersion
                            InstallationPath = installationPath
                            InstallationVersion = fullVersion
                            DisplayName = legacyVsNames |> Map.find version
                            Description = ""
                            State = None
                            Packages = List.empty
                            Product = None
                            ProductPath = if productPathExists then Some legacyProductPath else None
                            Errors = None
                            IsLaunchable = None
                            IsComplete = None
                            Properties = Map.empty
                            EnginePath = None
                            IsPrerelease = None
                            CatalogInfo = Map.empty
                        }
                        |> Some
                    with
                    | exn ->
                        Trace.TraceError("Failed to parse legacy Visual Studio: {0}", exn)
                        None)
                |> List.ofSeq
            with
            | _ -> List.empty

/// <summary>
/// Get legacy VS instances (before VS2017: VS .NET 2002 to VS2015).
/// <para>Note that the information for legacy ones is limited.</para>
/// </summary>
[<CompiledName("GetLegacy")>]
let getLegacy(): VsSetupInstance list =
    Legacy.getAll()

/// <summary>
/// Get all VS2017+ instances (Visual Studio stable, preview, Build tools, ...)
/// <para>This method return instances that have installation errors and pre-releases.</para>
/// </summary>
[<CompiledName("GetAll")>]
let getAll (): VsSetupInstance list =
    if Environment.OSVersion.Platform <> PlatformID.Win32NT then
        // No Visual Studio outside of windows
        List.empty
    else
        try
            enumAllInstances ()
            |> Seq.choose parseInstanceOrNone
            |> List.ofSeq
        with
        | :? COMException ->
            List.empty

/// <summary>
/// Get all Visual Studio instances including legacy VS instances (before VS2017: VS .NET 2002 to VS2015).
/// <para>Note that the information for legacy ones is limited.</para>
/// </summary>
[<CompiledName("GetAllWithLegacy")>]
let getAllWithLegacy (): VsSetupInstance list =
    getAll() @ getLegacy()

/// Get VS2017+ instances that are completely installed
[<CompiledName("GetCompleted")>]
let getCompleted (includePrerelease: bool): VsSetupInstance list =
    getAll ()
    |> List.filter (fun vs ->
        (vs.IsComplete = None || vs.IsComplete = Some true)
        && (includePrerelease || vs.IsPrerelease <> Some true)
    )

/// Get VS2017+ instances that are completely installed and have a specific package ID installed
[<CompiledName("GetWithPackage")>]
let getWithPackage (packageId: string) (includePrerelease: bool): VsSetupInstance list =
    getAll ()
    |> List.filter (fun vs ->
        (vs.IsComplete = None || vs.IsComplete = Some true)
        && (includePrerelease || vs.IsPrerelease <> Some true)
        && vs.Packages |> List.exists (fun p -> p.Id = packageId)
    )
