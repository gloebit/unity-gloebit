unity-gloebit
=============

Unity plugin for accessing Gloebit's API

Works with webplayer

Put Gloebit.cs in your project's plugins directory.

When user first visits website, there will be no gloebit access token.

First, check for an access token

    Gloebit.GloebitUser gbit;
    gbit = gameObject.AddComponent<Gloebit.GloebitUser>();
    access_code = gbit.getAccessCode ("test-consumer");

and (since there isn't one) call

    if (access_code == null) {
      gbit.Authorize (Application.absoluteURL);
    }

The user will arrive at Gloebit's website, where they can authenticate
and authorized your website's access to their account.

The user will be redirected back to Application.absoluteURL, this time
with an access token.

Once your application has a Gloebit access token, it can use the other
Gloebit API calls supported by the Gloebit.GloebitUser class.


GloebitUser API
===============

### getAccessCode (string consumer_key)

This should be called when the application starts.  It will either
extract the access token from the "code" query-argument or return null.  If
it returns null, the application should call Authorize.

The value for **consumer_key** can be found on the
[Merchant Settings](https://www.gloebit.com/merchant-tools/)
page of your Gloebit account.  For testing, you can use "test-consumer".

### Authorize (url)

Your application should call this when it finds that it doesn't have a
valid access token.  The **url** argument should be the url of your
application.  The user's webbrowser will be redirected to Gloebit's
website, and OAuth 2 will be used to create an access token for your
application.  Once the user has logged in, their browser will be sent
back to the url provided in the argument to Authorize.

### GetBalance (callback)

This is used to request the current gloebit balance of a user's account.
**callback** should be a function that accepts one argument -- a float which
represents the user's balance.  The balance is retrived by using
a coroutine to make requests to Gloebit's server.

### GetProducts (callback)

Request a list of *products* associated with the current user's Gloebit
account.  *products* are tags (strings) which are predefined using
the [Merchant Products](https://www.gloebit.com/merchant-products) page.
A user has zero or more of each defined *product*.  **BuyProductWorker**
can be used to grant a user more of a *product* type, and **ConsumeProduct**
can be used to reduce the number of a user's *product* type.

**callback** should be a function that accepts one argument -- a
Dictionary<string,object> which has product names as keys and counts
as values.

### BuyProduct (string product_name, int count, Action<bool, float, int> cb)

This call will attempt to increase the number of a type of product
associated with the current user's Gloebit account by **count**.
**cb** should be a function that accepts 3 arguments: a bool success,
a float representing the user's new gloebit balance, and an int
which represents how many of **product_name** the user now has.

### ConsumeProduct (string product_name, int count, Action<bool, int> cb)

This will reduce the number of **product_name** associated with the user's
Gloebit account by **count**.  **cb** should be a function which accepts
2 arguments: a bool success and an int which represents how many of
**product_name** the user now has.
