FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY Farming.csproj ./
RUN dotnet restore Farming.csproj
COPY . .
RUN dotnet publish Farming.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
# QuestPDF (SkiaSharp) needs fontconfig on Linux.
RUN apt-get update \
    && apt-get install -y --no-install-recommends libfontconfig1 \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production

# The app reads one key=value connection string from DB_CONNECTION (see AddDatabase in
# MyDbContext.cs). Use it as-is when set (Neon); otherwise assemble it from the separate
# DB_HOST/DB_PORT/DB_NAME/DB_USER/DB_PASSWORD values (Render's own Postgres, local testing).
CMD ["sh", "-c", "if [ -z \"$DB_CONNECTION\" ]; then export DB_CONNECTION=\"Host=${DB_HOST};Port=${DB_PORT};Database=${DB_NAME};Username=${DB_USER};Password='${DB_PASSWORD}'\"; fi; exec dotnet Farming.dll"]
