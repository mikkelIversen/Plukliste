namespace Plukliste.Readers;

public sealed class CsvMontørReader : IPluklisteReader
{
    public bool CanRead(string filePath)
        => Path.GetExtension(filePath).Equals(".csv", StringComparison.OrdinalIgnoreCase);

    public Pluklist Read(string filePath)
    {
        var name = Path.GetFileNameWithoutExtension(filePath);

        var lines = File.ReadLines(filePath)
                        .Skip(1)
                        .Select(ParseItem)
                        .ToList();

        return new Pluklist
        {
            Name = name,
            Forsendelse = "Pickup",
            Lines = lines
        };
    }

    private static Item ParseItem(string line)
    {
        var parts = line.Split(';');

        return new Item
        {
            ProductID = parts[0],
            Type = Enum.Parse<ItemType>(parts[1]),
            Title = parts[2],
            Amount = int.Parse(parts[3])
        };
    }
}
