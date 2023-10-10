using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PicChron.Core;

namespace PicChron.Application
{
	public class FileInfoProvider : IFileInfoProvider
	{
		public string GetFileName(string filePath)
		{
			var fileInfo = new FileInfo(filePath);

			return fileInfo.Name;
		}
	}
}
