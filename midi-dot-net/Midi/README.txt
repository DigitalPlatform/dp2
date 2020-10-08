
midi-dot-net
Copyright (c) 2009 Tom Lokovic

----------------------------------------------------------------------------------------------------

OVERVIEW

This is a .NET library, written in C#, which provides support for MIDI input/output devices.
For details and the latest downloads, see the project page:

                          http://code.google.com/p/midi-dot-net/

----------------------------------------------------------------------------------------------------

INSTALLATION (BINARY DISTRIBUTION)

If you received the binary distribution, you can use it from another Visual Studio project as
follows.  Right-click on your project in the Solution Explorer and choose Add Reference from the
context menu.  Select the Browse tab, then browse to the location of Midi.dll and select that as
the reference.

The binary distribution also includes a copy of the HTML API documentation.  You can browse this
locally with your favorite web browser by starting at docs/index.html.

----------------------------------------------------------------------------------------------------

INSTALLATION (SOURCE DISTRIBUTION)

If you received the source distribution, simply open MidiDotNet.sln in Visual Studio.  The
solution consists of two projects:

       - Midi: The MIDI support library.
       - MidiExamples: An interactive example program.
       
To run the example program, ensure that MidiExamples is the StartUp project by right-clicking on it
in the Solution Explorer, and choose Set as StartUp Project from the context menu.  Then run the
program with Debug > Start Debugging (or press F5).

You can make one of your own projects refer to the Midi library from the source distribution as
follows.  Build MidiDotNet.sln with Build > Build Solution (or press F6).  Locate the resulting
Midi.dll under bin/Debug or bin/Release, depending how you built it.  Then open your project,
right-click on your project in the Solution Explorer, and choose Add Reference from the context
menu.    Select the Browse tab, then browse to the location of Midi.dll and select that as the
reference.

The binary distribution also includes a copy of the HTML API documentation.  You can browse this
locally with your favorite web browser by starting at Midi/docs/index.html.

----------------------------------------------------------------------------------------------------

LICENSE

This library is distributed under the the New BSD License.  Each source file should have a
copy of the license in a comment at the top.  Under the terms of the license, redistribution of the
source code is allowed but requires, among other things, that the license text be retained.  For
details, see that license text or the official site:

                         http://www.opensource.org/licenses/bsd-license.php

----------------------------------------------------------------------------------------------------