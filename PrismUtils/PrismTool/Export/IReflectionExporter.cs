using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Export
{
	public interface IReflectionExporter
	{
		string GenerateIncludeContent();
		string GenerateSourceContent();
	}
}
