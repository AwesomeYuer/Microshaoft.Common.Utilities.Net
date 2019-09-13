FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["Samples/WebApplication.ASPNETCore.StoreProcedureExecutor/WebApplication.ASPNetCore.StoreProcedureExecutor.csproj", "Samples/WebApplication.ASPNETCore.StoreProcedureExecutor/"]
COPY ["CommonUtilities/WebApiUtilities/WebApiUtilities.NET.Core.2.x/WebApiUtilities.NET.Core.2.x.csproj", "CommonUtilities/WebApiUtilities/WebApiUtilities.NET.Core.2.x/"]
COPY ["CommonUtilities/CommonUtilities.Lite/CommonUtilities.Lite.NET.Core.2.x/CommonUtilities.Lite.NET.Core.2.x.csproj", "CommonUtilities/CommonUtilities.Lite/CommonUtilities.Lite.NET.Core.2.x/"]
RUN dotnet restore "Samples/WebApplication.ASPNETCore.StoreProcedureExecutor/WebApplication.ASPNetCore.StoreProcedureExecutor.csproj"
COPY . .
WORKDIR "/src/Samples/WebApplication.ASPNETCore.StoreProcedureExecutor"
RUN dotnet build "WebApplication.ASPNetCore.StoreProcedureExecutor.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "WebApplication.ASPNetCore.StoreProcedureExecutor.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "WebApplication.ASPNetCore.StoreProcedureExecutor.dll"]