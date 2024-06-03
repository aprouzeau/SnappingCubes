using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SnappingManager : MonoBehaviour
{

    public GameObject cube1;
    public GameObject cube2;

    public class pairPoint
    {
        public Vector3 pointC1;
        public Vector3 pointC2;
        public Color c;
        public float distance;
    }

    IDictionary<pairPoint, float> pointsToMatch = new Dictionary<pairPoint, float>();
    List<pairPoint> pointsToMatchList = new List<pairPoint>();

    int countOfTest = 0;
    [SerializeField]
    int TotalTrial;
    bool testDone = false;

    [SerializeField]
    string Cube1Pos;

    [SerializeField]
    string Cube1Rot;

    [SerializeField]
    string Cube2Pos;

    [SerializeField]
    string Cube2Rot;

    [SerializeField]
    bool prodCode;



    // Start is called before the first frame update
    void Start()
    {

        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo( "en-US" );

        System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo( "en-US" );

        // Generate random positions within a 1x1x1 meter cube
        string[] cube1PosArr= Cube1Pos.Split(',');
        string[] cube1RotArr= Cube1Rot.Split(',');
        cube1.transform.position = new Vector3(float.Parse(cube1PosArr[0].Trim()), float.Parse(cube1PosArr[1].Trim()), float.Parse(cube1PosArr[2]));
        cube1.transform.eulerAngles = new Vector3(float.Parse(cube1RotArr[0]), float.Parse(cube1RotArr[1]), float.Parse(cube1RotArr[2]));

        string[] cube2PosArr= Cube2Pos.Split(',');
        string[] cube2RotArr= Cube2Rot.Split(',');
        cube2.transform.position = new Vector3(float.Parse(cube2PosArr[0]), float.Parse(cube2PosArr[1]), float.Parse(cube2PosArr[2]));
        cube2.transform.eulerAngles = new Vector3(float.Parse(cube2RotArr[0]), float.Parse(cube2RotArr[1]), float.Parse(cube2RotArr[2]));
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.M)){
            testMultiple();
        }

        if(Input.GetKeyDown(KeyCode.T)){
            testOne();
        }
    }

    void testOne(){

        string[] cube1PosArr= Cube1Pos.Split(',');
        string[] cube1RotArr= Cube1Rot.Split(',');
        cube1.transform.position = new Vector3(float.Parse(cube1PosArr[0].Trim()), float.Parse(cube1PosArr[1].Trim()), float.Parse(cube1PosArr[2]));
        cube1.transform.eulerAngles = new Vector3(float.Parse(cube1RotArr[0]), float.Parse(cube1RotArr[1]), float.Parse(cube1RotArr[2]));

        string[] cube2PosArr= Cube2Pos.Split(',');
        string[] cube2RotArr= Cube2Rot.Split(',');
        cube2.transform.position = new Vector3(float.Parse(cube2PosArr[0]), float.Parse(cube2PosArr[1]), float.Parse(cube2PosArr[2]));
        cube2.transform.eulerAngles = new Vector3(float.Parse(cube2RotArr[0]), float.Parse(cube2RotArr[1]), float.Parse(cube2RotArr[2]));

        Vector3 euler1 = cube1.transform.eulerAngles;
        Vector3 euler2 = cube2.transform.eulerAngles;
        Vector3 randomPosition1 = cube1.transform.position;
        Vector3 randomPosition2 = cube2.transform.position;
        getClosestThreePoints2(prodCode);
        getTransform(prodCode);
        if(!areCubeAligned()){
            Debug.Log("Cube1");
            Debug.Log("Pos: "+randomPosition1);
            Debug.Log("Rot: "+euler1);
            Debug.Log("Cube2");
            Debug.Log("Pos: "+randomPosition2);
            Debug.Log("Rot: "+euler2);
        }
        pointsToMatch.Clear();
    }

    void testMultiple()
    {
        Debug.Log("Test starting.");
        int numberFail = 0;
        while (countOfTest < TotalTrial){
            //Debug.Log("Test " + countOfTest);
            Vector3 randomPosition1 = new Vector3(UnityEngine.Random.Range(0f, 10f), UnityEngine.Random.Range(0f, 10f), UnityEngine.Random.Range(0f, 10f));
            Vector3 randomPosition2 = new Vector3(UnityEngine.Random.Range(0f, 10f), UnityEngine.Random.Range(0f, 10f), UnityEngine.Random.Range(0f, 10f));

            // Generate random rotations
            Quaternion randomRotation1 = UnityEngine.Random.rotation;
            Quaternion randomRotation2 = UnityEngine.Random.rotation;

            // Apply random positions and rotations to the game objects
            cube1.transform.position = randomPosition1;
            cube1.transform.rotation = randomRotation1;
            Vector3 euler1 = cube1.transform.eulerAngles;

            cube2.transform.position = randomPosition2;
            cube2.transform.rotation = randomRotation2;
            Vector3 euler2 = cube2.transform.eulerAngles;

            getClosestThreePoints2(prodCode);
            getTransform(prodCode);

            if(!areCubeAligned()){
                Debug.Log("Cube1");
                Debug.Log("Pos: "+randomPosition1);
                Debug.Log("Rot: "+euler1);
                Debug.Log("Cube2");
                Debug.Log("Pos: "+randomPosition2);
                Debug.Log("Rot: "+euler2);
                numberFail ++;
            }

            pointsToMatch.Clear();
            countOfTest ++;
        }

        
        Debug.Log("Test done.");
        Debug.Log("Test failed:" + numberFail);
    }

    bool areCubeAligned(){

        float epsilon = 0.05f;
        Mesh mesh1 = cube1.GetComponent<MeshFilter>().mesh;
        Vector3[] verticesLoc1 = GetUniqueVertices(mesh1);

        Vector3[] verticesGlo1 = new Vector3[verticesLoc1.Length];
        for (var i = 0; i < verticesGlo1.Length; i++)
        {
            verticesGlo1[i] = cube1.transform.TransformPoint(verticesLoc1[i]);
            
        }

        Mesh mesh2 = cube2.GetComponent<MeshFilter>().mesh;
        Vector3[] verticesLoc2 = GetUniqueVertices(mesh2);

        Vector3[] verticesGlo2 = new Vector3[verticesLoc2.Length];
        for (var i = 0; i < verticesGlo2.Length; i++)
        {
            verticesGlo2[i] = cube2.transform.TransformPoint(verticesLoc2[i]);
        }

        int nbSimilarVertices = 0;

        for (var i = 0; i < verticesGlo1.Length; i++){
            for (var j = 0; j < verticesGlo2.Length; j++){
                float dist = Vector3.Distance(verticesGlo1[i], verticesGlo2[j]);
                if(dist<epsilon){
                    // GameObject sphere1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    // sphere1.name = "SphereDebug"; 
                    // sphere1.transform.position = verticesGlo1[i];
                    // sphere1.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                    nbSimilarVertices++;
                }
            }
        }

        if(nbSimilarVertices != 4){
            Debug.Log("Oh Shit it is not aligned: "+nbSimilarVertices);
            
            return false;
        }

        return true;

    }
    

    void getClosestThreePoints2(bool prod)
    {
        IDictionary<pairPoint, float> DicoOfDistance = new Dictionary<pairPoint, float>();

        // Get arrays of vertices that are unique
        Mesh mesh1 = cube1.GetComponent<MeshFilter>().mesh;
        Vector3[] verticesLoc1 = GetUniqueVertices(mesh1);

        Vector3[] verticesGlo1 = new Vector3[verticesLoc1.Length];
        for (var i = 0; i < verticesGlo1.Length; i++)
        {
            verticesGlo1[i] = cube1.transform.TransformPoint(verticesLoc1[i]);
        }

        Mesh mesh2 = cube2.GetComponent<MeshFilter>().mesh;
        Vector3[] verticesLoc2 = GetUniqueVertices(mesh2);

        Vector3[] verticesGlo2 = new Vector3[verticesLoc2.Length];
        for (var i = 0; i < verticesGlo2.Length; i++)
        {
            verticesGlo2[i] = cube2.transform.TransformPoint(verticesLoc2[i]);
        }

        Vector3[] vertices1 = verticesGlo1.Distinct().ToArray();
        Vector3[] vertices2 = verticesGlo2.Distinct().ToArray();

        List<pairPoint> pairs = new List<pairPoint>();

        for (var i = 0; i < vertices1.Length; i++)
        {
            // vertices[i] += Vector3.up * Time.deltaTime;
            Vector3 pointToCompare = vertices1[i];
            for (var j = 1; j < vertices2.Length; j++)
            {
                float distanceTemp = Vector3.Distance(pointToCompare, vertices2[j]);
                pairPoint temp = new pairPoint();
                temp.pointC1 = pointToCompare;
                temp.pointC2 = vertices2[j];
                temp.distance = distanceTemp;
                pairs.Add(temp);
                DicoOfDistance[temp] = distanceTemp;
            }
            
        }

        pairs.Sort((a, b) => a.distance.CompareTo(b.distance));

        pointsToMatchList = FindOptimalPairs(pairs, vertices1.Length, vertices2.Length);

        //Debug.Log("List Valid: " + IsValidSolution(pointsToMatchList));


        /* pairPoint p1 = getMinInDict(DicoOfDistance);
        p1.c = Color.red;
        pointsToMatch[p1] = DicoOfDistance[p1];
        DicoOfDistance.Remove(p1);

        pairPoint p2 = findMinNotUsed(DicoOfDistance);
        p2.c = Color.blue;
        pointsToMatch[p2] = DicoOfDistance[p2];
        DicoOfDistance.Remove(p2);

        pairPoint p3 = findMinNotUsed(DicoOfDistance);
        p3.c = Color.green;
        pointsToMatch[p3] = DicoOfDistance[p3];
        DicoOfDistance.Remove(p3);*/

        if(!prod){
            DebugPoint2();
        }

    }

    /*private pairPoint findMinNotUsed(IDictionary<pairPoint, float> dicoOfDistance)
    {
        bool notFound = true;
        pairPoint pFound = null;

        var distSorted = from pair in dicoOfDistance
                         orderby pair.Value ascending
                         select pair;
        foreach (KeyValuePair<pairPoint, float> pair in distSorted)
        {
            if (notFound)
            {
                if (!isVertexAlreadyUsed(pair.Key.pointC1) && !isVertexAlreadyUsed(pair.Key.pointC2))
                {
                    notFound = false;
                    pFound = pair.Key;
                }
            }
        }

        

        if (pFound == null)
        {
            throw new Exception("All hell breaks loose");

        }
        return pFound;
    }*/

    // bool isVertexAlreadyUsed(Vector3 v)
    // {

    //     bool isItIn = false;
    //     foreach (KeyValuePair<pairPoint, float> kvp in pointsToMatch)
    //     {
    //         Vector3 v1  = kvp.Key.pointC1;
    //         Vector3 v2 = kvp.Key.pointC2;
    //         if (v1.Equals(v))
    //         {
    //             isItIn = true;
    //         }
    //         if (v2.Equals(v))
    //         {
    //             isItIn = true;
    //         }
    //     }

    //     return isItIn;
    // }

    void getTransform(bool prod)
    {

        Vector3[] p1 = new Vector3[3];
        Vector3[] p2 = new Vector3[3];

        int i = 0;
        //foreach (KeyValuePair<pairPoint, float> kvp in pointsToMatch)
        foreach (pairPoint kvp in pointsToMatchList)
        {
            p1[i] = kvp.pointC1;
            p2[i] = kvp.pointC2;
            i++;
        }

        Plane pl1 = new Plane(p1[0], p1[1], p1[2]);
        Vector3 projectionCenter1 = pl1.ClosestPointOnPlane(cube1.transform.position);
        Vector3 projectedCentre1 = cube1.transform.position + 2 * new Vector3(projectionCenter1.x- cube1.transform.position.x, projectionCenter1.y - cube1.transform.position.y, projectionCenter1.z - cube1.transform.position.z);
        //showPoint(projectionCenter1);

        GameObject project1 = new GameObject("Projection 1");
        project1.transform.position = projectionCenter1;
        project1.transform.LookAt(projectedCentre1);


        Plane pl2 = new Plane(p2[0], p2[1], p2[2]);
        Vector3 projectionCenter2 = pl2.ClosestPointOnPlane(cube2.transform.position);
        GameObject project2 = new GameObject("Projection 2");
        project2.transform.position = projectionCenter2;
        project2.transform.LookAt(cube2.transform.position);
        Vector3 projectedCentre2 = cube2.transform.position + 2 * new Vector3(projectionCenter2.x - cube2.transform.position.x, projectionCenter2.y - cube2.transform.position.y, projectionCenter2.z - cube2.transform.position.z);

        GameObject project2t = new GameObject("Projection 2 Tempo");
        project2t.transform.position = projectedCentre2;
        project2t.transform.LookAt(cube2.transform.position);

        GameObject project1t = new GameObject("Projection 1 Tempo");
        project1t.transform.position = cube1.transform.position;
        project1t.transform.LookAt(projectionCenter1);


        /*Vector3 translation = projectedCentre2 - cube1.transform.position;
        Quaternion rotation = project2.transform.rotation * Quaternion.Inverse(project1.transform.rotation);
        Quaternion rotation2 = project2t.transform.rotation * Quaternion.Inverse(project1t.transform.rotation);*/

        //Vector3 rotation2 = project2.transform.eulerAngles - project1.transform.eulerAngles;

        //project1.transform.Translate(translation, Space.World);
        //project1.transform.rotation = rotation * project1.transform.rotation;
        int id0 = 0;
        int id1 = 1;
        if(Vector3.Distance(p1[0], p1[1]) > Vector3.Distance(p1[1], p1[2])){
            id0 = 1;
            id1 = 2;
        }
        
        Vector3 p2p11 = p1[id0] + (p1[id1] - p1[id0]) / 2;
        if(!prod){
            showPoint(p2p11, "p2p11");
        }
        var directInit = project1.transform.up;
        var direction = (p2p11 - project1.transform.position).normalized;

        //atan2((Va x Vb) . Vn, Va . Vb)
        Vector3 crossPro = Vector3.Cross(directInit, direction);
        float angleToRotate = Mathf.Atan2(Vector3.Dot(crossPro, project1.transform.forward), Vector3.Dot(directInit, direction)) * Mathf.Rad2Deg;
        project1.transform.Rotate(0, 0, angleToRotate);
        project1t.transform.eulerAngles = new Vector3(project1t.transform.eulerAngles.x, project1t.transform.eulerAngles.y, project1.transform.eulerAngles.z);

        Vector3 p2p12 = p2[id0] + (p2[id1] - p2[id0]) / 2;
        if(!prod){
            showPoint(p2p12, "p2p12");
        }
        var directInit2 = project2.transform.up;
        var direction2 = (p2p12 - project2.transform.position).normalized;

        //atan2((Va x Vb) . Vn, Va . Vb)
        Vector3 crossPro2 = Vector3.Cross(directInit2, direction2);
        float angleToRotate2 = Mathf.Atan2(Vector3.Dot(crossPro2, project2.transform.forward), Vector3.Dot(directInit2, direction2)) * Mathf.Rad2Deg;
        
        project2.transform.Rotate(0, 0, angleToRotate2);
        project2t.transform.eulerAngles = new Vector3(project2t.transform.eulerAngles.x, project2t.transform.eulerAngles.y, project2.transform.eulerAngles.z);

        Vector3 translation = projectedCentre2 - cube1.transform.position;
        Quaternion rotation = project2.transform.rotation * Quaternion.Inverse(project1.transform.rotation);
        Quaternion rotation2 = project2t.transform.rotation * Quaternion.Inverse(project1t.transform.rotation);

        //project1.transform.rotation = Quaternion.FromToRotation(directInit, direction);
        if(prod){
            cube1.transform.Translate(translation, Space.World);
            cube1.transform.rotation = rotation2 * cube1.transform.rotation;

            Destroy(project1);
            Destroy(project1t);
            Destroy(project2);
            Destroy(project2t);
        }


        //cube1.transform.Rotate(rotation2);

    }

    void showPoint(Vector3 v3, string name)
    {
        GameObject sphere1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere1.transform.position = v3;
        sphere1.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        sphere1.transform.LookAt(cube1.transform.position);
        sphere1.name = name;
        //sphere1.transform.Rotate(sphere1.transform.up, 180);
    }


    void showLine(Vector3 v1, Vector3 v2, Color m, string name){
        GameObject lineR = new GameObject(name);
        LineRenderer lineRenderer = lineR.AddComponent<LineRenderer>();
        lineRenderer.widthMultiplier = 0.2f;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, v1);
        lineRenderer.SetPosition(1, v2);
        
        
        // Create a new material with a shader that supports color (e.g., Sprites/Default)
        Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material = lineMaterial;

        // Set the color
        lineRenderer.startColor = m; // Set your desired start color
        lineRenderer.endColor = m;   // Set your desired end color

    }
    void DebugPoint()
    {
        foreach (KeyValuePair<pairPoint, float> kvp in pointsToMatch)
        {
            //Material material = new Material();
            

            GameObject sphere1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere1.transform.position = kvp.Key.pointC1;
            sphere1.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            sphere1.GetComponent<Renderer>().material.SetColor("_Color", kvp.Key.c);

            GameObject sphere2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere2.transform.position = kvp.Key.pointC2;
            sphere2.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            sphere2.GetComponent<Renderer>().material.SetColor("_Color", kvp.Key.c);

            showLine(kvp.Key.pointC1, kvp.Key.pointC2, kvp.Key.c, "LineTest");
        }
    }
    void DebugPoint2()
    {
        // Initialize the array with 3 elements
        Color[] colors = new Color[3];

        // Assign colors to the array
        colors[0] = Color.red;
        colors[1] = Color.green;
        colors[2] = Color.blue;
        int count = 0;
        
        foreach (pairPoint kvp in pointsToMatchList)
        {
            //Material material = new Material();
            kvp.c = colors[count];

            GameObject sphere1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere1.transform.position = kvp.pointC1;
            sphere1.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            sphere1.GetComponent<Renderer>().material.SetColor("_Color", kvp.c);

            GameObject sphere2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere2.transform.position = kvp.pointC2;
            sphere2.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            sphere2.GetComponent<Renderer>().material.SetColor("_Color", kvp.c);

            showLine(kvp.pointC1, kvp.pointC2, kvp.c, "LineTest");
            count ++;
        }
    }
    // pairPoint getMinInDict(IDictionary<pairPoint, float> theDict)
    // {
    //     pairPoint pFound = null;
    //     float distToBeat = 10000;
    //     foreach (KeyValuePair<pairPoint, float> kvp in theDict)
    //     {
    //         if(kvp.Value < distToBeat)
    //         {
    //             pFound = kvp.Key;
    //             distToBeat = kvp.Value;
    //         }
    //     }
    //     if (pFound == null)
    //     {
    //         throw new Exception("All hell breaks loose");
            
    //     }
    //     return pFound;

    // }

    public static Vector3[] GetUniqueVertices(Mesh mesh)
    {
        // Retrieve the vertices from the mesh
        Vector3[] vertices = mesh.vertices;

        // Use a HashSet to store unique vertices
        HashSet<Vector3> uniqueVertices = new HashSet<Vector3>(vertices);

        // Convert the HashSet back to an array and return
        return new List<Vector3>(uniqueVertices).ToArray();
    }




