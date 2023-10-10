using PicChron.Core;
using System.Net.Sockets;

namespace PicChron.Application
{
	public class PicChroner : IPicChroner
	{
		private readonly IDateTimeProvider _exifDateTimeProvider = new ExifDateTimeProvider();
		private readonly IDateTimeProvider _regexDateTimeProvider = new RegexDateTimeProvider();
		
		private readonly IDestinationPathProvider _destinationPathProvider;
		private readonly PicChronOptions _picChronOptions;

		private int _totalFiles;
		private int _filesProcessed;

		private string[] _validMediaFileExtensions =
			new string[] { ".jpg", ".jpeg", ".tiff", ".png", ".gif", ".mp4", ".3gp", ".mov", ".avi", ".webp" };
		

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
						var exifDateTime = await (_exifDateTimeProvider.GetDateTime(file.FullName) ?? Task.FromResult<DateTime?>(null));
						var regexDateTime = await (_regexDateTimeProvider.GetDateTime(file.FullName) ?? Task.FromResult<DateTime?>(null));

						var dateTime = exifDateTime ?? regexDateTime;

						if (dateTime is null)
						{
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

		private void CreateDirectoryIfNotExists(string destDir)
		{
			if (!Directory.Exists(destDir))
			{
				Directory.CreateDirectory(destDir);
			}
		}

		private void ProcessFile(string destPath, FileInfo file, DateTime dateTime)
		{
			switch (_picChronOptions.FileTransferMode)
			{
				case FileTransferMode.Move:
					file.MoveTo(destPath);
					break;
				case FileTransferMode.Copy:
					file.CopyTo(destPath);
					break;
			}

			if(_picChronOptions.RewriteFileAccessAndWriteTime)
			{
				file.LastAccessTime = dateTime;
				file.LastWriteTime = dateTime;
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
