//Eksempel på funktionel kodning hvor der kun bliver brugt et model lag

using Plukliste.Readers;

namespace Plukliste;

class PluklisteProgram { 

    static void Main()
    {
        //Arrange
        char readKey = ' ';
        List<string> files;
        var index = -1;
        var standardColor = Console.ForegroundColor;
        Directory.CreateDirectory("import");
        Directory.CreateDirectory("print");
        Directory.CreateDirectory("templates");

        if (!Directory.Exists("export"))
        {
            Console.WriteLine("Directory \"export\" not found");
            Console.ReadLine();
            return;
        }
        files = Directory.EnumerateFiles("export").ToList();

        //ACT
        while (readKey != 'Q')
        {
            bool canPrint = false;
            if (files.Count == 0)
            {
                Console.WriteLine("No files found.");

            }
            else
            {
                if (index == -1) index = 0;

                Console.WriteLine($"Plukliste {index + 1} af {files.Count}");
                Console.WriteLine($"\nfile: {files[index]}");

                //read file
                PluklisteReaderFactory factory = new PluklisteReaderFactory();
                IPluklisteReader reader = factory.Get(files[index]);
                Pluklist? plukliste = reader.Read(files[index]);


                //print plukliste
                if (plukliste != null && plukliste.Lines.Count > 0)
                {
                    Console.WriteLine("\n{0, -13}{1}", "Name:", plukliste.Name);
                    Console.WriteLine("{0, -13}{1}", "Forsendelse:", plukliste.Forsendelse);
                    //TODO: Add adresse to screen print

                    Console.WriteLine("\n{0,-7}{1,-9}{2,-20}{3}", "Antal", "Type", "Produktnr.", "Navn");
                    foreach (var item in plukliste.Lines)
                    {
                        Console.WriteLine("{0,-7}{1,-9}{2,-20}{3}", item.Amount, item.Type, item.ProductID, item.Title);
                        if(item.Type == ItemType.Print) canPrint = true;
                    }
                    
                }
            }

            //Print options
            Console.WriteLine("\n\nOptions:");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Q");
            Console.ForegroundColor = standardColor;
            Console.WriteLine("uit");
            if (index >= 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("A");
                Console.ForegroundColor = standardColor;
                Console.WriteLine("fslut plukseddel");
            }
            if (index > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("F");
                Console.ForegroundColor = standardColor;
                Console.WriteLine("orrige plukseddel");
            }
            if (index < files.Count - 1)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("N");
                Console.ForegroundColor = standardColor;
                Console.WriteLine("æste plukseddel");
            }
            if (canPrint)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("P");
                Console.ForegroundColor = standardColor;
                Console.WriteLine("rint plukseddel");
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("G");
            Console.ForegroundColor = standardColor;
            Console.WriteLine("enindlæs pluksedler");

            readKey = Console.ReadKey().KeyChar;
            if (readKey >= 'a') readKey -= (char)('a' - 'A'); //HACK: To upper
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Red; //status in red
            switch (readKey)
            {
                case 'G':
                    files = Directory.EnumerateFiles("export").ToList();
                    index = -1;
                    Console.WriteLine("Pluklister genindlæst");
                    break;
                case 'F':
                    if (index > 0) index--;
                    break;
                case 'N':
                    if (index < files.Count - 1) index++;
                    break;
                case 'A':
                    //Move files to import directory
                    var filewithoutPath = files[index].Substring(files[index].LastIndexOf('\\'));
                    File.Move(files[index], string.Format(@"import\\{0}", filewithoutPath));
                    Console.WriteLine($"Plukseddel {files[index]} afsluttet.");
                    files.Remove(files[index]);
                    if (index == files.Count) index--;
                    break;
                case 'P':
                    PluklisteReaderFactory factory = new PluklisteReaderFactory();
                    IPluklisteReader reader = factory.Get(files[index]);
                    Pluklist? plukliste = reader.Read(files[index]);

                    if (plukliste != null && plukliste.Lines.Count > 0)
                    {
                        foreach (var item in plukliste.Lines)
                        {
                            if (item.Type != ItemType.Print) continue;

                            string[] fileEntries = Directory.GetFiles("templates");
                            string[] fileEntriesNoPath = fileEntries
                                .Select(f => Path.GetFileNameWithoutExtension(f))
                                .ToArray();
                            if(!fileEntriesNoPath.Contains(item.ProductID)) continue;

                            for (int i = 0; i < fileEntriesNoPath.Length; i++)
                            {
                                if(fileEntriesNoPath[i] != item.ProductID) continue;
                                
                                String html =  File.ReadAllText(fileEntries[i]);
                                html = html.Replace("[Adresse]", plukliste.Adresse);
                                html = html.Replace("[Name]", plukliste.Name);

                                string plukList = "";
                                foreach (Item listItem in plukliste.Lines)
                                {
                                    if (listItem.Type == ItemType.Fysisk)
                                    {
                                        plukList += $"{listItem.Title} - {listItem.Amount}<br>";
                                    }
                                }
                                html = html.Replace("[Plukliste]", plukList);

                                string path = Path.Combine("print", $"{plukliste.Name}-{plukliste.Adresse}.html");
                                if(File.Exists(path)) File.Delete(path);
                                File.WriteAllText(path, html);
                            }
                        }
                    }
                    break;
            }
            Console.ForegroundColor = standardColor; //reset color

        }
    }
}
