using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConfigUtils {
	public static partial class Config {

		/// <summary>
		/// Get the current settings as JObject.
		/// </summary>
		/// <returns></returns>
		public static JObject GetCurrentSettings() {
			//Create writers
			using StringWriter SW = new StringWriter();
			using JsonTextWriter writer = new JsonTextWriter(SW) {
				Formatting = Formatting.Indented,
			};

			string indentation = new string(writer.IndentChar, writer.Indentation * 2);

			//Build each section
			writer.WriteStartObject();
			foreach(Type T in GetMappingClasses()) {

				writer.WritePropertyName(T.Name);
				writer.WriteStartObject();

				//Get all fields
				foreach(FieldInfo F in T.GetFields()) {

					//If this field has a comment, write it now.
					CommentAttribute Attr = F.GetCustomAttribute<CommentAttribute>();
					if(Attr != null) {
						writer.WriteWhitespace($"\n{indentation}");
						writer.WriteComment(Attr.Comment.Replace("\n", $"\n{indentation}"));
					}

					//Write fields
					writer.WritePropertyName(F.Name);
					//If the field is a list, write it as an array.
					if(F.FieldType.IsGenericType && F.FieldType.GetGenericTypeDefinition() == typeof(List<>)) {
						writer.WriteStartArray();
						IList L = (IList)F.GetValue(null);
						foreach(object Entry in L) {
							writer.WriteValue(Entry);
						}
						writer.WriteEndArray();
					} else {
						writer.WriteValue(F.GetValue(null));
					}
				}
				writer.WriteEndObject();
			}
			writer.WriteEndObject();

			return JObject.Parse(SW.ToString());
		}

		/// <summary>
		/// Write the current configuration to file.
		/// </summary>
		/// <param name="path"></param>
		public static void Write(string path) => File.WriteAllText(path, GetCurrentSettings().ToString(Formatting.Indented));
	}
}
