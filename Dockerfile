FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY MovieReviewsMVC/*.csproj ./MovieReviewsMVC/
WORKDIR /src/MovieReviewsMVC
RUN dotnet restore

WORKDIR /src
COPY MovieReviewsMVC/. ./MovieReviewsMVC/
WORKDIR /src/MovieReviewsMVC
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MovieReviewsMVC.dll"]
