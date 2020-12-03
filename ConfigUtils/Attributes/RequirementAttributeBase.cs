using System;

namespace ConfigUtils.Attributes
{
	/// <summary>
	/// Base class for config value requirement attributes.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public abstract class RequirementAttributeBase : Attribute
	{
		/// <summary>
		/// Checks whether the given value meets this requirement.
		/// </summary>
		/// <returns></returns>
		public abstract bool MeetsRequirement(object value);

		/// <summary>
		/// Returns user-friendly text describing this attribute's requirement.
		/// </summary>
		/// <returns></returns>
		public abstract string GetReason();
	}
}
