using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConfigUtils.Interfaces
{
	public interface IConfigSection
	{
		/// <inheritdoc cref="ConfigSection.Write(JsonTextWriter, PropertyInfo)"/>
		public void Write(JsonTextWriter writer, PropertyInfo containingProperty);

		/// <inheritdoc cref="ConfigSection.Load(JObject, PropertyInfo)"/>
		public LoadResult Load(JObject configJson, PropertyInfo containingProperty);

	}
}
