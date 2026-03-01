using PicChron.Core;
using System.Text.RegularExpressions;

namespace PicChron.Application
{
	public class RegexDateTimeProvider : IDateTimeProvider
	{
		private readonly IDateTimeValidator _dateTimeValidator;
		private readonly IFileInfoProvider _fileInfoProvider;

		/// <summary>
		/// Regex pattern to match YYYYMMDD format (8 consecutive digits) in filenames.
		/// This pattern is compiled for better performance.
		/// </summary>
		private static readonly Regex DatePattern = new Regex(@"(\d{8})", RegexOptions.Compiled);

		public RegexDateTimeProvider()
		{
			_dateTimeValidator = new DateTimeValidator();
			_fileInfoProvider = new FileInfoProvider();
		}

		public RegexDateTimeProvider(IDateTimeValidator dateTimeValidator, IFileInfoProvider fileInfoProvider)
		{
			_dateTimeValidator = dateTimeValidator;
			_fileInfoProvider = fileInfoProvider;
		}

		public async Task<DateTime?> GetDateTime(string filePath)
		{
			var fileName = _fileInfoProvider.GetFileName(filePath);

			if (!DatePattern.IsMatch(fileName))
			{
				return null;
			}

			var dateString = DatePattern.Match(fileName).Groups[0].Value;

			var result = await Task.Run(() =>
			{
				bool isParsed = false;
				try
				{
					isParsed = DateTime.TryParseExact(dateString, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var dateTimeParsed);
					if (isParsed && _dateTimeValidator.IsValidYear(dateTimeParsed))
					{
						return (DateTime?)dateTimeParsed;
					}
				}
				catch
				{
				}

				return null;
			});

			return result;
		}
	}
}
