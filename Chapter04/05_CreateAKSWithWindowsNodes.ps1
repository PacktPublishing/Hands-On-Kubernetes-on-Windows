Param(
    [Parameter()]
    [string]$azureLocation = "westeurope",
    [Parameter()]
    [string]$resourceGroupName = "aks-windows-resource-group",
    [Parameter()]
    [string]$aksClusterName = "aks-windows-cluster",
    [Parameter()]
    [string]$kubernetesVersion = "1.15.3",
    [Parameter()]
    [string]$windowsNodePoolName = "w1pool",
    [Parameter()]
    [string]$windowsUserName = "azureuser",
    [Parameter()]
    [string]$windowsPassword
)

# Run as ADMINISTRATOR.
# You can omit logging in if you are already logged in.
Connect-AzAccount
az login

az extension add --name aks-preview
az extension update --name aks-preview

az feature register `
   --name WindowsPreview `
   --namespace Microsoft.ContainerService

do {
    $featureInfo = az feature list `
                      -o table `
                      --query "[?contains(name, 'Microsoft.ContainerService/WindowsPreview')].{Name:name,State:properties.state}"
    $featureStatus = $featureInfo[2].Split()[2]
    if ($featureStatus -eq "Registered") {
        break;
    }

    Write-Host "Microsoft.ContainerService/WindowsPreview is still in state $featureStatus, waiting..."
    Start-Sleep -Seconds 10
} while ($featureInfo) 


az provider register `
   --namespace Microsoft.ContainerService

az group create `
   --name $resourceGroupName `
   --location $azureLocation

az aks create `
   --resource-group $resourceGroupName `
   --name $aksClusterName `
   --node-count 2 `
   --enable-addons monitoring `
   --kubernetes-version $kubernetesVersion `
   --generate-ssh-keys `
   --windows-admin-username $windowsUserName `
   --windows-admin-password $windowsPassword `
   --enable-vmss `
   --network-plugin azure

az aks nodepool add `
   --resource-group $resourceGroupName `
   --cluster-name $aksClusterName `
   --os-type Windows `
   --name $windowsNodePoolName l `
   --node-count 1 `
   --kubernetes-version $kubernetesVersion

az aks install-cli

az aks get-credentials `
   --resource-group $resourceGroupName `
   --name $aksClusterName

kubectl get nodes
kubectl get pods --all-namespaces

kubectl apply -f https://raw.githubusercontent.com/PacktPublishing/Hands-On-Kubernetes-on-Windows/master/Chapter04/06_windows-example/windows-example.yaml --record