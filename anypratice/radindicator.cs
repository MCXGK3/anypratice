namespace anypractice;

internal class radIndicators : MonoBehaviour
{
    private PlayMakerFSM attackCommands;
    private LineRenderer leftBoundRenderer;
    private LineRenderer rightBoundRenderer;
    private LineRenderer upperBoundRenderer;
    private LineRenderer lowerBoundRenderer;
    private LineRenderer innerBoundRenderer;
    private LineRenderer outerBoundRenderer;
    private float thetaScale = 0.01f;
    private float theta = 0;
    private int size;

    private void Awake()
    {
        attackCommands = base.gameObject.LocateMyFSM("Attack Commands");
    }

    private void Start()
    {
        GameObject leftBound = new GameObject();
        leftBound.AddComponent<LineRenderer>();
        leftBoundRenderer = leftBound.GetComponent<LineRenderer>();
        createLine(leftBoundRenderer, Color.red);

        GameObject rightBound = new GameObject();
        rightBound.AddComponent<LineRenderer>();
        rightBoundRenderer = rightBound.GetComponent<LineRenderer>();
        createLine(rightBoundRenderer, Color.red);

        GameObject upperBound = new GameObject();
        upperBound.AddComponent<LineRenderer>();
        upperBoundRenderer = upperBound.GetComponent<LineRenderer>();
        createLine(upperBoundRenderer, Color.red);

        GameObject lowerBound = new GameObject();
        lowerBound.AddComponent<LineRenderer>();
        lowerBoundRenderer = lowerBound.GetComponent<LineRenderer>();
        createLine(lowerBoundRenderer, Color.red);

        GameObject innerBound = new GameObject();
        innerBound.AddComponent<LineRenderer>();
        innerBoundRenderer = innerBound.GetComponent<LineRenderer>();
        createLine(innerBoundRenderer, Color.red);

        GameObject outerBound = new GameObject();
        outerBound.AddComponent<LineRenderer>();
        outerBoundRenderer = outerBound.GetComponent<LineRenderer>();
        createLine(outerBoundRenderer, Color.red);
    }

    private void Update()
    {
        int orbMinX = attackCommands.FsmVariables.GetFsmInt("Orb Min X").Value;
        int orbMinY = attackCommands.FsmVariables.GetFsmInt("Orb Min Y").Value;
        int orbMaxX = attackCommands.FsmVariables.GetFsmInt("Orb Max X").Value;
        int orbMaxY = attackCommands.FsmVariables.GetFsmInt("Orb Max Y").Value;

        leftBoundRenderer.SetPosition(0, new Vector3(orbMinX, orbMinY, 0));
        leftBoundRenderer.SetPosition(1, new Vector3(orbMinX, orbMaxY, 0));
        rightBoundRenderer.SetPosition(0, new Vector3(orbMaxX, orbMinY, 0));
        rightBoundRenderer.SetPosition(1, new Vector3(orbMaxX, orbMaxY, 0));
        upperBoundRenderer.SetPosition(0, new Vector3(orbMinX, orbMaxY, 0));
        upperBoundRenderer.SetPosition(1, new Vector3(orbMaxX, orbMaxY, 0));
        lowerBoundRenderer.SetPosition(0, new Vector3(orbMinX, orbMinY, 0));
        lowerBoundRenderer.SetPosition(1, new Vector3(orbMaxX, orbMinY, 0));

        createCircle(innerBoundRenderer, attackCommands.GetAction<FloatCompare>("Orb Pos", 6).float2.Value);
        createCircle(outerBoundRenderer, attackCommands.GetAction<FloatCompare>("Orb Pos", 7).float2.Value);
    }

    private void createLine(LineRenderer renderer, Color color)
    {
        renderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply")); ;
        renderer.startColor = color;
        renderer.endColor = color;
        renderer.startWidth = 0.15f;
        renderer.endWidth = 0.15f;
    }

    private void createCircle(LineRenderer renderer, float radius)
    {
        theta = 0f;
        size = (int)((1f / thetaScale) + 1f);
        renderer.positionCount = size;
        for (int i = 0; i < size; i++)
        {
            theta += (2.0f * Mathf.PI * thetaScale);
            float x = radius * Mathf.Cos(theta);
            float y = radius * Mathf.Sin(theta);
            renderer.SetPosition(i, new Vector3(base.gameObject.transform.GetPositionX() + x, base.gameObject.transform.GetPositionY() + y));
        }
    }

    public void Unload()
    {
        GameObject.DestroyImmediate(leftBoundRenderer);
        GameObject.DestroyImmediate(rightBoundRenderer);
        GameObject.DestroyImmediate(upperBoundRenderer);
        GameObject.DestroyImmediate(lowerBoundRenderer);
        GameObject.DestroyImmediate(innerBoundRenderer);
        GameObject.DestroyImmediate(outerBoundRenderer);
    }
}