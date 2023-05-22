"use strict";
$(() => init());
function init() {
    $('#signMeUpBtn').on("click", () => joinBetaProgramme());
    $('#emailAddress').on("keypress", () => { $('#message').addClass("d-none"); });
}
function joinBetaProgramme() {
    let emailAddress = $('#emailAddress').val();
    if (emailAddress != "") {
        if (validateEmail(emailAddress)) {
            var data = { emailAddress: $('#emailAddress').val() };
            subscribe(data).then((response) => {
                if (response.error == false) {
                    success("You have been successfully subscribed");
                }
            });
        }
        else {
            error("You have entered an invalid email address!");
        }
    }
}
function error(text) {
    $('#message').removeClass("alert-success").addClass("alert-danger").text(text).removeClass("d-none");
}
function success(text) {
    $('#message').removeClass("alert-danger").addClass("alert-success").text(text).removeClass("d-none");
}
function subscribe(request) {
    const options = {
        method: "POST",
        headers: {
            Accept: "application/json",
            "Content-Type": "application/json;charset=UTF-8",
        },
        body: JSON.stringify(request)
    };
    return fetch(`/api/subscriber`, options)
        .then(response => {
        if (!response.ok) {
            throw response;
        }
        return response.json();
    })
        .catch(err => {
        err.text().then((errorMessage) => {
            console.error(errorMessage);
        });
        return Promise.reject();
    });
}
function validateEmail(emailAddress) {
    if (/^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/.test(emailAddress)) {
        return (true);
    }
    return (false);
}
