using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using LPE.JSON;


public class JSONTestMain : MonoBehaviour {
    void Start() {
        var obj = new JSONObject();

        // basic info
        obj["firstName"].SetValue("John");
        obj["lastName"].SetValue("Smith");
        obj["isAlive"].SetValue(true);
        obj["age"].SetValue(27.93781f);

        // address
        obj["adress"].SetAsObject();
        obj["adress"]["streetAddress"].SetValue("123123 Main Street");
        obj["adress"]["city"].SetValue("Chicago");
        obj["adress"]["state"].SetValue("IL");
        obj["adress"]["postalCode"].SetValue("83513");

        // phoneNumbers
        obj["phoneNumbers"].SetAsArray();

        obj["phoneNumbers"].ArrayAddObject();
        obj["phoneNumbers"].ArrayLast()["type"].SetValue("home");
        obj["phoneNumbers"].ArrayLast()["number"].SetValue("123 457-7863");

        obj["phoneNumbers"].ArrayAddObject();
        obj["phoneNumbers"].ArrayLast()["type"].SetValue("office");
        obj["phoneNumbers"].ArrayLast()["number"].SetValue("136 127-6915");

        // children
        obj["children"].SetAsArray();
        obj["children"].ArrayAdd("Adam");
        obj["children"].ArrayAdd("Bob");
        obj["children"].ArrayAdd("Charlie");

        // spouse
        obj["spouse"].SetAsNull();

        var json = obj.ToString();
        print(json);
        var obj2 = new JsonParser().Parse(json);
        print(obj2.ToString());
    }
}
//public static class GameSaveDataUtility {
//    public static readonly string saveDir = Application.dataPath + $"/saves";
//    static readonly string[] saveSlotPaths = {
//            saveDir + "/slot_1.txt",
//            saveDir + "/slot_2.txt",
//            saveDir + "/slot_3.txt"
//        };

//    static readonly string[] autoSaveSlotPaths = {
//            saveDir + "/slot_1_auto.txt",
//            saveDir + "/slot_2_auto.txt",
//            saveDir + "/slot_3_auto.txt"
//    }; 


//    public static bool SaveExists(int slot, bool auto) {
//        return File.Exists((auto ? autoSaveSlotPaths : saveSlotPaths)[slot]);
//    }
//    public static void SaveToFile(JSONObject json, bool auto) {
//        Debug.LogWarning("Remember to change path of save directory");
//        Directory.CreateDirectory(saveDir);
//        var txt = json.ToString(indent: 0);
//        File.WriteAllText( saveSlotPaths[0], txt);
//    }

//    //public static GameSaveData LoadSave(int slot, bool auto) {
//    //    var path = (auto ? autoSaveSlotPaths : saveSlotPaths)[slot];
//    //    var source = File.ReadAllText(path);
//    //    var json = EncryptionUtility.Decrypt(source);

//    //    GameSaveData result = new GameSaveData();
//    //    result.json = new JsonParser().Parse(json);
//    //    result.saveSlot = slot;
//    //    return result;
//    //}
//}