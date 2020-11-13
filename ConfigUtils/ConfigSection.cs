using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Configuration
{
	public abstract class ConfigSection
	{
		/// <summary>
		/// Writes the fields contained in this configuration section to the given JsonTextWriter 
		/// </summary>
		/// <param name="writer">The JsonTextWriter to write the section to.</param>
		/// <param name="containingProperty">The PropertyInfo object of the ConfigFile property that contains this section.</param>
		internal void Write(JsonTextWriter writer, PropertyInfo containingProperty)
		{
			string indentation = new string(writer.IndentChar, writer.Indentation * 2);

			writer.WritePropertyName(containingProperty.Name);
			writer.WriteStartObject();

			//Get all fields this configuration class contains
			foreach (PropertyInfo P in this.GetType().GetProperties())
			{
				//If this field has a comment attribute, add a comment now.
				CommentAttribute Attr = P.GetCustomAttribute<CommentAttribute>();
				if (Attr != null)
				{
					writer.WriteWhitespace($"\n{indentation}");
					writer.WriteComment(Attr.Comment.Replace("\n", $"\n{indentation}"));
				}

				//Write the field. If the field is an IList, write it as a JSON array instead of a regular field.
				writer.WritePropertyName(P.Name);
				if (P.PropertyType.IsGenericType && P.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
				{
					writer.WriteStartArray();
					var L = (IList)P.GetValue(this);
					foreach (object Entry in L)
					{
						writer.WriteValue(Entry);
					}
					writer.WriteEndArray();
				}
				else
				{
					writer.WriteValue(P.GetValue(this));
				}
			}
			writer.WriteEndObject();
		}

		/// <summary>
		/// Loads this configuration section from the given configuration file.
		/// </summary>
		/// <param name="configJson">A JObject containing the configuration file.</param>
		/// <param name="containingProperty">The PropertyInfo object of the ConfigFile property that contains this section.</param>
		/// <returns></returns>
		internal List<string> Load(JObject configJson, PropertyInfo containingProperty)
		{
			// If a section is missing, add the sections amount of fields to `missing`
			if (!configJson.ContainsKey(containingProperty.Name))
				return (from P in this.GetType().GetProperties() select $"{containingProperty.Name}.{P.Name}").ToList();

			// Get the config section and look for missing keys in said section
			List<string> missing = new();
			var section = (JObject)configJson[containingProperty.Name];
			foreach (PropertyInfo prop in this.GetType().GetProperties())
			{
				// Increment `missing` if a field is missing
				if (!section.ContainsKey(prop.Name))
				{
					missing.Add($"{containingProperty.Name}.{prop.Name}");
					continue;
				}
				// Convert the JValue to the field's type and set the field. This also serves as a typecheck.
				prop.SetValue(this, section[prop.Name].ToObject(prop.PropertyType));
			}
			return missing;
		}
	}
}
