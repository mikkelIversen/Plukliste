using System.Text.Json;

namespace WarehouseAPI.Services;

public class JsonDataService : IDataService
{
    private readonly string _dataPath = "Data";
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonDataService()
    {
        Directory.CreateDirectory(_dataPath);
        _jsonOptions = new JsonSerializerOptions { WriteIndented = true };
    }

    public List<T> Load<T>(string filename)
    {
        var path = Path.Combine(_dataPath, filename);
        if (!File.Exists(path)) 
            return new List<T>();
        
        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
        }
        catch
        {
            return new List<T>();
        }
    }

    public void Save<T>(string filename, List<T> data)
    {
        var path = Path.Combine(_dataPath, filename);
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        File.WriteAllText(path, json);
    }
}
