using './main.bicep'

param name = 'cloakcare'
param environment = 'Development'
param aspKind = 'linux'
param aspSku = 'F1'
param location = 'westeurope'
param tags = {
  App: 'CloakCare'
}
param containerName = 'appointments'
