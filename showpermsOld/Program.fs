
open System.IO
open Mono.Unix.Native
  type Fp = FilePermissions

open showperms

// I left this older version here because the explore function is a good example
// of the complicated flags-and-if-else way of doing things.
// It also intermingles the exploration with the output formatting.
// By using some cool F# features, this all becomes cleaner and more declarative
// and thus easier to read and understand.

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

let entriesOf dirPath =
  let comparator a b = System.NaturalStringComparer().Compare(a, b)
  System.IO.Directory.GetFileSystemEntries dirPath
  |> Array.sortWith comparator

/// Yield a string for each file or directory.
let rec explore path  : string seq = seq {
  match Syscall.stat path with
  | -1, _    -> yield  $"      Error: %s{path} – %s{Syscall.strerror (Stdlib.GetLastError())}" 
  | _ , stat ->
  let isDir = isSet stat Fp.S_IFDIR
  let slash = if isDir then "/" else ""
  let isReadable = Syscall.access(path, AccessModes.R_OK) = 0
  let noAccessChar = if not isDir || isReadable then " " else "!"
  yield $"{statToInfoString stat} {noAccessChar} {path}{slash}"
  if isDir && isReadable then 
    let searchable = Syscall.access(path, AccessModes.X_OK) = 0
    let entries = entriesOf path
    if searchable then  for path in entries do yield! explore path
                  else  for path in entries do yield $"             %s{path}" }

/// From argv make a sequence of strings, one for each file and directory within the given paths.
let renderFiles argv  : string seq =
  argv
  |> Seq.collect explore

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
