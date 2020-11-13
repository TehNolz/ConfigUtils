using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Configuration
{
	/// <summary>
	/// Base class for configuration files.
	/// </summary>
	public abstract class ConfigFile
	{
		/// <summary>
		/// The configuration file's file path.
		/// </summary>
		public string Filepath { get; private set; }

		/// <summary>
		/// Writes the configuration file to disk. If no path is given, the path that was used to previously load/write this configuration file will be used instead.
		/// </summary>
		/// <param name="path">The file the configuration file will be written to.</param>
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
			foreach (PropertyInfo P in from T in this.GetType().GetProperties() where T.PropertyType.IsSubclassOf(typeof(ConfigSection)) select T)
				((ConfigSection)P.GetValue(this)).Write(writer, P);

			writer.WriteEndObject();
			File.WriteAllText(path, SW.ToString());
		}

		/// <summary>
		/// Load the configuration file at the specified path. The path will be saved for future reference.
		/// </summary>
		/// <param name="path">The path of the configuration file to load.</param>
		/// <returns>The amount of missing values.</returns>
		/// <exception cref="FileNotFoundException"/>
		/// <exception cref="JsonReaderException">The file at the given <paramref name="path"/> is not a valid JSON file.</exception>
		/// <returns>The amount of missing values.</returns>
		public List<string> Load(string path)
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
		public List<string> Load(JObject configJson)
		{
			//Null checks
			if (configJson is null)
				throw new ArgumentNullException(nameof(configJson));

			// Load all configuration settings, counting all missing fields in the process.
			List<string> missing = new();
			foreach (PropertyInfo P in from T in this.GetType().GetProperties() where T.PropertyType.IsSubclassOf(typeof(ConfigSection)) select T)
				missing.AddRange(((ConfigSection)P.GetValue(this)).Load(configJson, P));
			return missing;
		}

		/// <summary>
		/// Loads the config file at the specified path, repairing it if necessary.
		/// </summary>
		/// <param name="path">The path of the configuration file to load.</param>
		/// <param name="createBackup">If true, a backup of the configuration file will be created before it is repaired.</param>
		/// <exception cref="FileNotFoundException"/>
		/// <returns>A list of missing or invalid configuration settings. Null if the specified file is not a valid JSON file.</returns>
		public List<string> LoadAndRepair(string path, bool createBackup = true)
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
			bool isFaulty;
			List<string> missing = null;
			try
			{
				missing = Load(path);
				isFaulty = !missing.Any();
			}
			catch (JsonReaderException)
			{
				isFaulty = true;
			}
			catch (FileNotFoundException){
				//Can't make a backup of a file that doesn't exist.
				createBackup = false;
				isFaulty = true;
			}

			//If the file turns out to be faulty, repair it.
			if (isFaulty)
			{
				//Create a backup of the faulty config file if requested.
				if (createBackup)
				{
					int fileCount = Directory.GetFiles("Logs", "config-backup-*.json").Length;
					File.Copy(path, Path.Combine(Path.GetDirectoryName(path), $"config-backup-{fileCount}.json"));
				}

				Write(path);
			}
			return missing;
		}
	}
}