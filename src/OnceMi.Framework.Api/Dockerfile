#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["OnceMi.Framework.Api/OnceMi.Framework.Api.csproj", "OnceMi.Framework.Api/"]
COPY ["OnceMi.Framework.Util/OnceMi.Framework.Util.csproj", "OnceMi.Framework.Util/"]
COPY ["OnceMi.Framework.IService/OnceMi.Framework.IService.csproj", "OnceMi.Framework.IService/"]
COPY ["OnceMi.Framework.Entity/OnceMi.Framework.Entity.csproj", "OnceMi.Framework.Entity/"]
COPY ["OnceMi.Framework.Model/OnceMi.Framework.Model.csproj", "OnceMi.Framework.Model/"]
COPY ["OnceMi.AspNetCore.MQ/OnceMi.AspNetCore.MQ.csproj", "OnceMi.AspNetCore.MQ/"]
COPY ["OnceMi.Framework.Extension/OnceMi.Framework.Extension.csproj", "OnceMi.Framework.Extension/"]
COPY ["OnceMi.Framework.IRepository/OnceMi.Framework.IRepository.csproj", "OnceMi.Framework.IRepository/"]
COPY ["OnceMi.Framework.Repository/OnceMi.Framework.Repository.csproj", "OnceMi.Framework.Repository/"]
COPY ["OnceMi.Framework.Config/OnceMi.Framework.Config.csproj", "OnceMi.Framework.Config/"]
COPY ["OnceMi.Framework.Service/OnceMi.Framework.Service.csproj", "OnceMi.Framework.Service/"]
COPY ["OnceMi.AspNetCore.AutoInjection/OnceMi.AspNetCore.AutoInjection.csproj", "OnceMi.AspNetCore.AutoInjection/"]
RUN dotnet restore "OnceMi.Framework.Api/OnceMi.Framework.Api.csproj"
COPY . .
WORKDIR "/src/OnceMi.Framework.Api"
RUN dotnet build "OnceMi.Framework.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OnceMi.Framework.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OnceMi.Framework.Api.dll"]