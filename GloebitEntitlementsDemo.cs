using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GloebitEntitlementsDemo : MonoBehaviour
{
  Dictionary<string,object> products = null;
  bool refreshing_products = false;

  // Test Consumer has a predefined list of products.
  string[] all_products = new string[7]{"hat", "shirt", "pants", "shoe",
                                        "backpack", "torch", "knife"};

  public int vertical_spacing = 25;
  public int vertical_size = 22;

  void Start ()
  {
    if (Gloebit.gloebit == null)
      {
        print ("add Gloebit.cs to a game object.\n");
      }
  }


  void refresh_products ()
  {
    if (! refreshing_products)
      {
        print ("refreshing user's product list\n");

        refreshing_products = true;
        Gloebit.gloebit.GetProducts ((success, reason, new_products) => {
            products = new_products;
            if (! success)
              {
                print ("GetProducts failed -- " + reason);
              }
            refreshing_products = false;
          });
      }
  }


  void OnGUI()
  {
    int y = 20;

    if (products == null)
      {
        refresh_products ();
        return;
      }

    // Loop through the predefined list of products and show
    // how many of each the current user has.
    foreach (string product_name in all_products)
      {
        int count = 0;

        if (products.ContainsKey (product_name))
          {
            System.Int64 v = (System.Int64) products[ product_name ];
            count = (int) v;
          }

        GUI.Label (new Rect (20, y, 100, vertical_size), product_name);
        GUI.Label (new Rect (120, y, 40, vertical_size), "" + count);
        if (GUI.Button (new Rect (165, y, 45, vertical_size), "more"))
          {
            // GrantProduct will attempt to increment the number
            // a given product the current user has
            Gloebit.gloebit.GrantProduct
              (product_name, 1, (success, reason, pname, new_count) => {
                if (success)
                  {
                    print ("GrantProduct succeeded.");
                    products[ pname ] = (System.Int64) new_count;
                  }
                else
                {
                  print ("GrantProduct failed: " + reason);
                }
              });
          }
        if (GUI.Button (new Rect (220, y, 40, vertical_size), "less"))
          {
            // ConsumeProduct will attempt to decrement the number
            // a given product the current user has
            Gloebit.gloebit.ConsumeProduct
              (product_name, 1, (success, reason, pname, new_count) => {
                if (success)
                  {
                    print ("ConsumeProduct succeeded.");
                    products[ pname ] = (System.Int64) new_count;
                  }
                else
                {
                  print ("ConsumeProduct failed: " + reason);
                }
              });
          }

        y += vertical_spacing;
      }
  }
}