using System;
using System.Collections.Generic;
using System.IO;
using GroupDocs.Assembly;
using GroupDocs.Assembly.Data;

namespace DockerReports
{
    /// <summary>
    /// Assembling a report and rendering its pages, the two things a container has to get right.
    /// The calls mirror the official examples repository
    /// (GroupDocs.Assembly-for-.NET/Examples/.../AdvancedUsage/GenerateDocumentPreview.cs).
    /// </summary>
    public static class Reports
    {
        /// <summary>Assembles the report from JSON and saves it as DOCX and PDF.</summary>
        public static void Assemble()
        {
            string template = RepositoryPaths.Template("Customer orders.docx");
            string data = RepositoryPaths.DataFile("Customers.json");

            DocumentAssembler assembler = new DocumentAssembler();

            assembler.AssembleDocument(
                template,
                RepositoryPaths.OutputFile("orders.docx"),
                new DataSourceInfo(new JsonDataSource(data), "customers"));

            assembler.AssembleDocument(
                template,
                RepositoryPaths.OutputFile("orders.pdf"),
                new LoadSaveOptions(FileFormat.Pdf),
                new DataSourceInfo(new JsonDataSource(data), "customers"));

            Console.WriteLine("wrote : " + RepositoryPaths.OutputFile("orders.docx"));
            Console.WriteLine("wrote : " + RepositoryPaths.OutputFile("orders.pdf"));
        }

        /// <summary>
        /// Renders every page of a document to PNG. Supported since 26.11.
        ///
        /// This is the call that makes a container's font setup visible: rendering lays text out,
        /// so a missing font shows up as wrong metrics or substituted glyphs in the image, where a
        /// plain DOCX would have hidden it until somebody opened the file on a desktop.
        /// </summary>
        public static IReadOnlyList<string> Preview(string documentPath, string outputDirectory, int resolution)
        {
            Directory.CreateDirectory(outputDirectory);
            List<string> written = new List<string>();

            string name = Path.GetFileNameWithoutExtension(documentPath);

            // A stream is created for every page and released when the page is written.
            PreviewOptions previewOptions = new PreviewOptions(
                pageNumber =>
                {
                    string file = Path.Combine(outputDirectory, $"{name}-page{pageNumber}.png");
                    written.Add(file);
                    return File.Create(file);
                },
                (pageNumber, pageStream) => pageStream.Dispose());

            previewOptions.PreviewFormat = PreviewFormats.PNG;
            previewOptions.Resolution = resolution; // Dots per inch, 96 by default.

            new DocumentAssembler().GeneratePreview(documentPath, previewOptions);
            return written;
        }

        /// <summary>
        /// Lists the font families the runtime can see.
        ///
        /// On a Linux image this is the difference between a correct report and one laid out with
        /// a substituted font: the .NET runtime images ship without fonts, and nothing in the
        /// assembly step fails when they are missing - only the output changes.
        /// </summary>
        public static void Fonts()
        {
            string[] directories =
            {
                "/usr/share/fonts",
                "/usr/local/share/fonts",
                Environment.GetFolderPath(Environment.SpecialFolder.Fonts)
            };

            int total = 0;

            foreach (string directory in directories)
            {
                if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                {
                    continue;
                }

                string[] files = Directory.GetFiles(directory, "*.*", System.IO.SearchOption.AllDirectories);
                int fonts = 0;

                foreach (string file in files)
                {
                    string extension = Path.GetExtension(file).ToLowerInvariant();
                    if (extension == ".ttf" || extension == ".otf" || extension == ".ttc")
                    {
                        fonts++;
                    }
                }

                total += fonts;
                Console.WriteLine($"{directory}: {fonts} font file(s)");
            }

            Console.WriteLine($"total: {total} font file(s)");

            if (total == 0)
            {
                Console.WriteLine();
                Console.WriteLine("No fonts found. Text will be laid out with a substitute and the report");
                Console.WriteLine("will not match what you see on a desktop. Install fontconfig and a font");
                Console.WriteLine("package in the image - see the Dockerfile in this repository.");
            }
        }
    }
}
