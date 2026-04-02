using UnityEngine;
using System.Collections.Generic;

public class StatManager : MonoBehaviour
{
    public static StatManager Instance;
    public class RowData
    {
        public int Level;
        public int EXP;
        public float HP;
        public float STR;
        public float DEF;
        public float MANA;
    }

    private Dictionary<int, RowData> statTable = new Dictionary<int, RowData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadCSV();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadCSV()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("PlayerStatus");
        if (csvFile == null)
        {
            Debug.LogError("Khong tim thay file PlayerStatus trong Resources!");
            return;
        }

        string[] lines = csvFile.text.Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] values = lines[i].Split(',');
            if (values.Length < 6) continue;

            RowData row = new RowData
            {
                Level = int.Parse(values[0]),
                EXP = int.Parse(values[1]),
                HP = float.Parse(values[2]),
                STR = float.Parse(values[3]),
                DEF = float.Parse(values[4]),
                MANA = float.Parse(values[5])
            };

            if (!statTable.ContainsKey(row.Level))
                statTable.Add(row.Level, row);
        }
        Debug.Log("Da load xong bảng chỉ số CSV.");
    }

    public RowData GetDataByLevel(int level)
    {
        if (statTable.Count == 0) LoadCSV();

        if (statTable.ContainsKey(level)) return statTable[level];

        Debug.LogError($"Không tìm thấy Level {level} trong bảng CSV!");
        return null;
    }
}