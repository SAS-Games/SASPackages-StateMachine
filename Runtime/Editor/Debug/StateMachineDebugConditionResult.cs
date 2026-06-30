#if UNITY_EDITOR
namespace SAS.StateMachineGraph
{
    public readonly struct StateMachineDebugConditionResult
    {
        internal StateMachineDebugConditionResult(
            int index,
            string name,
            string parameterType,
            string mode,
            string actualValue,
            string expectedValue,
            bool wasEvaluated,
            bool result,
            string error,
            ICustomCondition customCondition)
        {
            Index = index;
            Name = name;
            ParameterType = parameterType;
            Mode = mode;
            ActualValue = actualValue;
            ExpectedValue = expectedValue;
            WasEvaluated = wasEvaluated;
            Result = result;
            Error = error;
            CustomCondition = customCondition;
        }

        public int Index { get; }
        public string Name { get; }
        public string ParameterType { get; }
        public string Mode { get; }
        public string ActualValue { get; }
        public string ExpectedValue { get; }
        public bool WasEvaluated { get; }
        public bool Result { get; }
        public string Error { get; }
        public ICustomCondition CustomCondition { get; }
    }
}
#endif
