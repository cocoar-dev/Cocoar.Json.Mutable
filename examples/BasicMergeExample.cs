using Cocoar.Json.Mutable;

namespace Examples;

/// <summary>
/// Example demonstrating basic JSON merging with Cocoar.Json.Mutable
/// </summary>
public class BasicMergeExample
{
    public static void RunWithStringApi()
    {
        // Create an empty document using developer-friendly string API
        var config = new MutableJsonObject();
        
        // Build configuration programmatically
        var server = new MutableJsonObject();
        server.Set("host", new MutableJsonString("localhost"));
        server.Set("port", new MutableJsonNumber(8080));
        server.Set("timeout", new MutableJsonNumber(30));
        config.Set("server", server);
        
        var logging = new MutableJsonObject();
        logging.Set("level", new MutableJsonString("info"));
        config.Set("logging", logging);
        
        // Override some values
        var overrides = new MutableJsonObject();
        var serverOverrides = new MutableJsonObject();
        serverOverrides.Set("port", new MutableJsonNumber(9000));
        overrides.Set("server", serverOverrides);
        
        var loggingOverrides = new MutableJsonObject();
        loggingOverrides.Set("level", new MutableJsonString("debug"));
        loggingOverrides.Set("verbose", new MutableJsonBool(true));
        overrides.Set("logging", loggingOverrides);
        
        // Merge - nested objects merge, primitives override
        MutableJsonMerge.Merge(config, overrides);
        
        // Access values using string API
        var serverNode = config.Get("server") as MutableJsonObject;
        var port = serverNode?.Get("port") as MutableJsonNumber;
        var level = (config.Get("logging") as MutableJsonObject)?.Get("level") as MutableJsonString;
        
        Console.WriteLine($"Port: {System.Text.Encoding.UTF8.GetString(port!.ValueUtf8)}");
        Console.WriteLine($"Level: {System.Text.Encoding.UTF8.GetString(level!.ValueUtf8)}");
        
        // Serialize
        var json = MutableJsonDocument.ToUtf8Bytes(config);
        Console.WriteLine(System.Text.Encoding.UTF8.GetString(json));
    }
    
    public static void Run()
    {
        // Create an empty document to hold merged configuration
        var config = new MutableJsonObject();
        
        // Simulate loading base configuration
        var baseConfig = MutableJsonDocument.Parse(@"{
            ""server"": {
                ""port"": 8080,
                ""timeout"": 30
            },
            ""logging"": {
                ""level"": ""info""
            }
        }"u8);
        
        // Merge base config
        MutableJsonMerge.Merge(config, (MutableJsonObject)baseConfig);
        
        // Simulate loading environment-specific overrides
        var envConfig = MutableJsonDocument.Parse(@"{
            ""server"": {
                ""host"": ""localhost"",
                ""port"": 9000
            },
            ""features"": {
                ""experimental"": true
            }
        }"u8);
        
        // Merge environment config - nested objects merge, primitives override
        MutableJsonMerge.Merge(config, (MutableJsonObject)envConfig);
        
        // Simulate loading user-specific settings
        var userConfig = MutableJsonDocument.Parse(@"{
            ""logging"": {
                ""level"": ""debug"",
                ""verbose"": true
            }
        }"u8);
        
        // Merge user config
        MutableJsonMerge.Merge(config, (MutableJsonObject)userConfig);
        
        // Result contains merged configuration:
        // {
        //   "server": {
        //     "port": 9000,         // overridden by envConfig
        //     "timeout": 30,        // from baseConfig
        //     "host": "localhost"   // from envConfig
        //   },
        //   "logging": {
        //     "level": "debug",     // overridden by userConfig
        //     "verbose": true       // from userConfig
        //   },
        //   "features": {
        //     "experimental": true  // from envConfig
        //   }
        // }
        
        // Serialize to JSON
        var finalJson = MutableJsonDocument.ToUtf8Bytes(config);
        Console.WriteLine(System.Text.Encoding.UTF8.GetString(finalJson));
    }
    
    public static void DestructiveMergeExample()
    {
        // For better performance when you don't need the source objects anymore,
        // use MergeDestructive which moves values instead of cloning
        
        var target = new MutableJsonObject();
        var source = MutableJsonDocument.Parse("{\"key\": \"value\"}"u8);
        
        // Source values are moved to target (faster, but source is modified)
        MutableJsonMerge.MergeDestructive(target, (MutableJsonObject)source);
        
        // Don't use source after destructive merge!
    }
    
    public static void CloneExample()
    {
        // Create a template configuration
        var template = MutableJsonDocument.Parse(@"{
            ""database"": {
                ""type"": ""postgresql"",
                ""pool_size"": 10
            }
        }"u8);
        
        // Clone the template for each environment
        var devConfig = MutableJsonMerge.Clone(template);
        var prodConfig = MutableJsonMerge.Clone(template);
        
        // Customize each clone independently
        ((MutableJsonObject)devConfig).Set("environment"u8, 
            new MutableJsonString("development"u8.ToArray()));
        
        ((MutableJsonObject)prodConfig).Set("environment"u8, 
            new MutableJsonString("production"u8.ToArray()));
    }
}
