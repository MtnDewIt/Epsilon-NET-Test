### ROADMAP

✔ TAB and SHIFT+TAB to exit / re-enter the tag tree filter

✔ visible highlighting for arrow key navigation of tag tree window

✔ ENTER to open highlighted tag in tag tree

✔ allow copying blocks between open tags without having to close and reopen them

✔ separate apply and save button actions on options page

✔ added startup mod cache option	

✔ ~~disable keyboard navigation to the info window~~
✔ completely remove info window
✔ copy active Cache / Mod Info to clipboard with CTRL+I

✔ add Poke functionality from Epsilon-Poke application version

🔜 add Revert \ Undo Poke functionality

🔜 script window text editing (general improvements, shouldn't be garbage)

🔜 script window save button should also save the tag

🔜 script window compile errors should not be garbage

🔜 intelligent handling for multiple tabs  
(note to self: multiple edittag context for same tag)  
* if tag is already open, switch focus to it
* how can we know with certainty that a tag we're attempting to open is equal to one that's already open?
* add an option to open a "reference" tag - a snapshot / read-only version of the tag

🔜 show editing context stack per tag  
* or cache? idk exactly how the context works  
* hover a tag or a cache entry to see the stack  

🔜 arbitrary tag collections
- favorited / pinned tags
- named groups of tags
- open / close / reload / save / 'backup' all tags in a group
- persistent per-group setting to automatically open a group on startup  
(with optional dependency on a specific cache or mod package)

🔜 intuitive / standardized tab docking and navigation shortcuts

🔜 Migrate Codebase to .NET Core 8.0 and C#12
- This would include replacing the existing TagTool submodule with TagTool-NET-Test