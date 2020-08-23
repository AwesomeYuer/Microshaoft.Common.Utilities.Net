#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app

# COPY ["Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.Shared/wwwroot/", "/app/wwwroot/"]
COPY ["Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.Shared/wwwroot/", "/usr/local/wwwroot/"]
EXPOSE 5000
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/MsSqlCodeDiffVersioning.3x.csproj", "Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/"]
COPY ["Microshaoft.WebApiUtilities.NET/Microshaoft.WebApiUtilities.NET.Core.3.x/Microshaoft.WebApiUtilities.NET.Core.3.x.csproj", "Microshaoft.WebApiUtilities.NET/Microshaoft.WebApiUtilities.NET.Core.3.x/"]
COPY ["CommonUtilities/CommonUtilities.Lite.Standard.2.x/CommonUtilities.Lite.Standard.2.x.csproj", "CommonUtilities/CommonUtilities.Lite.Standard.2.x/"]
COPY ["Microshaoft.StoreProceduresExecutors.NET/Microshaoft.StoreProceduresExecutors.NET.Standard.2.x/Microshaoft.StoreProceduresExecutors.NET.Standard.2.x.csproj", "Microshaoft.StoreProceduresExecutors.NET/Microshaoft.StoreProceduresExecutors.NET.Standard.2.x/"]
COPY ["Microshaoft.Json.NET/Microshaoft.Json.NET.Standard.2.x/Microshaoft.Json.NET.Standard.2.x.csproj", "Microshaoft.Json.NET/Microshaoft.Json.NET.Standard.2.x/"]
# COPY ["Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.Shared/wwwroot/", "/app/wwwroot/"]
RUN dotnet restore "Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/MsSqlCodeDiffVersioning.3x.csproj"
COPY . .
WORKDIR "/src/Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x"
RUN dotnet build "MsSqlCodeDiffVersioning.3x.csproj" -c Release -o /app/build


FROM build AS publish
RUN dotnet publish "MsSqlCodeDiffVersioning.3x.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MsSqlCodeDiffVersioning.3x.dll"]