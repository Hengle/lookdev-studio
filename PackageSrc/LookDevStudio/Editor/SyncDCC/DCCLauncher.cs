﻿#if ENABLE_SYNC_DCC

#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

using UnityEditor;

public static class DCCLauncher
{
    public static string mayaPath; // maya.exe
    public static string maxPath;  // 3dsmax.exe
    public static string blenderPath; // blender.exe


    public static readonly List<string> DCCPath = new List<string>() { "maya.exe", "3dsmax.exe", "blender.exe" };

    static MayaCommands mayaCommands;
    static MaxCommands maxCommands;
    static BlenderCommands blenderCommands;


    // TBD
    static void GetDCCPath()
    {
        DriveInfo[] allDrives = DriveInfo.GetDrives();



        // Temp - Hardcoded paths
        mayaPath = @"C:\Program Files\Autodesk\Maya2019\bin\maya.exe";
        maxPath = @"C:\Program Files\Autodesk\3ds Max 2019\3dsmax.exe";
        blenderPath = @"C:\Program Files (x86)\Steam\steamapps\common\Blender\blender.exe";
    }


    public static void Load(DCCType currentDCCType, string targetFBX)
    {
        if (!File.Exists(targetFBX))
        {
            Debug.LogError(string.Format("Could not find the file : {0}", targetFBX));
            return;
        }

        GetDCCPath();

        if (mayaCommands == null)
            mayaCommands = new MayaCommands("localhost", 6000, 4096);

        if (maxCommands == null)
            maxCommands = new MaxCommands("localhost", 6001, 4096);

        if (blenderCommands == null)
            blenderCommands = new BlenderCommands("localhost", 6002, 4096);


        switch (currentDCCType)
        {
            case DCCType.MAYA:
                if (mayaCommands.IsMayaRunning() && mayaCommands.IsConnectedMaya())
                {
                    mayaCommands.SendPredesinedMessageToMaya(CommandType.IMPORT_FBX, new string[] { targetFBX });
                }
                else
                {
                    RunMayaWithFBX(targetFBX);
                }

                break;

            case DCCType.MAX:
                break;

            case DCCType.BLENDER:
                break;
        }
    }


    public static void Save(DCCType currentDCCType, string targetFBX)
    {
        GetDCCPath();

        if (mayaCommands == null)
            mayaCommands = new MayaCommands("localhost", 6000, 4096);

        if (maxCommands == null)
            maxCommands = new MaxCommands("localhost", 6001, 4096);

        if (blenderCommands == null)
            blenderCommands = new BlenderCommands("localhost", 6002, 4096);


        switch (currentDCCType)
        {
            case DCCType.MAYA:
                if (mayaCommands.IsMayaRunning() && mayaCommands.IsConnectedMaya())
                {
                    mayaCommands.SendPredesinedMessageToMaya(CommandType.EXPORT_FBX, new string[] { targetFBX });
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Could not find the Maya that is running or was connected to Unity.", "Ok");
                    return;
                }

                break;

            case DCCType.MAX:
                break;

            case DCCType.BLENDER:
                break;
        }

    }


    public static void CreateMaterial(DCCType currentDCCType, string materialName)
    {
        GetDCCPath();

        if (mayaCommands == null)
            mayaCommands = new MayaCommands("localhost", 6000, 4096);

        if (maxCommands == null)
            maxCommands = new MaxCommands("localhost", 6001, 4096);

        if (blenderCommands == null)
            blenderCommands = new BlenderCommands("localhost", 6002, 4096);


        switch (currentDCCType)
        {
            case DCCType.MAYA:
                //if (mayaCommands.IsMayaRunning() && mayaCommands.IsConnectedMaya())
                if (mayaCommands.IsMayaRunning())
                {
                    mayaCommands.SendPredesinedMessageToMaya(CommandType.CREATE_MATERIAL, new string[] { materialName });
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Could not find the Maya that is running or was connected to Unity.", "Ok");
                    return;
                }

                break;

            case DCCType.MAX:
                break;

            case DCCType.BLENDER:
                break;
        }
    }


    public static void CreateTexture(DCCType currentDCCType, string textureName, string texturePath)
    {
        GetDCCPath();

        if (mayaCommands == null)
            mayaCommands = new MayaCommands("localhost", 6000, 4096);

        if (maxCommands == null)
            maxCommands = new MaxCommands("localhost", 6001, 4096);

        if (blenderCommands == null)
            blenderCommands = new BlenderCommands("localhost", 6002, 4096);


        switch (currentDCCType)
        {
            case DCCType.MAYA:
                //if (mayaCommands.IsMayaRunning() && mayaCommands.IsConnectedMaya())
                if (mayaCommands.IsMayaRunning())
                {
                    mayaCommands.SendPredesinedMessageToMaya(CommandType.CREATE_TEXTURE, new string[] { textureName, texturePath });
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Could not find the Maya that is running or was connected to Unity.", "Ok");
                    return;
                }

                break;

            case DCCType.MAX:
                break;

            case DCCType.BLENDER:
                break;
        }
    }


