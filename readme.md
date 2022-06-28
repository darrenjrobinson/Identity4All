# Microsoft Identity for All Hackathon
## Orange Interstellar Corporation Event WebApp
Our team’s submission to the [Microsoft Identity for All Hackathon](https://id4all.devpost.com/) is an Event Web Application solution for a Metaverse online event built entirely on the Azure Platform that combines Bring Your Own IDentity (BYOID), Identity Proofing, Passwordless and  Decentralised Identity with token binding using biometrics for multifactor authentication.  

This repo contains the projects from our submission. 

### Azure WebApp
The WebApp Folder contains the event website for the Orange Interstellar Event App. 
Look into the AppCreationScripts.md for how to deploy the [Microsoft Sample](https://docs.microsoft.com/en-us/azure/active-directory/develop/tutorial-v2-javascript-auth-code). We modified the configure.ps1 script to name the AAD Registered App for our purpose. 

Numerous changes were made to the sample, additional pages created, authentication flows modified and of course the entire look and feel. 

It's not perfect or production ready, but an example of what you can do over a weekend and a couple of late nights. 

### Azure Function App
The FunctionApp Folder contains the Azure Function App that is called by the External Identities User Flow API Connector for Sign-Up. 

The basis is the [AAD External Identities API Connector Python Function example](https://github.com/Azure-Samples/active-directory-python-external-identities-api-connector-azure-function-validate/blob/master/README.md)

It have been modified to use the MSAL Python Package and connect to Azure AD Graph using a certificate retreived from an Azure KeyVault using a system managed identity. 

### VerifiedID Verifiable Credentials
The VerifiedID Folder contains the VerifiedID VC issuance and presentation solution elements demonstrated in our submissions. 

It has been heavily modified to include the facial recognition enrolment during VC issuance and facical recognition comparison to the registered image during VC presentation. 

## More Info
For more information see the [hackathon detailed submission here](https://devpost.com/software/orange-interstellar-corporation-event-webapp)

## Solution Team
The Privacy Preserving Identity Proofing Event Web Application Solution is the work of [Darren Robinson](https://www.linkedin.com/in/darrenjrobinson/), [Elias Ekonomou](https://www.linkedin.com/in/elias-ekonomou-a124b011/), [Christian Chung-Tak-Man](https://www.linkedin.com/in/christianchung/) and [Farzan Akhtar](https://www.linkedin.com/in/farzan-a-088644127/) built specifically for the [Microsoft Identity for All Hackathon](https://id4all.devpost.com/).
