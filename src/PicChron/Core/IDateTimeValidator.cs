using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicChron.Core
{
	public interface IDateTimeValidator
	{
		bool IsValidYear(DateTime dateTime);
	}
}
