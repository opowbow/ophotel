ARG DOTNET_VERSION=6.0

FROM mcr.microsoft.com/dotnet/sdk:$DOTNET_VERSION AS build
LABEL maintainer="paul.farrington@goldmedal.co.uk"

ARG VersionPrefix=0.0.0
WORKDIR /src
COPY hotels.csproj .
RUN dotnet restore hotels.csproj

COPY . .
RUN dotnet build -c Release --no-restore /p:VersionPrefix=${VersionPrefix}

FROM build AS publish
RUN dotnet publish -c Release --no-restore --no-build -o /out

FROM mcr.microsoft.com/dotnet/aspnet:$DOTNET_VERSION AS runtime

# Download and install the Datadog Tracer
RUN apt-get update && apt-get install -y curl
RUN mkdir -p /opt/datadog \
    && mkdir -p /var/log/datadog \
    && TRACER_VERSION=$(curl -s https://api.github.com/repos/DataDog/dd-trace-dotnet/releases/latest | grep tag_name | cut -d '"' -f 4 | cut -c2-) \
    && curl -LO https://github.com/DataDog/dd-trace-dotnet/releases/download/v${TRACER_VERSION}/datadog-dotnet-apm_${TRACER_VERSION}_amd64.deb \
    && dpkg -i ./datadog-dotnet-apm_${TRACER_VERSION}_amd64.deb \
    && /opt/datadog/createLogPath.sh \
    && rm ./datadog-dotnet-apm_${TRACER_VERSION}_amd64.deb

# Enable the Datadog tracer
ENV CORECLR_ENABLE_PROFILING=1
ENV CORECLR_PROFILER={846F5F1C-F9AE-4B07-969E-05C26BC060D8}
ENV CORECLR_PROFILER_PATH=/opt/datadog/Datadog.Trace.ClrProfiler.Native.so
ENV DD_DOTNET_TRACER_HOME=/opt/datadog
ENV DD_INTEGRATIONS=/opt/datadog/integrations.json
ENV DD_RUNTIME_METRICS_ENABLED=true
ENV DD_LOGS_INJECTION=true
ENV DD_TRACE_AGENT_URL='unix:///var/run/datadog/apm.socket'
ENV LD_PRELOAD=/opt/datadog/continuousprofiler/Datadog.Linux.ApiWrapper.x64.so
ENV DD_PROFILING_ENABLED=1

WORKDIR /app
COPY --from=build /src/waster.xml .
COPY --from=publish /out .

EXPOSE 80
ENTRYPOINT ["dotnet", "hotels.dll"]