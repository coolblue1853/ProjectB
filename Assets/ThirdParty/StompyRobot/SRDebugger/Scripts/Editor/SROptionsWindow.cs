namespace SRDebugger.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SRF;
    using UnityEngine;
    using UnityEditor;
    using System.ComponentModel;
    using SRF.Helpers;
#if !DISABLE_SRDEBUGGER
    using Internal;
    using Services;
    using UI.Controls.Data;
#endif

    public class SROptionsWindow : EditorWindow
    {
        [MenuItem(SRDebugEditorPaths.SROptionsMenuItemPath, false, 8000)]
        public static void Open()
        {
            var window = GetWindow<SROptionsWindow>(false, "SROptions", true);
            window.minSize = new Vector2(100, 100);
            window.Show();
        }

#if DISABLE_SRDEBUGGER
        private bool _isWorking;

        void OnGUI()
        {
            SRDebugEditor.DrawDisabledWindowGui(ref _isWorking);
        }
#else
        [Serializable]
        private class CategoryState
        {
            public string Name;
            public bool IsOpen;
        }

        [SerializeField]
        private List<CategoryState> _categoryStates = new();

        private Dictionary<Type, Action<OptionDefinition>> _typeLookup;
        private Dictionary<string, List<OptionDefinition>> _options;

        private Vector2 _scrollPosition;
        private bool _queueRefresh;
        private bool _isDirty;

        [NonSerialized]
        private GUIStyle _divider;

        [NonSerialized]
        private GUIStyle _foldout;

        private IOptionsService _activeOptionsService;

        public void OnInspectorUpdate()
        {
            if (EditorApplication.isPlaying && (_options == null || _isDirty))
            {
                Populate();
                _queueRefresh = true;
                _isDirty = false;
            }
            else if (!EditorApplication.isPlaying && _options != null)
            {
                Clear();
                _queueRefresh = true;
            }

            if (_queueRefresh)
            {
                Repaint();
            }

            _queueRefresh = false;
        }

        private void OnDisable()
        {
            Clear();
        }

        private void PopulateTypeLookup()
        {
            _typeLookup = new Dictionary<Type, Action<OptionDefinition>>()
            {
                {typeof(int), OnGUI_Int},
                {typeof(float), OnGUI_Float},
                {typeof(double), OnGUI_Double},
                {typeof(string), OnGUI_String},
                {typeof(bool), OnGUI_Boolean},
                {typeof(uint), OnGUI_AnyInteger},
                {typeof(ushort), OnGUI_AnyInteger},
                {typeof(short), OnGUI_AnyInteger},
                {typeof(sbyte), OnGUI_AnyInteger},
                {typeof(byte), OnGUI_AnyInteger},
                {typeof(long), OnGUI_AnyInteger},
            };
        }

        private void Clear()
        {
            _options = null;
            _isDirty = false;

            if (_activeOptionsService != null)
            {
                _activeOptionsService.OptionsUpdated -= OnOptionsUpdated;
            }

            _activeOptionsService = null;
        }

        private void Populate()
        {
            if (_typeLookup == null)
            {
                PopulateTypeLookup();
            }

            if (_activeOptionsService != null)
            {
                _activeOptionsService.OptionsUpdated -= OnOptionsUpdated;
            }

            if (_options != null)
            {
                foreach (KeyValuePair<string, List<OptionDefinition>> kv in _options)
                {
                    foreach (OptionDefinition option in kv.Value)
                    {
                        if (option.IsProperty)
                        {
                            option.Property.ValueChanged -= OnOptionPropertyValueChanged;
                        }
                    }
                }
            }

            _options = new Dictionary<string, List<OptionDefinition>>();

            foreach (OptionDefinition option in Service.Options.Options)
            {
                List<OptionDefinition> list;

                if (!_options.TryGetValue(option.Category, out list))
                {
                    list = new List<OptionDefinition>();
                    _options[option.Category] = list;
                }

                list.Add(option);

                if (option.IsProperty)
                {
                    option.Property.ValueChanged += OnOptionPropertyValueChanged;
                }
            }

            foreach (KeyValuePair<string, List<OptionDefinition>> kv in _options)
            {
                kv.Value.Sort((d1, d2) => d1.SortPriority.CompareTo(d2.SortPriority));
            }

            _activeOptionsService = Service.Options;
            _activeOptionsService.OptionsUpdated += OnOptionsUpdated;
        }

        private void OnOptionPropertyValueChanged(PropertyReference property)
        {
            _queueRefresh = true;
        }

        private void OnOptionsUpdated(object sender, EventArgs e)
        {
            _isDirty = true;
            _queueRefresh = true;
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();

            if (!EditorApplication.isPlayingOrWillChangePlaymode || _options == null)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("SROptions can only be edited in play-mode.");
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                return;
            }

            if (_divider == null)
            {
                _divider = new GUIStyle(GUI.skin.box);
                _divider.stretchWidth = true;
                _divider.fixedHeight = 2;
            }

            if (_foldout == null)
            {
                _foldout = new GUIStyle(EditorStyles.foldout);
                _foldout.fontStyle = FontStyle.Bold;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            foreach (KeyValuePair<string, List<OptionDefinition>> kv in _options)
            {
                CategoryState state = _categoryStates.FirstOrDefault(p => p.Name == kv.Key);

                if (state == null)
                {
                    state = new CategoryState()
                    {
                        Name = kv.Key,
                        IsOpen = true,
                    };
                    _categoryStates.Add(state);
                }

                state.IsOpen = EditorGUILayout.Foldout(state.IsOpen, kv.Key, _foldout);

                if (!state.IsOpen)
                {
                    continue;
                }

                EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
                OnGUI_Category(kv.Value);
                EditorGUILayout.Space();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private void OnGUI_Category(List<OptionDefinition> options)
        {
            for (var i = 0; i < options.Count; i++)
            {
                OptionDefinition op = options[i];

                if (op.Property != null)
                {
                    OnGUI_Property(op);
                }
                else if (op.Method != null)
                {
                    OnGUI_Method(op);
                }
            }
        }

        private void OnGUI_Method(OptionDefinition op)
        {
            if (GUILayout.Button(op.Name))
            {
                op.Method.Invoke(null);
            }
        }

        private void OnGUI_Property(OptionDefinition op)
        {
            Action<OptionDefinition> method;

            if (op.Property.PropertyType.IsEnum)
            {
                method = OnGUI_Enum;
            }
            else if (!_typeLookup.TryGetValue(op.Property.PropertyType, out method))
            {
                OnGUI_Unsupported(op);
                return;
            }

            if (!op.Property.CanWrite)
            {
                EditorGUI.BeginDisabledGroup(true);
            }

            method(op);

            if (!op.Property.CanWrite)
            {
                EditorGUI.EndDisabledGroup();
            }
        }

        private void OnGUI_String(OptionDefinition op)
        {
            EditorGUI.BeginChangeCheck();
            string newValue = EditorGUILayout.TextField(op.Name, (string) op.Property.GetValue());

            if (EditorGUI.EndChangeCheck())
            {
                op.Property.SetValue(Convert.ChangeType(newValue, op.Property.PropertyType));
            }
        }

        private void OnGUI_Boolean(OptionDefinition op)
        {
            EditorGUI.BeginChangeCheck();
            bool newValue = EditorGUILayout.Toggle(op.Name, (bool) op.Property.GetValue());

            if (EditorGUI.EndChangeCheck())
            {
                op.Property.SetValue(Convert.ChangeType(newValue, op.Property.PropertyType));
            }
        }

        private void OnGUI_Enum(OptionDefinition op)
        {
            EditorGUI.BeginChangeCheck();
            Enum newValue = EditorGUILayout.EnumPopup(op.Name, (Enum) op.Property.GetValue());

            if (EditorGUI.EndChangeCheck())
            {
                op.Property.SetValue(Convert.ChangeType(newValue, op.Property.PropertyType));
            }
        }

        private void OnGUI_Int(OptionDefinition op)
        {
            var range = op.Property.GetAttribute<NumberRangeAttribute>();

            int newValue;

            EditorGUI.BeginChangeCheck();

            if (range != null)
            {
                newValue = EditorGUILayout.IntSlider(op.Name, (int) op.Property.GetValue(), (int) range.Min,
                    (int) range.Max);
            }
            else
            {
                newValue = EditorGUILayout.IntField(op.Name, (int) op.Property.GetValue());
            }

            if (EditorGUI.EndChangeCheck())
            {
                op.Property.SetValue(Convert.ChangeType(newValue, op.Property.PropertyType));
            }
        }

        private void OnGUI_Float(OptionDefinition op)
        {
            var range = op.Property.GetAttribute<NumberRangeAttribute>();

            float newValue;

            EditorGUI.BeginChangeCheck();

            if (range != null)
            {
                newValue = EditorGUILayout.Slider(op.Name, (float) op.Property.GetValue(), (float) range.Min,
                    (float) range.Max);
            }
            else
            {
                newValue = EditorGUILayout.FloatField(op.Name, (float) op.Property.GetValue());
            }

            if (EditorGUI.EndChangeCheck())
            {
                op.Property.SetValue(Convert.ChangeType(newValue, op.Property.PropertyType));
            }
        }

        private void OnGUI_Double(OptionDefinition op)
        {
            var range = op.Property.GetAttribute<NumberRangeAttribute>();

            double newValue;

            EditorGUI.BeginChangeCheck();

            if (range != null && range.Min > float.MinValue && range.Max < float.MaxValue)
            {
                newValue = EditorGUILayout.Slider(op.Name, (float) op.Property.GetValue(), (float) range.Min,
                    (float) range.Max);
            }
            else
            {
                newValue = EditorGUILayout.DoubleField(op.Name, (double) op.Property.GetValue());

                if (range != null)
                {
                    if (newValue > range.Max)
                    {
                        newValue = range.Max;
                    }
                    else if (newValue < range.Min)
                    {
                        newValue = range.Min;
                    }
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                op.Property.SetValue(Convert.ChangeType(newValue, op.Property.PropertyType));
            }
        }

        private void OnGUI_AnyInteger(OptionDefinition op)
        {
            NumberControl.ValueRange range;

            if (!NumberControl.ValueRanges.TryGetValue(op.Property.PropertyType, out range))
            {
                Debug.LogError("Unknown integer type: " + op.Property.PropertyType);
                return;
            }

            var userRange = op.Property.GetAttribute<NumberRangeAttribute>();

            EditorGUI.BeginChangeCheck();

            var oldValue = (long) Convert.ChangeType(op.Property.GetValue(), typeof(long));
            long newValue = EditorGUILayout.LongField(op.Name, oldValue);

            if (newValue > range.MaxValue)
            {
                newValue = (long) range.MaxValue;
            }
            else if (newValue < range.MinValue)
            {
                newValue = (long) range.MinValue;
            }

            if (userRange != null)
            {
                if (newValue > userRange.Max)
                {
                    newValue = (long) userRange.Max;
                }
                else if (newValue < userRange.Min)
                {
                    newValue = (long) userRange.Min;
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                op.Property.SetValue(Convert.ChangeType(newValue, op.Property.PropertyType));
            }
        }

        private void OnGUI_Unsupported(OptionDefinition op)
        {
            EditorGUILayout.PrefixLabel(op.Name);
            EditorGUILayout.LabelField("Unsupported Type: {0}".Fmt(op.Property.PropertyType));
        }
#endif
    }
}
