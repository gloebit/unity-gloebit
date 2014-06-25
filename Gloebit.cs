using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MiniJSON;


public class Gloebit : MonoBehaviour
{
  public static Gloebit gloebit = null; // singleton


  private string access_code = null;
  public string consumer_key = "test-consumer";
  // public string gloebit_base_url = "https://sandbox.gloebit.com";
  public string gloebit_base_url = "https://api0.gloebit.com";


  void Awake ()
  {
    if (gloebit == null)
      {
        DontDestroyOnLoad (gameObject);
        gloebit = this;
      }
    else
      {
        Destroy (gameObject);
      }
  }


  public Gloebit ()
  {
  }


  private string EncodeQueryData (Dictionary<String,String> data) {
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
    access_code = getQueryVariable ("code");
    return access_code;
  }


  public void setAccessCode (string new_access_code) {
    access_code = new_access_code;
  }


  public void Authorize (string url) {
    Dictionary<string, string> qargs =
      new Dictionary<string, string>();
    qargs.Add ("scope", "inventory");
    qargs.Add ("redirect_uri", url);
    qargs.Add ("response_type", "token");
    qargs.Add ("client_id", consumer_key);
    // qargs.Add ("r", "test");
    qargs.Add ("return-to", url);

    Application.OpenURL (gloebit_base_url + "/oauth2/authorize" +
                         EncodeQueryData (qargs));
  }



  private IEnumerator NoOpWorker
  (Action<bool> cb) {
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
    StartCoroutine (NoOpWorker (cb));
  }





  private IEnumerator GetProductsWorker
  (Action<Dictionary<string, object>> cb) {
    string gp_url = gloebit_base_url + "/get-user-products/";
    Hashtable headers = new Hashtable ();
    byte[] post_data = System.Text.Encoding.UTF8.GetBytes ("ignore");
    headers.Add ("Authorization", "Bearer " + access_code);

    WWW www = new WWW (gp_url, post_data, headers);
    yield return www;

    Dictionary<string,object> response =
      (Dictionary<string,object>) Json.Deserialize (www.text);

    bool success = (bool) response[ "success" ];
    if (success) {
      Dictionary<string,object> products =
        (Dictionary<string,object>) response[ "products" ];
      cb (products);
    }
  }


  public void GetProducts (Action<Dictionary<string,object>> cb) {
    StartCoroutine (GetProductsWorker (cb));
  }


  private IEnumerator ConsumeProductWorker
  (string product_name, int count, Action<bool, int> cb) {
    string cp_url = gloebit_base_url + "/consume-user-product/" +
      product_name + "/" + count.ToString ();
    Hashtable headers = new Hashtable ();
    byte[] post_data = System.Text.Encoding.UTF8.GetBytes ("ignore");
    headers.Add ("Authorization", "Bearer " + access_code);

    WWW www = new WWW (cp_url, post_data, headers);
    yield return www;

    Dictionary<string,object> response =
      (Dictionary<string,object>) Json.Deserialize (www.text);

    bool success = (bool) response[ "success" ];
    if (success) {
      System.Int64 i64 = (System.Int64) response[ "product-count" ];
      int new_count = (int) i64;
      cb (success, new_count);
    }
    else {
      cb (success, -1);
    }
  }


  public void ConsumeProduct
  (string product_name, int count, Action<bool, int> cb) {
    StartCoroutine (ConsumeProductWorker (product_name, count, cb));
  }
}

