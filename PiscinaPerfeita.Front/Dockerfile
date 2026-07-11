# syntax=docker/dockerfile:1

# ============================================================
#  PiscinaPerfeita.Front — Dockerfile
#  Build multi-stage: compila os assets estáticos com Node/Vite,
#  serve com Nginx (imagem final leve, sem Node/toolchain).
# ============================================================

# ---- Build stage ----
FROM node:20-alpine AS build
WORKDIR /app

# corepack garante uma versão consistente do Yarn, independente do que
# vier pré-instalado na imagem base do Node.
RUN corepack enable && corepack prepare yarn@1.22.22 --activate

# Copia primeiro só os manifests para aproveitar o cache de camadas do
# Docker: "yarn install" só roda de novo quando as dependências mudam.
COPY package.json yarn.lock ./
RUN yarn install --frozen-lockfile

COPY . .

# O Vite injeta variáveis VITE_* no bundle em TEMPO DE BUILD (não dá para
# trocar depois em runtime só com uma env var no container). O padrão é
# "/api", relativo ao próprio host que serve o front — funciona out-of-the-box
# com o reverse proxy configurado em nginx.conf (ver server { location /api }).
# Para apontar para outra API, sobrescreva no build: 
#   docker build --build-arg VITE_API_BASE_URL=https://minha-api.com/api .
ARG VITE_API_BASE_URL=/api
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL

RUN yarn build

# ---- Runtime stage ----
FROM nginx:1.27-alpine AS final

COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

HEALTHCHECK CMD wget -q --spider http://127.0.0.1/ || exit 1

CMD ["nginx", "-g", "daemon off;"]
