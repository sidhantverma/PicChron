using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using PicChron.Core;

namespace PicChron.Application
{
	public class ExifDateTimeProvider : IDateTimeProvider
	{
		private readonly IDateTimeValidator _dateTimeValidator = new DateTimeValidator();
		
		public async Task<DateTime?> GetDateTime(string filePath)
		{
			// All image formats (including HEIC) use MetadataExtractor
			return await GetDateTimeFromExif(filePath);
		}

		private async Task<DateTime?> GetDateTimeFromExif(string filePath)
		{
			try
			{
				// For most image formats, try MetadataExtractor first (more robust)
				var directories = await Task.Run(() => ImageMetadataReader.ReadMetadata(filePath));

				var exifDir = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
				if (exifDir != null)
				{
					// Try DateTime tags in order of preference
					if (exifDir.TryGetDateTime(ExifDirectoryBase.TagDateTime, out var dateTime) &&
						_dateTimeValidator.IsValidYear(dateTime))
					{
						return dateTime;
					}

					if (exifDir.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var dateTimeOriginal) &&
						_dateTimeValidator.IsValidYear(dateTimeOriginal))
					{
						return dateTimeOriginal;
					}

					if (exifDir.TryGetDateTime(ExifDirectoryBase.TagDateTimeDigitized, out var dateTimeDigitized) &&
						_dateTimeValidator.IsValidYear(dateTimeDigitized))
					{
						return dateTimeDigitized;
					}
				}

				// Also check SubIfd
				var subIfdDir = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
				if (subIfdDir != null)
				{
					if (subIfdDir.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var dateTimeOriginal) &&
						_dateTimeValidator.IsValidYear(dateTimeOriginal))
					{
						return dateTimeOriginal;
					}
				}

				return null;
			}
			catch
			{
				return null;
			}
		}
	}
}
