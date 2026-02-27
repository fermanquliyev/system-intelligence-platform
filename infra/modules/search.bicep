@description('Project name used for resource naming')
param projectName string

@description('Environment name')
param environment string

@description('Azure region for resources')
param location string

@description('Azure Search admin key. Retrieve after deployment using: az search admin-key show --resource-group <rg> --service-name <service> --query primaryKey -o tsv')
@secure()
param searchAdminKey string = ''

var resourcePrefix = '${projectName}-${environment}'
var searchServiceName = replace('${resourcePrefix}-search', '-', '')

// Azure AI Search (Free tier)
resource searchService 'Microsoft.Search/searchServices@2023-11-01' = {
  name: searchServiceName
  location: location
  sku: {
    name: 'free'
  }
  properties: {
    replicaCount: 1
    partitionCount: 1
    hostingMode: 'default'
    publicNetworkAccess: 'enabled'
  }
}

output serviceName string = searchService.name
output endpoint string = 'https://${searchService.name}.search.${environment().suffixes.search}'
output key string = searchAdminKey
