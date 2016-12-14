##Ways to represent sources
* Config file with list
* UID can be path

##Finding files
* Scan each source at runtime
	- allows other applications to modify source
* Have a map from path to actual file
	- faster for each operation, but longer loading time

##Writes
* COW
	- slow
* Copy chunk on write
* Store deltas only

##Symlinks/shortcuts
* Where these link back into the VFS, they must represent the virtual version of the file.

#Systems

##Selector
* Initialised with set of sources
* VFS Path goes in
* 'Real path' comes out
	- May need to be a more complex thing if different byte ranges need to come from different places.
	- Can identify which sources have bonus behaviour with overloaded functions specific to interfaces.

##Reader
* Takes stuff from Selector
* Asks sources to read some files
* Returns actual data

##Writer
* Takes stuff from Selector
* Potentially gets Reader to do some stuff
* Asks sources to meddle with actual files.
* Doesn't return anything interesting

##Source
* Abstract Class
* Gives list of files
* Actually reads from/writes to files (or whatever)
* Implementations could be directories, archives etc.
* Extra interfaces specify bonus behaviour.