using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Json으로 읽어들이는 데이터의 포맷 클래스는 ILoader인터페이스를 구현해야 함.
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public interface ILoader<TKey, TValue>
{
    /// <summary>
    /// 데이터를 Dictionary 형태로 변환하는 메서드. 
    /// Dictionary의 키와 값은 인터페이스의 제네릭 타입으로 지정.
    /// </summary>
    /// <returns></returns>
    Dictionary<TKey, TValue> MakeDict();
}

/// <summary>
/// 데이터를 관리하는 매니저.
/// </summary>
public class DataManager
{
    /// <summary>
    /// 저장 경로 
    /// </summary>
    private string _path;
    
    /// <summary>
    /// 게임 내 정보를 담는 Dictionary. 
    /// 처음 게임이 시작될 때, Init() 함수를 통해 필요한 정보를 여기에 캐싱하고,
    /// 이후에 필요할 때는 여기를 참조해 정보를 활용한다.
    /// </summary>
    public Dictionary<int, Data.Stat> StatDict { get; private set; } = new Dictionary<int, Data.Stat>();

    /// <summary>
    /// 인벤토리 칸의 갯수. 현재는 테스트용으로 10칸만 존재한다.
    /// </summary>
    public const int NumberOfInventorySlots = 10;

    /// <summary>
    /// 아이템 정보를 캐싱하는 ushort 배열
    /// </summary>
    public ushort[] InventoryTable;
    
    /// <summary>
    /// 아이템 id에 해당하는 아이템의 이름을 저장하는 딕셔너리
    /// </summary>
    Dictionary<int, string> itemCodeDict = new Dictionary<int, string>()
    {
        {0, "NONE" },
        {1, "Sword" },
        {2,  "Bow"  },
        {100, "Wood" },
        {101, "Stone" },
    };
    
    /// <summary>
    /// 아이템 이름에 해당하는 아이템의 코드를 저장하는 딕셔너리
    /// </summary>
    Dictionary<string, int> reverseItemCodeDict = new Dictionary<string, int>()
    {
        {"NONE", 0 },
        {"Sword", 1 },
        {"Bow", 2  },
        {"Wood", 100},
        {"Stone", 101 },
    };

    public const int BOUNDARY = 100;
    
    public UnityAction OnClose = null;
    
    /// <summary>
    /// 게임이 시작할 때 데이터를 로드하는 초기화 메서드. 
    /// LoadJson 메서드를 사용해 지정된 경로의 Json 파일로부터 데이터를 로드하고,
    /// ILoader 인터페이스를 구현하는 객체의 MakeDict 함수를 호출해 정보를 캐싱한다.
    /// </summary>
    public void Init()
    {
        // 데이터 저장 기본 경로
        _path = Application.persistentDataPath + "/";
        
        // 다음과 같은 형태로 사용 가능
        StatDict = LoadJson<Data.StatData, int, Data.Stat>("StatData").MakeDict();
        
        // 인벤토리 데이터 테스트용 데이터 초기화
        InventoryTable = new ushort[NumberOfInventorySlots];
        for(int i = 0; i < NumberOfInventorySlots; i++)
            InventoryTable[i] = (ushort)0;

        if(!LoadInventoryData())
            // 인벤토리를 테스트 데이터로 채우는 함수
            MakeItemTest();
        else
            LoadInventoryData();
    }

    /// <summary>
    /// Json 파일에서 데이터를 로드하고, 해당 데이터를 처리하기 위한 ILoader 인터페이스를 구현하는 타입의 인스턴스를 생성한다.
    /// </summary>
    /// <param name="path"></param>
    /// <typeparam name="TLoader"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    TLoader LoadJson<TLoader, TKey, TValue>(string path) where TLoader : ILoader<TKey, TValue>
    {
        TextAsset textAsset = Managers.Resource.Load<TextAsset>($"Data/{path}");
        return JsonUtility.FromJson<TLoader>(textAsset.text);
    }
    
    /// <summary>
    /// 인벤토리를 테스트 데이터로 채우는 함수
    /// </summary>
    public void MakeItemTest()
    {
        // 테스트용 더미 데이터 
        InventoryTable[0] = (ushort)0;
        InventoryTable[1] = (ushort)49279;            // 종결옵 Sword (퀄리티 최상, 풀내구도, 풀강)
        InventoryTable[2] = (ushort)160;              // 똥 활 (퀄리티 최하, 내구도 절반, 0강)
        InventoryTable[3] = (ushort)39169;            // 나무 재료 (중간 퀄리티, 2개)
        InventoryTable[4] = (ushort)39295;            // 돌 재료 (중간 퀄리티, 64개)

        // 나머지는 빈값으로 채우기
        for (int i = 5; i < 10; i++)
            InventoryTable[i] = (ushort)0;

        DebugEx.Log("makeItemTest ============== ");
        for (int i = 0; i < NumberOfInventorySlots; i++)
            DebugEx.Log(InventoryTable[i]);
        DebugEx.Log("makeItemTest ============== ");
    }

