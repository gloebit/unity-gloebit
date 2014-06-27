using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MiniJSON;


public class Gloebit : MonoBehaviour
{
  // We want only one copy of this object.
  public static Gloebit gloebit = null; // singleton

  // access_code is an oauth2 access-code
  public string access_code = null;

  // consumer_key can be found on Gloebit's website under
  // the merchant-tools menu item.  It will usually be a uuid.
  public string consumer_key = "test-consumer";

  // gloebit_base_url controls which Gloebit server to talk to.
  public string gloebit_base_url = "https://sandbox.gloebit.com";

  void Awake ()
  {
    if (gloebit == null)
      {
        // This is the first instance of this object.  store it in
        // the static variable and indicate that it shouldn't be
        // destroyed.
        DontDestroyOnLoad (gameObject);
        gloebit = this;
      }
    else
      {
        // There's already an instance of this object, so destroy this one.
        Destroy (gameObject);
      }
  }


  public Gloebit ()
  {
  }


  private string EncodeQueryData (Dictionary<String,String> data) {
    // turn a Dictionary into a http query-args string
    bool first = true;
    string ret = "";

    foreach (KeyValuePair<string, string> qarg in data)
      {
        if (first) {
          ret += '?';
          first = false;
        }
        else
          ret += '&';
        ret += WWW.EscapeURL (qarg.Key) + "=" + WWW.EscapeURL (qarg.Value);
      }
    return ret;
  }


  private string getQueryVariable (string variable) {
    // extract a value from the query-args of the current url
    string[] parts = Application.absoluteURL.Split ('?');
    string[] vars;

    if (parts.Length < 2)
      return null;

    vars = parts[ 1 ].Split ('&');

    for (var i = 0; i < vars.Length; i++) {
      var pair = vars[ i ].Split ('=');
      if (WWW.UnEscapeURL (pair[ 0 ]) == variable) {
        return WWW.UnEscapeURL (pair[ 1 ]);
      }
    }
    return null;
  }


  public string getAccessCode () {
    // get the oauth2 access-code from the query-args of the current url
    access_code = getQueryVariable ("code");
    return access_code;
  }


  public void setAccessCode (string new_access_code) {
    // set a new access code
    access_code = new_access_code;
  }


  public void Authorize (string url) {
    // send the user's browser to Gloebit's OAuth2 Auth/Login page
    Dictionary<string, string> qargs =
      new Dictionary<string, string>();
    qargs.Add ("scope", "inventory user");
    qargs.Add ("redirect_uri", url);
    qargs.Add ("response_type", "token");
    qargs.Add ("client_id", consumer_key);
    qargs.Add ("return-to", url);

    Application.OpenURL (gloebit_base_url + "/oauth2/authorize" +
                         EncodeQueryData (qargs));
  }


  private IEnumerator NoOpWorker
  (Action<bool> cb) {
    // no-op is a way to see if an access-token is valid
    string no_op_url = gloebit_base_url + "/no-op/";
    Hashtable headers = new Hashtable ();
    byte[] post_data = System.Text.Encoding.UTF8.GetBytes ("ignore");
    headers.Add ("Authorization", "Bearer " + access_code);

    WWW www = new WWW (no_op_url, post_data, headers);
    yield return www;

    Dictionary<string,object> response =
      (Dictionary<string,object>) Json.Deserialize (www.text);

    bool success = (bool) response[ "success" ];
    cb (success);
  }


  public void NoOp (Action<bool> cb) {
    // call NoOpWorker in background.  cb will be called when
    // the http conversation has completed.
    StartCoroutine (NoOpWorker (cb));
  }



  private IEnumerator GetUserDetailsWorker
  (Action<bool, string, string, string> cb) {
    // Ask for details about the current user.
    string ud_url = gloebit_base_url + "/user/";
    Hashtable headers = new Hashtable ();
    byte[] post_data = System.Text.Encoding.UTF8.GetBytes ("ignore");
    headers.Add ("Authorization", "Bearer " + access_code);

    WWW www = new WWW (ud_url, post_data, headers);
    yield return www;

    Dictionary<string,object> response =
      (Dictionary<string,object>) Json.Deserialize (www.text);

    bool success = (bool) response[ "success" ];
    if (success) {
      string user_id = (string) response[ "id" ];
      string user_name = (string) response[ "full-name" ];
      cb (success, (string) response[ "reason" ], user_id, user_name);
    }
    else {
      cb (success, (string) response[ "reason" ], null, null);
    }
  }

