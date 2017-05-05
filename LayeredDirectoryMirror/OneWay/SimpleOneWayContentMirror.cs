using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

using DokanNet;

using LVFS.External;

namespace LayeredDirectoryMirror.OneWay
{
	/// <summary>
	/// An LVFS source which allows all operations on its entire contents, but causes no changes to be propagated to predecessor sources. Achieves this by copying files from lower priority sources when they are modified.
	/// </summary>
	public class SimpleOneWayContentMirror : WritableSource
	{
		/// <summary>
		/// The directory being mirrored
		/// </summary>
		public string DirectoryPath { get; private set; }

		/// <summary>
		/// Constructs the class to mirror the specified path, creating it if it does not exist and <paramref name="create"/> is set.
		/// </summary>
		/// <param name="path">The path to mirror</param>
		/// <param name="create">Whether to create the path if it does not exist</param>
		public SimpleOneWayContentMirror(string path, bool create)
		{
			if (create && !Directory.Exists(path))
				Directory.CreateDirectory(path);
			DirectoryPath = path;
		}

		/// <summary>
		/// Constructs the class to mirror the specified path if it exists.
		/// </summary>
		/// <param name="path">The path to mirror</param>
		public SimpleOneWayContentMirror(string path) : this(path, false)
		{
		}

		private string ConvertPath(string path)
		{
			path = EscapePath(path);
			return Path.Combine(DirectoryPath, path);
		}

		private string EscapePath(string path)
		{
			if (path == "" || path == null)
				return "";
			else
				return Path.Combine(EscapePath(Path.GetDirectoryName(path)), EscapeFileName(Path.GetFileName(path)));
		}

		private string EscapeFileName(string filename)
		{
			if (filename.StartsWith(".LVFS.shadow."))
				filename = ".LVFS.escape" + filename;
			else if (filename.StartsWith(".LVFS.escape."))
				filename = ".LVFS.escape" + filename;
			return filename;
		}

		private string UnescapeFileName(string filename)
		{
			if (filename.StartsWith(".LVFS.escape.LVFS.shadow."))
				filename = filename.Substring(".LVFS.escape".Length);
			else if (filename.StartsWith(".LVFS.escape.LVFS.escape."))
				filename = filename.Substring(".LVFS.escape".Length);
			return filename;
		}

