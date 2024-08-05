using System;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace NorthstarModsSettingReadWrite;

public static partial class NsModsSettingReadWrite
{
    
    public static JsonSerializerOptions SourceGenOptions = new JsonSerializerOptions
    {
        TypeInfoResolver = SourceGenerationContext.Default,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        WriteIndented = true,
    };
        
    public static List<NorthstarModJson>? GetMods(string R2Northstar)
    {
        var modJsons = new List<NorthstarModJson>();
        var existEnabledMods = JsonSerializer.Deserialize<Dictionary<string, bool>>(File.ReadAllText(Path.Combine(R2Northstar, "enabledmods.json"),Encoding.UTF8),SourceGenOptions);
           
        if (!Directory.Exists(Path.Combine(R2Northstar, "mods")))
        {
            return null;
        }
        
        foreach (var directory in Directory.GetDirectories(Path.Combine(R2Northstar, "mods")))
        {
            if (File.Exists(Path.Combine(directory,"mod.json")))
            {
                var mod = JsonSerializer.Deserialize<NorthstarModJson>(
                    File.ReadAllText(Path.Combine(directory, "mod.json")), SourceGenOptions);
                existEnabledMods.TryGetValue(mod.Name, out bool enabled);
                mod.Enabled = enabled;
                mod.Path = directory;
                modJsons.Add(mod);        
            }
        }
            
        return modJsons;
    }
        
    public static void SaveEnabledMods(string R2Northstar, List<NorthstarModJson> mods,bool forceSave = false)
    {
        var enabledMods = new Dictionary<string, bool>();
            
        foreach (var mod in mods)
        {
            enabledMods.TryAdd(mod.Name, mod.Enabled);   
        }
        
        if (!Directory.Exists(R2Northstar))
        {
            if (!forceSave)
            {
                throw new DirectoryNotFoundException("给定的R2Northstar路径不存在");
            }else{
                Directory.CreateDirectory(R2Northstar);
            }
        }
            
        File.WriteAllText(Path.Combine(R2Northstar, "enabledmods.json"),JsonSerializer.Serialize(enabledMods, SourceGenOptions),Encoding.UTF8);
    }
        
    public struct NorthstarModJson()
    {
        [JsonPropertyName("Name")]
        public string Name{ get; set; }
        
        [JsonPropertyName("Description")]
        public string Description{ get; set; }
        
        [JsonPropertyName("Version")]
        public string Version{ get; set; }
        
        [JsonPropertyName("LoadPriority")]
        public int LoadPriority{ get; set; }
        
        [JsonPropertyName("Image")]
        public string[]? Image{ get; set; }

        [JsonIgnore]
        public bool Enabled { get; set; } = true;
        
        [JsonIgnore]
        public string Path { get; set; }
    }
    
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(NorthstarModJson))]
    [JsonSerializable(typeof(Dictionary<string, bool>))]
    internal partial class SourceGenerationContext : JsonSerializerContext
    {

    }
}