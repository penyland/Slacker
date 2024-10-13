// Parameters
@minLength(2)
@maxLength(60)
@description('The name of the application/service i.e ExampleService.')
param applicationName string

@minLength(3)
@maxLength(24)
@description('The shortened name of the application/service i.e Environment. This is used as the shortened name for resources that doesn\'t support more than 24 characters.')
param applicationShortName string

@description('ASP.NET Core runtime environment configuration, e.g. Development, Staging, or Production. If no value is set, it defaults to Production.')
param aspNetCoreEnvironment string = 'Production'

@description('The shortname of the environment parameter used to postfix the service name. E.g. Dev, Ref, Acc, Prod etc.')
param environment string = 'Prod'
