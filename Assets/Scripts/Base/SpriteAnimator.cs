using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Newtonsoft.Json;

public class Animation
{
    public string Name;
    public int FPS;
    public bool Loop;
    public List<int> Frames;
}

public class AnimationList
{
    public List<Animation> Animations;
}

public class SpriteAnimator : MonoBehaviour
{
    // Private Members
    private Dictionary<string, Animation> _animations = new Dictionary<string, Animation>();
    private SpriteRenderer _spriteRenderer;
    private Animation _anim;
    public string _lastAnimation = "";

    public List<Sprite> Sprites = new List<Sprite>();
    public string SpriteSheetName = "";
    public string AnimationsJson = "";
    public int SpriteCount = 0;

    // Private Methods
    private IEnumerator RunAnimation()
    {
        int i = 0;
        while (i < _anim.Frames.Count)
        {
            _spriteRenderer.sprite = Sprites[_anim.Frames[i]];
            i++;
            yield return new WaitForSeconds(1.0f / _anim.FPS);
            yield return 0;
        }
        if (_animations[_lastAnimation].Loop)
            StartCoroutine(RunAnimation());
    }

    private void Animate(string animationName)
    {
        if (_lastAnimation != animationName)
        {
            StopAllCoroutines();
            if (_animations.ContainsKey(animationName))
            {
                _lastAnimation = animationName;
                _anim = _animations[animationName];
                StartCoroutine(RunAnimation());
            }
            else
                throw new System.Exception("Animation '" + animationName + "' Does Not Exist on GameObject named " + gameObject.name);
        }
    }

    // Public Members
    public string CurrentAnimation { get { return _lastAnimation; } }

    // Public Methods
    public void SetSpriteRenderer(SpriteRenderer renderer)
    {
        _spriteRenderer = renderer;
    }

    public void PlayAnimation(string animationName)
    {
        Animate(animationName);
    }

    public void Stop()
    {
        Animate(_lastAnimation.Replace("Walk", "Idle"));
    }

    public void Initialise()
    {
        _spriteRenderer = GetComponentInParent<SpriteRenderer>();

        Sprites = new List<Sprite>();

        Sprite[] sprites = Resources.LoadAll<Sprite>("Characters/" + SpriteSheetName + "/" + SpriteSheetName);
        for (int i = 0; i < sprites.Length; i++)
            Sprites.Add(sprites[i]);


        AnimationList animations = JsonConvert.DeserializeObject<AnimationList>(AnimationsJson);
        foreach (Animation anim in animations.Animations)
            _animations.Add(anim.Name, anim);
    }
}
