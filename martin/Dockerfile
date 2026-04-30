FROM debian:bookworm-slim

# Martin version to install — update this to upgrade
ARG MARTIN_VERSION=martin-v1.7.0
# Automatically set by Docker BuildKit to the native CPU (arm64 or amd64) — no emulation needed
ARG TARGETARCH

RUN case "$TARGETARCH" in \
     # Apple Silicon / ARM servers — musl = statically linked, no glibc dependency
     arm64) ARCH="aarch64-unknown-linux-musl"  ;; \
     # Intel / AMD desktops and servers
     amd64) ARCH="x86_64-unknown-linux-musl"   ;; \
     *)     echo "Unsupported arch: $TARGETARCH" && exit 1 ;; \
     esac \
     # Install wget to fetch the martin binary, then remove it after
     && apt-get update && apt-get install -y wget ca-certificates \
     && wget -q "https://github.com/maplibre/martin/releases/download/${MARTIN_VERSION}/martin-${ARCH}.tar.gz" -O /tmp/martin.tar.gz \
     && tar -xzf /tmp/martin.tar.gz -C /tmp && mv /tmp/martin /usr/local/bin/martin && rm /tmp/martin.tar.gz \
     && apt-get purge -y wget && apt-get autoremove -y && apt-get clean && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["martin"]
