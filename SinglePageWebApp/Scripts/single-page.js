var idTokenKey = 'idToken';
var sessionTokenKey = 'sessionToken';
var userLoginKey = 'userLogin';


function renderOktaWidget() {
    oktaSessionsMe(function (authenticated) {
        console.log('Is user authenticated? ' + authenticated);
        if (!authenticated) {
            showAuthUI(false, "");
            oktaSignIn.renderEl(
                { el: '#okta-sign-in-widget' },
                function (res) {
                    if (res.status === 'SUCCESS') {
                        console.log(res);
                        var id_token = res.id_token || res.idToken;
                        console.log('id token: ' + id_token);
                        sessionStorage.setItem(idTokenKey, id_token);
                        sessionStorage.setItem(userLoginKey, res.claims.preferred_username);
                        showAuthUI(true, res.claims.preferred_username);
                    }
                },
                function (err) { console.log('Unexpected error authenticating user: %o', err); }
            );
        }
        else {
            var userLogin = sessionStorage.getItem(userLoginKey);
            if (userLogin) {
                console.log('user Login is ' + userLogin);
            }
            showAuthUI(true, userLogin);
        }
    });
}

function showAuthUI(isAuthenticated, user_id) {
    if (isAuthenticated) {
        $('#navbar > ul').empty().append('<li><a id="logout" href="/logout">Sign out</a></li>');
        $('#logout').click(function (event) {
            event.preventDefault();
            signOut();
        });
        $('#okta-sign-in-widget').hide();
        $('#logged-out-message').hide();
        $('#logged-in-message').show();
        if (user_id) {
            $('#okta-user-id').empty().append(user_id);
            $('#logged-in-user-id').show();
        }
    }
    else {
        $('#navbar > ul').empty();
        $('#logged-in-message').hide();
        $('#logged-out-message').show();
        $('#logged-in-user-id').hide();
        $('#okta-sign-in .okta-form-input-field input[type="password"]').val('');
        console.log('show sign-in widget')
        $('#okta-sign-in-widget').show();
    }
}

function callUnsecureWebApi() {
    $.ajax({
        type: "GET",
        dataType: 'json',
        url: webApiRootUrl + "/hello",
        success: function (data) {
            console.log(data);
            $('#logged-in-res').text(data);
        }
    });
}

function callSecureWebApi() {
    $.ajax({
        type: "GET",
        dataType: 'json',
        url: webApiRootUrl + "/secure/hello",
        beforeSend: function (xhr) {
            var id_token = sessionStorage.getItem(idTokenKey);
            console.log("callSecureWebApi ID Token: " + id_token);
            if (id_token) {
                xhr.setRequestHeader("Authorization", "Bearer " + id_token);
            }
        },
        success: function (data) {
            $('#logged-in-res').text(data);
        },
        error: function(textStatus, errorThrown) {
        $('#logged-in-res').text("You must be logged in AND have the proper permissions to access this API endpoint");
    }
    });
}

function callUserInfo() {
    $.ajax({
        type: "GET",
        dataType: 'json',
        url: oktaOrgUrl + "/oauth2/v1/userinfo",
        beforeSend: function (xhr) {
            var id_token = sessionStorage.getItem(idTokenKey);
            if (id_token) {
                xhr.setRequestHeader("Authorization", "Bearer " + id_token);
            }
        },
        success: function (data) {
            console.log(data);
        },
        error: function (textStatus, errorThrown) {
            $('#logged-in-res').text("You must be logged in to call this API");
        }
    });
}

function callUsersMe() {
    oktaUsersMe(function (authenticated) {
        console.log('Is user authenticated? ' + authenticated);
        return authenticated;
    });
}

function callSessionsMe() {
    oktaSessionsMe(function (authenticated) {
        console.log('Is user authenticated? ' + authenticated);
        return authenticated;
    });
}


function signOut() {
    console.log('signing out');
    oktaSessionsMe(function (authenticated) {
        if (authenticated) {
            var sessionToken;
            var sessionTokenString = sessionStorage.getItem(sessionTokenKey);
            if (sessionTokenString) {
                sessionToken = JSON.parse(sessionTokenString);
                console.log(sessionToken);
                var sessionId = sessionToken.id;
                console.log('closing session ' + sessionId);
                closeSession(function (success) {
                    console.log('Is session closed? ' + success);
                    if(success)
                        renderOktaWidget();
                })
            }
        }
    });
   
}

function oktaSessionsMe(callBack) {
    $.ajax({
        type: "GET",
        dataType: 'json',
        url: oktaOrgUrl + "/api/v1/sessions/me",
        xhrFields: {
            withCredentials: true
        },
        success: function (data) {
            console.log('setting success to true');
            console.log("My session: ");
            console.log(data);
            sessionStorage.setItem(sessionTokenKey, JSON.stringify(data));
            return callBack(true);
            //$('#logged-in-res').text(data);
        },
        error: function (textStatus, errorThrown) {
            console.log('setting success to false');
            //$('#logged-in-res').text("You must be logged in to call this API");
            return callBack(false);
        },
        async: true
    });
}


function oktaUsersMe(callBack) {
    $.ajax({
        type: "GET",
        dataType: 'json',
        url: oktaOrgUrl + "/api/v1/users/me",
        xhrFields: {
            withCredentials: true
        },
        success: function (data) {
            console.log(data);
            return callBack(true);
        },
        error: function (textStatus, errorThrown) {
            return callBack(false);
        },
        async: true
    });
}

function closeSession(callback) {
    $.ajax({
        type: "DELETE",
        dataType: 'json',
        url: oktaOrgUrl + "/api/v1/sessions/me",
        xhrFields: {
            withCredentials: true
        },
        success: function (data) {
            console.log('success deleting session');
            console.log(data);
            console.log('removing session from sessionStorage');
            sessionStorage.removeItem(sessionTokenKey);
            console.log('removed session from sessionStorage');
            console.log('removing user Login from sessionStorage');
            sessionStorage.removeItem(userLoginKey);
            console.log('removed user Login from sessionStorage');
            console.log('removing id Token from sessionStorage');
            sessionStorage.removeItem(idTokenKey);
            console.log('removed id Token from sessionStorage');
            $('#logged-in-res').text('');
            return callback(true);
        },
        error: function (textStatus, errorThrown) {
            console.log('error deleting session: ' + JSON.stringify(textStatus));
            console.log(errorThrown);
            return callback(false);
        },
        async: true
    });
}
