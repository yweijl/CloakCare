using './main.bicep'

param name = 'cloakcare'
param aspKind = 'linux'
param aspSku = 'F1'
param location = 'westeurope'
param tags = {
  App: 'CloakCare'
}
param containerName = 'appointments'
