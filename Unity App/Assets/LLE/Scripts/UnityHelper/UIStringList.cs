using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class UIStringList : MonoBehaviour
{

    public GameObject ItemPrefab;
    public GameObject Container;
    public Color selectionColor = Color.green;

    public UnityEvent<string> OnItemSelected = new UnityEvent<string>();
    public string selectedItem = "";

    private List<string> stringItems = new List<string>();
    private bool isDataUpdate;
    private List<Button> itemBtn = new List<Button>();

    void Start()
    {

    }

    
    void Update()
    {
        if (isDataUpdate)
        {
            foreach (Transform t in Container.transform)
            {
                Destroy(t.gameObject);
            }
            foreach (string strItem in stringItems)
            {
                GameObject item = Instantiate(ItemPrefab);
                item.transform.SetParent(Container.transform, false);
                item.GetComponentInChildren<Text>().text = strItem;
                var btn = item.GetComponentInChildren<Button>();
                btn.onClick.AddListener(() => SetSelected(btn, strItem));
                itemBtn.Add(btn);
            }
            selectedItem = "";
            isDataUpdate = false;
        }
    }

    public List<string> getItems()
    {
        return stringItems;
    }

    internal void Remove(string anchorId)
    {
        if (stringItems.Remove(anchorId))
            isDataUpdate = true;
    }

    internal void RemoveAll()
    {
        stringItems.Clear();
        isDataUpdate = true;
    }
    private ColorBlock org;
    private Button selectedBtn;
    private void SetSelected(Button btn, string strItem)
    {
        if (selectedBtn != null)
        {
            selectedBtn.colors = org;

        }
        selectedBtn = btn;
        org = btn.colors;

        ColorBlock cb = btn.colors;
        cb.normalColor = selectionColor;
        cb.selectedColor = selectionColor;
        btn.colors = cb;

        OnItemSelected.Invoke(strItem);
        selectedItem = strItem;
        Debug.Log("UIStringList.OnItemSelected: " + strItem);
    }
    public void Change(string strItem, bool isAdd)
    {
        if (isAdd)
            Add(strItem);
        else
            Remove(strItem);
    }
    public void Add(string strItem)
    {
        stringItems.Add(strItem);
        isDataUpdate = true;
    }
    public void Add(ICollection<string> strItems)
    {
        foreach (var item in strItems)
            stringItems.Add(item);

        isDataUpdate = true;
    }
}
