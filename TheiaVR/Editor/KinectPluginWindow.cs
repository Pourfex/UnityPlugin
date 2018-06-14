﻿using System;
using System.Collections.Generic;
using TheiaVR.Controllers;
using TheiaVR.Graphics;
using TheiaVR.Helpers;
using TheiaVR.Model;
using UnityEngine;
using UnityEditor;

namespace TheiaVR.Editor
{
    public class KinectPluginWindow : EditorWindow
    {
        private bool enableUnityLogs;
        private bool enableFileLogs;
        private bool started;
        private bool stopped;

        public GameObject cloudMesh;


        private List<NetworkConfig> networkConfigs;

        private readonly GUILayoutOption[] widthTable = new[]
        {
            GUILayout.Width(150),
            GUILayout.Width(75),
            GUILayout.Width(75),
            GUILayout.Width(40),
            GUILayout.Width(40),
            GUILayout.Width(150),
            GUILayout.Width(75)
        };

        private int minValueSlider = 0; 
        private int maxValueSlider = 5; 

        // Add menu item named "Kinect Plugin" to the Window menu
        [MenuItem("Kinect Plugin/Show plugin")]
        public static void ShowWindow()
        {
            GetWindow(typeof(KinectPluginWindow), false, "Kinect Plugin");
        }

        private void OnEnable()
        {
            networkConfigs = new List<NetworkConfig>();

            int networkConfigsCount = EditorPrefs.GetInt("networkConfigsCount", 1);
            enableUnityLogs = EditorPrefs.GetBool("enableUnityLogs", true);
            enableFileLogs = EditorPrefs.GetBool("enableFileLogs", false);

            for (int i = 0; i < networkConfigsCount; i++)
            {
                string ipAddress = EditorPrefs.GetString("ipAddress" + i, "127.0.0.1");
                int cloudPort = EditorPrefs.GetInt("cloudPort" + i, 9876);
                int skelPort = EditorPrefs.GetInt("skelPort" + i, 9877);
                bool enableCloud = EditorPrefs.GetBool("enableCloud" + i, true);
                bool enableSkel = EditorPrefs.GetBool("enableSkel" + i, true);
                int remanence = EditorPrefs.GetInt("remanence" + i, 0);

                networkConfigs.Add(new NetworkConfig(i,ipAddress, cloudPort, skelPort, enableCloud, enableSkel, remanence));
            }
        }

        private void OnDisable()
        {
            EditorPrefs.SetInt("networkConfigsCount", networkConfigs.Count);
            EditorPrefs.SetBool("enableUnityLogs", enableUnityLogs);
            EditorPrefs.SetBool("enableFileLogs", enableFileLogs);

            for (int i = 0; i < networkConfigs.Count; i++)
            {
                EditorPrefs.SetString("ipAddress" + i, networkConfigs[i].IpAddress);
                EditorPrefs.SetInt("cloudPort" + i, networkConfigs[i].CloudPort);
                EditorPrefs.SetInt("skelPort" + i, networkConfigs[i].SkelPort);
                EditorPrefs.SetBool("enableCloud" + i, networkConfigs[i].EnableCloud);
                EditorPrefs.SetBool("enableSkel" + i, networkConfigs[i].EnableSkel);
                EditorPrefs.SetInt("remanence" + i, networkConfigs[i].Remanence);
            }
        }

        private void Awake()
        {
            if (PluginController.GetInstance().IsActive())
            {
                DisplayStopUI();
                Messages.Log("Displaying stop");
            }
            else
            {
                DisplayStartUI();
                Messages.Log("Displaying start");
            }
        }        

        private void DisplayStartUI()
        {
            stopped = true;
            started = false;
        }

        private void DisplayStopUI()
        {
            stopped = false;
            started = true;
        }

        private void CheckPlayModeState(PlayModeStateChange aState)
        {
            if (aState == PlayModeStateChange.ExitingPlayMode)
            {
                DisplayStartUI();
            }
        }