    /// <summary>
    /// 인벤토리 데이터를 바이너리 파일로 저장하는 함수
    /// </summary>
    public void SaveInventoryData()
    {
        const string dataFileName = "save.bin";
        FileStream fs = File.Open(_path + dataFileName, FileMode.Create);

        using (BinaryWriter wr = new BinaryWriter(fs))
        {
            foreach (ushort inventoryItem in InventoryTable)
            {
                wr.Write((ushort)inventoryItem);
            }
            DebugEx.Log("Inventory Save Success");
        }
    }

    /// <summary>
    /// 인벤토리 데이터를 바이너리 파일로 읽어오는 함수
    /// </summary>
    /// <returns>데이터가 존재한다면 true, 그렇지 않다면 false를 반환.</returns>
    public bool LoadInventoryData()
    {
        const string dataFileName = "save.bin";
        try
        {
            using (BinaryReader rdr = new BinaryReader(File.Open(_path + dataFileName, FileMode.Open)))
            {
                InventoryTable = new ushort[NumberOfInventorySlots];
                for (int i = 0; i < NumberOfInventorySlots; i++)
                {
                    if (rdr.BaseStream.Position < rdr.BaseStream.Length)
                    {
                        InventoryTable[i] = (ushort)rdr.ReadUInt16();
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        catch(Exception e)
        {
            DebugEx.LogWarning("There are no any save files.");
            DebugEx.Log($"Exception message : {e}");
            return false;
        }

        DebugEx.Log("LoadInventoryData ============== ");
        for (int i = 0; i < NumberOfInventorySlots; i++)
        {
            DebugEx.Log(InventoryTable[i]);
        }
        DebugEx.Log("LoadInventoryData ============== ");
    
        return true;
    }

    /// <summary>
    /// 아이템 정보를 인벤토리 팝업에서 읽어갈 수 있도록 변환하는 함수
    /// </summary>
    public List<ItemData> ItemInfos(bool printConsole = false)
    {
        List<ItemData> itemDatas = new List<ItemData>();
        
        if(printConsole)
            DebugEx.Log("############# ItemDescription ##############");
        for(int i = 0; i < NumberOfInventorySlots; i++)
        {
            // 1. 아이템 종류 구하기
            int id = ((int)InventoryTable[i] >> 6) & 255;      
            string name = itemCodeDict[id];
            
            // 2. 아이템 퀄리티 구하기
            int quality = (InventoryTable[i] >> 8);

            // 3. 아이템 내구도, 강화도 (갯수) 구하기
            int durability = (InventoryTable[i] & 63) >> 2;
            int numOfReinforce = InventoryTable[i] & 3;
            
            // amount = durability * 4 + numOfReinforce              // 뒤 6비트로 갯수를 파악 (숫자를 1~64로 받기위해 + 1)
            
            if (printConsole)
            {
                string data = $"{name} {durability * 4 + numOfReinforce} 개";
                DebugEx.Log(data);
            }
            
            // 지금 읽어온 아이템이 Tool인지, Material인지 구분해야한다.
            if (id >= BOUNDARY)
                itemDatas.Add(new Ingredient(id, name, quality, durability * 4 + numOfReinforce));
            else
                itemDatas.Add(new Tool(id, name, quality, durability, numOfReinforce));
            
        }
        if(printConsole)
            DebugEx.Log("############# ItemDescription ##############");

        return itemDatas;
    }

    // 리스트로 입력받은 인벤토리 데이터를 다시 배열로 바꾸는 함수
    public void TransDataListIntoArray(List<ItemData> inventory)
    {
        ushort[] _inventoryTable = new ushort[NumberOfInventorySlots];
        
        for (int i = 0; i < inventory.Count; i++)
        {
            // item 이름으로 코드 찾기
            int itemCode = reverseItemCodeDict[inventory[i].Name];
            
            // 갯수 조정 (1을 빼서 0~63 범위로 맞춤)
            //int adjustedItemCount = inventory[i].Count - 1; TODO
            //ushort item = (ushort)((itemCode << 6) | adjustedItemCount);

            //_inventoryTable[i] = item;
        }
        InventoryTable = _inventoryTable;
    }
}