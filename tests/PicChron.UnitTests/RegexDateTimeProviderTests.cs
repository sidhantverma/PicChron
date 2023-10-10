using PicChron.Application;
using FakeItEasy;
using PicChron.Core;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PicChronTests
{
	public class RegexDateTimeProviderTests
	{

		public static IEnumerable<object[]> TestData =>
		new List<object[]>
		{
			new object[] { @"F:\VID20231009000509.mp4", "VID20231009000509.mp4", new DateTime(2023, 10, 09) },
			new object[] { @"F:\IMG20231009000317_20231010111254.jpg", "IMG20231009000317_20231010111254.jpg", new DateTime(2023, 10, 09) },
			new object[] { @"F:\VID-20231009-WA0001.mp4", "VID-20231009-WA0001.mp4", new DateTime(2023, 10, 09) },
			new object[] { @"F:\IMG_20231009_174510.jpg", "IMG_20231009_174510.jpg", new DateTime(2023, 10, 09) }
		};

		[Theory]
		[MemberData(nameof(TestData))]
		public void GetDateTime_ReturnsValidDateTime(string filePath, string fileName, DateTime? dateTimeParsed)
		{
			// Arrange
			IDateTimeValidator _dateTimeValidator = A.Fake<IDateTimeValidator>();
			A.CallTo(() => _dateTimeValidator.IsValidYear(A<DateTime>.Ignored)).Returns(true);

			IFileInfoProvider _fileInfoProvider = A.Fake<IFileInfoProvider>();
			A.CallTo(() => _fileInfoProvider.GetFileName(filePath)).Returns(fileName);

			RegexDateTimeProvider _regexDateTimeProvider = new RegexDateTimeProvider(_dateTimeValidator, _fileInfoProvider);


			// Act
			var result = _regexDateTimeProvider.GetDateTime(filePath).GetAwaiter().GetResult();


			// Assert
			Assert.Equal(dateTimeParsed, result);
		}
	}
}