FROM alpine:3.21

WORKDIR /app

RUN addgroup -S appgroup && adduser -S appuser -G appgroup
RUN apk add --no-cache libstdc++ libgcc
RUN mkdir -p /data /data/config && chown -R appuser:appgroup /data

COPY --chown=appuser:appgroup /build/bin/Release/net10.0/linux-musl-x64/publish/ .

USER appuser

EXPOSE 57206

ENV \
    DOTNET_GCServer=1 \
    DOTNET_GCHeapCount=0 \
    DOTNET_ThreadPool_MinThreads=8 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 \
    DOTNET_EnableDiagnostics=0 \
    DOTNET_TieredPGO=1

ENTRYPOINT ["./gara-server"]