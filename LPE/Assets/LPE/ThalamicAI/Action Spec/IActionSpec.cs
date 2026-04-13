namespace LPE.AI.LPE {
    public interface IActionSpec {
    }
    internal interface IActionSpec_internal : IActionSpec {
        float ComputePartialScore(DesireStorage _desires, ActionProposalStorage _proposal, DesireStrength strength);
    }
}
