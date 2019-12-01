Param(
    [Parameter()]
    [string]$repository = "packtpubkubernetesonwindows/voting-application",
    [Parameter(mandatory=$true)]
    [string]$version
)

$patchVersion = $version
$minorVersion = $version.split(".")[0..1] -Join "."
$majorVersion = $version.split(".")[0]

docker build -t $repository`:$patchVersion -f .\Dockerfile.production .

docker tag $repository`:$patchVersion $repository`:$minorVersion
docker tag $repository`:$patchVersion $repository`:$majorVersion
docker tag $repository`:$patchVersion $repository`:latest

docker push $repository`:$patchVersion
docker push $repository`:$minorVersion
docker push $repository`:$majorVersion
docker push $repository`:latest