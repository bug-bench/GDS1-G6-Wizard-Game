using UnityEngine;

public class CharacterLayerSync : MonoBehaviour
{
    [Header("Animators — one per layer")]
    public Animator bodyAnimator;
    public Animator headwearAnimator;
    public Animator bodywearAnimator;
    public Animator facewearAnimator;

    private Animator[] all;

    void Awake()
    {
        all = new Animator[]
        {
            bodyAnimator,
            headwearAnimator,
            bodywearAnimator,
            facewearAnimator
        };
    }

    public void SetBool(string param, bool value)
    {
        foreach (var a in all)
            if (a != null) a.SetBool(param, value);
    }

    public void SetTrigger(string param)
    {
        foreach (var a in all)
            if (a != null) a.SetTrigger(param);
    }

    public void SetFloat(string param, float value)
    {
        foreach (var a in all)
            if (a != null) a.SetFloat(param, value);
    }

    public void SetInteger(string param, int value)
    {
        foreach (var a in all)
            if (a != null) a.SetInteger(param, value);
    }

    public void ApplySkin(CharacterSkin skin)
    {
        if (skin == null) return;
        if (bodyAnimator     != null && skin.bodyController     != null)
            bodyAnimator.runtimeAnimatorController     = skin.bodyController;
        if (headwearAnimator != null && skin.headwearController != null)
            headwearAnimator.runtimeAnimatorController = skin.headwearController;
        if (bodywearAnimator != null && skin.bodywearController != null)
            bodywearAnimator.runtimeAnimatorController = skin.bodywearController;
        if (facewearAnimator != null && skin.facewearController != null)
            facewearAnimator.runtimeAnimatorController = skin.facewearController;
    }
}