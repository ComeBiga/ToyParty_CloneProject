using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Block : MonoBehaviour
{
    public enum EColor { Red, Orange, Yellow, Green, Blue, Purple };

    public int index;
    public EColor colorType = EColor.Red;

    [SerializeField]
    private Sprite[] _sprtBlock;

    public void SetColor(EColor colorType)
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.sprite = _sprtBlock[(int)colorType];
        this.colorType = colorType;

#if UNITY_EDITOR
        EditorUtility.SetDirty(this.gameObject);
#endif
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        SetColor(colorType);
    }

#endif
}
