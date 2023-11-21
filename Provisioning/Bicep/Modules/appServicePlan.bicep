param location string
param tags object = {}
param aspName string
param aspSku string
param aspKind string

resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: aspName
  location: location
  tags: tags
  properties: {
    reserved: true
  }
  sku: {
    name: aspSku
  }
  kind: aspKind
}

output id string = appServicePlan.id
