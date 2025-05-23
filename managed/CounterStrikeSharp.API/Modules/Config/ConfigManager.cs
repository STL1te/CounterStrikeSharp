/*
 *  This file is part of CounterStrikeSharp.
 *  CounterStrikeSharp is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  CounterStrikeSharp is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with CounterStrikeSharp.  If not, see <https://www.gnu.org/licenses/>. *
 */

using System.Reflection;
using System.Text;
using System.Text.Json;
using CounterStrikeSharp.API.Core.Logging;
using Microsoft.Extensions.Logging;
using Tomlyn;

namespace CounterStrikeSharp.API.Modules.Config
{
    enum ConfigType
    {
        Json,
        Toml
    }

    public static class ConfigManager
    {
        private static readonly DirectoryInfo? _rootDir;

        private static readonly string _pluginConfigsFolderPath;
        private static ILogger _logger = CoreLogging.Factory.CreateLogger("ConfigManager");

        internal static JsonSerializerOptions JsonSerializerOptions { get; } = new()
        {
            WriteIndented = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        internal static TomlModelOptions TomlModelOptions { get; } = new()
        {
            ConvertPropertyName = name => name
        };

        static ConfigManager()
        {
            _rootDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.Parent;
            _pluginConfigsFolderPath = Path.Combine(_rootDir.FullName, "configs", "plugins");
        }

        public static T Load<T>(string pluginName) where T : class, IBasePluginConfig, new()
        {
            string directoryPath = Path.Combine(_pluginConfigsFolderPath, pluginName);
            string configPath = Path.Combine(directoryPath, $"{pluginName}");
            string exampleConfigPath = Path.Combine(directoryPath, $"{pluginName}.example");

            string[] configFilePaths =
            [
                $"{configPath}.toml",
                $"{configPath}.json",
            ];

            foreach (var path in configFilePaths)
            {
                if (File.Exists(path))
                {
                    return Deserialize<T>(path);
                }
            }

            string[] exampleFilePaths =
            [
                $"{exampleConfigPath}.toml",
                $"{exampleConfigPath}.json"
            ];

            foreach (var path in exampleFilePaths)
            {
                if (!File.Exists(path)) continue;

                try
                {
                    _logger.LogInformation("Copying example configuration file for {PluginName}", pluginName);
                    var destPath = Path.Combine(directoryPath, Path.GetFileName(path).Replace(".example", ""));
                    File.Copy(path, destPath);
                    return Deserialize<T>(destPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to copy example configuration file for {PluginName}", pluginName);
                }
            }

            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var config = new T();
                var output = Serialize(config, ConfigType.Json, pluginName);
                File.WriteAllText(Path.Combine(directoryPath, $"{pluginName}.json"), output);

                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate configuration file for {PluginName}", pluginName);
                return new T();
            }
        }

        private static T Deserialize<T>(string path) where T : class, IBasePluginConfig, new()
        {
            switch (Path.GetExtension(path))
            {
                case ".toml":
                    return Toml.ToModel<T>(File.ReadAllText(path), options: TomlModelOptions);
                case ".json":
                    return JsonSerializer.Deserialize<T>(File.ReadAllText(path), JsonSerializerOptions)!;
            }

            throw new NotSupportedException("Unsupported configuration file format");
        }

        private static string Serialize<T>(T config, ConfigType configType, string pluginName) where T : class, IBasePluginConfig, new()
        {
            StringBuilder builder = new StringBuilder();
            string comment =
                $"This configuration was automatically generated by CounterStrikeSharp for plugin '{pluginName}', at {DateTimeOffset.Now:yyyy/MM/dd hh:mm:ss}\n";

            switch (configType)
            {
                case ConfigType.Json:
                    builder.Append($"// {comment}");
                    builder.Append(JsonSerializer.Serialize<T>(config, JsonSerializerOptions));
                    break;
                case ConfigType.Toml:
                    builder.Append($"# {comment}");
                    builder.Append(Toml.FromModel(config, options: TomlModelOptions));
                    break;
                default:
                    throw new NotSupportedException("Unsupported configuration file format");
            }

            return builder.ToString();
        }
    }
}
