#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
ADD http://ftp.us.debian.org/debian/pool/main/c/ca-certificates/ca-certificates_20210119_all.deb .
RUN dpkg -i ca-certificates_20210119_all.deb
RUN apt-get update && apt-get install -y ca-certificates && update-ca-certificates && rm -rf /var/lib/apt/lists/*
WORKDIR /src
COPY ["CC.ElectronicCommerce.UserMicroservice/CC.ElectronicCommerce.UserMicroservice.csproj", "CC.ElectronicCommerce.UserMicroservice/"]
COPY ["CC.ElectronicCommerce.Interface/CC.ElectronicCommerce.Interface.csproj", "CC.ElectronicCommerce.Interface/"]
COPY ["CC.ElectronicCommerce.DTOModel/CC.ElectronicCommerce.DTOModel.csproj", "CC.ElectronicCommerce.DTOModel/"]
COPY ["CC.ElectronicCommerce.Model/CC.ElectronicCommerce.Model.csproj", "CC.ElectronicCommerce.Model/"]
COPY ["CC.ElectronicCommerce.Common/CC.ElectronicCommerce.Common.csproj", "CC.ElectronicCommerce.Common/"]
COPY ["CC.ElectronicCommerce.Core/CC.ElectronicCommerce.Core.csproj", "CC.ElectronicCommerce.Core/"]
COPY ["CC.ElectronicCommerce.Service/CC.ElectronicCommerce.Service.csproj", "CC.ElectronicCommerce.Service/"]
RUN dotnet restore "CC.ElectronicCommerce.UserMicroservice/CC.ElectronicCommerce.UserMicroservice.csproj"
COPY . .
WORKDIR "/src/CC.ElectronicCommerce.UserMicroservice"
RUN dotnet build "CC.ElectronicCommerce.UserMicroservice.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CC.ElectronicCommerce.UserMicroservice.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CC.ElectronicCommerce.UserMicroservice.dll"]