using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

// Déclaration de l'overlay sur la SceneView avec un nom
[Overlay(typeof(SceneView), "Place Actor Bar")]
public class PlaceActorBar : ToolbarOverlay
{
    private static float _placeDistance = 10f;

    // Permet d'ajouter les classes dans la toolbar
    private PlaceActorBar() : base(AddCube.Id, AddSphere.Id, AddCapsule.Id, AddCylinder.Id, AddPlane.Id,
        AddQuad.Id, PlaceDistance.Id)
    {
    }

    private static void AddPrimitive(PrimitiveType primitiveType)
    {
        var sceneView = SceneView.lastActiveSceneView;
        if (sceneView == null)
        {
            Debug.LogError("Aucune vue de scène active trouvée.");
            return;
        }

        // Calcule le centre de la vue
        var centerPoint = new Vector2(
            sceneView.position.width / 2f,
            sceneView.position.height / 2f
        );

        // Crée un rayon depuis le centre de la vue
        var ray = HandleUtility.GUIPointToWorldRay(centerPoint);

        var go = GameObject.CreatePrimitive(primitiveType);

        // Effectue le raycast
        if (Physics.Raycast(ray, out var hit))
            go.transform.position = hit.point;
        else
            // Si le rayon ne touche rien, place l'objet à une distance devant la caméra
            go.transform.position = ray.GetPoint(_placeDistance);

        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }

    // Méthode pour obtenir le chemin du package

    [EditorToolbarElement(Id, typeof(SceneView))]
    private class AddCube : EditorToolbarButton
    {
        public const string Id = "Place Actor Bar/AddCube";

        public AddCube()
        {
            text = "Add Cube";
            tooltip = "Create a cube in the scene";
            icon = EditorGUIUtility.Load("Packages/com.crimsonteam.crimsontool/Icons/cube.png") as Texture2D;
            clicked += () => AddPrimitive(PrimitiveType.Cube);
        }
    }

    [EditorToolbarElement(Id, typeof(SceneView))]
    private class AddSphere : EditorToolbarButton
    {
        public const string Id = "Place Actor Bar/AddSphere";

        public AddSphere()
        {
            text = "Add Sphere";
            tooltip = "Create a sphere in the scene";
            icon = EditorGUIUtility.Load("Packages/com.crimsonteam.crimsontool/Icons/sphere.png") as Texture2D;
            clicked += () => AddPrimitive(PrimitiveType.Sphere);
        }
    }

    [EditorToolbarElement(Id, typeof(SceneView))]
    private class AddCapsule : EditorToolbarButton
    {
        public const string Id = "Place Actor Bar/AddCapsule";

        public AddCapsule()
        {
            text = "Add Capsule";
            tooltip = "Create a capsule in the scene";
            icon = EditorGUIUtility.Load("Packages/com.crimsonteam.crimsontool/Icons/capsule.png") as Texture2D;
            clicked += () => AddPrimitive(PrimitiveType.Capsule);
        }
    }

    [EditorToolbarElement(Id, typeof(SceneView))]
    private class AddCylinder : EditorToolbarButton
    {
        public const string Id = "Place Actor Bar/AddCylinder";

        public AddCylinder()
        {
            text = "Add Cylinder";
            tooltip = "Create a cylinder in the scene";
            icon = EditorGUIUtility.Load("Packages/com.crimsonteam.crimsontool/Icons/cylinder.png") as Texture2D;
            clicked += () => AddPrimitive(PrimitiveType.Cylinder);
        }
    }

    [EditorToolbarElement(Id, typeof(SceneView))]
    private class AddPlane : EditorToolbarButton
    {
        public const string Id = "Place Actor Bar/AddPlane";

        public AddPlane()
        {
            text = "Add Plane";
            tooltip = "Create a plane in the scene";
            icon = EditorGUIUtility.Load("Packages/com.crimsonteam.crimsontool/Icons/plane.png") as Texture2D;
            clicked += () => AddPrimitive(PrimitiveType.Plane);
        }
    }

    [EditorToolbarElement(Id, typeof(SceneView))]
    private class AddQuad : EditorToolbarButton
    {
        public const string Id = "Place Actor Bar/AddQuad";

        public AddQuad()
        {
            text = "Add Quad";
            tooltip = "Create a quad in the scene";
            icon = EditorGUIUtility.Load("Packages/com.crimsonteam.crimsontool/Icons/quad.png") as Texture2D;
            clicked += () => AddPrimitive(PrimitiveType.Quad);
        }
    }

    [EditorToolbarElement(Id, typeof(SceneView))]
    private class PlaceDistance : EditorToolbarFloatField
    {
        public const string Id = "Place Actor Bar/PlaceDistance";

        public PlaceDistance()
        {
            this.RegisterValueChangedCallback(evt => { _placeDistance = evt.newValue; });
            value = _placeDistance;
            tooltip = "Modify the distance where the actor is placed";
        }
    }
}