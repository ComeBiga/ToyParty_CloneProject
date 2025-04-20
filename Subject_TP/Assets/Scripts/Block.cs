using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Block : MonoBehaviour
{
    public enum EColor { Red, Orange, Yellow, Green, Blue, Purple };

    public int index;
    public bool isMatchable = true;
    public EColor colorType = EColor.Red;

    [SerializeField]
    private Sprite[] _sprtBlock;
    [SerializeField]
    private Animator _animator;
    [SerializeField]
    private Animator _animatorLight;

    public virtual void Init(int index)
    {
        this.index = index;

        // SetColor((Block.EColor)UnityEngine.Random.Range(0, 6));
    }

    public void SetColor(EColor colorType)
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.sprite = _sprtBlock[(int)colorType];
        this.colorType = colorType;

        if (_animator != null)
        {
            _animator.enabled = true;
            _animator?.SetTrigger(colorType.ToString());
        }

#if UNITY_EDITOR
            EditorUtility.SetDirty(this.gameObject);
#endif
    }

    public void AnimateLight()
    {
        if (_animatorLight == null)
            return;

        _animatorLight.SetTrigger("Light");
    }

    public void AnimateDestroy()
    {
        if (_animator != null)
        {
            _animator.enabled = true;
            _animator?.SetTrigger("Destroy");
        }
    }

    public virtual bool IsMatchable(Block srcblock)
    {
        return colorType == srcblock.colorType;
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        SetColor(colorType);
    }

#endif
}
