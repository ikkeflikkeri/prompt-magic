# syntax=docker/dockerfile:1

# 1) Build the SvelteKit SPA
FROM node:25-alpine AS web
WORKDIR /web
RUN corepack enable
COPY web/package.json web/pnpm-lock.yaml web/pnpm-workspace.yaml ./
RUN pnpm install --frozen-lockfile
COPY web/ ./
RUN pnpm build

# 2) Build and publish the .NET API
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api
WORKDIR /src
COPY global.json Directory.Build.props Directory.Packages.props PromptMagic.slnx ./
COPY src/ src/
RUN dotnet publish src/PromptMagic.Api -c Release -o /app /p:UseAppHost=false

# 3) Runtime image: API + static SPA in wwwroot
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=api /app ./
COPY --from=web /web/build ./wwwroot
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "PromptMagic.Api.dll"]
