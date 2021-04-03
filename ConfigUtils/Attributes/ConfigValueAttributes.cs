using System;
using System.Text.RegularExpressions;

namespace ConfigUtils.Attributes
{
	/// <summary>
	/// Specifies that this configuration option is required, and therefore must not be <see langword="null"/>
	/// </summary>
	public class RequiredAttribute : Attribute { }

	/// <summary>
	/// Specifies that the value must be within the specified range (inclusive).
	/// </summary>
	public class IntRangeAttribute : RequirementAttributeBase
	{
		public int Begin { get; }
		public int End { get; }

		public IntRangeAttribute(int begin, int end)
		{
			Begin = begin;
			End = end;
		}
		public override bool MeetsRequirement(object value) => (value is int @int) && @int >= Begin && @int <= End;

		public override string GetReason() => $"Value out of range. Must be at least {Begin} and at most {End}";
	}

	/// <summary>
	/// Specifies that the value must match the specified regex pattern.
	/// </summary>
	public class RegexAttribute : RequirementAttributeBase
	{
		public string Pattern { get; }
		public RegexAttribute(string pattern)
		{
			Pattern = pattern;
		}

		public override string GetReason() => $"Value does not match regex {Pattern}";

		public override bool MeetsRequirement(object value) => value != null && Regex.IsMatch(value as string, Pattern);
	}
}
