param name string
@allowed([ 'westeurope' ])
param location string
param tags object = {}

param environment string

param aspKind string
param aspSku string
param containerName string
param patientId string

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
    appsettings: [
      {
        name: 'ASPNETCORE_ENVIRONMENT'
        value: environment
      }
      {
        name: 'CosmosSettings__DbName'
        value: '${name}-db'
      }
      {
        name: 'CosmosSettings__Endpoint'
        value: 'https://${name}-db.documents.azure.com:443/'
      }
      {
        name: 'CosmosSettings__Container'
        value: containerName
      }
      {
        name: 'CosmosSettings__PatientId'
        value: patientId
      }
    ]
  }
}

module cosmosDb 'Modules/cosmos.bicep' = {
  scope: resourceGroup
  name: '${name}-db'
  params: {
    dbName: '${name}-db'
    containerName: containerName
    location: location
    appPrincipalId: appService.outputs.appPrincipalId
  }
}
