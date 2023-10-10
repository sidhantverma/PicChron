using PicChron.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicChron.Application
{
	public class DateTimeValidator : IDateTimeValidator
	{
		public bool IsValidYear(DateTime dateTime)
		{
			return dateTime.Year > 1900 && dateTime.Year <= DateTime.Now.Year;
		}
	}
}
