@echo off
Echo This build script builds TypeRefViewer without requiring the
Echo Visual Studio.NET IDE.  Ambitious readers are welcome to
Echo incorporate its components into an IDE Solution

REM First invoke CL to build MetaDataHelper.DLL
cl /O1 /CLR /LDd /W3 /GX MetaDataHelper.cpp MetaDataImportWrapper.cpp ole32.lib

REM Delete files left behind by CL
del *.obj

REM Now invoke CSC to build the C# code, and reference MetaDataHelper.DLL
csc /t:winexe /r:MetaDataHelper.DLL TypeRefViewer.cs TypeRefTreeNode.cs


