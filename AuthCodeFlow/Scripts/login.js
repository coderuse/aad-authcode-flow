(function (w) {
    'use strict';

    var conf = w.appConfig;

    function isB2CSignIn() {
        var result = false;

        var hash = window.location.hash;
        if (hash) {
            hash = Msal.UserAgentApplication.prototype.getHash(hash);
            var parameters = Msal.Utils.deserialize(hash);
            //window.location.href = conf.redirectUri;
            if (parameters) {
                if (parameters.hasOwnProperty('error_description')) {
                    var msalErrorDescription = parameters['error_description'];
                    return {
                        error: {
                            description: msalErrorDescription
                        }
                    };
                }
                else if (parameters.hasOwnProperty('code')) {
                    var authCode = parameters['code'];
                    return authCode;
                }
            }
        }

        return result;
    }

    $(document).ready(function () {
        var intrvl = setInterval(function () {
            var scopes = encodeURIComponent(conf.b2cScopes + ' offline_access profile');
            var code = isB2CSignIn();
            if (code) {
                clearInterval(intrvl);
                if (code.error) {
                    console.log(code.error.description);
                    return;
                }

                $.ajax({
                    type: "POST",
                    url: '/home/Token',
                    data: {
                        grantType: 'authorization_code',
                        code: code
                    },
                    success: function (d) {
                        console.log(d);
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        console.log('jqXHR' + jqXHR, 'textStatus' + textStatus, 'errorThrown' + errorThrown)
                    }
                });
            }
            else {
                var authorizationUrl = conf.b2cAuthorizationEndPoint;
                var params = [];
                params.push('client_id=' + conf.b2cClientId);
                params.push('response_type=code');
                params.push('redirect_uri=' + encodeURIComponent(conf.redirectUri));
                params.push('response_mode=fragment');
                params.push('scope=' + scopes);
                params.push('state=' + Msal.Utils.createNewGuid());
                //params.push('p=' + conf.b2cSignUpSignInPolicyId);
                authorizationUrl += '?' + params.join('&');
                window.location.replace(authorizationUrl);
            }
        }, 3000);
    });
}(window));