#!/bin/sh

mono bin/Prebuild.exe /target nant
mono bin/Prebuild.exe /target vs2005
