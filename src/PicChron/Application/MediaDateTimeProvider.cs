using PicChron.Core;
using TagLib;

namespace PicChron.Application
{
	public class MediaDateTimeProvider : IDateTimeProvider
	{
		private readonly IDateTimeValidator _dateTimeValidator = new DateTimeValidator();

		public async Task<DateTime?> GetDateTime(string filePath)
		{
			try
			{
				var file = await Task.FromResult(TagLib.File.Create(filePath));

				if (file == null)
				{
					return null;
				}

				DateTime? dateTime = null;

				// First, try to get date from tag properties
				if (file.Tag != null)
				{
					var tag = file.Tag;
					
					// TagLib# extracts the Year from MP4/media files
					// For MP4, this represents the media creation date's year
					if (tag.Year > 0 && tag.Year < 2100)
					{
						try
						{
							// Create a date with January 1st if we only have year
							dateTime = new DateTime((int)tag.Year, 1, 1);
						}
						catch
						{
							// Invalid year value
						}
					}
				}

				// If no date from tag, try file system properties
				if (dateTime == null)
				{
					var fileInfo = new FileInfo(filePath);
					
					// Use file creation time as fallback (this is the QuickTime creation date on Windows)
					if (fileInfo.CreationTimeUtc.Year > 1980 && fileInfo.CreationTimeUtc.Year < 2100)
					{
						dateTime = fileInfo.CreationTimeUtc;
					}
					// Otherwise use file modification time
					else if (fileInfo.LastWriteTimeUtc.Year > 1980 && fileInfo.LastWriteTimeUtc.Year < 2100)
					{
						dateTime = fileInfo.LastWriteTimeUtc;
					}
				}

				if (dateTime == null)
				{
					return null;
				}

				return _dateTimeValidator.IsValidYear((DateTime)dateTime) ? dateTime : null;
			}
			catch
			{
				// If TagLib fails, return null to allow fallback to other providers
			}

			return null;
		}
	}
}

