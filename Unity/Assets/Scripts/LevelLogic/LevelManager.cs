﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static bool EditorEnabled => _editorEnabled;

    private const int MAX_LEVEL_SIZE = 100;

    private string _dataPath;

    private static bool _editorEnabled = false;

    public GameObject LevelTilePrefab;
    public GameObject TileGrid;

    public InputField LevelIdInput;

    public GameObject EditorUI;
    public GameObject GameUI;

    void Start()
    {
        _dataPath = Application.persistentDataPath + "/Levels/";

        LevelIdInput.text = SceneManager.LevelId.ToString();

        LoadLevelFromFile();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            _editorEnabled = !_editorEnabled;
        }
        EditorUI.SetActive(_editorEnabled);
        //GameUI.SetActive(!_editorEnabled);
    }

    public void ChangeLevelID()
    {
        if (LevelIdInput.text == "") return;

        Debug.Log("Level ID changed -> " + LevelIdInput.text);
        SceneManager.LevelId = Convert.ToInt32(LevelIdInput.text);
    }

    public void SaveLevelToFile()
    {
        Debug.Log("Saving...");
        File.WriteAllText(GetLevelFile(), "");//TODO: Header

        //Find all level tiles and serialize them
        LevelTileX[] tiles = FindObjectsOfType<LevelTileX>();
        TileData[] tileData = new TileData[tiles.Length];
        for(int i = 0; i < tiles.Length; i++)
        {
            tileData[i] = tiles[i].GetTileData();
        }
        //Save level tiles
        SaveObjectToFile(tileData, "tiles");

        Debug.Log("Level Layout saved!");

        //Save Dispenser Items
        ItemDispenser[] dispenser = Resources.FindObjectsOfTypeAll<ItemDispenser>().Where(d => d.Save == true).ToArray();
        DispenserData[] dispenserData = new DispenserData[dispenser.Length];
        for(int i = 0; i < dispenser.Length; i++)
        {
            dispenserData[i] = dispenser[i].GetDispenserData();
        }
        SaveObjectToFile(dispenserData, "dispenser");

        Debug.Log("Dispenser data saved!");
    }

    private void SaveObjectToFile(object obj, string suffix)
    {
        string targetPath = GetLevelFile() + "." + suffix;

        if (File.Exists(targetPath))
        {
            File.Delete(targetPath);
        }

        FileStream file = File.Create(targetPath, 1024, FileOptions.None);
        DataContractSerializer bf = new DataContractSerializer(obj.GetType());
        bf.WriteObject(file, obj);
        file.Close();
    }

    public void LoadLevelFromFile()
    {
        TileGrid.transform.RemoveAllChildren();
        if (File.Exists(GetLevelFile()))
        {
            TileData[] tileData = (TileData[])LoadObjectFromFile(typeof(TileData[]), "tiles");
            foreach (TileData tile in tileData)
            {
                tile.Instantiate(LevelTilePrefab, TileGrid);
            }

            DispenserData[] dispenserData = (DispenserData[])LoadObjectFromFile(typeof(DispenserData[]), "dispenser");
            ItemDispenser[] dispenser = Resources.FindObjectsOfTypeAll<ItemDispenser>().Where(d => d.Save == true).ToArray();
            for (int i = 0; i < dispenser.Length && i < dispenserData.Length; i++)
            {
                 dispenser[i].SetDispenserData(dispenserData[i]);
            }
        }
    }

    private object LoadObjectFromFile(Type type, string suffix)
    {
        string targetPath = GetLevelFile() + "." + suffix;

        FileStream file = File.OpenRead(targetPath);
        DataContractSerializer bf = new DataContractSerializer(type);
        object obj = bf.ReadObject(file);
        file.Close();
        return obj;
    }

    private string GetLevelFile()
    {
        string targetPath = "";
#if UNITY_EDITOR
        targetPath = Application.dataPath + "/Resources/Levels/" + SceneManager.LevelId;
#else
        targetPath = _dataPath + SceneManager.LevelId;
#endif

        return targetPath;
    }
}
