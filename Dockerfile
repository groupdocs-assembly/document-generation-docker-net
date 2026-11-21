# The point of this image: a .NET runtime image has no fonts, and document generation needs them.
# Build it, run `fonts`, then comment the apt-get block out and run `fonts` again to see the
# difference - and then look at the page images the two produce.

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/DockerReports/DockerReports.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /work

# fontconfig finds the fonts, fonts-liberation provides metric-compatible substitutes for
# Arial, Times New Roman and Courier New, which is what most templates ask for.
RUN apt-get update \
 && apt-get install -y --no-install-recommends fontconfig fonts-liberation \
 && rm -rf /var/lib/apt/lists/*

COPY --from=build /app /app
COPY templates /work/templates
COPY data /work/data

ENTRYPOINT ["dotnet", "/app/report.dll"]
CMD ["report"]
