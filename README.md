# LVFS

LVFS: a layered virtual filesystem library.

Wraps Dokan.NET allowing easier creation of virtual filesystems which are composed of multiple layers.

## Layers?

In LVFS, a layer represents a transformation taking one VFS and creating another.
An example of a simple layer would be one which added the contents of a directory into the VFS (as implemented in ReadOnlyDirectoryMirror and WritableDirectoryMirror).
Many more ideas, both sensible and not, could be implemented, including:

* Adding the files contained in an archive.
* Appending padding to every file so that the size is a multiple of 1 GiB.
* Hiding every file without a specific string in the name.
* Renaming files whose original names were incompatible with a certain application's IO system, but only when viewed from that application.
* Presenting the same files, but organised into directories based on their last modified date.
* Decompressing any archives and presenting them instead as regular directories.
* Presenting the same files, but if any modification is attempted, copying the affected files to the current layer and having the modification only affect that version (as implemented in SimpleOneWayContentMirror).
  This would provide COW features to the filesystem, protecting existing files, and could be used in place of traditional backups or as a version control system in leiu of Git.
* Adding a single extra file with a random name in a random location to create a weird game of hide and seek.

## Architecture

When initialising LVFS, you will give it a collection of *sources* (objects which form an individual layer - the name should really be *layer*, but it was written before things had been fully planned out).
When LVFS receives an IO call, it is passed to the highest priority source, which is expected to fulfil the call.
To do so, it is able to access the source with next lowest priority as if it were a complete VFS.

For example, to add files to the predecessor source's VFS, when a request to list the files in a directory was received, a source would forward the request to its predecessor, and then add its own contents to the result.

## Usage

To use LVFS, first build the `LVFS` project with Visual Studio (the Mono equivalent is unlikely to work unless Dokan.NET is updated to work under Unix).
The `.dll` you get will need to be bundled with your application.

Next, use the classes in `LVFS/External` to create a class implementing either `Source` or `WritableSource`.
The sample implementations in the LayeredDirectoryMirror project may be helpful, as might the non-layered version in the Dokan.NET examples.
API documentation exists at https://github.com/AnyOldName3/LVFS-Doc/wiki

To run your application, you'll need to have a version of Dokany installed which is compatible with the version of Dokan.NET which Visual Studio automatically downloads from Nuget.

## LayeredDirectoryMirror

To use the LayeredDirectoryMirror example, build it on a machine with a compatible Dokany version installed.
The resulting `.exe` can be called from the command line as follows:

```
LayeredDirectoryMirror.exe <Mount Point> [-w= OR -o=]<Source path 1> [<More source paths with or without prefixes>]
```

where the `-w=` option mounts the source as writeable, the `-o=` option mounts it as a COW layer, preventing writes from affecting the previous layers, and with no option prefix, sources are mounted as read-only.

So, for example, to mount the contents of `C:\FolderA`, `C:\FolderB`, and `C:\FolderC` to `X:\` with all sources being writable, the command would be

```
LayeredDirectoryMirror.exe X: -w=C:\FolderA -w=C:\FolderB -w=C:\FolderC
```

To mount the contents of `C:\FolderA`, `C:\FolderB`, and `C:\FolderC` to `C:\Combined` with write protection, and all changes going to `C:\Overwrite`, the command would be

```
LayeredDirectoryMirror.exe C:\Combined C:\FolderA C:\FolderB C:\FolderC -o=C:\Overwrite
```
