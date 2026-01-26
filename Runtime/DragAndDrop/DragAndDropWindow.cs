using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Samples.Editor.General
{
    public partial class DragAndDropWindow : EditorWindow
    {
        private VisualElement _actorArea;
        private DragAndDropManipulator _manipulator;
        private VisualElement _root;
        private PlaceActorSettings _settings;
        private string _settingsPath;
        private StyleSheet _styleSheet;
        private VisualTreeAsset _visualTree;


        [MenuItem("Crimson Tool/Place Actor")]
        public static void ShowExample()
        {
            var wnd = GetWindow<DragAndDropWindow>();
            wnd.titleContent = new GUIContent("Place Actor");
        }

        // Permet d'ajouter le prefab dans l'UI 
        private void DrawPrefabs()
        {
            if (_settings == null)
                return;

            _actorArea.Clear();

            foreach (var prefab in _settings.prefabs)
            {
                if (prefab == null)
                    continue;

                var element = CreatePrefabElement(prefab);
                _actorArea.Add(element);
            }
        }


        // Settings contenant la liste des prefabs 
        [Serializable]
        public class PlaceActorSettings
        {
            public List<GameObject> prefabs = new();
        }

        #region CRUD logic

        // Permet d'ajouter un prefab dans le actor area et d'actualiser le tout
        private void PerformCreation(GameObject prefab)
        {
            _settings.prefabs.Add(prefab);
            SaveSettings();
            DrawPrefabs();
        }


        private VisualElement CreatePrefabElement(GameObject prefab)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Column;
            container.style.alignItems = Align.Center;
            container.style.marginBottom = 8;

            container.userData = prefab;

            // Logique de drag and drop & de context menu
            new SceneDropManipulator(container);
            AddContextMenuToPrefab(container, prefab);


            var icon = AssetPreview.GetAssetPreview(prefab);
            if (icon == null)
                icon = AssetPreview.GetMiniThumbnail(prefab);

            var image = new VisualElement();
            image.style.width = 64;
            image.style.height = 64;
            image.style.backgroundImage = icon;
            image.style.marginBottom = 4;

            var label = new Label(prefab.name);

            container.Add(image);
            container.Add(label);

            return container;
        }


        // Permet d'ajouter un context menu avec des actions custom au prefab. 
        private void AddContextMenuToPrefab(VisualElement container, GameObject prefab)
        {
            container.AddManipulator(new ContextualMenuManipulator(evt =>
                {
                    // Supprimer le prefab
                    evt.menu.AppendAction(
                        "Delete",
                        a => DeletePrefab(prefab),
                        action => DropdownMenuAction.Status.Normal
                    );

                    // Permet d'aller à la source dans l'explorer
                    evt.menu.AppendAction(
                        "Show in explorer",
                        a => EditorGUIUtility.PingObject(prefab),
                        action => DropdownMenuAction.Status.Normal
                    );
                }
            ));
        }

        private void DeletePrefab(GameObject prefab)
        {
            _settings.prefabs.Remove(prefab);
            SaveSettings();
            DrawPrefabs();
        }

        #endregion

        #region LifeCycle

        private void Awake()
        {
            _settingsPath = Path.Combine(
                Application.dataPath,
                "Editor",
                "CrimsonToolPlaceActor.json"
            );

            LoadSettingsDataOnly();
            // test
        }

        private void OnDisable()
        {
            _manipulator.target.RemoveManipulator(_manipulator);
        }

        private void CreateGUI()
        {
            _root = rootVisualElement;

            // Charge le VisualTreeAsset
            _visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "./DragAndDropWindow.uxml"
            );

            // Charge le StyleSheet
            _styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "./DragAndDropWindow.uss"
            );

            // Ajoute le VisualTreeAsset à la racine
            if (_visualTree != null)
                _visualTree.CloneTree(_root);

            // Ajoute le StyleSheet
            if (_styleSheet != null)
                _root.styleSheets.Add(_styleSheet);

            _actorArea = _root.Q<VisualElement>(className: "actor-area");

            DrawPrefabs();

            _manipulator = new DragAndDropManipulator(rootVisualElement, this);
        }

        #endregion

        #region Save Settings

        // Permet de save les settings dans un json 
        private void SaveSettings()
        {
            try
            {
                var json = JsonUtility.ToJson(_settings, true);
                File.WriteAllText(_settingsPath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Erreur sauvegarde settings : {e.Message}");
            }
        }

        private void CreateDefaultSettings()
        {
            _settings = new PlaceActorSettings();
        }

        // Permet de load les data save du json. 
        private void LoadSettingsDataOnly()
        {
            if (!File.Exists(_settingsPath))
            {
                CreateDefaultSettings();
                SaveSettings();
                return;
            }

            try
            {
                var json = File.ReadAllText(_settingsPath);
                _settings = JsonUtility.FromJson<PlaceActorSettings>(json);

                if (_settings == null)
                    CreateDefaultSettings();
            }
            catch
            {
                CreateDefaultSettings();
            }
        }

        #endregion
    }
}