namespace Plukliste.Readers;

public sealed class XmlPluklisteReader : IPluklisteReader
{
    public bool CanRead(string filePath)
        => Path.GetExtension(filePath).Equals(".xml", StringComparison.OrdinalIgnoreCase);

    
    public Pluklist? Read(string filePath)
    {
        using var file = File.OpenRead(filePath);
        var serializer = new System.Xml.Serialization.XmlSerializer(typeof(Pluklist));
        return (Pluklist)serializer.Deserialize(file);
    }
}
