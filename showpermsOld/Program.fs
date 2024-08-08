
// https://github.com/mono/mono/blob/main/mcs/class/Mono.Posix/Mono.Unix.Native/Syscall.cs
open Mono.Unix.Native
  type Fp = FilePermissions

open showperms

// This older version has some hard-to-understand code.  For example,
// - It uses flags and if-else to select output formatting.
//   - The choices are implicit, buried in code.
//   - The new version chooses what to do by matching against named cases.
// - It intermingles code that explores the file hierarchy with code that renders formatted output.
//   - The new version splits these into two functions, used as steps in an F# pipeline.
// The new version uses concise F# features for naming things:
// - a choice type, a.k.a. discriminated union, with these choices:
//   - NotFound NameOnly File Dir DirRO DirNA
//   - Each of these choices carries useful data with it.
// - two multi-case active pattern functions, which return choices:
//   - IsDir IsFile
//   - ReadAndSearch ReadOnly SearchOnly NoAccess
//   - These choices don’t carry data.
// Using good names for things, of course, makes code more explicit and understandable.
// The improvements in the new version can be made more or less in any language, but F# syntax
// is typically more clean and concise.  The active pattern functions are particularly cool.
//
// I recommend diffing the two versions.

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

let mask  stat bits = bits &&& (stat: Stat).st_mode
let isSet stat bits = bits = mask stat bits
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
  let r    bit = bit |> pick                                  ("r", "-")
  let w    bit = bit |> pick                                  ("w", "-")
  let xUsr bit = bit |> pick (Fp.S_ISUID |> pick (("s", "S"), ("x", "-")))
  let xGrp bit = bit |> pick (Fp.S_ISGID |> pick (("s", "S"), ("x", "-")))
  let xOth bit = bit |> pick (Fp.S_ISVTX |> pick (("t", "T"), ("x", "-")))
  t  +  r Fp.S_IRUSR  +  w Fp.S_IWUSR  +  xUsr Fp.S_IXUSR
     +  r Fp.S_IRGRP  +  w Fp.S_IWGRP  +  xGrp Fp.S_IXGRP
     +  r Fp.S_IROTH  +  w Fp.S_IWOTH  +  xOth Fp.S_IXOTH

//–––––––––––––––––––––––

/// Yield a string for each file or directory.
let rec exploreAndRender path  : string seq = seq {
  match Syscall.stat path with
  | -1, _    -> yield  $"      Error: %s{path} – %s{Syscall.strerror (Stdlib.GetLastError())}" 
  | _ , stat ->
  let isDir = isSet stat Fp.S_IFDIR
  let slash = if isDir then "/" else ""
  let isReadable = Syscall.access(path, AccessModes.R_OK) = 0
  let entriesOf dirPath  : string array =
    let comparator a b = System.NaturalStringComparer().Compare(a, b)
    System.IO.Directory.GetFileSystemEntries dirPath
    |> Array.sortWith comparator
  let noAccessChar = if not isDir || isReadable then " " else "!"
  yield $"{statToInfoString stat} {noAccessChar} {path}{slash}"
  if isDir && isReadable then 
    let searchable = Syscall.access(path, AccessModes.X_OK) = 0
    let entries = entriesOf path
    if searchable then  for path in entries do yield! exploreAndRender path
                  else  for path in entries do yield $"             %s{path}" }

//–––––––––––––––––––––––

/// From argv make a string containing a line for each file and directory within the given paths.
let explorePaths argv  : string =
  argv
  |> Seq.collect exploreAndRender
  |> String.concat "\n"

let diffFiles runDiff argv  : unit =
  argv
  |> explorePaths
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
  | argv                                    ->  explorePaths argv |> printfn "%s"
  0
