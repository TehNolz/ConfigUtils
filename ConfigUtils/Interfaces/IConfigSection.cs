using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Reflection;

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
