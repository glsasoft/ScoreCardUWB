using System;
using LetsPiu.Component;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.UI.UIScript.SettingScripts.UI
{
    public class SettingPanelUI : MonoBehaviour
    {
        
        
        public event Action<string, int> TabSelectedEvent;
        
        [SerializeField] private SingleScrollView tabScrollView;
        public Button btnLogout;
        public Button btnExit;

        private string[] tabNames = new string[]
        {
            "基础设置",
            "声音设置",
        };

        private int selectedTabIndex = 0;

        public RectTransform tabContainer;
        
        private void Start()
        {
            tabScrollView.onCellUpdate = OnListItemUpdate;
            tabScrollView.Refresh(tabNames.Length, true);
        }
        
        private void OnListItemUpdate(Transform root, int index)
        {
            var data = tabNames[index];
            var label = root.Find<TMP_Text>("Label");
            var icon = root.Find<Image>("Icon");
            var button = root.GetComponent<Button>();
            
            label.text = data;
            
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                if(selectedTabIndex == index) return;
                selectedTabIndex = index;
                tabScrollView.Refresh(tabNames.Length, false);
                TabSelectedEvent?.Invoke(data, index);
            });
            
            if (selectedTabIndex == index)
            {
                //已选中
                icon.gameObject.SetActive(true);
                label.color = Color.white;
            }
            else
            {
                //未选中
                icon.gameObject.SetActive(false);
                label.color = new Color(1f, 1f, 1f, 0.2f);
            }
        }
    }
}
