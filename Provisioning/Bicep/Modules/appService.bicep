param location string
param tags object = {}
param asName string
param serverFarmId string
param appsettings array

resource appService 'Microsoft.Web/sites@2022-09-01' = {
  name: asName
  location: location
  tags: tags
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    reserved: true
    serverFarmId: serverFarmId
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|7.0'
      appSettings: appsettings
    }
  }
}

output appPrincipalId string = appService.identity.principalId
