FROM ubuntu

COPY ./BmwDiscovery /opt/BmwDiscovery
ENTRYPOINT ["/opt/BmwDiscovery"]