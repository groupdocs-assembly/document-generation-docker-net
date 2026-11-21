using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using GroupDocs.Assembly;

namespace DockerReports
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            ApplyLicense();

            if (args.Length == 0 || args[0] == "-h" || args[0] == "--help")
            {
                Usage();
                return args.Length == 0 ? 1 : 0;
            }

            try
            {
                switch (args[0].ToLowerInvariant())
                {
                    case "report":
                        Reports.Assemble();
                        return 0;

                    case "preview":
                        return Preview(args);

                    case "fonts":
                        Reports.Fonts();
                        return 0;

                    default:
                        Console.Error.WriteLine($"Unknown command '{args[0]}'.");
                        Usage();
                        return 1;
                }
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine("Error: " + exception.Message);
                return 1;
            }
        }

        private static int Preview(string[] args)
        {
            string? document = args.Skip(1).FirstOrDefault(a => !a.StartsWith("--", StringComparison.Ordinal));
            document ??= RepositoryPaths.OutputFile("orders.docx");

            if (!File.Exists(document))
            {
                throw new FileNotFoundException("Document not found: " + document, document);
            }

            string directory = Option(args, "--out") ?? Path.Combine(RepositoryPaths.Output, "preview");
            int resolution = int.Parse(Option(args, "--dpi") ?? "120", CultureInfo.InvariantCulture);

            IReadOnlyList<string> files = Reports.Preview(document, directory, resolution);

            foreach (string file in files)
            {
                Console.WriteLine("  " + file);
            }

            Console.WriteLine($"{files.Count} page image(s) at {resolution} dpi.");
            return 0;
        }

        private static string? Option(string[] args, string name)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                {
                    return args[i + 1];
                }
            }

            return null;
        }

        /// <summary>Optional: without a licence the output carries evaluation marks.</summary>
        private static void ApplyLicense()
        {
            string? path = Environment.GetEnvironmentVariable("GROUPDOCS_ASSEMBLY_LICENSE")?.Trim();
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            if (File.Exists(path))
            {
                new License().SetLicense(path);
            }
            else
            {
                Console.Error.WriteLine("GROUPDOCS_ASSEMBLY_LICENSE points at a missing file: " + path);
            }
        }

        private static void Usage()
        {
            Console.WriteLine("Generating documents in a container - GroupDocs.Assembly for .NET");
            Console.WriteLine();
            Console.WriteLine("  report");
            Console.WriteLine("            assembles the sample report as DOCX and PDF");
            Console.WriteLine("  preview   [document] [--out directory] [--dpi 120]");
            Console.WriteLine("            renders every page of a document to PNG");
            Console.WriteLine("  fonts");
            Console.WriteLine("            lists the font files the runtime can see");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  dotnet run --project src/DockerReports -- report");
            Console.WriteLine("  dotnet run --project src/DockerReports -- preview output/orders.docx");
            Console.WriteLine("  docker run --rm document-generation-dotnet fonts");
        }
    }
}
