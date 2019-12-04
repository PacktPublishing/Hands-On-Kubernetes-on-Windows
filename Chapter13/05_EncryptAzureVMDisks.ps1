Param(
    [Parameter()]
    [string]$azureLocation = "westeurope",
    [Parameter()]
    [string]$resourceGroupName = "aks-engine-windows-resource-group",
    [Parameter()]
    [string]$encryptionKeyVaultName = "AksEngineEncryptionVault",
    [Parameter(mandatory=$true)]
    [string]$vmName
)

# You can omit logging in if you are already logged in.
az login

az keyvault create `
   --resource-group "$resourceGroupName" `
   --name "$encryptionKeyVaultName" `
   --location "$azureLocation"

az keyvault update `
   --resource-group "$resourceGroupName" `
   --name "$encryptionKeyVaultName" `
   --enabled-for-disk-encryption "true"

az vm encryption enable `
   --resource-group "$resourceGroupName" `
   --name "$vmName" `
   --disk-encryption-keyvault "$encryptionKeyVaultName" `
   --volume-type All

az vm encryption show `
   --resource-group "$resourceGroupName" `
   --name "$vmName"