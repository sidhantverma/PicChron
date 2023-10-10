using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicChron.Core
{
	public class PicChronOptions
	{
		[Option('s', Required = true)]
		public  string SourcePath { get; set; } = string.Empty;

		[Option('d', Required = false)]
		public string DestinationPath { get; set; } = string.Empty;

		[Option('m', Required = false, Default = FileTransferMode.Move)]
		public FileTransferMode FileTransferMode { get; set; }

		[Option('t', Required = false, Default = ScanType.Current)]
		public ScanType ScanType { get; set; }

		[Option('r', Required = false, Default = false)]
		public bool RewriteFileAccessAndWriteTime { get; set; }
	}
}
