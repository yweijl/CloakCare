param name string
@allowed([ 'westeurope' ])
param location string
param tags object = {}

param aspKind string
param aspSku string
param containerName string

var resourceGroup = az.resourceGroup('${name}-rg')

module appServicePlan 'Modules/appServicePlan.bicep' = {
  scope: resourceGroup
  name: '${name}-asp'
  params: {
    aspKind: aspKind
    aspName: '${name}-asp'
    aspSku: aspSku
    location: location
    tags: tags
  }
}

module appService 'Modules/appService.bicep' = {
  scope: resourceGroup
  name: '${name}-as'
  params: {
    asName: '${name}-as'
    location: location
    serverFarmId: appServicePlan.outputs.id
    tags: tags
  }
}

module cosmosDb 'Modules/cosmos.bicep' = {
  scope: resourceGroup
  name: '${name}-db'
  params: {
    accountName: '${name}-db-account'
    containerName: containerName
    dbName: '${name}-db'
    location: location
  }
}

output appName string = appService.name
