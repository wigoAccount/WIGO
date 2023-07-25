using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CoroutineDispatcher : MonoBehaviour
{
    LinkedList<Action> _coroutines = new LinkedList<Action>();
    public static event Action OnUpdate;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        ServiceLocator.Set(this);
    }

    void OnDestroy()
    {
        OnUpdate = null;
        ServiceLocator.Release<CoroutineDispatcher>();
    }

    public void Execute(IEnumerator routine, Action callback)
    {
        var node = _coroutines.AddLast(callback);
        StartCoroutine(Executor(routine, node));
    }

    public void Execute(YieldInstruction routine, Action callback, CancellationToken token = default)
    {
        var node = _coroutines.AddLast(callback);
        StartCoroutine(Executor(routine, node), token);
    }

    public Coroutine StartCoroutine(IEnumerator routine, CancellationToken cancellationToken)
    {
        return StartCoroutine(CancellableCoroutine(routine, cancellationToken));
    }

    static IEnumerator CancellableCoroutine(IEnumerator routine, CancellationToken token)
    {
        if (routine == null)
            throw new ArgumentNullException(nameof(routine));

        if (token == CancellationToken.None)
            throw new ArgumentException("Invalid argument: CancellationToken.None", nameof(token));

        while (!token.IsCancellationRequested && routine.MoveNext())
            yield return routine.Current;
    }

    IEnumerator CoroutineHandler(IEnumerator routine)
    {
        do
        {
            var currentStep = routine.Current as IEnumerator;
            if (currentStep != null)
            {
                yield return CoroutineHandler(currentStep);
            }
        } while (routine.MoveNext());
    }

    IEnumerator YieldInstuctionHandler(YieldInstruction routine)
    {
        yield return routine;
    }

    IEnumerator Executor(IEnumerator routine, LinkedListNode<Action> callback)
    {
        yield return CoroutineHandler(routine);
        _coroutines.Remove(callback);
        callback.Value?.Invoke();
    }

    IEnumerator Executor(YieldInstruction routine, LinkedListNode<Action> callback)
    {
        yield return YieldInstuctionHandler(routine);
        _coroutines.Remove(callback);
        callback.Value?.Invoke();
    }

    private void Update()
    {
        OnUpdate?.Invoke();
    }
}
