#!/bin/bash
# Must be run as root or with sudo

apt-get update
apt-get dist-upgrade -y
apt-get install apt-transport-https ca-certificates curl software-properties-common ebtables ethtool -y 

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