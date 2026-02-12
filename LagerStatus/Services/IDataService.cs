namespace WarehouseAPI.Services;

public interface IDataService
{
    List<T> Load<T>(string filename);
    void Save<T>(string filename, List<T> data);
}