  public void GetUserDetails
  (Action<bool, string, string, string> cb) {
    StartCoroutine (GetUserDetailsWorker (cb));
  }



  private IEnumerator GetProductsWorker
  (Action<bool, string, Dictionary<string, object>> cb) {
    // Get a list of all products and product counts for the
    // current user.
    string gp_url = gloebit_base_url + "/get-user-products/";
    Hashtable headers = new Hashtable ();
    byte[] post_data = System.Text.Encoding.UTF8.GetBytes ("ignore");
    headers.Add ("Authorization", "Bearer " + access_code);

    WWW www = new WWW (gp_url, post_data, headers);
    yield return www;

    Dictionary<string,object> response =
      (Dictionary<string,object>) Json.Deserialize (www.text);

    bool success = (bool) response[ "success" ];
    if (success)
      {
        Dictionary<string,object> products =
          (Dictionary<string,object>) response[ "products" ];
        cb (success, (string) response[ "reason" ], products);
      }
    else
      {
        cb (success, (string) response[ "reason" ], null);
      }
  }


  public void GetProducts (Action<bool, string, Dictionary<string,object>> cb) {
    StartCoroutine (GetProductsWorker (cb));
  }


  private IEnumerator ConsumeProductWorker
  (string product_name, int count, Action<bool, string, string, int> cb) {
    string cup_url = gloebit_base_url + "/consume-user-product/" +
      product_name + "/" + count.ToString ();
    Hashtable headers = new Hashtable ();
    byte[] post_data = System.Text.Encoding.UTF8.GetBytes ("ignore");
    headers.Add ("Authorization", "Bearer " + access_code);

    WWW www = new WWW (cup_url, post_data, headers);
    yield return www;

    Dictionary<string,object> response =
      (Dictionary<string,object>) Json.Deserialize (www.text);

    bool success = (bool) response[ "success" ];
    if (success) {
      System.Int64 i64 = (System.Int64) response[ "product-count" ];
      int new_count = (int) i64;
      cb (success, (string) response[ "reason" ], product_name, new_count);
    }
    else {
      cb (success, (string) response[ "reason" ], product_name, -1);
    }
  }

  public void ConsumeProduct
  (string product_name, int count, Action<bool, string, string, int> cb) {
    StartCoroutine (ConsumeProductWorker (product_name, count, cb));
  }



  private IEnumerator GrantProductWorker
  (string product_name, int count, Action<bool, string, string, int> cb) {
    string gup_url = gloebit_base_url + "/grant-user-product/" +
      product_name + "/" + count.ToString ();
    Hashtable headers = new Hashtable ();
    byte[] post_data = System.Text.Encoding.UTF8.GetBytes ("ignore");
    headers.Add ("Authorization", "Bearer " + access_code);

    WWW www = new WWW (gup_url, post_data, headers);
    yield return www;

    Dictionary<string,object> response =
      (Dictionary<string,object>) Json.Deserialize (www.text);

    bool success = (bool) response[ "success" ];
    if (success) {
      System.Int64 i64 = (System.Int64) response[ "product-count" ];
      int new_count = (int) i64;
      cb (success, (string) response[ "reason" ], product_name, new_count);
    }
    else {
      cb (success, (string) response[ "reason" ], product_name, -1);
    }
  }

  public void GrantProduct
  (string product_name, int count, Action<bool, string, string, int> cb) {
    StartCoroutine (GrantProductWorker (product_name, count, cb));
  }



  private IEnumerator SetProductCountWorker
  (string product_name, int count, Action<bool, string, string, int> cb) {
    string gup_url = gloebit_base_url + "/set-user-product-count/" +
      product_name + "/" + count.ToString ();
    Hashtable headers = new Hashtable ();
    byte[] post_data = System.Text.Encoding.UTF8.GetBytes ("ignore");
    headers.Add ("Authorization", "Bearer " + access_code);

    WWW www = new WWW (gup_url, post_data, headers);
    yield return www;

    Dictionary<string,object> response =
      (Dictionary<string,object>) Json.Deserialize (www.text);

    bool success = (bool) response[ "success" ];
    if (success) {
      System.Int64 i64 = (System.Int64) response[ "product-count" ];
      int new_count = (int) i64;
      cb (success, (string) response[ "reason" ], product_name, new_count);
    }
    else {
      cb (success, (string) response[ "reason" ], product_name, -1);
    }
  }

  public void SetProductCount
  (string product_name, int count, Action<bool, string, string, int> cb) {
    StartCoroutine (SetProductCountWorker (product_name, count, cb));
  }
}

