FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-noble-chiseled

USER $APP_UID
WORKDIR /app
COPY --chown=$APP_UID:$APP_UID ./BmwDiscovery .
ENTRYPOINT ["./BmwDiscovery"]