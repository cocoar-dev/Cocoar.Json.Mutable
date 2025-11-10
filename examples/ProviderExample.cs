using Cocoar.Json.Mutable;

namespace Examples;

/// <summary>
/// Example showing how to use Cocoar.Json.Mutable with a provider that gives ReadOnlyMemory<byte>
/// </summary>
public class ProviderExample
{
    // Simulated provider interface
    public interface IJsonProvider
    {
        ReadOnlyMemory<byte> GetJsonData(string key);
    }
    
    // Example: Merge multiple configurations from a provider
    public static void MergeFromProvider(IJsonProvider provider)
    {
        // Create the document to hold merged configuration
        var config = new MutableJsonObject();
        
        // Get JSON from provider - it gives us ReadOnlyMemory<byte>
        // This is efficient - no copying, just working with provider's buffer
        var baseConfig = provider.GetJsonData("base.json");
        var envConfig = provider.GetJsonData("environment.json");
        var userConfig = provider.GetJsonData("user.json");
        
        // Parse directly from ReadOnlyMemory<byte>
        var baseParsed = MutableJsonDocument.Parse(baseConfig);
        var envParsed = MutableJsonDocument.Parse(envConfig);
        var userParsed = MutableJsonDocument.Parse(userConfig);
        
        // Merge them in order (later configs override earlier ones)
        MutableJsonMerge.Merge(config, (MutableJsonObject)baseParsed);
        MutableJsonMerge.Merge(config, (MutableJsonObject)envParsed);
        MutableJsonMerge.Merge(config, (MutableJsonObject)userParsed);
        
        // Now config contains the merged result
        var finalJson = MutableJsonDocument.ToUtf8Bytes(config);
        Console.WriteLine(System.Text.Encoding.UTF8.GetString(finalJson));
    }
    
    // Example: Direct usage with byte array
    public static void DirectUsageExample()
    {
        // Your provider gives you this
        byte[] jsonData = "{\"database\": {\"host\": \"localhost\", \"port\": 5432}}"u8.ToArray();
        ReadOnlyMemory<byte> memory = new ReadOnlyMemory<byte>(jsonData);
        
        // Parse it
        var config = MutableJsonDocument.Parse(memory) as MutableJsonObject;
        
        // Modify it
        config?.Set("database:timeout"u8, new MutableJsonNumber("30"u8));
        
        // Get values
        var dbConfig = config?.Get("database"u8) as MutableJsonObject;
        var host = dbConfig?.Get("host"u8) as MutableJsonString;
        
        if (host != null)
        {
            Console.WriteLine($"Database host: {System.Text.Encoding.UTF8.GetString(host.ValueUtf8)}");
        }
    }
    
    // Real-world example: Configuration system
    public class ConfigurationManager
    {
        private readonly IJsonProvider _provider;
        private MutableJsonObject? _cachedConfig;
        
        public ConfigurationManager(IJsonProvider provider)
        {
            _provider = provider;
        }
        
        public void LoadConfiguration()
        {
            _cachedConfig = new MutableJsonObject();
            
            // Load multiple config files in priority order
            var configFiles = new[] { "appsettings.json", "appsettings.Development.json", "user-overrides.json" };
            
            foreach (var file in configFiles)
            {
                try
                {
                    var jsonMemory = _provider.GetJsonData(file);
                    if (jsonMemory.Length > 0)
                    {
                        var parsed = MutableJsonDocument.Parse(jsonMemory);
                        MutableJsonMerge.Merge(_cachedConfig, (MutableJsonObject)parsed);
                    }
                }
                catch (FileNotFoundException)
                {
                    // Optional file doesn't exist - skip it
                    continue;
                }
            }
        }
        
        public string? GetString(string key)
        {
            if (_cachedConfig == null) return null;
            
            var value = _cachedConfig.Get(System.Text.Encoding.UTF8.GetBytes(key));
            if (value is MutableJsonString str)
            {
                return System.Text.Encoding.UTF8.GetString(str.ValueUtf8);
            }
            return null;
        }
        
        public int? GetInt(string key)
        {
            if (_cachedConfig == null) return null;
            
            var value = _cachedConfig.Get(System.Text.Encoding.UTF8.GetBytes(key));
            if (value is MutableJsonNumber num)
            {
                var numStr = System.Text.Encoding.UTF8.GetString(num.ValueUtf8);
                if (int.TryParse(numStr, out var result))
                    return result;
            }
            return null;
        }
    }
}
