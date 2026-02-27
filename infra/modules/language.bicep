@description('Project name used for resource naming')
param projectName string

@description('Environment name')
param environment string

@description('Azure region for resources')
param location string

var resourcePrefix = '${projectName}-${environment}'
var languageServiceName = replace('${resourcePrefix}-lang', '-', '')

// Azure Language Service (Free tier - S0)
resource languageService 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: languageServiceName
  location: location
  kind: 'TextAnalytics'
  sku: {
    name: 'S'
    tier: 'Free'
  }
  properties: {
    apiProperties: {
      statisticsEnabled: false
    }
  }
}

output serviceName string = languageService.name
output endpoint string = languageService.properties.endpoint
output key string = languageService.listKeys().key1
