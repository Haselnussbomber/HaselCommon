using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using HaselCommon.Interfaces;
using HaselCommon.Services;

namespace HaselCommon;

public static class ConfigurationManager
{
    public static readonly JsonSerializerOptions DefaultSerializerOptions = new()
    {
        IncludeFields = true,
        WriteIndented = true
    };

    public delegate bool MigrateDelegate(int version, ref JsonObject config);
    public delegate T? DeserializeDelegate<T>(ref JsonObject config);

    public static T Load<T>(int currentConfigVersion, DeserializeDelegate<T> deserializeDelegate, MigrateDelegate? migrateDelegate = null) where T : IConfiguration, new()
    {
        try
        {
            var configPath = Service.Get<DalamudPluginInterface>().ConfigFile.FullName;
            if (!File.Exists(configPath))
                return new();

            var jsonData = File.ReadAllText(configPath);
            if (string.IsNullOrEmpty(jsonData))
                return new();

            var config = JsonNode.Parse(jsonData);
            if (config is not JsonObject configObject)
                return new();

            var version = (int?)configObject[nameof(Version)] ?? 0;
            if (version == 0)
                return new();

            var migrated = false;

            if (version < currentConfigVersion && migrateDelegate != null)
            {
                Service.Get<IPluginLog>().Information("Starting config migration: {currentVersion} -> {targetVersion}", version, currentConfigVersion);

                var success = migrateDelegate.Invoke(version, ref configObject);

                config[nameof(Version)] = currentConfigVersion;

                if (success)
                {
                    Service.Get<IPluginLog>().Information("Config migration completed successfully!");
                }
                else
                {
                    var configBackupPath = $"{configPath}_v{version}.bak";

                    Service.Get<IPluginLog>().Warning("Config migration completed with errors! Writing backup of previous config to {configBackupPath}", configBackupPath);

                    try
                    {
                        File.WriteAllText(configBackupPath, jsonData);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Could not write backup config", ex);
                    }
                }

                migrated = true;
            }

            var deserializedConfig = deserializeDelegate(ref configObject);
            if (deserializedConfig == null)
                return new();

            if (migrated)
            {
                deserializedConfig.Save();
            }
            else
            {
                deserializedConfig.LastSavedConfigHash = deserializedConfig.Serialize().GetHashCode();
            }

            return deserializedConfig;
        }
        catch (Exception ex)
        {
            Service.Get<IPluginLog>().Error(ex, "Could not load the configuration file. Creating a new one.");

            if (!Service.Get<TranslationManager>().TryGetTranslation("Plugin.DisplayName", out var pluginName))
                pluginName = Service.Get<DalamudPluginInterface>().InternalName;

            Service.Get<INotificationManager>().AddNotification(new()
            {
                Content = t("Notification.CouldNotLoadConfig"),
                Type = NotificationType.Error,
                InitialDuration = TimeSpan.FromSeconds(10),
                Minimized = false
            });

            return new();
        }
    }

    public static void Save(IConfiguration config)
    {
        try
        {
            var serialized = config.Serialize();
            var hash = serialized.GetHashCode();

            if (config.LastSavedConfigHash != hash)
            {
                Util.WriteAllTextSafe(Service.Get<DalamudPluginInterface>().ConfigFile.FullName, serialized);
                config.LastSavedConfigHash = hash;
                Service.Get<IPluginLog>().Information("Configuration saved.");
            }
        }
        catch (Exception e)
        {
            Service.Get<IPluginLog>().Error(e, "Error saving config");
        }
    }
}
