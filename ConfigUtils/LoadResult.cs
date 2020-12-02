using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigUtils
{
	/// <summary>
	/// Stores the result of a load operation. Contains information about which settings were missing or invalid.
	/// </summary>
	public class LoadResult
	{
		private List<(LoadResultType, string, string)> Results {get; } = new ();

		/// <summary>
		/// Returns a list of all missing settings.
		/// </summary>
		public IEnumerable<(string, string)> Missing => from E in Results where E.Item1 == LoadResultType.Missing select (E.Item2, E.Item3);
		/// <summary>
		/// Returns a list of all invalid settings which did not pass the condition set by their attribute(s).
		/// </summary>
		public IEnumerable<(string, string)> Invalid => from E in Results where E.Item1 == LoadResultType.Invalid select (E.Item2, E.Item3);
		/// <summary>
		/// Returns a list of all valid settings.
		/// </summary>
		public IEnumerable<string> Success => from E in Results where E.Item1 == LoadResultType.Success select E.Item2;

		/// <summary>
		/// Indicates whether the specified configuration file was found to be valid, incomplete, or corrupt, during the load action. 
		/// </summary>
		public FileStatus FileStatus { get; internal set;}
		/// <summary>
		/// <see langword="true"/>if the file was repaired as part of a LoadndRepair operation."/>
		/// </summary>
		public bool Repaired { get; internal set;}

		/// <summary>
		/// Registers a setting as missing.
		/// </summary>
		/// <param name="setting"></param>
		internal void AddMissing(string setting) => Results.Add((LoadResultType.Missing, setting, null));
		
		/// <summary>
		/// Registers a setting as present and valid.
		/// </summary>
		/// <param name="setting"></param>
		internal void AddSuccessful(string setting) => Results.Add((LoadResultType.Success, setting, null));

		/// <summary>
		/// Registers a setting as invalid due to the given reason.
		/// </summary>
		/// <param name="setting"></param>
		/// <param name="reason"></param>
		internal void AddInvalid(string setting, string reason) => Results.Add((LoadResultType.Invalid, setting, reason));

		/// <summary>
		/// Merges a second LoadResult into the current object.
		/// </summary>
		/// <param name="other"></param>
		internal void Merge(LoadResult other) => Results.AddRange(other.Results);
	}

	public enum LoadResultType {
		Missing, //Setting was missing
		Invalid, //Setting value doesn't meet requirements
		Success  //Setting value is OK
	}

	public enum FileStatus {
		Valid,			//No issues detected
		Invalid,		//Some missing or invalid settings detected
		ReadFailed,		//Could not be read
	}
}
