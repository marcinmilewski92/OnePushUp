# Azure DevOps self-hosted agent (Linux) with .NET + Android toolchain
FROM ubuntu:22.04

ARG DEBIAN_FRONTEND=noninteractive
ARG AGENT_VERSION=3.257.0            # e.g., 3.257.0 or 4.257.0
ARG AGENT_ARCH=arm64                 # use x64 on Intel hosts

# --------------------------------------------------------------------
# Base packages + JDK 17
# --------------------------------------------------------------------
RUN apt-get update && apt-get install -y --no-install-recommends \
    ca-certificates curl wget jq git tar unzip sudo dnsutils iputils-ping gnupg apt-transport-https \
    openjdk-17-jdk \
 && rm -rf /var/lib/apt/lists/*

# Set JAVA_HOME dynamically to support both arm64/amd64 and add to PATH
RUN JH="$(dirname "$(dirname "$(readlink -f "$(which javac)")")")" \
 && echo "Detected JAVA_HOME=$JH" \
 && ln -s "$JH" /usr/lib/jvm/default-java
ENV JAVA_HOME=/usr/lib/jvm/default-java
ENV PATH="$JAVA_HOME/bin:${PATH}"

# --------------------------------------------------------------------
# Create non-root 'agent' user (passwordless sudo)
# --------------------------------------------------------------------
RUN useradd -m agent && adduser agent sudo && echo "agent ALL=(ALL) NOPASSWD:ALL" >> /etc/sudoers

# --------------------------------------------------------------------
# Install .NET SDK 8 & 9 (system-wide via Microsoft apt repo)
# --------------------------------------------------------------------
RUN wget -q https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb \
 && dpkg -i packages-microsoft-prod.deb \
 && rm packages-microsoft-prod.deb \
 && apt-get update && apt-get install -y --no-install-recommends \
    dotnet-sdk-9.0 dotnet-sdk-8.0 \
 && rm -rf /var/lib/apt/lists/*

# --------------------------------------------------------------------
# Android SDK (cmdline-tools) + required packages
# --------------------------------------------------------------------
ENV ANDROID_SDK_ROOT=/opt/android-sdk
RUN mkdir -p ${ANDROID_SDK_ROOT}/cmdline-tools /tmp/android-cli \
 && cd /tmp/android-cli \
 && wget -q https://dl.google.com/android/repository/commandlinetools-linux-11076708_latest.zip \
 && unzip -q commandlinetools-linux-*_latest.zip \
    # move actual cmdline-tools to .../cmdline-tools/latest
 && mkdir -p ${ANDROID_SDK_ROOT}/cmdline-tools/latest \
 && mv cmdline-tools/* ${ANDROID_SDK_ROOT}/cmdline-tools/latest/ \
 && rm -rf /tmp/android-cli

# ensure Android tools on PATH
ENV PATH="${ANDROID_SDK_ROOT}/cmdline-tools/latest/bin:${ANDROID_SDK_ROOT}/platform-tools:${PATH}"

# Accept licenses & install platform tools, build-tools, platform (adjust versions as needed)
RUN yes | ${ANDROID_SDK_ROOT}/cmdline-tools/latest/bin/sdkmanager --sdk_root="${ANDROID_SDK_ROOT}" --licenses || true \
 && ${ANDROID_SDK_ROOT}/cmdline-tools/latest/bin/sdkmanager --sdk_root="${ANDROID_SDK_ROOT}" \
      "platform-tools" \
      "build-tools;35.0.0" \
      "platforms;android-35" \
      "cmdline-tools;latest"

# Make SDK writable by the 'agent' user so workloads/tools can cache stuff
RUN mkdir -p ${ANDROID_SDK_ROOT} && chown -R agent:agent ${ANDROID_SDK_ROOT}

# --------------------------------------------------------------------
# Azure DevOps agent binaries
# --------------------------------------------------------------------
WORKDIR /azp
RUN set -eux; \
    case "${AGENT_VERSION%%.*}" in \
      3) FILE_PREFIX="vsts-agent";; \
      4) FILE_PREFIX="pipelines-agent";; \
      *) echo "Unsupported AGENT_VERSION: ${AGENT_VERSION}"; exit 1;; \
    esac; \
    mkdir -p /azp/agent; cd /azp/agent; \
    URL="https://download.agent.dev.azure.com/agent/${AGENT_VERSION}/${FILE_PREFIX}-linux-${AGENT_ARCH}-${AGENT_VERSION}.tar.gz"; \
    echo "Fetching $URL"; \
    curl -fL "$URL" | tar -xz; \
    test -f /azp/agent/config.sh; \
    /azp/agent/bin/installdependencies.sh || true

# Copy your start script that registers the agent using AZP_* env vars
COPY start.sh /azp/start.sh
RUN chmod +x /azp/start.sh && chown -R agent:agent /azp

# --------------------------------------------------------------------
# Preinstall MAUI workload for the 'agent' user (optional, speeds up builds)
# --------------------------------------------------------------------
USER agent
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
# Let MAUI know where the Android SDK is for this user
ENV ANDROID_SDK_ROOT=/opt/android-sdk
RUN dotnet --info && dotnet workload install maui

# Back to start directory and set entry
WORKDIR /azp
ENTRYPOINT ["/azp/start.sh"]