//Optimising the sum of distance between points

    bool IsUniqueCombination(List<pairPoint> combo, int count1, int count2)
    {
        var set1 = new HashSet<Vector3>();
        var set2 = new HashSet<Vector3>();

        foreach (var pair in combo)
        {
            set1.Add(pair.pointC1);
            set2.Add(pair.pointC2);
        }

        return set1.Count == combo.Count && set2.Count == combo.Count;
    }

    List<List<pairPoint>> GetCombinations(List<pairPoint> pairs, int count1, int count2)
    {
        var results = new List<List<pairPoint>>();

        for (int i = 0; i < pairs.Count; i++)
        {
            for (int j = i + 1; j < pairs.Count; j++)
            {
                for (int k = j + 1; k < pairs.Count; k++)
                {
                    var combo = new List<pairPoint> { pairs[i], pairs[j], pairs[k] };

                    if (IsUniqueCombination(combo, count1, count2) && IsValidSolution(combo))
                    {
                        results.Add(combo);
                    }
                }
            }
        }

        return results;
    }
    List<pairPoint> FindOptimalPairs(List<pairPoint> pairs, int count1, int count2)
    {
        List<pairPoint> optimalPairs = null;
        float minSumDistance = float.MaxValue;

        var combinations = GetCombinations(pairs, count1, count2);

        foreach (var combo in combinations)
        {
            if (combo.Count == 3)
            {
                float sumDistance = combo.Sum(p => p.distance);

                if (sumDistance < minSumDistance)
                {
                    minSumDistance = sumDistance;
                    optimalPairs = combo;
                }
            }
        }

        return optimalPairs;
    } 


    bool IsValidSolution(List<pairPoint> pairs, float tolerance = 0.01f)
    {
        if (pairs.Count != 3)
        {
            return false;
        }

        Vector3 p1 = pairs[0].pointC1;
        Vector3 p2 = pairs[1].pointC1;
        Vector3 p3 = pairs[2].pointC1;

        Vector3 q1 = pairs[0].pointC2;
        Vector3 q2 = pairs[1].pointC2;
        Vector3 q3 = pairs[2].pointC2;

        // Form vectors for Cube 1
        Vector3 V1 = p2 - p1;
        Vector3 V2 = p3 - p1;
        Vector3 V3 = p3 - p2;

        // Form vectors for Cube 2
        Vector3 U1 = q2 - q1;
        Vector3 U2 = q3 - q1;
        Vector3 U3 = q3 - q2;

        // Compute angles for Cube 1
        float angle1C1 = Vector3.Angle(V1, V2);
        float angle2C1 = Vector3.Angle(V1, V3);
        float angle3C1 = Vector3.Angle(V2, V3);

        // Compute angles for Cube 2
        float angle1C2 = Vector3.Angle(U1, U2);
        float angle2C2 = Vector3.Angle(U1, U3);
        float angle3C2 = Vector3.Angle(U2, U3);

        Vector3 crossV1V2 = Vector3.Cross(V1, V2);
        Vector3 crossU1U2 = Vector3.Cross(U1, U2);

        if (Vector3.Dot(crossV1V2, crossU1U2) < 0)
        {
            return false; // The points are not in the same order on Cube 1
        }

        // Compare angles with a tolerance
        if (Mathf.Abs(angle1C1 - angle1C2) < tolerance &&
            Mathf.Abs(angle2C1 - angle2C2) < tolerance &&
            Mathf.Abs(angle3C1 - angle3C2) < tolerance)
        {
            return true;
        }

        return false;
    }

}

