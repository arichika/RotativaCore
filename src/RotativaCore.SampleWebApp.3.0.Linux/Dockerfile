FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["src/RotativaCore.SampleWebApp.3.0.Linux/RotativaCore.SampleWebApp.3.0.Linux.csproj", "src/RotativaCore.SampleWebApp.3.0.Linux/"]
RUN dotnet restore "src/RotativaCore.SampleWebApp.3.0.Linux/RotativaCore.SampleWebApp.3.0.Linux.csproj"
COPY . .
WORKDIR "/src/src/RotativaCore.SampleWebApp.3.0.Linux"
RUN dotnet build "RotativaCore.SampleWebApp.3.0.Linux.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RotativaCore.SampleWebApp.3.0.Linux.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .


### Customize for your environment. ###
RUN apt-get update && apt-get install -y \
  wget \
  libfontconfig1 \
  libxrender1 \
  libxext6
### /Customize for your environment. ###



ENTRYPOINT ["dotnet", "RotativaCore.SampleWebApp.3.0.Linux.dll"]