using ConfigUtils.Attributes;

using System.Collections.Generic;

/// <summary>
/// Configuration file template for unit tests
/// </summary>
namespace ConfugUtils.Tests
{
	public class Config : ConfigFile
	{
		public Section1Config Section1 { get; private set; } = new();
		public class Section1Config : ConfigSection
		{
			[Comment("Hello World")]
			public int Value { get; set; } = 100;

			public List<string> Values { get; set; } = new List<string>() { "aaa", "bbb", "ccc" };
		}

		public Section2Config Section2 { get; private set; } = new();
		public class Section2Config : ConfigSection
		{
			[Comment("Comment1")]
			public string Value { get; set; } = "Hello World!";

			public string Value2 { get; set; } = "Wheee!";
		}
	}
}
