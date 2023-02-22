FROM ubuntu
RUN apt update && apt install -y libicu70 && rm -rf /var/lib/apt/lists/*

COPY ./artifact/BmwDiscovery /opt/BmwDiscovery
ENTRYPOINT ["/opt/BmwDiscovery"]