    public static void LinkTextureToMaterial(DCCType currentDCCType, string textureName, string textureAttribute, string targetMaterialName, string targetAttributeName)
    {
        if (textureName == string.Empty || textureAttribute == string.Empty || targetMaterialName == string.Empty || targetAttributeName == string.Empty)
            return;

        GetDCCPath();

        if (mayaCommands == null)
            mayaCommands = new MayaCommands("localhost", 6000, 4096);

        if (maxCommands == null)
            maxCommands = new MaxCommands("localhost", 6001, 4096);

        if (blenderCommands == null)
            blenderCommands = new BlenderCommands("localhost", 6002, 4096);


        switch (currentDCCType)
        {
            case DCCType.MAYA:
                //if (mayaCommands.IsMayaRunning() && mayaCommands.IsConnectedMaya())
                if (mayaCommands.IsMayaRunning())
                {
                    mayaCommands.SendPredesinedMessageToMaya(CommandType.LINK_TEXTURETOMATERIAL, new string[] { string.Format("{0}.{1}", textureName, textureAttribute), string.Format("{0}.{1}", targetMaterialName, targetAttributeName) });
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Could not find the Maya that is running or was connected to Unity.", "Ok");
                    return;
                }

                break;

            case DCCType.MAX:
                break;

            case DCCType.BLENDER:
                break;
        }
    }


    public static void SendCommand(DCCType currentDCCType, string command)
    {
        GetDCCPath();

        if (mayaCommands == null)
            mayaCommands = new MayaCommands("localhost", 6000, 4096);

        if (maxCommands == null)
            maxCommands = new MaxCommands("localhost", 6001, 4096);

        if (blenderCommands == null)
            blenderCommands = new BlenderCommands("localhost", 6002, 4096);


        switch (currentDCCType)
        {
            case DCCType.MAYA:
                //if (mayaCommands.IsMayaRunning() && mayaCommands.IsConnectedMaya())
                if (mayaCommands.IsMayaRunning())
                {
                    mayaCommands.SendMessageToMaya(command);
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Could not find the Maya that is running or was connected to Unity.", "Ok");
                    return;
                }

                break;

            case DCCType.MAX:
                break;

            case DCCType.BLENDER:
                break;
        }
    }




    static void RunMayaWithFBX(string FBXPath)
    {

        if (mayaCommands.IsMayaRunning())
        {
            mayaCommands.KillMayaProcess();
        }

        string[] guids = AssetDatabase.FindAssets("InitCommandPortOnMaya", new string[] { "Assets" });

        if (guids.Length == 0)
        {
            Debug.LogError("Could not find the MEL Script : InitCommandPortOnMaya");
            return;
        }

        string cmdPortScriptPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        cmdPortScriptPath = Path.GetFullPath(cmdPortScriptPath);
        cmdPortScriptPath = cmdPortScriptPath.Replace("\\", "/");


        if (!File.Exists(cmdPortScriptPath))
        {
            Debug.LogError("Could not find the path : " + cmdPortScriptPath);
            return;
        }

        string scriptPath = cmdPortScriptPath.Replace(Path.GetFileName(cmdPortScriptPath), string.Empty);
        scriptPath = scriptPath.Replace("\\", "/");


        string[] melScripts = Directory.GetFiles(scriptPath, "*.mel");

        string command = string.Empty;

        foreach (string melScript in melScripts)
        {
            string curMelPath = melScript.Replace("\\", "/");
            command = command + string.Format("source \\\"{0}\\\";", curMelPath);
        }

        if (File.Exists(FBXPath))
        {
            string modelImpotCmd = string.Format("ImportTargetFBX(\\\"{0}\\\");", FBXPath);
            command = command + modelImpotCmd;
        }

        System.Diagnostics.Process proc = new System.Diagnostics.Process();


        proc.StartInfo.FileName = mayaPath;
        proc.StartInfo.Arguments = string.Format("-command \"{0}\"", command);

        Debug.Log(proc.StartInfo.FileName);
        Debug.Log(proc.StartInfo.Arguments);

        proc.Start();
        proc.Close();

    }

    /*
    private static List<string> GetDefaultVendorDirectories()
    {

        List<string> existingDirectories = new List<string>();
        List<string> searchDirectories = new List<string>();

        switch (Application.platform)
        {
            case RuntimePlatform.WindowsEditor:
                {
                    DriveInfo[] allDrives = DriveInfo.GetDrives();
                    foreach (DriveInfo drive in allDrives)
                    {
                        searchDirectories.Add(Path.Combine(drive.Name, @"Program Files\Autodesk"));
                        searchDirectories.Add(Path.Combine(drive.Name, @"Program Files\Blender Foundation"));
                    }
                    break;
                }
            case RuntimePlatform.OSXEditor:
                {
                    searchDirectories.Add("/Applications/Autodesk");
                    searchDirectories.Add("/Applications");
                    break;
                }
            case RuntimePlatform.LinuxEditor:
                {

                    searchDirectories.Add(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile));
                    searchDirectories.Add("/usr/autodesk");
                    break;
                }
            default:
                {
                    throw new System.NotImplementedException();
                }
        }

        foreach (string path in searchDirectories)
        {
            if (Directory.Exists(path))
            {
                existingDirectories.Add(path);
            }
        }

        return existingDirectories;
    }
    */


}

#endif

#endif