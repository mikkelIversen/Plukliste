using Plukliste.Readers;

namespace Plukliste;

public sealed class PluklisteReaderFactory
{
    private readonly List<IPluklisteReader> _readers = new()
    {
        new XmlPluklisteReader(),
        new CsvMontørReader()
    };

    public IPluklisteReader Get(string filePath)
        => _readers.First(r => r.CanRead(filePath));
}
