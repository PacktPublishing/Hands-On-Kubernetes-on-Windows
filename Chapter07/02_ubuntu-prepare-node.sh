#!/bin/bash
# Must be run as root or with sudo

apt-get update
apt-get dist-upgrade -y
apt-get install apt-transport-https ca-certificates curl software-properties-common ebtables ethtool -y 

# Configure OS
swapoff -a
sed -e '/swap/ s/^#*/#/' -i /etc/fstab

cat << EOF > /etc/modules-load.d/kubernetes.conf
br_netfilter
EOF

cat << EOF > /etc/sysctl.d/99-kubernetes.conf
net.bridge.bridge-nf-call-ip6tables = 1
net.bridge.bridge-nf-call-iptables = 1
net.bridge.bridge-nf-call-arptables = 1
EOF

modprobe br_netfilter
sysctl --system

# Install Docker
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | apt-key add -

add-apt-repository \
  "deb [arch=amd64] https://download.docker.com/linux/ubuntu \
  $(lsb_release -cs) \
  stable"

apt-get update
apt-get install docker-ce=5:18.09.9~3-0~ubuntu-bionic -y
apt-mark hold docker-ce

cat << EOF > /etc/docker/daemon.json
{
  "exec-opts": ["native.cgroupdriver=systemd"],
  "log-driver": "json-file",
  "log-opts": {
    "max-size": "100m"
  },
  "storage-driver": "overlay2"
}
EOF

systemctl daemon-reload
systemctl restart docker

docker run hello-world