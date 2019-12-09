FROM ubuntu:18.04

ARG ETCD_VERSION="v3.3.15"

WORKDIR /temp
RUN apt-get update \
 && apt-get install curl -y \
 && curl -L https://github.com/coreos/etcd/releases/download/$ETCD_VERSION/etcd-$ETCD_VERSION-linux-amd64.tar.gz -o etcd-$ETCD_VERSION-linux-amd64.tar.gz \
 && tar xzvf etcd-$ETCD_VERSION-linux-amd64.tar.gz \
 && rm etcd-$ETCD_VERSION-linux-amd64.tar.gz \
 && cd etcd-$ETCD_VERSION-linux-amd64 \
 && cp etcdctl /usr/local/bin/ \
 && rm -rf etcd-$ETCD_VERSION-linux-amd64

RUN curl -sL https://aka.ms/InstallAzureCLIDeb | bash

WORKDIR /backup-worker
COPY ./docker-entrypoint.sh .
RUN chmod +x docker-entrypoint.sh

ENTRYPOINT ["/backup-worker/docker-entrypoint.sh"]