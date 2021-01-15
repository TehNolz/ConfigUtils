using ConfigUtils.Interfaces;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ConfigUtils
{
	/// <summary>
	/// Base class for configuration files.
	/// </summary>
	public abstract class ConfigFile : IConfigFile
	{
		/// <summary>
		/// The configuration file's file path.
		/// </summary>
		public abstract string Filepath { get; set; }

		/// <summary>
		/// Writes the configuration file to disk. If no path is given, the path that was used to previously load/write this configuration file will be used instead.
		/// </summary>
		/// <param name="path">The file the configuration file will be written to. If set, this will change the config file's <see cref="Filepath"/> property, indicating its new location.</param>
		/// <param name="overwrite">If true, the configuration file will be written even if a file already exists at the specified location.</param>
		public void Write(string path = null, bool overwrite = true)
		{
			//Check if a valid path was given.
			if (string.IsNullOrWhiteSpace(path))
			{
				if (string.IsNullOrWhiteSpace(Filepath))
					throw new ArgumentNullException(path);
				path = Filepath;
			}
			Filepath = path;

			//If a file already exists at this path and overwrite is false, throw an exception.
			if (!overwrite && File.Exists(path))
				throw new ArgumentException("A configuration file already exists at this path.");

			//Create writers.
			using var SW = new StringWriter();
			using var writer = new JsonTextWriter(SW)
			{
				Formatting = Formatting.Indented,
			};
			writer.WriteStartObject();

			//Start writing all configuration sections
			foreach (PropertyInfo P in from T in GetType().GetProperties() where T.PropertyType.IsSubclassOf(typeof(ConfigSection)) select T)
				((ConfigSection)P.GetValue(this)).Write(writer, P);

			writer.WriteEndObject();
			File.WriteAllText(path, SW.ToString());
		}

		/// <summary>
		/// Load the configuration file at the specified path. The path will be saved for future reference.
		/// </summary>
		/// <param name="path">The file the configuration file will be loaded from. If set, this will change the config file's <see cref="Filepath"/> property, indicating its new location.</param>
		/// <returns>The amount of missing values.</returns>
		/// <exception cref="FileNotFoundException"/>
		/// <exception cref="JsonReaderException">The file at the given <paramref name="path"/> is not a valid JSON file.</exception>
		/// <returns>The amount of missing values.</returns>
		public LoadResult Load(string path = null)
		{
			//Check if a valid path was given.
			if (string.IsNullOrWhiteSpace(path))
			{
				if (string.IsNullOrWhiteSpace(Filepath))
					throw new ArgumentNullException(path);
				path = Filepath;
			}
			Filepath = path;

			//Load the config file
			return Load(JObject.Parse(File.ReadAllText(path)));
		}

		/// <summary>
		/// Loads the given configuration file.
		/// </summary>
		/// <param name="configJson"><inheritdoc cref="Load(JObject)"/></param>
		/// <returns>The amount of missing values.</returns>
		public LoadResult Load(JObject configJson)
		{
			//Null checks
			if (configJson is null)
				throw new ArgumentNullException(nameof(configJson));

			// Load all configuration settings, counting all missing fields in the process.
			LoadResult result = new();
			foreach (PropertyInfo P in from T in GetType().GetProperties() where T.PropertyType.IsSubclassOf(typeof(ConfigSection)) select T)
				result.Merge(((ConfigSection)P.GetValue(this)).Load(configJson, P));

			result.FileStatus = result.Missing.Any() || result.Invalid.Any() ? FileStatus.Invalid : FileStatus.Valid;
			return result;
		}

		/// <summary>
		/// Loads the config file at the specified path, repairing it if necessary.
		/// </summary>
		/// <param name="path">The path of the configuration file to load.</param>
		/// <param name="createBackup">If true, a backup of the configuration file will be created before it is repaired.</param>
		/// <exception cref="FileNotFoundException"/>
		/// <returns>A list of missing or invalid configuration settings. Null if the specified file is not a valid JSON file.</returns>
		public LoadResult LoadAndRepair(string path = null, bool createBackup = true)
		{
			//Check if a valid path was given.
			if (string.IsNullOrWhiteSpace(path))
			{
				if (string.IsNullOrWhiteSpace(Filepath))
					throw new ArgumentNullException(path);
				path = Filepath;
			}
			Filepath = path;

			//Load the configuration file and repair it if any settings are missing.
			LoadResult result = new();
			try
			{
				result = Load(path);
				result.FileStatus = result.Missing.Any() || result.Invalid.Any() ? FileStatus.Invalid : FileStatus.Valid;
			}
			catch (JsonReaderException)
			{
				result.FileStatus = FileStatus.ReadFailed;
			}
			catch (FileNotFoundException)
			{
				//Can't make a backup of a file that doesn't exist.
				createBackup = false;
				result.FileStatus = FileStatus.ReadFailed;
			}

			//If the file turns out to be faulty, repair it and attempt to load it again.
			if (result.FileStatus is FileStatus.Invalid or FileStatus.ReadFailed)
			{
				//Create a backup of the faulty config file if requested.
				if (createBackup)
				{
					int fileCount = Directory.GetFiles(".", "config-backup-*.json").Length;
					File.Copy(path, Path.Combine(Path.GetDirectoryName(path), $"config-backup-{fileCount}.json"));
				}

				//Write the defaults.
				Write(path);

				//Attempt to load the newly created file. This will fail if some settings don't have a default value, but that's OK.
				result = Load(path);
				result.FileStatus = result.Missing.Any() || result.Invalid.Any() ? FileStatus.Invalid : FileStatus.Valid;
			}
			return result;
		}
	}
}
