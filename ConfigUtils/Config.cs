using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ConfigUtils {
	public partial class Config {
		/// <summary>
		/// Get all mapping classes for the specified assembly.
		/// </summary>
		/// <param name="assembly"></param>
		/// <returns></returns>
		private static IEnumerable<Type> GetMappingClasses() {
			return from Type T in Assembly.GetEntryAssembly().GetTypes() where T.GetCustomAttribute<ConfigSectionAttribute>() != null select T;
		}

		/// <summary>
		/// Marks this class as a config mapping class. Any config setting will be mapped to this class.
		/// Multiple mapping classes are allowed.
		/// </summary>
		public class ConfigSectionAttribute : Attribute { }

		/// <summary>
		/// Adds a comment to a configuration value.
		/// </summary>
		[AttributeUsage(AttributeTargets.Field)]
		public class CommentAttribute : Attribute {
			public readonly string Comment;
			public CommentAttribute(string Comment) {
				this.Comment = Comment;
			}
		}
	}
}
