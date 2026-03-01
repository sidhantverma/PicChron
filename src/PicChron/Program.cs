using CommandLine;
using PicChron.Application;
using PicChron.Core;

namespace PicChron
{
	public class Program
	{
		private const int ExitCodeSuccess = 0;
		private const int ExitCodeCancelled = 1;
		private const int ExitCodeError = 2;

		static List<(string fileName, string exMsg)> unprocessedFiles = new List<(string fileName, string exMsg)>();

		static int Main(string[] args)
		{
			return Parser.Default.ParseArguments<PicChronOptions>(args)
				  .MapResult(
					(options) =>
					{
						return RunAndReturnExitCode(options).GetAwaiter().GetResult();
					},
					_ => ExitCodeError);
		}

		private async static Task<int> RunAndReturnExitCode(PicChronOptions options)
		{
			if (string.IsNullOrEmpty(options.DestinationPath))
			{
				options.DestinationPath = options.SourcePath;
			}

			if (!Directory.Exists(options.SourcePath))
			{
				Console.WriteLine("Error: Source path does not exist: {0}", options.SourcePath);
				return ExitCodeError;
			}

			if (!Directory.Exists(options.DestinationPath))
			{
				// Create destination directory if it doesn't exist
				try
				{
					Directory.CreateDirectory(options.DestinationPath);
				}
				catch (Exception ex)
				{
					Console.WriteLine("Error: Could not create destination directory: {0}", options.DestinationPath);
					Console.WriteLine(ex.ToString());
					return ExitCodeError;
				}
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
		

			// Warn user if using Move mode
			if (options.FileTransferMode == FileTransferMode.Move)
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("\n*** WARNING: Move mode will DELETE original files after organizing them.");
				Console.ResetColor();
			}

			Console.WriteLine("\nPress 'Y' to start sorting or any other key to cancel.");

			if (Console.ReadKey(true).Key == ConsoleKey.Y)
			{
				await picChroner.StartSorting();
				return ExitCodeSuccess;
			}
			else
			{
				Console.WriteLine("\nOperation cancelled.");
				return ExitCodeCancelled;
			}
		}

		private static void Sorter_PercentageCompleted(object? sender, int e)
		{
			Console.Write($"\rProgress: {e}%                    ");
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
		Console.WriteLine($"\n[OK] {message}");
		if (unprocessedFiles.Count() > 0)
		{
			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("Files not processed ({0}):", unprocessedFiles.Count);
			Console.ResetColor();
			foreach (var (name, msg) in unprocessedFiles.Take(10))
			{
				Console.WriteLine($"  • {name}: {msg}");
			}
			if (unprocessedFiles.Count > 10)
			{
				Console.WriteLine($"  ... and {unprocessedFiles.Count - 10} more files");
			}
		}
	}
}
}