// Previous function that does not work

/*void getClosestThreePoints()
{

    IDictionary<pairPoint, float> ClosestVertices = new Dictionary<pairPoint, float>();

    Mesh mesh1 = cube1.GetComponent<MeshFilter>().mesh;
    Vector3[] verticesLoc1 = mesh1.vertices;

    Vector3[] verticesGlo1 = new Vector3[verticesLoc1.Length];
    for (var i = 0; i < verticesGlo1.Length; i++)
    {
        verticesGlo1[i] = cube1.transform.TransformPoint(verticesLoc1[i]);
    }

    Mesh mesh2 = cube2.GetComponent<MeshFilter>().mesh;
    Vector3[] verticesLoc2 = mesh2.vertices;

    Vector3[] verticesGlo2 = new Vector3[verticesLoc2.Length];
    for (var i = 0; i < verticesGlo2.Length; i++)
    {
        verticesGlo2[i] = cube2.transform.TransformPoint(verticesLoc2[i]);
    }

    Vector3[] vertices1 = verticesGlo1.Distinct().ToArray();
    Vector3[] vertices2 = verticesGlo2.Distinct().ToArray();

    for (var i = 0; i < vertices1.Length; i++)
    {
        // vertices[i] += Vector3.up * Time.deltaTime;
        Vector3 pointToCompare = vertices1[i];
        float distanceFound = Vector3.Distance(pointToCompare, vertices2[0]);
        Vector3 closestPoint = vertices2[0];
        for (var j = 1; j < vertices2.Length; j++)
        {
            float distanceTemp = Vector3.Distance(pointToCompare, vertices2[j]);
            if (distanceTemp < distanceFound)
            {
                distanceFound = distanceTemp;
                closestPoint = vertices2[j];
            }
        }
        pairPoint tempWinner = new pairPoint();
        tempWinner.pointC1 = pointToCompare;
        tempWinner.pointC2 = closestPoint;
        ClosestVertices[tempWinner] = distanceFound;
    }

    Debug.Log("Number of pairs: " + ClosestVertices.Count);
    // Ici on pourrait faire une boucle mais je fais les points individuellement pour changer les couleurs
    pairPoint p1 = getMinInDict(ClosestVertices);
    p1.c = Color.red;
    pointsToMatch[p1] = ClosestVertices[p1];
    ClosestVertices.Remove(p1);

    pairPoint p2 = getMinInDict(ClosestVertices);
    p2.c = Color.blue;
    pointsToMatch[p2] = ClosestVertices[p2];
    ClosestVertices.Remove(p2);

    pairPoint p3 = getMinInDict(ClosestVertices);
    p3.c = Color.green;
    pointsToMatch[p3] = ClosestVertices[p3];
    ClosestVertices.Remove(p3);

    pairPoint p4 = getMinInDict(ClosestVertices);
    p4.c = Color.green;
    pointsToMatch[p4] = ClosestVertices[p4];
    ClosestVertices.Remove(p4);

    DebugPoint();


}*/
