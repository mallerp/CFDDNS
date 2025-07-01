using System.IO;
using System.Text.Json;
using System.Diagnostics;

namespace CFDDNS
{
    public static class ConfigManager
    {
        private static readonly string ConfigPath = GetConfigPath();
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        private static string GetConfigPath()
        {
            // For single-file executable, AppContext.BaseDirectory is a temporary folder.
            // We want the config file to be next to the .exe itself.
            var exePath = Process.GetCurrentProcess().MainModule?.FileName;
            var exeDir = Path.GetDirectoryName(exePath);
            
            if (!string.IsNullOrEmpty(exeDir))
            {
                return Path.Combine(exeDir, "config.json");
            }

            // Fallback for scenarios where getting exe path fails
            return Path.Combine(AppContext.BaseDirectory, "config.json");
        }

        public static AppConfig LoadConfig()
        {
            if (!File.Exists(ConfigPath))
            {
                // Create a default config if it doesn't exist
                var defaultConfig = new AppConfig();
                defaultConfig.Domains.Add(new DomainConfig()); // Add one example domain
                SaveConfig(defaultConfig);
                return defaultConfig;
            }

            var json = File.ReadAllText(ConfigPath);
            return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
        }

        public static void SaveConfig(AppConfig config)
        {
            var json = JsonSerializer.Serialize(config, JsonOptions);
            File.WriteAllText(ConfigPath, json);
        }
    }
} 