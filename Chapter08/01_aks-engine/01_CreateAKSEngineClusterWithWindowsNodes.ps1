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

echo "Your AKS Engine Service Principal:\n$servicePrincipal"

if (!(Test-Path "~\.ssh\id_rsa.pub")) {
    ssh-keygen
}


Invoke-WebRequest -UseBasicParsing https://raw.githubusercontent.com/PacktPublishing/Hands-On-Kubernetes-on-Windows/master/Chapter08/02_windows-apimodel/kubernetes-windows.json -OutFile kubernetes-windows-template.json
$apimodel = Get-Content .\kubernetes-windows-template.json | ConvertFrom-Json
$apimodel.properties.masterProfile.dnsPrefix = $dnsPrefix
$apimodel.properties.windowsProfile.adminUsername = $windowsUserName
$apimodel.properties.windowsProfile.adminPassword = $windowsPassword
$apimodel.properties.linuxProfile.ssh.publicKeys[0].keyData = [string](Get-Content "~/.ssh/id_rsa.pub")
$apimodel.properties.servicePrincipalProfile.clientId = $servicePrincipal.appId
$apimodel.properties.servicePrincipalProfile.secret = $servicePrincipal.password
$apimodel | ConvertTo-Json -Depth 10 | Out-File -Encoding ascii -FilePath "kubernetes-windows.json"


aks-engine generate .\kubernetes-windows.json

az group deployment create `
   --name $deploymentName `
   --resource-group $resourceGroupName `
   --template-file "./_output/$dnsPrefix/azuredeploy.json" `
   --parameters "./_output/$dnsPrefix/azuredeploy.parameters.json"

$env:KUBECONFIG=".\_output\$dnsPrefix\kubeconfig\kubeconfig.$azureLocation.json;$env:USERPROFILE\.kube\config"

kubectl config use-context $dnsPrefix
kubectl get nodes
kubectl get pods --all-namespaces