using './main.bicep'

param name = 'cloakcare'
param environment = 'Development'
param aspKind = 'linux'
param aspSku = 'F1'
param location = 'westeurope'
param tags = {
  App: 'CloakCare'
}
param containerName = 'patients'
param patientId = '18f22fa8-fc92-4d5f-85bc-20752fd2fe84'
