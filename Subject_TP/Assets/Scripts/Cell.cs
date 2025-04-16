using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public bool enable = true;
    public bool spawn = false;

    [SerializeField]
    private Sprite _sprtEnable;
    [SerializeField]
    private Sprite _sprtDisable;

    private SpriteRenderer _sprtRenderer;

    private void Start()
    {

    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        setSprite(enable);
    }

    private void setSprite(bool enable)
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();

        if (enable)
        {
            spriteRenderer.sprite = _sprtEnable;
        }
        else
        {
            spriteRenderer.sprite = _sprtDisable;
        }

        EditorUtility.SetDirty(this.gameObject);
    }

#endif
}
