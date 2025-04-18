using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableBlock : Block
{
    public int hp = 2;

    [SerializeField]
    private Animator _animator;

    public override void Init(int index)
    {
        this.index = index;
    }

    public override bool IsMatchable(Block srcblock)
    {
        return false;
    }

    public void DecreaseHP(int amount)
    {
        hp -= amount;

        if(hp == 1)
        {
            _animator.SetTrigger("Activate");
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        
    }

#endif
}
