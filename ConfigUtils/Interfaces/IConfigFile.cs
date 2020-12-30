using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigUtils.Interfaces
{
	public interface IConfigFile
	{
		/// <inheritdoc cref="ConfigFile.Filepath"/>
		public string Filepath {get; set;}

		/// <inheritdoc cref="ConfigFile.Write(string, bool)"/>
		public void Write(string path = null, bool overwrite = true);

		/// <inheritdoc cref="ConfigFile.Load(string)"/>
		public LoadResult Load(string path = null);

		/// <inheritdoc cref="ConfigFile.Load(JObject)"/>
		public LoadResult Load(JObject configJson);

		/// <inheritdoc cref="ConfigFile.LoadAndRepair(string, bool)"/>
		public LoadResult LoadAndRepair(string path = null, bool createBackup = true);
	}
}
