# Works on Intel and Apple Silicon
FROM ubuntu:22.04

# Basic deps
RUN apt-get update && DEBIAN_FRONTEND=noninteractive apt-get install -y \
    ca-certificates curl jq git tar unzip sudo \
 && rm -rf /var/lib/apt/lists/*

# Create a non-root user to run the agent
RUN useradd -m agent && adduser agent sudo && echo "agent ALL=(ALL) NOPASSWD:ALL" >> /etc/sudoers
WORKDIR /azp
COPY start.sh /azp/start.sh
RUN chmod +x /azp/start.sh && chown -R agent:agent /azp

USER agent
ENTRYPOINT ["/azp/start.sh"]