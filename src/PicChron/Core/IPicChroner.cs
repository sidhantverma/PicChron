namespace PicChron.Core
{
	public interface IPicChroner
	{
		event EventHandler<Exception>? OnError;
		event EventHandler<int>? PercentageCompleted;
		event EventHandler<string>? SortCompleted;
		Task StartSorting();
	}
}