        void OnGUI()
        {
            // Listening to play button
            EditorApplication.playModeStateChanged += CheckPlayModeState;


            GUILayout.Label("Network settings", EditorStyles.boldLabel);

            if (networkConfigs.Count >= 1)
            {
                DrawTable();
            }

            DrawAddButton();

            GUILayout.Label("Logs", EditorStyles.boldLabel);
            if (EditorGUILayout.Toggle("Display Unity logs", enableUnityLogs) != enableUnityLogs)
            {
                enableUnityLogs = !enableUnityLogs;
                if (enableUnityLogs)
                {
                    Messages.EnableUnityLogs();
                }

                else
                {
                    Messages.DisableUnityLogs();
                }
            }
            
            if (EditorGUILayout.Toggle("Store logs in file", enableFileLogs) != enableFileLogs)
            {
                enableFileLogs = !enableFileLogs;
                if (enableFileLogs)
                {
                    Messages.EnableFileLogs();
                }
                else
                {
                    Messages.DisableFileLogs();
                }
            }

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (stopped)
            {
                EditorGUI.BeginDisabledGroup(CheckIfNothingIsChecked() || !EditorApplication.isPlaying);
                if (GUILayout.Button("Start", GUILayout.Width(70)))
                {
                    Messages.Log("Starting TheiaVR plugin");
                    try
                    {
                        PluginController.GetInstance().Start(networkConfigs);
                        
                        DisplayStopUI();
                    }
                    catch (Exception vException)
                    {
                        Messages.LogError(vException.Message);
                    }

                    Messages.Log("TheiaVR correctly started");
                }

                EditorGUI.EndDisabledGroup();
            }

            if (started)
            {
                if (GUILayout.Button("Stop", GUILayout.Width(70)))
                {
                    Messages.Log("Stopping TheiaVR plugin");
                    try
                    {
                        PluginController.GetInstance().Stop();
                        DisplayStartUI();
                    }
                    catch (Exception vException)
                    {
                        Messages.LogError(vException.Message);
                    }
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            minSize = new Vector2(601f, 248f);
        }

        void DrawTable()
        {
            EditorGUILayout.BeginVertical();

            DrawHeader();
            EditorGUI.BeginDisabledGroup(started);
            for (int i = 0; i < networkConfigs.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                networkConfigs[i].IpAddress = EditorGUILayout.TextField(networkConfigs[i].IpAddress, widthTable[0]);
                networkConfigs[i].CloudPort = EditorGUILayout.IntField(networkConfigs[i].CloudPort, widthTable[1]);
                networkConfigs[i].SkelPort = EditorGUILayout.IntField(networkConfigs[i].SkelPort, widthTable[2]);
                if (EditorGUILayout.Toggle(networkConfigs[i].EnableCloud, widthTable[3]) != networkConfigs[i].EnableCloud)
                {
                    networkConfigs[i].EnableCloud = !networkConfigs[i].EnableCloud;
                }

                if (EditorGUILayout.Toggle(networkConfigs[i].EnableSkel, widthTable[4]) != networkConfigs[i].EnableSkel)
                {
                    for (int j = 0; j < networkConfigs.Count; j++)
                    {
                        if (j != i)
                        {
                            networkConfigs[j].EnableSkel = false;
                        }
                    }

                    networkConfigs[i].EnableSkel = !networkConfigs[i].EnableSkel;
                }

//                networkConfigs[i].Remanence = EditorGUILayout.IntField(networkConfigs[i].Remanence, widthTable[5]);
                networkConfigs[i].Remanence = EditorGUILayout.IntSlider(networkConfigs[i].Remanence, minValueSlider, maxValueSlider, widthTable[5]);

                if (GUILayout.Button("Delete", widthTable[6]))
                {
                    networkConfigs.RemoveAt(i);
                    EditorPrefs.DeleteKey("ipAddress" + i);
                    EditorPrefs.DeleteKey("cloudPort" + i);
                    EditorPrefs.DeleteKey("skelPort" + i);
                    EditorPrefs.DeleteKey("enableCloud" + i);
                    EditorPrefs.DeleteKey("enableSkel" + i);
                    EditorPrefs.DeleteKey("remanence" + i);
                }
                

                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();
        }

        void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("IP Address", widthTable[0]);
            GUILayout.Label("Cloud port", widthTable[1]);
            GUILayout.Label("Skel port", widthTable[2]);
            GUILayout.Label("Cloud", widthTable[3]);
            GUILayout.Label("Skel", widthTable[4]);
            GUILayout.Label("Remanence", widthTable[5]);
            GUILayout.Label("", widthTable[6]);

            EditorGUILayout.EndHorizontal();
        }

        void DrawAddButton()
        {
            EditorGUILayout.Separator();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(started);
            if (GUILayout.Button("Add a new Kinect"))
            {
                networkConfigs.Add(new NetworkConfig(networkConfigs.Count, "127.0.0.1", 9876, 9877, true, false, 0));
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        bool CheckIfNothingIsChecked()
        {
            foreach (var conf in networkConfigs)
            {
                if (conf.EnableCloud || conf.EnableSkel)
                {
                    return false;
                }
            }

            return true;
        }
    }
}