﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if WINDOWS_UWP
using System.Reflection;
#endif

namespace Arthas.UI
{
    public struct WindowInfo : IComparable<WindowInfo>
    {
        public int? Order { get; set; }
        public bool IsHeader { get; set; }
        public bool IsExclusive { get; set; }
        public BaseUI UI { get; set; }

        public void SetOrder(int headerCount, int amountCount, int index)
        {
            var i = (IsHeader ? amountCount : amountCount - headerCount) - index - 1;
            UI.transform.SetSiblingIndex(i);
            UI.gameObject.SetActive(true);
        }

        public bool ContainBrother(BaseUI ui)
        {
            return UI && UI.BrotherWindows.Contains(ui);
        }

        public int CompareTo(WindowInfo other)
        {
            return other.Order.Value.CompareTo(Order.Value);
        }
    }

    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public class UIManager : SingletonBehaviour<UIManager>
    {
        private static readonly Dictionary<BaseUI, WindowInfo> windows = new Dictionary<BaseUI, WindowInfo>();
        private static readonly List<WindowInfo> showedWindows = new List<WindowInfo>();
        private static readonly List<WindowInfo> showedHeaderWindows = new List<WindowInfo>();

        public static Canvas Canvas { get; private set; }

        [SerializeField]
        private BaseUI startUI;

        protected override void Awake()
        {
            base.Awake();
            Canvas = GetComponent<Canvas>();
            var uis = GetComponentsInChildren<BaseUI>(true);
            for (var i = 0; i < uis.Length; i++)
            {
                AddUI(uis[i]);
                if (uis[i].isActiveAndEnabled)
                {
                    var window = CreateWindowInfo(uis[i]);
                    var showed = window.IsHeader ? showedHeaderWindows : showedWindows;
                    showed.Add(window);
                }
            }
        }

        protected void Start()
        {
            if (!startUI)
            {
#if UNITY_EDITOR
                UnityEditor.Selection.activeGameObject = gameObject;
#endif
                Debug.LogError(@"UISystem initialize fail , Cannot found start ui which has a <color=cyan>[UIStart]</color> Attribute and inherit <color=cyan>:WindowUI<T></color>");
                Debug.DebugBreak();
                return;
            }
            startUI.Show();
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                var comp = child.GetComponent<BaseUI>();
                if (!comp)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 上一个显示的窗口
        /// </summary>
        public static WindowInfo CurrentWindow { get; private set; }

        public static WindowInfo PrevWindow { get; private set; }

        /// <summary>
        /// 添加UI
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ui"></param>
        public static void AddUI(BaseUI ui)
        {
            if (!windows.ContainsKey(ui))
            {
                var window = CreateWindowInfo(ui);
                windows.Add(ui, window);
                ui.UIShowEvent += OnShow;
                ui.UIHideEvent += OnHide;
            }
        }

        private static WindowInfo CreateWindowInfo(BaseUI ui)
        {
            var uiType = ui.GetType();
#if WINDOWS_UWP
            var header = uiType.GetTypeInfo().IsDefined(typeof(UIHeaderAttribute));
            var exclusive = uiType.GetTypeInfo().IsDefined(typeof(UIExclusiveAttribute), false);
            var order = uiType.GetTypeInfo().GetCustomAttributes(typeof(UIOrderAttribute), false);
#else
            var header = uiType.IsDefined(typeof(UIHeaderAttribute), false);
            var exclusive = uiType.IsDefined(typeof(UIExclusiveAttribute), false);
            var order = uiType.GetCustomAttributes(typeof(UIOrderAttribute), false);
#endif
            var uiOrder = order.Length > 0 ? order[0] as UIOrderAttribute : new UIOrderAttribute(ui.SortOrder);

            var window = new WindowInfo()
            {
                IsHeader = header,
                IsExclusive = exclusive,
                Order = uiOrder.SortOrder,
                UI = ui
            };
            return window;
        }

        /// <summary>
        /// 当UI显示
        /// </summary>
        /// <param name="name"></param>
        private static void OnShow(BaseUI ui)
        {
            if (windows.ContainsKey(ui))
            {
                var window = windows[ui];
                var windowList = window.IsHeader ? showedHeaderWindows : showedWindows;
                if (window.IsExclusive)
                {
                    var array = windowList.ToArray();
                    foreach (var item in array)
                    {
                        if (window.ContainBrother(item.UI) || item.UI.Equals(ui)) continue;
                        item.UI.Hide();
                    }
                }
                PrevWindow = CurrentWindow;
                CurrentWindow = window;
                var sortWindows = new List<WindowInfo>(new WindowInfo[] { CurrentWindow });
                var brothers = CurrentWindow.UI.BrotherWindows;
                for (var i = 0; i < brothers.Count; i++)
                {
                    if (windows.ContainsKey(brothers[i]))
                    {
                        var brotherWindow = windows[brothers[i]];
                        sortWindows.Add(brotherWindow);
                    }
                }
                sortWindows.Sort();
                for (var i = 0; i < sortWindows.Count; i++)
                {
                    var sortWindow = sortWindows[i];
                    if (!windowList.Contains(sortWindow))
                        windowList.Add(sortWindow);
                    sortWindow.SetOrder(showedHeaderWindows.Count, Instance.transform.childCount, i);
                }
            }
        }

        /// <summary>
        /// 当UI隐藏
        /// </summary>
        /// <param name="name"></param>
        private static void OnHide(BaseUI ui)
        {
            if (windows.ContainsKey(ui))
            {
                var window = windows[ui];
                if (window.IsHeader)
                    showedWindows.Remove(window);
                else
                    showedHeaderWindows.Remove(window);
            }
        }
    }
}