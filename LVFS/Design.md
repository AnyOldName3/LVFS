#Ways to represent sources
*	Config file with list
*	UID can be path

#Finding files
*	Scan each source at runtime
	- allows other applications to modify source
*	Have a map from path to actual file
	- faster for each operation, but longer loading time

#Writes
*	COW
	- slow
*	Copy chunk on write
*	Store deltas only