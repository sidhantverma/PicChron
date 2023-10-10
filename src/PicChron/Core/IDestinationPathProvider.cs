namespace PicChron.Core
{
	public interface IDestinationPathProvider
	{
		(string destinationDirPath, string destinationFilePath) GetDestinationPaths(FileInfo file, DateTime dateTime);
	}
}
