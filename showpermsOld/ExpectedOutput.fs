module showperms.ExpectedOutput

open Mono.Unix.Native

let runningAsRoot = Syscall.geteuid() = 0u
if runningAsRoot then printfn "Running as root."

let part1 = """      Error: nonexistentDir – No such file or directory
drwxr-xr-x   testDir/
drwxr-xr-x   testDir/1d/
drwxr-xr-x   testDir/2d/
-rw-r--r--   testDir/2d/1f
drwxr-xr-x   testDir/2d/2d/
-rwxr-xr-x   testDir/2d/3f
----------   testDir/2d/4f
dr--------   testDir/3d/"""

let part2NotRoot = """
             testDir/3d/1d
             testDir/3d/2f
d--------- ! testDir/4d/"""

let part2AsRoot = """
d---------   testDir/3d/1d/
-rw-r--r--   testDir/3d/2f
d---------   testDir/4d/
-rw-r--r--   testDir/4d/1f"""

let part3 = """
drwxr-xr-x   testDir/p/
dr--------   testDir/p/0400d/
-r--------   testDir/p/0400f
dr-------x   testDir/p/0401d/
-r-------x   testDir/p/0401f
dr----x---   testDir/p/0410d/
-r----x---   testDir/p/0410f
dr----x--x   testDir/p/0411d/
-r----x--x   testDir/p/0411f
dr-x------   testDir/p/0500d/
-r-x------   testDir/p/0500f
dr-x-----x   testDir/p/0501d/
-r-x-----x   testDir/p/0501f
dr-x--x---   testDir/p/0510d/
-r-x--x---   testDir/p/0510f
dr-x--x--x   testDir/p/0511d/
-r-x--x--x   testDir/p/0511f
dr-------T   testDir/p/1400d/
-r-------T   testDir/p/1400f
dr-------t   testDir/p/1401d/
-r-------t   testDir/p/1401f
dr----x--T   testDir/p/1410d/
-r----x--T   testDir/p/1410f
dr----x--t   testDir/p/1411d/
-r----x--t   testDir/p/1411f
dr-x-----T   testDir/p/1500d/
-r-x-----T   testDir/p/1500f
dr-x-----t   testDir/p/1501d/
-r-x-----t   testDir/p/1501f
dr-x--x--T   testDir/p/1510d/
-r-x--x--T   testDir/p/1510f
dr-x--x--t   testDir/p/1511d/
-r-x--x--t   testDir/p/1511f
dr----S---   testDir/p/2400d/
-r----S---   testDir/p/2400f
dr----S--x   testDir/p/2401d/
-r----S--x   testDir/p/2401f
dr----s---   testDir/p/2410d/
-r----s---   testDir/p/2410f
dr----s--x   testDir/p/2411d/
-r----s--x   testDir/p/2411f
dr-x--S---   testDir/p/2500d/
-r-x--S---   testDir/p/2500f
dr-x--S--x   testDir/p/2501d/
-r-x--S--x   testDir/p/2501f
dr-x--s---   testDir/p/2510d/
-r-x--s---   testDir/p/2510f
dr-x--s--x   testDir/p/2511d/
-r-x--s--x   testDir/p/2511f
dr----S--T   testDir/p/3400d/
-r----S--T   testDir/p/3400f
dr----S--t   testDir/p/3401d/
-r----S--t   testDir/p/3401f
dr----s--T   testDir/p/3410d/
-r----s--T   testDir/p/3410f
dr----s--t   testDir/p/3411d/
-r----s--t   testDir/p/3411f
dr-x--S--T   testDir/p/3500d/
-r-x--S--T   testDir/p/3500f
dr-x--S--t   testDir/p/3501d/
-r-x--S--t   testDir/p/3501f
dr-x--s--T   testDir/p/3510d/
-r-x--s--T   testDir/p/3510f
dr-x--s--t   testDir/p/3511d/
-r-x--s--t   testDir/p/3511f
dr-S------   testDir/p/4400d/
-r-S------   testDir/p/4400f
dr-S-----x   testDir/p/4401d/
-r-S-----x   testDir/p/4401f
dr-S--x---   testDir/p/4410d/
-r-S--x---   testDir/p/4410f
dr-S--x--x   testDir/p/4411d/
-r-S--x--x   testDir/p/4411f
dr-s------   testDir/p/4500d/
-r-s------   testDir/p/4500f
dr-s-----x   testDir/p/4501d/
-r-s-----x   testDir/p/4501f
dr-s--x---   testDir/p/4510d/
-r-s--x---   testDir/p/4510f
dr-s--x--x   testDir/p/4511d/
-r-s--x--x   testDir/p/4511f
dr-S-----T   testDir/p/5400d/
-r-S-----T   testDir/p/5400f
dr-S-----t   testDir/p/5401d/
-r-S-----t   testDir/p/5401f
dr-S--x--T   testDir/p/5410d/
-r-S--x--T   testDir/p/5410f
dr-S--x--t   testDir/p/5411d/
-r-S--x--t   testDir/p/5411f
dr-s-----T   testDir/p/5500d/
-r-s-----T   testDir/p/5500f
dr-s-----t   testDir/p/5501d/
-r-s-----t   testDir/p/5501f
dr-s--x--T   testDir/p/5510d/
-r-s--x--T   testDir/p/5510f
dr-s--x--t   testDir/p/5511d/
-r-s--x--t   testDir/p/5511f
dr-S--S---   testDir/p/6400d/
-r-S--S---   testDir/p/6400f
dr-S--S--x   testDir/p/6401d/
-r-S--S--x   testDir/p/6401f
dr-S--s---   testDir/p/6410d/
-r-S--s---   testDir/p/6410f
dr-S--s--x   testDir/p/6411d/
-r-S--s--x   testDir/p/6411f
dr-s--S---   testDir/p/6500d/
-r-s--S---   testDir/p/6500f
dr-s--S--x   testDir/p/6501d/
-r-s--S--x   testDir/p/6501f
dr-s--s---   testDir/p/6510d/
-r-s--s---   testDir/p/6510f
dr-s--s--x   testDir/p/6511d/
-r-s--s--x   testDir/p/6511f
dr-S--S--T   testDir/p/7400d/
-r-S--S--T   testDir/p/7400f
dr-S--S--t   testDir/p/7401d/
-r-S--S--t   testDir/p/7401f
dr-S--s--T   testDir/p/7410d/
-r-S--s--T   testDir/p/7410f
dr-S--s--t   testDir/p/7411d/
-r-S--s--t   testDir/p/7411f
dr-s--S--T   testDir/p/7500d/
-r-s--S--T   testDir/p/7500f
dr-s--S--t   testDir/p/7501d/
-r-s--S--t   testDir/p/7501f
dr-s--s--T   testDir/p/7510d/
-r-s--s--T   testDir/p/7510f
dr-s--s--t   testDir/p/7511d/
-r-s--s--t   testDir/p/7511f"""

let expectedOutput = part1 + (if runningAsRoot then part2AsRoot else part2NotRoot) + part3
