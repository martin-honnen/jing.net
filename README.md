#IKVM cross-compiled Java RelaxNG validator jing

This is an IKVM based cross-compilation of the jing 20220510 release to .NET.

If you have the .NET SDK installed, you can use e.g. `dotnet tool install --global --add-source .\Downloads jing.net` to install it and then run
e.g. `jing.net schema.rng file.xml` to validate an XML document `file.xml` against an RelaxNG schema `schema.rng`.

See the jing-trang-original-copying-note.txt for the copyright notes.