namespace BlackFox.VsWhere

open System
open System.Runtime.InteropServices

[<Guid("B41463C3-8866-43B5-BC33-2B0676F7F42E")>]
[<InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>]
[<ComImport>]
[<AllowNullLiteral>]
type private ISetupInstance =
    abstract member GetInstanceId: unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetInstallDate: unit -> [<MarshalAs(UnmanagedType.Struct)>] System.Runtime.InteropServices.ComTypes.FILETIME
    abstract member GetInstallationName : unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetInstallationPath : unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetInstallationVersion : unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetDisplayName : [<MarshalAs(UnmanagedType.U4)>][<In>] lcid: int -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetDescription : [<MarshalAs(UnmanagedType.U4)>][<In>] lcid: int -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member ResolvePath : [<MarshalAs(UnmanagedType.LPWStr)>][<In>] pwszRelativePath: string -> [<MarshalAs(UnmanagedType.BStr)>] string

[<Flags>]
type InstanceState =
    | None = 0u
    | Local = 1u
    | Registered = 2u
    | NoRebootRequired = 4u
    | NoErrors = 8u
    | Complete = 0xFFFFFFFFu

[<Guid("DA8D8A16-B2B6-4487-A2F1-594CCCCD6BF5")>]
[<InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>]
[<ComImport>]
[<AllowNullLiteral>]
type private ISetupPackageReference =
    abstract member GetId : unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetVersion : unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetChip : unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetLanguage : unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetBranch : unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetType : unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetUniqueId : unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetIsExtension : unit -> [<MarshalAs(UnmanagedType.VariantBool)>] bool

[<Guid("E73559CD-7003-4022-B134-27DC650B280F")>]
[<InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>]
[<ComImport>]
[<AllowNullLiteral>]
type private ISetupFailedPackageReference =
    inherit ISetupPackageReference
    abstract member GetId : unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetVersion : unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetChip : unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetLanguage : unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetBranch : unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetType : unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetUniqueId : unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetIsExtension : unit -> [<MarshalAs(UnmanagedType.VariantBool)>] bool

[<Guid("E73559CD-7003-4022-B134-27DC650B280F")>]
[<InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>]
[<ComImport>]
[<AllowNullLiteral>]
type private ISetupFailedPackageReference2 =
    inherit ISetupFailedPackageReference
    abstract member GetId : unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetVersion : unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetChip : unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetLanguage : unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetBranch : unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetType : unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetUniqueId : unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetIsExtension : unit -> [<MarshalAs(UnmanagedType.VariantBool)>] bool
    abstract member GetLogFilePath : unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetDescription : unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetSignature : unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetDetails: unit -> [<MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)>] string[]
    abstract member GetAffectedPackages: unit -> [<MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_UNKNOWN)>] ISetupPackageReference[]

[<Guid("2A2F3292-958E-4905-B36E-013BE84E27AB")>]
[<InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>]
[<ComImport>]
[<AllowNullLiteral>]
type private ISetupErrorInfo =
    abstract member GetErrorHResult: unit -> int
    abstract member GetErrorClassName: unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetErrorMessage: unit -> [<MarshalAs(UnmanagedType.BStr)>] string

[<Guid("46DCCD94-A287-476A-851E-DFBC2FFDBC20")>]
[<InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>]
[<ComImport>]
[<AllowNullLiteral>]
type private ISetupErrorState =
    abstract member GetFailedPackages: unit -> [<MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_UNKNOWN)>] ISetupFailedPackageReference[]
    abstract member GetSkippedPackages: unit -> [<MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_UNKNOWN)>] ISetupPackageReference[]

[<Guid("9871385B-CA69-48F2-BC1F-7A37CBF0B1EF")>]
[<InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>]
[<ComImport>]
[<AllowNullLiteral>]
type private ISetupErrorState2 =
    inherit ISetupErrorState
    abstract member GetFailedPackages: unit -> [<MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_UNKNOWN)>] ISetupFailedPackageReference[]
    abstract member GetSkippedPackages: unit -> [<MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_UNKNOWN)>] ISetupPackageReference[]
    abstract member GetErrorLogFilePath : unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetLogFilePath : unit -> [<MarshalAs(UnmanagedType.BStr)>] string

[<Guid("290019AD-28E2-46D5-9DE5-DA4B6BCF8057")>]
[<InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>]
[<ComImport>]
[<AllowNullLiteral>]
type private ISetupErrorState3 =
    inherit ISetupErrorState2
    abstract member GetFailedPackages: unit -> [<MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_UNKNOWN)>] ISetupFailedPackageReference[]
    abstract member GetSkippedPackages: unit -> [<MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_UNKNOWN)>] ISetupPackageReference[]
    abstract member GetErrorLogFilePath : unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetLogFilePath : unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetRuntimeError : unit -> ISetupErrorInfo

