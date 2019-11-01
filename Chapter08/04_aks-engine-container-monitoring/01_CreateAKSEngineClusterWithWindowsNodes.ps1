Param(
    [Parameter()]
    [string]$azureLocation = "westeurope",
    [Parameter()]
    [string]$resourceGroupName = "aks-engine-windows-resource-group",
    [Parameter()]
    [string]$deploymentName = "kubernetes-windows-cluster",
    [Parameter()]
    [string]$windowsUserName = "azureuser",
    [Parameter(mandatory=$true)]
    [ValidatePattern("^(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%\^&\*\(\)])[a-zA-Z\d!@#$%\^&\*\(\)]{12,123}$")]
    [string]$windowsPassword,
    [Parameter(mandatory=$true)]
    [string]$azureSubscriptionId,
    [Parameter(mandatory=$true)]
    [string]$dnsPrefix
)

# You can omit logging in if you are already logged in.
az login

az group create `
   --name $resourceGroupName `
   --location $azureLocation

$servicePrincipal = az ad sp create-for-rbac `
   --role="Contributor" `
   --scopes="/subscriptions/$azureSubscriptionId/resourceGroups/$resourceGroupName" `
| ConvertFrom-Json

az role assignment create `
   --assignee $servicePrincipal.appId `
   --role "Log Analytics Contributor" `
   --scope="/subscriptions/$azureSubscriptionId"

if (!(Test-Path "~\.ssh\id_rsa.pub")) {
    ssh-keygen
}


Invoke-WebRequest -UseBasicParsing https://raw.githubusercontent.com/PacktPublishing/Hands-On-Kubernetes-on-Windows/master/Chapter08/05_windows-apimodel-container-monitoring/kubernetes-windows.json -OutFile kubernetes-windows-template.json
$apimodel = Get-Content .\kubernetes-windows-template.json | ConvertFrom-Json
$apimodel.properties.masterProfile.dnsPrefix = $dnsPrefix
$apimodel.properties.windowsProfile.adminUsername = $windowsUserName
$apimodel.properties.windowsProfile.adminPassword = $windowsPassword
$apimodel.properties.linuxProfile.ssh.publicKeys[0].keyData = [string](Get-Content "~/.ssh/id_rsa.pub")
$apimodel.properties.servicePrincipalProfile.clientId = $servicePrincipal.appId
$apimodel.properties.servicePrincipalProfile.secret = $servicePrincipal.password
$apimodel | ConvertTo-Json -Depth 10 | Out-File -Encoding ascii -FilePath "kubernetes-windows.json"


aks-engine deploy `
           --subscription-id $azureSubscriptionId `
           --resource-group $resourceGroupName `
           --location $azureLocation `
           --api-model .\kubernetes-windows.json `
           --client-id $servicePrincipal.appId `
           --client-secret $servicePrincipal.password `
           --force-overwrite

$env:KUBECONFIG=".\_output\$dnsPrefix\kubeconfig\kubeconfig.$azureLocation.json;$env:USERPROFILE\.kube\config"

kubectl config use-context $dnsPrefix
kubectl get nodes
kubectl get pods --all-namespaces