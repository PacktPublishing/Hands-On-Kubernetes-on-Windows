Param(
    [Parameter()]
    [string]$aksEngineAdminsGroupName = "AksEngineAdmins",
    [Parameter(mandatory=$true)]
    [string]$dnsPrefix
)

# You can omit logging in if you are already logged in.
az login


$serverApplicationId = az ad app create `
    --display-name "${dnsPrefix}Server" `
    --identifier-uris "https://${dnsPrefix}Server" `
    --query appId -o tsv

az ad app update `
   --id $serverApplicationId `
   --set groupMembershipClaims=All

az ad sp create `
   --id $serverApplicationId

$serverApplicationSecret = az ad sp credential reset `
    --name $serverApplicationId `
    --credential-description "AKSPassword" `
    --query password -o tsv

az ad app permission add `
   --id $serverApplicationId `
   --api 00000003-0000-0000-c000-000000000000 `
   --api-permissions e1fe6dd8-ba31-4d61-89e7-88639da4683d=Scope 06da0dbc-49e2-44d2-8312-53f166ab848a=Scope 7ab1d382-f21e-4acd-a863-ba3e13f7da61=Role

az ad app permission grant `
   --id $serverApplicationId `
   --api 00000003-0000-0000-c000-000000000000
   
az ad app permission admin-consent `
   --id $serverApplicationId



$clientApplicationId = az ad app create `
    --display-name "${dnsPrefix}Client" `
    --native-app `
    --reply-urls "https://${dnsPrefix}Client" `
    --query appId -o tsv

az ad sp create `
   --id $clientApplicationId

$oauth2PermissionId = az ad app show `
    --id $serverApplicationId `
    --query "oauth2Permissions[0].id" -o tsv

az ad app permission add `
   --id $clientApplicationId `
   --api $serverApplicationId `
   --api-permissions $oauth2PermissionId=Scope

az ad app permission grant `
   --id $clientApplicationId `
   --api $serverApplicationId


$adminGroupId = az ad group create `
    --display-name $aksEngineAdminsGroupName `
    --mail-nickname $aksEngineAdminsGroupName `
    --query "objectId" -o tsv

$currentUserObjectId = az ad signed-in-user show `
    --query "objectId" -o tsv

az ad group member add `
   --group $aksEngineAdminsGroupName `
   --member-id $currentUserObjectId

$tenantId = az account show `
    --query "tenantId" -o tsv


echo @"
"aadProfile": {
  "serverAppID": "$serverApplicationId",
  "clientAppID": "$clientApplicationId",
  "tenantID": "$tenantId",
  "adminGroupID": "$adminGroupId"
}
"@