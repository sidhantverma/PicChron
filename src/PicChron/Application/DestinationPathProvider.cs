using PicChron.Core;

namespace PicChron.Application
{
	public class DestinationPathProvider : IDestinationPathProvider
	{
		private readonly string _path;
		public DestinationPathProvider(string path)
		{
			_path = path;
		}
		public (string destinationDirPath, string destinationFilePath) GetDestinationPaths(FileInfo file, DateTime dateTime)
		{
			var year = dateTime.Year.ToString();
			var date = dateTime.ToString("yyyy-MM-dd");

			var destDirPath = Path.Combine(_path, year, date);
			var destFilePath = Path.Combine(_path, year, date, file.Name);

			return (destDirPath, destFilePath);
		}
	}
}
