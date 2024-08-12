using System;

namespace Project.Helpers.Attributes
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	public class NamespacePathAttribute : Attribute
	{
		public string Path;

		public NamespacePathAttribute(string path) => this.Path = path;
	}
}
