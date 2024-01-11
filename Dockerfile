FROM alpine

RUN apk add --no-cache libstdc++ libgcc

COPY ./BmwDiscovery /opt/BmwDiscovery
ENTRYPOINT ["/opt/BmwDiscovery"]