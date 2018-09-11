#!/bin/bash
function dotnetfw { if test "$OS" = "Windows_NT"; then $@; else mono $@; fi }
dotnetfw paket.exe $@
