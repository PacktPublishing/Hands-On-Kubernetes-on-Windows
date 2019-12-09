#!/bin/bash

snapshot_file="kubernetes-etcd-snapshot_$(date +"%Y%m%d_%H%M%S").db"

ETCDCTL_API=3 etcdctl \
   --endpoints=$SNAPSHOT_ETCD_ENDPOINTS \
   --cacert=/etc/kubernetes/certs/ca.crt \
   --cert=/etc/kubernetes/certs/etcdclient.crt \
   --key=/etc/kubernetes/certs/etcdclient.key \
   --debug \
   snapshot save \
   $snapshot_file

ETCDCTL_API=3 etcdctl --write-out=table snapshot status $snapshot_file


az login --service-principal \
   -u $SNAPSHOT_AZURE_PRINCIPAL_APPID \
   -p $SNAPSHOT_AZURE_PRINCIPAL_PASSWORD \
   --tenant $SNAPSHOT_AZURE_PRINCIPAL_TENANT

az storage container create \
   --account-name $SNAPSHOT_AZURE_ACCOUNT_NAME \
   --account-key "$SNAPSHOT_AZURE_ACCOUNT_KEY" \
   --name $SNAPSHOT_AZURE_CONTAINER_NAME

az storage blob upload \
   --account-name $SNAPSHOT_AZURE_ACCOUNT_NAME \
   --account-key "$SNAPSHOT_AZURE_ACCOUNT_KEY" \
   --container-name $SNAPSHOT_AZURE_CONTAINER_NAME \
   --name $snapshot_file \
   --file $snapshot_file

rm -f $snapshot_file

echo "Backup $snapshot_file uploaded successfully!"