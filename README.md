***Fibers*** provides an alternative co-operative multi-tasking approach to Coroutines with less overhead. It is not a drop-in replacement but is intended for heavy usage situations. The **only** way to take the load from the garbage collector is to precompile fibers and reuse them.

Coroutines are the core mechanism in Unity3d MonoBehaviour classes to provide independent processing as a form of co-operative multi-tasking. Activities that must occur in order use `yield return myCoroutine();` to wait on completion before continuing. Yield instructions must reside in methods that have an IEnumerator return type. C# turns them into a state machine. These state machines are not resettable, so they must be discarded once complete. Since every call generates a new state machine, this puts a heavy load on the garbage collector. Using coroutines in abundance can cause glitches in the running of VR and mobile applications.

On another subject, some unity packages, specifically FireBase, use C# 4+ Tasks, a preemptive multitasking interface. Anything using Task callbacks must be treated as independent threads with semaphores and the like to protect against data corruption. The Askowl Tasks class mergest tasks into Fibers so that they fit better and more safely into the Unity system.

[Full Documentation Here](https://paulmarrington.github.io/Unity-Documentation/Fibers/)
