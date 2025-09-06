#!/usr/bin/env bash
set -euo pipefail

: "${AZP_URL:?Set AZP_URL env var (e.g. https://dev.azure.com/yourorg)}"
: "${AZP_TOKEN:?Set AZP_TOKEN env var (PAT with Agent Pools (Read & manage))}"
AZP_POOL="${AZP_POOL:-Default}"
AZP_AGENT_NAME="${AZP_AGENT_NAME:-$(hostname)}"
AZP_WORK="${AZP_WORK:-_work}"
AGENT_VERSION="${AGENT_VERSION:-3.248.0}"

# Detect arch (Intel vs Apple Silicon)
arch="$(uname -m)"
case "$arch" in
  x86_64) AGENT_ARCH="x64" ;;
  aarch64|arm64) AGENT_ARCH="arm64" ;;
  *) echo "Unsupported arch: $arch"; exit 1 ;;
esac

cd /azp

if [ ! -d "/azp/agent" ]; then
  echo "Downloading Azure Pipelines agent $AGENT_VERSION for $AGENT_ARCH ..."
  mkdir -p /azp/agent
  cd /azp/agent
  curl -LsS \
    "https://vstsagentpackage.azureedge.net/agent/${AGENT_VERSION}/vsts-agent-linux-${AGENT_ARCH}-${AGENT_VERSION}.tar.gz" \
    | tar -xz
  ./bin/installdependencies.sh || true
fi

cleanup() {
  echo "Removing agent from pool..."
  ./config.sh remove --unattended --auth pat --token "$AZP_TOKEN" || true
}
trap 'cleanup; exit 0' SIGINT SIGTERM

cd /azp/agent

# Configure (idempotent if --replace)
./config.sh --unattended \
  --agent "$AZP_AGENT_NAME" \
  --url "$AZP_URL" \
  --auth pat \
  --token "$AZP_TOKEN" \
  --pool "$AZP_POOL" \
  --work "/azp/$AZP_WORK" \
  --replace \
  --acceptTeeEula

# Run (blocks)
./run.sh