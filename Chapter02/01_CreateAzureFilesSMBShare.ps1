 Param(
    [Parameter()]
    [string]$azureLocation = "westeurope",
    [Parameter()]
    [string]$resourceGroupName = "docker-storage-resource-group",
    [Parameter()]
    [string]$storageAccountName = "dockerstorageaccount",
    [Parameter()]
    [string]$fileShareName = "docker-bind-mount-share",
    [Parameter()]
    [string]$globalMappingLocalPath = "G:"
)

# Run as ADMINISTRATOR.
# You can omit logging in if you are already logged in.
Connect-AzAccount
az login

az group create `
   --name $resourceGroupName `
   --location $azureLocation

az storage account create `
   --name $storageAccountName `
   --resource-group $resourceGroupName `
   --location $azureLocation `
   --sku Standard_RAGRS `
   --kind StorageV2

$azureStorageAccountConnString = az storage account show-connection-string `
    --name $storageAccountName `
    --resource-group $resourceGroupName `
    --query "connectionString" `
    --output tsv

if (!$azureStorageAccountConnString) {
    Write-Error "Couldn't retrieve the connection string."
}

az storage share create `
   --name $fileShareName `
   --quota 2 `
   --connection-string $azureStorageAccountConnString

$storageAccount = Get-AzStorageAccount `
    -ResourceGroupName $resourceGroupName `
    -Name $storageAccountName
$storageAccountKeys = Get-AzStorageAccountKey `
    -ResourceGroupName $resourceGroupName `
    -Name $storageAccountName

Invoke-Expression -Command `
    ("cmdkey /add:$([System.Uri]::new($storageAccount.Context.FileEndPoint).Host) " + `
    "/user:AZURE\$($storageAccount.StorageAccountName) /pass:$($storageAccountKeys[0].Value)")

$fileShare = Get-AzStorageShare -Context $storageAccount.Context | Where-Object { 
    $_.Name -eq $fileShareName -and $_.IsSnapshot -eq $false
}

if ($fileShare -eq $null) {
    Write-Error "Azure File share not found"
}

$password = ConvertTo-SecureString `
    -String $storageAccountKeys[0].Value `
    -AsPlainText `
    -Force
$credential = New-Object System.Management.Automation.PSCredential `
    -ArgumentList "AZURE\$($storageAccount.StorageAccountName)", $password

New-SmbGlobalMapping `
    -RemotePath "\\$($fileShare.StorageUri.PrimaryUri.Host)\$($fileShare.Name)" `
    -Credential $credential `
    -Persistent $true `
    -LocalPath $globalMappingLocalPath