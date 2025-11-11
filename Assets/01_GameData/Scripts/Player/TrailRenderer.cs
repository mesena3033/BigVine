using UnityEngine;
[RequireComponent(typeof(SpriteRenderer))]

public class TrailRenderer : MonoBehaviour
{
    void Start()
    {
        // TrailRenderer を追加
        UnityEngine.TrailRenderer trail = gameObject.AddComponent<UnityEngine.TrailRenderer>();

        // 残る時間（0.3秒くらいで短め）
        trail.time = 0.3f;

        // 太さ
        trail.startWidth = 0.2f;
        trail.endWidth = 0f;

        // マテリアル（Sprites/Default でもOK）
        trail.material = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));

        // 色（青→透明）
        trail.startColor = Color.cyan;
        trail.endColor = new Color(0, 1, 1, 0);
    }

}
