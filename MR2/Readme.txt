More information about MonoRail can be found at http://www.castleproject.org/monorail/index.html

Why does git status show that all of my files are modified?
MonoRail is built by Windows users, so all of the text files have CRLF line endings. These line endings are stored as-is in git (which means we all have autocrlf turned off). If you have autocrlf enabled, when you retrieve files from git, it will modify all of your files. Your best bet is to turn off autocrlf, and re-create your clone of MonoRail.

    1. Delete your local clone of the MonoRail repository
    2. Type: git config --global core.autocrlf false
    3. Type: git config --system core.autocrlf false
    4. Clone the MonoRail repository again

