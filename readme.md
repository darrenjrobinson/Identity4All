# Microsoft Identity for All Hackathon
## Orange Interstellar Corporation Event WebApp
Our team’s submission to the [Microsoft Identity for All Hackathon](https://id4all.devpost.com/) is an Event Web Application solution for a Metaverse online event built entirely on the Azure Platform that combines Bring Your Own IDentity (BYOID), Identity Proofing, Passwordless and  Decentralised Identity with token binding using biometrics for multifactor authentication.  

This repo contains the projects from our submission. 

### Azure WebApp
The WebApp Folder contains the event website for the Orange Interstellar Event App. 
Look into the AppCreationScripts.md for how to deploy the [Microsoft Sample](https://docs.microsoft.com/en-us/azure/active-directory/develop/tutorial-v2-javascript-auth-code). We modified the configure.ps1 script to name the AAD Registered App for our purpose. 

Numerous changes were made to the sample, additional pages created, authentication flows modified and of course the entire look and feel. 

It's not perfect or production ready, but an example of what you can do over a weekend and a couple of late nights. 

#### Config Updates
Update authconfig.js 
- // 'Application (client) ID' of app registration in Azure portal - this value is a GUID
    - Update with your clientId: "yourClientID",
- // Full directory URL, in the form of https://login.microsoftonline.com/<tenant>    
    - Update with your TenantID authority: "https://login.microsoftonline.com/YOURTENANTID",
- redirectUri: "https://YOURWEBAPP.azurewebsites.net/event.html",
    - Update with your published WebApp URL
	
Update ui.js 
-  var enrolVerifiedID = popupWindow("https://YOURISSUER.azurewebsites.net/IssuerBio?session=" + session, "VerifiedID Enrolment", 640, 600);
    - Update for your Issuer URL *NOTE: it exists in many places

Update apps.json 
- "url": "https://YOURWEBAPP.azurewebsites.net/",
    - Update for your WebApp URL
	
Update Configure.ps1 
-  Spa @{RedirectUris = "https://YOURWEBAPP.azurewebsites.net/" } `
    - Update for your WebApp URL
	
	
### Azure Function App
The FunctionApp Folder contains the Azure Function App that is called by the External Identities User Flow API Connector for Sign-Up. 

The basis is the [AAD External Identities API Connector Python Function example](https://github.com/Azure-Samples/active-directory-python-external-identities-api-connector-azure-function-validate/blob/master/README.md)

It has been modified to retrieve the username and password from an Azure KeyVault using a system managed identity to validate against the basic auth credentials that are configured on the External Identities API Connector.

#### Config Updates
Update local.settings.json 
- (authUsername and authPassword) to include the 'name' of the Secrets in KeyVault that contains the username and password for Azure Function invocation validation as configured on External Identities API Connector. 
- (vaultURL) for the Azure KeyVault that contains the secrets above. e.g https://yourKeyVaultName.vault.azure.net
- (invites) for the list of inivitees allowed to Self-Service SignUp. In lowercase separated by ; e.g. user1@yourdomain.com;user2@yourdomain.com

### VerifiedID Verifiable Credentials
The VerifiedID Folder contains the VerifiedID VC issuance and presentation solution elements demonstrated in our submissions. 

It has been heavily modified to include the facial recognition enrolment during VC issuance and facical recognition comparison to the registered image during VC presentation. But it is based on the [Microsoft example found here](https://github.com/Azure-Samples/active-directory-verifiable-credentials-dotnet/tree/main/1-asp-net-core-api-idtokenhint)

#### Config Updates
Update appsettings.json
- (TenantId) your AAD Tenant ID
- (ClientId) your VC App Reg 
- (ClientSecret) your VC App Secret
- (IssuerAuthority) your DID Issuer e.g. "did:web:yourissuer.azurewebsites.net"
- (VerifierAuthority) your DID Verifier e.g "did:web:yourverifier.azurewebsites.net",
- (CredentialManifest) your DID Manifest e.g. "https://beta.eu.did.msidentity.com/v1.0/GUID/verifiableCredential/contracts/yourContract"

Update IssuerController.cs
- // SUBSCRIPTION KEY section with your API Key for Cognitive Services Facial Rec. 
    - client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "YOURAPIKEY");
- // Add image to get ID
    - Cognitive Services API Endpoint "https://YOURREGION.api.cognitive.microsoft.com/face/v1.0/detect?returnFaceId=true"

Update VerifierController.cs
- // SUBSCRIPTION KEY section with your API Key for Cognitive Services Facial Rec. 
    - client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "YOURAPIKEY");
- // Add image to get ID
    - Cognitive Services API Endpoint "https://YOURREGION.api.cognitive.microsoft.com/face/v1.0/detect?returnFaceId=true"
- // Make the call to Verify API
    - "https://YOURREGION.api.cognitive.microsoft.com/face/v1.0/verify"

Update issuance_request_config.json
- "url": "https://YOURENDPOINT.ngrok.io/api/issuer/issuanceCallback",
    - Update with your endpoint
- "authority": "did:web:yourissuer.azurewebsites.net",
    - Update with your issuer endpoint 
	
Update presentation_request_config.json
- "url": "https://YOURENDPOINT.ngrok.io/api/issuer/issuanceCallback",
    - Update with your endpoint
- "authority": "did:web:yourissuer.azurewebsites.net",
    - Update with your issuer endpoint 
- "acceptedIssuers": [ "did:web:yourissuer.azurewebsites.net" ]
    - Update with your issuer(s)
	
	
## More Info
For more information see the [hackathon detailed submission here](https://devpost.com/software/orange-interstellar-corporation-event-webapp)

## Solution Team
The Privacy Preserving Identity Proofing Event Web Application Solution is the work of [Darren Robinson](https://www.linkedin.com/in/darrenjrobinson/), [Elias Ekonomou](https://www.linkedin.com/in/elias-ekonomou-a124b011/), [Christian Chung-Tak-Man](https://www.linkedin.com/in/christianchung/) and [Farzan Akhtar](https://www.linkedin.com/in/farzan-a-088644127/) built specifically for the [Microsoft Identity for All Hackathon](https://id4all.devpost.com/).
