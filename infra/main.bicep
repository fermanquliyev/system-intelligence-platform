@description('Project name used for resource naming')
param projectName string = 'sipplatform'

@description('Azure region for resources')
param location string = resourceGroup().location

@description('SQL Server administrator password')
@secure()
param sqlAdminPassword string

@description('Environment name (e.g., dev, staging, prod)')
param environment string = 'prod'

@description('SQL Server administrator username')
param sqlAdminUsername string = 'sqladmin'

@description('Azure Search admin key (optional, can be set after deployment)')
@secure()
param searchAdminKey string = ''

var resourcePrefix = '${projectName}-${environment}'
var appServicePlanName = '${resourcePrefix}-asp'
var appServiceName = '${resourcePrefix}-app'
var functionAppName = '${resourcePrefix}-func'
var storageAccountName = replace('${resourcePrefix}stor', '-', '')
var keyVaultName = replace('${resourcePrefix}-kv', '-', '')
var logAnalyticsWorkspaceName = '${resourcePrefix}-law'
var appInsightsName = '${resourcePrefix}-appi'
var apiManagementName = replace('${resourcePrefix}-apim', '-', '')

// Deploy modules
module sqlModule 'modules/sql.bicep' = {
  name: 'sqlDeployment'
  params: {
    projectName: projectName
    environment: environment
    location: location
    sqlAdminUsername: sqlAdminUsername
    sqlAdminPassword: sqlAdminPassword
  }
}

module serviceBusModule 'modules/servicebus.bicep' = {
  name: 'serviceBusDeployment'
  params: {
    projectName: projectName
    environment: environment
    location: location
  }
}

module searchModule 'modules/search.bicep' = {
  name: 'searchDeployment'
  params: {
    projectName: projectName
    environment: environment
    location: location
    searchAdminKey: searchAdminKey
  }
}

module languageModule 'modules/language.bicep' = {
  name: 'languageDeployment'
  params: {
    projectName: projectName
    environment: environment
    location: location
  }
}

module signalRModule 'modules/signalr.bicep' = {
  name: 'signalRDeployment'
  params: {
    projectName: projectName
    environment: environment
    location: location
  }
}

// Log Analytics Workspace
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsWorkspaceName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

// Application Insights
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
    IngestionMode: 'LogAnalytics'
  }
}

// Storage Account for Azure Functions
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
  }
}

// App Service Plan (Basic B1)
resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'B1'
    tier: 'Basic'
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

// Function App Plan (Consumption)
resource functionAppPlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: '${resourcePrefix}-func-plan'
  location: location
  kind: 'functionapp'
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {
    reserved: true
  }
}

// Key Vault
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      name: 'standard'
      family: 'A'
    }
    tenantId: subscription().tenantId
    enabledForDeployment: false
    enabledForTemplateDeployment: true
    enabledForDiskEncryption: false
    enableRbacAuthorization: true
    accessPolicies: []
  }
}

// App Service
resource appService 'Microsoft.Web/sites@2023-01-01' = {
  name: appServiceName
  location: location
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      alwaysOn: true
      appSettings: [
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: applicationInsights.properties.InstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: applicationInsights.properties.ConnectionString
        }
        {
          name: 'Azure__KeyVault__VaultUri'
          value: keyVault.properties.vaultUri
        }
        {
          name: 'Azure__ServiceBus__ConnectionString'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=ServiceBusConnectionString)'
        }
        {
          name: 'Azure__Language__Endpoint'
          value: languageModule.outputs.endpoint
        }
        {
          name: 'Azure__Language__Key'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=LanguageServiceKey)'
        }
        {
          name: 'Azure__Search__Endpoint'
          value: searchModule.outputs.endpoint
        }
        {
          name: 'Azure__Search__Key'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=SearchServiceKey)'
        }
        {
          name: 'Azure__Search__IndexName'
          value: 'incidents-index'
        }
        {
          name: 'Azure__SignalR__ConnectionString'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=SignalRConnectionString)'
        }
        {
          name: 'ConnectionStrings__Default'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=SqlConnectionString)'
        }
      ]
    }
  }
}

// Function App
resource functionApp 'Microsoft.Web/sites@2023-01-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: functionAppPlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNET-ISOLATED|8.0'
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: toLower(functionAppName)
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: applicationInsights.properties.InstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: applicationInsights.properties.ConnectionString
        }
        {
          name: 'Azure__ServiceBus__ConnectionString'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=ServiceBusConnectionString)'
        }
        {
          name: 'Azure__KeyVault__VaultUri'
          value: keyVault.properties.vaultUri
        }
      ]
    }
  }
}

// API Management (Consumption tier)
resource apiManagement 'Microsoft.ApiManagement/service@2023-05-01-preview' = {
  name: apiManagementName
  location: location
  sku: {
    name: 'Consumption'
    capacity: 0
  }
  properties: {
    publisherEmail: 'admin@${projectName}.com'
    publisherName: projectName
  }
}

// Grant Key Vault access to App Service managed identity
resource appServiceKeyVaultAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, appService.id, 'KeyVaultSecretsUser')
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6') // Key Vault Secrets User
    principalId: appService.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// Grant Key Vault access to Function App managed identity
resource functionAppKeyVaultAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, functionApp.id, 'KeyVaultSecretsUser')
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6') // Key Vault Secrets User
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// Store secrets in Key Vault
resource sqlConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'SqlConnectionString'
  properties: {
    value: sqlModule.outputs.connectionString
  }
}

resource serviceBusConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'ServiceBusConnectionString'
  properties: {
    value: serviceBusModule.outputs.connectionString
  }
}

resource languageServiceKeySecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'LanguageServiceKey'
  properties: {
    value: languageModule.outputs.key
  }
}

resource searchServiceKeySecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'SearchServiceKey'
  properties: {
    value: searchModule.outputs.key
  }
}

resource signalRConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'SignalRConnectionString'
  properties: {
    value: signalRModule.outputs.connectionString
  }
}

output appServiceUrl string = 'https://${appService.properties.defaultHostName}'
output functionAppUrl string = 'https://${functionApp.properties.defaultHostName}'
output apiManagementUrl string = 'https://${apiManagement.properties.gatewayUrl}'
output keyVaultUri string = keyVault.properties.vaultUri
output applicationInsightsConnectionString string = applicationInsights.properties.ConnectionString
output sqlServerName string = sqlModule.outputs.serverName
output sqlDatabaseName string = sqlModule.outputs.databaseName
output serviceBusNamespace string = serviceBusModule.outputs.namespaceName
output searchServiceName string = searchModule.outputs.serviceName
output languageServiceEndpoint string = languageModule.outputs.endpoint
output signalRServiceName string = signalRModule.outputs.serviceName
