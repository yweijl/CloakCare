param location string
param tags object = {}
param asName string
param serverFarmId string

resource appService 'Microsoft.Web/sites@2022-09-01' = {
  name: asName
  location: location
  tags: tags
  properties: {
    reserved: true
    serverFarmId: serverFarmId
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|7.0'
    }
  }
}
