# Tools

## Table of contents:
- [Tools](#tools)
  - [Table of contents:](#table-of-contents)
  - [AzureDevOpsLibraryComparor](#azuredevopslibrarycomparor)
    - [Prerequisites](#prerequisites)
    - [Getting started:](#getting-started)
    - [Running the tool](#running-the-tool)

______________
## AzureDevOpsLibraryComparor
A tool for comparing variable groups across environments (test, acceptance, production). 

### Prerequisites
For this tool to work, variable groups in Azure DevOps Library must have a naming standard such as "some-service-{env}" or "something-{env}-service" where env may have the values t, a, and p for test, acceptance and production respectively. 

### Getting started: 
1. Clone the repository
2. Change directory to AzureDevOpsLibraryComparor: `cd .\AzureDevOpsLibraryComparor\`
3. Run `dotnet pack` and afterwards `dotnet tool install --global --add-source ./nupkg AzureDevOpsLibraryComparor`

### Running the tool
1. Run the following command: `azlib`
2. The tool will ask for a Personal Access Token (PAT). Obtain a PAT from Azure DevOps ([See guide](https://learn.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=azure-devops&tabs=Windows))
   Note: It is important that the PAT has read access to the following scopes: "Project and Team" and "Variable Groups". 
3. Afterwards the tool will ask for the base url (e.g. https://dev.azure.com/{YourOrganization}/)
4. Follow the instructions and compare variables across environments. 

________________