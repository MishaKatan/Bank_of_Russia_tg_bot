# Build stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Bank_of_Russia_tg_bot/Bank_of_Russia_tg_bot.csproj", "Bank_of_Russia_tg_bot/"]
RUN dotnet restore "Bank_of_Russia_tg_bot/Bank_of_Russia_tg_bot.csproj"
COPY . .
WORKDIR "/src/Bank_of_Russia_tg_bot"
RUN dotnet publish "Bank_of_Russia_tg_bot.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Bank_of_Russia_tg_bot.dll"]