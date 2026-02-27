@description('Project name used for resource naming')
param projectName string

@description('Environment name')
param environment string

@description('Azure region for resources')
param location string

var resourcePrefix = '${projectName}-${environment}'
var signalRServiceName = replace('${resourcePrefix}-signalr', '-', '')

// Azure SignalR Service (Free tier, Serverless mode)
resource signalRService 'Microsoft.SignalRService/signalR@2023-02-01' = {
  name: signalRServiceName
  location: location
  sku: {
    name: 'Free_F1'
    tier: 'Free'
    capacity: 1
  }
  kind: 'SignalR'
  properties: {
    features: [
      {
        flag: 'ServiceMode'
        value: 'Serverless'
      }
    ]
    cors: {
      allowedOrigins: ['*']
    }
    serverless: {
      connectionTimeoutInSeconds: 30
    }
    tls: {
      clientCertEnabled: false
    }
  }
}

output serviceName string = signalRService.name
output endpoint string = signalRService.properties.hostName
output connectionString string = signalRService.listKeys().primaryConnectionString
