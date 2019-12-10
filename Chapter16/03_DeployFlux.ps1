kubectl create namespace fluxcd

helm repo add fluxcd https://charts.fluxcd.io
helm upgrade -i flux fluxcd/flux `
     --namespace fluxcd `
     --set "nodeSelector.`"kubernetes\.io/os`"=linux" `
     --set "memcached.nodeSelector.`"kubernetes\.io/os`"=linux" `
     --set "helmOperator.nodeSelector.`"kubernetes\.io/os`"=linux" `
     --set git.url=git@github.com:hands-on-kubernetes-on-windows/voting-application-flux `
     --debug

kubectl apply -f https://raw.githubusercontent.com/fluxcd/helm-operator/helm-v3-dev/deploy/flux-helm-release-crd.yaml

helm upgrade -i helm-operator fluxcd/helm-operator `
     --namespace fluxcd `
     --set git.ssh.secretName=flux-git-deploy `
     --set configureRepositories.enable=true `
     --set configureRepositories.repositories[0].name=stable `
     --set configureRepositories.repositories[0].url=https://kubernetes-charts.storage.googleapis.com `
     --set extraEnvs[0].name=HELM_VERSION `
     --set extraEnvs[0].value=v3 `
     --set image.repository=docker.io/fluxcd/helm-operator-prerelease `
     --set image.tag=helm-v3-dev-ca9c8ba0 `
     --set "nodeSelector.`"kubernetes\.io/os`"=linux" 

fluxctl identity --k8s-fwd-ns fluxcd