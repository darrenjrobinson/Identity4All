// Select DOM elements to work with
const welcomeDiv = document.getElementById("WelcomeMessage");
const signInButton = document.getElementById("SignIn");
const cardDiv = document.getElementById("card-div");
const cardDivBody = document.getElementById("card-div-body");
const cardDivBodyMain = document.getElementById("card-div-body-main");
const cardDivWelcome = document.getElementById("WelcomeMessage");
const cardDivTitle = document.getElementById("EventCard");
const cardCompleteReg = document.getElementById("CompleteRegistrationCard");

const enrolVerifiedIDButton = document.getElementById("enrolVerifiedID");
const enrolFacialRecButton = document.getElementById("enrolFacialRec");
const profileDiv = document.getElementById("profile-div");
var sessionIdForVC = null;
var userNameFromVC = null;

function showWelcomeMessage(username) {
    // Reconfiguring DOM elements
    cardDiv.style.display = 'block';
    cardDivBody.style.display = 'block';
    cardDivBodyMain.style.display = 'block';
    cardCompleteReg.style.display = 'block';
    cardDivWelcome.style.display = 'block';
    cardDivTitle.style.display = 'block';
    welcomeDiv.innerHTML = `Welcome ${username}`;
    signInButton.setAttribute("onclick", "signOut();");
    signInButton.setAttribute('class', "btn btn-success");
    signInButton.innerHTML = "Sign Out";
}

function popupWindow(url, title, w, h) {
    var y = window.outerHeight / 2 + window.screenY - (h / 2)
    var x = window.outerWidth / 2 + window.screenX - (w / 2)
    return window.open(url, title, 'toolbar=no, location=no, directories=no, status=no, menubar=no, scrollbars=no, resizable=no, copyhistory=no, width=' + w + ', height=' + h + ', top=' + y + ', left=' + x);
}

function enrolFacialRec() {
    var enrolFacialRec = popupWindow("./bioEnrol.html", "Facial Recognition Enrolment", 640, 426);
    // alert("Registers with Facial Recognition.");
}

function enrolVerifiedID() {
    var session = btoa(unescape(encodeURIComponent(JSON.stringify(userDetails))));
    sessionId = null;

//    var enrolVerifiedID = popupWindow("https://yourissuerURL.ngrok.io/IssuerBio?session=" + session, "VerifiedID Enrolment", 640, 600);
    var enrolVerifiedID = popupWindow("https://yourissuerURL.azurewebsites.net/IssuerBio?session=" + session, "VerifiedID Enrolment", 640, 600);

    // alert("Get issued a verifiable credential from Microsoft Entra VerifiedID.");
}

function eventEntry() {
    sessionId = null;

//    fetch('https://yourissuerURL.ngrok.io/api/verifier/create-session')
    fetch('https://yourissuerURL.azurewebsites.net/api/verifier/create-session')
    .then(function (response) {
        response.text()
            .catch(error => console.log(error))
            .then(function (message) {
                respPresentationReq = JSON.parse(message);
                sessionIdForVC = respPresentationReq.id;
                console.log("Called VerifierBio with id: " + sessionIdForVC);
//                var enrolVerifiedID = popupWindow("https://yourissuerURL.ngrok.io/VerifierBio?session=" + sessionIdForVC, "VerifiedID Verification", 640, 600);
                var enrolVerifiedID = popupWindow("https://yourissuerURL.azurewebsites.net/VerifierBio?session=" + sessionIdForVC, "VerifiedID Verification", 640, 600);


                var checkStatus = setInterval(function () {
//                    fetch('https://yourissuerURL.ngrok.io/api/verifier/presentation-response?id=' + sessionIdForVC)
                    fetch('https://yourissuerURL.azurewebsites.net/api/verifier/presentation-response?id=' + sessionIdForVC)                    
                        .then(response => response.text())
                        .catch(error => document.getElementById("message").innerHTML = error)
                        .then(response => {
                            if (response.length > 0) {
                                console.log(response)
                                respMsg = JSON.parse(response);
                                if (respMsg.status == 'presentation_verified') {
                                    console.log("Received credential?:");
                                    console.log(response);
                                    enrolVerifiedID.close();
                                    console.log("respMsg: " + respMsg);
                                    userNameFromVC = respMsg.name;
                                    console.log("respMsg.name: " + respMsg.name + " userNameFromVC: " + userNameFromVC);
                                    sessionStorage.setItem("name", userNameFromVC);
                                    window.location.href = "metaverse.html";
                                }
                            }
                        })
                }, 1500); //change this to higher interval if you use ngrok to prevent overloading the free tier service


            }).catch(error => { console.log(error.message); })
    }).catch(error => { console.log(error.message); })


//    var enrolVerifiedID = popupWindow("./eventEntry.html", "Event Launch", 640, 600);

    // alert("Present a verifiable credential from Microsoft Entra VerifiedID.");
}

function updateUI(data, endpoint) {
    console.log('Graph API responded at: ' + new Date().toString());

    if (endpoint === graphConfig.graphMeEndpoint) {
        profileDiv.innerHTML = '';
        const givenName = document.createElement('p');
        givenName.innerHTML = "<strong>Given name: </strong>" + data.givenName;
        const surname = document.createElement('p');
        surname.innerHTML = "<strong>Surname: </strong>" + data.surname;
        const displayName = document.createElement('p');
        displayName.innerHTML = "<strong>Display name: </strong>" + data.displayName;

        // const phone = document.createElement('p');
        // phone.innerHTML = "<strong>Phone: </strong>" + data.businessPhones[0];
        // const address = document.createElement('p');
        // address.innerHTML = "<strong>Location: </strong>" + data.officeLocation;
        // profileDiv.appendChild(givenName);
        // profileDiv.appendChild(surname);
        // profileDiv.appendChild(displayName);
        // // profileDiv.appendChild(address);

    } else if (endpoint === graphConfig.graphMailEndpoint) {
        if (!data.value) {
            alert("You do not have a mailbox!");
        } else if (data.value.length < 1) {
            alert("Your mailbox is empty!");
        } else {
            const tabContent = document.getElementById("nav-tabContent");
            const tabList = document.getElementById("list-tab");
            tabList.innerHTML = ''; // clear tabList at each readMail call

            data.value.map((d, i) => {
                // Keeping it simple
                if (i < 10) {
                    const listItem = document.createElement("a");
                    listItem.setAttribute("class", "list-group-item list-group-item-action");
                    listItem.setAttribute("id", "list" + i + "list");
                    listItem.setAttribute("data-toggle", "list");
                    listItem.setAttribute("href", "#list" + i);
                    listItem.setAttribute("role", "tab");
                    listItem.setAttribute("aria-controls", i);
                    listItem.innerHTML = d.subject;
                    tabList.appendChild(listItem);

                    const contentItem = document.createElement("div");
                    contentItem.setAttribute("class", "tab-pane fade");
                    contentItem.setAttribute("id", "list" + i);
                    contentItem.setAttribute("role", "tabpanel");
                    contentItem.setAttribute("aria-labelledby", "list" + i + "list");
                    contentItem.innerHTML = "<strong> from: " + d.from.emailAddress.address + "</strong><br><br>" + d.bodyPreview + "...";
                    tabContent.appendChild(contentItem);
                }
            });
        }
    }
}
