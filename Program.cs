using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GeneticSearch
{
    class Program
    {
        struct GeneticData
        {
            public string protein;
            public string organism;
            public string amino_acids;
        }

        struct Command
        {
            public string name;
            public string parameter1;
            public string parameter2;
        }

        static List<Command> ReadCommands(string filename)
        {
            List<Command> commands = new List<Command>();
            using (StreamReader reader = new StreamReader(filename))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] parts = line.Split('\t');
                    Command command = new Command();
                    command.name = parts[0];
                    command.parameter1 = parts.Length > 1 ? parts[1] : string.Empty;
                    command.parameter2 = parts.Length > 2 ? parts[2] : string.Empty;

                    commands.Add(command);
                }
            }
            return commands;
        }

        static List<GeneticData> ReadData(string filename)
        {
            List<GeneticData> data = new List<GeneticData>();
            using (StreamReader reader = new StreamReader(filename))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] parts = line.Split('\t');
                    if (parts.Length >= 3)
                    {
                        GeneticData protein;
                        protein.protein = parts[0];
                        protein.organism = parts[1];
                        protein.amino_acids = parts[2];
                        data.Add(protein);
                    }
                }
            }
            return data;
        }

        static string RLEncoding(string amino_acids)
        {
            StringBuilder encoded = new StringBuilder();
            for (int i = 0; i < amino_acids.Length; i++)
            {
                char ch = amino_acids[i];
                int count = 1;
                while (i < amino_acids.Length - 1 && amino_acids[i + 1] == ch && count < 9)
                {
                    count++;
                    i++;
                }
                if (count > 2) encoded.Append(count).Append(ch);
                else if (count == 2) encoded.Append(ch).Append(ch);
                else encoded.Append(ch);
            }
            return encoded.ToString();
        }

        static string RLDecoding(string amino_acids)
        {
            StringBuilder decoded = new StringBuilder();
            for (int i = 0; i < amino_acids.Length; i++)
            {
                char ch = amino_acids[i];
                if (char.IsDigit(ch))
                {
                    char letter = amino_acids[i + 1];
                    int count = ch - '0';
                    for (int j = 1; j < count; j++)
                        decoded.Append(letter);
                }
                else
                {
                    decoded.Append(ch);
                }
            }
            return decoded.ToString();
        }

        static void CommandHandler(List<GeneticData> proteins, List<Command> commands, string outputPath)
        {
            using (StreamWriter writer = new StreamWriter(outputPath))
            {
                writer.WriteLine("Ivan Ivanov");
                writer.WriteLine("Genetic Searching");
                writer.WriteLine(new string('-', 80));

                for (int i = 0; i < commands.Count; i++)
                {
                    Command cmd = commands[i];
                    string operationNumber = (i + 1).ToString("D3");
                    if (cmd.name == "search")
                    {
                        string searchSequence = RLDecoding(cmd.parameter1);
                        writer.WriteLine($"{operationNumber}   search   {searchSequence}");

                        bool found = false;
                        bool headerPrinted = false;

                        foreach (var p in proteins)
                        {
                            string decodedAcids = RLDecoding(p.amino_acids);
                            if (decodedAcids.Contains(searchSequence))
                            {
                                if (!headerPrinted)
                                {
                                    writer.WriteLine("organism\t\t\tprotein");
                                    headerPrinted = true;
                                }
                                writer.WriteLine($"{p.organism}\t\t{p.protein}");
                                found = true;
                            }
                        }

                        if (!found)
                        {
                            writer.WriteLine("organism\t\t\tprotein");
                            writer.WriteLine("NOT FOUND");
                        }
                    }
                    else if (cmd.name == "diff")
                    {
                        writer.WriteLine($"{operationNumber}   diff   {cmd.parameter1}   {cmd.parameter2}");

                        var p1Index = proteins.FindIndex(p => p.protein == cmd.parameter1);
                        var p2Index = proteins.FindIndex(p => p.protein == cmd.parameter2);

                        if (p1Index == -1 || p2Index == -1)
                        {
                            List<string> missing = new List<string>();
                            if (p1Index == -1) missing.Add(cmd.parameter1);
                            if (p2Index == -1) missing.Add(cmd.parameter2);

                            writer.WriteLine("amino-acids difference:");
                            writer.WriteLine($"MISSING: {string.Join(", ", missing)}");
                        }
                        else
                        {
                            string seq1 = RLDecoding(proteins[p1Index].amino_acids);
                            string seq2 = RLDecoding(proteins[p2Index].amino_acids);

                            int diffCount = Math.Abs(seq1.Length - seq2.Length);
                            int minLength = Math.Min(seq1.Length, seq2.Length);

                            for (int k = 0; k < minLength; k++)
                            {
                                if (seq1[k] != seq2[k]) diffCount++;
                            }
                            writer.WriteLine("amino-acids difference:");
                            writer.WriteLine(diffCount);
                        }
                    }
                    else if (cmd.name == "mode")
                    {
                        writer.WriteLine($"{operationNumber}   mode   {cmd.parameter1}");

                        var pIndex = proteins.FindIndex(p => p.protein == cmd.parameter1);
                        if (pIndex == -1)
                        {
                            writer.WriteLine("amino-acid occurs:");
                            writer.WriteLine($"MISSING: {cmd.parameter1}");
                        }
                        else
                        {
                            string seq = RLDecoding(proteins[pIndex].amino_acids);
                            Dictionary<char, int> charCounts = new Dictionary<char, int>();

                            foreach (char c in seq)
                            {
                                if (char.IsLetter(c))
                                {
                                    if (charCounts.ContainsKey(c)) charCounts[c]++;
                                    else charCounts[c] = 1;
                                }
                            }

                            int maxCount = charCounts.Values.Max();
                            char mostFrequentChar = charCounts.Where(kvp => kvp.Value == maxCount)
                                                              .Select(kvp => kvp.Key)
                                                              .OrderBy(c => c)
                                                              .First();

                            writer.WriteLine("amino-acid occurs:");
                            writer.WriteLine($"{mostFrequentChar}\t\t{maxCount}");
                        }
                    }

                    writer.WriteLine(new string('-', 80));
                }
            }
        }

        static void Main(string[] args)
        {
            string dataFile = @"sequences.0.txt";
            string commandsFile = @"commands.0.txt";
            string outputFile = @"genedata0.txt";

            if (File.Exists(dataFile) && File.Exists(commandsFile))
            {
                List<GeneticData> data = ReadData(dataFile);
                List<Command> commands = ReadCommands(commandsFile);

                CommandHandler(data, commands, outputFile);
                Console.WriteLine("Операции успешно выполнены. Результат записан в " + outputFile);
            }
            else
            {
                Console.WriteLine("Входные файлы не найдены. Убедитесь, что sequences.0.txt и commands.0.txt лежат в папке с программой.");
            }
        }
    }
}
