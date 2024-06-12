using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_SeparateIngredientPopup : UI_Popup
{
    enum Images
    {
        Back,
    }

    enum InputFields
    {
        InputField,
    }

    enum Buttons
    {
        Increase,
        Decrease,
        Confirm,
        Cancel,
    }

    private TMP_InputField _inputField;
    
    [SerializeField] private UI_Inven_Item _originItem;

    private ICatcher _catcher;

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<TMP_InputField>(typeof(InputFields));
        Bind<Button>(typeof(Buttons));
        
        Get<Image>((int)Images.Back).gameObject.AddUIEvent(ClosePopupUI);
        Get<Button>((int)Buttons.Increase).onClick.AddListener(Increase);
        Get<Button>((int)Buttons.Decrease).onClick.AddListener(Decrease);
        Get<Button>((int)Buttons.Confirm).onClick.AddListener(Confirm);
        Get<Button>((int)Buttons.Cancel).onClick.AddListener(Deny);

        _inputField = Get<TMP_InputField>((int)InputFields.InputField);
        _inputField.onValueChanged.AddListener(CheckValue);
    }
    
    public void InitItemReference(UI_Inven_Item originItem)
    {
        _originItem = originItem;

        // _catcher를 초기화
        var parentPanel = _originItem.parentPanel.transform.parent;
        if (parentPanel == null)
        {
            parentPanel = _originItem.parentPanel.transform;
        }
        _catcher = parentPanel.GetComponent<ICatcher>();   
    }

    /// <summary>
    /// 값이 0이면 Confirm 버튼 비활성화
    /// </summary>
    /// <param name="str"></param>
    void CheckValue(string str)
    {
        Get<Button>((int)Buttons.Confirm).interactable = int.Parse(str) != 0;
    }
    
    void Increase()
    {
        var current = int.Parse(_inputField.text);
        if (current == (_originItem.Amount - 1)) return;
        _inputField.text = $"{current + 1}";
    }
    
    void Decrease()
    {
        var current = int.Parse(_inputField.text);
        if (current == 0) return;
        _inputField.text = $"{current - 1}";
    }

    void Confirm()
    {
        DebugEx.Log("구매!");
    }

    void Deny()
    {
        ClosePopupUI(null);
    }
}
