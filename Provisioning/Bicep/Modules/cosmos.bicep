param dbName string
param location string
param tags object = {}
param containerName string
param appPrincipalId string

targetScope = 'resourceGroup'

resource account 'Microsoft.DocumentDB/databaseAccounts@2023-09-15' = {
  name: dbName
  location: location
  tags: tags
  properties: {
    enableFreeTier: true
    databaseAccountOfferType: 'Standard'
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    locations: [
      {
        locationName: location
      }
    ]
  }
}

resource database 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2023-09-15' = {
  parent: account
  name: dbName
  tags: tags
  properties: {
    resource: {
      id: dbName
    }
    options: {
      throughput: 1000
    }
  }
}

resource container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-09-15' = {
  parent: database
  name: containerName
  tags: tags
  properties: {
    resource: {
      id: containerName
      partitionKey: {
        paths: [
          '/id'
        ]
        kind: 'Hash'
      }
      indexingPolicy: {
        indexingMode: 'consistent'
        includedPaths: [
          {
            path: '/*'
          }
        ]
        excludedPaths: [
          {
            path: '/_etag/?'
          }
        ]
      }
    }
  }
}

resource cosmosDataContributor 'Microsoft.DocumentDB/databaseAccounts/sqlRoleDefinitions@2023-09-15' existing = {
  parent: account
  name: '00000000-0000-0000-0000-000000000002'
}

resource sytemMiSqlWriteRoleAssignment 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2023-09-15' = {
  parent: account
  name: guid(account.id, appPrincipalId, cosmosDataContributor.id)
  properties: {
    roleDefinitionId: cosmosDataContributor.id
    principalId: appPrincipalId
    scope: account.id
  }
}
