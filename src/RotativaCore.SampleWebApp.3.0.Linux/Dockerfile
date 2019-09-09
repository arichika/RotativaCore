FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

### Customize for your environment. ###
RUN apt-get update -y && apt-get install -y \
  wget \
  libssl-dev \
  libxrender1 \
  libxext6 \
  libfontconfig1 \
  fontconfig \
  xfonts-75dpi \
  xfonts-base \
  libjpeg62-turbo
### /Customize for your environment. ###

RUN wget https://github.com/wkhtmltopdf/wkhtmltopdf/releases/download/0.12.5/wkhtmltox_0.12.5-1.stretch_amd64.deb
RUN dpkg -i wkhtmltox_0.12.5-1.stretch_amd64.deb
RUN cp /usr/local/bin/wkhtmlto* /usr/bin/


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
ENTRYPOINT ["dotnet", "RotativaCore.SampleWebApp.3.0.Linux.dll"]