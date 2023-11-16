# An F# example

F# projects here:

- [`showperms`](https://github.com/daveyostcom/showperms/blob/main/showperms/Program.fs) – a simple CLI utility, using F# for maximum readabiilty
- [`showpermsOld`](https://github.com/daveyostcom/showperms/blob/main/showpermsOld/Program.fs) – more complicated and irregular (but it’s shorter! and less readable!)
- [`DiffResults`](https://github.com/daveyostcom/showperms/blob/main/DiffResults/Library.fs) – test results rendered as a diff

## showperms
#### CLI command to show permissions for all files and directories within the given paths.

Usage looks like this:

```bash
Z% showperms notFound testDir
      Error: notFound – No such file or directory
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

A shebang form is provided: [`showperms.fsx`](https://github.com/daveyostcom/showperms/blob/main/showperms/showperms.fsx) 

Rider IDE files included.

Run `createTestData.bash` to create a `testDir/` folder hierarchy.

Depends on NuGet packages:
- [Mono.Posix.NETStandard](https://www.nuget.org/packages/Mono.Posix.NETStandard)
- [NaturalStringExtensions](https://www.nuget.org/packages/NaturalStringExtensions)

I have tried publishing a self-containd binary for osx-x64.  It runs but not if copied to /usr/local/bin/. See [stackoverflow](https://stackoverflow.com/questions/77492308/in-net-8-trying-to-build-a-cli-program-with-dotnet-publish).



## showpermsOld

Earlier version, not taking full advantage of F#.  Shorter but more complicated and irregular.



## DiffResults

#### See test failures all at once, with a diff, instead of stopping at the first error.

Instead of an assert for each fact, where you see only the first error, using the DiffResults library, you see a line for each fact, shown as a diff of expected vs actual, like the first column of this table:

```text
 diff      expected                actual
––––––     ––––––––                –––––– 
x not        what                    not
  what       where                   what
√ where      why                     but
x but        when                    why          
  why        [newline after when]    [no newline after why]
√ when      
√ 
```
Key:<br>
&nbsp;&nbsp; x = wrong: found in expected; is not in actual<br>
&nbsp;&nbsp; √ = right: not found in expected; is in actual

Depends on the [`DiffPlex`](https://www.nuget.org/packages/DiffPlex) NuGet package
