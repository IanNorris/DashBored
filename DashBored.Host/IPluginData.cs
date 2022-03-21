namespace DashBored.Host
{
	public interface IPluginData
	{
		public static void AddType(Type type)
		{
			lock (KnownTypes)
			{
				KnownTypes.Add(type);
			}
		}

		protected static HashSet<Type> KnownTypes = new HashSet<Type>();
	}
}
