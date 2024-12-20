﻿using System.Reflection;
using DashBored.PluginApi;
using Newtonsoft.Json.Linq;

namespace DashBored.Host
{
	public class PluginLoader
	{
		public PluginLoader()
		{
			LoadPlugins();
		}

		private void LoadPlugins()
		{
			var mainAssembly = Assembly.GetExecutingAssembly();
			LoadPluginsForAssembly(mainAssembly);

			var Plugins = Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "Plugin.*.dll");
			foreach (var Plugin in Plugins)
			{
				var assembly = Assembly.LoadFrom(Plugin);
				LoadPluginsForAssembly(assembly);
			}
		}

		public IPlugin CreateInstance(IServiceProvider serviceProvider, string typeName, JObject pluginData, string title)
		{
			if (_pluginTypes.TryGetValue(typeName, out var pluginType))
			{
				var dataType = pluginType.GetProperty("DataType", BindingFlags.Static | BindingFlags.Public);
				var pluginDataType = (Type)dataType.GetValue(null);

				return (IPlugin)CreateInstanceWithDependencyInjection(serviceProvider, pluginType, pluginDataType, pluginData, title);
			}
			else
			{
				throw new InvalidDataException($"Plugin {typeName} was not found.");
			}
		}

		private object CreateInstanceWithDependencyInjection(IServiceProvider serviceProvider, Type type, Type dataType, JObject pluginData, string title)
		{
			var constructors = type.GetConstructors();
			foreach (var constructor in constructors.OrderByDescending(c => c.GetParameters().Length))
			{
				var parameters = constructor.GetParameters();

				bool success = true;
				int parameterIndex = 0;
				object[] parametersOut = new object[parameters.Length];
				foreach (var parameter in parameters)
				{
					if(parameter.ParameterType == dataType)
					{
						var convertedPluginData = pluginData.ToObject(dataType);

						parametersOut[parameterIndex] = convertedPluginData;
					}

					if(parameter.ParameterType == typeof(string))
					{
						parametersOut[parameterIndex] = title ?? "";
					}

					if (parametersOut[parameterIndex] == null)
					{
						object resultingObject = serviceProvider.GetType().GetMethod("GetService").Invoke(serviceProvider, new object[] { parameter.ParameterType });
						parametersOut[parameterIndex] = resultingObject;
					}

					if (parametersOut[parameterIndex] == null)
					{
						success = false;
						break;
					}

					parameterIndex++;
				}

				if (success)
				{
					return Activator.CreateInstance(type, parametersOut);
				}
			}

			throw new InvalidDataException($"Unable to create instance of {type.Name}, no suitable construtor that can be filled with dependency injection.");
		}

		private void LoadPluginsForAssembly(Assembly assembly)
		{
			var assemblyTypes = assembly.GetTypes();

			var types = assemblyTypes.Where(t => t.IsAssignableTo(typeof(IPlugin)));
			foreach (var type in types)
			{
				_pluginTypes.Add(type.Name, type);
			}
		}

		private Dictionary<string, Type> _pluginTypes = new Dictionary<string, Type>();
	}
}
