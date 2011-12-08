Messaging Demo
==============

This demo shows how a .NET server (or any other TCP server) can provide
realtime messaging to a HTML client. To do so, the .NET server starts a small
node.js server which proxies messages to and from the web browser using 
[socket.io](http://socket.io).


Building
========

In Visual Studio 2010, open messaging-demo.sln and build the solution 
(Build -> Build solution).

From the command line, run MSBuild:

    > msbuild messaging-demo.sln /p:Configuration=Release


Running from Visual Studio
==========================

In Visual Studio 2010, there is some initial setup, as these type of settings
are stored in the project's .suo file.

1. Set both the ``Server`` project and the ``Web``
   website as startup projects:

    1. right-click ``messaging-demo.sln`` in Project Explorer
    2. select "Set StartUp Projects..."
    3. select "Multiple startup projects"
    4. set both project's Action to be "Start"

2. Start the browser when ``Web`` starts instead of running the 
   ASP.NET server:
   
    1. right-click on the ``Web`` project
    2. select "Property Pages"
    3. select "Start Options"
    4. set the "Start URL" to [http://localhost:9876](http://localhost:9876)

Then starting the solution will launch the application.


Or, running from the command line
=================================

From the command line, run Server.exe:

    > Server\bin\Release\Server.exe

And either in a new command prompt:

    > start http://localhost:9876

Or just [navigate there](http://localhost:9876) in an open browser of choice.
This will launch the demo.


Developing
==========

A majority of the server is .NET-based, so use your usual .NET development
tools (aka. Visual Studio 2010 or VIM+command-line for the brave).

The client and web-server ``Web`` can either be developed using
a text-editor and a command-line, as the server is [node.js](http://nodejs.org/)
based, and the client is only HTML, or Visual Studio.

The client uses [CoffeeScript](http://jashkenas.github.com/coffee-script/),
which most editors don't support out of the box, though you can download
plugins to provide support. Here are a few:

* [Visual Studio](http://visualstudiogallery.msdn.microsoft.com/2b96d16a-c986-4501-8f97-8008f9db141a)
* [TextMate](https://github.com/jashkenas/coffee-script-tmbundle)
* [VIM](http://www.vim.org/scripts/script.php?script_id=3590)
