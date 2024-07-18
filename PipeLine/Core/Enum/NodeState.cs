namespace PipeLine.Core
{
    public enum NodeState
    {
        Default = 0,
        Validate = 1,
        Failed = 2,
        Running = 3,
        Empty = 4
    }
}