using Core.Heroes;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Editor
{
    [CustomPropertyDrawer(typeof(HeroUpgradeData))]
    public class HeroUpgradeDataDrawer : PropertyDrawer
    {
        // Кэш: имя -> ID
        private static Dictionary<string, string> cachedHeroNameToId = null;

        // Кэш: ID -> имя (новый, для label)
        private static Dictionary<string, string> cachedHeroIdToName = null;
        private static List<string> cachedHeroNames = null;
        private static bool isCacheDirty = true;

        // Состояние раскрытия для каждого свойства (по propertyPath)
        private static Dictionary<string, bool> expandedStates = new Dictionary<string, bool>();

        // Метод для обновления кэша
        private static void UpdateHeroCache()
        {
            if (!isCacheDirty) return;

            cachedHeroNameToId = new Dictionary<string, string>();
            cachedHeroIdToName = new Dictionary<string, string>();
            cachedHeroNames = new List<string> { "None" }; // "None" всегда первым

            string[] guids = AssetDatabase.FindAssets("t:HeroData");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                HeroData heroData = AssetDatabase.LoadAssetAtPath<HeroData>(path);
                if (heroData != null && !string.IsNullOrEmpty(heroData.heroId) && heroData.heroCardData != null &&
                    !string.IsNullOrEmpty(heroData.heroCardData.heroName))
                {
                    string heroName = heroData.heroCardData.heroName;
                    if (!cachedHeroNameToId.ContainsKey(heroName)) // Избегаем дубликатов по имени
                    {
                        string heroId = heroData.heroId;
                        cachedHeroNameToId[heroName] = heroId;
                        cachedHeroIdToName[heroId] = heroName;
                        cachedHeroNames.Add(heroName);
                    }
                }
            }

            // Сортируем имена (кроме "None" в начале)
            var sortedNames = cachedHeroNames.Skip(1).OrderBy(name => name).ToList();
            cachedHeroNames = new List<string> { "None" }.Concat(sortedNames).ToList();

            isCacheDirty = false;
        }

        // Слушатель изменений
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            isCacheDirty = true;
            AssemblyReloadEvents.afterAssemblyReload += () => isCacheDirty = true;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            UpdateHeroCache();

            EditorGUI.BeginProperty(position, label, property);

            // Получаем heroId для этого элемента
            SerializedProperty heroIdProp = property.FindPropertyRelative("heroId");
            string heroId = heroIdProp.stringValue;
            string currentHeroName = GetHeroNameFromId(heroId);

            // Получаем состояние раскрытия
            string key = property.propertyPath;
            if (!expandedStates.ContainsKey(key))
            {
                expandedStates[key] = false;
            }

            bool isExpanded = expandedStates[key];

            // Вычисляем текущий индекс для Popup (в заголовке)
            int currentIndex = GetCurrentIndex(currentHeroName, heroId);

            // Разделяем rect на foldout-иконку и Popup для имени
            float foldoutWidth =20f; // Примерно ширина стрелки
            Rect foldoutRect = new Rect(position.x, position.y, foldoutWidth, EditorGUIUtility.singleLineHeight);
            Rect namePopupRect = new Rect(position.x + foldoutWidth, position.y, position.width - foldoutWidth,
                EditorGUIUtility.singleLineHeight);

            // Рисуем foldout-стрелку (toggle)
            isExpanded = GUI.Toggle(foldoutRect, isExpanded, GUIContent.none, EditorStyles.foldout);
            expandedStates[key] = isExpanded;

            // Рисуем Popup для выбора имени в заголовке (обновляет heroId)
            List<string> displayNames = GetDisplayNames(currentHeroName);
            GUIContent[] options = displayNames.Select(name => new GUIContent(name)).ToArray();
            int newIndex = EditorGUI.Popup(namePopupRect, GUIContent.none, currentIndex, options);
            if (newIndex != currentIndex)
            {
                string selectedName = displayNames[newIndex];
                if (selectedName == "None")
                {
                    heroIdProp.stringValue = "";
                }
                else if (cachedHeroNameToId.ContainsKey(selectedName))
                {
                    heroIdProp.stringValue = cachedHeroNameToId[selectedName];
                }

                // Если "Unknown", оставляем как есть (не меняем)
                EditorUtility.SetDirty(property.serializedObject.targetObject); // Принудительно сохраняем изменения
            }

            if (isExpanded)
            {
                // Итерация по дочерним полям (heroId как Label, остальные стандартно)
                float yOffset = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                SerializedProperty child = property.Copy();
                SerializedProperty endProperty = property.GetEndProperty();

                // Первое поле (heroId) — отдельно как Label
                if (child.NextVisible(true))
                {
                    Rect childRect = new Rect(position.x, position.y + yOffset, position.width,
                        EditorGUI.GetPropertyHeight(child));
                    DrawChildProperty(childRect, child);
                    yOffset += EditorGUI.GetPropertyHeight(child) + EditorGUIUtility.standardVerticalSpacing;
                }

                // Остальные поля
                while (child.NextVisible(false) && !SerializedProperty.EqualContents(child, endProperty))
                {
                    Rect childRect = new Rect(position.x, position.y + yOffset, position.width,
                        EditorGUI.GetPropertyHeight(child));
                    DrawChildProperty(childRect, child);
                    yOffset += EditorGUI.GetPropertyHeight(child) + EditorGUIUtility.standardVerticalSpacing;
                }
            }

            EditorGUI.EndProperty();
        }

        // Вспомогательный метод: получить имя по ID
        private static string GetHeroNameFromId(string heroId)
        {
            if (string.IsNullOrEmpty(heroId)) return "None";
            return cachedHeroIdToName.TryGetValue(heroId, out string name) ? name : "Unknown Hero (ID: " + heroId + ")";
        }

        // Получить индекс текущего имени в displayNames
        private static int GetCurrentIndex(string currentHeroName, string heroId)
        {
            List<string> displayNames = GetDisplayNames(currentHeroName);
            int index = displayNames.IndexOf(currentHeroName);
            if (index == -1)
            {
                // Если "Unknown", вставляем и возвращаем его индекс
                displayNames.Insert(1, currentHeroName);
                return 1;
            }

            return index;
        }

        // Получить список для отображения (с возможным "Unknown" после "None")
        private static List<string> GetDisplayNames(string currentHeroName)
        {
            List<string> displayNames = new List<string>(cachedHeroNames);
            if (currentHeroName != "None" && !displayNames.Contains(currentHeroName))
            {
                displayNames.Insert(1, currentHeroName); // После "None"
            }

            return displayNames;
        }

        // Рисование дочернего поля (Label для heroId, стандарт для остальных)
        private void DrawChildProperty(Rect rect, SerializedProperty child)
        {
            if (child.name == "heroId")
            {
                // Readonly Label для ID
                EditorGUI.LabelField(rect, "Hero ID", child.stringValue);
            }
            else
            {
                // Стандартное рисование для других полей
                EditorGUI.PropertyField(rect, child, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            string key = property.propertyPath;
            if (!expandedStates.ContainsKey(key))
            {
                expandedStates[key] = false;
            }

            bool isExpanded = expandedStates[key];

            if (!isExpanded)
            {
                // Только заголовок (foldout + popup, высота одной строки)
                return EditorGUIUtility.singleLineHeight;
            }

            // Суммируем высоты: заголовок + spacing + все дочерние (включая heroId как одну строку)
            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            SerializedProperty child = property.Copy();
            SerializedProperty endProperty = property.GetEndProperty();

            if (child.NextVisible(true))
            {
                height += EditorGUI.GetPropertyHeight(child) +
                          EditorGUIUtility.standardVerticalSpacing; // heroId — single line
            }

            while (child.NextVisible(false) && !SerializedProperty.EqualContents(child, endProperty))
            {
                height += EditorGUI.GetPropertyHeight(child) + EditorGUIUtility.standardVerticalSpacing;
            }

            return height;
        }
    }
}