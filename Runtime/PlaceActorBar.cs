using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

// Déclaration de l'overlay sur la SceneViex avec un nom
[Overlay(typeof(SceneView), "Place Actor Bar")]

// La class dérive de ToolBarOverlay forcément
public class PlaceActorBar : ToolbarOverlay
{
    private static float _placeDistance = 10f;

    // Permet d'ajouter les class dans la toolbar. Il doit contenir les ID des éléments et non pas les éléments eux mêmes. 
    private PlaceActorBar() : base(AddCube.Id, AddSphere.Id, AddCapsule.Id, AddCylinder.Id, AddPlane.Id,
        PlaceDistance.Id)
    {
    }

    // Prend en paramètre le type de primitive souhaité, ensuite crée l'objet, register l'action dans la classe UNDO puis met la selection sur l'objet crée. 
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

    // Permet de définir que c'est un élément du toolbar, contient son ID ainsi que la target
    [EditorToolbarElement(Id, typeof(SceneView))]

    // Class privé qui dérive de Editorhyjuku , ,;lo!ù4420 d'après Andrea allez tous vous faire enculer ❤️. 
    private class AddCube : EditorToolbarButton
    {
        public const string Id = "Place Actor Bar/AddCube";

        /*
         * Constructeur :
         * text : S'affiche quand la barre est déplié horizontalement
         * tooltip : S'affiche quand on hover l'icon longtemps
         * icon : Permet d'afficher une icon quand en vertical
         * clicked : Permet de définir l'action à faire quand on click. (Utilisation d'une lambda ici pour passer le primitive souhaité).
         */
        public AddCube()
        {
            text = "Add Cube";
            tooltip = "Create a cube in the scene";
            icon = LoadIcon("cube");
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
            icon = LoadIcon("sphere");
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
            icon = LoadIcon("capsule");
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
            icon = LoadIcon("cylinder");
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
            tooltip = "Create a place in the scene";
            icon = LoadIcon("plane");
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
            icon = LoadIcon("quad");
            clicked += () => AddPrimitive(PrimitiveType.Quad);
        }
    }

    // Permet de créer un float field 
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
    
    
    // Méthode pour obtenir le chemin du package
    private static string GetPackagePath()
    {
        MonoScript script = MonoScript.FromScriptableObject(ScriptableObject.CreateInstance<PlaceActorBar>());
        string scriptPath = AssetDatabase.GetAssetPath(script);
        PackageInfo packageInfo = PackageInfo.FindForAssetPath(scriptPath);

        if (packageInfo != null)
        {
            return packageInfo.assetPath;
        }
        else
        {
            // Si le script n'est pas dans un package, utilise le chemin relatif au projet
            return Path.GetDirectoryName(scriptPath);
        }
    }
    
    // Méthode pour charger une icône depuis le package
    private static Texture2D LoadIcon(string iconName)
    {
        string packagePath = GetPackagePath();
        string iconPath = Path.Combine(packagePath, "Icons", $"{iconName}.png");
        return AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
    }
}