﻿using System;
using UnityEngine;
using TheiaVR.Helpers;

namespace TheiaVR.Controllers
{
    public class UnityController : MonoBehaviour
    {
        void OnEnable()
        {
            Messages.Log("Enabling TheiaVR plugin");
        }

        void Start()
        {
            Messages.Log("Starting TheiaVR plugin");
            try
            {
                StreamController.GetInstance().Start("127.0.0.1", 11000);
            }catch(Exception vException)
            {
                Messages.Log("<color=red>" + vException.Message + "</color>");
            }
            Messages.Log("TheiaVR correctly started");
        }
        
        void OnApplicationQuit()
        {
            Messages.Log("Stopping TheiaVR plugin");
            try
            {
                StreamController.GetInstance().Stop();
            }
            catch (Exception vException)
            {
                Messages.Log("<color=red>" + vException.Message + "</color>");
            }
        }

        void OnDisable()
        {
            Messages.Log("Disabling TheiaVR plugin");
        }
    }

}
