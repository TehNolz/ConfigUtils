using System;
using System.Collections;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace ConfigUtils {
	public static partial class Config {

		private static readonly Stack PreviousConfigs = new Stack();

		/// <summary>
		/// Read configuration settings from a file.
		/// </summary>
		/// <param name="path">The path to the JSON file containing the configuration file.</param>
		/// <param name="createBackup">Whether a backup of the existing settings should be created.</param>
		public static void Read(string path, bool createBackup = false) => Read(JObject.Parse(File.ReadAllText(path)), createBackup);

		/// <summary>
		/// Read configuration settings 
		/// </summary>
		/// <param name="json">A JObject containing a configuration file.</param>
		/// <param name="createBackup">Whether a backup of the existing settings should be created.</param>
		public static void Read(JObject json, bool createBackup = false) {
			//Create a backup of the current configuration settings if requested
			if(createBackup)
				PreviousConfigs.Push(GetCurrentSettings());

			ApplySettings(json);
		}

		/// <summary>
		/// Revert to the previous configuration.
		/// </summary>
		/// <returns>The amount of stored configuration files.</returns>
		public static int Revert() {
			if(PreviousConfigs.Count == 0)
				throw new ArgumentException("No previous configurations available.");

			ApplySettings((JObject)PreviousConfigs.Pop());
			return PreviousConfigs.Count;
		}

		/// <summary>
		/// Apply the given configuration settings.
		/// </summary>
		/// <returns>The amount of missing settings.</returns>
		/// <param name="json">A JObject containing a configuration file.</param>
		private static int ApplySettings(JObject json) {
			int missing = 0;

			// Loop through all types from the caller's assembly which have the ConfigSectionAttribute
			foreach(Type T in GetMappingClasses()) {
				if(!json.ContainsKey(T.Name)) {
					// If a section is missing, add the sections amount of fields to `missing`
					missing += T.GetFields().Length;
					continue;
				}

				// Get the config section and look for missing keys in said section
				JObject section = (JObject)json[T.Name];
				foreach(FieldInfo field in T.GetFields()) {
					// Increment `missing` if a field is missing
					if(!section.ContainsKey(field.Name)) {
						missing++;
						continue;
					}
					// Convert the JValue to the field's type and set the field. This also serves as a typecheck.
					field.SetValue(null, section[field.Name].ToObject(field.FieldType));
				}
			}
			return missing;
		}
	}
}
