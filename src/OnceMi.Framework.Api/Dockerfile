#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim AS build
WORKDIR /src
COPY . /src
RUN dotnet restore "OnceMi.Framework.Api/OnceMi.Framework.Api.csproj"
WORKDIR "/src/OnceMi.Framework.Api"
RUN dotnet build "OnceMi.Framework.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OnceMi.Framework.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN sed -i 's/deb.debian.org/mirrors.ustc.edu.cn/g' /etc/apt/sources.list
RUN sed -i 's|security.debian.org/debian-security|mirrors.ustc.edu.cn/debian-security|g' /etc/apt/sources.list
RUN apt update && apt install apt-utils libgdiplus libc6-dev sqlite3 -y && apt clean
RUN mkdir -p /oncemi/data
RUN sqlite3 /oncemi/data/app.db "VACUUM;"

ENTRYPOINT ["dotnet", "OnceMi.Framework.Api.dll"]