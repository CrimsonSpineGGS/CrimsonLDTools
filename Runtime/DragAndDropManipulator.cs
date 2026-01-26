using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Samples.Editor.General
{
    public partial class DragAndDropWindow
    {
        private class DragAndDropManipulator : PointerManipulator
        {
            private readonly Label _dropLabel;
            private readonly DragAndDropWindow _windowParent;
            private string _assetPath = string.Empty;

            private Object _droppedObject;

            public DragAndDropManipulator(VisualElement root, DragAndDropWindow windowParent)
            {
                target = root.Q<VisualElement>(className: "drop-area");
                _dropLabel = root.Q<Label>(className: "drop-area__label");
                _windowParent = windowParent;
            }

            protected override void RegisterCallbacksOnTarget()
            {
                target.RegisterCallback<PointerDownEvent>(OnPointerDown);
                target.RegisterCallback<DragEnterEvent>(OnDragEnter);
                target.RegisterCallback<DragLeaveEvent>(OnDragLeave);
                target.RegisterCallback<DragUpdatedEvent>(OnDragUpdate);
                target.RegisterCallback<DragPerformEvent>(OnDragPerform);
            }

            protected override void UnregisterCallbacksFromTarget()
            {
                target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
                target.UnregisterCallback<DragEnterEvent>(OnDragEnter);
                target.UnregisterCallback<DragLeaveEvent>(OnDragLeave);
                target.UnregisterCallback<DragUpdatedEvent>(OnDragUpdate);
                target.UnregisterCallback<DragPerformEvent>(OnDragPerform);
            }

            private void OnPointerDown(PointerDownEvent _)
            {
                // Only do something if the window currently has a reference to an asset object.
                if (_droppedObject != null)
                {
                    // Clear existing data in DragAndDrop class.
                    DragAndDrop.PrepareStartDrag();

                    // Store reference to object and path to object in DragAndDrop static fields.
                    DragAndDrop.objectReferences = new[] { _droppedObject };
                    if (_assetPath != string.Empty)
                        DragAndDrop.paths = new[] { _assetPath };
                    else
                        DragAndDrop.paths = new string[] { };

                    // Start a drag.
                    DragAndDrop.StartDrag(string.Empty);
                }
            }

            // This method runs if a user brings the pointer over the target while a drag is in progress.
            private void OnDragEnter(DragEnterEvent _)
            {
                // Get the name of the object the user is dragging.
                var draggedName = string.Empty;
                if (DragAndDrop.paths.Length > 0)
                {
                    _assetPath = DragAndDrop.paths[0];
                    var splitPath = _assetPath.Split('/');
                    draggedName = splitPath[splitPath.Length - 1];
                }
                else if (DragAndDrop.objectReferences.Length > 0)
                {
                    draggedName = DragAndDrop.objectReferences[0].name;
                }

                // Change the appearance of the drop area if the user drags something over the drop area and holds it
                // there.
                _dropLabel.text = $"Dropping '{draggedName}'...";
                target.AddToClassList("drop-area--dropping");
            }

            // This method runs if a user makes the pointer leave the bounds of the target while a drag is in progress.
            private void OnDragLeave(DragLeaveEvent _)
            {
                _assetPath = string.Empty;
                _droppedObject = null;
                _dropLabel.text = "Drag an asset here...";
                target.RemoveFromClassList("drop-area--dropping");
            }

            // This method runs every frame while a drag is in progress.
            private void OnDragUpdate(DragUpdatedEvent _)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
            }

            // This method runs when a user drops a dragged object onto the target.
            private void OnDragPerform(DragPerformEvent _)
            {
                // Set droppedObject and draggedName fields to refer to dragged object.
                _droppedObject = DragAndDrop.objectReferences[0];

                _windowParent.PerformCreation(_droppedObject as GameObject);
                _dropLabel.text = "Drag an asset here...";
            }
        }

        private class SceneDropManipulator : PointerManipulator
        {
            private GameObject _draggedPrefab;

            public SceneDropManipulator(VisualElement root)
            {
                target = root;
                _draggedPrefab = null;
            }

            protected override void RegisterCallbacksOnTarget()
            {
                target.RegisterCallback<PointerDownEvent>(OnPointerDown);
                target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
                target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            }

            protected override void UnregisterCallbacksFromTarget()
            {
                target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
                target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
                target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            }

            private void OnPointerDown(PointerDownEvent evt)
            {
                if (evt.button != 0)
                    return;


                // Vérifie si le clic est sur un élément de prefab
                var clickedElement = evt.target as VisualElement;
                var prefab = FindPrefabFromElement(clickedElement);

                if (!prefab)
                    return;

                _draggedPrefab = prefab;
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new Object[] { _draggedPrefab };
                DragAndDrop.StartDrag(_draggedPrefab.name);
                evt.StopPropagation();
            }

            private void OnPointerMove(PointerMoveEvent evt)
            {
                if (_draggedPrefab != null && DragAndDrop.visualMode == DragAndDropVisualMode.None)
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            }

            private void OnPointerUp(PointerUpEvent evt)
            {
                _draggedPrefab = null;
            }

            // Trouve le prefab associé à un élément visuel
            private static GameObject FindPrefabFromElement(VisualElement element)
            {
                while (element != null && !(element.userData is GameObject)) element = element.parent;

                return element?.userData as GameObject;
            }
        }

        #region SceneView logic

        // Logique pour drang and drop dans la scene view
        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (DragAndDrop.paths.Length < 1 || DragAndDrop.objectReferences.Length < 1)
                return;

            var currentEvent = Event.current;

            switch (currentEvent.type)
            {
                case EventType.DragUpdated:
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    currentEvent.Use();
                    break;
                case EventType.DragPerform:
                    // Récupère le prefab depuis DragAndDrop
                    var draggedPrefab = DragAndDrop.objectReferences[0] as GameObject;
                    if (draggedPrefab != null)
                    {
                        // Raycast pour trouver la position dans la scène
                        var ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
                        if (Physics.Raycast(ray, out var hit))
                        {
                            // Instancie le prefab à la position du clic
                            var instance = PrefabUtility.InstantiatePrefab(draggedPrefab) as GameObject;
                            if (!instance)
                                return;
                            instance.transform.position = hit.point;
                        }
                        else
                        {
                            // Si aucun objet n'est touché, place le prefab devant la caméra
                            var instance = PrefabUtility.InstantiatePrefab(draggedPrefab) as GameObject;
                            if (!instance)
                                return;
                            instance.transform.position = ray.GetPoint(10);
                        }
                    }

                    DragAndDrop.AcceptDrag();
                    currentEvent.Use();
                    break;
            }
        }

        #endregion
    }
}