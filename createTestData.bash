#!/bin/bash

dir=testDir

function log {
  if false ; then echo @ ; ls -ld $( find $dir | grep -v /p ) ; fi 
}

log

# Clean up.
if [ -e $dir ] ; then
  chmod 700 $(find $dir 2> /dev/null)
  log
  chmod 700 $(find $dir 2> /dev/null)
  log
  rm -rf $dir
  log
fi

if [ "$#" -ne 0 ] ; then
  echo "At least one arg, so deleted $dir"
  exit 1
fi

# First group is ad hoc.
umask 022
mkdir -p  $dir/1d
mkdir -p  $dir/2d/2d
touch     $dir/2d/1f
touch     $dir/2d/3f
chmod a+x $dir/2d/3f
touch     $dir/2d/4f
chmod 000 $dir/2d/4f
mkdir -p  $dir/3d/1d
touch     $dir/3d/2f
chmod 0   $dir/3d/1d
chmod 400 $dir/3d
mkdir -p  $dir/4d
touch     $dir/4d/1f
chmod 000 $dir/4d
log

# Second group has combinations of permissions and setuid/setgid/setsticky.
mkdir -p $dir $dir/p
for       s in 0{0,1,2,3,4,5,6,7}400 ; do # 
  for     u in 0{0,1}00              ; do # r-------- r-x------
    for   g in 0{0,1}0               ; do # r-------- r----x---
      for o in 0{0,1}                ; do # r-------- r-------x
        x=$( printf "%04o" $(($s + $u + $g + $o)) )
        touch    $dir/p/${x}f ; chmod $x $dir/p/${x}f
        mkdir -p $dir/p/${x}d ; chmod $x $dir/p/${x}d
      done
    done
  done
done
