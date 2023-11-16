
// https://github.com/mono/mono/blob/main/mcs/class/Mono.Posix/Mono.Unix.Native/Syscall.cs
open Mono.Unix.Native
  type Fp = FilePermissions

open showperms

// Show permissions for all files and directories within the given paths.
// If a path does not exist it gets “Error”.
// If a dir is not readable, it gets “!”.
// If a dir is not searchable, items in it are shown with no d, permissions, or trailing slash.
//
// Z% showperms notFound testDir
//       Error: notFound – No such file or directory
// drwxr-xr-x   testDir/
// drwxr-xr-x   testDir/1d/
// drwxr-xr-x   testDir/2d/
// -rw-r--r--   testDir/2d/1f
// drwxr-xr-x   testDir/2d/2d/
// -rwxr-xr-x   testDir/2d/3f
// ----------   testDir/2d/4f
// dr--------   testDir/3d/
//              testDir/3d/1d
//              testDir/3d/2f
// d--------- ! testDir/4d/


//–––––––––––––––––––––––

let mask  stat bits = (stat: Stat).st_mode &&& bits
let isSet stat bit  = mask stat bit = bit
let pick  stat tCase fCase bit = if isSet stat bit then tCase else fCase 

// "drwsr-xr-x"
let statToInfoString stat  : string =
  let pick (tCase, fCase) bit = pick stat tCase fCase bit
  let t =
    match mask stat Fp.S_IFMT with
    | Fp.S_IFDIR  -> "d"
    | Fp.S_IFCHR  -> "c"
    | Fp.S_IFBLK  -> "b"
    | Fp.S_IFREG  -> "-"
    | Fp.S_IFLNK  -> "l"
    | Fp.S_IFSOCK -> "s"
    | Fp.S_IFIFO  -> "p"
    | _           -> "?"
  let r bit = bit        |> pick ("r", "-")
  let w bit = bit        |> pick ("w", "-")
  let xUsr  = Fp.S_IXUSR |> pick (pick (("s", "S"), ("x", "-")) Fp.S_ISUID)
  let xGrp  = Fp.S_IXGRP |> pick (pick (("s", "S"), ("x", "-")) Fp.S_ISGID)
  let xOth  = Fp.S_IXOTH |> pick (pick (("t", "T"), ("x", "-")) Fp.S_ISVTX)
  t  +  r Fp.S_IRUSR  +  w Fp.S_IWUSR  +  xUsr
     +  r Fp.S_IRGRP  +  w Fp.S_IWGRP  +  xGrp
     +  r Fp.S_IROTH  +  w Fp.S_IWOTH  +  xOth

//–––––––––––––––––––––––

let (|IsDir|IsFile|) stat = pick stat IsDir IsFile Fp.S_IFDIR

let (|ReadAndSearch|ReadOnly|SearchOnly|NoAccess|) dirPath =
  let (|Read  |NoRead  |) dirPath = if Syscall.access(dirPath, AccessModes.R_OK) = 0 then Read   else NoRead
  let (|Search|NoSearch|) dirPath = if Syscall.access(dirPath, AccessModes.X_OK) = 0 then Search else NoSearch
  match dirPath, dirPath with
  | Read  , Search   ->  ReadAndSearch
  | Read  , NoSearch ->  ReadOnly
  | NoRead, Search   ->  SearchOnly
  | NoRead, NoSearch ->  NoAccess

type Info =
| NotFound of string * Errno // not found (used only for command line args)
| NameOnly of string         // path was an entry in an unsearchable directory
| File     of string * Stat  // file
| Dir      of string * Stat  // directory
| DirRO    of string * Stat  // directory, readable only; contents have names only
| DirNA    of string * Stat  // directory, contents not accessible

let entriesOf dirPath =
  let comparator a b = System.NaturalStringComparer().Compare(a, b)
  System.IO.Directory.GetFileSystemEntries dirPath
  |> Array.sortWith comparator

/// Yield an Info for each file or directory.
let rec explore path  : Info seq = seq {
  match Syscall.stat path with
  | x, _  when x < 0 ->  yield NotFound (path, Stdlib.GetLastError())
  | _ , stat ->
  match stat with
  | IsFile           ->  yield File     (path, stat)
  | IsDir ->
  match path with
  | ReadAndSearch    ->  yield Dir      (path, stat) ; for path in (entriesOf path) do  yield! explore  path
  | ReadOnly         ->  yield DirRO    (path, stat) ; for path in (entriesOf path) do  yield  NameOnly path
  | SearchOnly
  | NoAccess         ->  yield DirNA    (path, stat) }

let render info  : string =
  match info with
  | NotFound (path    , errno) ->  $"      Error: %s{path} – %s{Syscall.strerror errno}"
  | NameOnly  path             ->  $"             %s{path}"
  | File     (filePath, stat ) ->  $"%s{statToInfoString stat}   %s{filePath}"
  | Dir      (dirPath , stat )
  | DirRO    (dirPath , stat ) ->  $"%s{statToInfoString stat}   %s{dirPath}/" 
  | DirNA    (dirPath , stat ) ->  $"%s{statToInfoString stat} ! %s{dirPath}/" 

/// From argv make a sequence of strings, one for each file and directory within the given paths.
let renderFiles argv  : string seq =
  argv
  |> Seq.collect explore
  |> Seq.map render

//–––––––––––––––––––––––

let pathsToString argv =
  argv
  |> renderFiles
  |> String.concat "\n"

let diffFiles runDiff argv =
  argv
  |> pathsToString
  |> runDiff ExpectedOutput.expectedOutput

let diffPathsToString  argv = argv |> diffFiles DiffResults.Diff.runToStringNotIgnoring 
let diffPathsToConsole argv = argv |> diffFiles DiffResults.Diff.runToConsoleNotIgnoring

[<EntryPoint>]
let main argv =
  let exeName = System.AppDomain.CurrentDomain.FriendlyName
  let testFilenames = [| "notFound" ; "testDir" |]
  match argv with
  | a when a.Length = 0 || a[0] = "--help"  ->  printfn $"Usage: %s{exeName} file ..."
  | a when                 a[0] = "--test"  ->  testFilenames |> diffPathsToString         
  | a when                 a[0] = "--testC" ->  testFilenames |> diffPathsToConsole
  | argv                                    ->  pathsToString argv |> printfn "%s"
  0
