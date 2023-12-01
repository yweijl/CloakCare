param location string
param tags object = {}
param asName string
param serverFarmId string
param environment string
param dbName string
param containerName string
param patientId string

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: '${asName}-Insight-ws'
  location: location
  tags: tags
  properties: {
    features: {
      immediatePurgeDataOn30Days: true
    }
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
    retentionInDays: 30
    sku: {
      name: 'PerGB2018'
    }
    workspaceCapping: {
      dailyQuotaGb: 1
    }
  }
}

resource appInsight 'Microsoft.Insights/components@2020-02-02' = {
  name: '${asName}-Insight'
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Flow_Type: 'Bluefield'
    IngestionMode: 'LogAnalytics'
    RetentionInDays: 30
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
    WorkspaceResourceId: logAnalytics.id
  }
}

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
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: environment
        }
        {
          name: 'CosmosSettings__DbName'
          value: dbName
        }
        {
          name: 'CosmosSettings__Endpoint'
          value: 'https://${dbName}.documents.azure.com:443/'
        }
        {
          name: 'CosmosSettings__Container'
          value: containerName
        }
        {
          name: 'CosmosSettings__PatientId'
          value: patientId
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsight.properties.InstrumentationKey
        }
      ]
    }
  }
}

output appPrincipalId string = appService.identity.principalId
