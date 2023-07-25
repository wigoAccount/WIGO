using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class LoadingSpinnerAnimation : MonoBehaviour
{
    [SerializeField] Image[] _parts;
    [SerializeField] float _rotateSpeed = 1f;

    List<Sequence> _animations = new List<Sequence>();
    Coroutine _launchRoutine;

    const float PART_CYCLE_TIME = 2.33f;
    const float BLINK_RELATIVE_TIME = 0.4f;

    private void Awake()
    {
        float blinkHalfTime = (1f / _rotateSpeed) * PART_CYCLE_TIME * BLINK_RELATIVE_TIME / 2f;

        for (int i = 0; i < _parts.Length; i++)
        {
            var animation = DOTween.Sequence();
            Image stick = _parts[i];
            animation.Join(stick.DOFade(1f, blinkHalfTime).SetLoops(2, LoopType.Yoyo))
                .Join(stick.rectTransform.DOScale(1.15f, blinkHalfTime).SetLoops(2, LoopType.Yoyo))
                .SetAutoKill(false);

            _animations.Add(animation);
        }
    }

    private void OnEnable()
    {
        _launchRoutine = StartCoroutine(LaunchAnimation());
    }

    private void OnDisable()
    {
        if (_launchRoutine != null)
        {
            StopCoroutine(_launchRoutine);
        }

        foreach (var anim in _animations)
        {
            anim.Rewind();
        }
    }

    IEnumerator LaunchAnimation()
    {
        float stepDelay = (1f / _rotateSpeed) * PART_CYCLE_TIME / _parts.Length;
        while (true)
        {
            for (int i = 0; i < _animations.Count; i++)
            {
                _animations[i].Restart();
                yield return new WaitForSeconds(stepDelay);
            }
        }
    }
}
