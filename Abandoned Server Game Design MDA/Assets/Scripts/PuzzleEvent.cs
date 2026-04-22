using UnityEngine;
using UnityEngine.Events;

public class PuzzleEvent : MonoBehaviour
{
    // This enum defines the DIFFERENT TYPES of puzzle logic we can use
    //all is every input must be active
    //any just one input is enough
    //sequence must be activated in specific order
    public enum LogicMode { ALL, ANY, SEQUENCE }

    [Header("Logic Settings")]
    //ALL = every input must be active. ANY = just one. SEQUENCE = must activate in order
    public LogicMode logicMode = LogicMode.ALL;

    //How many inputs this puzzle has (e.g. 2 pressure plates = 2 inputs
    public int inputCount = 2;

    [Header("Events")]
    //Fired when the puzzle condition is met
    public UnityEvent OnCompleted;

    //Fired when the puzzle resets (inputs deactivated)
    public UnityEvent OnReset;

    [Header("Settings")]
    //If true, once completed the puzzle cannot be reset
    public bool lockOnComplete = false;

    // ---- Private state ----
    private bool[] inputStates;         // tracks which inputs are currently active
    private int sequenceProgress = 0;   // for SEQUENCE mode
    private bool isCompleted = false;

    void Awake()
    {
        // Create the array based on how many inputs we want
        inputStates = new bool[inputCount];
    }

    // -------------------------------------------------------
    // ACTIVATE
    // Call this when an input is triggered.
    // Wire this to OnActivated on a PressurePlate or HingeObject.
    // The index tells us WHICH input was triggered.
    //
    // HOW TO WIRE IN INSPECTOR:
    //   PressurePlate OnActivated ? PuzzleEvent.Activate
    //   But UnityEvent can't pass an int directly from the Inspector
    //   for dynamic calls. Instead, use a wrapper method per input
    //   (see ActivateInput0, ActivateInput1 below)
    // -------------------------------------------------------
    public void Activate(int inputIndex)
    {
        if (isCompleted && lockOnComplete) return;
        if (inputIndex < 0 || inputIndex >= inputCount) return;

        if (logicMode == LogicMode.SEQUENCE)
        {
            // In sequence mode, inputs must fire in order (0, 1, 2...)
            if (inputIndex == sequenceProgress)
            {
                sequenceProgress++;
                Debug.Log($"Sequence progress: {sequenceProgress}/{inputCount}");

                if (sequenceProgress >= inputCount)
                    Complete();
            }
            else
            {
                // Wrong order reset sequence
                Debug.Log("Wrong order! Sequence reset.");
                sequenceProgress = 0;
            }
            return;
        }

        inputStates[inputIndex] = true;
        Debug.Log($"Input {inputIndex} activated");
        CheckCompletion();
    }

    // -------------------------------------------------------
    // DEACTIVATE
    // Call this when an input is released (e.g. object removed from plate)
    // Wire to OnDeactivated on a PressurePlate.
    // -------------------------------------------------------
    public void Deactivate(int inputIndex)
    {
        if (isCompleted && lockOnComplete) return;
        if (inputIndex < 0 || inputIndex >= inputCount) return;

        inputStates[inputIndex] = false;
        Debug.Log($"Input {inputIndex} deactivated");

        if (isCompleted)
        {
            isCompleted = false;
            OnReset?.Invoke();
        }
    }

    // ---- Inspector-friendly wrappers (wire these to UnityEvents) ----
    // Unity's Inspector can call parameterless methods via UnityEvent.
    // These wrappers let you avoid needing a dynamic int parameter.
    public void ActivateInput0() => Activate(0);
    public void ActivateInput1() => Activate(1);
    public void ActivateInput2() => Activate(2);
    public void ActivateInput3() => Activate(3);

    public void DeactivateInput0() => Deactivate(0);
    public void DeactivateInput1() => Deactivate(1);
    public void DeactivateInput2() => Deactivate(2);
    public void DeactivateInput3() => Deactivate(3);

    // -------------------------------------------------------
    // CHECK COMPLETION
    // -------------------------------------------------------
    void CheckCompletion()
    {
        bool shouldComplete = false;

        if (logicMode == LogicMode.ALL)
        {
            // All inputs must be active
            shouldComplete = true;
            foreach (bool state in inputStates)
                if (!state) { shouldComplete = false; break; }
        }
        else if (logicMode == LogicMode.ANY)
        {
            // At least one input must be active
            foreach (bool state in inputStates)
                if (state) { shouldComplete = true; break; }
        }

        if (shouldComplete && !isCompleted)
            Complete();
    }

    void Complete()
    {
        isCompleted = true;
        if (lockOnComplete) Debug.Log("Puzzle complete locked!");
        else Debug.Log("Puzzle complete!");
        OnCompleted?.Invoke();
    }

    // -------------------------------------------------------
    // RESET
    // Can be called manually or wired to an event.
    // -------------------------------------------------------
    public void ResetPuzzle()
    {
        if (lockOnComplete && isCompleted) return;

        for (int i = 0; i < inputStates.Length; i++)
            inputStates[i] = false;

        sequenceProgress = 0;
        isCompleted = false;
        OnReset?.Invoke();
        Debug.Log("Puzzle reset");
    }
}