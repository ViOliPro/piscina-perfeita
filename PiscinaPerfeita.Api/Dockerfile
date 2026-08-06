# syntax=docker/dockerfile:1

# ============================================================
#  PiscinaPerfeita.Api — Dockerfile
#  Build multi-stage: compila com o SDK, roda com o runtime ASP.NET
#  (imagem final bem menor e sem ferramentas de build).
# ============================================================

# ---- Build stage ----
# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1. Copia o arquivo da solução (ou projeto) mantendo a estrutura
COPY src/PiscinaPerfeita.Api.csproj src/
COPY src/PiscinaPerfeita.Api.slnx src/

# 2. Faz o restore especificando o caminho da solução ou do projeto
RUN dotnet restore src/PiscinaPerfeita.Api.csproj

# 3. Copia todo o código-fonte a partir da raiz do contexto
COPY . .

# 4. Entra na pasta do projeto e compila
WORKDIR /src/src
RUN dotnet publish -c Release -o /app/publish --no-restore

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV ASPNETCORE_URLS=http://+:10000
ENV ASPNETCORE_ENVIRONMENT=Production

# icu-libs: necessário com DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
# (a aplicação usa a cultura pt-BR — ver Program.cs).
# curl: usado apenas pelo HEALTHCHECK abaixo.
RUN apk add --no-cache icu-libs curl

# Executa como usuário sem privilégios — boa prática de segurança em
# containers (evita rodar o processo da aplicação como root).
RUN addgroup -S piscina && adduser -S piscina -G piscina

COPY --from=build --chown=piscina:piscina /app/publish .

USER piscina

EXPOSE 10000

HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
  CMD curl -f http://localhost:10000/health || exit 1

ENTRYPOINT ["dotnet", "PiscinaPerfeita.Api.dll"]
