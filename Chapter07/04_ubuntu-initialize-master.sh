#!/bin/bash

sudo kubeadm init --service-cidr "10.96.0.0/12" --pod-network-cidr "10.244.0.0/16"

mkdir -p $HOME/.kube
sudo cp -i /etc/kubernetes/admin.conf $HOME/.kube/config
sudo chown $(id -u):$(id -g) $HOME/.kube/config

kubectl taint nodes --all node-role.kubernetes.io/master-

kubeadm token create --print-join-command