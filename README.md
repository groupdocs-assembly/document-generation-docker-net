# Generating documents in a container

[![build](https://github.com/groupdocs-assembly/document-generation-docker-net/actions/workflows/build.yml/badge.svg)](https://github.com/groupdocs-assembly/document-generation-docker-net/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/GroupDocs.Assembly?label=GroupDocs.Assembly)](https://www.nuget.org/packages/GroupDocs.Assembly/)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%2010.0-512bd4)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

Document generation moves to Linux the moment it goes into a container, a Kubernetes job or a
function. The code does not change. The image does — and the single thing that catches everybody is
**fonts**: the .NET runtime images ship without any, nothing fails when they are missing, and the
output is quietly laid out with a substitute.

This sample makes that visible. It assembles a report, renders its pages to PNG — `GeneratePreview`
is new in 26.11 — and lists the fonts the runtime can actually see.

```
docker build -t document-generation-dotnet .
docker run --rm document-generation-dotnet fonts
```

![a rendered page of the report](docs/output.png)

## Read this with

| Resource | What it adds |
|---|---|
| [Blog: Generating documents in Docker with .NET](https://blog.groupdocs.com/assembly/generate-documents-in-docker-dotnet/) | the whole story, including what a missing font does to a page |
| [Docs: Running in Docker and serverless](https://docs.groupdocs.com/assembly/net/running-in-docker-and-serverless/) | the reference page |
| [This repository](https://github.com/groupdocs-assembly/document-generation-docker-net) | the code that runs |

## Quick start

```bash
git clone https://github.com/groupdocs-assembly/document-generation-docker-net.git
cd document-generation-docker-net

dotnet run --project src/DockerReports -- report     # assemble DOCX and PDF
dotnet run --project src/DockerReports -- preview    # render the pages to PNG
dotnet run --project src/DockerReports -- fonts      # what the runtime can see
```

In a container:

```bash
docker build -t document-generation-dotnet .
docker run --rm -v "$PWD/output:/work/output" document-generation-dotnet report
docker run --rm -v "$PWD/output:/work/output" document-generation-dotnet preview output/orders.docx
docker run --rm document-generation-dotnet fonts
```

## The three lines that matter

```dockerfile
RUN apt-get update \
 && apt-get install -y --no-install-recommends fontconfig fonts-liberation \
 && rm -rf /var/lib/apt/lists/*
```

`fontconfig` is what finds fonts at all; `fonts-liberation` provides metric-compatible substitutes
for Arial, Times New Roman and Courier New, which is what most templates ask for. Without them
`fonts` prints `total: 0 font file(s)` and the renderer falls back to whatever it can find — line
breaks move, tables overflow, and a page that was one page becomes two.

A template that uses other fonts needs those fonts in the image as well. Copy them into
`/usr/share/fonts` and run `fc-cache -f`, or point the assembler at a folder of your own.

## Rendering is the test

A DOCX hides a font problem until somebody opens it on a desktop. Rendering it does not:

```csharp
PreviewOptions previewOptions = new PreviewOptions(
    pageNumber => File.Create($"page{pageNumber}.png"),
    (pageNumber, pageStream) => pageStream.Dispose());

previewOptions.PreviewFormat = PreviewFormats.PNG;
previewOptions.Resolution = 120;           // dots per inch, 96 by default

new DocumentAssembler().GeneratePreview(documentPath, previewOptions);
```

`GeneratePreview` came with 26.11 and takes a path or a stream. Put one call in the smoke test of
your image and a missing font becomes a failing build instead of a support ticket.

## Serverless

The same image works as the base of an Azure Functions custom handler, an AWS Lambda container
image or a Cloud Run job. Three things to plan for:

* **Cold start.** The first assembly in a process pays for loading the engine. Keep the function
  warm, or accept the first call being slower.
* **Writable space.** Assemble to streams rather than to files — nothing in this sample needs a
  writable disk except the sample output itself.
* **The licence.** Ship it as a secret, not in the image, and apply it once per process.

## Layout

```
src/DockerReports/
  Program.cs     command line
  Reports.cs     assemble, preview and the font check
Dockerfile       runtime image with fonts
templates/Customer orders.docx
data/Customers.json
```

Every call mirrors an example of the product's examples repository
([GroupDocs.Assembly-for-.NET](https://github.com/groupdocs-assembly/GroupDocs.Assembly-for-.NET)).

## Notes

* **Licence.** Without one the library runs in evaluation mode and the output carries evaluation
  marks ([temporary licence](https://purchase.groupdocs.com/temporary-license)).
* **Frameworks.** The sample targets .NET 8; the package also builds for .NET Framework 4.0 Client
  Profile, 4.6.2 and 4.8, .NET Standard 2.0, .NET 6 and .NET 10.

## Related samples

* [aspnet-core-report-download-net](https://github.com/groupdocs-assembly/aspnet-core-report-download-net) — the same rendering, served to a browser
* [word-report-from-template-net](https://github.com/groupdocs-assembly/word-report-from-template-net) — the basics, if you are new to the library
* [GroupDocs.Assembly-for-.NET](https://github.com/groupdocs-assembly/GroupDocs.Assembly-for-.NET) — the full example set

## License

MIT, see [LICENSE](LICENSE). GroupDocs.Assembly itself is a commercial library with its own
[licence terms](https://purchase.groupdocs.com/policies/).
