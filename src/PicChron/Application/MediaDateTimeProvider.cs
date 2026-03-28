using PicChron.Core;
using TagLib;
using MetadataExtractor;

namespace PicChron.Application
{
	public class MediaDateTimeProvider : IDateTimeProvider
	{
		private readonly IDateTimeValidator _dateTimeValidator = new DateTimeValidator();

		public async Task<DateTime?> GetDateTime(string filePath)
		{
			try
			{
				// For MP4/MOV files, try to extract QuickTime creation date
				if (filePath.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
					filePath.EndsWith(".mov", StringComparison.OrdinalIgnoreCase) ||
					filePath.EndsWith(".m4v", StringComparison.OrdinalIgnoreCase))
				{
					var qtDate = await GetQuickTimeCreationDate(filePath);
					if (qtDate.HasValue)
					{
						return qtDate;
					}
				}

				// Fall back to TagLib for general media metadata
				var tagLibDate = await GetDateTimeFromTagLib(filePath);
				if (tagLibDate.HasValue)
				{
					return tagLibDate;
				}

				return null;
			}
			catch
			{
				return null;
			}
		}

		private async Task<DateTime?> GetQuickTimeCreationDate(string filePath)
		{
			try
			{
				var directories = await Task.Run(() => ImageMetadataReader.ReadMetadata(filePath));
				
				// Look for all directories to find QuickTime data
				foreach (var directory in directories)
				{
					// Check all tags for creation/modification date
					foreach (var tag in directory.Tags)
					{
						var tagName = tag.Name;
						var tagValue = tag.Description;
						
						// Look for creation/modification date tags (case insensitive)
						if (tagName.IndexOf("Create", StringComparison.OrdinalIgnoreCase) >= 0 ||
							tagName.IndexOf("Modify", StringComparison.OrdinalIgnoreCase) >= 0)
						{
							// Try to parse the date value with multiple formats
							if (!string.IsNullOrWhiteSpace(tagValue))
							{
								// Try different date formats
								string[] dateFormats = new string[]
								{
									"ddd MMM dd HH:mm:ss yyyy",  // Sat Feb 07 18:34:50 2026
									"yyyy-MM-dd HH:mm:ss",       // 2026-02-07 18:34:50
									"yyyy:MM:dd HH:mm:ss",       // 2026:02:07 18:34:50
									"o",                          // ISO 8601
								};
								
								if (DateTime.TryParseExact(tagValue, dateFormats, System.Globalization.CultureInfo.InvariantCulture, 
									System.Globalization.DateTimeStyles.None, out var parsedDate))
								{
									if (_dateTimeValidator.IsValidYear(parsedDate))
									{
										return parsedDate;
									}
								}
								else if (DateTime.TryParse(tagValue, System.Globalization.CultureInfo.InvariantCulture,
									System.Globalization.DateTimeStyles.None, out var fallbackDate))
								{
									if (_dateTimeValidator.IsValidYear(fallbackDate))
									{
										return fallbackDate;
									}
								}
							}
						}
					}
				}

				return null;
			}
			catch
			{
				return null;
			}
		}

		private async Task<DateTime?> GetDateTimeFromTagLib(string filePath)
		{
			try
			{
				var file = await Task.FromResult(TagLib.File.Create(filePath));

				if (file == null)
				{
					return null;
				}

				DateTime? dateTime = null;

				// Try to get date from tag properties
				if (file.Tag != null)
				{
					var tag = file.Tag;
					
					// Try DateTagged first (full date/time)
					if (tag.DateTagged.HasValue && tag.DateTagged.Value.Year > 1980)
					{
						dateTime = tag.DateTagged.Value;
					}
					// Fall back to Year only
					else if (tag.Year > 0 && tag.Year < 2100)
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

				if (dateTime == null)
				{
					return null;
				}

				return _dateTimeValidator.IsValidYear((DateTime)dateTime) ? dateTime : null;
			}
			catch
			{
				return null;
			}
		}
	}
}

