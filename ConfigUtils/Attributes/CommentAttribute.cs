using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigUtils.Attributes
{
	/// <summary>
	/// Adds a comment to a configuration value.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class CommentAttribute : Attribute
	{
		public readonly string Comment;
		public CommentAttribute(string Comment)
		{
			this.Comment = Comment;
		}
	}
}
