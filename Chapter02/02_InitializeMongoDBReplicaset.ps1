 Param(
    [Parameter()]
    [string]$dockerNetworkName = "mongo-cluster",
    [Parameter()]
    [string]$globalMappingLocalPath = "G:"
)

docker network create --driver nat $dockerNetworkName

New-Item -ItemType Directory -Force -Path $globalMappingLocalPath\MongoData1\db
New-Item -ItemType Directory -Force -Path $globalMappingLocalPath\MongoData1\configdb
New-Item -ItemType Directory -Force -Path $globalMappingLocalPath\MongoData2\db
New-Item -ItemType Directory -Force -Path $globalMappingLocalPath\MongoData2\configdb
New-Item -ItemType Directory -Force -Path $globalMappingLocalPath\MongoData3\db
New-Item -ItemType Directory -Force -Path $globalMappingLocalPath\MongoData3\configdb

docker run -d `
           --isolation=process `
           --volume $globalMappingLocalPath\MongoData1\db:C:\data\db `
           --volume $globalMappingLocalPath\MongoData1\configdb:C:\data\configdb `
           --name mongo-node1 `
           --net mongo-cluster `
           mongo-1903:latest `
           mongod --bind_ip_all --replSet replSet0

docker run -d `
           --isolation=process `
           --volume $globalMappingLocalPath\MongoData2\db:C:\data\db `
           --volume $globalMappingLocalPath\MongoData2\configdb:C:\data\configdb `
           --name mongo-node2 `
           --net mongo-cluster `
           mongo-1903:latest `
           mongod --bind_ip_all --replSet replSet0

docker run -d `
           --isolation=process `
           --volume $globalMappingLocalPath\MongoData3\db:C:\data\db `
           --volume $globalMappingLocalPath\MongoData3\configdb:C:\data\configdb `
           --name mongo-node3 `
           --net mongo-cluster `
           mongo-1903:latest `
           mongod --bind_ip_all --replSet replSet0

docker exec -it mongo-node1 mongo