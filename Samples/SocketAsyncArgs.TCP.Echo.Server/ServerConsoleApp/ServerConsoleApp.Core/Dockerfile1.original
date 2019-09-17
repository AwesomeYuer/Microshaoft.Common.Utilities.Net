FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY Samples/SocketAsyncArgs.TCP.Echo.Server/ServerConsoleApp/ServerConsoleApp.Core/ServerConsoleApp.Core.csproj Samples/SocketAsyncArgs.TCP.Echo.Server/ServerConsoleApp/ServerConsoleApp.Core/
COPY CommonUtilities/CommonUtilities.Full/CommonUtilities.NET.Standard.2.x/CommonUtilities.NET.Standard.2.x.csproj CommonUtilities/CommonUtilities.Full/CommonUtilities.NET.Standard.2.x/
RUN dotnet restore Samples/SocketAsyncArgs.TCP.Echo.Server/ServerConsoleApp/ServerConsoleApp.Core/ServerConsoleApp.Core.csproj
COPY . .
WORKDIR /src/Samples/SocketAsyncArgs.TCP.Echo.Server/ServerConsoleApp/ServerConsoleApp.Core
RUN dotnet build ServerConsoleApp.Core.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish ServerConsoleApp.Core.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ServerConsoleApp.Core.dll"]
