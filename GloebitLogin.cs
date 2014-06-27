using UnityEngine;
using System;
using System.Collections;

public class GloebitLogin : MonoBehaviour
{
  bool access_code_is_good = false;

  string access_code = null;
  String new_access_code = "";

  bool checking_code = false;
  String checked_code = "";

  bool loading_next_scene = false;

  void Start ()
  {
    if (Gloebit.gloebit == null)
      {
        // The Gloebit.cs asset needs to be attached to
        // some gameObject in the first scene.
        print ("add Gloebit.cs to a game object.\n");
      }
    else
      {
        // check the url for an OAuth2 access-code
        access_code = Gloebit.gloebit.getAccessCode ();
        if (running_from_webserver ())
          {
            check_code ();
          }
      }
  }


  void check_code ()
  {
    // do a no-op to see if Gloebit recognizes the OAuth2 access-token
    checking_code = true;
    checked_code = access_code;
    Gloebit.gloebit.NoOp ((success) => {
        if (success)
          {
            checking_code = false;
            access_code_is_good = true;
            Gloebit.gloebit.setAccessCode (checked_code);
          }
        else
          {
            checking_code = false;
          }
      });
  }


  bool running_from_webserver ()
  {
    // Is this code actually deployed, or are we running from
    // the Unity editor or perhaps from a file on the local filesystem?
    if (! Application.isWebPlayer)
      {
        return false;
      }

    if (Application.absoluteURL == "" ||
        Application.absoluteURL.StartsWith ("file:"))
      return false;

    return true;
  }


  void OnGUI()
  {
    if (Gloebit.gloebit == null)
      {
        GUI.TextField
          (new Rect(20, 20, 200, 25), "Add Gloebit.cs to a game object.");
      }
    else if (access_code_is_good == false)
      {
        if (GUI.Button(new Rect(20,20,80,25), "Login"))
          {
            string url = Application.absoluteURL;
            Gloebit.gloebit.Authorize (url);
          }

        if (! running_from_webserver ())
          {
            // Gloebit can't redirect the user's browser back to
            // the url of this application unless the application is
            // actually deployed.  As a workaround, we provide a way
            // for the user to copy and paste the access code.
            GUI.Label (new Rect (20, 70, 400, 50),
                       "Because this isn't running from a webserver, " + 
                       "you'll need to copy and paste the access-code " + 
                       "by hand.");

            GUI.Label (new Rect (20, 120, 40, 25), "Code: ");

            new_access_code = GUI.TextField
              (new Rect (70, 120, 200, 25), new_access_code, 36);

            if (GUI.Button(new Rect (290, 120, 40, 25), "Set"))
              {
                access_code = new_access_code;
                Gloebit.gloebit.setAccessCode (access_code);
              }

            if (checking_code)
              {
                GUI.Label (new Rect (20, 170, 400, 50), "checking code...");
              }
            else if (checking_code == false &&
                     access_code == checked_code &&
                     new_access_code == access_code &&
                     access_code != "" &&
                     access_code != null)
              {
                GUI.Label (new Rect (20, 170, 400, 50), "Code is not valid.");
              }

            if (checking_code == false &&
                access_code != checked_code &&
                access_code != "" &&
                access_code != null)
              {
                check_code ();
              }
          }
      }
    else if (! loading_next_scene)
      {
        // We have an access-token and it appears to work.  If there's
        // a scene after this one, load it.  The static Gloebit.gloebit
        // object will hang around and be available in the next scene.
        if (Application.loadedLevel + 1 < Application.levelCount)
          {
            loading_next_scene = true;
            print ("loading next scene...\n");
            Application.LoadLevel (Application.loadedLevel + 1);
          }
        else
          {
            // no next scene/level after this one, so just put up
            // a message.
            GUI.TextField
              (new Rect(20, 20, 200, 25), "Create the next scene.");
          }
      }
  }
}

