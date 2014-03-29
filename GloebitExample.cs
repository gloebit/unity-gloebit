using UnityEngine;
using System;
using System.Collections;
using Gloebit;

public class GloebitExample : MonoBehaviour {

  string access_code = null;
  string balance = null;

  string log_data = "";

  int product_hats = 0;
  int product_shoes = 0;

  Gloebit.GloebitUser gbit;


  void Start () {
    gbit = gameObject.AddComponent<Gloebit.GloebitUser>();
    access_code = gbit.getAccessCode ("test-consumer");
    if (access_code != null) {
      refresh_balance ();
      refresh_products ();
    }
  }


  void OnGUI() {
    if (access_code == null) {
      if (GUI.Button(new Rect(75,60,80,20), "Connect")) {
        string url = Application.absoluteURL;
        gbit.Authorize (url);
      }
    }
    else {
      GUI.Label (new Rect (40, 10, 300, 30),
                 "Balance: " + balance + " gloebits");

      GUI.Label (new Rect (40, 40, 80, 30),
                 "hats: " + product_hats);
      if (GUI.Button(new Rect(120,40,130,30), "Buy a hat: 50 G$")) {
        gbit.BuyProduct
          ("hat", 1,
           (success, current_balance, new_count) => {
            balance = gbit.formatBalance (current_balance);
            product_hats = new_count;
          });
      }
      if (GUI.Button(new Rect(260,40,150,30), "Destroy a hat")) {
        gbit.ConsumeProduct ("hat", 1, (success, new_count) => {
            product_hats = new_count;
          });
      }


      GUI.Label (new Rect (40, 70, 80, 30),
                 "shoes: " + product_shoes);
      if (GUI.Button(new Rect(120,70,130,30), "Buy a shoe: 15 G$")) {
        gbit.BuyProduct
          ("shoe", 1,
           (success, current_balance, new_count) => {
            balance = gbit.formatBalance (current_balance);
            product_shoes = new_count;
          });
      }
      if (GUI.Button(new Rect(260,70,150,30), "Destroy a shoe")) {
        gbit.ConsumeProduct ("shoe", 1, (success, new_count) => {
            product_shoes = new_count;
          });
      }

      GUI.Label (new Rect (40, 200, 800, 600), log_data);

    }
  }


  void refresh_balance () {
    gbit.GetBalance ((current_balance) => {
        balance = gbit.formatBalance (current_balance);
      });
  }


  void refresh_products () {
    gbit.GetProducts ((products) => {
        foreach (string product_name in products.Keys) {
          int product_count = (int) (System.Int64) products[ product_name ];

          if (product_name == "hat") {
            product_hats = product_count;
          }
          if (product_name == "shoe") {
            product_shoes = product_count;
          }
        }
      });
  }

}


