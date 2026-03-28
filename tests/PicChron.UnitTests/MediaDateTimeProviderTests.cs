using PicChron.Application;
using PicChron.Core;
using System.Reflection;

namespace PicChron.UnitTests
{
	public class MediaDateTimeProviderTests
	{
		[Fact]
		public void QuickTimeFormatParsing_ShouldParseCorrectly()
		{
			// This test validates that DateTime.TryParseExact works with QuickTime format
			string quickTimeDate = "Sat Feb 07 18:34:50 2026";
			string[] dateFormats = new string[]
			{
				"ddd MMM dd HH:mm:ss yyyy",  // Sat Feb 07 18:34:50 2026
				"yyyy-MM-dd HH:mm:ss",
				"yyyy:MM:dd HH:mm:ss",
				"o",
			};

			bool parsed = DateTime.TryParseExact(
				quickTimeDate,
				dateFormats,
				System.Globalization.CultureInfo.InvariantCulture,
				System.Globalization.DateTimeStyles.None,
				out var result
			);

			Assert.True(parsed, $"Failed to parse QuickTime date format: {quickTimeDate}");
			Assert.Equal(2026, result.Year);
			Assert.Equal(2, result.Month);
			Assert.Equal(7, result.Day);
			Assert.Equal(18, result.Hour);
			Assert.Equal(34, result.Minute);
			Assert.Equal(50, result.Second);
		}

		[Fact]
		public void QuickTimeFormatParsing_WithTryParse_ShouldFail()
		{
			// This test validates that DateTime.TryParse does NOT work with QuickTime format
			// This was the bug we fixed - TryParse() fails on the format "Sat Feb 07 18:34:50 2026"
			string quickTimeDate = "Sat Feb 07 18:34:50 2026";

			bool parsed = DateTime.TryParse(
				quickTimeDate,
				System.Globalization.CultureInfo.InvariantCulture,
				System.Globalization.DateTimeStyles.None,
				out var result
			);

			// This should fail with TryParse (unless the system culture happens to work)
			// But TryParseExact with the proper format should succeed
			Assert.False(parsed, "TryParse should fail on QuickTime format in most cultures");
		}
	}
}
