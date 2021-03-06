using ConfigUtils.Attributes;
using ConfigUtils.Interfaces;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ConfigUtils
{
	public abstract class ConfigSection : IConfigSection
	{
		/// <summary>
		/// Writes the fields contained in this configuration section to the given JsonTextWriter 
		/// </summary>
		/// <param name="writer">The JsonTextWriter to write the section to.</param>
		/// <param name="containingProperty">The PropertyInfo object of the ConfigFile property that contains this section.</param>
		public void Write(JsonTextWriter writer, PropertyInfo containingProperty)
		{
			string indentation = new(writer.IndentChar, writer.Indentation * 2);

			writer.WritePropertyName(containingProperty.Name);
			writer.WriteStartObject();

			//Get all fields this configuration class contains
			foreach (PropertyInfo P in GetType().GetProperties())
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
		public LoadResult Load(JObject configJson, PropertyInfo containingProperty)
		{
			LoadResult result = new();

			// If a section is missing, add the sections amount of fields to `missing`
			if (!configJson.ContainsKey(containingProperty.Name))
			{
				foreach (string missing in from prop in GetType().GetProperties() select $"{containingProperty.Name}.{prop.Name}")
					result.AddMissing(missing);
				return result;
			}

			// Get the config section and look for missing keys in said section
			var section = (JObject)configJson[containingProperty.Name];
			foreach (PropertyInfo prop in GetType().GetProperties())
			{
				// Increment `missing` if a field is missing
				if (!section.ContainsKey(prop.Name))
				{
					result.AddMissing($"{containingProperty.Name}.{prop.Name}");
					continue;
				}

				object value = section[prop.Name].ToObject(prop.PropertyType);

				// If this value is null, check if that's allowed.
				if (value == null && Attribute.IsDefined(prop, typeof(RequiredAttribute)))
				{
					result.AddInvalid($"{containingProperty.Name}.{prop.Name}", "This value is required.");
					return result;
				}

				// Check if the value meets all its configured requirements.
				if (value != null)
				{
					foreach (RequirementAttributeBase attribute in from A in prop.GetCustomAttributes() where A.GetType().IsSubclassOf(typeof(RequirementAttributeBase)) select A)
					{
						if (!attribute.MeetsRequirement(value))
							result.AddInvalid($"{containingProperty.Name}.{prop.Name}", attribute.GetReason());
					}
				}

				// Convert the JValue to the field's type and set the field. This also serves as a typecheck.
				prop.SetValue(this, value);
				result.AddSuccessful($"{containingProperty.Name}.{prop.Name}");
			}
			return result;
		}
	}
}
