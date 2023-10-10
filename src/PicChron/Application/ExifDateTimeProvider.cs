using ExifLibrary;
using PicChron.Core;

namespace PicChron.Application
{
	public class ExifDateTimeProvider : IDateTimeProvider
	{
		private readonly IDateTimeValidator _dateTimeValidator = new DateTimeValidator();
		public async Task<DateTime?> GetDateTime(string filePath)
		{
			try
			{
				var exifImageFile = await Task.FromResult(ImageFile.FromFile(filePath));

				DateTime? dateTime = 
					exifImageFile.Properties.Get<ExifDateTime>(ExifTag.DateTime) ??
					exifImageFile.Properties.Get<ExifDateTime>(ExifTag.DateTimeOriginal) ??
					exifImageFile.Properties.Get<ExifDateTime>(ExifTag.DateTimeDigitized) ?? null;

				if(dateTime is null)
				{
					return null;
				}

				return _dateTimeValidator.IsValidYear((DateTime)dateTime) ? dateTime : null;
			}
			catch (NotValidImageFileException)
			{ 
			}
			catch (Exception)
			{
			}

			return null;
		}
	}
}
