# showperms – an F# example

Console program to show permissions for all files and directories within given path hierarchies.

Usage looks like this:

```text
Z% showperms nonexistentDir testDir
      Error: nonexistentDir – No such file or directory
drwxr-xr-x   testDir/
drwxr-xr-x   testDir/1d/
drwxr-xr-x   testDir/2d/
-rw-r--r--   testDir/2d/1f
drwxr-xr-x   testDir/2d/2d/
-rwxr-xr-x   testDir/2d/3f
----------   testDir/2d/4f
dr--------   testDir/3d/
             testDir/3d/1d
             testDir/3d/2f
d--------- ! testDir/4d/
```

___
F# projects here:

- [showperms](https://github.com/daveyostcom/showperms/blob/main/showperms/Program.fs) – a simple CLI utility, exploiting F# features for maximum readabiilty
- [showpermsOld](https://github.com/daveyostcom/showperms/blob/main/showpermsOld/Program.fs) – shorter, but more complicated and irregular code, less readability!
- [DiffResults](https://github.com/daveyostcom/showperms/blob/main/DiffResults/Library.fs) – for rendering test results as a diff

Jetbrains Rider IDE files included.

___
## showperms
#### Fully utilizing F# features

A shebang form is provided: [`showperms.fsx`](https://github.com/daveyostcom/showperms/blob/main/showperms/showperms.fsx) 

Before testing, you must run `createTestData.bash` to create a `testDir/` folder hierarchy.

In Rider, there is a Run Configuration that makes an executable for osx-arm64. It’s big, tho.

Depends on NuGet packages:
- [Mono.Posix.NETStandard](https://www.nuget.org/packages/Mono.Posix.NETStandard)
- [NaturalStringExtensions](https://www.nuget.org/packages/NaturalStringExtensions)

___
## showpermsOld
#### Earlier version, not fully utilizing F# features.  It’s shorter but more complicated, irregular, and harder to understand.

Depends on NuGet packages:
- [Mono.Posix.NETStandard](https://www.nuget.org/packages/Mono.Posix.NETStandard)
- [NaturalStringExtensions](https://www.nuget.org/packages/NaturalStringExtensions)


___
## DiffResults library

#### Shows test failures all at once with a diff, instead of aborting at the first error.

Instead of an assert for each fact, where the test aborts at the first error, the test runs to completion and you see a diff of expected output printouts vs actual. Given these two files:

```text
 expected                actual
 ––––––––                –––––– 
   yes                     no
   yes                     no
   what                    not
   where                   what
   why                     but
   when                    why          
   [newline after when]    [no newline after why]
```
Output looks like this:
```text
x no
√ yes
x no
√ yes
x not
√ what
x what 
√ where
x but
  why
√ when
√ 
```
Key:<br>
&nbsp;&nbsp; x = wrong: found in expected; is not in actual<br>
&nbsp;&nbsp; √ = right: not found in expected; is in actual

Depends on NuGet package:
- [DiffPlex](https://www.nuget.org/packages/DiffPlex)
