using System;
#pragma warning disable CA1801 // Review unused parameters
namespace DNSMadeEasy.Json
{
	[AttributeUsage(AttributeTargets.Constructor)]
	public sealed class JsonConstructorAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Struct)]
	public sealed class TinyTypeAttribute : Attribute
	{

		public TinyTypeAttribute(Type type)
		{
		}
	}

	[AttributeUsage(AttributeTargets.Method)]
	public sealed class JsonParseMethodAttribute : Attribute
	{
		public JsonParseMethodAttribute()
		{
		}

		public JsonParseMethodAttribute(string writeMethod)
		{
		}
	}
}
