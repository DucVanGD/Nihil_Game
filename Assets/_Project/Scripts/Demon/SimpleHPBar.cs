using UnityEngine;

public class SimpleHPBar : MonoBehaviour
{
    public float yOffset = 1.2f;
    public float barWidth = 1.5f;
    public float barHeight = 0.15f;

    private GameObject fgObj;
    private GameObject bgObj;

    void Start()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        
        // Pivot 0.5, 0.5 (Background)
        Sprite bgSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        // Pivot 0, 0.5 (Foreground - Mở rộng từ trái sang phải)
        Sprite fgSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0f, 0.5f), 1f);

        bgObj = new GameObject("HPBar_BG");
        bgObj.transform.SetParent(transform);
        
        Vector3 parentScale = transform.localScale;
        // Kiểm tra nếu scale = 0 để tránh lỗi chia cho 0
        float absX = Mathf.Abs(parentScale.x) > 0 ? Mathf.Abs(parentScale.x) : 1f;
        float absY = Mathf.Abs(parentScale.y) > 0 ? Mathf.Abs(parentScale.y) : 1f;

        float actualWidth = barWidth / absX;
        float actualHeight = barHeight / absY;
        float actualY = yOffset / absY;

        bgObj.transform.localPosition = new Vector3(0, actualY, 0);
        bgObj.transform.localScale = new Vector3(actualWidth, actualHeight, 1f);

        SpriteRenderer bgSR = bgObj.AddComponent<SpriteRenderer>();
        bgSR.sprite = bgSprite;
        bgSR.color = Color.red; // Đỏ là nền
        bgSR.sortingOrder = 150;

        fgObj = new GameObject("HPBar_FG");
        fgObj.transform.SetParent(bgObj.transform);
        fgObj.transform.localPosition = new Vector3(-0.5f, 0f, -0.01f); // Nằm đè lên trước
        fgObj.transform.localScale = new Vector3(1f, 1f, 1f); // Bằng chiều rộng của nền
        
        SpriteRenderer fgSR = fgObj.AddComponent<SpriteRenderer>();
        fgSR.sprite = fgSprite;
        fgSR.color = Color.white; // Trắng là máu hiện tại
        fgSR.sortingOrder = 151;
    }

    void Update()
    {
        if (bgObj == null) return;

        // Chống lật thanh máu khi nhân vật lật mặt
        if (transform.localScale.x < 0)
        {
            bgObj.transform.localRotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            bgObj.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
    }

    public void UpdateHP(float currentHp, float maxHp)
    {
        if (fgObj != null)
        {
            float percent = Mathf.Clamp01(currentHp / maxHp);
            fgObj.transform.localScale = new Vector3(percent, 1f, 1f);
        }
    }
}
