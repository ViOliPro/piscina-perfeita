FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final

WORKDIR /app

COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:10000

EXPOSE 10000

USER $APP_UID

ENTRYPOINT ["dotnet", "PiscinaPerfeita.Api.dll"]