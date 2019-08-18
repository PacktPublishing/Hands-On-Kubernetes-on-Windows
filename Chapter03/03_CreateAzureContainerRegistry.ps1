Param(
    [Parameter()]
    [string]$azureLocation = "westeurope",
    [Parameter()]
    [string]$resourceGroupName = "acr-resource-group",
    [Parameter()]
    [string]$containerRegistryName = "handsonkubernetesonwinregistry"
)

# You can omit logging in if you are already logged in.
az login

az group create `
   --name $resourceGroupName `
   --location $azureLocation

az acr create `
   --resource-group $resourceGroupName `
   --name $containerRegistryName `
   --sku Basic

az acr login `
   --name $containerRegistryName
