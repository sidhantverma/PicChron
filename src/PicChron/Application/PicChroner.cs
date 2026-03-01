using PicChron.Core;
using System.Net.Sockets;

namespace PicChron.Application
{
	public class PicChroner : IPicChroner
	{
		private readonly IDateTimeProvider _exifDateTimeProvider = new ExifDateTimeProvider();
		private readonly IDateTimeProvider _regexDateTimeProvider = new RegexDateTimeProvider();
		private readonly IDateTimeProvider _mediaDateTimeProvider = new MediaDateTimeProvider();
		
		private readonly IDestinationPathProvider _destinationPathProvider;
		private readonly PicChronOptions _picChronOptions;

		private int _totalFiles;
		private int _filesProcessed;

		public string[] _validMediaFileExtensions =
			new string[] { ".jpg", ".jpeg", ".tiff", ".png", ".gif", ".mp4", ".3gp", ".mov", ".avi", ".webp", ".heic", ".mp4" };
		

		public event EventHandler<Exception>? OnError;
		public event EventHandler<string>? SortCompleted;
		public event EventHandler<int>? PercentageCompleted;

		public PicChroner(PicChronOptions options)
		{
			_picChronOptions = options;
			_destinationPathProvider = new DestinationPathProvider(options.DestinationPath);
		}

		public async Task StartSorting()
		{
			try
			{
				var files = 
					GetFiles()
					.Where(f => _validMediaFileExtensions.Any(ext => ext == f.Extension.ToLower())).ToArray();

				_totalFiles = files.Count();
				_filesProcessed = 0;

				if (_totalFiles == 0)
				{
					SortCompleted?.Invoke(this, "No files found to process");
					return;
				}

				foreach (var file in files)
				{
					try
					{
						// For video/media files, try media provider first; for images, try exif first
						var isVideoFile = IsVideoFile(file.Extension.ToLower());
						
						DateTime? dateTime;
						if (isVideoFile)
						{
							// Try media provider first for video files
							dateTime = await (_mediaDateTimeProvider.GetDateTime(file.FullName) ?? Task.FromResult<DateTime?>(null));
							// Fallback to exif and regex
							if (dateTime == null)
							{
								dateTime = await (_exifDateTimeProvider.GetDateTime(file.FullName) ?? Task.FromResult<DateTime?>(null));
							}
							if (dateTime == null)
							{
								dateTime = await (_regexDateTimeProvider.GetDateTime(file.FullName) ?? Task.FromResult<DateTime?>(null));
							}
						}
						else
						{
							// For image files, try exif first
							dateTime = await (_exifDateTimeProvider.GetDateTime(file.FullName) ?? Task.FromResult<DateTime?>(null));
							// Then try media provider (helpful for HEIC)
							if (dateTime == null)
							{
								dateTime = await (_mediaDateTimeProvider.GetDateTime(file.FullName) ?? Task.FromResult<DateTime?>(null));
							}
							// Finally try regex
							if (dateTime == null)
							{
								dateTime = await (_regexDateTimeProvider.GetDateTime(file.FullName) ?? Task.FromResult<DateTime?>(null));
							}
						}

						if (dateTime is null)
						{
							// log file name and reason for skipping
							Console.WriteLine($"Skipping file: {file.FullName}. Reason: No date/time information found.");
							continue;
						}

						var (destDir, destPath) = _destinationPathProvider.GetDestinationPaths(file, dateTime.Value);
						CreateDirectoryIfNotExists(destDir);
						ProcessFile(destPath, file, (DateTime)dateTime);
						MarkProgress();
					}
					catch (Exception ex)
					{
						OnError?.Invoke(file, ex);
					}
				}

				SortCompleted?.Invoke(this, GetCompletedMsg());
			}
			catch (Exception ex)
			{
				OnError?.Invoke(this, ex);
			}
		}

		private FileInfo[] GetFiles()
		{
			var directory = new DirectoryInfo(_picChronOptions.SourcePath);
			switch (_picChronOptions.ScanType)
			{
				case ScanType.All:
					return directory.GetFiles("*.*", SearchOption.AllDirectories);
				default:
					return directory.GetFiles("*.*", SearchOption.TopDirectoryOnly);
			}
		}

		private bool IsVideoFile(string extension)
		{
			var videoExtensions = new[] { ".mp4", ".3gp", ".mov", ".avi", ".webp", ".mkv", ".flv", ".wmv", ".m4v" };
			return videoExtensions.Contains(extension);
		}

		private void CreateDirectoryIfNotExists(string destDir)
		{
			if (!Directory.Exists(destDir))
			{
				Directory.CreateDirectory(destDir);
			}
		}

                private void ProcessFile(string destPath, FileInfo file, DateTime dateTime)
                {
                        FileInfo targetFile;

                        switch (_picChronOptions.FileTransferMode)
                        {
                                case FileTransferMode.Move:
                                        file.MoveTo(destPath);
                                        targetFile = file;
                                        break;
                                case FileTransferMode.Copy:
                                        targetFile = file.CopyTo(destPath);
                                        break;
                                default:
                                        // Should not reach here but assign to avoid uninitialized variable warning
                                        targetFile = file;
                                        break;
                        }

                        if(_picChronOptions.RewriteFileAccessAndWriteTime)
                        {
                                targetFile.LastAccessTime = dateTime;
                                targetFile.LastWriteTime = dateTime;
                        }
                }

		private string GetCompletedMsg()
		{
			if (_totalFiles == _filesProcessed) return "All files processed successfully";
			else return $"Files processed: {_filesProcessed}, Files skipped: {_totalFiles - _filesProcessed}";
		}

		private void MarkProgress()
		{
			_filesProcessed += 1;
			PercentageCompleted?.Invoke(this, GetPercentageCompleted(_totalFiles, _filesProcessed));
		}

		private int GetPercentageCompleted(int totalFiles, int filesProcessed)
		{
			var percentage = (int)Math.Round((float)filesProcessed / totalFiles * 100);
			return percentage;
		}
	}
}
