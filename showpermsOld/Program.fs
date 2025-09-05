module Program

// https://github.com/mono/mono/blob/main/mcs/class/Mono.Posix/Mono.Unix.Native/Syscall.cs
open Mono.Unix.Native
  type Fp = FilePermissions

open showperms
open DiffResults

// See comments at end of file for a discussion of the code.

// Show permissions for all files and directories within the given paths.
// If a path does not exist, it gets “Error”.
// If a dir is not readable, it gets “!”.
// If a dir is not searchable, items in it are shown with no info.
//
// Z% showperms nonexistentDir testDir
//       Error: nonexistentDir – No such file or directory
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
let pick  stat bit (tCase, fCase) = if isSet stat bit then tCase else fCase 

// Examples
// 00000 ----------
// 00444 -r--r--r--
// 00222 --w--w--w-
// 00100 ---x------
// 04100 ---s------
// 04000 ---S------
// 00010 ------x---
// 02010 ------s---
// 02000 ------S---
// 00001 ---------x
// 01001 ---------t
// 01000 ---------T
let statToInfoString stat  : string =
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
  let r    bit =              ("r", "-")                          |> pick stat bit 
  let w    bit =              ("w", "-")                          |> pick stat bit 
  let xUsr bit = (("s", "S"), ("x", "-")) |> pick stat Fp.S_ISUID |> pick stat bit 
  let xGrp bit = (("s", "S"), ("x", "-")) |> pick stat Fp.S_ISGID |> pick stat bit 
  let xOth bit = (("t", "T"), ("x", "-")) |> pick stat Fp.S_ISVTX |> pick stat bit 
  System.String.Concat(
    t,
    r Fp.S_IRUSR, w Fp.S_IWUSR, xUsr Fp.S_IXUSR,
    r Fp.S_IRGRP, w Fp.S_IWGRP, xGrp Fp.S_IXGRP,
    r Fp.S_IROTH, w Fp.S_IWOTH, xOth Fp.S_IXOTH)

//–––––––––––––––––––––––

/// Yield a string for every file and directory.
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
  let noAccessChar = if isDir && not isReadable then "!" else " "
  yield $"{statToInfoString stat} {noAccessChar} {path}{slash}"
  if isDir && isReadable then 
    let searchable = Syscall.access(path, AccessModes.X_OK) = 0
    let entries = entriesOf path
    if searchable then  for path in entries do yield! exploreAndRender path
                  else  for path in entries do yield $"             %s{path}" }

//–––––––––––––––––––––––

/// From argv make a sequence with a string for every file and directory within the given paths.
let explorePaths argv  : string seq =
  argv
  |> Seq.collect exploreAndRender

//–––––––––––––––––––––––
// testing

let test diffRunner  : unit =
  [| "nonexistentDir" ; "testDir" |]
  |> explorePaths
  |> String.concat "\n"
  |> diffRunner ExpectedOutput.expectedOutput

let runToString  expected actual = Diff.RunToString (expected, actual, ignoreWhiteSpace = false, mode = DiffMode.LineByLine) |> printf "%s"
let runToConsole expected actual = Diff.RunToConsole(expected, actual, ignoreWhiteSpace = false, mode = DiffMode.LineByLine)

//–––––––––––––––––––––––

[<EntryPoint>]
let main argv =
  let exeName = System.AppDomain.CurrentDomain.FriendlyName
  match argv with
  | a when a.Length = 0 || a[0] = "--help"  ->  printfn $"Usage: %s{exeName} file ..."
  | a when                 a[0] = "--test"  ->  test runToString
  | a when                 a[0] = "--testC" ->  test runToConsole
  | argv                                    ->  argv |> explorePaths |> Seq.iter (printfn "%s")
  0

// This older version has some hard-to-understand code.  For example,
// - It uses flags and if-else to select output formatting.
//   - The choices are implicit, buried in code.
//   - The newer version chooses what to do by matching against named cases.
// - It intermingles code that explores the file hierarchy with code that renders formatted output.
//   - The newer version splits these into two functions, used as steps in an F# pipeline.
// The new version uses concise F# features for naming things:
//   - a choice type, a.k.a. discriminated union, with these choices:
//     - NotFound NameOnly File Dir DirRO DirNA
//     - Each of these choices
//       - carries useful data with it
//       - is a naming opportunity
//       - is a great place for comments that explain and give examples
//   - two multi-case active pattern functions, which return choices:
//     - IsDir IsFile
//     - ReadAndSearch ReadOnly SearchOnly NoAccess
//       made from these multi-case active pattern functions
//       - Read NoRead
//       - Search NoSearch
//     These choices don’t carry data, but they could.
// Using good names for things makes code more explicit and understandable.
// Some of the improvements in this newer version can be made more or less in any language, but F# syntax
// is typically more clean and concise.  F#’s active pattern functions are particularly cool.
//
// I recommend a side-by-side diff of older vs newer.
