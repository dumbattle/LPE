

/// <summary>
/// Expected usage:
/// <code>
/// ISequence s;
/// 
/// while (true) {
///     s.Next();       // call next before checking done
///     if (s.Done()) { // Allow at least 1 guarenteed call to Next 
///         break();    // Further calls to Next() may break sequence
///     }
/// }
/// s.OnComplete();     // Clears all data, possibly returns sequences to object pools
/// s = null;           // Sequence may be reused elsewhere, clear reference to avoid problems
/// </code>
/// </summary>
public interface ISequence {

    /// <summary>
    /// Next should always be called at least once
    /// </summary>
    void Next();

    /// <summary>
    /// Make sure to call Next() before checking this
    /// Single step sequences may always return true here,
    /// </summary>
    bool Done();

    /// <summary>
    /// The sequence may be returned to an object pool, so you should release any references
    /// </summary>
    void OnComplete();
}