		private bool CopyFromPredecessor(string path)
		{
			//Content
			//Security
			//Information
			// - attributes
			// - times
			// - name
			// - length

			try
			{
				var fileInfo = GetPredecessorFileInformation(path).Value;
				var fileSecurity = GetPredecessorFileSystemSecurity(path, AccessControlSections.All);
				var directoryPath = Path.GetDirectoryName(path);
				if (!IsDirectoryVisible(ConvertPath(directoryPath)))
					CopyFromPredecessor(directoryPath);

				var convertedPath = ConvertPath(path);
				// Exit early if the file already exists
				if (IsFileVisible(convertedPath))
					return !fileInfo.Attributes.HasFlag(FileAttributes.Directory);
				else if (IsDirectoryVisible(convertedPath))
					return fileInfo.Attributes.HasFlag(FileAttributes.Directory);

				if (IsFileShadowed(convertedPath))
				{
					RemoveFileShadow(convertedPath);
					if (File.Exists(convertedPath))
						File.Delete(convertedPath);
				}

				if (!fileInfo.Attributes.HasFlag(FileAttributes.Directory))
				{
					using (var stream = File.Create(convertedPath, 4096, FileOptions.SequentialScan, fileSecurity as FileSecurity))
					{
						var currentOffset = 0L;
						var end = fileInfo.Length;
						var buffer = new byte[end < int.MaxValue ? end : int.MaxValue];

						while (currentOffset < end)
						{
							int bytesRead;
							PredecessorReadFile(path, buffer, out bytesRead, currentOffset, null);
							stream.Write(buffer, 0, bytesRead);
							currentOffset += bytesRead;
						}
					}
					File.SetAttributes(convertedPath, fileInfo.Attributes);
					File.SetCreationTime(convertedPath, fileInfo.CreationTime.Value);
					File.SetLastAccessTime(convertedPath, fileInfo.LastAccessTime.Value);
					File.SetLastWriteTime(convertedPath, fileInfo.LastWriteTime.Value);
				}
				else
				{
					if (IsDirectoryShadowed(convertedPath))
						RemoveDirectoryShadow(convertedPath);

					DirectoryInfo dirInfo;
					if (Directory.Exists(convertedPath))
						dirInfo = new DirectoryInfo(convertedPath);
					else
						dirInfo = Directory.CreateDirectory(convertedPath, fileSecurity as DirectorySecurity);
					dirInfo.Attributes = fileInfo.Attributes;
					dirInfo.CreationTime = fileInfo.CreationTime.Value;
					dirInfo.LastAccessTime = fileInfo.LastAccessTime.Value;
					dirInfo.LastWriteTime = fileInfo.LastWriteTime.Value;
				}

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		private bool CopyFromPredecessorExcludingContent(string path)
		{
			//Security
			//Information
			// - attributes
			// - times
			// - name
			// - length

			try
			{
				var fileInfo = GetPredecessorFileInformation(path).Value;
				var fileSecurity = GetPredecessorFileSystemSecurity(path, AccessControlSections.All);
				var directoryPath = Path.GetDirectoryName(path);
				if (!IsDirectoryVisible(ConvertPath(directoryPath)))
					CopyFromPredecessorExcludingContent(directoryPath);

				var convertedPath = ConvertPath(path);
				// Exit early if the file already exists
				if (IsFileVisible(convertedPath))
					return !fileInfo.Attributes.HasFlag(FileAttributes.Directory);
				else if (IsDirectoryVisible(convertedPath))
					return fileInfo.Attributes.HasFlag(FileAttributes.Directory);

				if (IsFileShadowed(convertedPath))
				{
					RemoveFileShadow(convertedPath);
					if (File.Exists(convertedPath))
						File.Delete(convertedPath);
				}

				if (!fileInfo.Attributes.HasFlag(FileAttributes.Directory))
				{
					using (var stream = File.Create(convertedPath, 0, FileOptions.None, fileSecurity as FileSecurity))
					{
						// Do nothing. The file now exists.
					}
					File.SetAttributes(convertedPath, fileInfo.Attributes);
					File.SetCreationTime(convertedPath, fileInfo.CreationTime.Value);
					File.SetLastAccessTime(convertedPath, fileInfo.LastAccessTime.Value);
					File.SetLastWriteTime(convertedPath, fileInfo.LastWriteTime.Value);
				}
				else
				{
					if (IsDirectoryShadowed(convertedPath))
						RemoveDirectoryShadow(convertedPath);

					DirectoryInfo dirInfo;
					if (Directory.Exists(convertedPath))
						dirInfo = new DirectoryInfo(convertedPath);
					else
						dirInfo = Directory.CreateDirectory(convertedPath, fileSecurity as DirectorySecurity);
					dirInfo.Attributes = fileInfo.Attributes;
					dirInfo.CreationTime = fileInfo.CreationTime.Value;
					dirInfo.LastAccessTime = fileInfo.LastAccessTime.Value;
					dirInfo.LastWriteTime = fileInfo.LastWriteTime.Value;
				}

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		private void TransferFileHandle(string path, LVFSContextInfo info)
		{
			var context = info.Context[this] as OneWayContext;
			var result = CreateFileHandle(path, context.CreationAccess, context.CreationShare, context.CreationMode, context.CreationOptions, context.CreationAttributes, info);
			PredecessorCleanupFileHandle(path, info);
			PredecessorCloseFileHandle(path, info);
		}

		private void EnsureModifiable(string path, LVFSContextInfo info)
		{
			var context = info?.Context[this] as OneWayContext ?? null;
			if (context != null)
			{
				if (context.OneWayControls)
					return;
				else
				{
					var convertedPath = ConvertPath(path);
					if (File.Exists(convertedPath) || Directory.Exists(convertedPath))
						TransferFileHandle(path, info);
					else
					{
						CopyFromPredecessor(path);
						TransferFileHandle(path, info);
					}
				}
			}
			else
			{
				var convertedPath = ConvertPath(path);
				if (!(File.Exists(convertedPath) || Directory.Exists(convertedPath)))
					CopyFromPredecessor(path);
			}
		}

		private bool IsDirectoryVisible(string path)
		{
			return !IsDirectoryShadowed(path) && Directory.Exists(path);
		}

		private bool IsFileVisible(string path)
		{
			return !IsFileShadowed(path) && File.Exists(path);
		}

		private bool IsPredecessorDirectoryVisible(string path)
		{
			return !IsDirectoryShadowed(ConvertPath(path)) && PredecessorHasDirectory(path);
		}

		private bool IsPredecessorFileVisible(string path)
		{
			return !IsFileShadowed(ConvertPath(path)) && PredecessorHasRegularFile(path);
		}

		private bool IsDirectoryShadowed(string path)
		{
			if (Path.GetFullPath(path).Equals(Path.GetFullPath(DirectoryPath), StringComparison.OrdinalIgnoreCase))
				return false;
			else
			{
				var shadowPath = Path.Combine(Path.GetDirectoryName(path), ".LVFS.shadow." + Path.GetFileName(path));
				return Directory.Exists(shadowPath) || IsDirectoryShadowed(Path.GetDirectoryName(path));
			}
		}

		private bool IsFileShadowed(string path)
		{
			if (Path.GetFullPath(path).Equals(Path.GetFullPath(DirectoryPath), StringComparison.OrdinalIgnoreCase))
				return false;
			else
			{
				var shadowPath = Path.Combine(Path.GetDirectoryName(path), ".LVFS.shadow." + Path.GetFileName(path));
				return File.Exists(shadowPath) || IsDirectoryShadowed(Path.GetDirectoryName(path));
			}
		}

		private void MoveShadows(string source, string destination)
		{
			var shadows = Directory.EnumerateFiles(source, ".LVFS.shadow.*");
			foreach (var shadow in shadows)
			{
				var name = Path.GetFileName(shadow);
				File.Move(shadow, Path.Combine(destination, name));
			}
			shadows = Directory.EnumerateDirectories(source, ".LVFS.shadow.*");
			foreach (var shadow in shadows)
			{
				var destShadow = Path.Combine(destination, Path.GetFileName(shadow));
				if (!Directory.Exists(destShadow))
					Directory.CreateDirectory(destShadow);
				MoveShadows(shadow, destShadow);
			}
		}

		private void ShadowDirectory(string path)
		{
			var shadowPath = Path.Combine(Path.GetDirectoryName(path), ".LVFS.shadow." + Path.GetFileName(path));
			Directory.CreateDirectory(shadowPath);
			if (Directory.Exists(path))
			{
				MoveShadows(path, shadowPath);
			}
		}

		private void ShadowFile(string path)
		{
			var shadowPath = Path.Combine(Path.GetDirectoryName(path), ".LVFS.shadow." + Path.GetFileName(path));
			using (var state = File.Create(shadowPath))
			{ }
		}

		private void RemoveDirectoryShadow(string path)
		{
			var shadowPath = Path.Combine(Path.GetDirectoryName(path), ".LVFS.shadow." + Path.GetFileName(path));
			Directory.Move(shadowPath, path);
		}

		private void RemoveFileShadow(string path)
		{
			var shadowPath = Path.Combine(Path.GetDirectoryName(path), ".LVFS.shadow." + Path.GetFileName(path));
			File.Delete(shadowPath);
		}

		private void SafeDirectoryDelete(string path)
		{
			if (Directory.Exists(path) && !Directory.EnumerateFileSystemEntries(path).Any())
				Directory.Delete(path);

			ShadowDirectory(path);
		}

		/// <inheritdoc/>
		public override NtStatus CheckDirectoryDeletable(string path)
		{
			if (HasFilesInDirectory(path))
				return DokanResult.DirectoryNotEmpty;
			else
				return DokanResult.Success;
		}

		/// <inheritdoc/>
		public override NtStatus CheckFileDeletable(string path)
		{
			if (HasDirectory(path))
				return DokanResult.AccessDenied;
			else if (HasRegularFile(path))
				return DokanResult.Success;
			else
				return DokanResult.FileNotFound;
		}

		/// <inheritdoc/>
		public override bool CleanupFileHandle(string path, LVFSContextInfo info)
		{
			try
			{
				var convetedPath = ConvertPath(path);

				object context;
				if (info.Context.TryGetValue(this, out context))
				{
					var stream = ((context as OneWayContext)?.Context as FileStream);
					if (stream != null)
					{
						stream.Dispose();
						if (!File.Exists(convetedPath))
							ShadowFile(convetedPath);
					}
					info.Context.Remove(this);
				}

				if (info.DeleteOnClose)
				{
					if (info.IsDirectory)
					{
						SafeDirectoryDelete(convetedPath);
					}
					else
					{
						if (File.Exists(convetedPath))
						{
							File.Delete(convetedPath);

							if (!File.Exists(convetedPath))
								ShadowFile(convetedPath);
						}
					}
				}
			}
			catch (Exception)
			{
				// Because there're no checked exceptions in C#, I can't tell what might go wrong here and catch specific exceptions.

				info.DeleteOnClose = false;
				PredecessorCleanupFileHandle(path, info);
				return false;
			}
			info.DeleteOnClose = false;
			return PredecessorCleanupFileHandle(path, info);
		}

		/// <inheritdoc/>
		public override bool CloseFileHandle(string path, LVFSContextInfo info)
		{
			try
			{
				var convetedPath = ConvertPath(path);

				object context;
				if (info.Context.TryGetValue(this, out context))
				{
					var stream = ((context as OneWayContext)?.Context as FileStream);
					if (stream != null)
					{
						stream.Dispose();
						if (!File.Exists(convetedPath))
							ShadowFile(convetedPath);
					}
					info.Context.Remove(this);
				}

				if (info.DeleteOnClose)
				{
					if (info.IsDirectory)
					{
						SafeDirectoryDelete(convetedPath);
					}
					else
					{
						if (File.Exists(convetedPath))
						{
							File.Delete(convetedPath);

							if (!File.Exists(convetedPath))
								ShadowFile(convetedPath);
						}
					}
				}
			}
			catch (Exception)
			{
				// Because there're no checked exceptions in C#, I can't tell what might go wrong here and catch specific exceptions.

				info.DeleteOnClose = false;
				PredecessorCleanupFileHandle(path, info);
				return false;
			}
			info.DeleteOnClose = false;
			return PredecessorCloseFileHandle(path, info);
		}

		/// <inheritdoc/>
		public override bool ControlsFile(string path)
		{
			var convertedPath = ConvertPath(path);
			return File.Exists(convertedPath) || Directory.Exists(convertedPath) || IsFileShadowed(convertedPath) || IsDirectoryShadowed(convertedPath);
		}

		/// <inheritdoc/>
		public override NtStatus CreateFileHandle(string path, DokanNet.FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, LVFSContextInfo info)
		{
			var context = new OneWayContext(path, access, share, mode, options, attributes);
			info.Context[this] = context;

			var copiedFromPredecessor = false;
			var directoryShadowRemoved = false;
			var fileShadowRemoved = false;
			var convertedPath = ConvertPath(path);
			var directoryExists = Directory.Exists(convertedPath);
			var fileExists = File.Exists(convertedPath);
			var directoryShadowed = IsDirectoryShadowed(convertedPath);
			var fileShadowed = IsFileShadowed(convertedPath);

			var controlsFile = File.Exists(convertedPath) || Directory.Exists(convertedPath) || IsFileShadowed(convertedPath) || IsDirectoryShadowed(convertedPath);
			context.OneWayControls = controlsFile;

			if (!controlsFile && (mode == FileMode.Open || mode == FileMode.OpenOrCreate) && PredecessorHasFile(path))
				return PredecessorCreateFileHandle(path, access, share, mode, options, attributes, info);

			if (info.IsDirectory)
			{
				try
				{
					switch (mode)
					{
						case FileMode.Open:
							{
								if (directoryExists)
								{
									if (!directoryShadowed)
									{
										context.OneWayControls = true;
										return DokanResult.Success;
									}
								}
								else if (fileExists)
								{
									if (!fileShadowed)
										return NtStatus.NotADirectory;
								}

								if (fileShadowed || directoryShadowed)
									return DokanResult.FileNotFound;
								else
									return PredecessorCreateFileHandle(path, access, share, mode, options, attributes, info);
							}
						case FileMode.CreateNew:
							{
								if (directoryExists || PredecessorHasDirectory(path))
								{
									if (!directoryShadowed)
										return DokanResult.AlreadyExists;
								}
								else if (fileExists || PredecessorHasRegularFile(path))
								{
									if (!fileShadowed)
										return DokanResult.FileExists;
								}

								if (directoryShadowed)
								{
									RemoveDirectoryShadow(convertedPath);
									directoryShadowRemoved = true;
								}

								if (!Directory.Exists(convertedPath))
									Directory.CreateDirectory(convertedPath);

								context.OneWayControls = true;
								return DokanResult.Success;
							}
						case FileMode.OpenOrCreate:
							{
								if (!directoryShadowed)
								{
									if (directoryExists)
									{
										context.OneWayControls = true;
										return DokanResult.Success;
									}
									else if (PredecessorHasDirectory(path))
										return PredecessorCreateFileHandle(path, access, share, mode, options, attributes, info);
									else
									{
										RemoveDirectoryShadow(convertedPath);
										directoryShadowRemoved = true;
									}
								}
								else if (fileExists || PredecessorHasRegularFile(path))
								{
									if (!fileShadowed)
										return NtStatus.NotADirectory;
								}

								if (!Directory.Exists(convertedPath))
									Directory.CreateDirectory(convertedPath);

								context.OneWayControls = true;
								return DokanResult.Success;
							}
						default:
							{
								// I don't think any other file modes can actually used with directories, so we should be free to die in any arbitrary way here. In fact, I don't think OpenOrCreate can actually be used with directories either, but the associated behaviour was simple enough, so I implemented it anyway.
								return DokanResult.NotImplemented;
							}
					}
				}
				catch (UnauthorizedAccessException)
				{
					if (directoryShadowRemoved)
						ShadowDirectory(convertedPath);
					if (fileShadowRemoved)
						ShadowFile(convertedPath);
					if (copiedFromPredecessor)
					{
						if (File.Exists(convertedPath))
							File.Delete(convertedPath);
						else
							SafeDirectoryDelete(convertedPath);
					}

					return DokanResult.AccessDenied;
				}
			}
			else
			{
				switch (mode)
				{
					case FileMode.Open:
						{
							if (fileShadowed && directoryShadowed)
								return DokanResult.FileNotFound;

							if (directoryShadowed)
							{
								if (!fileExists)
									PredecessorCreateFileHandle(path, access, share, mode, options, attributes, info);
							}
							else if (fileShadowed)
							{
								if (!directoryExists)
									PredecessorCreateFileHandle(path, access, share, mode, options, attributes, info);
							}

							if (fileExists || directoryExists)
							{
								var dataAccess = DokanNet.FileAccess.ReadData | DokanNet.FileAccess.WriteData | DokanNet.FileAccess.AppendData | DokanNet.FileAccess.Execute | DokanNet.FileAccess.GenericExecute | DokanNet.FileAccess.GenericWrite | DokanNet.FileAccess.GenericRead;
								var readWriteOnlyAttributes = (access & dataAccess) == 0;

								if (directoryExists || readWriteOnlyAttributes)
								{
									if (directoryExists && access.HasFlag(DokanNet.FileAccess.Delete) && !access.HasFlag(DokanNet.FileAccess.Synchronize))
										// Delete request on (potentially) non-empty directory
										return DokanResult.AccessDenied;

									info.IsDirectory = directoryExists;

									context.OneWayControls = true;
									return DokanResult.Success;
								}
								else
									// Go to the regular handler
									break;
							}
							else
								return PredecessorCreateFileHandle(path, access, share, mode, options, attributes, info);
						}
					case FileMode.CreateNew:
						{
							if (directoryExists)
							{
								if (!directoryShadowed)
									return DokanResult.AlreadyExists;
								else if (!fileShadowed)
								{
									if (PredecessorHasRegularFile(path))
										return DokanResult.AlreadyExists;
								}
								SafeDirectoryDelete(convertedPath);
								break;
							}
							else if (fileExists)
							{
								if (fileShadowed)
								{
									if (!directoryShadowed && PredecessorHasDirectory(path))
										return DokanResult.AlreadyExists;

									File.Delete(convertedPath);
									RemoveFileShadow(convertedPath);
									fileShadowRemoved = true;

									break;
								}
								else
									return DokanResult.AlreadyExists;
							}
							else
							{
								if ((!fileShadowed && PredecessorHasRegularFile(path)) || (!directoryShadowed && PredecessorHasDirectory(path)))
									return DokanResult.AlreadyExists;

								if (fileShadowed)
								{
									RemoveFileShadow(convertedPath);
									fileShadowRemoved = true;
								}

								break;
							}
						}
					case FileMode.OpenOrCreate:
						{
							if (directoryExists)
							{
								if (directoryShadowed)
								{
									if (!fileShadowed && PredecessorHasRegularFile(path))
										return PredecessorCreateFileHandle(path, access, share, mode, options, attributes, info);

									SafeDirectoryDelete(convertedPath);
								}

								break;
							}
							else if (fileExists)
							{
								if (fileShadowed)
								{
									if (!directoryShadowed && PredecessorHasDirectory(path))
										return PredecessorCreateFileHandle(path, access, share, mode, options, attributes, info);

									File.Delete(convertedPath);
								}

								break;
							}
							else
							{
								if ((!directoryShadowed && PredecessorHasDirectory(path)) || (!fileShadowed && PredecessorHasRegularFile(path)))
									return PredecessorCreateFileHandle(path, access, share, mode, options, attributes, info);
								else
									break;
							}
						}
					case FileMode.Truncate:
						{
							if (directoryExists)
							{
								if (directoryShadowed)
								{
									if (!fileShadowed && PredecessorHasRegularFile(path))
									{
										copiedFromPredecessor |= CopyFromPredecessorExcludingContent(path);
										break;
									}

									return DokanResult.FileNotFound;
								}
								else
									break;
							}
							else if (fileExists)
							{
								if (fileShadowed)
								{
									if (!directoryShadowed && PredecessorHasDirectory(path))
									{
										copiedFromPredecessor |= CopyFromPredecessorExcludingContent(path);
										break;
									}

									return DokanResult.FileNotFound;
								}
								else
									break;
							}
							else
							{
								if ((!directoryShadowed && PredecessorHasDirectory(path)) || (!fileShadowed && PredecessorHasRegularFile(path)))
								{
									copiedFromPredecessor |= CopyFromPredecessorExcludingContent(path);
									break;
								}

								return DokanResult.FileNotFound;
							}
						}
					case FileMode.Create:
						{
							if (directoryExists)
							{
								if (directoryShadowed)
								{
									if (!fileShadowed && PredecessorHasRegularFile(path))
										copiedFromPredecessor |= CopyFromPredecessorExcludingContent(path);
									else
										SafeDirectoryDelete(convertedPath);
								}

								break;
							}
							else if (fileExists)
							{
								if (fileShadowed)
								{
									if (!directoryShadowed && PredecessorHasDirectory(path))
										copiedFromPredecessor |= CopyFromPredecessorExcludingContent(path);
									else
										File.Delete(convertedPath);
								}

								break;
							}
							else
							{
								if ((!directoryShadowed && PredecessorHasDirectory(path)) || (!fileShadowed && PredecessorHasRegularFile(path)))
									copiedFromPredecessor |= CopyFromPredecessorExcludingContent(path);

								break;
							}
						}
					case FileMode.Append:
						{
							if (directoryExists)
							{
								if (directoryShadowed)
								{
									if (!fileShadowed && PredecessorHasRegularFile(path))
									{
										copiedFromPredecessor |= CopyFromPredecessor(path);
										break;
									}
									else
										return DokanResult.FileNotFound;
								}
								else
									break;
							}
							else if (fileExists)
							{
								if (fileShadowed)
								{
									if (!directoryShadowed && PredecessorHasDirectory(path))
									{
										copiedFromPredecessor |= CopyFromPredecessor(path);
										break;
									}
									else
										return DokanResult.FileNotFound;
								}
								else
									break;
							}
							else
							{
								if ((!directoryShadowed && PredecessorHasDirectory(path)) || (!fileShadowed && PredecessorHasRegularFile(path)))
								{
									copiedFromPredecessor |= CopyFromPredecessor(path);
									break;
								}
								else
									return DokanResult.FileNotFound;
							}
						}
					default:
						// This code should never be reached
						throw new ArgumentException("Unknown file mode: " + mode, nameof(mode));
				}

				var dataWriteAccess = DokanNet.FileAccess.WriteData | DokanNet.FileAccess.AppendData | DokanNet.FileAccess.Delete | DokanNet.FileAccess.GenericWrite;
				var readAccessOnly = (access & dataWriteAccess) == 0;

				try
				{
					var result = DokanResult.Success;

					var parentDirectory = Path.GetDirectoryName(convertedPath);
					if (IsDirectoryShadowed(parentDirectory))
					{
						if (directoryShadowRemoved)
							ShadowDirectory(convertedPath);
						if (fileShadowRemoved)
							ShadowFile(convertedPath);
						if (copiedFromPredecessor)
						{
							if (File.Exists(convertedPath))
								File.Delete(convertedPath);
							else
								SafeDirectoryDelete(convertedPath);
						}

						return DokanResult.PathNotFound;
					}
					else
					{
						if (!Directory.Exists(parentDirectory))
						{
							if (PredecessorHasDirectory(Path.GetDirectoryName(path)))
								CopyFromPredecessor(Path.GetDirectoryName(path));
							else
							{
								if (directoryShadowRemoved)
									ShadowDirectory(convertedPath);
								if (fileShadowRemoved)
									ShadowFile(convertedPath);
								if (copiedFromPredecessor)
								{
									if (File.Exists(convertedPath))
										File.Delete(convertedPath);
									else
										SafeDirectoryDelete(convertedPath);
								}

								return DokanResult.PathNotFound;
							}
						}
					}
					
					context.Context = new FileStream(convertedPath, mode, readAccessOnly ? System.IO.FileAccess.Read : System.IO.FileAccess.ReadWrite, share, 4096, options);

					if ((fileExists || directoryExists) && (mode == FileMode.OpenOrCreate || mode == FileMode.Create))
						result = DokanResult.AlreadyExists;

					if (mode == FileMode.CreateNew || mode == FileMode.Create)
						attributes |= FileAttributes.Archive;

					File.SetAttributes(convertedPath, attributes);

					if (File.Exists(convertedPath) && IsFileShadowed(convertedPath))
						RemoveFileShadow(convertedPath);
					else if (Directory.Exists(convertedPath) && IsDirectoryShadowed(convertedPath))
						RemoveDirectoryShadow(convertedPath);

					return result;
				}
				catch (UnauthorizedAccessException)
				{
					if (directoryShadowRemoved)
						ShadowDirectory(convertedPath);
					if (fileShadowRemoved)
						ShadowFile(convertedPath);
					if (copiedFromPredecessor)
					{
						if (File.Exists(convertedPath))
							File.Delete(convertedPath);
						else
							SafeDirectoryDelete(convertedPath);
					}

					return DokanResult.AccessDenied;
				}
				catch (DirectoryNotFoundException)
				{
					if (directoryShadowRemoved)
						ShadowDirectory(convertedPath);
					if (fileShadowRemoved)
						ShadowFile(convertedPath);
					if (copiedFromPredecessor)
					{
						if (File.Exists(convertedPath))
							File.Delete(convertedPath);
						else
							SafeDirectoryDelete(convertedPath);
					}

					return DokanResult.PathNotFound;
				}
				catch (Exception ex)
				{
					var hr = (uint)System.Runtime.InteropServices.Marshal.GetHRForException(ex);
					switch (hr)
					{
						case 0x80070020: //Sharing violation
							{
								if (directoryShadowRemoved)
									ShadowDirectory(convertedPath);
								if (fileShadowRemoved)
									ShadowFile(convertedPath);
								if (copiedFromPredecessor)
								{
									if (File.Exists(convertedPath))
										File.Delete(convertedPath);
									else
										SafeDirectoryDelete(convertedPath);
								}

								return DokanResult.SharingViolation;
							}
						default:
							{
								if (directoryShadowRemoved)
									ShadowDirectory(convertedPath);
								if (fileShadowRemoved)
									ShadowFile(convertedPath);
								if (copiedFromPredecessor)
								{
									if (File.Exists(convertedPath))
										File.Delete(convertedPath);
									else
										SafeDirectoryDelete(convertedPath);
								}

								throw;
							}
					}
				}
			}
		}

		/// <inheritdoc/>
		public override NtStatus FlushBuffers(string path, LVFSContextInfo info)
		{
			EnsureModifiable(path, info);

			if (info.Context.ContainsKey(this))
			{
				try
				{
					var stream = (info.Context[this] as OneWayContext).Context as FileStream;
					if (stream != null)
						stream.Flush();
				}
				catch (IOException)
				{
					return DokanResult.DiskFull;
				}
			}

			return DokanResult.Success;
		}

		/// <inheritdoc/>
		public override FileInformation? GetFileInformation(string path)
		{
			var convertedPath = ConvertPath(path);

			FileSystemInfo fileInfo;

			if (!IsFileShadowed(convertedPath))
			{
				fileInfo = new FileInfo(convertedPath);
				if (!fileInfo.Exists)
				{
					if (!IsDirectoryShadowed(convertedPath))
					{
						fileInfo = new DirectoryInfo(convertedPath);
						if (!fileInfo.Exists)
							return GetPredecessorFileInformation(path);
					}
					else
					{
						if (PredecessorHasRegularFile(path))
							return GetPredecessorFileInformation(path);
					}
				}
			}
			else
			{
				if (!IsDirectoryShadowed(convertedPath))
				{
					fileInfo = new DirectoryInfo(convertedPath);
					if (!fileInfo.Exists)
					{
						if (PredecessorHasDirectory(path))
							return GetPredecessorFileInformation(path);
					}
				}
				else
					return null;
			}

			return new FileInformation
			{
				FileName = path,
				Attributes = fileInfo.Attributes,
				CreationTime = fileInfo.CreationTime,
				LastAccessTime = fileInfo.LastAccessTime,
				LastWriteTime = fileInfo.LastWriteTime,
				Length = (fileInfo as FileInfo)?.Length ?? 0
			};
		}

		/// <inheritdoc/>
		public override FileSystemSecurity GetFileSystemSecurity(string path, AccessControlSections sections)
		{
			var convertedPath = ConvertPath(path);
			if (IsFileShadowed(convertedPath))
			{
				if (IsDirectoryShadowed(convertedPath))
					return null;
				else
				{
					if (Directory.Exists(convertedPath))
						return Directory.GetAccessControl(convertedPath, sections);
					else
					{
						if (PredecessorHasDirectory(path))
							return GetPredecessorFileSystemSecurity(path, sections);
						else
							return null;
					}
				}
			}
			else
			{
				if (File.Exists(convertedPath))
					return File.GetAccessControl(convertedPath, sections);
				else
				{
					if (IsDirectoryShadowed(convertedPath))
					{
						if (PredecessorHasRegularFile(path))
							return GetPredecessorFileSystemSecurity(path, sections);
						else
							return null;
					}
					else
					{
						if (Directory.Exists(convertedPath))
							return Directory.GetAccessControl(convertedPath, sections);
						else
							return GetPredecessorFileSystemSecurity(path, sections);
					}
				}
			}
		}

		/// <inheritdoc/>
		public override Tuple<long, long, long> GetSpaceInformation()
		{
			var driveInfo = DriveInfo.GetDrives().Single(drive => string.Equals(drive.RootDirectory.Name, Path.GetPathRoot(DirectoryPath + "\\"), StringComparison.OrdinalIgnoreCase));
			
			return new Tuple<long, long, long>(driveInfo.TotalFreeSpace, driveInfo.TotalSize, driveInfo.AvailableFreeSpace);
		}

		/// <inheritdoc/>
		public override bool HasDirectory(string path)
		{
			return IsDirectoryVisible(ConvertPath(path)) || IsPredecessorDirectoryVisible(path);
		}

		/// <inheritdoc/>
		public override bool HasFile(string path)
		{
			return HasDirectory(path) || HasRegularFile(path);
		}

		/// <inheritdoc/>
		public override bool HasFilesInDirectory(string path)
		{
			var convertedPath = ConvertPath(path);
			if (Directory.Exists(convertedPath) && !IsDirectoryShadowed(convertedPath))
			{
				var rawFiles = Directory.EnumerateFiles(convertedPath);
				foreach (var file in rawFiles)
				{
					if (Path.GetFileName(file).StartsWith(".LVFS.shadow."))
						continue;
					if (!IsFileShadowed(file))
						return true;
				}
				rawFiles = Directory.EnumerateDirectories(convertedPath);
				foreach (var dir in rawFiles)
				{
					if (Path.GetFileName(dir).StartsWith(".LVFS.shadow."))
						continue;
					if (!IsDirectoryShadowed(dir))
						return true;
				}
			}
			return PredecessorHasFilesInDirectory(path);
		}

		/// <inheritdoc/>
		public override bool HasRegularFile(string path)
		{
			return IsFileVisible(ConvertPath(path)) || IsPredecessorFileVisible(path);
		}

		/// <inheritdoc/>
		public override IList<FileInformation> ListFiles(string path)
		{
			var convertedPath = ConvertPath(path);
			if (IsDirectoryShadowed(convertedPath))
				return null;

			IList<FileInformation> predecessorList = ListPredecessorFiles(path);

			HashSet<string> names = new HashSet<string>();
			IList<FileInformation> resultList;
			if (Directory.Exists(convertedPath))
			{
				IEnumerable<FileInformation> thisCollection = new DirectoryInfo(convertedPath).GetFileSystemInfos().Where((fileInfo) =>
				{
					if (fileInfo.Name.StartsWith(".LVFS.shadow."))
						return false;
					names.Add(UnescapeFileName(fileInfo.Name));
					if (fileInfo.Attributes.HasFlag(FileAttributes.Directory))
						return !IsDirectoryShadowed(fileInfo.FullName);
					else
						return !IsFileShadowed(fileInfo.FullName);
				}).Select((fileInfo) =>
				{
					return new FileInformation
					{
						FileName = UnescapeFileName(fileInfo.Name),
						Attributes = fileInfo.Attributes,
						CreationTime = fileInfo.CreationTime,
						LastAccessTime = fileInfo.LastAccessTime,
						LastWriteTime = fileInfo.LastWriteTime,
						Length = (fileInfo as FileInfo)?.Length ?? 0
					};
				});
				resultList = new List<FileInformation>(thisCollection);
			}
			else
				resultList = new List<FileInformation>();

			foreach (FileInformation fileInfo in predecessorList)
			{
				if (!names.Contains(fileInfo.FileName))
				{
					var convFilePath = Path.Combine(convertedPath, EscapeFileName(fileInfo.FileName));
					if ((fileInfo.Attributes.HasFlag(FileAttributes.Directory) && !IsDirectoryShadowed(convFilePath)) || (!fileInfo.Attributes.HasFlag(FileAttributes.Directory) && !IsFileShadowed(convFilePath)))
						resultList.Add(fileInfo);
				}
			}

			return resultList;
		}

		/// <inheritdoc/>
		public override NtStatus MoveFile(string currentPath, string newPath, bool replace, LVFSContextInfo info)
		{
			EnsureModifiable(currentPath, info);

			var fullCurrentPath = ConvertPath(currentPath);
			var fullNewPath = ConvertPath(newPath);

			try
			{
				object rawContext;
				info.Context.TryGetValue(this, out rawContext);
				var context = rawContext as OneWayContext;
				(context.Context as FileStream)?.Dispose();
				context.Context = null;

				if (!HasFile(newPath))
				{
					if (info.IsDirectory)
					{
						if (IsDirectoryShadowed(fullNewPath))
							RemoveDirectoryShadow(fullNewPath);
						Directory.Move(fullCurrentPath, fullNewPath);
					}
					else
					{
						if (IsFileShadowed(fullNewPath))
							RemoveFileShadow(fullNewPath);
						File.Move(fullCurrentPath, fullNewPath);
					}

					return DokanResult.Success;
				}
				else if (replace)
				{
					// replace is incompatible with directories
					if (info.IsDirectory)
						return DokanResult.AccessDenied;

					if (File.Exists(fullNewPath))
					{
						File.Delete(fullNewPath);
						File.Move(fullCurrentPath, fullNewPath);
						return DokanResult.Success;
					}
					else
					{
						// A predecessor source has the file to be replaced, so we can ignore it.
						File.Move(fullCurrentPath, fullNewPath);
						return DokanResult.Success;
					}
				}
				else
					return DokanResult.FileExists;
			}
			catch (UnauthorizedAccessException)
			{
				return DokanResult.AccessDenied;
			}
		}

		/// <inheritdoc/>
		public override bool OnMount()
		{
			return Directory.Exists(DirectoryPath);
		}

		/// <inheritdoc/>
		public override bool ReadFile(string path, byte[] buffer, out int bytesRead, long offset, LVFSContextInfo info)
		{
			var convertedPath = ConvertPath(path);
			if (info.Context.ContainsKey(this))
			{
				var context = info.Context[this] as OneWayContext;
				if (!context.OneWayControls && File.Exists(convertedPath))
					TransferFileHandle(path, info);
				if (context.OneWayControls)
				{
					var stream = context.Context as FileStream;
					lock (stream)
					{
						stream.Position = offset;
						bytesRead = stream.Read(buffer, 0, buffer.Length);
					}
					return true;
				}
			}
			if (File.Exists(convertedPath))
			{
				using (var stream = new FileStream(convertedPath, FileMode.Open, System.IO.FileAccess.Read))
				{
					stream.Position = offset;
					bytesRead = stream.Read(buffer, 0, buffer.Length);
				}
				return true;
			}

			return PredecessorReadFile(path, buffer, out bytesRead, offset, info);
		}

		/// <inheritdoc/>
		public override NtStatus SetAllocatedSize(string path, long allocationSize, LVFSContextInfo info)
		{
			// C# offers no ability to allocate space for a file without actually setting its length, so this function cannot be implemented without calling native code. In the mirror example, this is implemented with incorrect semantics. Despite that, many operations require this to claim to work, so we return success whenever this is called.
			EnsureModifiable(path, info);
			if (allocationSize < ((info.Context[this] as OneWayContext).Context as FileStream).Length)
				((info.Context[this] as OneWayContext).Context as FileStream).SetLength(allocationSize);
			return DokanResult.Success;
		}

		/// <inheritdoc/>
		public override NtStatus SetFileAttributes(string path, FileAttributes attributes)
		{
			EnsureModifiable(path, null);
			try
			{
				File.SetAttributes(ConvertPath(path), attributes);
				return DokanResult.Success;
			}
			catch (UnauthorizedAccessException)
			{
				return DokanResult.AccessDenied;
			}
			catch (FileNotFoundException)
			{
				return DokanResult.FileNotFound;
			}
			catch (DirectoryNotFoundException)
			{
				return DokanResult.PathNotFound;
			}
		}

		/// <inheritdoc/>
		public override NtStatus SetFileSecurity(string path, FileSystemSecurity security, AccessControlSections sections, LVFSContextInfo info)
		{
			EnsureModifiable(path, info);
			var convertedPath = ConvertPath(path);
			FileSystemSecurity actualSecurity;
			if (info.IsDirectory)
				actualSecurity = Directory.GetAccessControl(convertedPath, sections);
			else
				actualSecurity = File.GetAccessControl(convertedPath, sections);

			var desiredSddlForm = security.GetSecurityDescriptorSddlForm(sections);
			actualSecurity.SetSecurityDescriptorSddlForm(desiredSddlForm, sections);

			if (info.IsDirectory)
				Directory.SetAccessControl(convertedPath, actualSecurity as DirectorySecurity);
			else
				File.SetAccessControl(convertedPath, actualSecurity as FileSecurity);
			return DokanResult.Success;
		}

		/// <inheritdoc/>
		public override NtStatus SetFileTimes(string path, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime)
		{
			EnsureModifiable(path, null);
			var realPath = ConvertPath(path);
			try
			{
				if (creationTime.HasValue)
					File.SetCreationTime(realPath, creationTime.Value);
				if (lastAccessTime.HasValue)
					File.SetLastAccessTime(realPath, lastAccessTime.Value);
				if (lastWriteTime.HasValue)
					File.SetLastWriteTime(realPath, lastWriteTime.Value);

				return DokanResult.Success;
			}
			catch (UnauthorizedAccessException)
			{
				return DokanResult.AccessDenied;
			}
			catch (FileNotFoundException)
			{
				return DokanResult.FileNotFound;
			}
		}

		/// <inheritdoc/>
		public override NtStatus SetLength(string path, long length, LVFSContextInfo info)
		{
			EnsureModifiable(path, info);
			try
			{
				object rawContext;
				info.Context.TryGetValue(this, out rawContext);
				var stream = ((rawContext as OneWayContext)?.Context ?? null) as FileStream;
				if (stream != null)
					stream.SetLength(length);
				else
					using (stream = new FileStream(ConvertPath(path), FileMode.Open))
					{
						stream.SetLength(length);
					}
				return DokanResult.Success;
			}
			catch (IOException)
			{
				return DokanResult.DiskFull;
			}
			catch (UnauthorizedAccessException)
			{
				return DokanResult.AccessDenied;
			}
		}

		/// <inheritdoc/>
		public override bool TryLockFileRegion(string path, long startOffset, long length, LVFSContextInfo info)
		{
			if (info.Context.ContainsKey(this))
			{
				var context = info.Context[this] as OneWayContext;
				if (!context.OneWayControls && File.Exists(ConvertPath(path)))
					TransferFileHandle(path, info);
				if (context.OneWayControls)
				{
					try
					{
						(context.Context as FileStream)?.Lock(startOffset, length);
						return true;
					}
					catch (IOException)
					{
						return false;
					}
				}
			}
			return PredecessorTryLockFileRegion(path, startOffset, length, info);
		}

		/// <inheritdoc/>
		public override bool TryUnlockFileRegion(string path, long startOffset, long length, LVFSContextInfo info)
		{
			if (info.Context.ContainsKey(this))
			{
				var context = info.Context[this] as OneWayContext;
				if (!context.OneWayControls && File.Exists(ConvertPath(path)))
					TransferFileHandle(path, info);
				if (context.OneWayControls)
				{
					try
					{
						(context.Context as FileStream)?.Unlock(startOffset, length);
						return true;
					}
					catch (IOException)
					{
						return false;
					}
				}
			}
			return PredecessorTryUnlockFileRegion(path, startOffset, length, info);
		}

		/// <inheritdoc/>
		public override NtStatus WriteFile(string path, byte[] buffer, out int bytesWritten, long offset, LVFSContextInfo info)
		{
			EnsureModifiable(path, info);
			if (info.Context.ContainsKey(this))
			{
				var context = info.Context[this] as OneWayContext;
				try
				{
					var stream = context.Context as FileStream;
					lock (stream)
					{
						stream.Position = offset;
						stream.Write(buffer, 0, buffer.Length);
					}
					bytesWritten = buffer.Length;

					return DokanResult.Success;
				}
				catch (UnauthorizedAccessException)
				{
					bytesWritten = 0;
					return DokanResult.AccessDenied;
				}
				catch (IOException)
				{
					bytesWritten = 0;
					return DokanResult.DiskFull;
				}
			}
			else
			{
				using (var stream = new FileStream(ConvertPath(path), FileMode.Open, System.IO.FileAccess.Write))
				{
					try
					{
						stream.Position = offset;
						stream.Write(buffer, 0, buffer.Length);
						bytesWritten = buffer.Length;
					}
					catch (UnauthorizedAccessException)
					{
						bytesWritten = 0;
						return DokanResult.AccessDenied;
					}
					catch (IOException)
					{
						bytesWritten = 0;
						return DokanResult.DiskFull;
					}
				}
				return DokanResult.Success;
			}
		}
	}
}
