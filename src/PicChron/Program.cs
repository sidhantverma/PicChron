using CommandLine;
using PicChron.Application;
using PicChron.Core;

namespace PicChron
{
	public class Program
	{
		static List<(string fileName, string exMsg)> unprocessedFiles = new List<(string fileName, string exMsg)>();

		static int Main(string[] args)
		{
			return Parser.Default.ParseArguments<PicChronOptions>(args)
				  .MapResult(
					(options) =>
					{
						return RunAndReturnExitCode(options);
					},
					_ => 1);
		}

		private static int RunAndReturnExitCode(PicChronOptions options)
		{

			if (options.DestinationPath == String.Empty)
			{
				options.DestinationPath = options.SourcePath;
			}

			if (!Directory.Exists(options.SourcePath))
			{
				Console.WriteLine("Invalid value for source path given.");
				return 1;
			}

			var picChroner = new PicChroner(options);

			picChroner.PercentageCompleted += Sorter_PercentageCompleted;
			picChroner.OnError += Sorter_OnError;
			picChroner.SortCompleted += Sorter_SortCompleted;

			Console.WriteLine($"{nameof(options.SourcePath)}: {new DirectoryInfo(options.SourcePath).FullName}");
			Console.WriteLine($"{nameof(options.DestinationPath)}: {new DirectoryInfo(options.DestinationPath).FullName}");
			Console.WriteLine($"{nameof(options.FileTransferMode)}: {options.FileTransferMode}");
			Console.WriteLine($"{nameof(options.ScanType)}: {options.ScanType}");
			Console.WriteLine($"{nameof(options.RewriteFileAccessAndWriteTime)}: {options.RewriteFileAccessAndWriteTime}");

			Console.Write("Press Y to start or press any other key to cancel.");
			Console.WriteLine();

			if (Console.ReadKey().Key == ConsoleKey.Y)
			{
				picChroner.StartSorting();
				return 1;
			}
			else
			{
				return 0;
			}
		}

		private static void Sorter_PercentageCompleted(object? sender, int e)
		{
			Console.Clear();
			Console.WriteLine($"Completed: {e}%");
		}

		private static void Sorter_OnError(object? sender, Exception e)
		{
			//Console.WriteLine(e.ToString());
			if (sender is not null && sender is FileInfo)
			{
				var file = (FileInfo)sender;
				unprocessedFiles.Add((file.Name, e.Message));
			}
		}
		private static void Sorter_SortCompleted(object? sender, string message)
		{
			Console.WriteLine(message);
			if(unprocessedFiles.Count() > 0)
			{
				Console.WriteLine();
				Console.WriteLine("List of unprocessed files:");
				foreach (var (name, msg) in unprocessedFiles)
				{
					Console.WriteLine($"{name}:{msg}");
				}
			}
		}
	}
}