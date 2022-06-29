import logging
import json
import azure.functions as func
import os
import base64
from azure.identity import DefaultAzureCredential
from azure.keyvault.secrets import SecretClient

def main(req: func.HttpRequest) -> func.HttpResponse:  # API version
    API_VERSION = "1.0.5"

    logging.info(
        f"Python HTTP trigger function processed a request, API version: {API_VERSION}")

    # Allow list for RSVP redemption
    # Download the latest from the Applicaiton Settings and convert to an array
    inviteList = os.environ["invites"].replace(';', '","')
    allowed_invites = '["'+inviteList+'"]'
    logging.info("Invite List: "+allowed_invites)

    # Check HTTP basic authorization
    if not authorize(req):
        logging.info("HTTP basic authentication validation failed.")
        return func.HttpResponse(status_code=401)

    # Get the request body
    try:
        req_body = req.get_json()
        print(req_body)
        logging.info(req_body)
    except:
        return func.HttpResponse(
            json.dumps({"version": API_VERSION, "action": "ShowBlockPage",
                        "userMessage": "There was a problem with your request."}),
            status_code=200,
            mimetype="application/json"
        )

    # If email claim not found, show block page. Email is required and sent by default.
    if 'email' not in req_body or not req_body.get('email') or "@" not in req_body.get('email'):
        return func.HttpResponse(
            json.dumps({"version": API_VERSION, "action": "ShowBlockPage",
                        "userMessage": "Email is mandatory."}),
            status_code=200,
            mimetype="application/json"
        )

    # Triggering Facial Recognition and then VerifiedID Enrolment
    if email.lower() in allowed_invites:
        return func.HttpResponse(
            json.dumps({"version": API_VERSION, "action": "Continue", "userMessage": f"Congratulations '{(email)}'. We have confirmed you were sent an exclusive invitation to our Orange Interstellar Corp launch event. To complete your registration to attend the illustrious launch in the metaverse we will need to issue you with an 'Interstellar Passport' secured by Microsoft Entra VerifiedID and facial recognition. Let's complete the registration process"}),
            status_code=200,
            mimetype="application/json")
    else:
        return func.HttpResponse(
            json.dumps({"version": API_VERSION, "action": "ShowBlockPage",
                        "userMessage": f"Hello '{(email)}'. You do not appear on our exclusive and illustrious invitation list using that address. The Orange Interstellar Corp launch event is restricted to those that have received the platinum invite."}),
            status_code=200,
            mimetype="application/json")


def authorize(req: func.HttpRequest):
    # Get the secret names from environment's credentials to find secret by name in KeyVault
    # userSecretName = os.environ["authUsername"]
    userSecretName = os.environ["authUsername"]
    pwdSecretName = os.environ["authPassword"]

    # Connect to Azure KeyVault to get Certificate to AuthN to MSFT Graph
    VAULT_URL = os.environ["vaultURL"]
    credential = DefaultAzureCredential()
    secret_client = SecretClient(vault_url=VAULT_URL, credential=credential)
    
    digestAuthUserName = secret_client.get_secret(userSecretName)
    digestAuthPWD = secret_client.get_secret(pwdSecretName)

    username = digestAuthUserName.value
    password = digestAuthPWD.value 

    # Returns authorized if the username is empty or not exists.
    if not username:
        logging.info("HTTP basic authentication is not set.")
        return True

    # Check if the HTTP Authorization header exist
    if not req.headers.get("Authorization"):
        logging.info("Missing HTTP basic authentication header.")
        return False

    # Read the authorization header
    auth = req.headers.get("Authorization")

    # Ensure the type of the authorization header id `Basic`
    if "Basic " not in auth:
        logging.info(
            "HTTP basic authentication header must start with 'Basic '.")
        return False

    # Get the HTTP basic authorization credentials
    auth = auth[6:]
    authBytes = base64.b64decode(auth)
    auth = authBytes.decode("utf-8")
    cred = auth.split(':')

    # Evaluate the credentials and return the result
    return (cred[0] == username and cred[1] == password)


