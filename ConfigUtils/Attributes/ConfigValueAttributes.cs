namespace ConfigUtils.Attributes
{
	/// <summary>
	/// Specifies that this configuration option must be changed and cannot be the type's default value (null).
	/// </summary>
	public class RequiredAttribute : RequirementAttributeBase
	{
		public override bool MeetsRequirement(object value) => value != default;
		public override string GetReason() => "This value is required.";
	}

	/// <summary>
	/// Specifies that the integer contained within this configuration option must be within the specified range (inclusive).
	/// </summary>
	public class IntRangeAttribute : RequirementAttributeBase
	{
		public int Begin { get; set; }
		public int End { get; set; }

		public IntRangeAttribute(int begin, int end)
		{
			Begin = begin;
			End = end;
		}
		public override bool MeetsRequirement(object value) => (value is int @int) && @int >= Begin && @int <= End;
		public override string GetReason() => $"Value out of range. Must be at least {Begin} and at most {End}";
	}
}