[<Guid("c601c175-a3be-44bc-91f6-4568d230fc83")>]
[<InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>]
[<ComImport>]
[<AllowNullLiteral>]
type private ISetupPropertyStore =
    abstract member GetNames : unit -> [<MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)>] string[]
    abstract member GetValue: [<MarshalAs(UnmanagedType.LPWStr)>][<In>] pwszName: string -> obj

[<Guid("B41463C3-8866-43B5-BC33-2B0676F7F42E")>]
[<InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>]
[<ComImport>]
[<AllowNullLiteral>]
type private ISetupInstance2 =
    inherit ISetupInstance
    abstract member GetInstanceId: unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetInstallDate: unit -> [<MarshalAs(UnmanagedType.Struct)>] System.Runtime.InteropServices.ComTypes.FILETIME
    abstract member GetInstallationName : unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetInstallationPath : unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetInstallationVersion : unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetDisplayName : [<MarshalAs(UnmanagedType.U4)>][<In>] lcid: int -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetDescription : [<MarshalAs(UnmanagedType.U4)>][<In>] lcid: int -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member ResolvePath : [<MarshalAs(UnmanagedType.LPWStr)>][<In>] pwszRelativePath: string -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetState: unit -> [<MarshalAs(UnmanagedType.U4)>] InstanceState
    abstract member GetPackages: unit -> [<MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_UNKNOWN)>] ISetupPackageReference[]
    abstract member GetProduct: unit -> ISetupPackageReference
    abstract member GetProductPath: unit -> [<MarshalAs(UnmanagedType.BStr)>] string
    abstract member GetErrors: unit -> ISetupErrorState
    abstract member IsLaunchable: unit -> [<MarshalAs(UnmanagedType.VariantBool)>] bool
    abstract member IsComplete: unit -> [<MarshalAs(UnmanagedType.VariantBool)>] bool
    abstract member GetProperties: unit -> ISetupPropertyStore
    abstract member GetEnginePath: unit -> [<MarshalAs(UnmanagedType.BStr)>] string

[<Guid("6380BCFF-41D3-4B2E-8B2E-BF8A6810C848")>]
[<InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>]
[<ComImport>]
[<AllowNullLiteral>]
type private IEnumSetupInstances =
    abstract member Next:
        [<MarshalAs(UnmanagedType.U4)>][<In>] celt: int
        * [<MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Interface)>][<Out>] rgelt: ISetupInstance[]
        * [<MarshalAs(UnmanagedType.U4)>][<Out>] pceltFetched: byref<int>
        -> unit
    abstract member Skip: [<MarshalAs(UnmanagedType.U4)>][<In>] celt: int -> unit
    abstract member Reset: unit -> unit
    abstract member Clone: unit -> [<MarshalAs(UnmanagedType.Interface)>] IEnumSetupInstances

[<Guid("42843719-DB4C-46C2-8E7C-64F1816EFD5B")>]
[<InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>]
[<ComImport>]
[<AllowNullLiteral>]
type private ISetupConfiguration =
    abstract member EnumInstances: unit -> [<MarshalAs(UnmanagedType.Interface)>] IEnumSetupInstances
    abstract member GetInstanceForCurrentProcess: unit -> [<MarshalAs(UnmanagedType.Interface)>] ISetupInstance
    abstract member GetInstanceForPath: [<MarshalAs(UnmanagedType.LPWStr)>][<In>] path: string -> [<MarshalAs(UnmanagedType.Interface)>] ISetupInstance

[<Guid("26AAB78C-4A60-49D6-AF3B-3C35BC93365D")>]
[<InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>]
[<ComImport>]
[<AllowNullLiteral>]
type private ISetupConfiguration2 =
    inherit ISetupConfiguration
    abstract member EnumInstances: unit -> [<MarshalAs(UnmanagedType.Interface)>] IEnumSetupInstances
    abstract member GetInstanceForCurrentProcess: unit -> [<MarshalAs(UnmanagedType.Interface)>] ISetupInstance
    abstract member GetInstanceForPath: [<MarshalAs(UnmanagedType.LPWStr)>][<In>] path: string -> [<MarshalAs(UnmanagedType.Interface)>] ISetupInstance
    abstract member EnumAllInstances: unit -> [<MarshalAs(UnmanagedType.Interface)>] IEnumSetupInstances

[<Guid("9AD8E40F-39A2-40F1-BF64-0A6C50DD9EEB")>]
[<InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>]
[<ComImport>]
type private ISetupInstanceCatalog =
    abstract member GetCatalogInfo: unit -> ISetupPropertyStore
    abstract member IsPrerelease: unit -> [<MarshalAs(UnmanagedType.VariantBool)>] bool
