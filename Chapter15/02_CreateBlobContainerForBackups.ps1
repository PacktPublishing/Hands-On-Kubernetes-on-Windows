Param(
    [Parameter()]
    [string]$azureLocation = "westeurope",
    [Parameter()]
    [string]$resourceGroupName = "aks-engine-windows-resource-group",
    [Parameter()]
    [string]$storageAccountName = "aksenginebackups",
    [Parameter(mandatory=$true)]
    [string]$azureSubscriptionId,
    [Parameter(mandatory=$true)]
    [string]$dnsPrefix
)


# You can omit logging in if you are already logged in.
az login

$servicePrincipal = az ad sp create-for-rbac `
   --role="Storage Blob Data Contributor" `
   --scopes="/subscriptions/$azureSubscriptionId/resourceGroups/$resourceGroupName" `
| ConvertFrom-Json


az storage account create `
   --name $storageAccountName `
   --resource-group $resourceGroupName `
   --location $azureLocation `
   --sku Standard_ZRS `
   --encryption blob

$accountKeys = az storage account keys list `
   --account-name $storageAccountName `
   --resource-group $resourceGroupName `
| ConvertFrom-Json


Write-Host "appId: $($servicePrincipal.appId)"
Write-Host "password: $($servicePrincipal.password)"
Write-Host "tenant: $($servicePrincipal.tenant)"
Write-Host "storageAccountKey1: $($accountKeys[0].value)"