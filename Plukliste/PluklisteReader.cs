namespace Plukliste.Readers;

public interface IPluklisteReader
{
    bool CanRead(string filePath);
    Pluklist? Read(string filePath);
}
