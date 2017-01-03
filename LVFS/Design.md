##Ways to represent sources
* Config file with list
* UID can be path

##Finding files
* Scan each source at runtime
	- allows other applications to modify source
* Have a map from path to actual file
	- faster for each operation, but longer loading time

##Listing files
* Selector could ask each Source what it has in turn
	- Selector lives up to its name.
	- It would have to know the semantics of each Source.
	- Would almost certainly end up ridiculously complicated.
* Could give each Source the output of the previous to do with as it pleases.
	- Allows sources to make a lot of changes, but potentially too many.
	- Needs entire output of lower priority sources if we allow deletions, renaming etc. not just the requested folder.
	- That would probably be very slow, so must be only done once - we cannot modify sources except via the VFS this way.
* Something complicated
	- Pass each Source a pointer to its predecessor
		* It can interrogate it about its contents
		* Potentially allows too much to be known/asked about
			- Could limit this by making the previous source a private member of the abstract class and giving limited interactions with it via protected methods.
		* Probably hits the lower bound on stuff actually required.
		* Sources would have to know their predecessorm which is transitive, so somewhat wipes out the need for Selector.
		* Only viable option designed so far.

##Writes
* COW
	- slow
* Copy chunk on write
* Store deltas only

##Symlinks/shortcuts
* Where these link back into the VFS, they must represent the virtual version of the file.

##Deleting directories
* Must remember that contents should not reappear if replacement directory is made.
	- Could mark all children as deleted on deletion of parent
	- Could mark children as deleted when replacement directory is made.
* This is the responsibility of a Source.

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

#IDokanOperations

##Read
* FindFiles
	- Done
* FindFilesWithPattern
	- Skipped
* GetDiskFreeSpace
	- Done
* GetFileInformation
	- Done
* GetFileSecurity
	- Done
* GetVolumeInformation
	- Done
* ReadFile

##Write
* DeleteDirectory
* DeleteFile
* MoveFile
* SetEndOfFile
* SetFileAttributes
* SetFileSecurity
* SetFileTime
* WriteFile

##Other
* Cleanup
* CloseFile
* CreateFile
	- Done
* LockFile
* Mounted
	- Done
* UnlockFile
* Unmounted
	- Done

##Unknown
* FindStreams
	- Not implemented in Mirror example
* FlushFileBuffers
	- In Mirror is simply
	
	```cs
	((FileStream) (info.Context)).Flush();
	```
* SetAllocationSize
	- in Mirror, calls `FileStream.SetLength`