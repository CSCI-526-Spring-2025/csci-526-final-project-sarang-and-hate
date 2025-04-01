using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NavPanelBuilder : MonoBehaviour
{
    public Canvas canvas; // Assign your Canvas in Inspector (or find it in Start)
    private GameObject navPanel2;

    void Start()
    {
        CreateNavPanel2();
    }

    void CreateNavPanel2()
    {
        if (!canvas) canvas = FindObjectOfType<Canvas>();
        if (!canvas) return;

        // Panel
        navPanel2 = new GameObject("navPanel2", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        navPanel2.transform.SetParent(canvas.transform);
        RectTransform panelRect = navPanel2.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0); // bottom center
        panelRect.anchorMax = new Vector2(0.5f, 0);
        panelRect.pivot = new Vector2(0.5f, 0);
        panelRect.anchoredPosition = new Vector2(0, 30);
        panelRect.sizeDelta = new Vector2(400, 120);

        Image panelImage = navPanel2.GetComponent<Image>();
        panelImage.color = new Color(1f, 1f, 1f, 0.8f); // light translucent background

        // Vertical layout
        VerticalLayoutGroup layout = navPanel2.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 5;
        layout.padding = new RectOffset(10, 10, 10, 10);

        ContentSizeFitter fitter = navPanel2.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Add Key Labels
        AddKeyText("W - Move Forward");
        AddKeyText("A - Move Left");
        AddKeyText("S - Move Back");
        AddKeyText("D - Move Right");
        AddKeyText("C - Use Power-Up");
    }

    void AddKeyText(string label)
    {
        GameObject textObj = new GameObject(label, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(navPanel2.transform);

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 24;
        tmp.color = Color.black;
        tmp.alignment = TextAlignmentOptions.Center;

        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
    }
}

