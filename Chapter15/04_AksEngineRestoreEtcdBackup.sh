#!/bin/bash

# The following environment variables must be passed, for example:
# export RESTORE_ETCD_MEMBER_NAME=k8s-master-50659983-0  # Change for each master node
# export RESTORE_ETCD_INITIAL_ADVERTISE_PEER_URLS=https://10.255.255.5:2380  # Change for each master node
# export RESTORE_ETCD_INITIAL_CLUSTER_TOKEN=k8s-etcd-cluster
# export RESTORE_ETCD_INITIAL_CLUSTER=k8s-master-50659983-0=https://10.255.255.5:2380,k8s-master-50659983-1=https://10.255.255.6:2380,k8s-master-50659983-2=https://10.255.255.7:2380
# export SNAPSHOT_AZURE_PRINCIPAL_APPID=1775963c-8414-434d-839c-db5d417c4293 
# export SNAPSHOT_AZURE_PRINCIPAL_PASSWORD=276952a9-fa51-44ef-b6c6-905e322dbaed 
# export SNAPSHOT_AZURE_PRINCIPAL_TENANT=86be0945-a0f3-44c2-8868-9b6aa96b0b62 
# export SNAPSHOT_AZURE_ACCOUNT_NAME=aksenginebackups 
# export SNAPSHOT_AZURE_ACCOUNT_KEY="L/aJk3W3OzVoS9mPQGKaR/epn/zekxZXMnn2p6ot+ApSFDn1g3WVLR8OClzwzizyTrO7ag6Jn/Ks4p0E+AateA==" 
# export SNAPSHOT_AZURE_CONTAINER_NAME=handson-aks-engine-win3 
# export SNAPSHOT_FILENAME=kubernetes-etcd-snapshot_20191208_182555.db
#
# Example usage: sudo -E ./04_AksEngineRestoreEtcdBackup.sh 
#

if [[ $EUID -ne 0 ]]; then
   echo "This script must be run as root" 
   exit 1
fi

if ! [ -x "$(command -v az)" ]; then
  echo "Installing Azure CLI..."
  curl -sL https://aka.ms/InstallAzureCLIDeb | bash
fi


az login --service-principal \
   -u $SNAPSHOT_AZURE_PRINCIPAL_APPID \
   -p $SNAPSHOT_AZURE_PRINCIPAL_PASSWORD \
   --tenant $SNAPSHOT_AZURE_PRINCIPAL_TENANT

echo "Downloading snapshot $SNAPSHOT_FILENAME..."

az storage blob download \
   --account-name $SNAPSHOT_AZURE_ACCOUNT_NAME \
   --account-key "$SNAPSHOT_AZURE_ACCOUNT_KEY" \
   --container-name $SNAPSHOT_AZURE_CONTAINER_NAME \
   --name $SNAPSHOT_FILENAME \
   --file snapshot.db

echo "Restoring snapshot..."

rm -rf /var/lib/etcddisk-restored
ETCDCTL_API=3 etcdctl snapshot restore snapshot.db \
  --name $RESTORE_ETCD_MEMBER_NAME \
  --initial-cluster $RESTORE_ETCD_INITIAL_CLUSTER \
  --initial-cluster-token $RESTORE_ETCD_INITIAL_CLUSTER_TOKEN \
  --initial-advertise-peer-urls $RESTORE_ETCD_INITIAL_ADVERTISE_PEER_URLS \
  --data-dir=/var/lib/etcddisk-restored \
  --debug

mv /etc/kubernetes/manifests /etc/kubernetes/manifests-stopped
service etcd stop

echo "Waiting for kubelet manifests Pods to stop..."
sleep 15

service kubelet stop
docker stop $(docker ps -q)

read -p "Press any to continue when ALL master nodes are ready to restore the snapshot and start etcd service."

rm -rf /var/lib/etcddisk-old || true
mkdir -p /var/lib/etcddisk-old
mv /var/lib/etcddisk/member /var/lib/etcddisk-old/
mv /var/lib/etcddisk-restored/member /var/lib/etcddisk/
chown etcd -R /var/lib/etcddisk
chgrp etcd -R /var/lib/etcddisk
ls -al /var/lib/etcddisk/member/
rm -rf /var/lib/etcddisk-restored


echo "Starting etcd and kubelet..."

service etcd start
mv /etc/kubernetes/manifests-stopped /etc/kubernetes/manifests
service kubelet start
etcdctl cluster-health
