unity-gloebit
=============

https://github.com/gloebit/unity-gloebit/tree/unity-asset-store


### C# class for accessing [Gloebit](http://dev.gloebit.com/)'s API from Unity's webplayer.

Gloebit is a backend service for web applications.
[OAuth2](http://oauth.net/2/)
is used to grant your application permission to access a user's
Gloebit account.  Once your application has an OAuth2 access-token, it can
get information about the user and has access to an
entitlements/inventory database.

To start, create a scene and place the "Gloebit" and "Gloebit Login"
prefabs into it.  Create a another scene and place the GloebitDemo
prefab into it.

In build settings, select Unity Webplayer and add both scenes (make
sure the scene with "Gloebit Login" is first).

Select the first scene by double-clicking it in the Assets view.

Once this is done, run the application and make sure everything
is working; click on the Login button.

Gloebit.cs contains a "singleton" object instance which is used
to communicate with Gloebit's service.  The prefab "Gloebit" has this
script as a component.

GloebitLogin.cs contains a bare-bones login sequence.  The prefab
"Gloebit Login" has this script as a component.

GloebitEntitlementsDemo.cs contains an example class which queries for
the user's ID and has some buttons linked to api calls.

Applications also have Gloebit accounts.  The prefabs come
preconfigured with the OAuth "consumer-key" for the "Test Consumer"
account.  Once the basic setup is working, you should log into
Gloebit's [sandbox](https://sandbox.gloebit.com/login/) where you can
create an account for your application.  Next, visit Gloebit's
[Merchant Signup Form](https://sandbox.gloebit.com/merchant-signup/?u=0&r=)
and enter any relevant information (all fields are optional on the
sanbox server).  Once this form has been submitted, the "Merchant
Tools" menu item will appear in the dropdown menu.  In the "Merchant
Tools" page, find the OAuth section and find your application's OAuth
Key.  This key should replace "test-consumer" in Gloebit.cs

```C#
  public string consumer_key = "test-consumer";
```

At this point, you can remove or modify the GloebitDemo prefab and
replace it with your own code.


Gloebit API
===========

These methods are available on the static singleton object:

```C#
Gloebit.gloebit
```

See [GloebitEntitlementsDemo.cs](https://github.com/gloebit/unity-gloebit/blob/unity-asset-store/GloebitEntitlementsDemo.cs) for example usage.

### getAccessCode

```C#
getAccessCode (string consumer_key)
```

This call is taken care of by GloebitLogin.cs -- you shouldn't need
to call it, directly.

This should be called when the application starts.  It will either
extract the access token from the "code" query-argument or return null.  If
it returns null, the application should call Authorize.

The value for **consumer_key** can be found on the
[Merchant Settings](https://sandbox.gloebit.com/merchant-tools/)
page of your Gloebit account.  For testing, you can use "test-consumer".


### Authorize

```C#
Authorize (string url)
```

This call is taken care of by GloebitLogin.cs -- you shouldn't need
to call it, directly.

Your application should call this when it finds that it doesn't have a
valid access token.  The **url** argument should be the url of your
application.  The user's webbrowser will be redirected to Gloebit's
website, and OAuth 2 will be used to create an access token for your
application.  Once the user has logged in, their browser will be sent
back to the url provided in the argument to Authorize.  If no url is
provided, the user will arrive on a page that displays their new
access-code.  It will be up to the user to relay this information to
your application.


### NoOp

```C#
NoOp (Action<bool> cb)
```

Check token and server communication.

**cb** should be a function which accepts a bool.  This call simply
contacts Gloebit's server -- the bool will be true if the access-token
is valid.

### GetUserDetails

```C#
GetUserDetails (Action<bool, string, string, string> cb)
```

Request information about the logged-in user.  

**cb** should be a function that accepts 4 arguments: a bool
success, a string with a message about any failure, a string containing
the users ID
([a UUID](http://en.wikipedia.org/wiki/Universally_unique_identifier)),
and a string containing the user's chosen display-name.  The
display-name will be null if the user hasn't selected one.

### GetProducts

```C#
GetProducts (Action<bool, string, Dictionary<string,object>> cb)
```

Request a list of *products* associated with the current user's Gloebit
account.  *products* are tags (strings) which are predefined using
the [Merchant Products](https://sandbox.gloebit.com/merchant-products) page.
A user has zero or more of each defined *product*.

**cb** should be a function that accepts 3 argument -- a
boolean to indicate success, a string with details about any failure,
and a Dictionary<string,object> which has product names as keys and counts
as values.

### ConsumeProduct

```C#
ConsumeProduct (string product_name, int count, Action<bool, string, string, int> cb)
```

This will decrease the number of **product_name** associated with the user's
Gloebit account by **count**.  **cb** should be a function which accepts
4 arguments: a bool success, a string with information about any failure,
a string which will be the same as the **product_name** argument,
and an int which represents how many of **product_name** the user has
after this call has completed.

### GrantProduct

```C#
GrantProduct (string product_name, int count, Action<bool, string, int> cb)
```

This will increase the number of **product_name** associated with the user's
Gloebit account by **count**.  **cb** should be a function which accepts
4 arguments: a bool success, a string with information about any failure,
a string which will be the same as the **product_name** argument,
and an int which represents how many of **product_name** the user has
after this call has completed.


### SetProductCount

```C#
SetProductCount (string product_name, int count, Action<bool, string, string, int> cb)
```

This will set the number of **product_name** associated with the user's
Gloebit account to **count**.  **cb** should be a function which accepts
4 arguments: a bool success, a string with information about any failure,
a string which will be the same as the **product_name** argument,
and an int which represents how many of **product_name** the user has
after this call has completed.
