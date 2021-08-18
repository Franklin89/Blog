Title: Use Auth0 custom actions to enrich user tokens with business data 
Published: 06/04/2021 21:00
Tags: 
    - ASP.NET Core
    - Auth0
    - Security
    - Custom Action
---

Have you ever had the need to enrich your users access token or id token with custom data from your backend after they login? With Auth0 and their actions this has become really easy. In this blog post I want to show you how you can do an API call to one of your backends and enrich the user tokens.

Auth0 used to have (well still have but they are legacy) rules and hooks where you could customize the authentication pipeline. Newer to their platform and the recommend way Auth0 offers `Actions` where you can define `Custom Actions` and include them in various `Flows`. The so called `Login` flow can be used exactly for the task I described above.

So lets look at that one a bit closer.

When you go to `Actions -> Flows` you can click on the login flow and you will see a nice flow designer. On the right side of the designer you have your custom actions if you have already created any. If not, go ahead and create one and you will be redirected to an empty code editor with some function bodies ready for you to create your custom logic.

```js
/**
 * Handler that will be called during the execution of a PostLogin flow.
 *
 * @param {Event} event - Details about the user and the context in which they are logging in.
 * @param {PostLoginAPI} api - Interface whose methods can be used to change the behavior of the login.
 */
exports.onExecutePostLogin = async (event, api) => {
};

/**
 * Handler that will be invoked when this action is resuming after an external redirect. If your
 * onExecutePostLogin function does not perform a redirect, this function can be safely ignored.
 *
 * @param {Event} event - Details about the user and the context in which they are logging in.
 * @param {PostLoginAPI} api - Interface whose methods can be used to change the behavior of the login.
 */
// exports.onContinuePostLogin = async (event, api) => {
// };
```

To call our backend from the custom action we actually need to authenticate ourselves. We can not do this call on behalf of the user that is in the process of signing in because the token has not yet been generated. Which kind of makes sense but would have been easier. So we need to get an access token for our backend call. We can do this by calling our token endpoint with the credentials flow. But for this to work we need to register an application for our rule.

## Register application for our rule

Go to `Applications -> Create Application`. Here you will enter a name for your application and select _Machine to Machine Applications_. After the application has been created go to the `APIs` tab in the application settings and toggle the backend API you want to call to _Authorized_.

That's it for setting that part up.

## Storing secrets

Auth0 allows you to store secrets with the custom actions that then can be accessed via the action context: `event.secrets.`. We added the Auth0 Domain, the Client Id and Client Secret into the secret store to use them in our implementation.

## Implement the custom action

After retrieving an `access_token` for our backend we can then call it with the retrieved `access_token` in the authorization header and enrich the users `access_token` and `id_token`.

```js
/**
 * Handler that will be called during the execution of a PostLogin flow.
 *
 * @param {Event} event - Details about the user and the context in which they are logging in.
 * @param {PostLoginAPI} api - Interface whose methods can be used to change the behavior of the login.
 */
exports.onExecutePostLogin = async (event, api) => {
  //
  // Get extra user info from backend
  //
  // 1. First we need to retrieve an access token
  // To do so we need to create a M2M application that has access to our secure api, in this case the PostLoginApi
  // We request a token with the client id and client secret through the credentials flow
  let access_token = await axios.post(`https://${event.secrets.auth0_domain}/oauth/token`, {
    "client_id": event.secrets.client_id,
    "client_secret": event.secrets.client_secret,
    "audience": "http://PostLoginApi",
    "grant_type": "client_credentials"
  });

  // 2. Call our api and retrieve the custom data
  let users_response = await axios.get(`http://2adff678f535.ngrok.io/api/users/${event.user.email}`, {
    headers: {
      'Authorization': `Bearer ${access_token.data.access_token}`
    }
  });

  // 3. Update the access_token and id_token for the user that is currently logging in
  if(users_response.data){
    api.accessToken.setCustomClaim("http://test.org/extra_info", users_response.data);
    api.idToken.setCustomClaim("http://test.org/extra_info", users_response.data);
  }
};
```

After you have saved the custom action you can try it out from the code editor and then deploy it. Add it to the login flow and finished you are.

## Summary

Although we only enriched the `access_token` and `id_token` there is so much more that you can do with these custom actions. But be aware that this action will be called on every login of a user. So the call should be fast so that the user has a good experience.

If you like this blog post drop a comment or buy me a coffee at the bottom of the page <i class="fa fa-coffee"></i>
