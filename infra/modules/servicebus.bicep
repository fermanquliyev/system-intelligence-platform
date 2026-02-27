@description('Project name used for resource naming')
param projectName string

@description('Environment name')
param environment string

@description('Azure region for resources')
param location string

var resourcePrefix = '${projectName}-${environment}'
var serviceBusNamespaceName = replace('${resourcePrefix}-sb', '-', '')
var queueName = 'log-ingestion'

// Service Bus Namespace (Basic tier)
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: serviceBusNamespaceName
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
  }
  properties: {
    minimumTlsVersion: '1.2'
  }
}

// Service Bus Queue
resource serviceBusQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: serviceBusNamespace
  name: queueName
  properties: {
    deadLetteringOnMessageExpiration: true
    maxDeliveryCount: 10
    lockDuration: 'PT30S'
    defaultMessageTimeToLive: 'P14D'
    enablePartitioning: false
  }
}

// Get connection string from authorization rule
resource serviceBusAuthRule 'Microsoft.ServiceBus/namespaces/authorizationRules@2022-10-01-preview' = {
  parent: serviceBusNamespace
  name: 'RootManageSharedAccessKey'
  properties: {
    rights: ['Listen', 'Manage', 'Send']
  }
}

output namespaceName string = serviceBusNamespace.name
output queueName string = queueName
output connectionString string = serviceBusAuthRule.listKeys().primaryConnectionString
