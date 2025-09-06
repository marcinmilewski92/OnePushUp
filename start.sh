#!/usr/bin/env bash
set -euo pipefail

: "${AZP_URL:?Set AZP_URL (e.g. https://dev.azure.com/your-org)}"
: "${AZP_TOKEN:?Set AZP_TOKEN (PAT with Agent Pools (Read & manage))}"

AZP_POOL="${AZP_POOL:-Default}"
AZP_AGENT_NAME="${AZP_AGENT_NAME:-$(hostname)}"
AZP_WORK="${AZP_WORK:-_work}"

# Let the agent (and tool installers) know where to cache tools
export AGENT_TOOLSDIRECTORY="${AGENT_TOOLSDIRECTORY:-/azp/_work/_tool}"

# Make sure mounted paths exist and are owned by the 'agent' user
sudo mkdir -p "/azp/${AZP_WORK}" "$AGENT_TOOLSDIRECTORY"
sudo chown -R agent:agent "/azp/${AZP_WORK}" "$AGENT_TOOLSDIRECTORY"

cd /azp/agent

cleanup() {
  echo "Removing agent from pool..."
  ./config.sh remove --unattended --auth pat --token "$AZP_TOKEN" || true
}
trap 'cleanup; exit 0' SIGINT SIGTERM

if [ ! -f "./config.sh" ]; then
  echo "ERROR: Agent files missing in image. Did you build the image correctly?"
  ls -la /azp/agent || true
  exit 1
fi

./config.sh --unattended \
  --agent "$AZP_AGENT_NAME" \
  --url "$AZP_URL" \
  --auth pat \
  --token "$AZP_TOKEN" \
  --pool "$AZP_POOL" \
  --work "/azp/${AZP_WORK}" \
  --replace \
  --acceptTeeEula

./run.sh